using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Hardcodet.NetDrives.Shared.Converters
{
  /// <summary>
  /// Converts a boolean value into a <see cref="Visibility"/>
  /// flag, optionally inverted by settings the parameter
  /// to an arbitrary value. If no parameter is being set,
  /// a boolean value of True is being converted to
  /// <see cref="Visibility.Visible"/>, while a false value
  /// will result in <see cref="Visibility.Collapsed"/>.
  /// </summary>
  [ValueConversion(typeof (bool), typeof (Visibility))]
  public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
  {
    private static BoolToVisibilityConverter converter;

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
        converter = new BoolToVisibilityConverter();
      }

      return converter;
    }



    /// <summary>
    /// Collapses an item if the submitted value is not true.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      bool status = (bool?) value ?? false;
      if (parameter != null) status = !status;
      return status ? Visibility.Visible : Visibility.Collapsed;
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Visibility visiblity = (Visibility) value;
      return visiblity == Visibility.Visible;
    }
  }
}