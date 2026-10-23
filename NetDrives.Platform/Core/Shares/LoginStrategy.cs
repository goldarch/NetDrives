namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Login strategies.
  /// </summary>
  public enum LoginStrategy
  {
    /// <summary>
    /// Try an automatic login.
    /// </summary>
    Automatic = 0,
    /// <summary>
    /// Prompt for user ID and password before connecting.
    /// </summary>
    Prompt = 1

  }
}
