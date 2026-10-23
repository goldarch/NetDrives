using System;
using Hardcodet.NetDrives.ViewModel;
using Hardcodet.NetDrives.Properties;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Opens / closes / minimizes the application.
  /// </summary>
  public class ApplicationCommand : CommandExtension<ApplicationCommand>
  {

    /// <summary>
    /// Opens, closes, or minimizes the application depending on the
    /// submitted command parameter.
    /// </summary>
    public override void Execute(object parameter)
    {
      var controller = AppController.Instance;
      switch ((string) parameter)
      {
        case "Open":
          controller.ShowMainWindow();
          break;
        case "Quit":
          controller.CloseMainApplication(true);
          break;
        case "Close":
          controller.CloseMainApplication(false);
          break;
        case "Minimize":
          controller.MinimizeToTray();
          break;
        default:
          string template = Resources.ResourceManager.GetString("Error_InvalidCommandParameter", Resources.Culture)
                            ?? "ApplicationCommand fired with missing or invalid parameter: {0}";
          throw new InvalidOperationException(String.Format(template, parameter));
      }
    }
  }
}