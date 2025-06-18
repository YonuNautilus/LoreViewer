using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoreViewer.Settings
{
  /// <summary>
  /// Represents the Lore's schema with all references hooked up (the post-processed raw settings deserialized directly from YAML)
  /// </summary>
  public class LoreSettings : IDeepCopyable<LoreSettings>
  {
    const string LoreSettingsFileName = "Lore_Settings.yaml";

    [YamlIgnore]
    public string CurrentYAML
    {
      get
      {
        LoreSettings settingsToSerialize = this;

        var serializer = new Serializer(
            new SerializerSettings{ EmitDefaultValues = false, IgnoreNulls = true, EmitAlias = false
            }
          );

        if (settings.EnableSerializationPruning)
        {
          settingsToSerialize = this.Clone();
          settingsToSerialize.PostProcess();
          settingsToSerialize.PruneForSerialization();
        }

        return serializer.Serialize(settingsToSerialize);
      }
    }

    private void PruneForSerialization()
    {
      LorePruner.Prune(this);
    }

    [YamlIgnore]
    public string OriginalYAML { get; private set; }

    [YamlMember(0)]
    public List<LoreTypeDefinition> types = new List<LoreTypeDefinition>();
    [YamlMember(1)]
    public List<LoreCollectionDefinition> collections = new List<LoreCollectionDefinition>();

    [YamlIgnore]
    public string FolderPath { get; set; }

    private bool m_bHadFatalError;

    [YamlIgnore]
    public bool HadFatalError { get => m_bHadFatalError; }

    [YamlMember(1000)]
    public AppSettings settings { get; set; }

    public LoreSettings()
    {

    }

    public bool HasTypeDefinition(string typeName) => types.Any(type => type.name.Equals(typeName));
    public LoreTypeDefinition GetTypeDefinition(string typeName) => types.FirstOrDefault(type => type.name.Equals(typeName));
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => col.name.Equals(collectionName));
    public LoreCollectionDefinition GetCollectionDefinition(string typeName) => collections.FirstOrDefault(type => type.name.Equals(typeName));



    public static LoreSettings ParseSettingsFromFolder(string folderPath)
    {
      string fullSettingsPath = Path.Combine(folderPath, LoreSettingsFileName);
      if (!File.Exists(fullSettingsPath))
        throw new Exception($"Did not find file {fullSettingsPath}");


      var deserializer = new Serializer();

      string settingsText = File.ReadAllText(fullSettingsPath);


      LoreSettings newSettings = deserializer.Deserialize<LoreSettings>(settingsText);
      newSettings.FolderPath = folderPath;
      newSettings.OriginalYAML = settingsText;

      newSettings.PostProcess();

      return newSettings;
    }


    public void PostProcess()
    {
      CheckDuplicateNames();

      ResolveTypeInheritance();

      ResolveReferencedTypes();
    }

    private void CheckDuplicateNames()
    {
      IEnumerable<LoreDefinitionBase> defsWithSameName = types.Concat<LoreDefinitionBase>(collections).GroupBy(d => d.name).Where(group => group.Count() > 1).SelectMany(def => def);
      if (defsWithSameName.Any())
      {
        throw new DuplicateDefinitionNamesException(defsWithSameName.First());
      }
    }

    private void ResolveTypeInheritance()
    {
      List<LoreTypeDefinition> typesInheritanceOrder = InheritanceOrderedTypeDefinition(types);

      foreach (LoreTypeDefinition ltd in typesInheritanceOrder)
      {
        if (!string.IsNullOrWhiteSpace(ltd.extends))
        {
          LoreTypeDefinition parentType = GetTypeDefinition(ltd.extends);
          ltd.MergeFrom(parentType);
        }
      }
    }

    private void ResolveReferencedTypes()
    {
      foreach (LoreTypeDefinition ltd in types)
        if (!ltd.processed)
          ltd.PostProcess(this);

      foreach (LoreCollectionDefinition lcd in collections)
        lcd.PostProcess(this);
    }

    /// <summary>
    /// Resolving inheritance is no laughing matter. We must ensure resolving inheritance is done in the correct order.
    /// This method takes all embeddedNodeDefs, and gives a list of the LoreTypeDefinitions in an order that will not break inheritance
    /// when inheritance resolution occurs.
    /// </summary>
    /// <param name="allTypes"></param>
    /// <returns></returns>
    private List<LoreTypeDefinition> InheritanceOrderedTypeDefinition(List<LoreTypeDefinition> allTypes)
    {
      List<LoreTypeDefinition> res = new List<LoreTypeDefinition>();
      HashSet<string> visited = new HashSet<string>();
      HashSet<string> visiting = new HashSet<string>();
      Dictionary<string, LoreTypeDefinition> typesMapping = allTypes.ToDictionary(ltd => ltd.name, ltd => ltd);

      void visit(string typeName)
      {

        if (!typesMapping.ContainsKey(typeName))
          throw new InheritingMissingTypeDefinitionException(typeName);

        LoreTypeDefinition type = typesMapping[typeName];

        if (visited.Contains(typeName)) return;
        if (visiting.Contains(typeName))
          throw new CyclicalInheritanceException(type);

        visiting.Add(typeName);

        if (!string.IsNullOrWhiteSpace(type.extends))
          visit(type.extends);

        visiting.Remove(typeName);
        visited.Add(typeName);
        res.Add(type);
      }

      foreach (LoreTypeDefinition type in allTypes)
        visit(type.name);

      return res;
    }

    public LoreSettings Clone()
    {
      LoreSettings newSettings = new();

      newSettings.types = this.types.Select(t => t.Clone()).ToList();  
      newSettings.collections = this.collections.Select(c => c.Clone()).ToList();

      newSettings.settings = this.settings.Clone();

      newSettings.OriginalYAML = this.OriginalYAML;

      return newSettings;
    }

    public LoreSettings CloneFromParent()
    {
      return Clone();
    }
  }
  public class AppSettings : IDeepCopyable<AppSettings>
  {
    [YamlMember(0)]
    public bool ignoreCase { get; set; }
    [YamlMember(1)]
    public bool softLinking { get; set; } = false;
    [YamlMember(2)]
    public string defaultSort { get; set; } = string.Empty;
    [YamlMember(3)]
    public bool EnableSerializationPruning { get; set; } = true;
    [YamlMember(4)]
    public List<string> markdownExtensions { get; set; } = new List<string>();
    [YamlMember(5)]
    public List<string> blockedPaths { get; set; } = new List<string>();

    public AppSettings Clone()
    {
      AppSettings newSettings = MemberwiseClone() as AppSettings;

      newSettings.markdownExtensions = markdownExtensions.Select(t => t).ToList();
      newSettings.blockedPaths = blockedPaths.Select(t => t).ToList();

      return newSettings;
    }

    public AppSettings CloneFromParent()
    {
      return Clone();
    }
  }
}
