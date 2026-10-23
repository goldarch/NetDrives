using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using Hardcodet.NetDrives.Platform;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  internal class Given_Network_Util_Is_Handling_Connection_Without_Mapping_Drive_Letter
  {
    [Test]
    public void Connection_Should_Be_Successfully_Registered()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);

      using (new UncShareGuard(configuration, false))
      {
        Assert.IsNull(configuration.DriveLetter);
        var addStatus = NetworkUtil.Connect(configuration);
        Assert.AreEqual(ConnectResult.Success, addStatus);
      }
    }


    //As a matter of fact, this result is indeterminate - remote test location
    //causes access denied / local one a login failure...
    [Test]
    public void Connecting_With_Unknown_User_Id_Should_Cause_Access_Denied()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      configuration.UserName = "sdfsldf";

      using (new UncShareGuard(configuration, false))
      {
        var addStatus = NetworkUtil.Connect(configuration);
        Assert.AreEqual(ConnectResult.UnknownUserOrAccessDenied, addStatus);
      }
    }


    [Test]
    public void Connecting_With_Invalid_Password_Should_Cause_Login_Failure()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      configuration.Password = "sljfd".ToSecureString();

      using (new UncShareGuard(configuration, false))
      {
        var addStatus = NetworkUtil.Connect(configuration);
        //not using a mapped drive does not result in LoginFailure but InvalidCredentials on the test sys
        Assert.AreEqual(ConnectResult.InvalidCredentials, addStatus);
      }
    }


    [Test]
    public void Connecting_Secured_Location_Without_Password_Should_Cause_Login_Failure()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      configuration.Password = null;

      using (new UncShareGuard(configuration, false))
      {
        var addStatus = NetworkUtil.Connect(configuration);
        //not using a mapped drive does not result in LoginFailure but InvalidCredentials on the test sys
        Assert.AreEqual(ConnectResult.InvalidCredentials, addStatus);
      }
    }




    [Test]
    public void Connecting_Secured_Location_Without_Credentials_Should_Cause_Access_Denied()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);
      configuration.UserName = null;
      configuration.Password = null;

      using (new UncShareGuard(configuration, false))
      {
        var addStatus = NetworkUtil.Connect(configuration);
        Assert.AreEqual(ConnectResult.UnknownUserOrAccessDenied, addStatus);
      }
    }


    /*************************
     * Connect to same share *
     *************************/


    [Test]
    public void Registering_Multiple_Connections_To_Same_Share_Using_Same_Credentials_Should_Work()
    {
      //establish two connections using the same credentials

      var config1 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        //register same connection again
        addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);
      }
    }


    /// <summary>
    /// A secure / unsecure connection are two connections with different credentials...
    /// </summary>
    [Test]
    public void Registering_Secured_And_Unsecured_Connection_To_Same_Share_Should_Fail()
    {
      var config1 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var config2 = TestUtil.GetConfiguration(TestUtil.UncTempUnsecured);

      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        //try to connect with other credentials
        addStatus = NetworkUtil.Connect(config2);
        Assert.AreEqual(ConnectResult.SessionCredentialConflict, addStatus);
      }
    }


    [Test]
    public void Registering_Multiple_Connections_To_Same_Share_Using_Different_Credentials_Should_Fail()
    {
      var config1 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var config2 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser2);

      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        //try to connect with other credentials
        addStatus = NetworkUtil.Connect(config2);
        Assert.AreEqual(ConnectResult.SessionCredentialConflict, addStatus);
      }
    }


    /*******************************
     * Connect to different shares *
     *******************************/

    [Test]
    public void Registering_Multiple_Connections_To_Different_Shares_Using_Same_Credientials_Should_Work()
    {
      //1 host / 2 shares / 1 logon

      var config1 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var config2 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser1);

      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        using (new UncShareGuard(config2, false))
        {
          //try to connect with other credentials
          addStatus = NetworkUtil.Connect(config2);
          Assert.AreEqual(ConnectResult.Success, addStatus);
        }
      }
    }


    [Test]
    public void Registering_Multiple_Connections_To_Different_Shares_Using_Different_Credentials_Should_Fail()
    {
      //1 host / 2 shares / 2 different logons

      var config1 = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);
      var config2 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser2);

      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        using (new UncShareGuard(config2, false))
        {
          //try to connect with other credentials
          addStatus = NetworkUtil.Connect(config2);
          Assert.AreEqual(ConnectResult.SessionCredentialConflict, addStatus);
        }
      }
    }


    [Test]
    public void Registering_Multiple_Connections_To_Different_Shares_And_Hosts_Should_Support_Different_Credentials()
    {
      //2 hosts / 2 shares / 2 different logons
      var config1 = TestUtil.GetConfiguration(TestUtil.LocalSecuredShare);
      var config2 = TestUtil.GetConfiguration(TestUtil.UncPublicSecuredUser2);
      config1.DriveLetter = null;


      using (new UncShareGuard(config1, false))
      {
        var addStatus = NetworkUtil.Connect(config1);
        Assert.AreEqual(ConnectResult.Success, addStatus);

        using (new UncShareGuard(config2, false))
        {
          //try to connect with other credentials
          addStatus = NetworkUtil.Connect(config2);
          Assert.AreEqual(ConnectResult.Success, addStatus);
        }
      }
    }


    [Test]
    public void Deregistering_Already_Cancelled_Connection_Should_Indicate_There_Was_No_Connection()
    {
      var configuration = TestUtil.GetConfiguration(TestUtil.UncTempSecuredUser1);

      using (new UncShareGuard(configuration, true))
      {
      }

      var status = NetworkUtil.CancelUncConnection(configuration.NetworkLocation, false);
      Assert.AreEqual(DisconnectResult.NotConnected, status);
    }
  }
}