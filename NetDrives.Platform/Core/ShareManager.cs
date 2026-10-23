using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;

namespace Hardcodet.NetDrives.Platform.Core
{
  /// <summary>
  /// Manages and observes shares and connections.
  /// </summary>
  public class ShareManager : SimpleDisposableObject
  {
    /// <summary>
    /// A timer that causes a periodic refresh of all network shares and attempts
    /// to recover connection errors.
    /// </summary>
    private Timer refreshTimer;

    /// <summary>
    /// The store that maintains the available share configurations.
    /// </summary>
    public ShareStore Store { get; private set; }

    /// <summary>
    /// The currently available shares, both connected and disconnected.
    /// </summary>
    public ObservableCollection<Share> Shares { get; private set; }

    /// <summary>
    /// Monitors the system's connections and triggers status refreshes
    /// for the shares.
    /// </summary>
    public ConnectionMonitor Monitor { get; private set; }


    #region SummaryStatus

    private ConnectionStatus summaryStatus = ConnectionStatus.Disconnected;

    /// <summary>
    /// Gets an overall status of managed connections. Returns
    /// <see cref="ConnectionStatus.Disconnected"/> in case all shares
    /// are disconnected, <see cref="ConnectionStatus.Connected"/> if
    /// at least one connection was established and no connection errors
    /// occurred, <see cref="ConnectionStatus.Degraded"/> in case of at least
    /// one degraded connection, and <see cref="ConnectionStatus.ConnectionError"/>
    /// in case a connection failed.
    /// </summary>
    public ConnectionStatus SummaryStatus
    {
      get { return summaryStatus; }
      set
      {
        //ignore if values are equal
        if (value == summaryStatus) return;

        summaryStatus = value;
        OnPropertyChanged("SummaryStatus");
      }
    }

    /// <summary>
    /// Refreshes the <see cref="SummaryStatus"/> flag.
    /// </summary>
    private void RefreshSummaryStatusInternal()
    {
      if (Shares.Contains(s => s.Status.Is(ConnectionStatus.ConnectionError)))
      {
        //we have invalid connections
        SummaryStatus = ConnectionStatus.ConnectionError;
      }
      else if (Shares.Contains(s => s.Status.Is(ConnectionStatus.Degraded)))
      {
        //we have invalid connections
        SummaryStatus = ConnectionStatus.Degraded;
      }
      else if (Shares.Contains(s => s.Status.Is(ConnectionStatus.Connected)))
      {
        //we have only valid connections
        SummaryStatus = ConnectionStatus.Connected;
      }
      else
      {
        //we have neither valid nor invalid connections
        SummaryStatus = ConnectionStatus.Disconnected;
      }
    }

    #endregion

    #region UnmanagedConnections

    private IEnumerable<NetworkConnectionInfo> unmanagedConnections = new NetworkConnectionInfo[0];

    /// <summary>
    /// Gets all connections of the system that cannot be linked to any
    /// of the maintained <see cref="ShareConfiguration"/> instances of
    /// the <see cref="Store"/>.  
    /// </summary>
    public IEnumerable<NetworkConnectionInfo> UnmanagedConnections
    {
      get { return unmanagedConnections; }
      private set
      {
        unmanagedConnections = value;
        OnPropertyChanged("UnmanagedConnections");
      }
    }

    #endregion

    #region construction

    /// <summary>
    /// Creates a new instance of the <see cref="ShareManager"/>
    /// class based on a given configuration.
    /// </summary>
    /// <param name="store">Maintains configured network shares.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="store"/>
    /// is a null reference.</exception>
    public ShareManager(ShareStore store)
    {
      Ensure.ArgumentNotNull(() => store);

      Store = store;
      store.Element.Changed += OnStoreConfigurationChanged;

      //parse all available share instances
      IEnumerable<Share> shares = store.ShareConfigurations.Select(c => new Share(c));
      Shares = new ObservableCollection<Share>(shares);

      //continue further initialization on background thread
      ThreadPool.QueueUserWorkItem(InitShares);
    }



    /// <summary>
    /// Inits the connection monitor and connects to configured shares
    /// with <see cref="IShareConfiguration.AutoConnect"/> set to true.
    /// </summary>
    private void InitShares(object state)
    {
      lock (this)
      {
        if (IsDisposed) return;

        //init monitor and register for upcoming connection events
        //TODO make query interval configurable
        Monitor = new ConnectionMonitor(2);
        Monitor.ConnectionStatusChanged += OnConnectionStatusChanged;
      }

      //analyze existing connections
      RefreshShareStates(null);

      //run asynchronous connection for all automatically connected shares
      //(does not hurt if an underlying connection is already in place)
      Shares.Where(s => s.Configuration.AutoConnect)
          .ToArray()
          .DoAsync(s =>
          {
            //enable reconnection
            s.IsReconnectEnabled = true;

            //connect if required
            if (s.Status == ConnectionStatus.Disconnected) Connect(s);
          });


      //init refresh timer and trigger an immediate update
      ChangeRefreshTimerInterval(3000, 30000);
    }

