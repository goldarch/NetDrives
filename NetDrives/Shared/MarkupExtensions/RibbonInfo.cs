using System;
using System.Windows;
using Infragistics.Windows.Ribbon;
using Resources=Hardcodet.NetDrives.Properties.Resources;


namespace Hardcodet.NetDrives.Shared.MarkupExtensions
{
  /// <summary>
  /// Extension that creates a <see cref="XamRibbonScreenTip"/>
  /// that can be used to display a rich tooltip on the
  /// ribbon.
  /// </summary>
  public class RibbonInfo: Info
  {
    /// <summary>
    /// An optional string that's either the tooltip's footer
    /// text or a resource key that can be used
    /// to look up the footer value.
    /// </summary>
    public string Footer { get; set; }


        /// <summary>
    /// Empty default constructor.
    /// </summary>
    public RibbonInfo()
    {
    }

    /// <summary>
    /// Inits the <see cref="Info"/> markup extension for
    /// a tooltip that provides only the tooltip text (no title).
    /// </summary>
    public RibbonInfo(string body) : this(body, String.Empty)
    {
    }

   
    
    /// <summary>
    /// Inits the markup extension with title and body.
    /// </summary>
    public RibbonInfo(string title, string body) : this(title, body, String.Empty)
    {
    }


    /// <summary>
    /// Inits the markup extension with title, body, and footer.
    /// </summary>
    public RibbonInfo(string title, string body, string footer)
    {
      Title = title;
      Body = body;
      Footer = footer;
    }


    /// <summary>
    /// Performs a lookup for the defined <see cref="Info.Title"/> and
    /// <see cref="Info.Body"/> and creates the tooltip control.
    /// </summary>
    /// <returns>
    /// The value of the resource that is specified by the
    /// <see cref="Info.Title"/> property. If the property is not
    /// set, a null reference is returned.
    /// </returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      //create the user control that 
      XamRibbonScreenTip tt = new XamRibbonScreenTip();

      if (!String.IsNullOrEmpty(Title))
      {
        //look up title - if the string is not a
        //resource key, use it directly
        var result = Resources.ResourceManager.GetObject(Title) ?? Title;
        tt.Header = result;
      }


      if (!String.IsNullOrEmpty(Body))
      {
        //look up body text - if the string is not a
        //resource key, use it directly
        var result = Resources.ResourceManager.GetObject(Body) ?? Body;
        tt.Content = result;
      }


      if (!String.IsNullOrEmpty(Footer))
      {
        //look up body text - if the string is not a
        //resource key, use it directly
        var result = Resources.ResourceManager.GetObject(Footer) ?? Footer;
        tt.Footer = result;
        tt.FooterSeparatorVisibility = Visibility.Visible;
      }
      else
      {
        tt.FooterSeparatorVisibility = Visibility.Collapsed;
      }

      //create tooltip and make sure it's not visible
      return tt;
    }
  }
}
