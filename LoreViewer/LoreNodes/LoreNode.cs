using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreNodes
{
  public class LoreNode
  {
    public string SourcePath;
    public int BlockIndex;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

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
  }
}
