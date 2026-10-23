using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.NetDrives.ViewModel.Connections;
using Microsoft.Windows.Controls;


namespace Hardcodet.NetDrives.UI.SystemTray
{
  /// <summary>
  /// Interaction logic for SysTrayPopup.xaml
  /// </summary>
  public partial class SysTrayPopup : Popup
  {
    public SysTrayPopup()
    {
      InitializeComponent();
    }



    /// <summary>
    /// Attempts to open and browse a given network location.
    /// </summary>
    private void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
      DependencyObject source = (DependencyObject)e.OriginalSource;
      var row = UIHelpers.TryFindParent<DataGridRow>(source);

      //the user did not click on a row
      if (row == null) return;

      var share = AppController.Instance.ActiveShare;
      if (share != null)
      {
        bool status = share.IsConnected || ConnectionUtil.Connect(share) == true;
        if (status) ConnectionUtil.BrowseShare(share);
      }

      e.Handled = true;
    }

  }
}
