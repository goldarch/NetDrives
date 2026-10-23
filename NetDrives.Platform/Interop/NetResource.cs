using System.Runtime.InteropServices;

namespace Hardcodet.NetDrives.Platform.Interop
{
  [StructLayout(LayoutKind.Sequential)]
  public struct NetResource
  {
    /// <summary>
    /// The scope of the enumeration.
    /// </summary>
    public ResourceScope Scope;

    /// <summary>
    /// The type of network resource to connect to.
    /// If the lpLocalName member points to a nonempty string,
    /// this member can be equal to Disk or Print.
    /// If lpLocalName is NULL, or if it points to an empty string,
    /// ResourceType can be equal to Disk, Print,
    /// or Any.
    /// Although this member is required, its information may be ignored by
    ///  the network service provider.
    /// </summary>
    public ResourceType ResourceType;
    public ResourceDisplayType DisplayType;
    public ResourceUsage Usage;
    /// <summary>
    /// A pointer to a null-terminated string that specifies the name
    /// of a local device to redirect, such as "F:" or "LPT1".
    /// The string is treated in a case-insensitive manner.
    /// If the string is empty or if lpLocalName is NULL, the
    /// function makes a connection to the network resource without
    /// redirecting a local device.
    /// If the Scope member is equal to Connected or
    /// Remembered, this member is a pointer to a null-terminated 
    /// character string that specifies the name of a local device. 
    /// This member is NULL if the connection does not use a device.
    /// </summary>
    public string LocalName;
    /// <summary>
    /// A pointer to a null-terminated string that specifies the network resource
    /// to connect to. The string can be up to MAX_PATH characters in length,
    /// and must follow the network provider's naming conventions.
    /// If the entry is a current or persistent connection, lpRemoteName member
    /// points to the network name associated with the name pointed to by the
    /// lpLocalName member.
    /// </summary>
    public string RemoteName;
    /// <summary>
    /// A pointer to a NULL-terminated string that contains a comment supplied 
    /// by the network provider.
    /// </summary>
    public string Comment;
    /// <summary>
    /// A pointer to a null-terminated string that specifies the network provider to connect to.
    /// If lpProvider is NULL, or if it points to an empty string, the operating system attempts
    /// to determine the correct provider by parsing the string pointed to by the lpRemoteName member.
    /// If this member is not NULL, the operating system attempts to make a connection only
    /// to the named network provider.
    /// You should set this member only if you know which network provider you want to use.
    /// Otherwise, let the operating system determine which network provider the network name maps to.
    /// </summary>
    public string Provider;



    /// <summary>
    /// Creates a resource instance for a given network location.
    /// </summary>
    /// <param name="driveLetter">The currently used or assigned
    /// drive letter.</param>
    /// <param name="remoteAddress">The network location of the resource.</param>
    /// <returns>Network resource component.</returns>
    public static NetResource Create(char? driveLetter, string remoteAddress)
    {
      var res = new NetResource();
      
      res.Scope = ResourceScope.GlobalNet;
      res.ResourceType = ResourceType.Disk;
      res.DisplayType = ResourceDisplayType.Share;
      res.Usage = ResourceUsage.Connectable;

      //only map to a local drive if the drive letter was specified
      string locName = driveLetter.HasValue ? driveLetter.Value.ToDriveLetterString() : null;

      res.LocalName = locName;
      res.RemoteName = remoteAddress;
      res.Provider = null;
      
      return res;
    }
  }
}