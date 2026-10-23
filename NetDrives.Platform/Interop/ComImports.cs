using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Hardcodet.NetDrives.Platform.Interop
{
  /// <summary>
  /// Provides COM import declarations.
  /// </summary>
  public static class ComImports
  {

    [DllImport("mpr.dll")]
    public static extern int WNetAddConnection2A(ref NetResource netResource, string password, string username, int flags);

    [DllImport("mpr.dll")]
    public static extern int WNetAddConnection3A(IntPtr hWndOwner, ref NetResource netResource, string password, string username, int flags);

    [DllImport("mpr.dll")]
    public static extern int WNetCancelConnection2A(string psName, int piFlags, bool pfForce);
    [DllImport("mpr.dll")]
    public static extern int WNetConnectionDialog(int phWnd, int piType);
    [DllImport("mpr.dll")]
    public static extern int WNetDisconnectDialog(int phWnd, int piType);
    [DllImport("mpr.dll")]
    public static extern int WNetRestoreConnectionW(int phWnd, string psLocalDrive);

    /// <summary>
    /// Gets information about a given network resource.
    /// </summary>
    /// <param name="localName"></param>
    /// <param name="remoteName">A string builder that gets the remote name.</param>
    /// <param name="length">The size of the <paramref name="remoteName"/> buffer (input).
    /// If the maximum length is not big enough, this parameter also returns the
    /// the required buffer size.</param>
    /// <returns>Status flag. If the buffer was not big enough,   </returns>
    [DllImport("mpr.dll", SetLastError = true)]
    public static extern int WNetGetConnection(string localName, StringBuilder remoteName, ref int length);
  }
}
