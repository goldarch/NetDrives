using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.UI.Common;

namespace Hardcodet.NetDrives.Shared.MarkupExtensions
{
  /// <summary>
  /// A markup extension that returns a
  /// <see cref="InfoPopup"/> control preconfigured
  /// with header and text information according to the
  /// <see cref="Title"/> and <see cref="Body"/>
  /// properties.
  /// </summary>
  public class Info : MarkupExtension
  {
    /// <summary>
    /// Either a title text or a resource key that can be used
    /// to look up the title.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Either a tooltip's main text or a resource key that can be used
    /// to look up the text.
    /// </summary>
    public string Body { get; set; }


    /// <summary>
    /// Empty default constructor.
    /// </summary>
    public Info()
    {
    }

    /// <summary>
    /// Inits the <see cref="Info"/> markup extension for
    /// a tooltip that provides only the tooltip text (no title).
    /// </summary>
    public Info(string body) : this(body, String.Empty)
    {
    }


    /// <summary>
    /// Inits the <see cref="Info"/> markup extension
    /// with title and body.
    /// </summary>
    public Info(string title, string body)
    {
      Title = title;
      Body = body;
    }

    /// <summary>
    /// Performs a lookup for the defined <see cref="Title"/> and
    /// <see cref="Body"/> and creates the tooltip control.
    /// </summary>
    /// <returns>
    /// The value of the resource that is specified by the
    /// <see cref="Title"/> property. If the property is not
    /// set, a null reference is returned.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      //create the user control that 
      InfoPopup popup = new InfoPopup();

      if (!String.IsNullOrEmpty(Title))
      {
        //look up title - if the string is not a
        //resource key, use it directly
        var result = Resources.ResourceManager.GetObject(Title) ?? Title;
        popup.HeaderText = (string)result;
      }

      if (!String.IsNullOrEmpty(Body))
      {
        //look up body text - if the string is not a
        //resource key, use it directly
        var result = Resources.ResourceManager.GetObject(Body) ?? Body;
        popup.BodyText = (string)result;
      }

      //create tooltip and make sure it's not visible
      ToolTip tt = new ToolTip();
      tt.HasDropShadow = false;
      tt.BorderThickness = new Thickness(0);
      tt.Background = Brushes.Transparent;
      tt.Content = popup;

      return tt;
    }
  }
}