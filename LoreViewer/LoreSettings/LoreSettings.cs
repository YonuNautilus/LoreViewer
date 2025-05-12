using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace LoreViewer.Settings
{
  public class LoreSettings
  {
    public Dictionary<string, LoreTypeDefinition> types = new Dictionary<string, LoreTypeDefinition>();
    public Dictionary<string, LoreCollectionDefinition> collections = new Dictionary<string, LoreCollectionDefinition>();
    public AppSettings Settings { get; set; }

    public LoreSettings()
    {
      Settings = new AppSettings();
    }

    public bool HasTypeDefinition(string type) => types.ContainsKey(type);
    public LoreTypeDefinition GetTypeDefinition(string type) => types[type];
    public LoreCollectionDefinition GetCollectionDefinition(string type) => collections[type];

    public string GetTypeName(LoreTypeDefinition typeDef) => types.FirstOrDefault(kvp => kvp.Value == typeDef).Key;

  }
  public class AppSettings
  {
    public bool IgnoreCase { get; set; }
    public bool SoftLinking { get; set; }
    public string DefaultSort { get; set; } = string.Empty;
    public List<string> MarkdownExtensions { get; set; } = new List<string>();
  }
}
