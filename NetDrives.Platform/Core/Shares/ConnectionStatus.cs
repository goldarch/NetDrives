namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Provides possible connection states for a given
  /// <see cref="Share"/>.
  /// </summary>
  public enum ConnectionStatus
  {
    /// <summary>
    /// The status was reset, e.g. because of a configuration
    /// change.
    /// </summary>
    Reset = -1,
    /// <summary>
    /// No connection was established yet.
    /// </summary>
    Disconnected = 0,
    /// <summary>
    /// An existing connection was removed on request.
    /// </summary>
    ManuallyDisconnected = 1,
    /// <summary>
    /// A valid connection has been established.
    /// </summary>
    Connected = 2,
    /// <summary>
    /// The configured could not be established or it was
    /// lost during operation.
    /// </summary>
    ConnectionError = 3,
    /// <summary>
    /// The network was degraded.
    /// </summary>
    Degraded = 4
  }
}