    #endregion

    #region configuration changes

    /// <summary>
    /// Raised whenever the underlying store's configuration was changed.
    /// </summary>
    public event Action<ShareManager> StoreConfigurationChanged;


    /// <summary>
    /// Bubbles change events through the <see cref="StoreConfigurationChanged"/>
    /// event.
    /// </summary>
    private void OnStoreConfigurationChanged(object sender, XObjectChangeEventArgs e)
    {
      if (StoreConfigurationChanged != null) StoreConfigurationChanged(this);
    }

    #endregion

    #region ChangeRefreshInterval

    /// <summary>
    /// Adjusts the referesh interval in which the manager actively
    /// tries to fix broken connections by explicitly reconnecting.
    /// </summary>
    /// <param name="dueTime">Timespan to the first event. Submit
    /// <see cref="Timeout.Infinite"/> in order to suppress an initial
    /// timer event that is out of the defined interval.</param>
    /// <param name="interval">The refresh interval in milliseconds.</param>
    /// <exception cref="ArgumentOutOfRangeException">In case the interval
    /// is below 5000 milliseconds.</exception>
    public void ChangeRefreshTimerInterval(int dueTime, int interval)
    {
      string msg = "Minimum refresh interval is 5 seconds";
      if (interval < 5000) throw new ArgumentOutOfRangeException("interval", msg);

      lock(this)
      {
        if (IsDisposed) return;

        if (refreshTimer == null)
        {
          refreshTimer = new Timer(RefreshShares, null, dueTime, interval);
        }
        else
        {
          refreshTimer.Change(dueTime, interval);
        }
      }
    }

    #endregion

    #region connect / disconnect

    /// <summary>
    /// Establishes a connection for a given share, using the default
    /// credentials of the underlying configuration.
    /// </summary>
    /// <param name="share">The share that is supposed to be connected.</param>
    public ConnectResult Connect(Share share)
    {
      return Connect(share, share.Configuration.CreateDefaultCredentials());
    }


    /// <summary>
    /// Establishes a connection for a given share with explicitly specified credentials.
    /// </summary>
    /// <param name="share">The share that is supposed to be connected.</param>
    /// <param name="credentials">The credentials to be used in order to connect.</param>
    public ConnectResult Connect(Share share, ShareCredentials credentials)
    {
      //try to connect (don't care about the current status)
      ConnectResult result = NetworkUtil.Connect(share.Configuration, credentials);

      //store the credentials no matter what the results were
      share.Credentials = credentials;

      //immediately set the status - this will also be announced through WMI, but do it immediatly
      ConnectionStatus status = result == ConnectResult.Success
                                    ? ConnectionStatus.Connected
                                    : ConnectionStatus.ConnectionError;

      share.ChangeStatus(status, result);

      return result;
    }


    /// <summary>
    /// Attempts to disconnect a given network connection and resets the
    /// submitted share's <see cref="Share.Status"/> to
    /// <see cref="ConnectionStatus.ManuallyDisconnected"/>.
    /// </summary>
    /// <param name="share">The share to disconnect.</param>
    /// <param name="forceDisconnect">Whether the connection should be forcibly
    /// cut, even if it is use.</param>
    /// <returns>The result of the disconnection request.</returns>
    public DisconnectResult Disconnect(Share share, bool forceDisconnect)
    {
      IShareConfiguration configuration = share.Configuration;

      DisconnectResult result;

      //if possible, use the underlying network connection to disconnect
      if (share.ConnectionInfo != null && share.ConnectionInfo.Connection != null)
      {
        result = NetworkUtil.Disconnect(share.ConnectionInfo.Connection, forceDisconnect);
      }
      else if (configuration.DriveLetter.HasValue)
      {
        result = NetworkUtil.RemoveMappedDriveConnection(configuration, forceDisconnect);
      }
      else
      {
        result = NetworkUtil.CancelUncConnection(configuration.NetworkLocation, forceDisconnect);
      }


      //update status if the disconnection was ok, including the fact that the connection
      //no longer exists
      if (result == DisconnectResult.Success || result == DisconnectResult.NotConnected)
      {
        //update status flags
        ConnectionStatus status = ConnectionStatus.ManuallyDisconnected;
        share.ChangeStatus(status, null);

        //also remove the underlying connection
        share.ConnectionInfo = null;
      }

      return result;
    }

