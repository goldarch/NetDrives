using System;
using System.Threading;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ShareManager_When_Disconnecting_Shares
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
    public void Disconnecting_A_Share_Should_Update_Status()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      var share = manager.AddShare(configuration);
      using(new DriveShareGuard(configuration))
      {
        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        var result = manager.Disconnect(share, false);
        Assert.AreEqual(DisconnectResult.Success, result);

        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.ManuallyDisconnected, share.Status);
      }

    }


    [Test]
    public void Disconnecting_A_Share_With_Broken_Connection_Should_Set_Share_To_Manually_Disconnected()
    {
      //make sure we can properly disconnect a share that is not valid
      var configuration = TestUtil.GetDefaultConfiguration();
      var share = manager.AddShare(configuration);
      using (new DriveShareGuard(configuration))
      {
        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        //disconnect (not via manager)
        NetworkUtil.RemoveMappedDriveConnection(configuration, false);

        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.ConnectionError, share.Status);

        //disconnect via manager
        var result = manager.Disconnect(share, false);
        Assert.AreEqual(DisconnectResult.NotConnected, result);
        Assert.AreEqual(ConnectionStatus.ManuallyDisconnected, share.Status);

        //make sure that the share remains disconnected if the connection comes up again
        Assert.AreEqual(ConnectResult.Success, NetworkUtil.Connect(configuration));
      }

    }



    [Test]
    public void Manually_Disconnected_Share_Should_Switch_Back_To_Connected_If_The_Connection_Comes_Back_Otherwise()
    {
      //if a connection reappers, its just a status update
      //make sure we can properly disconnect a share that is not valid
      var configuration = TestUtil.GetDefaultConfiguration();
      var share = manager.AddShare(configuration);
      
      using (new DriveShareGuard(configuration))
      {
        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        var result = manager.Disconnect(share, false);
        Assert.AreEqual(DisconnectResult.Success, result);

        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.ManuallyDisconnected, share.Status);
        
        //reconnect via manager
        NetworkUtil.Connect(configuration);

        //disconnect via manager
        Thread.Sleep(3000);
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);
      }

    }



  }
}