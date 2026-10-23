using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Management;

namespace Hardcodet.NetDrives.Platform.Interop.Instrumentation
{
  public class NetworkConnection : Component
  {
    // Private property to hold the WMI namespace in which the class resides.

    // Private property to hold the name of WMI class which created this class.

    #region AccessMaskValues enum

    public enum AccessMaskValues
    {
      FILE_READ_DATA_file_or_FILE_LIST_DIRECTORY_directory_ = 0,

      FILE_WRITE_DATA_file_or_FILE_ADD_FILE_directory_ = 1,

      FILE_APPEND_DATA_file_or_FILE_ADD_SUBDIRECTORY_directory_ = 2,

      FILE_READ_EA = 3,

      FILE_WRITE_EA = 4,

      FILE_EXECUTE_file_or_FILE_TRAVERSE_directory_ = 5,

      FILE_DELETE_CHILD_directory_ = 6,

      FILE_READ_ATTRIBUTES = 7,

      FILE_WRITE_ATTRIBUTES = 8,

      DELETE0 = 16,

      READ_CONTROL = 17,

      WRITE_DAC = 18,

      WRITE_OWNER = 19,

      SYNCHRONIZE = 20,

      NULL_ENUM_VALUE = 40,
    }

    #endregion

    private static string CreatedClassName = "Win32_NetworkConnection";
    private static string CreatedWmiNamespace = "ROOT\\CIMV2";

    // Private member variable to hold the ManagementScope which is used by the various methods.
    private static ManagementScope statMgmtScope;

    // Private variable to hold the embedded property representing the instance.
    private readonly ManagementBaseObject embeddedObj;
    private bool AutoCommitProp;

    // The current WMI object used
    private ManagementBaseObject curObj;

    // Flag to indicate if the instance is an embedded object.
    private bool isEmbedded;
    private ManagementObject PrivateLateBoundObject;
    private ManagementSystemProperties PrivateSystemProperties;

    // Below are different overloads of constructors to initialize an instance of the class with a WMI object.
    public NetworkConnection()
    {
      InitializeObject(null, null, null);
    }

    public NetworkConnection(string keyName)
    {
      InitializeObject(null, new ManagementPath(ConstructPath(keyName)), null);
    }

    public NetworkConnection(ManagementScope mgmtScope, string keyName)
    {
      InitializeObject(((mgmtScope)), new ManagementPath(ConstructPath(keyName)), null);
    }

    public NetworkConnection(ManagementPath path, ObjectGetOptions getOptions)
    {
      InitializeObject(null, path, getOptions);
    }

    public NetworkConnection(ManagementScope mgmtScope, ManagementPath path)
    {
      InitializeObject(mgmtScope, path, null);
    }

    public NetworkConnection(ManagementPath path)
    {
      InitializeObject(null, path, null);
    }

    public NetworkConnection(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions)
    {
      InitializeObject(mgmtScope, path, getOptions);
    }

    public NetworkConnection(ManagementObject theObject)
    {
      Initialize();
      if (CheckIfProperClass(theObject))
      {
        PrivateLateBoundObject = theObject;
        PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
        curObj = PrivateLateBoundObject;
      }
      else
      {
        throw new ArgumentException("Class name does not match.");
      }
    }

    public NetworkConnection(ManagementBaseObject theObject)
    {
      Initialize();
      if (CheckIfProperClass(theObject))
      {
        embeddedObj = theObject;
        PrivateSystemProperties = new ManagementSystemProperties(theObject);
        curObj = embeddedObj;
        isEmbedded = true;
      }
      else
      {
        throw new ArgumentException("Class name does not match.");
      }
    }

