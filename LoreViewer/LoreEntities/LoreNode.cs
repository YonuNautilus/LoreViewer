using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;

namespace LoreViewer.LoreElements
{
  public class LoreNode : LoreNarrativeElement, ILoreNode
  {
    public override LoreDefinitionBase Definition { get => _definition ; set { _definition = value as LoreTypeDefinition; } }
    private LoreTypeDefinition _definition;

    #region IFieldContainer Implementation
    public ObservableCollection<LoreAttribute> Attributes { get; set; } = new ObservableCollection<LoreAttribute>();
    public LoreAttribute? GetAttribute(string name) => Attributes.FirstOrDefault(a => a.Name == name);
    public bool HasAttribute(string name) => Attributes.Any(a => a.Name == name);
    #endregion
    #region ISectionContainer Implementation
    public ObservableCollection<LoreSection> Sections { get; set; } = new ObservableCollection<LoreSection>();
    public LoreSection? GetSection(string name) => Sections.FirstOrDefault(s => s.Name == name);
    public bool HasSection(string name) => Sections.Any(s => s.Name == name);
    #endregion

    #region INodeContainer Implementation
    public ObservableCollection<LoreNode> Nodes { get; } = new ObservableCollection<LoreNode>();

    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);

    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion

    public ObservableCollection<LoreNodeCollection> CollectionChildren = new ObservableCollection<LoreNodeCollection>();

    public LoreNode(string name, LoreTypeDefinition definition) : base(name, definition) { }

    public bool HasCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.Any(c => c.Type == typeDef);

    public LoreNodeCollection? GetCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.FirstOrDefault(c => c.Type == typeDef);

    public bool HasCollectionOfTypeName(string typeName) => CollectionChildren.Any(c => c.Type.name.Equals(typeName));

    public LoreNodeCollection? GetCollectionOfTypeName(string typeName) => CollectionChildren.FirstOrDefault(c => c.Type.name == typeName);

    public void MergeIn(LoreNode toMergeIn)
    {
      foreach (LoreAttribute la in toMergeIn.Attributes)
        Attributes.Add(la);

      foreach (LoreSection ls in toMergeIn.Sections)
        Sections.Add(ls);
      
      foreach (LoreNode ln in toMergeIn.Nodes)
        Nodes.Add(ln);

      foreach (LoreNodeCollection lnc in toMergeIn.CollectionChildren)
        CollectionChildren.Add(lnc);
    }

    public ILoreNode MergeWith(LoreNode node) => new LoreCompositeNode(this, node);
  }
}
