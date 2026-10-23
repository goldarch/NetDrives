using System;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// Common interface for WMI observer classes.
  /// </summary>
  public interface IWmiObserver : IDisposable
  {
    /// <summary>
    /// An optional identifier for this observer. Defaults to
    /// <see cref="String.Empty"/>.
    /// </summary>
    string ObserverId { get; }

    /// <summary>
    /// Bubbles an event of an underlying <see cref="WmiClassObserver.Watcher"/>
    /// if required.
    /// </summary>
    event EventHandler<WmiEventArgs> WmiEventFired;
  }
}