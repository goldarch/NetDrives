using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Hardcodet.NetDrives.Util
{
    /// <summary>
    /// Taken from: http://simpcode.blogspot.com/2008/07/c-set-and-unset-auto-start-for-windows.html
    /// </summary>
    public static class AutoStart
    {
        /// <summary>
        /// Command line argument that signals the application that
        /// it should start minimized.
        /// </summary>
        public const string MinimizedArgument = "/minimized";

        /// <summary>
        /// Application registry key.
        /// </summary>
        public const string NetDrivesKey = "NetDrives";

        /// <summary>
        /// The registery path that hosts automatically started applications
        /// for the current user.
        /// </summary>
        public const string AutoStartKey = @"Software\Microsoft\Windows\CurrentVersion\Run";


        /// <summary>
        /// Checks whether the auto-start option is enabled.
        /// </summary>
        public static bool IsAutoStartEnabled()
        {
            return File.Exists(GetShortcutPath());
        }


        /// <summary>
        /// Migrates a legacy registry (Run key) entry to a Startup folder shortcut,
        /// and refreshes an existing shortcut if the executable location changed.
        /// </summary>
        public static void RepairAutoStart()
        {
            try
            {
                bool hadRegistryEntry;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(AutoStartKey))
                {
                    hadRegistryEntry = key != null && key.GetValue(NetDrivesKey) != null;
                }

                if (hadRegistryEntry || File.Exists(GetShortcutPath()))
                {
                    SetAutoStart(true);
                }
            }
            catch
            {
            }
        }


        /// <summary>
        /// Enables / disables auto start.
        /// </summary>
        /// <param name="isEnabled">Set true to enable auto start.</param>
        public static void SetAutoStart(bool isEnabled)
        {
            // always remove the legacy registry entry so the app is not started twice
            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(AutoStartKey))
            {
                key.DeleteValue(NetDrivesKey, false);
            }

            string shortcut = GetShortcutPath();
            if (!isEnabled)
            {
                if (File.Exists(shortcut)) File.Delete(shortcut);
                return;
            }

            // the Startup folder is processed independently of the Run key,
            // so a broken entry of another program cannot prevent the launch
            //保存于用户目录：C:\Users\goldarch\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup
            string exe = GetExePath();
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            object shell = Activator.CreateInstance(shellType);
            object link = shellType.InvokeMember("CreateShortcut", BindingFlags.InvokeMethod, null, shell,
                new object[] { shortcut });
            Type linkType = link.GetType();
            linkType.InvokeMember("TargetPath", BindingFlags.SetProperty, null, link, new object[] { exe });
            linkType.InvokeMember("Arguments", BindingFlags.SetProperty, null, link,
                new object[] { MinimizedArgument });
            linkType.InvokeMember("WorkingDirectory", BindingFlags.SetProperty, null, link,
                new object[] { Path.GetDirectoryName(exe) });
            linkType.InvokeMember("Save", BindingFlags.InvokeMethod, null, link, null);
        }


        private static string GetShortcutPath()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            return Path.Combine(folder, NetDrivesKey + ".lnk");
        }


        private static string GetExePath()
        {
            return Assembly.GetEntryAssembly().Location;
        }
    }
}