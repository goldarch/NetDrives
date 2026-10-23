using System;
using System.Linq;
using System.Xml.Linq;
using Hardcodet.NetDrives.Platform.Core.Shares;
using NUnit.Framework;
using Platform_Tests.Properties;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_ShareStore_When_Registering_Shares
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
    public void New_Shares_Should_Receive_Highest_Order_Index()
    {
      for(int i=0; i<20; i++)
      {
        var config = TestUtil.GetConfiguration();
        Assert.AreEqual(0, config.OrderIndex);

        store.AddConfiguration(config);
        Assert.AreEqual(store.ShareConfigurations.Count() - 1, config.OrderIndex);
      }
    }
  }
}