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
  public class Given_Share_Manager_When_Initializing
  {

    [Test]
    public void Initializing_Manager_With_Autoconnected_Share_With_AutoConnection_Should_Immediately_Connect()
    {
      var manager = TestUtil.GetEmptyShareManager();
      ShareStore store = new ShareStore(manager.Store.Element);
      manager.Dispose();

      var configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      configuration.AutoConnect = true;
      store.AddConfiguration(configuration);

      using (new DriveShareGuard(configuration, false))
      {
        //create new manager with auto-connected store in place
        manager = new ShareManager(store);
        manager.ChangeRefreshTimerInterval(2000, 5000);

        Thread.Sleep(5000);

        //get the share that corresponds to the configuration
        var share = manager.Shares.Single(s => ReferenceEquals(configuration, s.Configuration));

        share.StatusLog.Do(le => le.ToString().ToConsole());

        share.Status.ToConsole();
        share.ConnectionResult.ToConsole();
        Assert.AreEqual(ConnectionStatus.Connected, share.Status);

        Assert.AreEqual(ConnectionStatus.Connected, manager.SummaryStatus);
      }
    }


    [Test]
    public void Initialization_Share_Manager_With_Invalid_AutoConnection_Should_Set_Summary_Status_To_Invalid()
    {
      var manager = TestUtil.GetEmptyShareManager();
      ShareStore store = new ShareStore(manager.Store.Element);
      manager.Dispose();

      //create invalid configuration with autoconnect
      var configuration = TestUtil.GetConfiguration(TestUtil.RemoteSecuredShare);
      configuration.AutoConnect = true;
      configuration.Password = "xxzz".ToSecureString();
      store.AddConfiguration(configuration);

      using (new DriveShareGuard(configuration, false))
      {
        //create new manager with auto-connected store in place
        manager = new ShareManager(store);
        manager.ChangeRefreshTimerInterval(2000, 5000);

        Thread.Sleep(5000);

        //get the share that corresponds to the configuration
        var share = manager.Shares.Single(s => ReferenceEquals(configuration, s.Configuration));

        share.StatusLog.Do(le => le.ToString().ToConsole());

        share.Status.ToConsole();
        share.ConnectionResult.ToConsole();
        Assert.AreEqual(ConnectionStatus.ConnectionError, share.Status);

        Assert.AreEqual(ConnectionStatus.ConnectionError, manager.SummaryStatus);
      }
    }
  }
}