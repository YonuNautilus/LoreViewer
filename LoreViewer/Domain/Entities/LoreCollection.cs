using LoreViewer.Domain.Settings.Definitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Domain.Entities
{
  /// <summary>
  /// An Collection represents a set of Nodes of the same type definition.
  /// <para/>
  /// Supports narrative/descriptive text.
  /// <para/>
  /// Definition Type: LoreCollectionDefinition
  /// <br/>
  /// <br/>
  /// <c>LoreCollection</c> can contain:
  /// <list type="bullet">
  /// <item>Collections</item>
  /// <item>Nodes</item>
  /// </list>
  /// </summary>
  public class LoreCollection : LoreNarrativeElement, INodeContainer, ICollectionContainer
  {
    public override LoreDefinitionBase Definition { get; set; }
    #region INodeContainer Implementation
    public List<LoreNode> Nodes { get; set; } = new List<LoreNode>();
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion
    #region INodeCollectionContainer Implementation
    public List<LoreCollection> Collections { get; }
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfDefinedName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionWithDefinedName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
    #endregion

    public LoreCollection(string name, LoreDefinitionBase colType) : base(name, colType)
    {
      switch (colType)
      {
        case LoreTypeDefinition ltd:
          Nodes = new List<LoreNode>();
          break;
        case LoreCollectionDefinition lcd:
          Collections = new List<LoreCollection>();
          break;
        default:
          break;
      }
    }

    public LoreCollection(string name, LoreDefinitionBase colType, string filePath, int blockIndex, int lineNumber) : base(name, colType, filePath, blockIndex, lineNumber)
    {
      switch (colType)
      {
        case LoreTypeDefinition ltd:
          Nodes = new List<LoreNode>();
          break;
        case LoreCollectionDefinition lcd:
          Collections = new List<LoreCollection>();
          break;
        default:
          break;
      }
    }


    public bool ContainsNodes => (Definition as LoreCollectionDefinition).ContainedType is LoreTypeDefinition;
    public bool ContainsCollections => (Definition as LoreCollectionDefinition).ContainedType is LoreCollectionDefinition;

    public int Count => ContainsNodes ? Nodes.Count : Collections.Count();

    public IEnumerator<LoreElement> GetEnumerator() => ContainsNodes ? Nodes.GetEnumerator() : Collections.GetEnumerator();
  }
}
