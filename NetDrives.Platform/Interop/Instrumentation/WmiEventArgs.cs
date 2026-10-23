using System;
using System.Management;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// Provides event information for a given WMI class.
  /// </summary>
  public class WmiEventArgs : EventArgs
  {
    /// <summary>
    /// The observer class that received the event.
    /// </summary>
    public WmiClassObserver Observer { get; private set; }

    /// <summary>
    /// Indicates the event type.
    /// </summary>
    public WmiEventClass EventClass { get; private set; }

    /// <summary>
    /// The object that was observed.
    /// </summary>
    public ManagementBaseObject EventSource { get; private set; }

    /// <summary>
    /// Event context information as submitted by the received
    /// WMI event (<see cref="ManagementEventArgs.Context"/>).
    /// </summary>
    public object Context { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.EventArgs" /> class.
    /// </summary>
    public WmiEventArgs(WmiClassObserver observer, EventArrivedEventArgs args)
    {
      Observer = observer;
      Context = args.Context;
      EventSource = args.NewEvent;


      //try to determine the event class
      var eventClass = (string) EventSource["__CLASS"];
      switch (eventClass)
      {
        case WmiClasses.__InstanceCreationEvent:
          EventClass = WmiEventClass.InstanceCreation;
          break;
        case WmiClasses.__InstanceDeletionEvent:
          EventClass = WmiEventClass.InstanceDeletion;
          break;
        case WmiClasses.__InstanceModificationEvent:
          EventClass = WmiEventClass.InstanceModification;
          break;
        case WmiClasses.__InstanceOperationEvent:
          EventClass = WmiEventClass.AnyInstanceOperation;
          break;
        default:
          EventClass = WmiEventClass.Undefined;
          break;
      }
    }


    /// <summary>
    /// Gets the target instance of the <see cref="EventSource"/> by retrieving
    /// the <c>TargetInstance</c> property.
    /// </summary>
    /// <exception cref="ManagementException">If the underlying query was not
    /// an instance query and thus does not provide a matching property.</exception>
    public ManagementBaseObject GetTargetInstance()
    {
      return (ManagementBaseObject) EventSource.GetPropertyValue("TargetInstance");
    }
  }
}