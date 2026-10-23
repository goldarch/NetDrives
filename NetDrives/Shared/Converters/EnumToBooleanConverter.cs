using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Hardcodet.NetDrives.Shared.Converters
{
  ///<summary>
  /// Converts a enum value in to a bool if the submitted
  /// converter parameter matches a bound enum value.
  ///</summary>
  [ValueConversion(typeof(Enum), typeof(bool))]
  public class EnumToBooleanConverter
      : MarkupExtension, IValueConverter
  {
    private static EnumToBooleanConverter converter;

    /// <summary>
    /// When implemented in a derived class, returns an object that is set
    /// as the value of the target property for this markup extension. 
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
        converter = new EnumToBooleanConverter();
      }

      return converter;
    }


    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string suggestedEnum = parameter as string;
      if (suggestedEnum == null) return Binding.DoNothing;

      //if the bound value is not a declared enum value, don't do anything
      if (!Enum.IsDefined(value.GetType(), value)) return Binding.DoNothing;

      object enumValue = Enum.Parse(value.GetType(), suggestedEnum);
      return enumValue.Equals(value);
    }


    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      string enumValue = parameter as string;
      if (enumValue == null) return Binding.DoNothing;

      return Enum.Parse(targetType, enumValue);
    }

  }
}