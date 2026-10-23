namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// Provides flags for intrinsic events that should cause an event to be
  /// fired for a given WMI query. This is only a subset of supported
  /// WMI classes.
  /// <seealso cref="http://msdn.microsoft.com/en-us/library/aa390355(VS.85).aspx"/>
  /// </summary>
  public enum WmiEventClass
  {
    /// <summary>
    /// The source has not been defined or is unknown.
    /// </summary>
    Undefined = 0,
    /// <summary>
    /// Only handle events if an instance of the targeted WMI class was 
    /// created.
    /// </summary>
    InstanceCreation = 1,
    /// <summary>
    /// Only handle events if an instance of the targeted WMI class was 
    /// deleted.
    /// </summary>
    InstanceDeletion = 2,
    /// <summary>
    /// Notifies a consumer when an existing instance is modified.
    /// </summary>
    InstanceModification = 3,
    /// <summary>
    /// Handle all kind of events for an instance of a targeted WMI class,
    /// such as creation, deletion, or modification of the instance.
    /// You can use this class in queries to get all types events associated
    /// with an instance.
    /// </summary>
    AnyInstanceOperation = 4
  }
}