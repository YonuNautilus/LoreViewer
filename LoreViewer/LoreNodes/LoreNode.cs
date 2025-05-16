using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;

namespace LoreViewer.LoreNodes
{
  public class LoreNode : LoreElement
  {
    public string SourcePath;
    public int BlockIndex;
    public Guid Id { get; set; }

    public LoreTypeDefinition Type { get; set; }

    public ObservableCollection<LoreAttribute> Attributes = new ObservableCollection<LoreAttribute>();

    public ObservableCollection<LoreNode> Children = new ObservableCollection<LoreNode>();

    public ObservableCollection<LoreNodeCollection> CollectionChildren = new ObservableCollection<LoreNodeCollection>();

    public ObservableCollection<LoreSection> Sections = new ObservableCollection<LoreSection>();

    public LoreNode(LoreTypeDefinition type, string name)
    {
      Type = type;
      Name = name;
    }

    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);

    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);

    public bool HasSection(string sectionName) => Sections.Any(s => s.Name.Equals(sectionName));

    public LoreSection? GetSection(string sectionName) => Sections.FirstOrDefault(s => s.Name.Equals(sectionName));

    public bool HasCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.Any(c => c.Type == typeDef);

    public LoreNodeCollection? GetCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.FirstOrDefault(c => c.Type == typeDef);

    public bool HasCollectionOfTypeName(string typeName) => CollectionChildren.Any(c => c.Type.name.Equals(typeName));

    public LoreNodeCollection? GetCollectionOfTypeName(string typeName) => CollectionChildren.FirstOrDefault(c => c.Type.name == typeName);
  }
}
