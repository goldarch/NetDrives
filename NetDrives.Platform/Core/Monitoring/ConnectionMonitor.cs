using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using Hardcodet.XItems;
using SimpleDisposableObject=Hardcodet.Commons.SimpleDisposableObject;

namespace Hardcodet.NetDrives.Platform.Core.Monitoring
{
  /// <summary>
  /// Observes running connections of the system and maintains
  /// a repository of network locations.
  /// </summary>
  public class ConnectionMonitor : SimpleDisposableObject
  {
    /// <summary>
    /// Underlying WMI observer that notifies about changes of
    /// the system's network connections.
    /// </summary>
    private readonly NetworkConnectionObserver connectionObserver;

    /// <summary>
    /// The underlying connections.
    /// </summary>
    private readonly ObservableCollection<NetworkConnectionInfo> connections;

    /// <summary>
    /// Provides information about maintained connections.
    /// </summary>
    public IEnumerable<NetworkConnectionInfo> Connections
    {
      get { return connections; }
    }

    
    /// <summary>
    /// Fired if a connection was changed, added, or deleted. Used
    /// to indicates that a re-evaluation of connections should be performed.
    /// </summary>
    public event Action<ConnectionMonitor> ConnectionStatusChanged;


    #region construction

    /// <summary>
    /// Creates the monitor which immediately subscribes for
    /// events regarding network connections of the system,
    /// using a default interval as specified by the
    /// <see cref="WmiClassObserver.DefaultQueryInterval"/>
    /// constant.
    /// </summary>
    public ConnectionMonitor() : this(WmiClassObserver.DefaultQueryInterval)
    {
    }


    /// <summary>
    /// Creates the monitor which immediately subscribes for
    /// events regarding network connections of the system.
    /// </summary>
    /// <param name="queryInterval">The WMI polling interval
    /// that is used to monitor underlying system connections
    /// in seconds.</param>
    public ConnectionMonitor(int queryInterval)
    {
      connections = new ObservableCollection<NetworkConnectionInfo>();

      //get all network connections
      var cons = NetworkConnection.GetInstances();
      foreach (NetworkConnection con in cons)
      {
        CreateConnectionInfo(con);
      }

      //register listener for connection changes
      connectionObserver = new NetworkConnectionObserver(queryInterval);
      connectionObserver.WmiEventFired += OnConnectionChanged;
    }

    #endregion


    /// <summary>
    /// Creates a new <see cref="NetworkConnectionInfo"/> for a given
    /// <see cref="NetworkConnection"/> and stores it in the
    /// internal <see cref="Connections"/> list.
    /// </summary>
    /// <param name="networkConnection">The connection to be managed.</param>
    private void CreateConnectionInfo(NetworkConnection networkConnection)
    {
      NetworkConnectionInfo ci = new NetworkConnectionInfo(networkConnection);
      connections.Add(ci);
      RefreshHostLocations(ci);
    }



    #region network connection change events

    /// <summary>
    /// WMI event listener that gets notified if a network connection
    /// of the system is added, removed, or modified.
    /// </summary>
    private void OnConnectionChanged(object sender, WmiEventArgs e)
    {    
      lock (this)
      {
        var baseObject = e.GetTargetInstance();

        NetworkConnection connection = new NetworkConnection(baseObject);

        //check if we have matching connections (the "Name" property is a key value)
        var matches = Connections.Where(con => con.Connection.Name == connection.Name).ToArray();
       
        switch (e.EventClass)
        {
          case WmiEventClass.InstanceCreation:
            //if the connection was added and we already have that one,
            //remove it to make sure
            RemoveConnections(matches);
            //create new connection info
            CreateConnectionInfo(connection);
            break;
          case WmiEventClass.InstanceDeletion:
            RemoveConnections(matches);
            break;
          case WmiEventClass.InstanceModification:
            //if the connection was modified, run refresh
            foreach (var connectionInfo in matches)
            {
              connectionInfo.Connection = connection;
              RefreshHostLocations(connectionInfo);
            }
            break;
          default:
            //fail in debug mode - should not happen
            var msg = "Unexpected connection event class: {0}".Put(e.EventClass);
            Debug.Fail(msg);
            //update underlying connection
            matches.Do(ci => ci.Connection = connection);
            break;
        }
      }

      //announce connection update
      if (ConnectionStatusChanged != null) ConnectionStatusChanged(this);
    }


    /// <summary>
    /// Removes and disposes a number of connections that are no longer
    /// required.
    /// </summary>
    private void RemoveConnections(IEnumerable<NetworkConnectionInfo> matches)
    {
      //if the connection was cancelled, remove the connections
      foreach (var match in matches)
      {
        match.IsRemoved = true;
        connections.Remove(match);
        match.Dispose();
      }
    }

    #endregion


    #region refresh share connections

