using DynamicData;
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
  public abstract class LoreCollection : LoreNarrativeElement
  {
    public LoreCollection(string name, LoreDefinitionBase containedType) : base(name, containedType) { }
  }

  public class LoreNodeCollection : LoreCollection, INodeContainer
  {
    #region INodeContainer Implementation
    public ObservableCollection<LoreNode> Nodes { get; set; } = new ObservableCollection<LoreNode>();
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion

    public override LoreDefinitionBase Definition { get; set; }

    public LoreTypeDefinition Type { get; set; }

    public LoreNodeCollection(string name, LoreDefinitionBase containedType) : base(name, containedType) { }

    public int Count => Nodes.Count();

    public void Add(LoreNode item) => Nodes.Add(item);

    public IEnumerator<LoreNode> GetEnumerator() => Nodes.GetEnumerator();

    public override string ToString() => $"Collection of {Definition.name}";
  }

  public class LoreCollectionCollection : LoreCollection, INodeCollectionContainer
  {
    public override LoreDefinitionBase Definition { get; set; }

    #region INodeCollectionContainer Implementation
    public ObservableCollection<LoreNodeCollection> Collections { get; }
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreNodeCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreTypeDefinition typeDef) => Collections.Any(c => c.Type == typeDef);
    public LoreNodeCollection? GetCollectionOfType(LoreTypeDefinition typeDef) => Collections.FirstOrDefault(c => c.Type == typeDef);
    public bool HasCollectionOfTypeName(string typeName) => Collections.Any(c => c.Type.name.Equals(typeName));
    public LoreNodeCollection? GetCollectionOfTypeName(string typeName) => Collections.FirstOrDefault(c => c.Type.name == typeName);
    #endregion

    public LoreCollectionCollection(string name, LoreDefinitionBase containedType) : base(name, containedType) { }

    public int Count => Collections.Count();
    public void Add(LoreNodeCollection item) => Collections.Add(item);
    public IEnumerator<LoreNodeCollection> GetEnumerator() => Collections.GetEnumerator();
    public override string ToString() => $"Collection of {Definition.name}";
  }
}
