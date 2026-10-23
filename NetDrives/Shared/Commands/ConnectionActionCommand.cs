using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Core.Shares;
using Hardcodet.NetDrives.ViewModel.Connections;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Connects, disconnects or browses a given network location.
  /// </summary>
  public class ConnectionActionCommand : CommandExtension<ConnectionActionCommand>
  {

    public const string ConnectParameter = "Connect";
    public const string DisconnectParameter = "Disconnect";
    public const string BrowseParameter = "Browse";


    public override void Execute(object parameter)
    {
      string action = parameter as string;
      if (String.IsNullOrEmpty(action)) return;

      switch(action)
      {
        case ConnectParameter:
          ConnectionUtil.Connect(Controller.ActiveShare);
          break;
        case DisconnectParameter:
          var share = Controller.ActiveShare;
          ConnectionUtil.Disconnect(share, false);
          break;
        case BrowseParameter:
          ConnectionUtil.BrowseShare(Controller.ActiveShare);
          break;
        default:
          break;
      }

      
    }


    public override bool CanExecute(object parameter)
    {
      string action = parameter as string;
      if (String.IsNullOrEmpty(action)) return false;

      switch (action)
      {
        case ConnectParameter:
          return Controller.ActiveShare != null && !Controller.ActiveShare.IsConnected;
        case DisconnectParameter:
          return Controller.ActiveShare != null &&
                     Controller.ActiveShare.Status.Is(ConnectionStatus.Connected,
                                                      ConnectionStatus.Degraded,
                                                      ConnectionStatus.ConnectionError);
        case BrowseParameter:
          return IsActiveShareConnected();
        default:
          return false;
      }

      
    }

  }
}
