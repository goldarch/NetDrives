using System;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop.Instrumentation;

namespace Hardcodet.NetDrives.Platform
{
  public static class Extensions
  {
    /// <summary>
    /// Converts a single drive letter character into a drive
    /// letter string that is suffixed with a colon (:) and also
    /// makes sure the drive letter is in upper case.
    /// </summary>
    /// <param name="driveLetter">The drive letter.</param>
    /// <returns>Drive letter string if the submitted <paramref name="driveLetter"/>
    /// has a value, otherwise an empty string.</returns>
    public static string ToDriveLetterString(this char? driveLetter)
    {
      return driveLetter.HasValue
                 ? driveLetter.Value.ToDriveLetterString()
                 : String.Empty;
    }


    /// <summary>
    /// Converts a single drive letter character into a drive
    /// letter string that is suffixed with a colon (:) and also
    /// makes sure the drive letter is in upper case.
    /// </summary>
    /// <param name="driveLetter">The drive letter.</param>
    /// <returns>Drive letter string.</returns>
    public static string ToDriveLetterString(this char driveLetter)
    {
      return char.ToUpperInvariant(driveLetter) + ":";
    }


    /// <summary>
    /// Returns a drive letter string that matches the returned value of the
    /// <see cref="ToDriveLetterString(char?)"/> extension method, if the submitted
    /// <paramref name="connection"/> refers to a local drive.
    /// </summary>
    /// <param name="connection">A network connection to be processed.</param>
    /// <returns>A drive letter followed by a colon (:), if the connection refers
    /// to a mapped drive. Otherwise an empty string.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="connection"/>
    /// is a null reference.</exception>
    public static string GetDriveLetterString(this NetworkConnection connection)
    {
      Ensure.ArgumentNotNull(() => connection);

      if (String.IsNullOrEmpty(connection.LocalName)) return String.Empty;
      return connection.LocalName[0].ToDriveLetterString();
    }

  }
}