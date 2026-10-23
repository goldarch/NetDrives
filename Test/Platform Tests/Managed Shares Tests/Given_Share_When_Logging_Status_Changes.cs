using System;
using System.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public class Given_Share_When_Logging_Status_Changes
  {
    private Share share;
    private DateTime now;

    [SetUp]
    public void Init()
    {
      SystemTime.Now = () => new DateTime(2000, 1, 1);
      now = SystemTime.Now();
      
      var config = TestUtil.GetDefaultConfiguration();
      share = new Share(config);
    }
    

    [TearDown]
    public void TearDown()
    {
      SystemTime.Reset();
    }



    [Test]
    public void Share_Should_Create_Default_Entry_During_Creation()
    {
      Assert.AreEqual(1, share.StatusLog.Count());
      Assert.AreEqual(SystemTime.Now(), share.LatestLogEntry.Timestamp);
    }


    [Test]
    public void When_Requesting_Latest_Log_Entry_The_Latest_Entry_Should_Be_Returned()
    {
      SystemTime.Now = () => now;
      for (int i=0; i<10; i++)
      {
        now = now.AddSeconds(1);
        var status = i%2 == 0 ? ConnectionStatus.Connected : ConnectionStatus.ConnectionError;
        share.ChangeStatus(status, null);
        Assert.AreEqual(status, share.LatestLogEntry.Status);
      }
    }


    [Test]
    public void Testing_For_Age_Of_Item_Should_Calculate_Proper_Time_Range()
    {
      //create an entry with a timestamp that is 10 seconds ago
      var timestamp = SystemTime.Now().AddSeconds(-10);
      var entry = new StatusLogEntry(timestamp, ConnectionStatus.Connected, null);

      Assert.IsTrue(entry.IsOlderThan(9));
      Assert.IsFalse(entry.IsOlderThan(10));
      Assert.IsFalse(entry.IsOlderThan(11));
    }
  }
}