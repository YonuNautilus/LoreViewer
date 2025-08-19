using LoreViewer.Domain.Settings.Definitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Domain.Entities
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
    public List<LoreAttribute> Attributes => new List<LoreAttribute>(_internalNodes.SelectMany(ln => ln.Attributes));
    public LoreAttribute? GetAttribute(string name) => Attributes.FirstOrDefault(a => a.Name == name);
    public bool HasAttribute(string name) => Attributes.Any(a => a.Name == name);
    #endregion

    #region ISectionContainer Implementation
    public List<LoreSection> Sections => new List<LoreSection>(_internalNodes.SelectMany(ln => ln.Sections));
    public LoreSection? GetSection(string name) => Sections.FirstOrDefault(s => s.Name == name);
    public bool HasSection(string name) => Sections.Any(s => s.Name == name);
    #endregion

    #region INodeContainer Implementation
    public List<LoreNode> Nodes => new List<LoreNode>(_internalNodes.SelectMany(ln => ln.Nodes));
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);

    // Check if the embedded node already exists.
    public bool ContainsEmbeddedNode(LoreTypeDefinition embeddedNodeType, string embeddedNodeTitle)
    {
      // titles of embedded nodes cannot be the same, do that check first, will be quicker than the other checks
      if (Nodes.Any(n => n.Name == embeddedNodeTitle)) return true;

      // Otherwise, we have to check embedded node definitions.
      List<LoreEmbeddedNodeDefinition> matchingEmbeddedDefByType = DefinitionAs<LoreTypeDefinition>().embeddedNodeDefs.Where(d => embeddedNodeType.IsATypeOf(d.nodeType)).ToList();

      // If there are NONE, that really shouldn't happen since we had other checks preventing an embedded node of unallowed type being added... So throw an exception
      if (matchingEmbeddedDefByType.Count == 0)
        throw new Exception($"TRIED TO ADD EMBEDDED NODE ({embeddedNodeTitle}) OF DISALLOWED TYPE {embeddedNodeType} TO NODE {Name} OF TYPE {Definition.name}");

      // If there's only one embedded node definition for the new embedded node's type, that's an easy check -- see if the Nodes list has any nodes which are a type of the one specified in the definition.
      // Note that, if there's only one matching embedded definition that shares a type or is an ancestor type of the node we are trying to add, it doesn't matter what the name of the definition or the 
      //    title of the node we are trying to add are.
      //
      // If there is, the node already exists, return true.
      if (matchingEmbeddedDefByType.Count == 1)
      {
        LoreEmbeddedNodeDefinition lend = matchingEmbeddedDefByType[0];
        return Nodes.Any(n => n.DefinitionAs<LoreTypeDefinition>().IsATypeOf(lend.nodeType));
      }



      // If there are multiple, we need to start checking more specifics.
      // Essentially we are finding embedded node definitions (from matchingEmbeddedDefByType) that are not yet satisfied by nodes in the Nodes list.
      else
      {
        foreach (LoreEmbeddedNodeDefinition def in matchingEmbeddedDefByType)
        {
          bool defIsSatisfied = Nodes.Any(n =>
              n.DefinitionAs<LoreTypeDefinition>().IsATypeOf(def.nodeType) && (def.hasTitleRequirement ? n.Name == def.name : true)
          );

          if (!defIsSatisfied)
          {
            // This definition is available. Would the incoming node satisfy it?
            bool matchesThisDefinition =
                def.name == null || def.name == embeddedNodeTitle;

            if (matchesThisDefinition)
            {
              // A valid slot for this node is open, and no conflicts found
              return false;
            }
          }
        }
      }
      return true;
    }
    #endregion

    #region ICollectionContainer Implementation
    public List<LoreCollection> Collections => new List<LoreCollection>(_internalNodes.SelectMany(ln => ln.Collections));
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
    public bool HasCollectionOfType(LoreDefinitionBase typeDef) => Collections.Any(c => c.Definition == typeDef);
    public LoreCollection? GetCollectionOfType(LoreDefinitionBase typeDef) => Collections.FirstOrDefault(c => c.Definition == typeDef);
    public bool HasCollectionOfDefinedName(string typeName) => Collections.Any(c => c.Definition.name.Equals(typeName));
    public LoreCollection? GetCollectionWithDefinedName(string typeName) => Collections.FirstOrDefault(c => c.Definition.name == typeName);
    #endregion

    private List<LoreNode> _internalNodes = new List<LoreNode>();

    public List<LoreNode> InternalNodes { get { return _internalNodes; } }

    public LoreCompositeNode(string name, LoreTypeDefinition definition) : base(name, definition) { }

    public LoreCompositeNode(LoreNode newNode) : base(newNode.Name, newNode.Definition) { _internalNodes.Add(newNode); }

    /// <summary>
    /// The Merging constructor. Merged two LoreNodes into a new LoreCompositeNode. This assumes that the two nodes have equivalent LoreTagInfo.
    /// </summary>
    /// <param name="original"></param>
    /// <param name="newNode"></param>
    public LoreCompositeNode(LoreNode original, LoreNode newNode) : this(original)
    {
      _internalNodes.Add(newNode);
      SetTag(original.CurrentTag.Value.CreateCompositeNodeTag(newNode.CurrentTag.Value));
    }

    public ILoreNode MergeWith(LoreNode newNode)
    {
      _internalNodes.Add(newNode);
      return this;
    }

    public bool CanMergeWith(LoreNode newNode)
    {
      if (!tag.HasValue || !newNode.CurrentTag.HasValue) return false;
      else return tag.Value.CanMergeWith(newNode.CurrentTag.Value);
    }
  }
}
