using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using Autofac;
using Autofac.Builder;
using CommonServiceLocator.AutofacAdapter;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;
using Hardcodet.Util.Logging;
using Hardcodet.XItems;
using Microsoft.Practices.ServiceLocation;
using NUnit.Framework;
using Platform_Tests.Properties;

namespace Platform_Tests
{
  public static class TestUtil
  {
    public const string LocalSecuredShare = "LocalSecured";
    public const string LocalUnsecuredShare = "LocalUnsecured";
    public const string RemoteSecuredShare = "RemoteSecured";
    public const string RemoteUnsecuredShare = "RemoteUnsecured";

    public const string UncTempSecuredUser1 = "UncTempSecuredUser1";
    public const string UncTempSecuredUser2 = "UncTempSecuredUser2";
    public const string UncTempUnsecured = "UncTempUnsecured";

    public const string UncPublicSecuredUser1 = "UncPublicSecuredUser1";
    public const string UncPublicSecuredUser2 = "UncPublicSecuredUser2";
    public const string UncPublicUnsecured = "UncPublicUnsecured";


    static TestUtil()
    {
      var builder = new ContainerBuilder();

      //register single logger instance
      builder.Register(new ConsoleLogger()).As<ILogger>();
      builder.Register(c => ShareFactory.CreateConfiguration()).As<IShareConfiguration>();

      IContext context = builder.Build();
      var locator = new AutofacServiceLocator(context);

      //register as the default provider
      ServiceLocator.SetLocatorProvider(() => locator);
    }


    #region network connection (NIC adapter)

    public static void SetNicStatus(bool enabled)
    {
      string id = "Local Area Connection";
      string condition = "NetConnectionID = '{0}'".Put(id);
      NetworkAdapter adapter = NetworkAdapter.GetInstances(condition)
          .Cast<NetworkAdapter>()
          .FirstOrDefault();

      Assert.IsNotNull(adapter, "Invalid NIC ID - adjust");
      uint status;
      if (enabled)
        status = adapter.Enable();
      else
        status = adapter.Disable();

      Assert.AreEqual(0, status);
    }

    #endregion

    #region local test share

    public const string ShareName = "_ShareSharp_Test";

    [DllImport("Netapi32.dll")]
    private static extern uint NetShareAdd(
        [MarshalAs(UnmanagedType.LPWStr)] string strServer,
        Int32 dwLevel,
        ref SHARE_INFO_502 buf,
        out uint parm_err
        );

    [DllImport("netapi32.dll", SetLastError = true)]
    private static extern uint NetShareDel(
        [MarshalAs(UnmanagedType.LPWStr)] string strServer,
        [MarshalAs(UnmanagedType.LPWStr)] string strNetName,
        Int32 reserved //must be 0
        );


    /// <summary>
    /// Gets the qualified path of the test share folder, and creates
    /// it on the fly, if necessary.
    /// </summary>
    public static string TestShareFolder
    {
      get
      {
        string tmp = Path.GetTempPath();
        var folder = Path.Combine(tmp, ShareName);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

        return folder;
      }
    }


