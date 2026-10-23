using System.Linq;
using System.Windows;
using Hardcodet.NetDrives.Platform.Core.Shares;

namespace Hardcodet.NetDrives.UI.Connections
{
  /// <summary>
  /// Displays the status log entries of a given share.
  /// </summary>
  public partial class StatusLogDialog : Window
  {
    public StatusLogDialog(Share share)
    {
      InitializeComponent();

      //get an independent list (the original collection is modified on
      //worker threads...)
      DataContext = share.StatusLog.ToList();
    }

    private void CloseDialog(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}