using System;
using Hardcodet.NetDrives.Properties;

namespace Hardcodet.NetDrives.Util
{
  internal static class Loc
  {
    public static string T(string key)
    {
      return Resources.ResourceManager.GetString(key, Resources.Culture) ?? key;
    }

    public static string F(string key, params object[] args)
    {
      return String.Format(T(key), args);
    }
  }
}