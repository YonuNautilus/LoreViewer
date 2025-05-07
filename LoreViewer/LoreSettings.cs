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

  /// <summary>
  /// The top-level of a lore definition. Can be used to define characters, organizations, races, etc.
  /// </summary>
  public class LoreTypeDefinition
  {
    private List<string> RelevantFilePaths = new List<string>();

    /// <summary>
    /// Collection of field definitions that should be visible at the top of the markdown on a declared object of a type.
    /// For example, a character will have a field for name, a field for race, etc.
    /// </summary>
    public List<LoreFieldDefinition> fields { get; set; } = new List<LoreFieldDefinition>();

    /// <summary>
    /// A collection of sections in an object's file (or section). These will contain more infomation, usually paragraphs,
    /// that expand upon base level fields, or add new information. Sections can contain their own fields and subsections.
    /// </summary>
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition
  {
    public string name { get; set; }
    public string type { get; set; }

    public bool freeform { get; set; } = false;

    public List<LoreFieldDefinition> fields { get; set; }
    public List<LoreSectionDefinition> sections { get; set; }
  }

  public class LoreFieldDefinition
  {
    public string name { get; set; } = string.Empty;
    public string style { get; set; } = string.Empty; // bullet_point, bullet_list, heading_paragraph

    public bool required = false;

    public List<LoreFieldDefinition> NestedFields { get; set; } // for fields like Date with Start/End
  }

  public class LoreCollectionDefinition
  {
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
