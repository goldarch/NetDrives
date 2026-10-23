using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.Util.Logging;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Practices.ServiceLocation;

namespace Hardcodet.NetDrives.Util
{
    ///<summary>
    /// Common util classes.
    ///</summary>
    public static class AppUtil
    {
        /// <summary>
        /// The name of the application data folder, which is being expected in the
        /// <see cref="Environment.SpecialFolder.LocalApplicationData"/> folder.
        /// </summary>
        public const string LocalDataFolderName = "NetDrives";

        /// <summary>
        /// The relative path of the settings file (for now) from the point of the
        /// <see cref="Environment.SpecialFolder.LocalApplicationData"/> folder.
        /// </summary>
        public const string SettingsFileName = "NetDrivesData.xml";

        /// <summary>
        /// The name of the application's log file.
        /// </summary>
        public const string LogFileName = "Log.txt";


        #region IsDesignMode

        private static readonly bool isDesignMode;

        /// <summary>
        /// Checks whether the application is currently in design mode.
        /// </summary>
        public static bool IsDesignMode
        {
            get { return isDesignMode; }
        }

        #endregion


        /// <summary>
        /// Gets the full path of the appliation's settings file.
        /// </summary>
        public static string SettingsFile
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, LocalDataFolderName);
                return Path.Combine(path, SettingsFileName);
            }
        }


        /// <summary>
        /// Gets the full path of the application's log file.
        /// </summary>
        public static string LogFile
        {
            get
            {
                //enable for quick diagnostics
                //return FileUtil.GetAbsolutePath(LogFileName);

                /*
        【说明】
        在现代 Windows 系统（Windows 7 / 10 / 11 / Server 2016 等）中，Environment.SpecialFolder.LocalApplicationData 默认指向的位置是：
        C:\Users\<你的用户名>\AppData\Local
        关键特性说明：
        默认隐藏： AppData 文件夹在 Windows 中默认是隐藏的。如果你想在文件资源管理器中直接打开它，最快捷的方法是在地址栏输入 %localappdata% 并回车。
        应用场景： 它用于存储特定于当前用户且不需要在域（Active Directory）网络中漫游同步的应用程序数据。通常用于存放体积较大、仅依赖于当前物理机器的数据，例如本地缓存、大型配置文件或日志文件。
        对比： 如果你需要保存用户在域环境中登录任何机器都应该能读取到的配置，应该使用 SpecialFolder.ApplicationData（对应 AppData\Roaming）。
                 */

                var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, LocalDataFolderName);
                return Path.Combine(path, LogFileName);
            }
        }


        static AppUtil()
        {
            isDesignMode =
                (bool)
                DependencyPropertyDescriptor
                    .FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement))
                    .Metadata.DefaultValue;
        }


        /// <summary>
        /// Upgrade application settings, if necessary.
        /// </summary>
        public static void UpgradeSettings()
        {
            if (IsDesignMode) return;

            if (Settings.Default.IsUpdateRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.IsUpdateRequired = false;
            }
        }


        /// <summary>
        /// Parses the configuration file into a share store.
        /// </summary>
        /// <returns></returns>
        public static ShareStore ReadConfiguration()
        {
            var path = SettingsFile;

            if (File.Exists(path))
            {
                try
                {
                    var doc = XDocument.Load(path);
                    var store = new ShareStore(doc.Root);
                    return store;
                }
                catch (Exception e)
                {
                    //log exception
                    string msg = Resources.Logged_Message_If_Configuration_File_Could_Not_Be_Read;
                    msg = String.Format(msg, path);
                    ServiceLocator.Current.GetInstance<ILogger>().Log(msg, e);

                    //if anything goes wrong with building the store,
                    //create a copy and return an empty store - should never happen
                    try
                    {
                        File.Copy(path, path + ".bak", true);
                    }
                    catch
                    {
                        /* ignore */
                    }
                }
            }


            //create empty store
            XElement element = new XElement(ShareStore.RootElementName);
            return new ShareStore(element);
        }


        /// <summary>
        /// Stores the configuration into the application settings file.
        /// </summary>
        /// <param name="store"></param>
        public static void WriteConfiguration(ShareStore store)
        {
            //dx:2024.11.01 保存到 "C:\\Users\\goldarch\\AppData\\Local\\NetDrives\\NetDrivesData.xml"
            //C:\Users\goldarch\AppData\Local\NetDrives
            var path = SettingsFile;

            FileInfo fi = new FileInfo(path);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }

            store.Element.Save(path);
        }


        /// <summary>
        /// Checks whether a given address is a host address or not.
        /// </summary>
        /// <param name="address">The address to be analized.</param>
        /// <returns>True in case of a host address, false in case a
        /// share on the host is addessed. Null in case of an invalid
        /// uril.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="address"/>
        /// is a null reference.</exception>
        public static bool? IsHostAddress(string address)
        {
            Ensure.ArgumentNotNull(() => address);

            Uri uri;
            bool status = Uri.TryCreate(address, UriKind.Absolute, out uri);
            if (!status) return null;

            //make sure we operate on the local path
            status = Uri.TryCreate(uri.LocalPath, UriKind.Absolute, out uri);
            if (!status) return null;

            return uri.Segments.Length == 1;
        }


        /// <summary>
        /// Inits the component that displays status information in the
        /// system tray.
        /// </summary>
        public static TaskbarIcon InitSystemTray()
        {
            var tbIcon = (TaskbarIcon)Application.Current.FindResource("TrayIcon");
            tbIcon.DataContext = AppController.Instance;
            return tbIcon;
        }


        /// <summary>
        /// Displays an error dialog containing an error message which is
        /// formatted with the submitted exception's <see cref="Exception.Message"/>,
        /// and logs the exception to the application log.
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        /// <param name="errorMessage">A string pattern which is formatted with the
        /// exception message.</param>
        public static void ReportAndLogException(this Exception exception, string errorMessage)
        {
            Ensure.ArgumentNotNull(() => exception);

            //show error
            string msg = String.Format(errorMessage, exception.Message);
            Dialogs.ShowError(msg);

            //log exception
            LogException(exception);
        }


        /// <summary>
        /// Tries to write a given exception to the log.
        /// </summary>
        /// <param name="exception">Exception to be logged.</param>
        public static void LogException(object exception)
        {
            ILogger logger = null;

            // 1) 尝试拿 logger，但不能让这里的异常中断兜底文件日志
            try
            {
                try
                {
                    IServiceLocator locator = ServiceLocator.Current;
                    if (locator != null)
                    {
                        logger = locator.GetInstance<ILogger>();
                    }
                }
                catch
                {
                    // ignore and fallback
                }

                if (logger != null)
                {
                    Exception ex = exception as Exception;
                    if (ex != null)
                    {
                        logger.Log(ex);
                    }
                    else
                    {
                        var message = exception == null ? "Unknown source" : exception.ToString();
                        logger.Log(message, System.Diagnostics.TraceEventType.Critical);
                    }
                    return;
                }
            }
            catch
            {
                // ignore and fallback
            }

            // 2) 无论上面发生什么，最终都写文件
            try
            {
                var path = LogFile;
                var dir = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

                var ex = exception as Exception;
                string entry;
                if (ex != null)
                {
                    entry = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss} [FALLBACK] {1}\r\nMessage: {2}\r\nTargetSite: {3}\r\nStack:\r\n{4}\r\n\r\n",
                        DateTime.Now,
                        ex.GetType().FullName,
                        ex.Message,
                        ex.TargetSite == null ? "(null)" : ex.TargetSite.ToString(),
                        ex.StackTrace ?? "(null)");
                }
                else
                {
                    entry = string.Format(
                        "{0:yyyy-MM-dd HH:mm:ss} [FALLBACK] {1}\r\n\r\n",
                        DateTime.Now,
                        exception ?? "Unknown source");
                }

                System.IO.File.AppendAllText(path, entry);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }

        // fallback so very-early exceptions still persist to disk
        public static void LogException2(object exception)
        {
            try
            {
                var locator = ServiceLocator.Current;
                var logger = locator == null ? null : locator.GetInstance<ILogger>();

                if (logger != null)
                {
                    if (exception is Exception ex) logger.Log(ex);
                    else
                        logger.Log(exception?.ToString() ?? "Unknown source",
                            System.Diagnostics.TraceEventType.Critical);
                    return;
                }

                // fallback: append to log file
                try
                {
                    var path = LogFile;
                    var dir = System.IO.Path.GetDirectoryName(path);
                    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    var entry = string.Format("{0:yyyy-MM-dd HH:mm:ss} - {1}{2}", DateTime.Now,
                        exception ?? "Unknown source", Environment.NewLine);
                    System.IO.File.AppendAllText(path, entry);
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }
    }
}