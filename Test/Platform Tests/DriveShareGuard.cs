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
  /// Creates a connection to a given share and disconnects
  /// during disposal.
  /// </summary>
  internal class DriveShareGuard : Guard
  {

    /// <summary>
    /// The status result of the initial connection attempt.
    /// </summary>
    public ConnectResult ConnectResult { get; private set; }


    
    /// <summary>
    /// Inits the guard to work with the default configuration.
    /// </summary>
    public DriveShareGuard() : this(TestUtil.GetDefaultConfiguration())
    {
    }

    /// <summary>
    ///  Inits the guard and immediately attempts to connect to the
    /// submitted configuration's specified share. Whether or not the
    /// result was successful can be determined by checking the
    /// <see cref="ConnectResult"/> property.
    /// </summary>
    public DriveShareGuard(IShareConfiguration configuration) 
      : this(configuration, true)
    {
    }


    /// <summary>
    ///  Inits the guard and optionally connects to the underlying share. Whether or not the
    /// result was successful can be determined by checking the
    /// <see cref="ConnectResult"/> property.
    /// </summary>
    public DriveShareGuard(IShareConfiguration configuration, bool autoConnect)
      : base(() => NetworkUtil.RemoveMappedDriveConnection(configuration, false))
    {
      Ensure.ArgumentNotNull(() => configuration);
      if (autoConnect)
      {
        ConnectResult = NetworkUtil.Connect(configuration);
      }
    }
  }
}
