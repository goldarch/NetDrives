using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_NetworkUtil_When_Evaluating_Drive_Letters
  {

    [SetUp]
    public void Init()
    {
    }


    [Test]
    public void Unused_Drive_Letters_Should_Be_Marked_Available()
    {

      var config = TestUtil.GetDefaultConfiguration();

      var letters = NetworkUtil.GetAvailableDriveLetters();
      Assert.IsFalse(NetworkUtil.IsDriveInUse(config.DriveLetter.Value));
      Assert.IsTrue(letters.Contains(config.DriveLetter.Value));
    }



    [Test]
    public void Reserved_Drive_Letter_Should_Return_Appropriate_Status()
    {
      var config = TestUtil.GetDefaultConfiguration();
      config.DriveLetter = 'H';
      string unc;
      var status = NetworkUtil.GetUncName(config, out unc);
      "Status = {0}".ToConsole(status);
      Console.Out.WriteLine("unc = {0}", unc);
    }



    [Test]
    public void Reserved_Drive_Letters_Should_Be_Marked_Unavailable()
    {
      var config = TestUtil.GetDefaultConfiguration();
      using (new DriveShareGuard(config))
      {
        var letters = NetworkUtil.GetAvailableDriveLetters();
        Assert.IsTrue(NetworkUtil.IsDriveInUse(config.DriveLetter.Value));
        Assert.IsFalse(letters.Contains(config.DriveLetter.Value));
      }

      //check right after disconnection
      Unused_Drive_Letters_Should_Be_Marked_Available();
    }


    

  }
}