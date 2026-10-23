using System;
using System.Windows.Input;
using Hardcodet.Commons.Events;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.ViewModel;
using Microsoft.Windows.Controls.Ribbon;

namespace Hardcodet.NetDrives.Shared.Commands.RibbonCommands
{

  ///<summary>
  /// Base class for custom <see cref="RibbonCommand"/>
  /// implementations. Implements weak event listeners for
  /// <see cref="RibbonCommand.Executed"/> and
  /// <see cref="RibbonCommand.CanExecute"/> events.
  ///</summary>
  public abstract class RibbonCommandBase : RibbonCommand
  {
    //we need to keep event handler explicitly in order to prevent them
    //from being garbage collected
    private readonly EventHandler<ExecutedRoutedEventArgs> executedHandler;
    private readonly EventHandler<CanExecuteRoutedEventArgs> canExecuteHandler;


    /// <summary>
    /// Weak event proxy for the <see cref="RibbonCommand.Executed"/>
    /// event.
    /// </summary>
    private readonly WeakEventProxy<ExecutedRoutedEventArgs> executedEventProxy;

    /// <summary>
    /// Weak event proxy for the <see cref="RibbonCommand.CanExecute"/>
    /// event.
    /// </summary>
    private readonly WeakEventProxy<CanExecuteRoutedEventArgs> canExecuteEventProxy;


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
    /// Creates the command and registers weak event listeners.
    /// </summary>
    protected RibbonCommandBase()
    {
      executedHandler = OnExecute;
      executedEventProxy = new WeakEventProxy<ExecutedRoutedEventArgs>(executedHandler);
      Executed += executedEventProxy.Handler;

      canExecuteHandler = OnCanExecute;
      canExecuteEventProxy = new WeakEventProxy<CanExecuteRoutedEventArgs>(canExecuteHandler);
      CanExecute += canExecuteEventProxy.Handler;
    }


    /// <summary>
    /// Determines whether the command can be executed or not.
    /// The default implementation always allows that.
    /// </summary>
    protected virtual void OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
      e.CanExecute = true;
    }


    /// <summary>
    /// Command implementation - executes command logic.
    /// </summary>
    protected abstract void OnExecute(object sender, ExecutedRoutedEventArgs e);
  }

}
