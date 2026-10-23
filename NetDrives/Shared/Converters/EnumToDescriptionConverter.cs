using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Hardcodet.Commons;


namespace Hardcodet.NetDrives.Shared.Converters
{
  ///<summary>
  /// Converts an enum value into a description.
  ///</summary>
  [ValueConversion(typeof(Enum), typeof(string))]
  public class EnumToDescriptionConverter : MarkupExtension, IValueConverter
  {
    private static EnumToDescriptionConverter converter;

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
        converter = new EnumToDescriptionConverter();
      }

      return converter;
    }



    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Enum en = value as Enum;
      if (en == null) return null;

      return en.GetDescription();
    }



    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException("This converter supports only one-way conversion");
    }
  }
}