using System.Windows.Input;

namespace Hardcodet.NetDrives.Shared.Commands.RibbonCommands
{
  ///<summary>
  /// Opens a given share location through the system's file manager.
  ///</summary>
  public class BrowseNetLocationCommand : CommandExtension<BrowseNetLocationCommand>
  {
    private readonly ConnectionActionCommand cmd = new ConnectionActionCommand();

    protected override void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = cmd.CanExecute(ConnectionActionCommand.BrowseParameter);
    }


    protected override void OnExecute(object sender, ExecutedRoutedEventArgs e)
    {
      cmd.Execute(ConnectionActionCommand.BrowseParameter);
    }
  }
}