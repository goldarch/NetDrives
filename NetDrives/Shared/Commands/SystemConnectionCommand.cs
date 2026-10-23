using System.Windows.Input;
using Hardcodet.NetDrives.Platform.Core.Monitoring;
using Microsoft.Windows.Controls;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Base class for commands that deal with unmanaged
  /// system connections.
  /// </summary>
  public abstract class SystemConnectionCommand<T> : CommandExtension<T> where T : class, ICommand, new()
  {
    /// <summary>
    /// Tries to resolve the underlying
    /// <see cref="NetworkConnectionInfo"/> of a given command parameter.
    /// </summary>
    /// <returns>The underlying <see cref="NetworkConnectionInfo"/>, if available,
    /// otherwise null.</returns>
    protected NetworkConnectionInfo TryGetConnectionInfo(object commandParameter)
    {
      DataGridRow row = (DataGridRow) commandParameter;
      if (row == null) return null;

      return row.Item as NetworkConnectionInfo;
    }


    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <returns>
    /// true if this command can be executed; otherwise, false.
    /// </returns>
    /// <param name="parameter">
    /// Data used by the command.  If the command does not require data to be passed,
    /// this object can be set to null.
    /// </param>
    public override bool CanExecute(object parameter)
    {
      return true;
    }
  }
}