using System;
using System.Linq;
using System.Xml.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using Hardcodet.NetDrives.Platform.Util;
using NUnit.Framework;
using Platform_Tests.Properties;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ShareStore_When_Reading_Configuration
  {
    private XDocument doc;
    private ShareStore store;

    [SetUp]
    public void Init()
    {
      doc = XDocument.Parse(Resources.ShareConfiguration);
      store = new ShareStore(doc.Root);
    }


    [TearDown]
    public void Cleanup()
    {
    }


    [Test]
    public void Version_Should_Be_Available()
    {
      Assert.IsNotNull(store.Version);
      Assert.AreEqual("1.0.0", store.Version);
    }


    [Test]
    public void Static_Version_Analysis_Should_Return_Correct_Version()
    {
      //get document and compare version of static method with property
      Assert.AreEqual(ShareStore.GetConfigurationVersion(doc), store.Version);
    }


    [Test]
    public void Configurations_Should_Have_Been_Parsed()
    {
      var configs = store.ShareConfigurations.ToList();
      Assert.AreEqual(10, configs.Count);
    }


    [Test]
    public void Requesting_Share_By_Name_Should_Return_Matching_Element()
    {
      var localName = "LocalSecured";
      var remoteName = "RemoteSecured";

      var local = store.GetShare(localName);
      var remote = store.GetShare(remoteName);

      Assert.IsNotNull(local);
      Assert.IsNotNull(remote);

      Assert.AreEqual(localName, local.ItemId);
      Assert.AreEqual(remoteName, remote.ItemId);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Requesting_Share_By_Null_Should_Throw_Exception()
    {
      store.GetShare(null);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void Requesting_Unknown_Share_Should_Throw_Exception()
    {
      store.GetShare("xyz");
    }
    
  }
}