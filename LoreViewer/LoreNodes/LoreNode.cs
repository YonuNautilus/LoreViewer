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

    public Dictionary<string, LoreNode> Children = new Dictionary<string, LoreNode>();

    public Dictionary<string, LoreNodeCollection> CollectionChildren = new Dictionary<string, LoreNodeCollection>();

    public List<LoreSection> Sections = new List<LoreSection>();

    public LoreNode(LoreTypeDefinition type, string name)
    {
      Type = type;
      Name = name;
    }

    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);

    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
  }
}
