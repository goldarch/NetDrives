using System;
using System.Linq;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_Share_Manager_When_Adding_Or_Removing_Shares
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
    public void Adding_Shares_Should_Update_Configuration_Store()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      manager.AddShare(configuration);
      Assert.IsTrue(manager.Store.ShareConfigurations.ContainsInstance(configuration));

      configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      manager.AddShare(configuration);
      Assert.IsTrue(manager.Store.ShareConfigurations.ContainsInstance(configuration));

      Assert.AreEqual(2, manager.Store.ShareConfigurations.Count());
    }



    [Test]
    public void Adding_Share_Configurations_Should_Create_ShareInstances()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      var share = manager.AddShare(configuration);
      Assert.AreSame(configuration, share.Configuration);
      Assert.IsTrue(manager.Shares.ContainsInstance(share));
      
      configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      share = manager.AddShare(configuration);
      Assert.AreSame(configuration, share.Configuration);
      Assert.IsTrue(manager.Shares.ContainsInstance(share));

      Assert.AreEqual(2, manager.Shares.Count);
    }



    [Test]
    public void Removing_Share_Should_Remove_Share_Instance_And_Configuration()
    {
      var configuration = TestUtil.GetDefaultConfiguration();
      var share = manager.AddShare(configuration);

      manager.RemoveShare(configuration);
      Assert.AreEqual(0, manager.Store.ShareConfigurations.Count());
      Assert.AreEqual(0, manager.Shares.Count);
    }




    [Test]
    public void Added_Shares_Should_Reflect_Status_If_A_Corresponding_Connection_Already_Exists()
    {
      var configuration = TestUtil.GetDefaultConfiguration();

      //create connection
      var status = NetworkUtil.Connect(configuration);
      Assert.AreEqual(ConnectResult.Success, status);

      Thread.Sleep(5000);
      
      using (new DriveShareGuard(configuration, false))
      {
        //add share
        var share = manager.AddShare(configuration);

        //should connect immediately
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        Thread.Sleep(4000);

        //make sure the status remains ok
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        NetworkUtil.RemoveMappedDriveConnection(configuration, false);
      }
    }

  }
}