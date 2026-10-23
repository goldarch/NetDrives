using System;
using System.Linq;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ShareManager_When_Updating_Unconfigured_Connections
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
    public void Connections_Should_Be_Determined_At_Runtime_And_During_Initialization()
    {
      var c1 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      var c2 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var c3 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);

      Action action = () =>
                        {
                          NetworkUtil.CancelUncConnection(c1.NetworkLocation, false);
                          NetworkUtil.CancelUncConnection(c2.NetworkLocation, false);
                          NetworkUtil.RemoveMappedDriveConnection(c3, false);
                        };

      using (new Guard(action))
      {
        Assert.AreEqual(0, manager.UnmanagedConnections.Count());

        NetworkUtil.Connect(c1).ToConsole();
        NetworkUtil.Connect(c2).ToConsole();
        NetworkUtil.Connect(c3).ToConsole();

        Thread.Sleep(5000);

        Assert.AreEqual(3, manager.UnmanagedConnections.Count());
      }

    }


    [Test]
    public void Connection_List_Should_Be_Updated_If_Network_Connection_Is_Cancelled()
    {
      var c1 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      var c2 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var c3 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);

      Action action = () =>
      {
        NetworkUtil.CancelUncConnection(c1.NetworkLocation, false);
        NetworkUtil.CancelUncConnection(c2.NetworkLocation, false);
        NetworkUtil.RemoveMappedDriveConnection(c3, false);
      };

      using (new Guard(action))
      {
        Assert.AreEqual(0, manager.UnmanagedConnections.Count());

        NetworkUtil.Connect(c1);
        NetworkUtil.Connect(c2);
        NetworkUtil.Connect(c3);

        Thread.Sleep(5000);
        Assert.AreEqual(3, manager.UnmanagedConnections.Count());
        
        //kill UNC share
        NetworkUtil.CancelUncConnection(c2.NetworkLocation, false);
        Thread.Sleep(2000);
        Assert.AreEqual(2, manager.UnmanagedConnections.Count());

        //kill mapped drive share
        NetworkUtil.RemoveMappedDriveConnection(c3, false);
        Thread.Sleep(2000);
        Assert.AreEqual(1, manager.UnmanagedConnections.Count());
      }
    }

    
    [Test]
    public void Removing_A_Configured_Share_Should_Move_The_Connection_To_The_List_Of_Unconfigured_Connections()
    {
      var c1 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      var c2 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var c3 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);

      //need to cache locations because the configs will have been disposed when
      //it comes to disconnecting
      var c1Loc = c1.NetworkLocation;
      var c2Loc = c2.NetworkLocation;

      Action action = () =>
      {
        NetworkUtil.CancelUncConnection(c1Loc, false);
        NetworkUtil.CancelUncConnection(c2Loc, false);
        NetworkUtil.RemoveMappedDriveConnection(c3, false);
      };

      using (new Guard(action))
      {
        Assert.AreEqual(0, manager.UnmanagedConnections.Count());

        NetworkUtil.Connect(c1);
        NetworkUtil.Connect(c2);
        NetworkUtil.Connect(c3);

        Thread.Sleep(5000);

        //add share - reduces unmanaged connections list
        var share1 = manager.AddShare(c1);
        Assert.AreEqual(2, manager.UnmanagedConnections.Count());
        Assert.IsFalse(manager.UnmanagedConnections.ContainsInstance(share1.ConnectionInfo));

        //remove share again
        manager.RemoveShare(share1.Configuration);
        Assert.IsTrue(manager.UnmanagedConnections.ContainsInstance(share1.ConnectionInfo));
        
      }
    }


    [Test]
    public void Adding_A_Configuration_To_Manager_Should_Remove_Related_Connection_From_Unconfigured_List()
    {
      var c1 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      var c2 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var c3 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);

      Action action = () =>
      {
        NetworkUtil.CancelUncConnection(c1.NetworkLocation, false);
        NetworkUtil.CancelUncConnection(c2.NetworkLocation, false);
        NetworkUtil.RemoveMappedDriveConnection(c3, false);
      };

      using (new Guard(action))
      {
        Assert.AreEqual(0, manager.UnmanagedConnections.Count());

        NetworkUtil.Connect(c1);
        NetworkUtil.Connect(c2);
        NetworkUtil.Connect(c3);

        Thread.Sleep(5000);
        Assert.AreEqual(3, manager.UnmanagedConnections.Count());

        var share1 = manager.AddShare(c1);
        Assert.AreEqual(2, manager.UnmanagedConnections.Count());
        
        var share3 = manager.AddShare(c3);
        Assert.AreEqual(1, manager.UnmanagedConnections.Count());

        var share2 = manager.AddShare(c2);
        Assert.AreEqual(0, manager.UnmanagedConnections.Count());
      }
    }

    

  }
}