using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hardcodet.Commons;
using Hardcodet.NetDrives.Platform.Interop;

namespace Hardcodet.NetDrives.Platform.Core.Shares
{
  /// <summary>
  /// Encapsulates status information for a given
  /// <see cref="ShareConfiguration"/>.
  /// </summary>
  public class ShareStatus : SimpleObject
  {

    #region ConnectionStatus

    /// <summary>
    /// Indicates the current connection status.
    /// </summary>
    private ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;


    /// <summary>
    /// Indicates the current connection status.
    /// </summary>
    public ConnectionStatus ConnectionStatus
    {
      get { return connectionStatus; }
      set
      {
        //ignore if values are equal
        if (value == connectionStatus) return;

        connectionStatus = value;
        OnPropertyChanged("ConnectionStatus");
      }
    }

    #endregion



    #region ConnectionResult

    /// <summary>
    /// The status that was returned for the latest connection
    /// </summary>
    private ConnectResult? connectionResult;


    /// <summary>
    /// The status that was returned for the latest connection.
    /// Defaults to null, which is also returned if the share
    /// is correctly disconnected.
    /// </summary>
    public ConnectResult? ConnectionResult
    {
      get { return connectionResult; }
      set
      {
        //ignore if values are equal
        if (value == connectionResult) return;

        connectionResult = value;
        OnPropertyChanged("ConnectionResult");
      }
    }

    #endregion

    /// <summary>
    /// An error message that provides background information,
    /// if required.
    /// </summary>
    public string Error { get; set; }



  }
}
