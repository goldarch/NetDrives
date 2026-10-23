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
  public class Given_Share_When_Connecting
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
    public void Status_Should_Reflect_Proper_Connection_If_Connection_Has_Been_Made_A_While_Ago()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      using(new DriveShareGuard(configuration))
      {
        Thread.Sleep(5000);
        var share = manager.AddShare(configuration);
        Thread.Sleep(5000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);
      }
    }



    [Test]
    public void Status_Should_Reflect_Proper_Connection_If_Connection_Is_Made_With_Share_Already_In_Place()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      using (new DriveShareGuard(configuration))
      {
        var share = manager.AddShare(configuration);
        Thread.Sleep(5000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);
      }
    }



    [Test]
    public void Share_With_Error_Can_Recover()
    {
      //create invalid configuration
      var configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);



      using (var guard = new DriveShareGuard(configuration))
      {
        Assert.AreEqual(ConnectResult.Success, guard.ConnectResult);
        Assert.Fail("THIS TESTS NEEDS MANUAL ASSISTANCE - UNCOMMENT TO USE IT");

        "Disable network now...".ToConsole();
        Thread.Sleep(30000);
        
        var share = manager.AddShare(configuration);
        Assert.AreEqual(ConnectionStatus.ConnectionError, share.Status);

        "Reenable network now...".ToConsole();
        Thread.Sleep(30000);
        
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);
      }
    }

  }
}