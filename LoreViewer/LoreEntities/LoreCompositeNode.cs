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
  /// <summary>
  /// An Composite Node represents a single object or idea in lore (i.e. Character, spell, location, faction),
  /// concatenating <c>LoreNode</c>s of the same kind (same name and type definition) from different files.
  /// <para/>
  /// As long as two top-level nodes have identical name and type definition, they will be compiled under a composite node.
  /// Attributes, collections, sections and child nodes of each node within the composite are concatenated into a single
  /// accessable list on the LoreCompositeNode.
  /// <para/>
  /// Supports narrative/descriptive text.
  /// <para/>
  /// Definition Type: LoreTypeDefinition
  /// <br/>
  /// <br/>
  /// <c>LoreCompositeNode</c> can contain:
  /// <list type="bullet">
  /// <item>Attributes</item>
  /// <item>Sections</item>
  /// <item>Collections</item>
  /// <item>Nodes</item>
  /// </list>
  /// </summary>
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

    #region ICollectionContainer Implementation
    public ObservableCollection<LoreCollection> Collections => new ObservableCollection<LoreCollection>(Nodes.SelectMany(ln => ln.Collections));
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfDefinedName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionWithDefinedName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
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
