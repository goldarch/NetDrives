using System;
using System.IO;
using System.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ConnectionUtil_When_Querying_Connections
  {



    [Test]
    public void Querying_For_Mapped_Drive_Should_Return_Matching_Connection()
    {
      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      using (new DriveShareGuard(config))
      {
        var con = ConnectionUtil.TryGetConnectionForDrive(config.DriveLetter.Value);
        Assert.IsNotNull(con);
        Assert.AreEqual(con.LocalName[0], config.DriveLetter.Value);
      }
    }


    [Test]
    public void Querying_For_Unmapped_Drive_Should_Return_Null()
    {
      var config = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      NetworkUtil.RemoveMappedDriveConnection(config, false);

      var con = ConnectionUtil.TryGetConnectionForDrive(config.DriveLetter.Value);
      Assert.IsNull(con);
    }


    [Test]
    public void Querying_For_Non_Network_Drive_Should_Return_Null()
    {
      var drives = DriveInfo.GetDrives().Where(di => di.DriveType != DriveType.Network);
      foreach (var info in drives)
      {
        char driveLetter = info.Name[0];
        Assert.IsNull(ConnectionUtil.TryGetConnectionForDrive(driveLetter));
      }
    }

    [Test]
    public void Querying_For_All_Drives_Should_Return_Listing()
    {
      //query connections
      var connections = ConnectionUtil.GetConnections().Where(c => !String.IsNullOrEmpty(c.LocalName));

      //get all network drives
      var drives  = DriveInfo.GetDrives().Where(di => di.DriveType == DriveType.Network);

      //make sure the collections match
      Assert.AreEqual(drives.Count(), connections.Count());
      foreach (var info in drives)
      {
        DriveInfo di = info;
        Assert.IsTrue(connections.Contains(c => c.LocalName[0] == di.Name[0]), "Not found: " + di.Name);
      }
    }
  }
}