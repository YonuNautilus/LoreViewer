using Avalonia.Media;
using LoreViewer.Exceptions;
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
      ResolveTypeInheritance();

      ResolveReferencedTypes();
    }

    private void ResolveTypeInheritance()
    {
      List<LoreTypeDefinition> typesInheritanceOrder = InheritanceOrderedTypeDefinition(types);

      foreach (LoreTypeDefinition ltd in typesInheritanceOrder)
      {
        if (!string.IsNullOrWhiteSpace(ltd.extends))
        {
          LoreTypeDefinition parentType = GetTypeDefinition(ltd.extends);
          ltd.SetParent(parentType);
        }
      }
    }

    private void ResolveReferencedTypes()
    {
      foreach (LoreTypeDefinition ltd in types)
        if(!ltd.processed)
          ltd.PostProcess(this);

      foreach (LoreCollectionDefinition lcd in collections)
        lcd.PostProcess(this);
    }

    /// <summary>
    /// Resolving inheritance is no laughing matter. We must ensure resolving inheritance is done in the correct order.
    /// This method takes all types, and gives a list of the LoreTypeDefinitions in an order that will not break inheritance
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

      foreach(LoreTypeDefinition type in allTypes)
        visit(type.name);

      return res;
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
