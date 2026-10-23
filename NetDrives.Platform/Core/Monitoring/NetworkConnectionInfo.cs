using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;

namespace Hardcodet.NetDrives.Platform.Core.Monitoring
{
  /// <summary>
  /// Manages alternative network location names for a given
  /// reference network <see cref="Connection"/>.
  /// </summary>
  public class NetworkConnectionInfo : SimpleDisposableObject
  {
    /// <summary>
    /// The operating system's identifier for the connection.
    /// Returns the <see cref="NetworkConnection.Name"/>, which
    /// acts as a key for the OS.
    /// </summary>
    public string Id
    {
      get { return Connection.Name; }
    }

    #region Connection

    private NetworkConnection connection;

    /// <summary>
    /// The registered network connection, which acts as a reference
    /// item.
    /// </summary>
    public NetworkConnection Connection
    {
      get { return connection; }
      internal set
      {
        connection = value;
        OnPropertyChanged(() => Connection);
        OnPropertyChanged(() => Id);
        OnPropertyChanged(() => IsConnected);
      }
    }

    #endregion

    #region NetworkLocations

    /// <summary>
    /// Gets valid network location names with the host name part
    /// in several formats once the host names and/or IP addresses
    /// have been resolved. Examples:<br/>
    /// <c>\\127.0.0.1\MyShare</c><br/>
    /// <c>\\192.168.0.5\MyShare</c><br/>
    /// <c>\\localhost\MyShare</c><br/>
    /// <c>\\MyComputerName\MyShare</c><br/>
    /// </summary>
    public IEnumerable<string> NetworkLocations { get; private set; }

    #endregion

    #region HostEntry

    private IPHostEntry hostEntry;

    /// <summary>
    /// The host entry that was used to resolve alternative host
    /// names for the entry. The class is being initialized with
    /// a custom-built host that only provides a single host name
    /// which matches the <see cref="Connection"/>.
    /// </summary>
    /// <remarks>Setting this property refreshes the <see cref="NetworkLocations"/>
    /// sequence.</remarks>
    /// <exception cref="ArgumentNullException">If <paramref name="value"/>
    /// is a null reference.</exception>
    public IPHostEntry HostEntry
    {
      get { return hostEntry; }
      set
      {
        if (value == null) throw new ArgumentNullException("value");

        hostEntry = value;
        RefreshNetworkLocationPaths();

        OnPropertyChanged(() => HostEntry);
      }
    }

    #endregion

    #region IsRemoved

    /// <summary>
    /// Set to true if the underlying <s
    /// </summary>
    private bool isRemoved;


    /// <summary>
    /// Set to true if the underlying <see cref="Connection"/>
    /// was advertised as being removed from the system's
    /// network connections.
    /// </summary>
    public bool IsRemoved
    {
      get { return isRemoved; }
      set
      {
        //ignore if values are equal
        if (value == isRemoved) return;

        isRemoved = value;

        OnPropertyChanged(() => IsRemoved);
        OnPropertyChanged(() => IsConnected);
      }
    }

    #endregion

    #region IsConnected

    /// <summary>
    /// Returns true if the <see cref="IsRemoved"/> flag is false, and
    /// the underlying <see cref="Connection"/> indicates a valid connection
    /// through its <see cref="NetworkConnection.ConnectionState"/> flag.
    /// </summary>
    public bool IsConnected
    {
      get
      {
        return !isRemoved &&
               (
                   Connection.ConnectionState == "Connected" ||
                   Connection.Status.ToUpperInvariant() == "OK" && Connection.ConnectionState == "Disconnected"
               );
      }
    }




    /// <summary>
    /// Checks whether the underlying connection is either
    /// <see cref="ConnectionStatus.Connected"/>,
    /// <see cref="ConnectionStatus.Degraded"/>, or
    /// <see cref="ConnectionStatus.Disconnected"/>.
    /// </summary>
    /// <returns>Low level connection flag.</returns>
    public ConnectionStatus GetLowLevelStatus()
    {
      if (isRemoved) return ConnectionStatus.Disconnected;
      if (Connection.ConnectionState == "Connected") return ConnectionStatus.Connected;
      if (Connection.Status.ToUpperInvariant() == "OK" &&
          Connection.ConnectionState.ToUpperInvariant() == "DISCONNECTED")
      {
        //ok, too - seems to be an inactive connection
        return ConnectionStatus.Connected;
      }

      if (Connection.Status.ToUpperInvariant() == "DEGRADED")
      {
        return ConnectionStatus.Degraded;
      }

      return ConnectionStatus.Disconnected;
    }


    #endregion

    #region construction

    /// <summary>
    /// Creates a new <see cref="NetworkConnectionInfo"/> instance for a
    /// given reference connection of the operating system.
    /// </summary>
    /// <param name="connection">The reference connection.s</param>
    /// <exception cref="ArgumentNullException">If <paramref name="connection"/>
    /// is a null reference.</exception>
    public NetworkConnectionInfo(NetworkConnection connection)
    {
      Ensure.ArgumentNotNull(() => connection);

      Connection = connection;
      HostEntry = CreateDefaultEntry();
    }

    #endregion

    #region ContainsLocation

    /// <summary>
    /// Checks whether the submitted path
    /// has a corresponding entry in the
    /// <see cref="NetworkLocations"/>
    /// collection.
    /// </summary>
    /// <param name="remotePath">The path to be evaluated.</param>
    /// <returns>True in case a match was found.</returns>
    public bool ContainsLocation(string remotePath)
    {
      string path = UnifyPath(remotePath);

      foreach (string loc in NetworkLocations)
      {
        var location = UnifyPath(loc);
        if (path == location) return true;
      }

      return false;
    }

