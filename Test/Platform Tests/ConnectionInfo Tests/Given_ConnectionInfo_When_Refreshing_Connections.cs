using System.Linq;
using System.Net;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ConnectionInfo_When_Refreshing_Connections
  {
    private IShareConfiguration config;
    private NetworkConnection connection;

    [SetUp]
    public void Init()
    {
      config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      NetworkUtil.Connect(config);

      connection = ConnectionUtil.TryGetConnectionForDrive(config.DriveLetter.Value);
    }


    public void TearDown()
    {
      NetworkUtil.RemoveMappedDriveConnection(config, false);
    }


    [Test]
    public void Default_Connection_Info_Should_Provide_Share_Location_Only()
    {
      //create info
      NetworkConnectionInfo ci = new NetworkConnectionInfo(connection);

      //immediately query network locations
      var list = ci.NetworkLocations.ToList();
      Assert.AreEqual(1, list.Count);
      Assert.AreEqual(config.NetworkLocation, list[0]);
    }



    [Test]
    public void Resolved_Remote_Locations_Should_Provide_Host_Name_And_IP_Address_If_Configured_With_IP()
    {
      NetworkConnectionInfo conInfo = new NetworkConnectionInfo(connection);
      conInfo.RefreshConnectionInfos(null);
      Thread.Sleep(5000);

      var list = conInfo.NetworkLocations.ToList();
      Assert.AreEqual(2, list.Count);
      //make sure we have two different locations
      StringAssert.AreNotEqualIgnoringCase(list[0], list[1]);

      var entry = Dns.GetHostEntry(conInfo.HostEntry.HostName);
      StringAssert.AreEqualIgnoringCase(entry.HostName, conInfo.HostEntry.HostName);
    }


    [Test]
    public void TESTNAME()
    {
      //Dns.GetHostAddresses("krusty").Do(ip => ip.ToString().ToConsole());
      var he = Dns.GetHostEntry("127.0.0.1"); //.AddressList.Do(ip => ip.ToString().ToConsole());
      he.Aliases.Do(al => al.ToConsole());
    }


    //[Test]
    //public void Resolved_Remote_Locations_Should_Provide_Host_Name_And_IP_Address_If_Configured_With_HostName()
    //{
    //  NetworkConnectionInfo conInfo = new NetworkConnectionInfo(connection);
    //  conInfo.RefreshConnectionInfos(null);
    //  Thread.Sleep(5000);

    //  var list = conInfo.NetworkLocations.ToList();
    //  Assert.AreEqual(2, list.Count);
    //  //make sure we have two different locations
    //  StringAssert.AreNotEqualIgnoringCase(list[0], list[1]);

    //  var entry = Dns.GetHostEntry(conInfo.HostEntry.HostName);
    //  StringAssert.AreEqualIgnoringCase(entry.HostName, conInfo.HostEntry.HostName);
    //}



    [Test]
    public void Refreshing_Connections_Should_Produce_Property_Change_Event_For_Network_Locations()
    {
      NetworkConnectionInfo conInfo = new NetworkConnectionInfo(connection);
      conInfo.AssertPropertyChangedEventCount("NetworkLocations", 1, ci =>
                                                               {
                                                                 ci.RefreshConnectionInfos(null);
                                                                 Thread.Sleep(5000);
                                                               });

      conInfo.NetworkLocations.Do(loc => loc.ToConsole());
    }


    [Test]
    public void Refresh_Should_Invoke_Callback()
    {
      NetworkConnectionInfo ci = new NetworkConnectionInfo(connection);
      ci.RefreshConnectionInfos(Assert.IsTrue);
    }



    [Test]
    public void Local_Connection_Through_Host_Should_Provide_Host_Name_And_Localhost()
    {
      //map through host name -> make sure the host name is available

      var localConfig = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);
      using(new DriveShareGuard(localConfig))
      {
        var driveLetter = localConfig.DriveLetter.Value;
        var localConnection = ConnectionUtil.TryGetConnectionForDrive(driveLetter);
        NetworkConnectionInfo ci = new NetworkConnectionInfo(localConnection);
        ci.RefreshConnectionInfos(null);

        Thread.Sleep(5000);

        string hostName = Dns.GetHostName();
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains(hostName)));
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains("127.0.0.1")));
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains("localhost")));
      }
    }




    [Test]
    public void Local_Connection_Through_LocalHost_Should_Provide_Host_Name_And_Localhost()
    {
      //map through localhost -> make sure the host name is available
      var localConfig = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);
      localConfig.NetworkLocation = localConfig.NetworkLocation.Replace("127.0.0.1", "localhost");

      using (new DriveShareGuard(localConfig))
      {
        var driveLetter = localConfig.DriveLetter.Value;
        var localConnection = ConnectionUtil.TryGetConnectionForDrive(driveLetter);
        NetworkConnectionInfo ci = new NetworkConnectionInfo(localConnection);
        ci.RefreshConnectionInfos(null);

        Thread.Sleep(5000);

        string hostName = Dns.GetHostName();
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains(hostName)));
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains("127.0.0.1")));
        Assert.IsTrue(ci.NetworkLocations.Contains(loc => loc.Contains("localhost")));
      }
    }


    [Test]
    public void Remote_Connection_Should_Not_Link_To_Local_Host()
    {
      //map through localhost -> make sure the host name is available
      var localConfig = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);

      using (new DriveShareGuard(localConfig))
      {
        var driveLetter = localConfig.DriveLetter.Value;
        var localConnection = ConnectionUtil.TryGetConnectionForDrive(driveLetter);
        NetworkConnectionInfo ci = new NetworkConnectionInfo(localConnection);
        ci.RefreshConnectionInfos(null);

        Thread.Sleep(5000);

        string hostName = Dns.GetHostName();
        Assert.IsFalse(ci.NetworkLocations.Contains(loc => loc.Contains(hostName)));
        Assert.IsFalse(ci.NetworkLocations.Contains(loc => loc.Contains("127.0.0.1")));
        Assert.IsFalse(ci.NetworkLocations.Contains(loc => loc.Contains("localhost")));
      }
    }


  }
}