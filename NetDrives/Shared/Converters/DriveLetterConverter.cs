using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Hardcodet.NetDrives.Platform;


namespace Hardcodet.NetDrives.Shared.Converters
{
  ///<summary>
  /// Returns a formatted drive letter string for a given
  /// drive letter character.
  ///</summary>
  [ValueConversion(typeof(string), typeof(string))]
  public class DriveLetterConverter : MarkupExtension, IValueConverter
  {
    private static DriveLetterConverter converter;

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
        converter = new DriveLetterConverter();
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
      char? driveLetter = value as char?;
      return driveLetter.ToDriveLetterString();
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