    #endregion

    #region Create default IPHostEntry

    /// <summary>
    /// Creates a default entry for the reference
    /// <see cref="Connection"/>.
    /// </summary>
    /// <returns>Defaut host entry that only provides the
    /// hosts's network location.</returns>
    private IPHostEntry CreateDefaultEntry()
    {
      string hostName = NetworkUtil.TryGetHostName(Connection.RemotePath);
      var entry = new IPHostEntry
                    {
                        HostName = hostName,
                        AddressList = new IPAddress[0]
                    };

      return entry;
    }

    #endregion

    #region Path Processing

    /// <summary>
    /// Unifies a given path by converting characters
    /// to lower string, removing special share suffixes
    /// and removing ending path separators.
    /// </summary>
    /// <param name="path">The path to be processed.</param>
    /// <returns></returns>
    public static string UnifyPath(string path)
    {
      path = path.ToLowerInvariant();
      path = StripRemotePath(path);

      if (path.EndsWith(@"\") || path.EndsWith("/"))
      {
        return path.Substring(0, path.Length - 1);
      }

      return path;
    }


    /// <summary>
    /// Cuts internal suffixes (such as <c>\IPC$</c>) from a
    /// network location path.
    /// </summary>
    /// <param name="remotePath">Path to be processed.</param>
    /// <returns>Adjusted network path.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="remotePath"/>
    /// is a null reference.</exception>
    public static string StripRemotePath(string remotePath)
    {
      Ensure.ArgumentNotNull(() => remotePath);

      string path = remotePath.ToLowerInvariant();

      //remove IPC$ suffix
      int index = path.IndexOf("ipc$");
      if (index > -1)
      {
        return remotePath.Substring(0, index);
      }

      if (path.EndsWith("$"))
      {
        //in case of a hidden share, just remove the dollar sign
        return remotePath.Substring(0, remotePath.Length - 1);
      }

      //return unchanged remote path
      return remotePath;
    }

    #endregion

    #region Refresh Host Locations

    /// <summary>
    /// Asynchronously refreshes the <see cref="HostEntry"/> and
    /// all possible network location paths of this
    /// <see cref="NetworkConnectionInfo"/> instance.
    /// </summary>
    /// <param name="requestCallback">The callback method that is
    /// being invoked once the host entry was refreshed. The boolean
    /// parameter indicates whether the request was processed successfully
    /// or not.</param>
    internal void RefreshConnectionInfos(Action<bool> requestCallback)
    {
      Uri uri;
      bool result = Uri.TryCreate(Connection.RemotePath, UriKind.RelativeOrAbsolute, out uri);
      if (!result) return;

      //start host name resolving 
      string host = uri.Host;
      Dns.BeginGetHostEntry(host, OnHostEntryResolved, requestCallback);
    }


    /// <summary>
    /// Callback handler of the <see cref="RefreshConnectionInfos"/> method,
    /// which gets an updated <see cref="IPHostEntry"/> for a given
    /// <see cref="NetworkConnectionInfo"/> instance.
    /// </summary>
    /// <param name="result"></param>
    private void OnHostEntryResolved(IAsyncResult result)
    {
      var callback = (Action<bool>) result.AsyncState;

      lock (this)
      {
        //ignore disposed items
        if (IsDisposed) return;

        try
        {
          HostEntry = Dns.EndGetHostEntry(result);

          //announce connection update
          if (callback != null) callback(true);
        }
        catch (SocketException)
        {
          //do not change things - a degraded network connection
          //is being detected otherwise
          if (callback != null) callback(false);
        }
      }
    }

    #endregion

    #region refresh network location paths

    /// <summary>
    /// Recreates the <see cref="NetworkLocations"/> list in order
    /// to reflect the value of the underlying <see cref="HostEntry"/>.
    /// </summary>
    private void RefreshNetworkLocationPaths()
    {
      var list = new ObservableCollection<string>();

      //get the stripped UNC path
      string path = UnifyPath(Connection.RemotePath);

      //resolve the host name
      string host = NetworkUtil.TryGetHostName(path);

      //store the path as submitted (don't care about duplicates)
      list.Add(path);

      //store location using host name
      InsertIfNotContained(list, path.Replace(host, HostEntry.HostName));

      //store locations using IP addresses
      foreach (var address in HostEntry.AddressList)
      {
        InsertIfNotContained(list, path.Replace(host, address.ToString()));
      }

      //if we're on a local share, add loopback and localhost
      if (HostEntry.HostName == Dns.GetHostName())
      {
        InsertIfNotContained(list, path.Replace(host, "127.0.0.1"));
        InsertIfNotContained(list, path.Replace(host, "localhost"));
      }


      //refresh network location list
      NetworkLocations = list;
      OnPropertyChanged(() => NetworkLocations);
    }

    /// <summary>
    /// Inserts an item into the list if there is no matching
    /// entry yet.
    /// </summary>
    private static void InsertIfNotContained<T>(ICollection<T> list, T item)
    {
      if (!list.Contains(item)) list.Add(item);
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Disposes the object and the underlying <see cref="Connection"/>.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Connection.Dispose();
      }

      base.Dispose(disposing);
    }

    #endregion

  }
}