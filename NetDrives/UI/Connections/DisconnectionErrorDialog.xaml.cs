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

namespace Hardcodet.NetDrives.UI.Connections
{
  /// <summary>
  /// Interaction logic for DisconnectionErrorDialog.xaml
  /// </summary>
  public partial class DisconnectionErrorDialog : Window
  {
    #region ShareName dependency property

    /// <summary>
    /// The name of the share that is about to be disconnected.
    /// </summary>
    public static readonly DependencyProperty ShareNameProperty =
        DependencyProperty.Register("ShareName",
                                    typeof (string),
                                    typeof (DisconnectionErrorDialog),
                                    new FrameworkPropertyMetadata(String.Empty));

    /// <summary>
    /// A property wrapper for the <see cref="ShareNameProperty"/>
    /// dependency property:<br/>
    /// The name of the share that is about to be disconnected.
    /// </summary>
    public string ShareName
    {
      get { return (string) GetValue(ShareNameProperty); }
      set { SetValue(ShareNameProperty, value); }
    }

    #endregion



    public bool ForceDisconnection
    {
      get { return rbCloseForced.IsChecked == true; }
    }


    public DisconnectionErrorDialog(string shareName)
    {
      InitializeComponent();
      ShareName = shareName;
    }

    private void btnCancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    private void btnDisconnect_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }
  }
}
