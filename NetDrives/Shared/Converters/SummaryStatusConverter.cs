using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Hardcodet.NetDrives.Platform.Core;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.Properties;


namespace Hardcodet.NetDrives.Shared.Converters
{
  ///<summary>
  /// Returns image sources or tooltip texts for the
  /// <see cref="ShareManager.SummaryStatus"/> flag.
  ///</summary>
  [ValueConversion(typeof(ConnectionStatus), typeof(string))]
  public class SummaryStatusConverter : MarkupExtension, IValueConverter
  {
    private static SummaryStatusConverter converter;

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
        converter = new SummaryStatusConverter();
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
                                    : (ConnectionStatus)value;

      string resolveType = parameter as string;

      if (resolveType == "TrayIcon")
      {
        switch (status)
        {
          case ConnectionStatus.Connected:
            return "/Shared/Images/TrayIcons/Connected.ico";
          case ConnectionStatus.ConnectionError:
            return "/Shared/Images/TrayIcons/ConnectionError.ico";
          case ConnectionStatus.Degraded:
            return "/Shared/Images/TrayIcons/Degraded.ico";
          default:
            return "/Shared/Images/TrayIcons/NoConnections.ico";
        }
      }

      if (resolveType == "ToolTipImage")
      {
        switch (status)
        {
          case ConnectionStatus.Connected:
            return "/Shared/Images/TrayImages/Connected.png";
          case ConnectionStatus.ConnectionError:
            return "/Shared/Images/TrayImages/ConnectionError.png";
          case ConnectionStatus.Degraded:
            return "/Shared/Images/TrayImages/Degraded.png";
          default:
            return "/Shared/Images/TrayImages/NoConnections.png";
        }
      }

      if (resolveType == "ToolTipText")
      {
        switch (status)
        {
          case ConnectionStatus.Connected:
            return Resources.SysTray_Tooltip_Text_When_All_Connections_Valid;
          case ConnectionStatus.ConnectionError:
            return Resources.SysTray_Tooltip_Text_When_Connection_Errors;
          case ConnectionStatus.Degraded:
            return Resources.SysTray_Tooltip_Text_When_Connection_Degraded;
          default:
            return Resources.SysTray_Tooltip_Text_When_No_Connections;
        }
      }

      return Binding.DoNothing;
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