    #endregion

    #region manage shares

    /// <summary>
    /// Stores a new share configuration with the store without attempting
    /// to connect to the target.
    /// </summary>
    /// <param name="configuration">A configuration for a given share.</param>
    /// <returns>The matching share instance.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    public Share AddShare(IShareConfiguration configuration)
    {
      Ensure.ArgumentNotNull(() => configuration);

      //store the configuration
      Store.AddConfiguration(configuration);
      var share = new Share(configuration);

      //immediately check for a matching connection
      //the monitor is created asynchronously, so it *might* be still null
      //(highly unlikely except for testing scenarios)
      if (Monitor != null)
      {
        NetworkConnectionInfo ci = Monitor.FindMatchingConnectionInfo(configuration);
        RefreshShareStatus(share, ci);

        //if we had an underlying connection that was not mapped to a
        //share, it is now so remove it from unmanaged connections list
        List<NetworkConnectionInfo> list = unmanagedConnections.ToList();
        if (ci != null && list.Contains(ci))
        {
          list.Remove(ci);
          UnmanagedConnections = list;
        }
      }

      Shares.Add(share);
      return share;
    }


    /// <summary>
    /// Removes a given share from the store.
    /// </summary>
    /// <param name="configuration">The configuration that should be removed.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    /// <remarks>Removing the share does not change the share's
    /// underlying configuration.</remarks>
    /// <exception cref="InvalidOperationException">If no such configuration is managed.
    /// </exception>
    public void RemoveShare(IShareConfiguration configuration)
    {
      Ensure.ArgumentNotNull(() => configuration);

      if (!Store.ShareConfigurations.Contains(c => c.ItemId == configuration.ItemId))
      {
        string msg = "Share configuration {0} ({1}) not found in the configuration store.";
        msg = String.Format(msg, configuration.Name, configuration.ItemId);
        throw new InvalidOperationException(msg);
      }

      //remove the share first
      Share share = Shares.Single(s => s.Configuration.ItemId == configuration.ItemId);
      Shares.Remove(share);

      //remove the configuration (will dispose it)
      Store.RemoveConfiguration(configuration);

      //if the share is mapped to an underlying connection, this one is now probably
      //unmanaged, so update state
      if (share.ConnectionInfo != null && !share.ConnectionInfo.IsRemoved)
      {
        RefreshShareStates(null);
      }
    }

    #endregion

    #region refresh share states

    /// <summary>
    /// Refreshes the <see cref="SummaryStatus"/> by reevaluating
    /// the status of the managed <see cref="Shares"/>.<br/>
    /// This method runs asynchronously and returns immediately.
    /// </summary>
    public void RefreshSummaryStatus()
    {
      ThreadPool.QueueUserWorkItem(delegate
                                     {
                                       lock (this)
                                       {
                                         RefreshSummaryStatusInternal();
                                       }
                                     });
    }

    /// <summary>
    /// A timer-signalled method which checks for invalid share states
    /// and tries to reconnect.
    /// </summary>
    private void RefreshShares(object state)
    {
      lock (this)
      {
        if (IsDisposed) return;

        //reconnect all invalid shares
        IEnumerable<Share> shares =
            Shares.Where(s => s.Status.Is(ConnectionStatus.ConnectionError) && s.IsReconnectEnabled);
        shares.DoAsync(share => Connect(share));

        //explicitly refresh degraded shares - we don't get WMI events if they
        //connections recover...
        shares = Shares.Where(s => s.Status.Is(ConnectionStatus.Degraded)).ToArray();
        Monitor.RefreshShareConnections(shares);
      }

      //refresh share states
      RefreshShareStates(null);
    }


    /// <summary>
    /// Re-evaluates all configured shares and updates their
    /// status, if necessary.
    /// </summary>
    private void OnConnectionStatusChanged(ConnectionMonitor monitor)
    {
      ThreadPool.QueueUserWorkItem(RefreshShareStates);
    }


    /// <summary>
    /// Runs a synchronized refresh of all shares by
    /// reevaluating all configured shares and updating their
    /// status, if necessary. This method is invoked from
    /// a worker thread.
    /// </summary>
    private void RefreshShareStates(object state)
    {
      lock (this)
      {
        if (IsDisposed) return;

        //link shares to underlying connections
        var pairs = from share in Shares
                    let ci = Monitor.FindMatchingConnectionInfo(share.Configuration, share.Credentials.UserName)
                    select new {Share = share, ConnectionInfo = ci};


        //determine connections that are not related to any of the configurations
        UnmanagedConnections = Monitor.Connections.Except(pairs.Select(p => p.ConnectionInfo)).ToArray();

        //update the managed share's states
        foreach (var pair in pairs)
        {
          RefreshShareStatus(pair.Share, pair.ConnectionInfo);
        }

        //update summary status
        RefreshSummaryStatusInternal();
      }
    }


