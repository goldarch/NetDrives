using System.Security;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Encapsulates configuration settings for a single share configuration, which
  /// are serialized to XML.
  /// </summary>
  public interface IShareConfiguration
  {
    /// <summary>
    /// A descriptive (user) name for the share.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// The order index of the share. Used to display shares in user-configured order.
    /// </summary>
    int OrderIndex { get; set; }

    /// <summary>
    /// The drive letter to be registered for this share, if any.
    /// </summary>
    char? DriveLetter { get; set; }

    /// <summary>
    /// A label to be registered for this share, which causes file managers
    /// to display the share with this label.
    /// </summary>
    string VolumeLabel { get; set; }

    /// <summary>
    /// Whether a connection to the share should be automatically
    /// established on application startup. Defaults to false.
    /// </summary>
    bool AutoConnect { get; set; }

    /// <summary>
    /// The remote (network) address of the share.
    /// </summary>
    string NetworkLocation { get; set; }

    /// <summary>
    /// The user name to be used when connecting to the share
    /// </summary>
    string UserName { get; set; }

    /// <summary>
    /// The password to be used when connecting to the share.
    /// </summary>
    SecureString Password { get; set; }

    /// <summary>
    /// Defines how the login procedure should be handled
    /// (automatic login vs. prompting for credentials).
    /// </summary>
    LoginStrategy Login { get; set; }

    /// <summary>
    /// The configuration's unique ID.
    /// </summary>
    string ItemId { get; }

    /// <summary>
    /// Whether the item was disposed or not.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates a <see cref="ShareCredentials"/> instance that
    /// corresponds to the configured user name and password.
    /// </summary>
    /// <returns>Corresponding <see cref="ShareCredentials"/>
    /// instance.</returns>
    ShareCredentials CreateDefaultCredentials();
  }
}