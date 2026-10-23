using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Interop;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Represents a given share and its state.
  /// </summary>
  public class Share : SimpleObject
  {
    /// <summary>
    /// The share's underlying configuration.
    /// </summary>
    public IShareConfiguration Configuration { get; private set; }

    #region StatusLog

    private readonly ObservableCollection<StatusLogEntry> statusLog = new ObservableCollection<StatusLogEntry>();

    /// <summary>
    /// Maintains a list of status changes for the share.
    /// </summary>
    public IEnumerable<StatusLogEntry> StatusLog
    {
      get { return statusLog; }
    }


    /// <summary>
    /// Gets the latest log entry.
    /// </summary>
    public StatusLogEntry LatestLogEntry
    {
      get { return statusLog.Last(); }
    }

    #endregion

    /// <summary>
    /// A convenience property which indicates whether the share is
    /// connected or not. Returns true if the <see cref="Status"/>
    /// equals <see cref="ConnectionStatus.Connected"/>.
    /// </summary>
    public bool IsConnected
    {
      get { return Status == ConnectionStatus.Connected; }
    }


    /// <summary>
    /// Indicates whether the share is not connected due to an
    /// erroneous condition. Returns true if the <see cref="Status"/>
    /// equals <see cref="ConnectionStatus.ConnectionError"/>.
    /// </summary>
    public bool HasErrors
    {
      get { return Status == ConnectionStatus.ConnectionError; }
    }

    #region Credentials

    /// <summary>
    /// The credentials that are currently used or suggested for the next login.
    /// </summary>
    private ShareCredentials credentials;


    /// <summary>
    /// The credentials that are currently used or suggested for the next login.
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public ShareCredentials Credentials
    {
      get { return credentials; }
      set
      {
        if (value == null) throw new ArgumentNullException("value");
        credentials = value;
        OnPropertyChanged("Credentials");
        OnPropertyChanged(() => UsesDefaultLogin);
      }
    }


    /// <summary>
    /// Indicates whether this share uses a custom login (different
    /// user ID / password) than the credentials that were configured
    /// through the underlying <see cref="ShareConfiguration"/>.<br/>
    /// Always returns true if the configuration is set to
    /// <see cref="LoginStrategy.Prompt"/> and no user name is configured.
    /// </summary>
    public bool UsesDefaultLogin
    {
      get
      {
        //if there is no password information stored, only compare
        //user IDs
        if (Configuration.Login == LoginStrategy.Prompt || Configuration.Password.IsNullOrEmpty())
        {
          return (Credentials.UserName ?? "") == (Configuration.UserName ?? "");
        }

        //compare credentials
        return Credentials.UserName == Configuration.UserName &&
               Credentials.Password.Matches(Configuration.Password);
      }
    }

    #endregion

    #region Status

    /// <summary>
    /// Indicates the current connection status.
    /// </summary>
    private ConnectionStatus status = ConnectionStatus.Disconnected;


    /// <summary>
    /// Indicates the current connection status.
    /// </summary>
    public ConnectionStatus Status
    {
      get { return status; }
      private set
      {
        //ignore if values are equal
        if (value == status) return;

        status = value;

        //raise property change events
        OnPropertyChanged(() => Status);
        OnPropertyChanged(() => IsConnected);
        OnPropertyChanged(() => HasErrors);
      }
    }

    #endregion

    #region ConnectionResult

    /// <summary>
    /// The status that was returned for the latest connection
    /// attempt.
    /// </summary>
    private ConnectResult? connectionResult;


    /// <summary>
    /// The status that was returned for the latest connection
    /// attempt. Returns null if the share is properly disconnected.
    /// </summary>
    public ConnectResult? ConnectionResult
    {
      get { return connectionResult; }
      private set
      {
        //ignore if values are equal
        if (value == connectionResult) return;

        connectionResult = value;
        OnPropertyChanged("ConnectionResult");
      }
    }

    #endregion

    #region Connection

    /// <summary>
    /// Provides the related connection once it has been resolved.
    /// </summary>
    private NetworkConnectionInfo connectionInfo;


    /// <summary>
    /// Provides the related connection once it has been resolved.
    /// </summary>
    public NetworkConnectionInfo ConnectionInfo
    {
      get { return connectionInfo; }
      set
      {
        connectionInfo = value;
        OnPropertyChanged("ConnectionInfo");
      }
    }

    #endregion

    #region IsReconnectEnabled

    /// <summary>
    /// Indicates whether the system should try to reconnect automatically
    /// if the connection is invalid.
    /// </summary>
    private bool isReconnectEnabled;


    /// <summary>
    /// Indicates whether the system should try to reconnect automatically
    /// if the connection is invalid. Defaults to false (needs to be
    /// set explicitly).
    /// </summary>
    public bool IsReconnectEnabled
    {
      get { return isReconnectEnabled; }
      set
      {
        isReconnectEnabled = value;
        OnPropertyChanged("IsReconnectEnabled");
      }
    }

    #endregion

    /// <summary>
    /// Creates a share with the underlying configuration.
    /// </summary>
    /// <param name="configuration"></param>
    public Share(IShareConfiguration configuration)
    {
      Configuration = configuration;
      Credentials = configuration.CreateDefaultCredentials();

      //init the log
      CreateLogEntry();
    }


    /// <summary>
    /// Updates the share's internal status and creates a log entry.
    /// </summary>
    public void ChangeStatus(ConnectionStatus connectionStatus, ConnectResult? connectResult)
    {
      Status = connectionStatus;
      ConnectionResult = connectResult;
      CreateLogEntry();
    }


    /// <summary>
    /// Creates a log entry that reflects the share's current
    /// status.
    /// </summary>
    private void CreateLogEntry()
    {
      var entry = new StatusLogEntry(SystemTime.Now(), Status, ConnectionResult);
      statusLog.Add(entry);

      lock (this)
      {
        while (statusLog.Count > 50)
        {
          statusLog.RemoveAt(0);
        }
      }

      //announce new item
      OnPropertyChanged(() => LatestLogEntry);
    }
  }
}