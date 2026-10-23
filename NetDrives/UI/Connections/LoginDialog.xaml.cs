using System;
using System.Security;
using System.Windows;
using Hardcodet.NetDrives.Platform;

namespace Hardcodet.NetDrives.UI.Connections
{
  /// <summary>
  /// Interaction logic for LoginDialog.xaml
  /// </summary>
  public partial class LoginDialog : Window
  {
    /// <summary>
    /// The user name to be used.
    /// </summary>
    public string UserName
    {
      get { return txtUserName.Text; }
      set { txtUserName.Text = value; }
    }

    /// <summary>
    /// The suggested password.
    /// </summary>
    public SecureString Password
    {
      get { return txtPassword.SecurePassword; }
      set { txtPassword.Password = value.Unwrap(); }
    }


    /// <summary>
    /// Inits the dialog with default username / password.
    /// </summary>
    public LoginDialog(string userName, SecureString password)
    {
      InitializeComponent();

      UserName = userName;
      Password = password;

      //if the user name is set and the password isn't, select
      //the password box. If both / none are set, select the UID box
      //if both user name and password are set, select the OK button
      if (!String.IsNullOrEmpty(userName) && password.IsNullOrEmpty())
      {
        txtPassword.Focus();
        txtPassword.SelectAll();
      }
      else if (!String.IsNullOrEmpty(userName) && !password.IsNullOrEmpty())
      {
        //we have both UID and password...
        btnOk.Focus();
      }
      else
      {
        txtUserName.Focus();
        txtUserName.SelectAll();
      }
    }


    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }
  }
}