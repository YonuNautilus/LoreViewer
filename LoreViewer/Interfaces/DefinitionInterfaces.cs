using SharpYaml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace LoreViewer.Settings.Interfaces
{
  public interface IFieldDefinitionContainer
  {
    List<LoreFieldDefinition> fields { get; set; }

    public bool HasFields => fields != null && fields.Count > 0;

    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));

    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    public void AddField(LoreFieldDefinition newFieldDef)
    {
      if (fields == null) fields = new List<LoreFieldDefinition>();
      fields.Add(newFieldDef);
    }

  }

  public interface ISectionDefinitionContainer
  {
    List<LoreSectionDefinition> sections { get; set; }

    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);

    public void AddSection(LoreSectionDefinition newSectionDef)
    {
      if (sections == null) sections = new List<LoreSectionDefinition>();
      sections.Add(newSectionDef);
    }
  }

  public interface ICollectionDefinitionContainer
  {
    List<LoreCollectionDefinition> collections { get; set; }
    public bool HasCollections => collections != null && collections.Count > 0;
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => collectionName == col.name);
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);

    public void AddCollection(LoreCollectionDefinition newCollectionDef)
    {
      if (collections == null) collections = new List<LoreCollectionDefinition>();
      collections.Add(newCollectionDef);
    }
  }

  public interface IEmbeddedNodeDefinitionContainer
  {
    List<LoreEmbeddedNodeDefinition> embeddedNodeDefs { get; set; }
    public bool HasTypeDefinition(string typeName) => embeddedNodeDefs.Any(t => typeName == t.name);
    public bool HasTypeDefinition(LoreTypeDefinition typeDef) => embeddedNodeDefs.Any(t => t.nodeType.IsParentOf(typeDef));

    public virtual bool HasNestedNodes => embeddedNodeDefs != null && embeddedNodeDefs.Count() > 0;

    // Check if the TYPE, regardless of title, is allowed as an embedded node.
    public bool IsAllowedEmbeddedType(LoreTypeDefinition typeDefinition) => embeddedNodeDefs.Any(t => t.nodeType == typeDefinition || t.nodeType.IsParentOf(typeDefinition));

    // If the node type we found is the same or extends the LoreEmbeddedNodeDefinition's node type definition, it is allowed.
    // If the LoreEmbeddedNodeDefinition does not have a title defined, it can have any title. Otherwise, title must match.
    public bool IsAllowedEmbeddedNode(LoreTypeDefinition typeDef, string nodeTitle) => embeddedNodeDefs.Any(
          t =>
          (t.nodeType == typeDef || t.nodeType.IsParentOf(typeDef)) &&
          (!string.IsNullOrWhiteSpace(t.name) ? t.name == nodeTitle : true));

    public void AddEmbedded(LoreEmbeddedNodeDefinition newEmbeddedDef)
    {
      if (embeddedNodeDefs == null) embeddedNodeDefs = new List<LoreEmbeddedNodeDefinition>();
      embeddedNodeDefs.Add(newEmbeddedDef);
    }
  }

  public interface IPicklistDefinitionContainer
  {
    List<LorePicklistDefinition> options { get; set; }

    public bool HasPicklistDefinition(string listItemName) => options.Any(t => listItemName == t.name);

    public bool HasOptions => options != null && options.Count() > 0;

    public void AddPicklistDefinition(LorePicklistDefinition picklistDefinition)
    {
      if (options == null) options = new List<LorePicklistDefinition>();
      if (!options.Contains(picklistDefinition)) options.Add(picklistDefinition);
    }
  }

  public interface IRequirable
  {
    [YamlMember(-90)]
    [DefaultValue(false)]
    bool required { get; set; }
  }

  public interface IDeepCopyable<T>
  {
    T Clone();

    T CloneFromBase();
  }
}
