using System;
using System.Windows.Input;
using System.Windows.Markup;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.ViewModel;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Basic implementation of the <see cref="ICommand"/>
  /// interface along with a few helper methods and
  /// properties.
  /// </summary>
  public abstract class CommandBase : ICommand
  {


    /// <summary>
    /// Occurs when changes occur that affect whether
    /// or not the command should execute.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
      add { CommandManager.RequerySuggested += value; }
      remove { CommandManager.RequerySuggested -= value; }
    }
    

    ///<summary>
    /// Gets the application controller instance.
    ///</summary>
    public AppController Controller
    {
      get { return AppController.Instance; }
    }


    /// <summary>
    /// Checks whether the <see cref="AppController.ActiveShare"/> is available
    /// and connected.
    /// </summary>
    /// <returns>True if the <see cref="AppController.ActiveShare"/> property
    /// is set, and the share's <see cref="Share.IsConnected"/> property is true.
    /// </returns>
    protected bool IsActiveShareConnected()
    {
      return Controller.ActiveShare != null && Controller.ActiveShare.IsConnected;
    }



    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.
    /// If the command does not require data to be passed,
    /// this object can be set to null.
    /// </param>
    public abstract void Execute(object parameter);


    /// <summary>
    /// Defines the method that determines whether the command
    /// can execute in its current state.
    /// </summary>
    /// <returns>
    /// This default implementation always returns true.
    /// </returns>
    /// <param name="parameter">Data used by the command.  
    /// If the command does not require data to be passed,
    /// this object can be set to null.
    /// </param>
    public virtual bool CanExecute(object parameter)
    {
      return true;
    }

  }
}