using System;
using System.ComponentModel;
using System.Windows;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.Util;
using Hardcodet.Wpf.TaskbarNotification;
using System.Configuration;
using System.IO;

namespace Hardcodet.NetDrives.ViewModel
{
  /// <summary>
  /// Main controller of the application.
  /// </summary>
  public class AppController : SimpleDisposableObject
  {
    #region singleton

    /// <summary>
    /// Singleton instance.
    /// </summary>
    private static readonly AppController instance = new AppController();

    /// <summary>
    /// Provides access to the singleton instance of
    /// the class.
    /// </summary>
    public static AppController Instance
    {
      get { return instance; }
    }

    /// <summary>
    /// Private constructor. A reference to the Singleton
    /// instance of this class is available through the
    /// static <see cref="Instance"/> property.
    /// </summary>
    private AppController()
    {
      Init();
    }

    #endregion

    #region Init

    /// <summary>
    /// Initializes the controller.
    /// </summary>
    private void Init()
    {
      //don't initialize share manager if we're in design mode
      if (AppUtil.IsDesignMode) return;

      AppUtil.UpgradeSettings();

      ShareStore store = AppUtil.ReadConfiguration();
      ShareManager = new ShareManager(store);
    }

    #endregion

    #region TaskbarIcon

    /// <summary>
    /// Provides access to the system tray area.
    /// </summary>
    private TaskbarIcon taskbarIcon;


    /// <summary>
    /// Provides access to the system tray area.
    /// </summary>
    public TaskbarIcon TaskbarIcon
    {
      get { return taskbarIcon; }
      set
      {
        if (taskbarIcon != null)
        {
          //dispose current tray handler
          taskbarIcon.Dispose();
        }

        taskbarIcon = value;
        OnPropertyChanged("TaskbarIcon");
      }
    }

    #endregion

    #region ShareManager

    private ShareManager shareManager;


    /// <summary>
    /// Manages the underyling configuration and network
    /// connections.
    /// </summary>
    public ShareManager ShareManager
    {
      get { return shareManager; }
      private set
      {
        if (shareManager != null)
        {
          shareManager.StoreConfigurationChanged -= OnStoreConfigurationChanged;
        }

        shareManager = value;

        if (shareManager != null)
        {
          shareManager.StoreConfigurationChanged += OnStoreConfigurationChanged;
        }

        OnPropertyChanged("ShareManager");
      }
    }

    #endregion

    #region ActiveShare

    /// <summary>
    /// The currently selected <see cref="Share"/>.
    /// </summary>
    private Share activeShare;


    /// <summary>
    /// The currently selected <see cref="Share"/>.
    /// </summary>
    public Share ActiveShare
    {
      get { return activeShare; }
      set
      {
        activeShare = value;
        OnPropertyChanged("ActiveShare");
      }
    }

    #endregion

    #region Configuration Changes

    /// <summary>
    /// Automatically persists changes of the underlying
    /// configuration to disk.
    /// </summary>
    /// <param name="manager">The manager that manitains the
    /// underlying configuration store.</param>
    private static void OnStoreConfigurationChanged(ShareManager manager)
    {
      AppUtil.WriteConfiguration(manager.Store);
    }

    #endregion

    #region Show Main Window

    /// <summary>
    /// Displays the main application window and assigns
    /// it as the application's <see cref="Application.MainWindow"/>.
    /// </summary>
    public void ShowMainWindow()
    {
      Application app = Application.Current;

      if (app.MainWindow == null)
      {
        app.MainWindow = new Main();
        app.MainWindow.Closing += OnMainWindowClosing;
        app.MainWindow.Show();
      }

      Window main = app.MainWindow;
      if (main.WindowState == WindowState.Minimized) main.WindowState = WindowState.Normal;
      if (!main.IsVisible) main.Show();

      main.Activate();
      main.Topmost = true;
      main.Topmost = false;
      main.Focus();

      // 不再强制隐藏托盘图标，交给参数控制
      RefreshTrayMode();
    }


    /// <summary>
    /// Minimizes the application to the system tray.
    /// </summary>
    public void MinimizeToTray()
    {
      if (!Settings.Default.ShowSystemTrayIcon) return;

      var mainWindow = Application.Current.MainWindow;
      if (mainWindow != null)
      {
        mainWindow.Closing -= OnMainWindowClosing;
        mainWindow.Close();
      }

      if (TaskbarIcon == null) TaskbarIcon = AppUtil.InitSystemTray();
      if (TaskbarIcon != null) TaskbarIcon.Visibility = Visibility.Visible;
    }

    public void RefreshTrayMode()
    {
      // 关闭：只隐藏，不销毁实例
      if (!Settings.Default.ShowSystemTrayIcon)
      {
        if (TaskbarIcon != null) TaskbarIcon.Visibility = Visibility.Collapsed;
        return;
      }

      // 开启：确保实例存在并显示
      if (TaskbarIcon == null)
      {
        TaskbarIcon = AppUtil.InitSystemTray();
      }

      if (TaskbarIcon != null)
      {
        TaskbarIcon.Visibility = Visibility.Visible;
      }
    }

    public void RecreateTrayIcon()
    {
      if (!Settings.Default.ShowSystemTrayIcon)
      {
        if (TaskbarIcon != null) TaskbarIcon.Visibility = Visibility.Collapsed;
        return;
      }

      // 这里保留重建逻辑（用于 explorer 重启等场景）
      if (TaskbarIcon != null) TaskbarIcon = null;
      TaskbarIcon = AppUtil.InitSystemTray();
      if (TaskbarIcon != null) TaskbarIcon.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Closes the main window and either exits the application or displays
    /// the taskbar icon and remains active.
    /// </summary>
    /// <param name="forceShutdown">Whether the application
    /// should perform a shutdown anyway.</param>
    public void CloseMainApplication(bool forceShutdown)
    {
      bool stayInTray = Settings.Default.ShowSystemTrayIcon && Settings.Default.CloseToSystemTray;

      if (!forceShutdown && stayInTray)
      {
        MinimizeToTray();
        return;
      }

      SaveSettingsSafely();

      Dispose();
      Application.Current.Shutdown();
    }

    //退出时改为“安全保存”，失败只记录不弹窗（tray 点关闭不该打断用户）
        private static void SaveSettingsSafely()
    {
      try
      {
        Settings.Default.Save();
      }
      catch (ArgumentException ex)
      {
        if (ex.ParamName != "sectionGroupName") throw;

        // 修复损坏的 per-user config，再重试一次
        var cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
        if (File.Exists(cfg.FilePath))
        {
          File.Delete(cfg.FilePath);
        }

        Settings.Default.Reload();
        Settings.Default.Save();
      }
    }

    #endregion

    #region dispose

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (TaskbarIcon != null)
        {
          //reset system tray handler
          TaskbarIcon = null;
        }

        //otherwise close
        if (shareManager != null)
        {
          shareManager.Dispose();
          ShareManager = null;
        }
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Window Events

    private void OnMainWindowClosing(object sender, CancelEventArgs e)
    {
      ((Window)sender).Closing -= OnMainWindowClosing;

      // reset first, then decide close behavior
      Application.Current.MainWindow = null;
      CloseMainApplication(false);
    }

    #endregion
  }
}