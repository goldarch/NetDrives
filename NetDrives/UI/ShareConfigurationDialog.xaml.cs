using System;
using System.Windows;
using System.Windows.Media;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;

namespace Hardcodet.NetDrives.UI
{
  /// <summary>
  /// Interaction logic for ShareConfigurationDialog.xaml
  /// </summary>
  public partial class ShareConfigurationDialog : Window
  {
    /// <summary>
    /// Wrapper class for the configuration.
    /// </summary>
    private readonly ConfigurationWrapper configWrapper;

    /// <summary>
    /// Displays the dialog for a given configuration.
    /// </summary>
    /// <param name="configuration"></param>
    public ShareConfigurationDialog(IShareConfiguration configuration)
        : this(new ConfigurationWrapper(configuration))
    {
    }


    /// <summary>
    /// Displays the dialog for a given configuration.
    /// </summary>
    /// <param name="configWrapper">A view model wrapper that encapsulates
    /// the displayed configuration.</param>
    public ShareConfigurationDialog(ConfigurationWrapper configWrapper)
    {
      InitializeComponent();

      //create wrapper class and assign to data context
      this.configWrapper = configWrapper;
      DataContext = configWrapper;

      // set password (cannot be bound)
      txtPassword.Password = txtPasswordConfirmation.Password = configWrapper.Configuration.Password.Unwrap();

      //get drive letter candidates
      cboDriveLetter.ItemsSource = configWrapper.GetDriveLetters();

      switch (configWrapper.LoginType)
      {
        case LoginType.NoPassword:
          rbNoPasswordRequired.IsChecked = true;
          break;
        case LoginType.PromptForCredentials:
          rbPromptForPassword.IsChecked = true;
          break;
        case LoginType.PasswordSupplied:
          rbAutoLogin.IsChecked = true;
          break;
      }

      //initially validate passwords
      ValidatePasswords();
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CommitChanges(object sender, RoutedEventArgs e)
    {
      bool isValid = configWrapper.IsValid &&
                     (configWrapper.LoginType != LoginType.PasswordSupplied ||
                      txtPassword.Password == txtPasswordConfirmation.Password);

      if (!isValid)
      {
        string msg = Properties.Resources.Warning_About_Invalid_Settings_In_Configuration_Dialog;
        Dialogs.ShowWarning(msg);
        return;
      }

      //commit changes and close dialog
      configWrapper.UpdateConfiguration();
      DialogResult = true;
    }


    private void Cancel(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }


    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      //update wrapper - there's no binding
      configWrapper.Password = txtPassword.SecurePassword;
      ValidatePasswords();
    }


    /// <summary>
    /// Validates password and confirmation
    /// </summary>
    private void ValidatePasswords()
    {
      var validBackgroundBrush = "TextBoxBackground";
      var invalidBackgroundBrush = "InvalidTextBoxBackground";

      var validPassword = String.IsNullOrEmpty(configWrapper["Password"]);
      if (validPassword)
      {
        txtPassword.Background = (Brush) FindResource(validBackgroundBrush);
        txtPassword.ToolTip = null;
      }
      else
      {
        txtPassword.Background = (Brush) FindResource(invalidBackgroundBrush);
        txtPassword.ToolTip = Properties.Resources.Warning_About_Missing_Password_If_Auto_Login_Was_Configured;
      }


      bool isMatchingConfirmation = txtPassword.Password == txtPasswordConfirmation.Password;
      if (configWrapper.IsPasswordRequired && validPassword && !isMatchingConfirmation)
      {
        txtPasswordConfirmation.Background = (Brush) FindResource(invalidBackgroundBrush);
        txtPasswordConfirmation.ToolTip = Properties.Resources.Warning_About_Wrong_Password_Confirmation;
      }
      else
      {
        //ignore confirmation value...
        txtPasswordConfirmation.Background = (Brush) FindResource(validBackgroundBrush);
        txtPasswordConfirmation.ToolTip = null;
      }
    }


    /// <summary>
    /// Updates the <see cref="ConfigurationWrapper.LoginType"/> if the
    /// value of a given radio button was changed.
    /// </summary>
    private void OnLoginTypeChanged(object sender, RoutedEventArgs e)
    {
      if (rbNoPasswordRequired.IsChecked == true)
      {
        configWrapper.LoginType = LoginType.NoPassword;
      }
      else if (rbPromptForPassword.IsChecked == true)
      {
        configWrapper.LoginType = LoginType.PromptForCredentials;
      }
      else if (rbAutoLogin.IsChecked == true)
      {
        configWrapper.LoginType = LoginType.PasswordSupplied;
      }

      //depending on the option, we need a password
      ValidatePasswords();
    }
  }
}