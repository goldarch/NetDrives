using Hardcodet.NetDrives.ViewModel.Connections;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Closes a given unmanaged connection.
  /// </summary>
  public class CloseUnmanagedConnectionCommand : SystemConnectionCommand<CloseUnmanagedConnectionCommand>
  {
    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command.  If the command does not require
    /// data to be passed, this object can be set to null.
    /// </param>
    public override void Execute(object parameter)
    {
      var connectionInfo = TryGetConnectionInfo(parameter);
      if (connectionInfo == null) return;

      ConnectionUtil.Disconnect(connectionInfo, false);
    }
  }
}