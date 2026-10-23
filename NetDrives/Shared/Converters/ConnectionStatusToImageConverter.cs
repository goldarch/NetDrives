using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Hardcodet.NetDrives.Platform.Core.Shares;


namespace Hardcodet.NetDrives.Shared.Converters
{
  ///<summary>
  /// Returns an image path that represents a status icon for a
  /// given <see cref="ConnectionStatus"/>.
  ///</summary>
  [ValueConversion(typeof(ConnectionStatus), typeof(string))]
  public class ConnectionStatusToImageConverter : MarkupExtension, IValueConverter
  {
    private static ConnectionStatusToImageConverter converter;

    /// <summary>
    /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension. 
    /// </summary>
    /// <returns>
    /// The object value to set on the property where the extension is applied. 
    /// </returns>
    /// <param name="serviceProvider">
    /// Object that can provide services for the markup extension.
    /// </param>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      if (converter == null)
      {
        converter = new ConnectionStatusToImageConverter();
      }

      return converter;
    }


    /// <summary>
    /// Converts a value. 
    /// </summary>
    /// <returns>
    /// A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    /// <param name="value">
    /// The value produced by the binding source.
    /// </param>
    /// <param name="targetType">
    /// The type of the binding target property.
    /// </param>
    /// <param name="parameter">
    /// The converter parameter to use.
    /// </param>
    /// <param name="culture">
    /// The culture to use in the converter.
    /// </param>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      ConnectionStatus status = value == null
                                    ? ConnectionStatus.Disconnected
                                    : (ConnectionStatus) value;

      switch(status)
      {
        case ConnectionStatus.Connected:
          return "/Shared/Images/Status/Connected.png";
        case ConnectionStatus.ConnectionError:
          return "/Shared/Images/Status/ConnectionError.png";
        case ConnectionStatus.Degraded:
          return "/Shared/Images/Status/Degraded.png";
        default:
          return "/Shared/Images/Status/Disconnected.png";
      }

    }


    /// <summary>
    /// Converts a value. 
    /// </summary>
    /// <returns>
    /// A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    /// <param name="value">
    /// The value that is produced by the binding target.
    /// </param>
    /// <param name="targetType">
    /// The type to convert to.
    /// </param>
    /// <param name="parameter">
    /// The converter parameter to use.
    /// </param>
    /// <param name="culture">
    /// The culture to use in the converter.
    /// </param>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException("This converter supports only one-way conversion");
    }
  }
}