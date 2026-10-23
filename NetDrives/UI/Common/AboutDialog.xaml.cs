using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace Hardcodet.NetDrives.UI.Common
{
  /// <summary>
  /// Interaction logic for AboutDialog.xaml
  /// </summary>
  public partial class AboutDialog : Window
  {
    /// <summary>
    /// Inits the dialog and assigns version information.
    /// </summary>
    public AboutDialog()
    {
      InitializeComponent();

      //set version number
      Version version = Assembly.GetEntryAssembly().GetName().Version;
      txtVersion.Text = version.ToString();
    }


    /// <summary>
    /// Close dialog.
    /// </summary>
    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }


    /// <summary>
    /// Opens the clicked hyperlink with the default browser.
    /// </summary>
    private void OnNavigationRequest(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
      Process.Start(e.Uri.ToString());
      e.Handled = true;
    }
  }
}