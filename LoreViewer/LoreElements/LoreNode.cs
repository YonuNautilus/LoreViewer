using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.AccessControl;

namespace LoreViewer.LoreElements
{
  public class LoreNode : LoreNarrativeElement, IFieldContainer, ISectionContainer
  {
    public LoreTypeDefinition Type { get; set; }

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

    public ObservableCollection<LoreNode> Children = new ObservableCollection<LoreNode>();

    public ObservableCollection<LoreNodeCollection> CollectionChildren = new ObservableCollection<LoreNodeCollection>();

    public LoreNode(LoreTypeDefinition type, string name)
    {
      Type = type;
      Name = name;
    }

    public bool HasCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.Any(c => c.Type == typeDef);

    public LoreNodeCollection? GetCollectionOfType(LoreTypeDefinition typeDef) => CollectionChildren.FirstOrDefault(c => c.Type == typeDef);

    public bool HasCollectionOfTypeName(string typeName) => CollectionChildren.Any(c => c.Type.name.Equals(typeName));

    public LoreNodeCollection? GetCollectionOfTypeName(string typeName) => CollectionChildren.FirstOrDefault(c => c.Type.name == typeName);
  }
}
