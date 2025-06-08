using LoreViewer.LoreElements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using static System.Collections.Specialized.BitVector32;

namespace LoreViewer.Settings.Interfaces
{
  public interface IFieldDefinitionContainer
  {
    List<LoreFieldDefinition> fields { get; }
    public bool HasFields => fields != null && fields.Count > 0;

    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));

    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

  }

  public interface ISectionDefinitionContainer
  {
    List<LoreSectionDefinition> sections { get; }
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
  }

  public interface ICollectionDefinitionContainer
  {
    List<LoreCollectionDefinition> collections { get; }
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => collectionName == col.name);
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
  }

  public interface IEmbeddedNodeDefinitionContainer
  {
    List<LoreEmbeddedNodeDefinition> embeddedNodeDefs { get; }
    public bool HasTypeDefinition(string typeName) => embeddedNodeDefs.Any(t => typeName == t.name);
    public bool HasTypeDefinition(LoreTypeDefinition typeDef) => embeddedNodeDefs.Any(t => t.nodeType.IsParentOf(typeDef));

    // Check if the TYPE, regardless of title, is allowed as an embedded node.
    public bool IsAllowedEmbeddedType(LoreTypeDefinition typeDefinition) => embeddedNodeDefs.Any(t => t.nodeType == typeDefinition || t.nodeType.IsParentOf(typeDefinition));

    // If the node type we found is the same or extends the LoreEmbeddedNodeDefinition's node type definition, it is allowed.
    // If the LoreEmbeddedNodeDefinition does not have a title defined, it can have any title. Otherwise, title must match.
    public bool IsAllowedEmbeddedNode(LoreTypeDefinition typeDef, string nodeTitle) => embeddedNodeDefs.Any(
          t =>
          (t.nodeType == typeDef || t.nodeType.IsParentOf(typeDef)) &&
          (!string.IsNullOrWhiteSpace(t.name) ? t.name == nodeTitle : true));
  }

  public interface IRequirable
  {
    bool required { get; set; }
  }
}
