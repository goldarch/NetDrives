using System;
using System.Collections.Generic;
using Hardcodet.Commons;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  /// <summary>
  /// A composite class that manages an arbitrary number of
  /// <see cref="WmiClassObserver"/> classes in order to monitor
  /// several WMI related events at once.
  /// </summary>
  public class CompositeWmiObserver : SimpleDisposableObject, IWmiObserver
  {
    /// <summary>
    /// The managed WMI observers.
    /// </summary>
    private readonly List<WmiClassObserver> observers = new List<WmiClassObserver>();


    /// <summary>
    /// An optional identifier for this observer. Defaults to
    /// <see cref="String.Empty"/>.
    /// </summary>
    public string ObserverId { get; set; }
    

    /// <summary>
    /// Bubbles an event of a managed <see cref="WmiClassObserver"/>
    /// if required.
    /// </summary>
    public event EventHandler<WmiEventArgs> WmiEventFired;


    /// <summary>
    /// Returns the managed observers as an independent sequence.
    /// </summary>
    public IEnumerable<WmiClassObserver> Observers
    {
      get { return observers.ToArray(); }
    }



    #region construction

    /// <summary>
    /// Creates an empty instance of the <see cref="CompositeWmiObserver"/>
    /// class that does not monitor any WMI events yet.
    /// </summary>
    public CompositeWmiObserver()
    {
      ObserverId = String.Empty;
    }


    /// <summary>
    /// Creates an instance of the <see cref="CompositeWmiObserver"/>
    /// that immediately registers with a set of WMI observers.
    /// </summary>
    /// <param name="observers">Observers for WMI events to be monitored.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="observers"/>
    /// is a null reference.</exception>
    public CompositeWmiObserver(params WmiClassObserver[] observers)
        : this(String.Empty, observers)
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="CompositeWmiObserver"/>
    /// that immediately registers with a set of WMI observers.
    /// </summary>
    /// <param name="observerId"></param>
    /// <param name="observers">Observers for WMI events to be monitored.</param>
    /// <exception cref="ArgumentNullException">If <paramref name="observers"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentNullException">If <paramref name="observerId"/>
    /// is a null reference.</exception>
    public CompositeWmiObserver(string observerId, params WmiClassObserver[] observers)
    {
      Ensure.ArgumentNotNull(() => observers);
      Ensure.ArgumentNotNull(() => observerId);

      ObserverId = observerId;
      foreach (var observer in observers)
      {
        AddObserver(observer);
      }
    }

    #endregion




    /// <summary>
    /// Adds a new observer to the class and registers
    /// </summary>
    /// <param name="observer"></param>
    /// <exception cref="ArgumentNullException">If <paramref name="observer"/>
    /// is a null reference.</exception>
    public void AddObserver(WmiClassObserver observer)
    {
      Ensure.ArgumentNotNull(() => observer);

      observer.WmiEventFired += OnObserverWmiEventFired;
      observers.Add(observer);
    }



    /// <summary>
    /// Removes a given observer from the internal list of managed
    /// <see cref="WmiClassObserver"/> instances.
    /// </summary>
    /// <param name="observer">The observer to be removed.</param>
    /// <returns>True if the observer was found in the internal list
    /// and successfully removed. Otherwise false.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="observer"/>
    /// is a null reference.</exception>
    public bool RemoveObserver(WmiClassObserver observer)
    {
      Ensure.ArgumentNotNull(() => observer);

      bool status = observers.Remove(observer);
      if (status)
      {
        observer.WmiEventFired -= OnObserverWmiEventFired;
      }

      return status;
    }



    /// <summary>
    /// Bubbles a given WMI event.
    /// </summary>
    private void OnObserverWmiEventFired(object source, WmiEventArgs e)
    {
      //bubble the event
      if (WmiEventFired != null && !IsDisposed) WmiEventFired(this, e);
    }



    /// <summary>
    /// Cleans up the class and the underlying observers.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing && !IsDisposed)
      {
        foreach (var observer in observers)
        {
          observer.WmiEventFired -= OnObserverWmiEventFired;
          observer.Dispose();
        }
      }

      base.Dispose(disposing);
    }
  }
}
