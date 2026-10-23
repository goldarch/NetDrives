using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.Shared.Commands
{
  ///<summary>
  /// 
  ///</summary>
  public class RemoveNetLocationCommand : CommandExtension<RemoveNetLocationCommand>
  {

    public override void Execute(object parameter)
    {
      var configuration = Controller.ActiveShare.Configuration;
      string msg = Resources.Ask_Whether_Configuration_Should_Be_Removed;
      msg = String.Format(msg, configuration.Name);

      if (Dialogs.ShowYesNo(msg, MessageBoxImage.Warning) == MessageBoxResult.Yes)
      {
        Controller.ShareManager.RemoveShare(configuration);
      }
    }


    public override bool CanExecute(object parameter)
    {
      return Controller.ActiveShare != null;
    }
  }
}