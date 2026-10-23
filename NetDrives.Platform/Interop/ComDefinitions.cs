using System;

namespace Hardcodet.NetDrives.Platform.Interop
{

  public enum ResourceScope
  {
    Connected = 1,
    GlobalNet = 2,
    Remembered = 3,
    Recent = 4,
    Context = 5
  } ;

  public enum ResourceType
  {
    Any = 0,
    Disk = 1,
    Print = 2,
    Reserved = 3
  } ;


  public enum ResourceUsage
  {
    Connectable = 0x00000001,
    Container = 0x00000002,
    NoLocalDevice = 0x00000004,
    Sibling = 0x00000008,
    Attached = 0x00000010,
    All = (Connectable | Container | Attached),
  } ;



  public enum ResourceDisplayType
  {
    Generic = 0x00000000,
    Domain = 0x00000001,
    Server = 0x00000002,
    Share = 0x00000003,
    File = 0x00000004,
    Group = 0x00000005,
    Network = 0x00000006,
    Root = 0x00000007,
    ShareAdmin = 0x00000008,
    Directory = 0x00000009,
    Tree = 0x0000000A,
    NdsContainer = 0x0000000A //according to MSDN, it's "A", too
  }



  /// <summary>
  /// Status flags for the <see cref="ComImports.WNetGetConnection"/>
  /// method.
  /// </summary>
  public enum GetConnectionStatus
  {
    /// <summary>
    /// The operation completed successfully.
    /// </summary>
    Success = 0,
    /// <summary>
    /// The string pointed to by the lpLocalName parameter is invalid.
    /// </summary>
    BadDevice = 1200,
    /// <summary>
    /// The device is not currently connected, but it is a persistent connection.
    /// For more information, see the following Remarks section.
    /// </summary>
    ConnectionUnavailable = 1201,
    /// <summary>
    /// A network-specific error occurred. To obtain a description of the error,
    /// call the WNetGetLastError function.
    /// </summary>
    ExtendedError = 1208,
    /// <summary>
    /// The buffer that was submitted to the function was too small.
    /// The lpnLength parameter points to a variable that contains the
    /// required buffer size. More entries are available with subsequent calls.
    /// </summary>
    MoreData = 234,
    /// <summary>
    /// Invalid operation.
    /// </summary>
    NotSupported = 50,
    /// <summary>
    /// None of the providers recognize the local name as having a connection.
    /// However, the network is not available for at least one provider to whom
    /// the connection may belong.
    /// </summary>
    NotNetworkOrBadPath = 1203,
    /// <summary>
    /// The network is unavailable.
    /// </summary>
    NoNetwork = 1222,
    /// <summary>
    /// The device specified by lpLocalName is not a redirected device.
    /// For more information, see the following Remarks section.
    /// </summary>
    NotConnected = 2250
    
  }
}