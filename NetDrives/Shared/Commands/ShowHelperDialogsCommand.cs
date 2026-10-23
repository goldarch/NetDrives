using System;
using System.Diagnostics;
using System.Windows;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.UI.Common;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Displays common helper dialogs.
  /// </summary>
  public class ShowHelperDialogsCommand : CommandExtension<ShowHelperDialogsCommand>
  {
    public override void Execute(object parameter)
    {
      string s = parameter as string;
      if (String.IsNullOrEmpty(s)) return;

      switch(s)
      {
        case "About":
          new AboutDialog().ShowCenteredDialog();
          break;
        case "CheckForUpdate":
          var uri = Properties.Resources.NetDrives_Homepage;
          Process.Start(uri);
          break;
        case "OrganizeShares":
          new ShareOrganizerDialog().ShowCenteredDialog();
          break;
        case "Options":
          new OptionsDialog().ShowCenteredDialog();
          break;
        default:
          string msg = "Command {0} invoked with unknown parameter {1}.";
          msg = String.Format(msg, GetType().Name, s);
          MessageBox.Show(msg);
          break;
      }
    }


  }
}