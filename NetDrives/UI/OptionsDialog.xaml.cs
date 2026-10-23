using System;
using System.Windows;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;

namespace Hardcodet.NetDrives.UI
{
  /// <summary>
  /// Interaction logic for OptionsDialog.xaml
  /// </summary>
  public partial class OptionsDialog : Window
  {
    public OptionsDialog()
    {
      InitializeComponent();

      // Insert code required on object creation below this point.
      chkAutoStart.IsChecked = AutoStart.IsAutoStartEnabled();
      chkShowSystemTray.IsChecked = Settings.Default.ShowSystemTrayIcon;
      chkCloseToTray.IsChecked = Settings.Default.CloseToSystemTray;

      UpdateCloseToTrayEnabled();

      cmbLanguage.ItemsSource = LocalizationManager.Instance.SupportedLanguages;
      foreach (var item in LocalizationManager.Instance.SupportedLanguages)
      {
        if (item.CultureName == Settings.Default.UiCulture)
        {
          cmbLanguage.SelectedItem = item;
          break;
        }
      }
      if (cmbLanguage.SelectedItem == null && cmbLanguage.Items.Count > 0)
      {
        cmbLanguage.SelectedIndex = 0;
      }
    }

    private void OnTrayOptionChanged(object sender, RoutedEventArgs e)
    {
      UpdateCloseToTrayEnabled();
    }

    private void UpdateCloseToTrayEnabled()
    {
      chkCloseToTray.IsEnabled = chkShowSystemTray.IsChecked == true;
    }


    /// <summary>
    /// Changes settings and closes the dialog.
    /// </summary>
    private void CommitChanges(object sender, RoutedEventArgs e)
    {
      try
      {
        AutoStart.SetAutoStart(chkAutoStart.IsChecked == true);
      }
      catch (Exception exception)
      {
        string msg = Properties.Resources.Error_Displayed_When_Setting_AutoStart_Failed;
        msg = String.Format(msg, exception.Message);
        Dialogs.ShowError(msg);
        return;
      }

      Settings.Default.ShowSystemTrayIcon = chkShowSystemTray.IsChecked == true;
      Settings.Default.CloseToSystemTray = chkCloseToTray.IsChecked == true;

      var lang = cmbLanguage.SelectedItem as LocalizationManager.LanguageOption;
      var cultureName = lang == null ? "en-US" : lang.CultureName;
      Settings.Default.UiCulture = cultureName;
      Settings.Default.Save();

      LocalizationManager.Instance.ApplyCulture(cultureName); // 立即刷新文本
      AppController.Instance.RefreshTrayMode();
      DialogResult = true;
    }


    /// <summary>
    /// Closes the dialog without updating the settings.
    /// </summary>
    private void Cancel(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }
  }
}