using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Hardcodet.Commons;
using Hardcodet.XItems;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// A wrapper for XML-based share configurations.
  /// </summary>
  public class ShareStore : XItem
  {
    /// <summary>
    /// The name of the XML root element that encapsulates
    /// the item's serialized data.
    /// </summary>
    public const string RootElementName = "NetDrives";


    #region Version

    /// <summary>
    /// The name of the XML attribute that contains the
    /// serialized value of the <see cref="Version" /> property.
    /// </summary>
    public const string VersionAttribute = "Version";

    /// <summary>
    /// The version of the underlying markup. Used in order to perform
    /// upgrades if necessary.<br/>
    /// This property's value is being serialized to the underlying
    /// XML <see cref="XItem.Element" /> as attribute
    /// <see cref="VersionAttribute" />.
    /// </summary>
    public string Version
    {
      get { return GetAttributeValue(VersionAttribute, String.Empty); }
      set { SetAttributeValue("Version", VersionAttribute, value); }
    }

    #endregion
    

    #region ShareConfigurations

    /// <summary>
    /// The name of the XML element that contains the
    /// serialized data of the application's shares.
    /// </summary>
    public const string ShareConfigurationsElementName = "Shares";

    /// <summary>
    /// Gets all registered shares of the application.
    /// </summary>
    private readonly HybridXCollection<ShareConfiguration, IShareConfiguration> shares;

    /// <summary>
    /// Gets all registered shares of the application.
    /// </summary>
    public IEnumerable<IShareConfiguration> ShareConfigurations
    {
      get { return shares; }
    }

    #endregion


    #region construction

    /// <summary>
    /// Creates the item based on the underlying XML element.
    /// </summary>
    /// <param name="element">Element that encapsulates the item's
    /// data.</param>
    /// <exception cref="T:System.ArgumentNullException">If <paramref name="element" />
    /// is a null reference.</exception>
    public ShareStore(XElement element) : base(element)
    {
      //init child shares collection
      var settings = XCollectionSettings<ShareConfiguration>.CreateStrict();
      XElement parent = GetOrCreateChildElement(ShareConfigurationsElementName);
      shares = new HybridXCollection<ShareConfiguration, IShareConfiguration>(parent, settings, ShareFactory.CreateConfiguration);

      //set version if not defined yet (new element)
      if (String.IsNullOrEmpty(Version)) Version = "1.0.0";
    }

    #endregion


    #region Add / Remove Configurations

    /// <summary>
    /// Adds a new configuration to the store.
    /// </summary>
    public void AddConfiguration(IShareConfiguration configuration)
    {
      //reset order indices (fill gaps)
      var exisingShares = shares.OrderBy<IShareConfiguration, int>(c => c.OrderIndex).ToArray();
      for (int i = 0; i < exisingShares.Length; i++)
      {
        exisingShares[i].OrderIndex = i;
      }

      shares.Add((ShareConfiguration) configuration);
      configuration.OrderIndex = exisingShares.Length;
    }

    /// <summary>
    /// Removes a given configuration from the store.
    /// </summary>
    public bool RemoveConfiguration(IShareConfiguration configuration)
    {
      return shares.Remove((ShareConfiguration) configuration);
    }


    /// <summary>
    /// Removes all registered configurations from the store.
    /// </summary> 
    public void Clear()
    {
      shares.Clear();
    }

    #endregion



    /// <summary>
    /// Gets the value of the <see cref="VersionAttribute"/> attribute from
    /// a given XML document.
    /// </summary>
    /// <param name="document">A share configuration to be inspected before
    /// parsing it into a <see cref="ShareStore"/>.</param>
    /// <returns>The value of the root element's <see cref="VersionAttribute"/>
    /// or <see cref="string.Empty"/> if root element or version attribute
    /// cannot be found.</returns>
    /// <exception cref="ArgumentNullException">If <paramref name="document"/>
    /// is a null reference.</exception>
    public static string GetConfigurationVersion(XDocument document)
    {
      Ensure.ArgumentNotNull(() => document);

      if (document.Root == null) return String.Empty;
      return document.Root.GetSafeAttributeValue(VersionAttribute, String.Empty);
    }



    /// <summary>
    /// Gets a given share by its <see cref="IdentifiableXItem.ItemId"/>.
    /// </summary>
    /// <param name="shareId">The share's ID.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException">If <paramref name="shareId"/>
    /// is a null reference.</exception>
    /// <exception cref="ArgumentException">If no share matches the submitted ID.</exception>
    public IShareConfiguration GetShare(string shareId)
    {
      Ensure.ArgumentNotNull(() => shareId);

      var config = shares.FirstOrDefault<IShareConfiguration>(s => s.ItemId == shareId);
      if (config == null)
      {
        var message = "Invalid share ID: {0}".Put(shareId);
        throw new ArgumentException(message);
      }

      return config;
    }

  }
}