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
  /// <summary>
  /// An Node represents a single object or idea in lore (i.e. Character, spell, location, faction).
  /// <para/>
  /// Supports narrative/descriptive text.
  /// <para/>
  /// Definition Type: LoreTypeDefinition
  /// <br/>
  /// <br/>
  /// <c>LoreNode</c> can contain:
  /// <list type="bullet">
  /// <item>Attributes</item>
  /// <item>Sections</item>
  /// <item>Collections</item>
  /// <item>Nodes</item>
  /// </list>
  /// </summary>
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
    #region ICollectionContainer Implementation
    public ObservableCollection<LoreCollection> Collections { get; } = new ObservableCollection<LoreCollection>();
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfDefinedName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionWithDefinedName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
    #endregion

    public LoreNode(string name, LoreTypeDefinition definition) : base(name, definition) { }


    public void MergeIn(LoreNode toMergeIn)
    {
      foreach (LoreAttribute la in toMergeIn.Attributes)
        Attributes.Add(la);

      foreach (LoreSection ls in toMergeIn.Sections)
        Sections.Add(ls);
      
      foreach (LoreNode ln in toMergeIn.Nodes)
        Nodes.Add(ln);

      foreach (LoreCollection lnc in toMergeIn.Collections)
        Collections.Add(lnc);
    }

    public ILoreNode MergeWith(LoreNode node) => new LoreCompositeNode(this, node);
  }
}