    /// <summary>
    /// Refreshes underlying <see cref="NetworkConnection"/> instances
    /// of a number of shares.
    /// </summary>
    /// <param name="shares">Shares to be refreshed.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="shares"/>
    /// is a null reference.</exception>
    public void RefreshShareConnections(IEnumerable<Share> shares)
    {
      if (shares == null) throw new ArgumentNullException("shares");

      foreach (Share share in shares)
      {
        //if there is no existing connection, query information is missing...
        if (share.ConnectionInfo == null || share.ConnectionInfo.Connection == null) continue;
        var oldConnection = share.ConnectionInfo.Connection;

        //create a new connection - use path information of the existing one
        ManagementPath path = new ManagementPath(oldConnection.SystemProperties.RELPATH);
        var connection = new NetworkConnection(path);
        share.ConnectionInfo.Connection = connection;
        oldConnection.Dispose();
      }
    }

    #endregion

    #region find connection info for share

    
    /// <summary>
    /// Tries to find a matching connection information for a given
    /// network configuration that is an exact match.
    /// Comparison strategies depend on the kind of configuration:<br/>
    /// - The network path<br/>
    /// - The user name<br/>
    /// - if the configuration creates a mapped drive, a <see cref="NetworkConnectionInfo"/>
    /// that refers to that drive is returned, if possible.<br/>
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns>A connection information item that provides the closes match,
    /// if possible.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    /// <remarks>In case the configuration contains only a simple user name (e.g. "philipp")
    /// rather than a qualified name (<c>COMPUTERNAME\philipp</c>), the host name of the
    /// underlying connection is ignored when it comes to comparing as it appears a connection
    /// can be created with a bogus host name anyway.</remarks>
    public NetworkConnectionInfo FindMatchingConnectionInfo(IShareConfiguration configuration)
    {
      return FindMatchingConnectionInfo(configuration, configuration.UserName);
    }


    /// <summary>
    /// Tries to find a matching connection information for a given
    /// network configuration that is an exact match.
    /// Comparison strategies depend on the kind of configuration:<br/>
    /// - The network path<br/>
    /// - The user name<br/>
    /// - if the configuration creates a mapped drive, a <see cref="NetworkConnectionInfo"/>
    /// that refers to that drive is returned, if possible.<br/>
    /// </summary>
    /// <param name="userName">The user name that is included in the comparison. A connection
    /// that refers to another login is not regarded a match.</param>
    /// <param name="configuration">The configuration that provides the network location.</param>
    /// <returns>A connection information item that provides the closes match,
    /// if possible.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    /// <remarks>In case the configuration contains only a simple user name (e.g. "philipp")
    /// rather than a qualified name (<c>COMPUTERNAME\philipp</c>), the host name of the
    /// underlying connection is ignored when it comes to comparing as it appears a connection
    /// can be created with a bogus host name anyway.</remarks>
    public NetworkConnectionInfo FindMatchingConnectionInfo(IShareConfiguration configuration, string userName)
    {
      Ensure.ArgumentNotNull(() => configuration);

      userName = (userName ?? String.Empty).ToLowerInvariant();
      string location = configuration.NetworkLocation;

      lock (this)
      {
        var matches = from con in Connections
                      let qualifiedUserName = (con.Connection.UserName ?? String.Empty).ToLowerInvariant()
                      let simpleUserName = Path.GetFileName(qualifiedUserName)
                      let driveLetter = con.Connection.GetDriveLetterString()

                      where (userName == qualifiedUserName || userName == simpleUserName) &&
                            driveLetter == configuration.DriveLetter.ToDriveLetterString() &&
                            con.ContainsLocation(location)
                      select con;

        if (matches.Count() > 1)
        {
          var msg =
              "Got duplicate connection matches for configuration. Should look into this cases. Configuration XML:\n{0}\n\n";
          msg = String.Format(msg, ((IXItem)configuration).Element);
          Debug.Fail(msg);
        }

        //return the first match
        return matches.FirstOrDefault();
      }
    }

    #endregion


    #region refresh host locations

    /// <summary>
    /// Asynchronously refreshes all possible network location names
    /// for a given <see cref="NetworkConnectionInfo"/> instance. Once
    /// the <paramref name="connectionInfo"/> has been refreshed, the
    /// <see cref="ConnectionStatusChanged"/> event will be fired.
    /// </summary>
    /// <param name="connectionInfo">The connection info to be refreshed.</param>
    private void RefreshHostLocations(NetworkConnectionInfo connectionInfo)
    {
      connectionInfo.RefreshConnectionInfos(status =>
                                            {
                                              //only fire status event if the request was successful,
                                              //a degraded network is being detected otherwise.
                                              if (status && ConnectionStatusChanged != null) ConnectionStatusChanged(this);
                                            });
    }

    #endregion


    #region Dispose

    /// <summary>
    /// Disposes the underlying observer.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        lock (this)
        {
          //clear observer
          connectionObserver.WmiEventFired -= OnConnectionChanged;
          connectionObserver.Dispose();

          //dispose and clear connection infos
          foreach (var connectionInfo in Connections.ToArray())
          {
            connectionInfo.Dispose();
            connections.Remove(connectionInfo);
          }
        }
      }

      base.Dispose(disposing);
    }

    #endregion


  }
}