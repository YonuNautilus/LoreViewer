using LoreViewer.Domain.Settings.Definitions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Domain.Entities
{
  /// <summary>
  /// Interface for LoreElements that can contain field definitions/attributes.
  /// </summary>
  public interface IAttributeContainer
  {
    List<LoreAttribute> Attributes { get; }
    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);
    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
    public virtual bool HasAttributes => Attributes.Any();
  }


  /// <summary>
  /// Interface for LoreElements that can contain sections.
  /// </summary>
  public interface ISectionContainer
  {
    List<LoreSection> Sections { get; }
    public bool HasSection(string sectionName) => Sections.Any(s => s.Name == sectionName);
    public LoreSection? GetSection(string sectionName) => Sections.FirstOrDefault(s => s.Name == sectionName);
    public bool HasSections => Sections.Any();
  }


  /// <summary>
  /// Interface for LoreElements that can contain collections.
  /// </summary>
  public interface ICollectionContainer
  {
    List<LoreCollection> Collections { get; }
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfDefinedName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionWithDefinedName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
  }


  /// <summary>
  /// Interface for LoreElements that can contain child nodes.
  /// </summary>
  public interface INodeContainer
  {
    List<LoreNode> Nodes { get; }
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    public bool HasNodes => Nodes.Any();

  }

  public interface IEmbeddedNodeContainer : INodeContainer
  {

    // Check if the embedded node already exists.
    public bool ContainsEmbeddedNode(LoreTypeDefinition embeddedNodeType, string embeddedNodeTitle);
  }
}
