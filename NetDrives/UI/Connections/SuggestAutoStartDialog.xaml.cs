using System.Windows;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.UI.Connections
{
  /// <summary>
  /// Interaction logic for SuggestAutoStartDialog.xaml
  /// </summary>
  public partial class SuggestAutoStartDialog : Window
  {
    /// <summary>
    /// Inits the dialog with the configured share.
    /// </summary>
    public SuggestAutoStartDialog(IShareConfiguration configuration)
    {
      InitializeComponent();
      DataContext = configuration;
    }


    /// <summary>
    /// Updates settings and closes the dialog.
    /// </summary>
    private void btnOk_Click(object sender, RoutedEventArgs e)
    {
      if (rbAutoStart.IsChecked == true)
      {
        AutoStart.SetAutoStart(true);
      }

      if (chkSuppressMessage.IsChecked == true)
      {
        Settings.Default.SuggestAutoStart = false;
      }

      DialogResult = true;
    }
  }
}