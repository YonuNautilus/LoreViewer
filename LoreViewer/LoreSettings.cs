using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.RepresentationModel;

namespace LoreViewer
{
  internal class LoreSettings
  {
    private Dictionary<string, LoreTypeDefinition> _types = new Dictionary<string, LoreTypeDefinition>();
    private Dictionary<string, LoreCollectionDefinition> _collections = new Dictionary<string, LoreCollectionDefinition>();
    public AppSettings Settings { get; set; }

    public LoreSettings()
    {
      Settings = new AppSettings();
    }

    public bool HasTypeDefinition(string type) => _types.ContainsKey(type);
    public LoreTypeDefinition GetTypeDefinition(string type) => _types[type];
    public LoreCollectionDefinition GetCollectionDefinition(string type) => _collections[type];

    public void ParseSettingsFromFile(string settingsYamlPath)
    {
      string settingsFileContent = File.ReadAllText(settingsYamlPath);

      YamlStream stream = new YamlStream();
      stream.Load(new StringReader(settingsFileContent));

      YamlMappingNode docRoot = (YamlMappingNode)stream.Documents[0].RootNode;

      YamlMappingNode types = (YamlMappingNode)docRoot.Children[new YamlScalarNode("types")];
      ParseTypes(types);
    }

    private void ParseTypes(YamlMappingNode typesNode)
    {
      foreach (var typeEntry in typesNode.Children)
      {
        string typeName = typeEntry.Key.ToString();

        LoreTypeDefinition newTypeDef = new LoreTypeDefinition();

        var definitions = (YamlMappingNode)typeEntry.Value;

        if (definitions["fields"] != null)
        {
          var fieldsNode = (YamlMappingNode)definitions["fields"];

          foreach (var fieldDefinition in fieldsNode.Children)
          {
            LoreFieldDefinition newFieldDef = new LoreFieldDefinition() { Name = fieldDefinition.Key.ToString() };
            var definitionValues = (YamlMappingNode)fieldDefinition.Value;

            newFieldDef.Style = definitionValues.Children["style"].ToString();
            newFieldDef.Required = definitionValues.Children.TryGetValue(new YamlScalarNode("required"), out var r) && bool.Parse(r.ToString());

            newTypeDef.Fields.Add(newFieldDef);
          }
        }
        _types.Add(typeName, newTypeDef);
      }
    }
  }

  public class LoreTypeDefinition
  {
    private List<string> RelevantFilePaths = new List<string>();
    public List<LoreFieldDefinition> Fields { get; set; } = new List<LoreFieldDefinition>();
  }

  public class LoreFieldDefinition
  {
    public string Name { get; set; } = string.Empty;

    public string Style { get; set; } = string.Empty; // bullet_point, bullet_list, heading_paragraph

    public bool Required = false;
  }

  public class LoreCollectionDefinition
  {
    public string File { get; set; } = string.Empty;

    public string EntryType { get; set;} = string.Empty;
    public string EntryStyle { get; set; } = string.Empty;
    public bool SortEntries { get; set; }
  }

  public class AppSettings
  {
    public bool IgnoreCase { get; set; }
    public bool SoftLinking { get; set; }
    public string DefaultSort { get; set; } = string.Empty;
    public List<string> MarkdownExtensions { get; set; } = new List<string>();
  }
}
