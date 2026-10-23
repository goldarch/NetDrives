using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hardcodet.NetDrives.Platform.Interop
{
  /// <summary>
  /// Status flags that are being returned upon removing a connection
  /// to a network share.<br/>
  /// These are basically return values of the <c>WNetCancelConnectionX</c>
  /// method:
  /// http://msdn.microsoft.com/en-us/library/aa385427.aspx
  /// </summary>
  public enum DisconnectResult
  {
    /// <summary>
    /// The connection was successfully removed.
    /// </summary>
    Success = 0,
    /// <summary>
    /// An unexpected failure occurred - no enum constant was defined for
    /// this case.
    /// </summary>
    UnknownDisconnectionFailure = -1,
    /// <summary>
    /// The user profile is in an incorrect format.
    /// </summary>
    BadProfile = 1206,
    /// <summary>
    /// The system is unable to open the user profile to process persistent connections.
    /// </summary>
    CannotOpenProfile = 1205,
    /// <summary>
    /// The device is in use by an active process and cannot be disconnected.
    /// </summary>
    DeviceInUse = 2404,
    /// <summary>
    /// A network-specific error occurred. To obtain a description of the error,
    /// call the WNetGetLastError function.
    /// </summary>
    ExtendedError = 1208,
    /// <summary>
    /// The name specified by the lpName parameter is not a redirected device,
    /// or the system is not currently connected to the device specified by
    /// the parameter.
    /// </summary>
    NotConnected = 2250,
    /// <summary>
    /// Files of the share are currently in use and disconnection was not
    /// forced.
    /// </summary>
    OpenFiles = 2401
  }
}
