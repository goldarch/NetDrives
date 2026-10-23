using System;
using System.Linq;
using System.Threading;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_Share_Credentials
  {

    [SetUp]
    public void Init()
    {
    }


    [TearDown]
    public void Cleanup()
    {
    }


    [Test]
    public void Shares_That_Were_Never_Connected_Should_Provide_Default_Credentials()
    {
      using(var manager = TestUtil.GetDefaultShareManager())
      {
        manager.Shares.Do(s =>
                            {
                              Assert.AreEqual(s.Configuration.CreateDefaultCredentials().UserName, s.Credentials.UserName);
                              Assert.IsTrue(s.Configuration.CreateDefaultCredentials().Password.Matches(s.Credentials.Password));
                            });
      }
    }



    [Test]
    public void AutoConnected_Shares_Should_Provide_Default_Credentials()
    {
      var config = TestUtil.GetDefaultConfiguration();
      config.AutoConnect = true;


      using (new DriveShareGuard(config, false))
      {
        //get store that contains the configuration
        var store = TestUtil.GetDefaultStore();
        store.Clear();
        store.AddConfiguration(config);

        using (ShareManager manager = new ShareManager(store))
        {
          Thread.Sleep(6000);

          Share share = manager.Shares[0];
          Assert.AreSame(config, share.Configuration);

          Assert.IsTrue(share.IsConnected, "Status is: " + share.Status);
          Assert.AreEqual(config.UserName, share.Credentials.UserName);
          Assert.IsTrue(config.Password.Matches(share.Credentials.Password));
        }
      }
    }



    [Test]
    public void Configured_Credentials_Should_Be_Assigned_If_Matching_Connection_For_Configuration_Is_Detected()
    {
      //if a connection is detected, we do not know the password
      using (ShareManager manager = TestUtil.GetDefaultShareManager())
      {
        Thread.Sleep(6000);
        string configId = TestUtil.GetDefaultConfiguration().ItemId;
        var share = manager.Shares.Single(sh => sh.Configuration.ItemId == configId);
        Assert.AreEqual(ConnectionStatus.Disconnected, share.Status);

        //set some bogus credentials
        share.Credentials = new ShareCredentials(share.Configuration.UserName, "XYZ".ToSecureString());

        //connect, but not via share manager
        using(new DriveShareGuard(share.Configuration))
        {    
          Thread.CurrentThread.Join(6000);
          Assert.IsTrue(share.IsConnected, "Share status is: " + share.Status);
          Assert.AreEqual(share.Configuration.UserName, share.Credentials.UserName);
        }
      }
    }


    [Test]
    public void Manually_Connecting_With_Default_Configuration_Should_Create_Corresponding_Credentials()
    {
      using(var manager = TestUtil.GetDefaultShareManager())
      {
        var config = manager.Store.GetShare(TestUtil.RemoteSecuredShare);
        var share = manager.Shares.Single(s => s.Configuration.ItemId == config.ItemId);

        manager.Connect(share);
        Assert.IsNotNull(share.Credentials);
        Assert.AreEqual(config.UserName, share.Credentials.UserName);
        Assert.IsTrue(config.Password.Matches(share.Credentials.Password));

        Thread.Sleep(4000);
        manager.Disconnect(share, false);
      }
    }



    [Test]
    public void Manually_Connecting_With_Custom_Login_Should_Store_Matching_Credentials()
    {
      using (var manager = TestUtil.GetDefaultShareManager())
      {
        var config = manager.Store.GetShare(TestUtil.RemoteSecuredShare);
        var share = manager.Shares.Single(s => s.Configuration.ItemId == config.ItemId);

        var customCredentials = new ShareCredentials("XX", "YY".ToSecureString());
        manager.Connect(share, customCredentials);

        Assert.IsNotNull(share.Credentials);
        Assert.AreEqual(share.Credentials, customCredentials);

        Thread.Sleep(4000);
        manager.Disconnect(share, false);
      }
    }


  }
}