    /// <summary>
    /// Manually triggers refresh of a single share. This method runs
    /// on a background thread and returns immediately.
    /// </summary>
    /// <param name="share">The share to be refreshed.</param>
    public void RefreshSingleShareState(Share share)
    {
      ThreadPool.QueueUserWorkItem(s =>
                                     {
                                       lock (this)
                                       {
                                         if (IsDisposed) return;

                                         string userName = share.Credentials.UserName;
                                         var ci = Monitor.FindMatchingConnectionInfo(share.Configuration, userName);
                                         RefreshShareStatus(share, ci);
                                       }
                                     });
    }


    /// <summary>
    /// Checks the status of a given share with regards to a corresponding
    /// <see cref="NetworkConnectionInfo"/>.
    /// </summary>
    /// <param name="ci">A connection information that corresponds to the share.</param>
    /// <param name="share">The processed share.</param>
    private static void RefreshShareStatus(Share share, NetworkConnectionInfo ci)
    {
      if (ci == null)
      {
        //there is no matching system connection - if the share was connected, there are 3
        //possibilities:
        //1: The connection was lost. In that case, the share still maintains its own reference
        //   to an earlier connection: We have an unexpected connection loss
        //2: The network connection was not yet reported through WMI, meaning we are infact
        //   connected. In this case, the share has no reference to a system connection
        //3: The share was connected and the connection was immediately lost. Would mean that
        //   the share has no reference to a connection and it was already removed.
        //-> 2/3 cannot be clearly separated, but state 2 can only appear for a short interval
        //-> check timestamps. if the connection was just made, let it slip, otherwise treat
        //   as invalid.
        if (share.Status.Is(ConnectionStatus.Connected, ConnectionStatus.Degraded))
        {
          if (share.ConnectionInfo != null && share.ConnectionInfo.IsRemoved)
          {
            //the connection was reported as being removed, hence the IsRemoved flag...
            share.ChangeStatus(ConnectionStatus.ConnectionError, ConnectResult.Lost);
            return;
          }

          StatusLogEntry logEntry = share.LatestLogEntry;
          bool isLogEntryConnected = logEntry.Status.Is(ConnectionStatus.Connected, ConnectionStatus.Degraded);

          if (share.ConnectionInfo != null || !isLogEntryConnected || logEntry.IsOlderThan(10))
          {
            //regard the connection as lost, otherwise let it slip
            share.ChangeStatus(ConnectionStatus.ConnectionError, ConnectResult.Lost);
          }
        }
        else
        {
          //if the share isn't connected and there is no underlying connection,
          //then there's nothing to do here - the share's just not in use...
        }
        
        return;
      }

      //map the connection to the share
      share.ConnectionInfo = ci;

      //handle switch to valid connection
      ConnectionStatus status = ci.GetLowLevelStatus();
      if (status == ConnectionStatus.Connected)
      {
        //no matter what status we were - we're connected now
        if (share.Status != ConnectionStatus.Connected)
        {
          //only update if the status isn't fine already
          share.ChangeStatus(ConnectionStatus.Connected, ConnectResult.Success);
        }

        return;
      }

      //handle switch to degraded network
      if (status == ConnectionStatus.Degraded)
      {
        if (share.Status != ConnectionStatus.Degraded)
        {
          share.ChangeStatus(ConnectionStatus.Degraded, ConnectResult.Degraded);
        }
        return;
      }
      
      //handle connection loss
      if (share.Status.Is(ConnectionStatus.Connected, ConnectionStatus.Degraded))
      {
        //only degrade status if we were infact connected
        share.ChangeStatus(ConnectionStatus.ConnectionError, ConnectResult.Lost);
      }
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Disposes the item and the underlying store.
    ///</remarks>
    protected override void Dispose(bool disposing)
    {
      if (!disposing)
      {
        base.Dispose(false);
        return;
      }


      lock (this)
      {
        if (!IsDisposed)
        {
          base.Dispose(true);

          Store.Element.Changed -= OnStoreConfigurationChanged;
          if (Monitor != null) Monitor.Dispose();
          Store.Dispose();
        }
      }
    }

    #endregion
  }

}