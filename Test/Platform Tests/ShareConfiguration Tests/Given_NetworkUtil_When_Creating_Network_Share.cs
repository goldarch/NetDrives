using System;
using System.IO;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop;
using NUnit.Framework;

namespace Platform_Tests.Share_Tests

{
  [TestFixture]
  public class Given_NetworkUtil_When_Creating_Network_Share : ShareTestBase
  {
    private IShareConfiguration config;

    protected override void Init()
    {
      config = TestUtil.GetDefaultConfiguration();

      var result = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.Success, result);
    }

    protected override void Cleanup()
    {
      NetworkUtil.RemoveMappedDriveConnection(config, false);
    }


    [Test]
    public void Manager_Should_Created_Network_Share_With_Correct_Drive_Letter()
    {
      DriveInfo di = new DriveInfo(config.DriveLetter.ToString());
      Assert.AreEqual(DriveType.Network, di.DriveType);
      Assert.IsTrue(di.RootDirectory.Exists);
    }


    [Test]
    public void Drive_Label_Should_Be_Correctly_Assigned()
    {
      throw new NotImplementedException(""); //TODO provide implementation (feature currently not available)
    }


    [Test]
    public void Request_For_Already_Used_Drive_Letter_Should_Fail()
    {
      var result = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.DriveLetterAlreadyAssigned, result);
    }


    [Test]
    public void Connecting_With_Wrong_User_Name_Should_Fail()
    {
      Cleanup();

      var invalidConfig = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      invalidConfig.UserName = "XXX";
      var status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.UnknownUserOrAccessDenied, status);
    }

    
    [Test]
    public void Connecting_With_Wrong_Password_Should_Fail()
    {
      Cleanup();

      var invalidConfig = TestUtil.GetDefaultConfiguration();
      invalidConfig.Password = "XXX".ToSecureString();
      var status = NetworkUtil.Connect(invalidConfig);

      //this is ridiculous - sometimes its LoginFailure, sometimes InvalidCredentials
      Assert.IsTrue(status.Is(ConnectResult.LoginFailure, ConnectResult.InvalidCredentials));
    }


    /// <summary>
    /// Ending slashes result in very unspecific general network
    /// exceptions - networkutil should edit them out.
    /// </summary>
    [Test]
    public void Share_Location_May_Contain_Slash_As_Suffix()
    {
      Cleanup();

      config = TestUtil.GetDefaultConfiguration();
      Assert.IsFalse(config.NetworkLocation.EndsWith(@"\"));

      config.NetworkLocation += @"\";
      var status = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.Success, status);
    }


    [Test]
    public void Share_Location_May_Not_Contain_Forward_Slashes()
    {
      Cleanup();

      config.NetworkLocation = config.NetworkLocation.Replace(@"\", "/");
      if (!config.NetworkLocation.EndsWith(@"\")) config.NetworkLocation += @"/";
      
      var status = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.Success, status);
    }


    [Test]
    public void Connecting_To_Unknown_Host_Should_Fail()
    {
      Cleanup();
      
      Console.Out.WriteLine("This will take a while due to network timeouts...");

      var invalidConfig = TestUtil.GetDefaultConfiguration();
      //numeric IP
      invalidConfig.NetworkLocation = @"\\10.0.0.73\xxx";
      var status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.HostNotFound, status);

      //invalid host name
      invalidConfig.NetworkLocation = @"\\UNKNOWN\xxx";
      status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.HostNotFound, status);
    }



    [Test]
    public void Connecting_To_Unknown_Share_On_Existing_Host_Should_Fail()
    {
      Cleanup();

      var invalidConfig = TestUtil.GetDefaultConfiguration();
      invalidConfig.NetworkLocation = Path.Combine(invalidConfig.NetworkLocation, "XXX");
      var status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.ShareNotFound, status);
    }



    [Test]
    public void Specifying_An_Invalid_Network_Address_Should_Fail_Gracefully()
    {
      Cleanup();

      var invalidConfig = TestUtil.GetDefaultConfiguration();
      invalidConfig.NetworkLocation = "helloworld";
      var status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.InvalidNetworkAddressFormat, status);

      invalidConfig.NetworkLocation = @"\\ab:?c";
      status = NetworkUtil.Connect(invalidConfig);
      Assert.AreEqual(ConnectResult.InvalidNetworkAddressFormat, status);

    }



    [Test]
    public void Connecting_To_Unsecured_Share_Without_Credentials_Should_Create_Connection()
    {
      Cleanup();

      config = TestUtil.GetConfiguration(TestUtil.RemoteUnsecuredShare);

      var status = NetworkUtil.Connect(config);
      Assert.AreEqual(ConnectResult.Success, status, "Unexpected status: {0}".Put(status));
    }

  }
}