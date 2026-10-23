using System;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// A single entry of a <see cref="StatusLog"/>.
  /// </summary>
  public struct StatusLogEntry
  {
    /// <summary>
    /// The timestamp of the action.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// The connection status of the share.
    /// </summary>
    public ConnectionStatus Status { get; set; }

    /// <summary>
    /// The connection status, if applicable. Null
    /// in case no connection attempt was made.
    /// </summary>
    public ConnectResult? ConnectResult { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Object" /> class.
    /// </summary>
    public StatusLogEntry(DateTime timestamp, ConnectionStatus status, ConnectResult? connectResult) : this()
    {
      Timestamp = timestamp;
      Status = status;
      ConnectResult = connectResult;
    }


    /// <summary>
    /// Checks the time spent since the entry was created.
    /// </summary>
    /// <param name="seconds">The maximum elapsed time in seconds
    /// by comparing the <see cref="Timestamp"/> of this log entry
    /// with the current time.</param>
    /// <returns>True if the entry is older than the elapsed time span
    /// defined by the <paramref name="seconds"/> parameter.</returns>
    public bool IsOlderThan(int seconds)
    {
      int range = seconds*1000;
      return SystemTime.Now().Subtract(Timestamp).TotalMilliseconds > range;
    }


    /// <summary>
    /// Returns a string representation of the item.
    /// </summary>
    public override string ToString()
    {
      return "{0}: {1} ({2})".Put(Timestamp, Status, ConnectResult);
    }


    /// <summary>
    /// Compares two log entries.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(StatusLogEntry left, StatusLogEntry right)
    {
      return left.Timestamp == right.Timestamp &&
             left.Status == right.Status &&
             left.ConnectResult == right.ConnectResult;
    }


    /// <summary>
    /// Negates a comparison of two log entries.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(StatusLogEntry left, StatusLogEntry right)
    {
      return !(left == right);
    }

    public bool Equals(StatusLogEntry obj)
    {
      return obj.Timestamp.Equals(Timestamp) && Equals(obj.Status, Status) && obj.ConnectResult.Equals(ConnectResult);
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <returns>
    /// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
    /// </returns>
    /// <param name="obj">
    /// Another object to compare to. 
    /// </param><filterpriority>2</filterpriority>
    public override bool Equals(object obj)
    {
      if (obj.GetType() != typeof (StatusLogEntry)) return false;
      return Equals((StatusLogEntry) obj);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is the hash code for this instance.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    public override int GetHashCode()
    {
      unchecked
      {
        int result = Timestamp.GetHashCode();
        result = (result*397) ^ Status.GetHashCode();
        result = (result*397) ^ (ConnectResult.HasValue ? ConnectResult.Value.GetHashCode() : 0);
        return result;
      }
    }
  }

}
