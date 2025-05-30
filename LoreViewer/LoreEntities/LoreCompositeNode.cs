using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreElements
{
  public class LoreCompositeNode : LoreEntity, ILoreNode
  {
    public override LoreDefinitionBase Definition { get => _definition; set { _definition = value as LoreTypeDefinition; } }
    private LoreTypeDefinition _definition;

    #region IFieldContainer Implementation
    public ObservableCollection<LoreAttribute> Attributes => new ObservableCollection<LoreAttribute>(Nodes.SelectMany(ln => ln.Attributes));
    public LoreAttribute? GetAttribute(string name) => Attributes.FirstOrDefault(a => a.Name == name);
    public bool HasAttribute(string name) => Attributes.Any(a => a.Name == name);
    #endregion

    #region ISectionContainer Implementation
    public ObservableCollection<LoreSection> Sections => new ObservableCollection<LoreSection>(Nodes.SelectMany(ln => ln.Sections));
    public LoreSection? GetSection(string name) => Sections.FirstOrDefault(s => s.Name == name);
    public bool HasSection(string name) => Sections.Any(s => s.Name == name);
    #endregion

    #region INodeContainer Implementation
    public ObservableCollection<LoreNode> Nodes { get; } = new ObservableCollection<LoreNode>();
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion

    #region INodeCollectionContainer Implementation
    public ObservableCollection<LoreNodeCollection> Collections => new ObservableCollection<LoreNodeCollection>(Nodes.SelectMany(ln => ln.Collections));
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreNodeCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreTypeDefinition typeDef) => Collections.Any(c => c.Type == typeDef);
    public LoreNodeCollection? GetCollectionOfType(LoreTypeDefinition typeDef) => Collections.FirstOrDefault(c => c.Type == typeDef);
    public bool HasCollectionOfTypeName(string typeName) => Collections.Any(c => c.Type.name.Equals(typeName));
    public LoreNodeCollection? GetCollectionOfTypeName(string typeName) => Collections.FirstOrDefault(c => c.Type.name == typeName);
    #endregion

    public LoreCompositeNode(string name, LoreTypeDefinition definition) : base(name, definition) { }

    public LoreCompositeNode(LoreNode newNode) : base(newNode.Name, newNode.Definition) { Nodes.Add(newNode); }

    public LoreCompositeNode(LoreNode original, LoreNode newNode) : this(original) { Nodes.Add(newNode); }

    public ILoreNode MergeWith(LoreNode newNode)
    {
      Nodes.Add(newNode);
      return this;
    }
  }
}
