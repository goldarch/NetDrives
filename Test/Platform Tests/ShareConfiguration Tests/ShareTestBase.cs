using System;
using NUnit.Framework;

namespace Platform_Tests
{
  [TestFixture]
  public abstract class ShareTestBase
  {

    [SetUp]
    public void BaseInit()
    {
//      TestUtil.CreateTestShare();
      Init();
    }

    protected abstract void Init();


    [TearDown]
    public void BaseCleanup()
    {
      Cleanup();
//      TestUtil.RemoveTestShare();
    }

    protected virtual void Cleanup()
    {
    }
  }
}