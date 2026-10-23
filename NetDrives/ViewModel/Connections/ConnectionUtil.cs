using System;
using System.ComponentModel;
using System.Diagnostics;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.UI.Common;
using Hardcodet.NetDrives.UI.Connections;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.ViewModel.Connections
{
  ///<summary>
  /// Manages connection handling for shares.
  ///</summary>
  public static class ConnectionUtil
  {
    #region Connect

    /// <summary>
    /// Connects to a given share and guides the user in case the
    /// connection cannot be established successfully.
    /// </summary>
    /// <param name="share">The share to connect to.</param>
    /// <returns>True if the connection was successfully established,
    /// false if the connection failed, and null if the user aborted.</returns>
    public static bool? Connect(Share share)
    {
      var status = Connect(share, share.Configuration.Login == LoginStrategy.Prompt);
      AppController.Instance.ShareManager.RefreshSummaryStatus();
      return status;
    }


    /// <summary>
    /// Connects to a given share and guides the user in case the
    /// connection cannot be established successfully.
    /// </summary>
    /// <param name="share">The share to connect to.</param>
    /// <param name="showLoginDialog">Whether to display a login dialog and ask
    /// for custom credentials.</param>
    /// <returns>True if the connection was successfully established.</returns>
    private static bool? Connect(Share share, bool showLoginDialog)
    {
      //disable reconnection for manual connection attempts
      share.IsReconnectEnabled = false;

      //try to connect
      bool? status = ConnectShare(share, showLoginDialog);
      if (status == null) return null;

      if (status == true)
      {
        //in case the connection gets lost again, try to reconnect automatically
        share.IsReconnectEnabled = true;
        return true;
      }

      //if connection fails, resolve reason and show connection error dialog
      ReconnectionStrategy strategy = HandleConnectionError(share);
      switch (strategy)
      {
        case ReconnectionStrategy.DoNotReconnect:
          return false;
        case ReconnectionStrategy.Retry:
          //just recurse, use the credentials that were used before
          return Connect(share, false);
        case ReconnectionStrategy.ReconfigureShare:
          //configure, then reconnect
          if (ShareUtil.ConfigureShare(share))
          {
            return Connect(share, showLoginDialog);
          }
          return false;
        case ReconnectionStrategy.LoginWithOtherCredentials:
          return Connect(share, true);
        default:
          throw new ArgumentOutOfRangeException("Unsupported reconnection strategy: " + strategy);
      }
    }


    /// <summary>
    /// Displays the <see cref="ConnectionErrorDialog"/> in order to
    /// determine how to proceed if an attempt to connect to a given
    /// share failed.
    /// </summary>
    /// <param name="share">The share that could not be processed
    /// successfully.</param>
    /// <returns>The user's chosen reconnection strategy.</returns>
    private static ReconnectionStrategy HandleConnectionError(Share share)
    {
      var dlg = new ConnectionErrorDialog(share);
      dlg.SetDialogOwnerAndLocation();
      var status = dlg.ShowDialog();

      if (status != true) return ReconnectionStrategy.DoNotReconnect;
      return dlg.Strategy;
    }


    /// <summary>
    /// Opens a given share location through the system's file manager.
    /// </summary>
    /// <param name="share">The share to be opened.</param>
    public static void BrowseShare(Share share)
    {
      IShareConfiguration configuration = share.Configuration;
      var location = configuration.DriveLetter.ToDriveLetterString();
      if (String.IsNullOrEmpty(location)) location = configuration.NetworkLocation;

      try
      {
        Process.Start(location);
      }
      catch (Win32Exception exception)
      {
        var msg = Resources.Error_Displayed_When_Browse_Request_Fails;
        msg = String.Format(msg, exception.NativeErrorCode, location, exception.Message);
        Dialogs.ShowError(msg);
      }
    }


    ///<summary>
    /// Connects to a given share and optionally opens the
    /// share's network location through the system's file manager.
    ///</summary>
    ///<param name="share">The share to be connected.</param>
    ///<param name="showLoginDialog">Whether to display a login dialog
    /// first which allows the user to supply custom credentials. If
    /// this parameter is false, the share's <see cref="Share.Credentials"/>
    /// are being used (NOT the one of the underlying <see cref="ShareConfiguration"/>).
    /// </param>
    /// <returns>Returns true if the connection was successfully created,
    /// false in case of a connection error, and null if the user
    /// manually cancelled the operation.</returns>
    private static bool? ConnectShare(Share share, bool showLoginDialog)
    {
      var credentials = share.Credentials;

      //if required, display login dialog
      if (showLoginDialog)
      {
        var login = new LoginDialog(credentials.UserName, credentials.Password);
        login.SetDialogOwnerAndLocation();
        if (login.ShowDialog() != true) return null;

        //adjust credentials
        credentials = new ShareCredentials(login.UserName, login.Password);
      }

      return ConnectAsync(share, credentials);
    }


    /// <summary>
    /// Displays the appliation progress dialog and runs the connection on
    /// a background thread.
    /// </summary>
    ///<param name="share">The share to be connected.</param>
    /// <param name="credentials">The credentials that are used to log on.</param>
    /// <returns></returns>
    private static bool ConnectAsync(Share share, ShareCredentials credentials)
    {
      string msg = Resources.Connection_In_Progress_Message;
      msg = String.Format(msg, share.Configuration.Name);

      ProgressDialog dlg = new ProgressDialog(msg);
      dlg.SetDialogOwnerAndLocation();

      //now connect
      ShareManager manager = AppController.Instance.ShareManager;
      ConnectResult result = ConnectResult.UnknownConnectionFailure;

      dlg.RunWorkerThread((sender, e) => { result = manager.Connect(share, credentials); });

      return result == ConnectResult.Success;
    }

    #endregion


    #region Disconnect

    /// <summary>
    /// Tries to close a connection to a given share and guides
    /// the user if the share cannot be closed.
    /// </summary>
    /// <param name="share">A connected share.</param>
    /// <param name="forceAbort">Whether the connection shuold be
    /// canceled by force.</param>
    /// <returns></returns>
    public static bool Disconnect(Share share, bool forceAbort)
    {
      Action refreshAction = AppController.Instance.ShareManager.RefreshSummaryStatus;
      using (new Guard(refreshAction))
      {
        string shareName = share.Configuration.Name;
        var manager = AppController.Instance.ShareManager;

        var result = DisconnectAsync(() => manager.Disconnect(share, forceAbort), shareName);
        if (result.Is(DisconnectResult.Success, DisconnectResult.NotConnected)) return true;

        //if disconnection failed, show disconnection dialog...
        DisconnectionErrorDialog dlg = new DisconnectionErrorDialog(share.Configuration.Name);
        dlg.SetDialogOwnerAndLocation();
        var status = dlg.ShowDialog();
        if (status != true) return false;

        //recurse with the selected option
        return Disconnect(share, dlg.ForceDisconnection);
      }
    }


    /// <summary>
    /// Tries to close a connection to an unmanaged network
    /// connection.
    /// </summary>
    /// <param name="connectionInfo">Provides information about the connection
    /// that should be closed.</param>
    /// <param name="forceAbort">Whether the connection shuold be
    /// canceled by force.</param>
    /// <returns></returns>
    public static bool Disconnect(NetworkConnectionInfo connectionInfo, bool forceAbort)
    {
      string shareName = connectionInfo.Connection.Name;
      
      //if the connection is mapped to a local drive, we need to submit the drive name
      //rather than the networ address
      string address = connectionInfo.Connection.LocalName;
      if (String.IsNullOrEmpty(address))
      {
        address = connectionInfo.Connection.RemotePath;
        address = NetworkConnectionInfo.StripRemotePath(address);
      }
      
      var result = DisconnectAsync(() => NetworkUtil.CancelUncConnection(address, forceAbort), shareName);
      if (result.Is(DisconnectResult.Success, DisconnectResult.NotConnected)) return true;

      //if disconnection failed, show disconnection dialog...
      DisconnectionErrorDialog dlg = new DisconnectionErrorDialog(connectionInfo.Connection.Name);
      dlg.SetDialogOwnerAndLocation();
      var status = dlg.ShowDialog();
      if (status != true) return false;

      //recurse with the selected option
      return Disconnect(connectionInfo, dlg.ForceDisconnection);
    }


    /// <summary>
    /// Triggers a disconnection function on a worker thread while
    /// displaying a progress dialog.
    /// </summary>
    /// <param name="disconnectionFunction">A function that is being invoked
    /// on a worker thread.</param>
    /// <param name="shareName">The name to be displayed.</param>
    /// <returns>The disconnection result.</returns>
    public static DisconnectResult DisconnectAsync(Func<DisconnectResult> disconnectionFunction, string shareName)
    {
      string msg = Resources.Disconnection_In_Progress_Message;
      msg = String.Format(msg, shareName);

      ProgressDialog dlg = new ProgressDialog(msg);
      dlg.SetDialogOwnerAndLocation();

      DisconnectResult result = DisconnectResult.UnknownDisconnectionFailure;

      //launch worker thread
      dlg.RunWorkerThread((sender, e) => { result = disconnectionFunction(); });

      return result;
    }

    #endregion
  }
}