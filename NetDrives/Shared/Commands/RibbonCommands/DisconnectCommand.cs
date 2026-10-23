using System.Windows.Input;

namespace Hardcodet.NetDrives.Shared.Commands.RibbonCommands
{
  /// <summary>
  /// Cancels a given connection.
  /// </summary>
  public class DisconnectCommand : RibbonCommandBase
  {
    private readonly ConnectionActionCommand cmd = new ConnectionActionCommand();

    protected override void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = cmd.CanExecute(ConnectionActionCommand.DisconnectParameter);
    }


    protected override void OnExecute(object sender, ExecutedRoutedEventArgs e)
    {
      cmd.Execute(ConnectionActionCommand.DisconnectParameter);
    }
  }
}