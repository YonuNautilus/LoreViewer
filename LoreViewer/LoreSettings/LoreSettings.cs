﻿using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Settings
{
  /// <summary>
  /// Represents the Lore's schema with all references hooked up (the post-processed raw settings deserialized directly from YAML)
  /// </summary>
  public class LoreSettings
  {
    public List<LoreTypeDefinition> types = new List<LoreTypeDefinition>();
    public List<LoreCollectionDefinition> collections = new List<LoreCollectionDefinition>();
    public AppSettings Settings { get; set; }

    public LoreSettings()
    {

    }

    public bool HasTypeDefinition(string typeName) => types.Any(type => type.name.Equals(typeName));
    public LoreTypeDefinition GetTypeDefinition(string typeName) => types.FirstOrDefault(type => type.name.Equals(typeName));
    public LoreCollectionDefinition GetCollectionDefinition(string typeName) => collections.FirstOrDefault(type => type.name.Equals(typeName));


    public void PostProcess()
    {
      foreach (LoreTypeDefinition ltd in types)
        ltd.PostProcess(this);
      foreach (LoreCollectionDefinition lcd in collections)
        lcd.PostProcess(this);
    }
  }
  public class AppSettings
  {
    public bool ignoreCase = false;
    public bool softLinking { get; set; } = false;
    public string defaultSort { get; set; } = string.Empty;
    public List<string> markdownExtensions { get; set; } = new List<string>();
    public List<string> blockedPaths { get; set; } = new List<string>();
  }
}
