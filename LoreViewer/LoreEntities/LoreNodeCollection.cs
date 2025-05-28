using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements
{
  public class LoreNodeCollection : LoreNarrativeElement, INodeContainer
  {
    public override LoreDefinitionBase Definition { get => _definition; set { _definition = value as LoreTypeDefinition; } }
    private LoreTypeDefinition _definition;

    #region INodeContainer Implementation
    public ObservableCollection<LoreNode> Nodes { get; set; } = new ObservableCollection<LoreNode>();

    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);

    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    #endregion

    public LoreTypeDefinition Type { get; set; }

    public LoreNodeCollection(string name, LoreTypeDefinition containedType) : base(name, containedType) { }

    public int Count => Nodes.Count();

    public void Add(LoreNode item) => Nodes.Add(item);

    public IEnumerator<LoreNode> GetEnumerator() => Nodes.GetEnumerator();

    public override string ToString() => $"Collection of {Definition.name}";
  }
}
