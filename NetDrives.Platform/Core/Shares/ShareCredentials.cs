using System.Security;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Contains credentials that were used to perform a log in.
  /// </summary>
  public class ShareCredentials
  {
    /// <summary>
    /// The user name that is used to log on to the share.
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// The password that is used to log on to the share.
    /// </summary>
    public SecureString Password { get; private set; }


    /// <summary>
    /// Inits the credentials with user name and password to be used.
    /// </summary>
    /// <param name="userName">Log on user name, if required.</param>
    /// <param name="password">The password, if required.</param>
    public ShareCredentials(string userName, SecureString password)
    {
      UserName = userName;
      Password = password;
    }



  }
}
