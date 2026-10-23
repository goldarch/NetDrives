using System;


namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// A listener that subscribes to connection-related WMI
  /// events.
  /// </summary>
  public class NetworkConnectionObserver : WmiClassObserver
  {
    /// <summary>
    /// The observed WMI class.
    /// </summary>
    public const string WmiConnectionClass = WmiClasses.Win32_NetworkConnection;


    /// <summary>
    /// Creates a new listener for WMI connection changes that subscribes
    /// any connection-related events (<see cref="WmiEventClass.AnyInstanceOperation"/>)
    /// with a default polling interval of 5 seconds.
    /// </summary>
    public NetworkConnectionObserver() : this(DefaultQueryInterval)
    {
    }


    /// <summary>
    /// Creates a new listener for WMI connection changes that subscribes
    /// any connection-related events (<see cref="WmiEventClass.AnyInstanceOperation"/>).
    /// </summary>
    /// <param name="queryInterval">The polling interval in whole SECONDS.</param>
    /// <exception cref="ArgumentOutOfRangeException">If interval is below 1.</exception>
    public NetworkConnectionObserver(int queryInterval)
      : this(WmiEventClass.AnyInstanceOperation, queryInterval)
    {
    }


    /// <summary>
    /// Creates a new observer for a given WQL query and immediately subscribes
    /// a listener for event changes.
    /// </summary>
    /// <param name="eventClass">Defines the scope of events that are
    /// being observed.</param>
    /// <param name="queryInterval">The polling interval in whole SECONDS.</param>
    /// <exception cref="ArgumentOutOfRangeException">If interval is below 1.</exception>
    public NetworkConnectionObserver(WmiEventClass eventClass, int queryInterval)
      : this(String.Empty, eventClass, queryInterval)
    {
    }


    /// <summary>
    /// Creates a new observer for a given WQL query and immediately subscribes
    /// a listener for event changes.
    /// </summary>
    /// <param name="observerId">An optional ID that can be used to identify the
    /// observer in case of common event listeners.</param>
    /// <param name="eventClass">Defines the scope of events that are
    /// being observed.</param>
    /// <param name="queryInterval">The polling interval in whole SECONDS.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="observerId"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">If interval is below 1.</exception>
    public NetworkConnectionObserver(string observerId, WmiEventClass eventClass, int queryInterval)
      : base(observerId, CreateQuery(WmiConnectionClass, eventClass, queryInterval))
    {
    }


  }
}
