using System;
using System.IO;
using System.Threading;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;


namespace Platform_Tests
{
  [TestFixture]
  public class Given_Network_Util_When_Checking_Status
  {

    [Test]
    public void Evaluated_Status_Should_Match_Current_Connection_Status()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);

      var status = NetworkUtil.CheckStatus(configuration);
      Assert.AreEqual(GetConnectionStatus.NotConnected, status);

      using(new DriveShareGuard(configuration))
      {
        status = NetworkUtil.CheckStatus(configuration);
        Assert.AreEqual(GetConnectionStatus.Success, status);     
      }

      status = NetworkUtil.CheckStatus(configuration);
      Assert.AreEqual(GetConnectionStatus.NotConnected, status);
    }



    [Test]
    public void Evaluating_Status_If_Connection_Is_Lost_Should_Indicate_Missing_Network()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);

      var status = NetworkUtil.CheckStatus(configuration);
      Assert.AreEqual(GetConnectionStatus.NotConnected, status);

      DriveInfo di = null;
      using (new DriveShareGuard(configuration))
      {
        status = NetworkUtil.CheckStatus(configuration);
        //Assert.AreEqual(GetConnectionStatus.NoNetwork, status);
        Console.Out.WriteLine("status = {0}", status);

        di = new DriveInfo(configuration.DriveLetter.ToString());
        Console.Out.WriteLine("di.IsReady = {0}", di.IsReady);
        Console.Out.WriteLine("di.RootDirectory = {0}", di.RootDirectory);

      }

      //di = new DriveInfo(configuration.DriveLetter.ToString());
      Console.Out.WriteLine("di.IsReady = {0}", di.IsReady);
      Console.Out.WriteLine("di.RootDirectory = {0}", di.RootDirectory);
    }
    
    

  }
}