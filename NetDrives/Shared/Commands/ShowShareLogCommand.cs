using System.Windows;
using System.Windows.Input;
using Hardcodet.NetDrives.UI.Connections;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Displays the status log entries of the currently active share.
  /// </summary>
  public class ShowShareLogCommand : CommandExtension<ShowShareLogCommand>
  {
    public override void Execute(object parameter)
    {
      var dlg = new StatusLogDialog(Controller.ActiveShare);
      dlg.SetDialogOwnerAndLocation();
      dlg.ShowDialog();
    }


    public override bool CanExecute(object parameter)
    {
      return Controller.ActiveShare != null;
    }

  }
}