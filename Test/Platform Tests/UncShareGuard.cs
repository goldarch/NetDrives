using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;

namespace Platform_Tests
{
  /// <summary>
  /// Creates a connection to a given UNC share and disconnects
  /// during disposal.
  /// </summary>
  internal class UncShareGuard : Guard
  {

    /// <summary>
    /// The status result of the initial connection attempt. Not set
    /// if the guard is created without automatic connection.
    /// </summary>
    public ConnectResult ConnectResult { get; private set; }


    /// <summary>
    ///  Inits the guard and immediately attempts to connect to the
    /// submitted configuration's specified share. Whether or not the
    /// result was successful can be determined by checking the
    /// <see cref="ConnectResult"/> property.
    /// </summary>
    public UncShareGuard(IShareConfiguration configuration, bool autoConnect)
      : base(() => NetworkUtil.CancelUncConnection(configuration.NetworkLocation, false))
    {
      Ensure.ArgumentNotNull(() => configuration);

      if (autoConnect)
      {
        ConnectResult = NetworkUtil.Connect(configuration);
      }
    }
  }
}
