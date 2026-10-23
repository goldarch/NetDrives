using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.ViewModel.Connections;

namespace Hardcodet.NetDrives.UI.Connections
{
  /// <summary>
  /// Interaction logic for ConnectionErrorDialog.xaml
  /// </summary>
  public partial class ConnectionErrorDialog : Window
  {
    /// <summary>
    /// Gets the selected strategy.
    /// </summary>
    public ReconnectionStrategy Strategy
    {
      get
      {
        if (DialogResult == false) return ReconnectionStrategy.DoNotReconnect;
        if (rbRetry.IsChecked == true) return ReconnectionStrategy.Retry;
        if (rbReconfigure.IsChecked == true) return ReconnectionStrategy.ReconfigureShare;
        if (rbChangeLogin.IsChecked == true) return ReconnectionStrategy.LoginWithOtherCredentials;

        throw new InvalidOperationException("No option was selected");
      }
    }


    /// <summary>
    /// Displays the options for a given share.
    /// </summary>
    /// <param name="share"></param>
    public ConnectionErrorDialog(Share share)
    {
      InitializeComponent();
      DataContext = share;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    private void btnConnect_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

  }
}
