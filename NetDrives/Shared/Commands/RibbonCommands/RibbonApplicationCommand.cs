using System;
using System.Windows.Input;

namespace Hardcodet.NetDrives.Shared.Commands.RibbonCommands
{
  /// <summary>
  /// Hides the main window.
  /// </summary>
  public class RibbonApplicationCommand : RibbonCommandBase
  {
    protected override void OnExecute(object sender, ExecutedRoutedEventArgs e)
    {
      switch ((string)e.Parameter)
      {
        case "Quit":
          Controller.CloseMainApplication(true);
          break;
        case "Close":
          Controller.CloseMainApplication(false);
          break;
        case "Minimize":
          Controller.MinimizeToTray();
          break;
        default:
          string msg = "TrayApplicationCommand fired with missing or invalid parameter: {0}";
          msg = String.Format(msg, e.Parameter);
          throw new InvalidOperationException(msg);
      }
    }
  }
}