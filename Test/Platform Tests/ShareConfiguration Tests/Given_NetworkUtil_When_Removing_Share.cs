using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests.Share_Tests
{
  public class Given_NetworkUtil_When_Removing_Share : ShareTestBase
  {
    private IShareConfiguration config;

    protected override void Init()
    {
      config = TestUtil.GetDefaultConfiguration();

      var result = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.Success, result);
    }

    protected override void Cleanup()
    {
      //just to make sure in case a test does not disconnect...
      NetworkUtil.RemoveMappedDriveConnection(config, false);
    }


    [Test]
    public void Removal_Should_Provide_Proper_Status_Without_Updating_The_Configuration_State()
    {
      var result = NetworkUtil.RemoveMappedDriveConnection(config, false);
      Assert.AreEqual(DisconnectResult.Success, result);
    }



    [Test]
    public void Removed_Drive_Should_No_Longer_Be_Available()
    {
      var di = new DriveInfo(config.DriveLetter.ToString());
      var folder = di.RootDirectory.FullName;
      Assert.IsTrue(Directory.Exists(folder));

      var result = NetworkUtil.RemoveMappedDriveConnection(config, false);
      Assert.AreEqual(DisconnectResult.Success, result);

      Assert.IsFalse(Directory.Exists(folder));      
    }


    [Test]
    public void Attempt_To_Remove_Externally_Disconnected_Share_Should_Indicate_There_Is_No_Connection()
    {
      var result = NetworkUtil.RemoveMappedDriveConnection(config, false);
      Assert.AreEqual(DisconnectResult.Success, result);

      result = NetworkUtil.RemoveMappedDriveConnection(config, false);
      Assert.AreEqual(DisconnectResult.NotConnected, result);
    }

  }


}
