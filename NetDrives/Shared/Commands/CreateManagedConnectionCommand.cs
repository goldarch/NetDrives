using System;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.Util;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.NetDrives.ViewModel.Connections;
using Microsoft.Practices.ServiceLocation;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Creates a new share configuration based on a given system connection.
  /// </summary>
  public class CreateManagedConnectionCommand : SystemConnectionCommand<CreateManagedConnectionCommand>
  {
    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command.  If the command does not require
    /// data to be passed, this object can be set to null.
    /// </param>
    public override void Execute(object parameter)
    {
      var connectionInfo = TryGetConnectionInfo(parameter);
      if (connectionInfo == null) return;

      NetworkConnection connection = connectionInfo.Connection;

      //create default configuration
      IShareConfiguration configuration = ServiceLocator.Current.GetInstance<IShareConfiguration>();
      configuration.UserName = connection.UserName;
      configuration.NetworkLocation = NetworkConnectionInfo.StripRemotePath(connection.RemotePath);
      configuration.Name = connection.Name;

      //set drive letter, if currently mapped
      if (!String.IsNullOrEmpty(connection.LocalName))
      {
        configuration.DriveLetter = connection.LocalName[0];
      }


      //if we have a user name, set option that asks for a password
      ConfigurationWrapper wrapper = new ConfigurationWrapper(configuration);
      if (String.IsNullOrEmpty(configuration.UserName))
      {
        wrapper.LoginType = LoginType.NoPassword;
      }
      else
      {
        //will require the user to enter a password...
        wrapper.LoginType = LoginType.PasswordSupplied;
      }


      //show editor dialog
      ShareConfigurationDialog dlg = new ShareConfigurationDialog(wrapper);
      dlg.SetDialogOwnerAndLocation();
      bool? result = dlg.ShowDialog();

      //cancel if user aborted
      if (result != true) return;

      //if user did not cancel, register configuration
      Controller.ShareManager.AddShare(configuration);

      //suggest auto-start
      ShareUtil.CheckAndSuggestAutoStart(configuration);
    }
  }
}