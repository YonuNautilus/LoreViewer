using LoreViewer.LoreElements;
using LoreViewer.Settings;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements.Interfaces
{
  public interface IFieldContainer
  {
    ObservableCollection<LoreAttribute> Attributes { get; }
    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);
    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
    public bool HasAttributes => Attributes.Any();
  }

  public interface ISectionContainer
  {
    ObservableCollection<LoreSection> Sections { get; }

    public bool HasSection(string sectionName) => Sections.Any(s => s.Name == sectionName);

    public LoreSection? GetSection(string sectionName) => Sections.FirstOrDefault(s => s.Name == sectionName);

    public bool HasSections => Sections.Any();
  }
  public interface INodeCollectionContainer
  {
    ObservableCollection<LoreNodeCollection> Collections { get; }
    public bool HasCollection(string collectionName) => Collections.Any(c => c.Name == collectionName);
    public LoreNodeCollection? GetCollection(string collectionName) => Collections.FirstOrDefault(c => c.Name == collectionName);
    public bool HasCollections => Collections.Any();
  }

  public interface INodeContainer
  {
    ObservableCollection<LoreNode> Nodes { get; }
    public bool HasNode(string NodeName) => Nodes.Any(n => n.Name == NodeName);
    public LoreNode? GetNode(string NodeName) => Nodes.FirstOrDefault(n => n.Name == NodeName);
    public bool HasNodes => Nodes.Any();
  }

  /// <summary>
  /// For any LoreEntity that needs to behave and display like a node (ie LoreNode and LoreCompositeNode)
  /// </summary>
  public interface ILoreNode: ILoreEntity, ISectionContainer, IFieldContainer, INodeContainer
  {
    ILoreNode MergeWith(LoreNode node);
  }

  /// <summary>
  /// Top-Level interface implemented by LoreEntity - declares Name, ID, important stuff ALL LoreEntities will need.
  /// </summary>
  public interface ILoreEntity
  {
    string Name { get; set; }
    LoreDefinitionBase Definition { get; set; }
    public T DefinitionAs<T>() where T : LoreDefinitionBase { return Definition as T; }
  }
}
