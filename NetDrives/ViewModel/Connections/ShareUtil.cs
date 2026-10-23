using System;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.UI.Connections;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.ViewModel.Connections
{
  /// <summary>
  /// Common util methods for shares and configurations.
  /// </summary>
  public static class ShareUtil
  {
    ///<summary>
    /// Displays the configuration dialog for a given share's
    /// <see cref="ShareConfiguration"/>.
    ///</summary>
    ///<param name="share">The share to be configured.</param>
    /// <returns>True if the user did not cancel the dialog.</returns>
    public static bool ConfigureShare(Share share)
    {
      var configuration = share.Configuration;

      var currentUserName = share.Credentials.UserName;
      var currentLocation = NetworkConnectionInfo.UnifyPath(configuration.NetworkLocation);
      var currentDrive = configuration.DriveLetter;

      ShareConfigurationDialog dlg = new ShareConfigurationDialog(configuration);
      dlg.SetDialogOwnerAndLocation();
      var result = dlg.ShowDialog();

      if (result == true)
      {
        //in case the dialog was confirmed, reset credentials.
        //everything else might confuse users
        share.Credentials = configuration.CreateDefaultCredentials();
 
        //if the user name or the network changed, treat it as a completely
        //different share
        if (currentUserName != share.Credentials.UserName ||
          currentLocation != NetworkConnectionInfo.UnifyPath(configuration.NetworkLocation) ||
          currentDrive != configuration.DriveLetter)
        {
          //reset share and refresh
          share.ChangeStatus(ConnectionStatus.Reset, null);
          AppController.Instance.ShareManager.RefreshSingleShareState(share);
        }
      }

      return result == true;
    }



    /// <summary>
    /// Suggests to enable NetDrives' auto-start feature in order to make
    /// sure connection to shares with <see cref="ShareConfiguration.AutoConnect"/>
    /// enabled are automatically created.
    /// </summary>
    /// <param name="configuration">The processed configuration.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    public static void CheckAndSuggestAutoStart(IShareConfiguration configuration)
    {
      Ensure.ArgumentNotNull(() => configuration);

      if (!configuration.AutoConnect || !Settings.Default.SuggestAutoStart || AutoStart.IsAutoStartEnabled())
      {
        //don't do anything if autostart is already enabled or not
        //required
        return;
      }

      //launch dialog
      SuggestAutoStartDialog dlg = new SuggestAutoStartDialog(configuration);
      dlg.SetDialogOwnerAndLocation();
      dlg.ShowDialog();
    }
  }
}
