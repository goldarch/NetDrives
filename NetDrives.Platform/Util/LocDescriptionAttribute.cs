using System;
using System.ComponentModel;
using Hardcodet.NetDrives.Platform.Properties;

namespace Hardcodet.NetDrives.Platform.Util
{
  /// <summary>
  /// Provides resource lookups based on a resource key.
  /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  internal sealed class LocDescriptionAttribute : DescriptionAttribute
  {
    /// <summary>
    /// Whether the resource lookup was already performed or not.
    /// </summary>
    private bool lookupPerformed;


    /// <summary>
    /// Inits the attribute with a given resource key.
    /// </summary>
    /// <param name="resourceKey">A key which is looked up
    /// on demand in order to resolve the requested resource string.</param>
    public LocDescriptionAttribute(string resourceKey) : base(resourceKey)
    {
    }


    /// <summary>
    /// Gets a localized description.
    /// </summary>
    public override string Description
    {
      get
      {
        if (!lookupPerformed)
        {
          lookupPerformed = true;
          DescriptionValue = Resources.ResourceManager.GetString(base.Description);
        }

        return base.Description;
      }
    }
  }
}