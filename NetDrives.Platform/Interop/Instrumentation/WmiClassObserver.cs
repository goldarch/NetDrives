using System;
using System.Management;
using Hardcodet.Commons;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// A simple class that encapsulates a single <see cref="ManagementEventWatcher"/>
  /// that reacts to a given WMI event which can be bubbled through the
  /// class' <see cref="WmiEventFired"/> event.<br/>
  /// Instances of this class should be properly disposed by invoking
  /// the <see cref="Dispose"/> method.
  /// </summary>
  public class WmiClassObserver : SimpleDisposableObject, IWmiObserver
  {
    /// <summary>
    /// The default query interval that is applied if not explicitly
    /// specified (5 seconds).
    /// </summary>
    public const int DefaultQueryInterval = 5;

    /// <summary>
    /// An optional identifier for this observer. Defaults to
    /// <see cref="String.Empty"/>.
    /// </summary>
    public string ObserverId { get; private set; }

    /// <summary>
    /// The watcher for the underlying WQL query.
    /// </summary>
    public ManagementEventWatcher Watcher { get; private set; }


    /// <summary>
    /// Bubbles an event of the underlying <see cref="Watcher"/>
    /// if required.
    /// </summary>
    public event EventHandler<WmiEventArgs> WmiEventFired;



    /// <summary>
    /// Creates a new observer for a given WQL query and immediately subscribes
    /// a listener for event changes.
    /// </summary>
    /// <param name="query">An event query that targets the observed
    /// event class and interval.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="query"/>
    /// is a null reference.</exception>
    public WmiClassObserver(WqlEventQuery query) : this(String.Empty, query)
    {
    }


    /// <summary>
    /// Creates a new observer for a given WQL query and immediately subscribes
    /// a listener for event changes.
    /// </summary>
    /// <param name="observerId">An optional ID that can be used to identify the
    /// observer in case of common event listeners.</param>
    /// <param name="query">An event query that targets the observed
    /// event class and interval.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="observerId"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="query"/>
    /// is a null reference.</exception>
    public WmiClassObserver(string observerId, WqlEventQuery query)
    {
      Init(observerId, query);
    }


    /// <summary>
    /// Parameterless constructor for deriving classes that need to
    /// perform a delayed initialization. You must invoke the <see cref="Init"/>
    /// method yourself.
    /// </summary>
    protected WmiClassObserver()
    { }



    /// <summary>
    /// Peforms initialization of the class.
    /// </summary>
    /// <param name="observerId">An optional ID that can be used to identify the
    /// observer in case of common event listeners.</param>
    /// <param name="query">An event query that targets the observed
    /// event class and interval.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="observerId"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="query"/>
    /// is a null reference.</exception>
    protected void Init(string observerId, WqlEventQuery query)
    {
      Ensure.ArgumentNotNull(() => observerId);
      Ensure.ArgumentNotNull(() => query);

      ObserverId = observerId;

      //create watcher and register event listener
      Watcher = new ManagementEventWatcher(query);
      Watcher.EventArrived += OnWatcherEvent;
      Watcher.Start();
    }



    /// <summary>
    /// Receives WMI events which are bubbled through the
    /// <see cref="WmiEventFired"/> event.
    /// </summary>
    protected virtual void OnWatcherEvent(object sender, EventArrivedEventArgs e)
    {
      //bubble event
      if (WmiEventFired != null && !IsDisposed)
      {
        WmiEventArgs args = new WmiEventArgs(this, e);
        WmiEventFired(this, args);
      }
    }



    /// <summary>
    /// Disposes the underlying WMI <see cref="Watcher"/>.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing && !IsDisposed)
      {
        Watcher.EventArrived -= OnWatcherEvent;
        Watcher.Stop();
        Watcher.Dispose();
      }

      base.Dispose(disposing);
    }


    #region CreateQuery

    /// <summary>
    /// Helper method that creates and instance event query for a
    /// given class, using <see cref="WmiEventClass.AnyInstanceOperation"/>
    /// and a <see cref="DefaultQueryInterval"/>.
    /// </summary>
    /// <param name="wmiClass">The targeted wmi class to be observed
    /// (e.g. <c>"Win32_NetworkConnection"</c>) or <c>"Win32_MappedLogicalDisk"</c>.
    /// </param>
    /// <returns>A query instance that can be used to observed the targeted
    /// <paramref name="wmiClass"/>.</returns>
    public static WqlEventQuery CreateQuery(string wmiClass)
    {
      return CreateQuery(wmiClass, WmiEventClass.AnyInstanceOperation);
    }

    /// <summary>
    /// Helper method that creates and instance event query for a
    /// given class, using the specified event class and a
    /// <see cref="DefaultQueryInterval"/>.
    /// </summary>
    /// <param name="wmiClass">The targeted wmi class to be observed
    /// (e.g. <c>"Win32_NetworkConnection"</c>) or <c>"Win32_MappedLogicalDisk"</c>.
    /// </param>
    /// <param name="eventClass">Defines the scope of events that are
    /// being observed.</param>
    /// <returns>A query instance that can be used to observed the targeted
    /// <paramref name="wmiClass"/>.</returns>
    public static WqlEventQuery CreateQuery(string wmiClass, WmiEventClass eventClass)
    {
      return CreateQuery(wmiClass, eventClass, DefaultQueryInterval);
    }

    /// <summary>
    /// Helper method that creates and instance event query for a
    /// given class, using <see cref="WmiEventClass.AnyInstanceOperation"/>
    /// and a specified interval (in seconds).
    /// </summary>
    /// <param name="wmiClass">The targeted wmi class to be observed
    /// (e.g. <c>"Win32_NetworkConnection"</c>) or <c>"Win32_MappedLogicalDisk"</c>.
    /// </param>
    /// <param name="interval"></param>
    /// <returns>A query instance that can be used to observed the targeted
    /// <paramref name="wmiClass"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If interval is below 1.</exception>
    public static WqlEventQuery CreateQuery(string wmiClass, int interval)
    {
      return CreateQuery(wmiClass, WmiEventClass.AnyInstanceOperation, interval);
    }


    /// <summary>
    /// Helper method that creates  a query that listens to WMI class instance
    /// events.
    /// </summary>
    /// <param name="wmiClass">The targeted wmi class to be observed
    /// (e.g. <c>"Win32_NetworkConnection"</c>) or <c>"Win32_MappedLogicalDisk"</c>.
    /// </param>
    /// <param name="eventClass">Defines the scope of events that are
    /// being observed.</param>
    /// <param name="interval">The polling interval in whole SECONDS.</param>
    /// <returns>A query instance that can be used to observed the targeted
    /// <paramref name="wmiClass"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If interval is below 1.</exception>
    public static WqlEventQuery CreateQuery(string wmiClass, WmiEventClass eventClass, int interval)
    {
      string eventType;
      if (interval < 1)
      {
        var msg = "Invalid query interval {0} - must be 1 or higher".Put(interval);
        throw new ArgumentOutOfRangeException("interval", msg);
      }

      switch (eventClass)
      {
        case WmiEventClass.InstanceCreation:
          eventType = WmiClasses.__InstanceCreationEvent;
          break;
        case WmiEventClass.InstanceDeletion:
          eventType = WmiClasses.__InstanceDeletionEvent;
          break;
        case WmiEventClass.InstanceModification:
          eventType = WmiClasses.__InstanceModificationEvent;
          break;
        case WmiEventClass.AnyInstanceOperation:
          eventType = WmiClasses.__InstanceOperationEvent;
          break;
        default:
          string msg = "Unsupported or unknown event source flag: {0}";
          msg = String.Format(msg, eventClass);
          throw new ArgumentOutOfRangeException(msg);
      }


      string mask = "SELECT * FROM {0} WITHIN {1} WHERE TargetInstance ISA \"{2}\"";
      mask = string.Format(mask, eventType, interval, wmiClass);
      return new WqlEventQuery(mask);
    }

    #endregion

  }
}