    // Property returns the namespace of the WMI class.
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string OriginatingNamespace
    {
      get { return "ROOT\\CIMV2"; }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string ManagementClassName
    {
      get
      {
        string strRet = CreatedClassName;
        if ((curObj != null))
        {
          if ((curObj.ClassPath != null))
          {
            strRet = ((string) (curObj["__CLASS"]));
            if (((strRet == null)
                 || (strRet == string.Empty)))
            {
              strRet = CreatedClassName;
            }
          }
        }
        return strRet;
      }
    }

    // Property pointing to an embedded object to get System properties of the WMI object.
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ManagementSystemProperties SystemProperties
    {
      get { return PrivateSystemProperties; }
    }

    // Property returning the underlying lateBound object.
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ManagementBaseObject LateBoundObject
    {
      get { return curObj; }
    }

    // ManagementScope of the object.
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public ManagementScope Scope
    {
      get
      {
        if ((isEmbedded == false))
        {
          return PrivateLateBoundObject.Scope;
        }
        else
        {
          return null;
        }
      }
      set
      {
        if ((isEmbedded == false))
        {
          PrivateLateBoundObject.Scope = value;
        }
      }
    }

    // Property to show the commit behavior for the WMI object. If true, WMI object will be automatically saved after each property modification.(ie. Put() is called after modification of a property).
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AutoCommit
    {
      get { return AutoCommitProp; }
      set { AutoCommitProp = value; }
    }

    // The ManagementPath of the underlying WMI object.
    [Browsable(true)]
    public ManagementPath Path
    {
      get
      {
        if ((isEmbedded == false))
        {
          return PrivateLateBoundObject.Path;
        }
        else
        {
          return null;
        }
      }
      set
      {
        if ((isEmbedded == false))
        {
          if ((CheckIfProperClass(null, value, null) != true))
          {
            throw new ArgumentException("Class name does not match.");
          }
          PrivateLateBoundObject.Path = value;
        }
      }
    }

    // Public static scope property which is used by the various methods.
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public static ManagementScope StaticScope
    {
      get { return statMgmtScope; }
      set { statMgmtScope = value; }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsAccessMaskNull
    {
      get
      {
        if ((curObj["AccessMask"] == null))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The AccessMask property is a bit array representing the access rights as set remo" +
                 "tely on the network share held by the user or group on whose behalf the instance" +
                 " is returned. This property is only supported under Windows NT and Windows 2000." +
                 " ")]
    [TypeConverter(typeof (WMIValueTypeConverter))]
    public AccessMaskValues AccessMask
    {
      get
      {
        if ((curObj["AccessMask"] == null))
        {
          return ((AccessMaskValues) (Convert.ToInt32(40)));
        }
        return ((AccessMaskValues) (Convert.ToInt32(curObj["AccessMask"])));
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The Caption property is a short textual description (one-line string) of the obje" +
                 "ct.")]
    public string Caption
    {
      get { return ((string) (curObj["Caption"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The Comment property contains a comment supplied by the network provider.")]
    public string Comment
    {
      get { return ((string) (curObj["Comment"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The ConnectionState property indicates the current state of the network connectio" +
                 "n.")]
    public string ConnectionState
    {
      get { return ((string) (curObj["ConnectionState"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The ConnectionType property indicates the persistence type of the connection used" +
                 " for connecting to the network.\nExample: Permanent.")]
    public string ConnectionType
    {
      get { return ((string) (curObj["ConnectionType"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The Description property provides a textual description of the object. ")]
    public string Description
    {
      get { return ((string) (curObj["Description"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The DisplayType property indicates how the network object should be displayed in " +
                 "a network browsing user interface.\nExample: Generic.")]
    public string DisplayType
    {
      get { return ((string) (curObj["DisplayType"])); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsInstallDateNull
    {
      get
      {
        if ((curObj["InstallDate"] == null))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The InstallDate property is datetime value indicating when the object was install" +
                 "ed. A lack of a value does not indicate that the object is not installed.")]
    [TypeConverter(typeof (WMIValueTypeConverter))]
    public DateTime InstallDate
    {
      get
      {
        if ((curObj["InstallDate"] != null))
        {
          return ToDateTime(((string) (curObj["InstallDate"])));
        }
        else
        {
          return DateTime.MinValue;
        }
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The LocalName property indicates the local name of the connected network device.\n" +
                 "Example: c:\\public.")]
    public string LocalName
    {
      get { return ((string) (curObj["LocalName"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The Name property indicates the name of the current network connection. It is the" +
                 " combination of the value in the RemoteName property and the value in the LocalN" +
                 "ame property.\nExample: \\\\NTRELEASE (c:\\public).")]
    public string Name
    {
      get { return ((string) (curObj["Name"])); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsPersistentNull
    {
      get
      {
        if ((curObj["Persistent"] == null))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The Persistent property determines whether this connection will be reconnected au" +
                 "tomatically by the operating system on the next logon.\nValues: TRUE or FALSE. A " +
                 "value of TRUE indicates the network connection will be automatically connected.")]
    [TypeConverter(typeof (WMIValueTypeConverter))]
    public bool Persistent
    {
      get
      {
        if ((curObj["Persistent"] == null))
        {
          return Convert.ToBoolean(0);
        }
        return ((bool) (curObj["Persistent"]));
      }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The ProviderName property contains the name of the provider that owns the resourc" +
                 "e. This property can be NULL if the provider name is unknown.")]
    public string ProviderName
    {
      get { return ((string) (curObj["ProviderName"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description(
        @"The RemoteName property contains the remote network resource name for a network resource. For a current or persistent connection, RemoteName contains the network name associated with the name of the value in the LocalName property. The name in RemoteName must follow the network provider's naming conventions. 
Example: \\NTRELEASE."
        )]
    public string RemoteName
    {
      get { return ((string) (curObj["RemoteName"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The RemotePath property contains the full path to the network resource.\nExample: " +
                 "\\\\infosrv1\\public")]
    public string RemotePath
    {
      get { return ((string) (curObj["RemotePath"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The ResourceType property identifies the type of resource to enumerate or connect" +
                 " to.")]
    public string ResourceType
    {
      get { return ((string) (curObj["ResourceType"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description(
        @"The Status property is a string indicating the current status of the object. Various operational and non-operational statuses can be defined. Operational statuses are ""OK"", ""Degraded"" and ""Pred Fail"". ""Pred Fail"" indicates that an element may be functioning properly but predicting a failure in the near future. An example is a SMART-enabled hard drive. Non-operational statuses can also be specified. These are ""Error"", ""Starting"", ""Stopping"" and ""Service"". The latter, ""Service"", could apply during mirror-resilvering of a disk, reload of a user permissions list, or other administrative work. Not all such work is on-line, yet the managed element is neither ""OK"" nor in one of the other states."
        )]
    public string Status
    {
      get { return ((string) (curObj["Status"])); }
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Description("The UserName property contains the user name or the default user name used to est" +
                 "ablish a network connection. \nExample: SYSTEM.")]
    public string UserName
    {
      get { return ((string) (curObj["UserName"])); }
    }

    private bool CheckIfProperClass(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions OptionsParam)
    {
      if (((path != null)
           && (string.Compare(path.ClassName, ManagementClassName, true, CultureInfo.InvariantCulture) == 0)))
      {
        return true;
      }
      else
      {
        return CheckIfProperClass(new ManagementObject(mgmtScope, path, OptionsParam));
      }
    }

    private bool CheckIfProperClass(ManagementBaseObject theObj)
    {
      if (((theObj != null)
           &&
           (string.Compare(((string) (theObj["__CLASS"])), ManagementClassName, true, CultureInfo.InvariantCulture) == 0)))
      {
        return true;
      }
      else
      {
        var parentClasses = ((Array) (theObj["__DERIVATION"]));
        if ((parentClasses != null))
        {
          int count = 0;
          for (count = 0; (count < parentClasses.Length); count = (count + 1))
          {
            if (
                (string.Compare(((string) (parentClasses.GetValue(count))), ManagementClassName, true,
                                CultureInfo.InvariantCulture) == 0))
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    private bool ShouldSerializeAccessMask()
    {
      if ((IsAccessMaskNull == false))
      {
        return true;
      }
      return false;
    }

    // Converts a given datetime in DMTF format to System.DateTime object.
    private static DateTime ToDateTime(string dmtfDate)
    {
      DateTime initializer = DateTime.MinValue;
      int year = initializer.Year;
      int month = initializer.Month;
      int day = initializer.Day;
      int hour = initializer.Hour;
      int minute = initializer.Minute;
      int second = initializer.Second;
      long ticks = 0;
      string dmtf = dmtfDate;
      DateTime datetime = DateTime.MinValue;
      string tempString = string.Empty;
      if ((dmtf == null))
      {
        throw new ArgumentOutOfRangeException();
      }
      if ((dmtf.Length == 0))
      {
        throw new ArgumentOutOfRangeException();
      }
      if ((dmtf.Length != 25))
      {
        throw new ArgumentOutOfRangeException();
      }
      try
      {
        tempString = dmtf.Substring(0, 4);
        if (("****" != tempString))
        {
          year = int.Parse(tempString);
        }
        tempString = dmtf.Substring(4, 2);
        if (("**" != tempString))
        {
          month = int.Parse(tempString);
        }
        tempString = dmtf.Substring(6, 2);
        if (("**" != tempString))
        {
          day = int.Parse(tempString);
        }
        tempString = dmtf.Substring(8, 2);
        if (("**" != tempString))
        {
          hour = int.Parse(tempString);
        }
        tempString = dmtf.Substring(10, 2);
        if (("**" != tempString))
        {
          minute = int.Parse(tempString);
        }
        tempString = dmtf.Substring(12, 2);
        if (("**" != tempString))
        {
          second = int.Parse(tempString);
        }
        tempString = dmtf.Substring(15, 6);
        if (("******" != tempString))
        {
          ticks = (long.Parse(tempString)*(((TimeSpan.TicksPerMillisecond/1000))));
        }
        if (((((((((year < 0)
                   || (month < 0))
                  || (day < 0))
                 || (hour < 0))
                || (minute < 0))
               || (minute < 0))
              || (second < 0))
             || (ticks < 0)))
        {
          throw new ArgumentOutOfRangeException();
        }
      }
      catch (Exception e)
      {
        throw new ArgumentOutOfRangeException(null, e.Message);
      }
      datetime = new DateTime(year, month, day, hour, minute, second, 0);
      datetime = datetime.AddTicks(ticks);
      TimeSpan tickOffset = TimeZone.CurrentTimeZone.GetUtcOffset(datetime);
      int UTCOffset = 0;
      int OffsetToBeAdjusted = 0;
      long OffsetMins = (((tickOffset.Ticks/TimeSpan.TicksPerMinute)));
      tempString = dmtf.Substring(22, 3);
      if ((tempString != "******"))
      {
        tempString = dmtf.Substring(21, 4);
        try
        {
          UTCOffset = int.Parse(tempString);
        }
        catch (Exception e)
        {
          throw new ArgumentOutOfRangeException(null, e.Message);
        }
        OffsetToBeAdjusted = ((int) ((OffsetMins - UTCOffset)));
        datetime = datetime.AddMinutes(((OffsetToBeAdjusted)));
      }
      return datetime;
    }

    // Converts a given System.DateTime object to DMTF datetime format.
    private static string ToDmtfDateTime(DateTime date)
    {
      string utcString = string.Empty;
      TimeSpan tickOffset = TimeZone.CurrentTimeZone.GetUtcOffset(date);
      long OffsetMins = (((tickOffset.Ticks/TimeSpan.TicksPerMinute)));
      if ((Math.Abs(OffsetMins) > 999))
      {
        date = date.ToUniversalTime();
        utcString = "+000";
      }
      else
      {
        if ((tickOffset.Ticks >= 0))
        {
          utcString = string.Concat("+", (((tickOffset.Ticks/TimeSpan.TicksPerMinute))).ToString().PadLeft(3, '0'));
        }
        else
        {
          string strTemp = ((OffsetMins)).ToString();
          utcString = string.Concat("-", strTemp.Substring(1, (strTemp.Length - 1)).PadLeft(3, '0'));
        }
      }
      string dmtfDateTime = ((date.Year)).ToString().PadLeft(4, '0');
      dmtfDateTime = string.Concat(dmtfDateTime, ((date.Month)).ToString().PadLeft(2, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, ((date.Day)).ToString().PadLeft(2, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, ((date.Hour)).ToString().PadLeft(2, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, ((date.Minute)).ToString().PadLeft(2, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, ((date.Second)).ToString().PadLeft(2, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, ".");
      var dtTemp = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, 0);
      long microsec = (((((date.Ticks - dtTemp.Ticks)
                          *1000)
                         /TimeSpan.TicksPerMillisecond)));
      string strMicrosec = ((microsec)).ToString();
      if ((strMicrosec.Length > 6))
      {
        strMicrosec = strMicrosec.Substring(0, 6);
      }
      dmtfDateTime = string.Concat(dmtfDateTime, strMicrosec.PadLeft(6, '0'));
      dmtfDateTime = string.Concat(dmtfDateTime, utcString);
      return dmtfDateTime;
    }

    private bool ShouldSerializeInstallDate()
    {
      if ((IsInstallDateNull == false))
      {
        return true;
      }
      return false;
    }

    private bool ShouldSerializePersistent()
    {
      if ((IsPersistentNull == false))
      {
        return true;
      }
      return false;
    }

    [Browsable(true)]
    public void CommitObject()
    {
      if ((isEmbedded == false))
      {
        PrivateLateBoundObject.Put();
      }
    }

    [Browsable(true)]
    public void CommitObject(PutOptions putOptions)
    {
      if ((isEmbedded == false))
      {
        PrivateLateBoundObject.Put(putOptions);
      }
    }

    private void Initialize()
    {
      AutoCommitProp = true;
      isEmbedded = false;
    }

    private static string ConstructPath(string keyName)
    {
      string strPath = "ROOT\\CIMV2:Win32_NetworkConnection";
      strPath = string.Concat(strPath, string.Concat(".Name=", string.Concat("\"", string.Concat(keyName, "\""))));
      return strPath;
    }

    private void InitializeObject(ManagementScope mgmtScope, ManagementPath path, ObjectGetOptions getOptions)
    {
      Initialize();
      if ((path != null))
      {
        if ((CheckIfProperClass(mgmtScope, path, getOptions) != true))
        {
          throw new ArgumentException("Class name does not match.");
        }
      }
      PrivateLateBoundObject = new ManagementObject(mgmtScope, path, getOptions);
      PrivateSystemProperties = new ManagementSystemProperties(PrivateLateBoundObject);
      curObj = PrivateLateBoundObject;
    }

    // Different overloads of GetInstances() help in enumerating instances of the WMI class.
    public static NetworkConnectionCollection GetInstances()
    {
      return GetInstances(null, null, null);
    }

    public static NetworkConnectionCollection GetInstances(string condition)
    {
      return GetInstances(null, condition, null);
    }

    public static NetworkConnectionCollection GetInstances(String[] selectedProperties)
    {
      return GetInstances(null, null, selectedProperties);
    }

    public static NetworkConnectionCollection GetInstances(string condition, String[] selectedProperties)
    {
      return GetInstances(null, condition, selectedProperties);
    }

    public static NetworkConnectionCollection GetInstances(ManagementScope mgmtScope, EnumerationOptions enumOptions)
    {
      if ((mgmtScope == null))
      {
        if ((statMgmtScope == null))
        {
          mgmtScope = new ManagementScope();
          mgmtScope.Path.NamespacePath = "root\\CIMV2";
        }
        else
        {
          mgmtScope = statMgmtScope;
        }
      }
      var pathObj = new ManagementPath();
      pathObj.ClassName = "Win32_NetworkConnection";
      pathObj.NamespacePath = "root\\CIMV2";
      var clsObject = new ManagementClass(mgmtScope, pathObj, null);
      if ((enumOptions == null))
      {
        enumOptions = new EnumerationOptions();
        enumOptions.EnsureLocatable = true;
      }
      return new NetworkConnectionCollection(clsObject.GetInstances(enumOptions));
    }

    public static NetworkConnectionCollection GetInstances(ManagementScope mgmtScope, string condition)
    {
      return GetInstances(mgmtScope, condition, null);
    }

    public static NetworkConnectionCollection GetInstances(ManagementScope mgmtScope, String[] selectedProperties)
    {
      return GetInstances(mgmtScope, null, selectedProperties);
    }

    public static NetworkConnectionCollection GetInstances(ManagementScope mgmtScope, string condition,
                                                           String[] selectedProperties)
    {
      if ((mgmtScope == null))
      {
        if ((statMgmtScope == null))
        {
          mgmtScope = new ManagementScope();
          mgmtScope.Path.NamespacePath = "root\\CIMV2";
        }
        else
        {
          mgmtScope = statMgmtScope;
        }
      }
      var ObjectSearcher = new ManagementObjectSearcher(mgmtScope,
                                                        new SelectQuery("Win32_NetworkConnection", condition,
                                                                        selectedProperties));
      var enumOptions = new EnumerationOptions();
      enumOptions.EnsureLocatable = true;
      ObjectSearcher.Options = enumOptions;
      return new NetworkConnectionCollection(ObjectSearcher.Get());
    }

    [Browsable(true)]
    public static NetworkConnection CreateInstance()
    {
      ManagementScope mgmtScope = null;
      if ((statMgmtScope == null))
      {
        mgmtScope = new ManagementScope();
        mgmtScope.Path.NamespacePath = CreatedWmiNamespace;
      }
      else
      {
        mgmtScope = statMgmtScope;
      }
      var mgmtPath = new ManagementPath(CreatedClassName);
      var tmpMgmtClass = new ManagementClass(mgmtScope, mgmtPath, null);
      return new NetworkConnection(tmpMgmtClass.CreateInstance());
    }

    [Browsable(true)]
    public void Delete()
    {
      PrivateLateBoundObject.Delete();
    }

    #region Nested type: ManagementSystemProperties

    [TypeConverter(typeof (ExpandableObjectConverter))]
    public class ManagementSystemProperties
    {
      private readonly ManagementBaseObject PrivateLateBoundObject;

      public ManagementSystemProperties(ManagementBaseObject ManagedObject)
      {
        PrivateLateBoundObject = ManagedObject;
      }

      [Browsable(true)]
      public int GENUS
      {
        get { return ((int) (PrivateLateBoundObject["__GENUS"])); }
      }

      [Browsable(true)]
      public string CLASS
      {
        get { return ((string) (PrivateLateBoundObject["__CLASS"])); }
      }

      [Browsable(true)]
      public string SUPERCLASS
      {
        get { return ((string) (PrivateLateBoundObject["__SUPERCLASS"])); }
      }

      [Browsable(true)]
      public string DYNASTY
      {
        get { return ((string) (PrivateLateBoundObject["__DYNASTY"])); }
      }

      [Browsable(true)]
      public string RELPATH
      {
        get { return ((string) (PrivateLateBoundObject["__RELPATH"])); }
      }

      [Browsable(true)]
      public int PROPERTY_COUNT
      {
        get { return ((int) (PrivateLateBoundObject["__PROPERTY_COUNT"])); }
      }

      [Browsable(true)]
      public string[] DERIVATION
      {
        get { return ((string[]) (PrivateLateBoundObject["__DERIVATION"])); }
      }

      [Browsable(true)]
      public string SERVER
      {
        get { return ((string) (PrivateLateBoundObject["__SERVER"])); }
      }

      [Browsable(true)]
      public string NAMESPACE
      {
        get { return ((string) (PrivateLateBoundObject["__NAMESPACE"])); }
      }

      [Browsable(true)]
      public string PATH
      {
        get { return ((string) (PrivateLateBoundObject["__PATH"])); }
      }
    }

    #endregion

    // Enumerator implementation for enumerating instances of the class.

    #region Nested type: NetworkConnectionCollection

    public class NetworkConnectionCollection : object, ICollection
    {
      private readonly ManagementObjectCollection privColObj;

      public NetworkConnectionCollection(ManagementObjectCollection objCollection)
      {
        privColObj = objCollection;
      }

      #region ICollection Members

      public virtual int Count
      {
        get { return privColObj.Count; }
      }

      public virtual bool IsSynchronized
      {
        get { return privColObj.IsSynchronized; }
      }

      public virtual object SyncRoot
      {
        get { return this; }
      }

      public virtual void CopyTo(Array array, int index)
      {
        privColObj.CopyTo(array, index);
        int nCtr;
        for (nCtr = 0; (nCtr < array.Length); nCtr = (nCtr + 1))
        {
          array.SetValue(new NetworkConnection(((ManagementObject) (array.GetValue(nCtr)))), nCtr);
        }
      }

      public virtual IEnumerator GetEnumerator()
      {
        return new NetworkConnectionEnumerator(privColObj.GetEnumerator());
      }

      #endregion

      #region Nested type: NetworkConnectionEnumerator

      public class NetworkConnectionEnumerator : object, IEnumerator
      {
        private readonly ManagementObjectCollection.ManagementObjectEnumerator privObjEnum;

        public NetworkConnectionEnumerator(ManagementObjectCollection.ManagementObjectEnumerator objEnum)
        {
          privObjEnum = objEnum;
        }

        #region IEnumerator Members

        public virtual object Current
        {
          get { return new NetworkConnection(((ManagementObject) (privObjEnum.Current))); }
        }

        public virtual bool MoveNext()
        {
          return privObjEnum.MoveNext();
        }

        public virtual void Reset()
        {
          privObjEnum.Reset();
        }

        #endregion
      }

      #endregion
    }

    #endregion

    // TypeConverter to handle null values for ValueType properties

    #region Nested type: WMIValueTypeConverter

    public class WMIValueTypeConverter : TypeConverter
    {
      private readonly TypeConverter baseConverter;

      private readonly Type baseType;

      public WMIValueTypeConverter(Type inBaseType)
      {
        baseConverter = TypeDescriptor.GetConverter(inBaseType);
        baseType = inBaseType;
      }

      public override bool CanConvertFrom(ITypeDescriptorContext context, Type srcType)
      {
        return baseConverter.CanConvertFrom(context, srcType);
      }

      public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
      {
        return baseConverter.CanConvertTo(context, destinationType);
      }

      public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
      {
        return baseConverter.ConvertFrom(context, culture, value);
      }

      public override object CreateInstance(ITypeDescriptorContext context, IDictionary dictionary)
      {
        return baseConverter.CreateInstance(context, dictionary);
      }

      public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
      {
        return baseConverter.GetCreateInstanceSupported(context);
      }

      public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value,
                                                                 Attribute[] attributeVar)
      {
        return baseConverter.GetProperties(context, value, attributeVar);
      }

      public override bool GetPropertiesSupported(ITypeDescriptorContext context)
      {
        return baseConverter.GetPropertiesSupported(context);
      }

      public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
      {
        return baseConverter.GetStandardValues(context);
      }

      public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
      {
        return baseConverter.GetStandardValuesExclusive(context);
      }

      public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
      {
        return baseConverter.GetStandardValuesSupported(context);
      }

      public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                       Type destinationType)
      {
        if ((baseType.BaseType == typeof (Enum)))
        {
          if ((value.GetType() == destinationType))
          {
            return value;
          }
          if ((((value == null)
                && (context != null))
               && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
          {
            return "NULL_ENUM_VALUE";
          }
          return baseConverter.ConvertTo(context, culture, value, destinationType);
        }
        if (((baseType == typeof (bool))
             && (baseType.BaseType == typeof (ValueType))))
        {
          if ((((value == null)
                && (context != null))
               && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
          {
            return "";
          }
          return baseConverter.ConvertTo(context, culture, value, destinationType);
        }
        if (((context != null)
             && (context.PropertyDescriptor.ShouldSerializeValue(context.Instance) == false)))
        {
          return "";
        }
        return baseConverter.ConvertTo(context, culture, value, destinationType);
      }
    }

    #endregion

    // Embedded class to represent WMI system Properties.
  }
}