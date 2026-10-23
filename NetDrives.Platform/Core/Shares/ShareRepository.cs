using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// A repository that maintains and monitors the share
  /// configurations and active shares of the application.
  /// </summary>
  public class ShareRepository
  {


    /// <summary>
    /// Raised if the connection status of a managed share is
    /// being changed.
    /// </summary>
    public event Action<Share> OnShareStatusChanged;



  }
}
