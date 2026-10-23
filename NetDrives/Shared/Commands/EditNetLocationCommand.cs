using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.UI;
using Hardcodet.NetDrives.ViewModel.Connections;

namespace Hardcodet.NetDrives.Shared.Commands
{
  ///<summary>
  /// Displays the configuration dialog for the currently active
  /// network share.
  ///</summary>
  public class EditNetLocationCommand : CommandExtension<EditNetLocationCommand>
  {

    public override void Execute(object parameter)
    {
      Share share = Controller.ActiveShare;
      var status = ShareUtil.ConfigureShare(share);

      //suggest auto-start
      if (status) ShareUtil.CheckAndSuggestAutoStart(share.Configuration);
    }


    public override bool CanExecute(object parameter)
    {
      return Controller.ActiveShare != null;
    }
  }
}