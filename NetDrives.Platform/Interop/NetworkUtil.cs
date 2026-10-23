using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.Commons.IO;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;

namespace Hardcodet.NetDrives.Platform.Interop
{
  /// <summary>
  /// Provides network related util and extension methods.
  /// </summary>
  public static class NetworkUtil
  {

    /// <summary>
    /// Establishes a connection to a given network share based on the
    /// submitted configuration.
    /// </summary>
    /// <param name="configuration">Share configuration to be used.</param>
    /// <returns>The status of the connection attempt, which is
    /// <see cref="ConnectResult.Success"/> in case the connection was
    /// successfully established.</returns>
    public static ConnectResult Connect(IShareConfiguration configuration)
    {
      var credentials = new ShareCredentials(configuration.UserName, configuration.Password);
      return Connect(configuration, credentials);
    }


    /// <summary>
    /// Establishes a connection to a given network share based on the
    /// submitted configuration.
    /// </summary>
    /// <param name="configuration">Share configuration to be used.</param>
    /// <param name="credentials">The credentials to be used to log in.</param>
    /// <returns>The status of the connection attempt, which is
    /// <see cref="ConnectResult.Success"/> in case the connection was
    /// successfully established.</returns>
    public static ConnectResult Connect(IShareConfiguration configuration, ShareCredentials credentials)
    {
      Ensure.ArgumentNotNull(() => configuration);

      //make sure we're using backslashes and there's not slash at the end of the URI
      var location = configuration.NetworkLocation ?? String.Empty;
      if (location.StartsWith("//")) location = location.Replace("/", @"\");
      if (location.EndsWith(@"\")) location = location.Substring(0, location.Length - 1);

      var resource = NetResource.Create(configuration.DriveLetter, location);

      string passwd = credentials.Password == null ? "" : credentials.Password.Unwrap();
      var status = ComImports.WNetAddConnection2A(ref resource, passwd, credentials.UserName, 0);
      if (Enum.IsDefined(typeof(ConnectResult), status))
      {
        return (ConnectResult) status;
      }
      else
      {
        string msg = "Got unknown network error {0} for when connection to '{1}'.";
        Debug.Write(String.Format(msg, status, location));
        return ConnectResult.UnknownConnectionFailure;
      }
    }






    /// <summary>
    /// Deregisters a given share from the system.
    /// </summary>
    /// <param name="configuration">The configuration that defines the share.</param>
    /// <param name="forceDisconnect">Whether to force disconnection, if the connection
    /// is actively used.</param>
    /// <returns>Disconnection status flag.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="configuration"/>
    /// is a null reference.</exception>
    /// <exception cref="InvalidOperationException">If the submitted configuration does
    /// not specify a drive letter.</exception>
    public static DisconnectResult RemoveMappedDriveConnection(IShareConfiguration configuration, bool forceDisconnect)
    {
      Ensure.ArgumentNotNull(() => configuration);

      if (!configuration.DriveLetter.HasValue)
      {
        string msg = "Configuration {0} does not specify a drive letter".Put(configuration.Name);
        throw new InvalidOperationException(msg);
      }

      //only cancel mapped drives, not the whole UNC connection
      var driveLetter = configuration.DriveLetter.Value.ToDriveLetterString();
      var status = ComImports.WNetCancelConnection2A(driveLetter, 1, forceDisconnect);

      if (Enum.IsDefined(typeof(DisconnectResult), status))
      {
        return (DisconnectResult)status;
      }
      else
      {
        return DisconnectResult.UnknownDisconnectionFailure;
      }
    }


    /// <summary>
    /// Disconnects a given network connection.
    /// </summary>
    /// <param name="connection">The connection to be closed.</param>
    /// <param name="forceDisconnect">Whether the the connection should be canceled
    /// even if there are active connections.</param>
    /// <returns></returns>
    public static DisconnectResult Disconnect(NetworkConnection connection, bool forceDisconnect)
    {
      string location = String.IsNullOrEmpty(connection.LocalName)
                            ? connection.RemotePath
                            : connection.LocalName;

      return CancelUncConnection(location, forceDisconnect);
    }



    /// <summary>
    /// Cancels a given connection to a remote server. This operations may affect
    /// several shares at once.
    /// </summary>
    /// <param name="uncAddress">The network address to be deregistered.</param>
    /// <param name="forceDisconnect">Whether the the connection should be canceled
    /// even if there are active connections.</param>
    /// <returns>Disconnection status flag.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="forceDisconnect"/>
    /// is a null reference.</exception>
    public static DisconnectResult CancelUncConnection(string uncAddress, bool forceDisconnect)
    {
      Ensure.ArgumentNotNull(() => uncAddress);

      var status = ComImports.WNetCancelConnection2A(uncAddress, 1, forceDisconnect);

      if (Enum.IsDefined(typeof(DisconnectResult), status))
      {
        return (DisconnectResult)status;
      }
      else
      {
        return DisconnectResult.UnknownDisconnectionFailure;
      }
    }


    /// <summary>
    /// Refreshes the status of a given share. If the
    /// share is not connected, this method returns immediately.
    /// </summary>
    public static GetConnectionStatus CheckStatus(IShareConfiguration configuration)
    {
      string unc;
      return GetUncName(configuration, out unc);
    }


    /// <summary>
    /// Refreshes a given connection's status and returns status
    /// information along with the share's UNC name.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="uncName"></param>
    /// <returns></returns>
    public static GetConnectionStatus GetUncName(IShareConfiguration configuration, out string uncName)
    {
      int length = 255;
      GetConnectionStatus status = GetConnectionStatus.MoreData;
      uncName = String.Empty;
      
      var driveLetter = configuration.DriveLetter.Value.ToDriveLetterString();
      //just make a loop in order to requery if the buffer was too small
      while (status == GetConnectionStatus.MoreData)
      {
        var sb = new StringBuilder(length);
        status = (GetConnectionStatus) ComImports.WNetGetConnection(driveLetter, sb, ref length);
        uncName = sb.ToString();
      }

      return status;
    }


    /// <summary>
    /// Clears all network connections of the system / user by
    /// invoking the net use command. This method invokes a background
    /// thread.
    /// </summary>
    public static void ClearSystemConnections()
    {
      ThreadPool.QueueUserWorkItem(delegate
                                     {
                                       ProcessStartInfo info = new ProcessStartInfo();
                                       info.FileName = "cmd";
                                       info.Arguments = "/K net use * /delete";
                                       info.WorkingDirectory = FileUtil.WorkingFolder;
                                       info.WindowStyle = ProcessWindowStyle.Normal;
                                       
                                       Process.Start(info);
                                     }
          );
    }


    /// <summary>
    /// Opens the console with the "net use" command. This method
    /// invokes a background thread.
    /// </summary>
    public static void ShowSystemConnectionsOnConsole()
    {
      ThreadPool.QueueUserWorkItem(delegate
                                     {
                                       ProcessStartInfo info = new ProcessStartInfo();
                                       info.FileName = "cmd";
                                       info.Arguments = "/K net use";
                                       info.WorkingDirectory = FileUtil.WorkingFolder;
                                       info.WindowStyle = ProcessWindowStyle.Normal;
                                       Process.Start(info);

                                       
                                     }
          );
    }


    /// <summary>
    /// Determines whether a given drive letter is already in use
    /// or not.
    /// </summary>
    /// <param name="driveLetter"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">If the submitted
    /// character is not a valid candidate (must be an alphabetic letter).
    /// </exception>
    public static bool IsDriveInUse(char driveLetter)
    {
      string letter = driveLetter.ToString().ToUpperInvariant();
      char letterChar = letter[0];

      if (letterChar < 'A' || letterChar > 'Z')
      {
        string msg = "'{0}' is not a valid drive letter".Put(driveLetter);
        throw new ArgumentOutOfRangeException(msg);
      }

      DriveInfo di = new DriveInfo(letter);
      return (di.DriveType != DriveType.NoRootDirectory);
    }




    /// <summary>
    /// Determines drive letters that have not been assigned to any
    /// logical drives.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Includes drive letters that have been reserved for network
    /// drives. Currently don't know how to work around this one.</remarks>
    public static IEnumerable<char> GetAvailableDriveLetters()
    {
      const int lower = 'A';
      const int upper = 'Z';

      List<char> availableLetters = new List<char>();
      var letters = Directory.GetLogicalDrives().Select(l => l.Substring(0, 1).ToUpperInvariant()[0]);
      for (int i=lower; i < upper; i++)
      {
        char letter = (char) i;
        if (!letters.Contains(letter))
        {
          availableLetters.Add(letter); 
        }
      }

      return availableLetters;
    }



    /// <summary>
    /// Tries to resolve a host name from a given string.
    /// </summary>
    /// <param name="uriString"></param>
    /// <returns></returns>
    public static string TryGetHostName(string uriString)
    {
      Uri uri;
      bool result = Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri);
      if (result) return uri.Host;

      uriString = uriString.Replace("/", @"\").Replace(@"\", "/");
      var entries = uriString.Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries);
      if (entries.Length > 0) return entries[0];

      return uriString;
    }

  }
}