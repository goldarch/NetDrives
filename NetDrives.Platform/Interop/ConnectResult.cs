using Hardcodet.NetDrives.Platform.Util;

namespace Hardcodet.NetDrives.Platform.Interop
{
  /// <summary>
  /// Status flags that are being returned upon creating a connection
  /// to a network share.<br/>
  /// These are basically return values of the <c>WNetAddConnectionX</c>
  /// method:
  /// http://msdn.microsoft.com/en-us/library/aa385413(VS.85).aspx
  /// </summary>
  public enum ConnectResult
  {
    /// <summary>
    /// The connection was successfully created.
    /// </summary>
    [LocDescription("ConnectResult_Success")]
    Success = 0,
    /// <summary>
    /// The connection was lost. This is not a system event number, but
    /// set if a previously established connection was lost.
    /// </summary>
    [LocDescription("ConnectResult_Lost")]
    Lost = -1,
    /// <summary>
    /// An unexpected failure occurred - no enum constant was defined for
    /// this case.
    /// </summary>
    [LocDescription("ConnectResult_UnknownConnectionFailure")]
    UnknownConnectionFailure = -2,
    [LocDescription("ConnectResult_Degraded")]
    Degraded = -3,
    /// <summary>
    /// The submitted user ID is unknown, or the caller does not
    /// have access to the network resource. 
    /// </summary>
    [LocDescription("ConnectResult_UnknownUserOrAccessDenied")]
    UnknownUserOrAccessDenied = 5,
    /// <summary>
    /// A device attached to the system is not functioning.
    /// </summary>
    [LocDescription("ConnectResult_GeneralNetworkFailure")]
    GeneralNetworkFailure = 31,
    /// <summary>
    /// The local device specified by the lpLocalName member
    /// is already connected to a network resource.
    /// </summary>
    [LocDescription("ConnectResult_DriveLetterAlreadyAssigned")]
    DriveLetterAlreadyAssigned = 85,
    /// <summary>
    /// The type of local device and the type of network
    /// resource do not match.
    /// </summary>
    [LocDescription("ConnectResult_BadDeviceType")]
    BadDeviceType = 66,
    /// <summary>
    /// The value specified by lpLocalName is invalid.
    /// </summary>
    [LocDescription("ConnectResult_BadDevice")]
    BadDevice = 1200,
    /// <summary>
    /// The server (host) address could not be resolved.
    /// </summary>
    [LocDescription("ConnectResult_HostNotFound")]
    HostNotFound = 53,
    /// <summary>
    /// The specified share was not found.
    /// </summary>
    [LocDescription("ConnectResult_ShareNotFound")]
    ShareNotFound = 55,
    /// <summary>
    /// The specified server cannot perform the requested operation.
    /// </summary>
    [LocDescription("ConnectResult_InvalidServerOperation")]
    InvalidServerOperation = 58,
    /// <summary>
    /// The value specified by the lpRemoteName member
    /// is not acceptable to any network resource provider,
    /// either because the resource name is invalid, or because
    /// the named resource cannot be located.
    /// </summary>
    [LocDescription("ConnectResult_InvalidNetworkAddressFormat")]
    InvalidNetworkAddressFormat = 67,
    /// <summary>
    /// The specified user name was invalid (invalid format).
    /// </summary>
    [LocDescription("ConnectResult_BadUserNameFormat")]
    BadUserNameFormat = 2202,
    /// <summary>
    /// The user profile is in an incorrect format.
    /// </summary>
    [LocDescription("ConnectResult_BadProfile")]
    BadProfile = 1206,
    /// <summary>
    /// The value specified by the lpProvider member does not
    /// match any provider.
    /// </summary>
    [LocDescription("ConnectResult_BadProvider")]
    BadProvider = 1204,
    /// <summary>
    /// The router or provider is busy, possibly initializing.
    /// The caller should retry.
    /// </summary>
    [LocDescription("ConnectResult_NetworkBusy")]
    NetworkBusy = 170,
    /// <summary>
    /// The attempt to make the connection was canceled by the user
    /// through a dialog box from one of the network resource providers,
    /// or by a called resource.
    /// </summary>
    [LocDescription("ConnectResult_Cancelled")]
    Cancelled = 1223,
    /// <summary>
    /// Multiple connections to a server or shared resource by the same user,
    /// using more than one user name, are not allowed.
    /// Disconnect all previous connections to the server or shared resource
    /// and try again.
    /// </summary>
    [LocDescription("ConnectResult_SessionCredentialConflict")]
    SessionCredentialConflict = 1219,
    /// <summary>
    /// The system is unable to open the user profile to process
    /// persistent connections.
    /// </summary>
    [LocDescription("ConnectResult_CannotOpenProfile")]
    CannotOpenProfile = 1205,
    /// <summary>
    /// The requested drive letter is already registered
    /// for another connection.
    /// </summary>
    [LocDescription("ConnectResult_DriveLetterAlreadyRegistered")]
    DriveLetterAlreadyRegistered = 1202,
    /// <summary>
    /// A network-specific error occurred. Call the WNetGetLastError
    /// function to obtain a description of the error.
    /// </summary>
    [LocDescription("ConnectResult_ExtendedError")]
    ExtendedError = 1208,
    /// <summary>
    /// The specified password is invalid and the CONNECT_INTERACTIVE flag is not set.
    /// Also returned in case of an invalid user ID if a standard UNC connection is
    /// made.<br/>
    /// From a user's point of view, <see cref="LoginFailure"/> and <see cref="InvalidCredentials"/>
    /// are interchangeable.
    /// </summary>
    [LocDescription("ConnectResult_InvalidCredentials")]
    InvalidCredentials = 86,
    /// <summary>
    /// Logon failure: unknown user name or bad password. This error appears to be
    /// used if a drive is mapped (as opposite to just creating a UNC connection).<br/>
    /// From a user's point of view, <see cref="LoginFailure"/> and <see cref="InvalidCredentials"/>
    /// are interchangeable.
    /// </summary>
    [LocDescription("ConnectResult_LoginFailure")]
    LoginFailure = 1326,
    /// <summary>
    /// The operation cannot be performed because a network component is not started
    /// or because a specified name cannot be used.
    /// </summary>
    [LocDescription("ConnectResult_NoNetworkOrInvalidNetworkAddress")]
    NoNetworkOrInvalidNetworkAddress = 1203,
    /// <summary>
    /// The network is unavailable.
    /// </summary>
    [LocDescription("ConnectResult_NetworkUnavailable")]
    NetworkUnavailable = 1222,
    /// <summary>
    /// The operation being requested was not performed because
    /// the user has not been authenticated.
    /// </summary>
    [LocDescription("ConnectResult_UserNotAuthenticated")]
    UserNotAuthenticated = 1244
  }
}