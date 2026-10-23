using System;
using System.Management;
using System.Threading;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_InstrumentationObserver_When_Registering_Query
  {
    private WmiClassObserver observer;

    [SetUp]
    public void Init()
    {
      observer = new NetworkConnectionObserver(1);
    }


    [TearDown]
    public void Cleanup()
    {
      observer.Dispose();
    }

    
    [Test]
    public void Adding_And_Removing_Connection_Should_Indicate_Proper_Event_Class()
    {
      WmiEventArgs args = null;
      observer.WmiEventFired += (s, e) => args = e;

      var configuration = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      using (new UncShareGuard(configuration, false))
      {
        Assert.IsNull(args);
        NetworkUtil.Connect(configuration);
        Thread.Sleep(3000);

        Assert.IsNotNull(args);
        Assert.AreEqual(WmiEventClass.InstanceCreation, args.EventClass);
      }

      Thread.Sleep(2000);
      Assert.AreEqual(WmiEventClass.InstanceDeletion, args.EventClass);
    }


    [Test]
    public void Disposed_Observers_Should_Not_Raise_Any_Events()
    {
      WmiEventArgs args = null;
      observer.WmiEventFired += (s, e) => args = e;

      //immediately dispose and suppress event publishing
      observer.Dispose();

      var configuration = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      using (new UncShareGuard(configuration, false))
      {
        NetworkUtil.Connect(configuration);
        Thread.Sleep(3000);
      }

      Assert.IsNull(args);
    }


    [Test]
    public void CreateObserver()
    {
      //use helper method to create query with 5 seconds interval
      var query = WmiClassObserver.CreateQuery("Win32_NetworkConnection",
                                               WmiEventClass.AnyInstanceOperation,
                                               5);

      //create observer
      observer = new WmiClassObserver(query);
      observer.WmiEventFired += OnWmiEventFired;

      CreateTempShare();
    }


    /// <summary>
    /// Raised if the observer detects a WMI event.
    /// </summary>
    private void OnWmiEventFired(object sender, WmiEventArgs e)
    {
      //the query target provides connection information
      ManagementBaseObject targetInstance = e.GetTargetInstance();
      //get the remote path (property of the Win32_NetworkConnection class)
      string remotePath = (string) targetInstance.GetPropertyValue("RemoteName");


      //tells us whether connection was created, deleted or modified
      switch (e.EventClass)
      {
        case WmiEventClass.InstanceCreation:
          Console.Out.WriteLine("Network connection {0} was created.", remotePath);
          break;
        case WmiEventClass.InstanceDeletion:
          Console.Out.WriteLine("Network connection {0} was deleted.", remotePath);
          break;
        case WmiEventClass.InstanceModification:
          Console.Out.WriteLine("Status of connection to {0} changed.", remotePath);
          break;
      }
    }


    [Test]
    public void Given_()
    {
      WqlEventQuery query;

      //register a connection change listener
      query = WmiClassObserver.CreateQuery("Win32_NetworkConnection",
                                           WmiEventClass.AnyInstanceOperation);
      var connectionObserver = new WmiClassObserver("Connection", query);

      //register a listener for created logical drives
      query = WmiClassObserver.CreateQuery("Win32_LogicalDisk",
                                           WmiEventClass.InstanceCreation);
      var diskObserver = new WmiClassObserver("Disk", query);


      //create composite observer
      var composite = new CompositeWmiObserver(connectionObserver, diskObserver);
      composite.WmiEventFired += OnWmiEventFired2;

      CreateTempShare();
    }


    //maps a drive and clears connection after a while
    private void CreateTempShare()
    {
      var config = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      config.DriveLetter = 'J';
      using (var guard = new DriveShareGuard(config))
      {
        Thread.Sleep(10000);
      }
    }


    private void OnWmiEventFired2(object sender, WmiEventArgs e)
    {
      switch(e.Observer.ObserverId)
      {
        case "Connection":
          Console.Out.WriteLine("A network connection was modified");
          break;
        case "Disk":
          Console.Out.WriteLine("A logical disk was created");
          break;
      }
    }
  }
}