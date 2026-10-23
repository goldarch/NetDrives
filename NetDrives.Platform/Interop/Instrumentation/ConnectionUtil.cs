using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// Network connection related helper methods.
  /// </summary>
  public static class ConnectionUtil
  {

    /// <summary>
    /// Checks all available network connections and returns the connection
    /// that is mapped to a given drive share.
    /// </summary>
    /// <param name="driveLetter">The drive letter to check for.</param>
    /// <returns>A matching network connection, if available. Otherwise null.</returns>
    public static NetworkConnection TryGetConnectionForDrive(char driveLetter)
    {
      //run query for the targeted drive
      string query = "LocalName = '{0}'";
      query = String.Format(query, driveLetter.ToDriveLetterString());
      var connections = RunQuery(query);

      //return the first item, if there is one
      return connections.FirstOrDefault(c => true);
    }


    /// <summary>
    /// Gets all network connections of the system.
    /// </summary>
    /// <returns>A list of connections.</returns>
    public static IList<NetworkConnection> GetConnections()
    {
      return NetworkConnection.GetInstances().Cast<NetworkConnection>().ToList();
    }


    /// <summary>
    /// Runs a WMI query for network connections using a given
    /// where clause that acts as a filter.
    /// </summary>
    /// <param name="condition">An optional condition. If the condition is empty,
    /// all connections of the system are being returned.</param>
    /// <exception cref="ManagementException">In case of an invalid query.</exception>
    public static IList<NetworkConnection> RunQuery(string condition)
    {
      return NetworkConnection.GetInstances(condition).Cast<NetworkConnection>().ToList();
    }
  }
}
