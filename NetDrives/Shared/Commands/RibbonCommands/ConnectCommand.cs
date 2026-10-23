using System.Windows.Input;

namespace Hardcodet.NetDrives.Shared.Commands.RibbonCommands
{
  /// <summary>
  /// Attempts to establish a connection to a given target.
  /// </summary>
  public class ConnectCommand : RibbonCommandBase
  {
    private readonly ConnectionActionCommand cmd = new ConnectionActionCommand();

    protected override void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = cmd.CanExecute(ConnectionActionCommand.ConnectParameter);
    }


    protected override void OnExecute(object sender, ExecutedRoutedEventArgs e)
    {
      cmd.Execute(ConnectionActionCommand.ConnectParameter);
    }
  }
}