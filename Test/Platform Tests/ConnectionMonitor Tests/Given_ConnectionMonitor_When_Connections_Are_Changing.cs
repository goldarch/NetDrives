using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ConnectionMonitor_When_Connections_Are_Changing
  {
    private ConnectionMonitor monitor;


    [SetUp]
    public void Init()
    {
      monitor = new ConnectionMonitor(1);
    }


    [TearDown]
    public void Cleanup()
    {
      monitor.Dispose();
    }



    [Test]
    public void Monitor_Should_Reflect_All_Current_Connections_After_Creation()
    {
      var connections = ConnectionUtil.GetConnections();

      //give it time 
      Thread.Sleep(4000);
      Assert.AreEqual(connections.Count, monitor.Connections.Count());

      //check with default connections
      foreach (var connection in connections)
      {
        Assert.IsTrue(monitor.Connections.Contains(c => c.Connection.Name == connection.Name));
      }

      //create connections
      var c1 = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      var c2 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);
      Assert.AreEqual(ConnectResult.Success, NetworkUtil.Connect(c1));
      Assert.AreEqual(ConnectResult.Success, NetworkUtil.Connect(c2));

      //create new monitor
      monitor.Dispose();
      monitor = new ConnectionMonitor();
      Thread.Sleep(4000);
      Assert.AreEqual(connections.Count + 2, monitor.Connections.Count());

      //cleanup
      NetworkUtil.RemoveMappedDriveConnection(c1, false);
      NetworkUtil.RemoveMappedDriveConnection(c2, false);
    }


    [Test]
    public void Creating_A_Connection_Should_Add_Item_In_Monitor()
    {
      Thread.Sleep(4000);
      var infos = monitor.Connections.ToArray();

      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      using(new DriveShareGuard(config))
      {
        Thread.Sleep(3000);
        Assert.AreEqual(infos.Length + 1, monitor.Connections.Count());

        var match = monitor.FindMatchingConnectionInfo(config);
        Assert.IsNotNull(match);
        Assert.IsTrue(
            match.NetworkLocations.Contains(
                loc => loc.ToLowerInvariant().Contains(config.NetworkLocation.ToLowerInvariant())));
      }
    }


    [Test]
    public void Removing_A_Connection_Should_Remove_Item_From_Monitor()
    {
      Thread.Sleep(4000);
      int count;

      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      using (new DriveShareGuard(config))
      {
        Thread.Sleep(2000);
        count = monitor.Connections.Count();
      }

      Thread.Sleep(2000);
      Assert.AreEqual(count -1, monitor.Connections.Count());
      Assert.IsNull(monitor.FindMatchingConnectionInfo(config));
    }


    [Test]
    public void Changing_Connection_Status_Should_Result_In_Monitor_Event()
    {
      //also make sure that add/remove results in events...
      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);

      int counter = 0;
      monitor.ConnectionStatusChanged += m => counter++;

      Assert.AreEqual(0, counter);
      using (new DriveShareGuard(config))
      {
        Thread.CurrentThread.Join(4000);
        Assert.AreEqual(1, counter);

        NetworkUtil.RemoveMappedDriveConnection(config, false);
        Thread.CurrentThread.Join(4000);
        Assert.AreEqual(2, counter);
      }
    }


    [Test]
    public void Creating_Two_Matching_Connections_Should_Return_Correct_Match_From_Monitor()
    {
      //one connection maps a drive, the other one doesn't, the third one is another use
      var c1 = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      var c2 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);

      Assert.IsNull(monitor.FindMatchingConnectionInfo(c2));

      var s1 = NetworkUtil.Connect(c1);
      var s2 = NetworkUtil.Connect(c2);

      Assert.AreEqual(ConnectResult.Success, s1);
      Assert.AreEqual(ConnectResult.Success, s2);

      Thread.Sleep(4000);

      //the first config refers to a drive letter so the others are ignored anyway
      var info1 = monitor.FindMatchingConnectionInfo(c1);
      Assert.IsNotNull(info1);
      Assert.AreEqual(c1.DriveLetter.ToDriveLetterString(), info1.Connection.GetDriveLetterString());

      //the second config only refers to a connection - it's there whether we use
      //a drive or not
      var info2 = monitor.FindMatchingConnectionInfo(c2);
      Assert.IsNotNull(info2);
      Assert.IsEmpty(info2.Connection.GetDriveLetterString());

      NetworkUtil.RemoveMappedDriveConnection(c1, false);
      NetworkUtil.CancelUncConnection(c2.NetworkLocation, false);
    }


    [Test]
    public void TestDisabling()
    {
      TestUtil.SetNicStatus(true);
    }


    [Test]
    public void Connection_Changes_Should_Update_Connection_Info_Items_With_Lates_NetworkConnection()
    {
      Assert.Fail("This test needs manual assistance, it cannot be automated.");

      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      using (new DriveShareGuard(config))
      {
        Thread.Sleep(2000);
        var connectionInfo = monitor.FindMatchingConnectionInfo(config);

        Assert.IsNotNull(connectionInfo);
        Assert.AreEqual("Connected", connectionInfo.Connection.ConnectionState);

        Console.Out.WriteLine("Disconnect now, then try accessing share through explorer..");
        new Ping().Send(connectionInfo.HostEntry.HostName);
        Thread.Sleep(35000);

        var networkStatus = connectionInfo.Connection.ConnectionState;
        "Network Status: {0}".ToConsole(networkStatus);
        Assert.AreNotEqual("Connected", networkStatus);
      }
    }

  }
}