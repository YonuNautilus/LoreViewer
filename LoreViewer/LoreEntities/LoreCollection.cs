using DynamicData;
using LoreViewer.Exceptions;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements
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
    public ObservableCollection<LoreNode> Nodes { get; set; } = new ObservableCollection<LoreNode>();
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion
    #region INodeCollectionContainer Implementation
    public ObservableCollection<LoreCollection> Collections { get; }
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfTypeName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionOfTypeName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
    #endregion

    public LoreCollection(string name, LoreDefinitionBase containedType) : base(name, containedType)
    {
      switch (containedType)
      {
        case LoreTypeDefinition ltd:
          Nodes = new ObservableCollection<LoreNode>();
          break;
        case LoreCollectionDefinition lcd:
          Collections = new ObservableCollection<LoreCollection>();
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
