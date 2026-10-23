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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Hardcodet.NetDrives.UI.SystemTray
{
  /// <summary>
  /// Interaction logic for BalloonPopup.xaml
  /// </summary>
  public partial class BalloonPopup : UserControl
  {
    #region BalloonText dependency property

    /// <summary>
    /// Description
    /// </summary>
    public static readonly DependencyProperty BalloonTextProperty =
        DependencyProperty.Register("BalloonText",
                                    typeof (string),
                                    typeof (BalloonPopup),
                                    new FrameworkPropertyMetadata(""));

    /// <summary>
    /// A property wrapper for the <see cref="BalloonTextProperty"/>
    /// dependency property:<br/>
    /// Description
    /// </summary>
    public string BalloonText
    {
      get { return (string) GetValue(BalloonTextProperty); }
      set { SetValue(BalloonTextProperty, value); }
    }

    #endregion


    public BalloonPopup()
    {
      InitializeComponent();
    }
  }
}