using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoreViewer.Settings;

namespace LoreViewer.LoreNodes
{
  public class LoreNode
  {
    public string SourcePath;
    public int BlockIndex;
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public LoreTypeDefinition Type { get; set; }

    public Dictionary<string, LoreAttribute> Attributes = new Dictionary<string, LoreAttribute>();

    public Dictionary<string, LoreNode> Children = new Dictionary<string, LoreNode>();

    public Dictionary<string, LoreNodeCollection> CollectionChildren = new Dictionary<string, LoreNodeCollection>();

    public List<LoreSection> Sections = new List<LoreSection>();

    public LoreNode(LoreTypeDefinition type, string name)
    {
      Type = type;
      Name = name;
    }
  }
}
