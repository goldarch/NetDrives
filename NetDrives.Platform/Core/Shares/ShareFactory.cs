using System;
using System.Xml.Linq;
using Hardcodet.XItems;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Provides builder methods for core elements.
  /// </summary>
  public static class ShareFactory
  {
    /// <summary>
    /// Creates a new share configuration with a unique
    /// identifier.
    /// </summary>
    /// <returns>Empty configuration.</returns>
    public static ShareConfiguration CreateConfiguration()
    {
      var id = Guid.NewGuid().ToString();
      var element = IdentifiableXItem.CreateKeyedElement(ShareConfiguration.RootElementName, id);
      return CreateConfiguration(element);
    }


    /// <summary>
    /// Creates a new share configuration instance based on a given element
    /// that encapsulates the configuration's data.
    /// </summary>
    /// <returns>Configuration instances.</returns>
    public static ShareConfiguration CreateConfiguration(XElement element)
    {
      return new ShareConfiguration(element);
    }
  }
}