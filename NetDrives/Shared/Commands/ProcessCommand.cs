using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Hardcodet.NetDrives.Platform.Interop;
using Hardcodet.NetDrives.Properties;
using Hardcodet.NetDrives.Util;

namespace Hardcodet.NetDrives.Shared.Commands
{
  /// <summary>
  /// Command that executes an external process.
  /// </summary>
  public class ProcessCommand : CommandExtension<ProcessCommand>
  {
    public const string OpenApplicationLogParameter = "ShowApplicationLog";
    public const string ListOnConsoleParameter = "ConsoleListConnections";
    public const string ClearViaConsoleParameter = "ConsoleClearConnections";

    public override void Execute(object parameter)
    {
      string action = parameter as string;
      if (String.IsNullOrEmpty(action)) return;

      switch (action)
      {
        case OpenApplicationLogParameter:
          string logFile = AppUtil.LogFile;
          if (File.Exists(logFile))
          {
            Process.Start(logFile);
          }
          else
          {
            Dialogs.ShowInformation(Resources.Information_That_No_Log_File_Is_Available);
          }
          break;
        case ListOnConsoleParameter:
          NetworkUtil.ShowSystemConnectionsOnConsole();
          break;
        case ClearViaConsoleParameter:
          NetworkUtil.ClearSystemConnections();
          break;
      }
    }

  }
}