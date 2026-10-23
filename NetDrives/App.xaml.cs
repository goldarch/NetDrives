using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Autofac.Builder;
using CommonServiceLocator.AutofacAdapter;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.Util.Logging;
using Hardcodet.Util.Logging.BitFactoryFacade;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Win32;
using System.Threading; // 新增

#if DEBUG
using System.Runtime.ExceptionServices;
#endif

namespace Hardcodet.NetDrives
{
    public partial class App : Application
    {
#if DEBUG
        private static DateTime _lastFirstChanceNreAt = DateTime.MinValue;
        private static string _lastFirstChanceNreFp;
#endif

        public IServiceLocator ContainerServiceLocator { get; private set; }

        // 防止同一时刻重复执行“重建托盘”
        private bool _recreatingTrayIcon;

        // 用于“延迟 500ms 后再重建托盘”的一次性定时器
        private DispatcherTimer _trayRecreateDelayTimer;

        private static DateTime _lastUnhandledAt = DateTime.MinValue;
        private static string _lastUnhandledFingerprint;

        // 新增：单实例控制
        private static Mutex _instanceMutex;
        private static bool _isPrimaryInstance;
        private static EventWaitHandle _showWindowEvent;
        private static RegisteredWaitHandle _showWindowEventRegistration;

        private const string SingleInstanceMutexName = @"Local\Hardcodet.NetDrives.SingleInstance";
        private const string ShowWindowEventName = @"Local\Hardcodet.NetDrives.ShowMainWindow";

        /// <summary>
        /// Either shows the application's main window or
        /// inits the application in the system tray.
        /// </summary>
        private void PerformStartup(object sender, StartupEventArgs e)
        {
            // 最先做：单实例检查（轻量，无UI）
            if (!EnsureSingleInstanceOrNotifyAndExit())
            {
                Shutdown();
                return;
            }

#if DEBUG
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
#endif

            // 先注册全局异常处理，覆盖容器初始化前的异常
            DispatcherUnhandledException += OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += (s, args) => AppUtil.LogException(args.ExceptionObject);
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, args) =>
            {
                AppUtil.LogException(args.Exception);
                args.SetObserved();
            };

            var isAutoStart = e.Args.Contains(AutoStart.MinimizedArgument);

            // 开机启动时工作目录为 System32，需切换到程序目录以保证相对路径正确
            try
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            }
            catch
            {
            }

            AutoStart.RepairAutoStart();

            // PerformStartup 里，尽量在 UI 创建前调用
            LocalizationManager.Instance.ApplyCulture(Hardcodet.NetDrives.Properties.Settings.Default.UiCulture);

            SplashScreen screen = null;
            if (!isAutoStart)
            {
                screen = new SplashScreen("Shared/Images/Logo.png");
                screen.Show(false);
            }

            //#if DEBUG
            //            //临时测试，在createContainer前后各写一次日志，确保日志初始化正常
            //            WriteLogSelfTestSeries();
            //#endif

            CreateContainer();

            //#if DEBUG
            //            WriteLogSelfTestSeries();
            //#endif

            // 关键：使用完整命名空间，避免与 Application.Properties 冲突
            if (isAutoStart && Hardcodet.NetDrives.Properties.Settings.Default.ShowSystemTrayIcon)
            {
                AppController.Instance.MinimizeToTray();
            }
            else
            {
                if (screen != null) screen.Close(TimeSpan.FromSeconds(1));
                AppController.Instance.ShowMainWindow();
            }


            SystemEvents.SessionSwitch += OnSessionSwitch;
            SystemEvents.SessionEnded += OnSessionEnded;
        }

        private bool EnsureSingleInstanceOrNotifyAndExit()
        {
            bool createdNew;
            _instanceMutex = new Mutex(true, SingleInstanceMutexName, out createdNew);
            _isPrimaryInstance = createdNew;

            try
            {
                _showWindowEvent = EventWaitHandle.OpenExisting(ShowWindowEventName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _showWindowEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowWindowEventName);
            }

            if (!createdNew)
            {
                try
                {
                    _showWindowEvent.Set();
                }
                catch
                {
                }

                return false;
            }

            _showWindowEventRegistration = ThreadPool.RegisterWaitForSingleObject(
                _showWindowEvent,
                OnShowWindowRequested,
                null,
                -1,
                false);

            return true;
        }