    public static string TestShareAddress
    {
      get { return @"\\127.0.0.1\" + ShareName + @""; }
    }

    /// <summary>
    /// Creates a local share to run tests against
    /// </summary>
    public static int CreateTestShare()
    {
      SHARE_INFO_502 info = new SHARE_INFO_502();
      info.shi502_netname = ShareName;
      info.shi502_type = SHARE_TYPE.STYPE_DISKTREE;
      info.shi502_remark = "Test Share";
      info.shi502_permissions = 0; // ignored for user-level security
      info.shi502_max_uses = -1;
      info.shi502_current_uses = 0; // ignored for set
      info.shi502_path = TestShareFolder;
      info.shi502_passwd = null; // ignored for user-level security
      info.shi502_reserved = 0;
      info.shi502_security_descriptor = IntPtr.Zero;

      uint error;
      return (int) NetShareAdd("127.0.0.1", 502, ref info, out error);
    }


    public static int RemoveTestShare()
    {
      return (int) NetShareDel("127.0.0.1", ShareName, 0);
    }

    #endregion

    #region share configurations

    /// <summary>
    /// Createsa an empty share configuration.
    /// </summary>
    /// <returns></returns>
    public static ShareConfiguration GetConfiguration()
    {
      return ShareFactory.CreateConfiguration();
    }

    public static ShareStore GetDefaultStore()
    {
      XDocument doc = XDocument.Parse(Resources.ShareConfiguration);
      return new ShareStore(doc.Root);
    }


    public static ShareManager GetDefaultShareManager()
    {
      var manager = new ShareManager(TestUtil.GetDefaultStore());

      //give the manager a short while to start initialization
      Thread.CurrentThread.Join(1000);
      return manager;
    }


    /// <summary>
    /// Gets the configured share that matches the
    /// <see cref="LocalSecuredShare"/> constant.
    /// </summary>
    /// <returns></returns>
    public static ShareConfiguration GetConfiguration(string configurationName)
    {
      var store = GetDefaultStore();
      ShareConfiguration configuration = (ShareConfiguration) store.GetShare(configurationName);
      
      //make sure the element is orphaned and can be assigned to another XDocument
      configuration.Element.Remove();
      
      return configuration;
    }


    /// <summary>
    /// Gets the configured share that matches the
    /// <see cref="LocalSecuredShare"/> constant.
    /// </summary>
    /// <returns></returns>
    public static ShareConfiguration GetDefaultConfiguration()
    {
      return GetConfiguration(RemoteSecuredShare);
    }


    /// <summary>
    /// Creates an share manager with an empty
    /// <see cref="ShareStore"/>.
    /// </summary>
    /// <returns></returns>
    public static ShareManager GetEmptyShareManager()
    {
      XElement root = IdentifiableXItem.CreateKeyedElement(ShareStore.RootElementName, "store");
      ShareStore store = new ShareStore(root);
      var manager = new ShareManager(store);

      //give the manager a short while to start initialization
      Thread.CurrentThread.Join(1000);
      return manager;

    }

    #endregion

    #region Assertion helpers

    /// <summary>
    /// Checks whether a given action produces a change event for
    /// a given property.
    /// </summary>
    /// <typeparam name="T">A class that implements <see cref="INotifyPropertyChanged"/>.
    /// </typeparam>
    /// <param name="host">The object that contains the changed property.</param>
    /// <param name="changeAction">An action that triggers the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="propertyName">The name of the changed property, as advertised by the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    public static void AssertFiresPropertyChangedEvent<T>(this T host, string propertyName,
                                                          Action<T> changeAction)
        where T : INotifyPropertyChanged
    {
      bool fired = false;

      //register handler with assertion
      PropertyChangedEventHandler handler = (sender, e) =>
                                              {
                                                fired = true;
                                                Assert.AreEqual(propertyName, e.PropertyName);
                                              };

      //register listener, trigger action, then deregister
      host.PropertyChanged += handler;
      changeAction(host);
      host.PropertyChanged -= handler;

      Assert.IsTrue(fired);
    }

    /// <summary>
    /// Checks whether a given action produces a specified number of property
    /// change events for a given property.<br/>
    /// This event listener just ignores event for properties other than
    /// <paramref name="propertyName"/>.
    /// </summary>
    /// <typeparam name="T">A class that implements <see cref="INotifyPropertyChanged"/>.
    /// </typeparam>
    /// <param name="host">The object that contains the changed property.</param>
    /// <param name="numberOfEvents">The expected number of change events for
    /// the specified property</param>
    /// <param name="changeAction">An action that triggers the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    /// <param name="propertyName">The name of the changed property, as advertised by the
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</param>
    public static void AssertPropertyChangedEventCount<T>(this T host, string propertyName,
                                                          int numberOfEvents,
                                                          Action<T> changeAction)
        where T : INotifyPropertyChanged
    {
      int eventCount = 0;

      //register handler with assertion
      PropertyChangedEventHandler handler = (sender, e) =>
                                              {
                                                //increment if the event refers to the tested property
                                                if (e.PropertyName == propertyName) eventCount++;
                                              };

      //register handler, trigger action, then deregister
      host.PropertyChanged += handler;
      changeAction(host);
      host.PropertyChanged -= handler;

      //compare counted events with expected number
      Assert.AreEqual(numberOfEvents, eventCount);
    }

    #endregion

  }


  internal enum SHARE_TYPE : uint
  {
    STYPE_DISKTREE = 0,
    STYPE_PRINTQ = 1,
    STYPE_DEVICE = 2,
    STYPE_IPC = 3,
    STYPE_SPECIAL = 0x80000000,
  }


  [StructLayout(LayoutKind.Sequential)]
  internal struct SHARE_INFO_502
  {
    [MarshalAs(UnmanagedType.LPWStr)] public string shi502_netname;
    public SHARE_TYPE shi502_type;
    [MarshalAs(UnmanagedType.LPWStr)] public string shi502_remark;
    public Int32 shi502_permissions;
    public Int32 shi502_max_uses;
    public Int32 shi502_current_uses;
    [MarshalAs(UnmanagedType.LPWStr)] public string shi502_path;
    [MarshalAs(UnmanagedType.LPWStr)] public string shi502_passwd;
    public Int32 shi502_reserved;
    public IntPtr shi502_security_descriptor;
  }
}