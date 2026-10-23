using System;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_Connected_Share_When_Underlying_Connection_Changes
  {
    private ShareManager manager;


    [SetUp]
    public void Init()
    {
      manager = TestUtil.GetEmptyShareManager();
    }


    [TearDown]
    public void Cleanup()
    {
      manager.Dispose();
    }


    [Test]
    public void Share_Should_Indicate_Error_If_Connection_Is_Removed()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      
      using (new DriveShareGuard(configuration, false))
      {
        var share = manager.AddShare(configuration);
        manager.Connect(share);

        Thread.Sleep(4000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        //remove connection
        var result = NetworkUtil.RemoveMappedDriveConnection(configuration, false);
        Assert.AreEqual(DisconnectResult.Success, result);

        //check if the error was detected or not
        Thread.Sleep(4000);
        Assert.AreEqual(ConnectionStatus.ConnectionError, share.Status);
      }

    }


    [Test]
    public void Share_Should_Indicate_Error_If_Connection_Becomes_Degraded_And_Should_Recover_As_Well()
    {
      var configuration = TestUtil.GetDefaultConfiguration();

      using (new DriveShareGuard(configuration, false))
      {
        var share = manager.AddShare(configuration);
        manager.Connect(share);

        Thread.Sleep(4000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        //remove connection
        Assert.Fail("This test needs manual assistance - comment out to continue");
        "DISABLE CONNECTION NOW...".ToConsole();

        //check if the error was detected or not
        Thread.Sleep(30000);
        Assert.AreEqual(ConnectionStatus.ConnectionError, share.Status);

        "RE-ENABLE LAN NOW...".ToConsole();
        Thread.Sleep(30000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);
      }
    }


  }
}