#if DEBUG
        private static void OnFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (!(e.Exception is NullReferenceException)) return;

            var fp = e.Exception.Message + "|" + e.Exception.StackTrace;
            var now = DateTime.UtcNow;
            var duplicated = (fp == _lastFirstChanceNreFp) && (now - _lastFirstChanceNreAt).TotalSeconds < 3;
            if (duplicated) return;

            _lastFirstChanceNreFp = fp;
            _lastFirstChanceNreAt = now;

            AppUtil.LogException(new Exception("[FIRST-CHANCE NRE] " + e.Exception, e.Exception));
        }
#endif

        private static void WriteLogSelfTestSeries()
        {
            // A. 普通对象日志
            AppUtil.LogException("SELFTEST-A: plain text message");

            // B. 常见异常日志
            try
            {
                string s = null;
                var len = s.Length; // NRE
            }
            catch (Exception ex)
            {
                AppUtil.LogException(ex);
            }

            // C. 业务异常
            AppUtil.LogException(new InvalidOperationException("SELFTEST-C: invalid operation"));

            // D. 嵌套异常
            try
            {
                throw new ArgumentNullException("name", "SELFTEST-D inner");
            }
            catch (Exception inner)
            {
                AppUtil.LogException(new ApplicationException("SELFTEST-D outer", inner));
            }
        }

        /// <summary>
        /// Gets any unhandled exceptions of the application.
        /// </summary>
        private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (!e.Dispatcher.CheckAccess())
            {
                e.Dispatcher.Invoke(new Action(() => OnUnhandledException(sender, e)), DispatcherPriority.Normal, null);
                return;
            }

            // 关键：抑制同一异常的弹窗风暴
            var fp = e.Exception.GetType().FullName + "|" + e.Exception.Message + "|" + e.Exception.StackTrace;
            var now = DateTime.UtcNow;
            bool duplicated = (fp == _lastUnhandledFingerprint) && (now - _lastUnhandledAt).TotalSeconds < 10;

            _lastUnhandledFingerprint = fp;
            _lastUnhandledAt = now;

            AppUtil.LogException(e.Exception);
            if (!duplicated)
            {
                e.Exception.ReportAndLogException(NetDrives.Properties.Resources.Message_About_Unhandled_Exception);
            }

            // NRE通常不可恢复，避免“继续运行+无限弹窗”
            if (e.Exception is NullReferenceException)
            {
                e.Handled = true;
                Current.Shutdown(-1);
                return;
            }

            e.Handled = true;
        }


        /// <summary>
        /// Inits the IOC container.
        /// </summary>
        private void CreateContainer()
        {
            var builder = new ContainerBuilder();

            //register single logger instance
            ILogger logger;
            if (AppUtil.IsDesignMode)
            {
                logger = new NullLogger();
            }
            else
            {
                //dx 2026.08.10 不区分debug和release，统一使用单文件日志
                //#if (DEBUG)
                //  logger = new ConsoleLogger();
                //#else
                //  logger = BitFactoryLogger.CreateSingleFileLogger(AppUtil.LogFile);
                //#endif

                logger = BitFactoryLogger.CreateSingleFileLogger(AppUtil.LogFile);
            }

            builder.Register(logger).As<ILogger>();

            //create share configurations via IOC
            builder.Register(c => ShareFactory.CreateConfiguration())
                .As<IShareConfiguration>()
                .WithScope(InstanceScope.Factory);

            IContext context = builder.Build();
            ContainerServiceLocator = new AutofacServiceLocator(context);

            //register as the default provider
            ServiceLocator.SetLocatorProvider(() => ContainerServiceLocator);
        }

        private static void OnShowWindowRequested(object state, bool timedOut)
        {
            var app = Current as App;
            if (app == null) return;

            app.Dispatcher.BeginInvoke(
                new Action(app.BringMainWindowToFront),
                DispatcherPriority.Normal);
        }

        private void BringMainWindowToFront()
        {
            try
            {
                AppController.Instance.ShowMainWindow();

                var main = Current.MainWindow;
                if (main == null) return;

                if (main.WindowState == WindowState.Minimized)
                    main.WindowState = WindowState.Normal;

                if (!main.IsVisible)
                    main.Show();

                main.Activate();
                main.Topmost = true;
                main.Topmost = false;
                main.Focus();
            }
            catch (Exception ex)
            {
                AppUtil.LogException(ex);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (_showWindowEventRegistration != null)
            {
                _showWindowEventRegistration.Unregister(null);
                _showWindowEventRegistration = null;
            }

            if (_showWindowEvent != null)
            {
                _showWindowEvent.Close();
                _showWindowEvent = null;
            }

            if (_instanceMutex != null)
            {
                if (_isPrimaryInstance)
                {
                    try
                    {
                        _instanceMutex.ReleaseMutex();
                    }
                    catch
                    {
                    }
                }

                _instanceMutex.Close();
                _instanceMutex = null;
            }

            SystemEvents.SessionSwitch -= OnSessionSwitch;
            SystemEvents.SessionEnded -= OnSessionEnded;

            // 退出时清理定时器，避免事件残留
            if (_trayRecreateDelayTimer != null)
            {
                _trayRecreateDelayTimer.Stop();
                _trayRecreateDelayTimer.Tick -= OnTrayRecreateDelayTimerTick;
                _trayRecreateDelayTimer = null;
            }

            base.OnExit(e);
        }

        private void OnSessionEnded(object sender, SessionEndedEventArgs e)
        {
            // 会话结束时可选清理，避免残留
            try
            {
                var c = AppController.Instance;
                if (c.TaskbarIcon != null) c.TaskbarIcon = null;
            }
            catch (Exception ex)
            {
                AppUtil.LogException(ex);
            }
        }

        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // 只处理会影响通知区域/桌面的切换场景
            // SessionUnlock: 解锁
            // ConsoleConnect: 切回本地控制台
            // RemoteConnect: 切到远程会话
            if (e.Reason != SessionSwitchReason.SessionUnlock &&
                e.Reason != SessionSwitchReason.ConsoleConnect &&
                e.Reason != SessionSwitchReason.RemoteConnect)
            {
                return;
            }

            // 懒创建定时器（只创建一次，后续复用）
            if (_trayRecreateDelayTimer == null)
            {
                _trayRecreateDelayTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher);
                _trayRecreateDelayTimer.Interval = TimeSpan.FromMilliseconds(500);
                _trayRecreateDelayTimer.Tick += OnTrayRecreateDelayTimerTick;
            }

            // 去抖动：
            // 如果短时间内连续收到多次 SessionSwitch，先 Stop 再 Start ，
            // 保证只在“最后一次事件后的 500ms”执行一次重建。
            _trayRecreateDelayTimer.Stop();
            _trayRecreateDelayTimer.Start();
        }

        private void OnTrayRecreateDelayTimerTick(object sender, EventArgs e)
        {
            // 一次性触发：先停止，防止重复进入
            _trayRecreateDelayTimer.Stop();

            // 二次保护：如果当前已在重建中，直接返回
            if (_recreatingTrayIcon) return;
            _recreatingTrayIcon = true;

            try
            {
                // 真正执行重建（在 AppController 中）
                AppController.Instance.RecreateTrayIcon();
            }
            catch (Exception ex)
            {
                // 只记录，不在这里再次弹窗，避免循环打扰用户
                AppUtil.LogException(ex);
            }
            finally
            {
                _recreatingTrayIcon = false;
            }
        }
    }
}