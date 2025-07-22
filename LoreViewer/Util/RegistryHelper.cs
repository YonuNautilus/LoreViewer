using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Util
{
  internal class RegistryHelper
  {
    const string REGPATH = @"Software\NautiluStudios\LoreViewer";
    const string RECENT_LORE_KEY = "RecentProjects";

    public static List<string> GetRecentProjects()
    {
      using var key = Registry.CurrentUser.OpenSubKey(REGPATH);
      var value = key?.GetValue(RECENT_LORE_KEY) as string;

      if (string.IsNullOrWhiteSpace(value))
        return new List<string>();

      return value.Split('|', StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
    }

    public static void SaveRecentProject(string path)
    {
      if(Registry.CurrentUser.OpenSubKey(REGPATH) == null)
        Registry.CurrentUser.CreateSubKey(REGPATH);

      using var key = Registry.CurrentUser.OpenSubKey(REGPATH, true);

      List<string> current = GetRecentProjects();
      if (current == null || current.Count() == 0)
      {
        key?.SetValue(RECENT_LORE_KEY, path, RegistryValueKind.String);
      }
      else
      {
        // If path already exists, put it at the front of the list so it appears first (it is the MOST recent)
        if (current.Contains(path))
        {
          current.Remove(path);
          current.Insert(0, path);
        }
        else
        {
          // If the key already has 10 paths in it, remove the last, put this most recent at the front
          if (current.Count() >= 10)
          {
            current.RemoveAt(current.Count() - 1);
          }
          // If MRU list has not reached 10 (or the last was removed because there were already 10 paths in it),
          // put this path we are inserting at the front.
          current.Insert(0, path);
        }

        key?.SetValue(RECENT_LORE_KEY, string.Join('|', current));
      }
    }
  }
}
