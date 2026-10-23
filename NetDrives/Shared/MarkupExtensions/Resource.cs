using System;
using System.Windows.Data;
using System.Windows.Markup;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.Shared.MarkupExtensions
{
  /// <summary>
  /// A markup extension that provides simple access to a given
  /// entry in the application's <see cref="Resources"/> file.
  /// </summary>
  public class Resource : MarkupExtension
  {
    /// <summary>
    /// The resource key to be used for the lookup.
    /// </summary>
    public string ResourceKey { get; set; }


    /// <summary>
    /// Default contructor - requires the <see cref="ResourceKey"/>
    /// to be assigned through property assignment.
    /// </summary>
    public Resource()
    {
    }

    /// <summary>
    /// Inits the <see cref="Resource"/> markup extension
    /// with the key to be assigned.
    /// </summary>
    /// <param name="resourceKey">The resource key to be assigned.</param>
    public Resource(string resourceKey)
    {
      ResourceKey = resourceKey;
    }


    /// <summary>
    /// Performs a lookup for the defined <see cref="ResourceKey"/>.
    /// </summary>
    /// <returns>
    /// The value of the resource that is specified by the
    /// <see cref="ResourceKey"/> property. If the property is not
    /// set, a null reference is returned.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (String.IsNullOrEmpty(ResourceKey)) return null;

      var binding = new Binding("[" + ResourceKey + "]");
      binding.Source = LocalizationManager.Instance;
      binding.Mode = BindingMode.OneWay;
      binding.FallbackValue = ResourceKey;

      return binding.ProvideValue(serviceProvider);
    }
  }
}
