namespace Hardcodet.NetDrives.ViewModel.Connections
{
  /// <summary>
  /// Determines whether (and how) to reconnect if a
  /// preceding connection attempt failed.
  /// </summary>
  public enum ReconnectionStrategy
  {
    /// <summary>
    /// Not reconnection is performed.
    /// </summary>
    DoNotReconnect = 0,
    /// <summary>
    /// The application attempts to reconnect with the
    /// unchanged configuration.
    /// </summary>
    Retry = 1,
    /// <summary>
    /// The user changes the share configuration.
    /// </summary>
    ReconfigureShare = 2,
    /// <summary>
    /// The user tries to log on with different user ID / password.
    /// </summary>
    LoginWithOtherCredentials = 3
  }
}
