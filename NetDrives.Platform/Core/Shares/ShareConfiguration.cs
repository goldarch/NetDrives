using System;
using System.Security;
using System.Xml.Linq;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Properties;
using Hardcodet.Util.Logging;
using Hardcodet.XItems;
using Microsoft.Practices.ServiceLocation;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Encapsulates configuration settings for a single share configuration, which
  /// are serialized to XML.
  /// </summary>
  public class ShareConfiguration : IdentifiableXItem, IContextRemovable, IShareConfiguration
  {
    /// <summary>
    /// The name of the XML root element that encapsulates
    /// the item's serialized data.
    /// </summary>
    public const string RootElementName = "Share";

    #region Name

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="Name" /> property.
    /// </summary>
    public const string NameAttribute = "Name";

    /// <summary>
    /// A descriptive (user) name for the share.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="NameAttribute" />.
    /// </summary>
    public string Name
    {
      get { return GetAttributeValue(NameAttribute, String.Empty); }
      set { SetAttributeValue("Name", NameAttribute, value); }
    }

    #endregion

    #region OrderIndex

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="OrderIndex" /> property.
    /// </summary>
    public const string IndexAttribute = "Index";

    /// <summary>
    /// The order index of the share. Used to display shares in user-configured order.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="IndexAttribute" />.
    /// </summary>
    public int OrderIndex
    {
      get { return GetAttributeValue(IndexAttribute, 0); }
      set { SetAttributeValue("OrderIndex", IndexAttribute, value); }
    }

    #endregion

    #region DriveLetter

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="DriveLetter" /> property.
    /// </summary>
    public const string DriveLetterAttribute = "DriveLetter";

    /// <summary>
    /// The drive letter to be registered for this share, if any.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="DriveLetterAttribute" />.
    /// </summary>
    public char? DriveLetter
    {
      get { return GetAttributeValue<char?>(DriveLetterAttribute, null); }
      set { SetAttributeValue("DriveLetter", DriveLetterAttribute, value); }
    }

    #endregion

    #region VolumeLabel

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="VolumeLabel" /> property.
    /// </summary>
    public const string VolumeLabelAttribute = "VolumeLabel";

    /// <summary>
    /// A label to be registered for this share, which causes file managers
    /// to display the share with this label.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="VolumeLabelAttribute" />.
    /// </summary>
    public string VolumeLabel
    {
      get { return GetAttributeValue(VolumeLabelAttribute, String.Empty); }
      set { SetAttributeValue("VolumeLabel", VolumeLabelAttribute, value); }
    }

    #endregion

    #region AutoConnect

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="AutoConnect" /> property.
    /// </summary>
    public const string AutoConnectAttribute = "AutoConnect";

    /// <summary>
    /// Whether a connection to the share should be automatically
    /// established on application startup. Defaults to false.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="AutoConnectAttribute" />.
    /// </summary>
    public bool AutoConnect
    {
      get { return GetAttributeValue(AutoConnectAttribute, false); }
      set { SetAttributeValue("AutoConnect", AutoConnectAttribute, value); }
    }

    #endregion

    #region NetworkLocation

    /// <summary>
    /// The name of the XML element that contains the
    /// serialized value of the <see cref="NetworkLocation" /> property.
    /// </summary>
    public const string NetworkLocationElement = "NetworkLocation";

    /// <summary>
    /// The remote (network) address of the share.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as element
    /// <see cref="NetworkLocationElement" />.
    /// </summary>
    public string NetworkLocation
    {
      get { return GetElementValue(NetworkLocationElement, String.Empty); }
      set { SetElementValue("NetworkLocation", NetworkLocationElement, value); }
    }

    #endregion

    #region UserName

    /// <summary>
    /// The name of the XML element that contains the
    /// serialized value of the <see cref="UserName" /> property.
    /// </summary>
    public const string UserNameElement = "UserName";

    /// <summary>
    /// The user name to be used when connecting to the share.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as element
    /// <see cref="UserNameElement" />.
    /// </summary>
    public string UserName
    {
      get { return GetElementValue(UserNameElement, String.Empty); }
      set { SetElementValue("UserName", UserNameElement, value); }
    }

    #endregion

    #region Password

    /// <summary>
    /// The name of the XML element that contains the
    /// serialized value of the <see cref="Password" /> property.
    /// </summary>
    public const string PasswordElement = "Password";

    /// <summary>
    /// The password to be used when connecting to the share.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as element
    /// <see cref="PasswordElement" />.
    /// </summary>
    public SecureString Password
    {
      get
      {
        var elementValue = GetElementValue(PasswordElement, String.Empty);
        if (String.IsNullOrEmpty(elementValue)) return null;

        try
        {
          //in case the user migrated or corrupted her configuration file,
          //decryption will fail - log error and reset the password in that case
          return elementValue.DecryptSecure();
        }
        catch(Exception e)
        {
          string msg = Resources.Error_Message_Logged_If_Password_Decryption_Failed;
          msg = String.Format(msg, Name);
          
          ServiceLocator.Current.GetInstance<ILogger>().Log(msg, e);
          Password = null;
          return null;
        }
      }
      set
      {
        string cipher = value == null ? String.Empty : value.Encrypt();
        SetElementValue("Password", PasswordElement, cipher);
      }
    }

    #endregion

    #region Login

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="Login" /> property.
    /// </summary>
    public const string LoginStrategyAttribute = "LoginStrategy";

    /// <summary>
    /// Defines how the login procedure should be handled (automatic login vs. prompting for credentials).<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="LoginStrategyAttribute" />.
    /// </summary>
    public LoginStrategy Login
    {
      get { return GetAttributeValue(LoginStrategyAttribute, LoginStrategy.Prompt); }
      set { SetAttributeValue("Login", LoginStrategyAttribute, value); }
    }

    #endregion


    /// <summary>
    /// Creates the item based on the underlying XML element.
    /// </summary>
    /// <param name="element">Element that encapsulates the item's
    /// data.</param>
    /// <exception cref="T:System.ArgumentNullException">If <paramref name="element" />
    /// is a null reference.</exception>
    /// <exception cref="T:System.InvalidOperationException">If the submitted element
    /// does not provide an identifier attribute with a name that matches the
    /// <see cref="F:Hardcodet.XItems.IdentifiableXItem.IdAttribute" /> constant
    /// (empty attribute is accepted).</exception>
    public ShareConfiguration(XElement element) : base(element)
    {
    }


    #region IContextRemovable

    /// <summary>
    /// Fired after the item has been removed from its context.
    /// </summary>
    public event EventHandler Removed;

    /// <summary>
    /// Marks the item as removed, which triggers the
    /// <see cref="Removed" />  event and sets the
    /// <see cref="IsRemoved" /> property to true.
    /// </summary>
    /// <exception cref="InvalidOperationException">If the item is already
    /// marked as removed.</exception>
    public void MarkRemoved()
    {
      if (IsRemoved)
      {
        var message = "Share {0} has already been marked as removed.".Put(ItemId);
        throw new InvalidOperationException(message);
      }

      IsRemoved = true;
      OnPropertyChanged("IsRemoved");
      if (Removed != null) Removed(this, EventArgs.Empty);
    }

    /// <summary>
    /// A boolean property which indicates whether the item was
    /// removed or not.
    /// </summary>
    public bool IsRemoved { get; private set; }

    #endregion


    #region CreateDefaultCredentials

    /// <summary>
    /// Creates a <see cref="ShareCredentials"/> instance that
    /// corresponds to the configured user name and password.
    /// 
    /// </summary>
    /// <returns>Corresponding <see cref="ShareCredentials"/>
    /// instance.</returns>
    public ShareCredentials CreateDefaultCredentials()
    {
      return new ShareCredentials(UserName, Password);
    }

    #endregion

  }
}
