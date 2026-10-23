using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hardcodet.NetDrives.Util
{
  /// <summary>
  /// Declares attached properties for arbitrary controls.
  /// </summary>
  public class Attach : DependencyObject
  {
    #region ButtonImage

    /// <summary>
    /// ImageSource Attached Dependency Property
    /// </summary>
    public static readonly DependencyProperty ButtonImageProperty;

    /// <summary>
    /// Gets the ImageSource property.  This dependency property 
    /// indicates the image to be included with an image button.
    /// </summary>
    public static ImageSource GetButtonImage(DependencyObject obj)
    {

      return (ImageSource)obj.GetValue(ButtonImageProperty);
    }

    /// <summary>
    /// Sets the ImageSource property.  This dependency property 
    /// indicates the image to be included with an image button.
    /// </summary>
    public static void SetButtonImage(DependencyObject obj, ImageSource value)
    {
      obj.SetValue(ButtonImageProperty, value);
    }

    #endregion


    static Attach()
    {
      ButtonImageProperty = 
          DependencyProperty.RegisterAttached("ButtonImage", typeof(ImageSource), typeof(Attach),
                                              new FrameworkPropertyMetadata((ImageSource)null));
    }
  }

}