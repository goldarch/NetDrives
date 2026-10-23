using System;
using System.Windows;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel.Connections;
using Microsoft.Practices.ServiceLocation;

namespace Hardcodet.NetDrives.Shared.Commands
{
  ///<summary>
  /// Guides the user in order to create a new network share.
  ///</summary>
  public class AddNetLocationCommand : CommandExtension<AddNetLocationCommand>
  {

    /// <summary>
    /// Creates a new network configuration and optionally
    /// connects to a given network location.
    /// </summary>
    public override void Execute(object parameter)
    {
      //create default configuration
      IShareConfiguration configuration = ServiceLocator.Current.GetInstance<IShareConfiguration>();
      configuration.UserName = Environment.UserName;
      configuration.Name = Resources.New_Share_Default_Name;

      //show editor dialog
      ShareConfigurationDialog dlg = new ShareConfigurationDialog(configuration);
      dlg.SetDialogOwnerAndLocation();
      bool? result = dlg.ShowDialog();

      //cancel if user aborted
      if (result != true) return;

      //if user did not cancel, register configuration
      var share = Controller.ShareManager.AddShare(configuration);

      //ask whether to connect to share
      var msg = Resources.Ask_Whether_To_Connect_To_New_Share;
      msg = String.Format(msg, configuration.Name);
      MessageBoxResult doConnect = Dialogs.ShowYesNo(msg, MessageBoxImage.Question);
      if (doConnect == MessageBoxResult.Yes)
      {
        ConnectionUtil.Connect(share);
      }

      //suggest auto-start
      ShareUtil.CheckAndSuggestAutoStart(share.Configuration);
    }
  }
}