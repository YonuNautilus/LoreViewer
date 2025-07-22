using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    public override LoreDefinitionBase Definition { get => _definition; set { _definition = value as LoreTypeDefinition; } }
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

    // Check if the embedded node already exists.
    public bool ContainsEmbeddedNode(LoreTypeDefinition embeddedNodeType, string embeddedNodeTitle)
    {
      // titles of embedded nodes cannot be the same, do that check first, will be quicker than the other checks
      if (Nodes.Any(n => n.Name == embeddedNodeTitle)) return true;

      // Otherwise, we have to check embedded node definitions.
      List<LoreEmbeddedNodeDefinition> matchingEmbeddedDefByType = DefinitionAs<LoreTypeDefinition>().embeddedNodeDefs.Where(d => embeddedNodeType.IsATypeOf(d.nodeType)).ToList();

      // If there are NONE, that really shouldn't happen since we had other checks preventing an embedded node of unallowed type being added... So throw an exception
      if (matchingEmbeddedDefByType.Count == 0)
        throw new Exception($"TRIED TO ADD EMBEDDED NODE ({embeddedNodeTitle}) OF DISALLOWED TYPE {embeddedNodeType} TO NODE {this.Name} OF TYPE {this.Definition.name}");

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
                (def.name == null || def.name == embeddedNodeTitle);

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
    public LoreNode(string name, LoreTypeDefinition definition, string filePath, int blockIndex, int lineNumber) : base(name, definition, filePath, blockIndex, lineNumber) { }

    private string m_sFileContent;

    public string FileContent
    {
      get => string.IsNullOrWhiteSpace(m_sFileContent) ? System.IO.File.ReadAllText(SourcePath) : m_sFileContent;
    }


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

    public bool CanMergeWith(LoreNode newNode)
    {
      if (!tag.HasValue || !newNode.CurrentTag.HasValue) return false;
      else return tag.Value.CanMergeWith(newNode.CurrentTag.Value);
    }
  }
}
