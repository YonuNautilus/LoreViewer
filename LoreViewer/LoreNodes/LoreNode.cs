using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreNodes
{
  public class LoreNode
  {
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Dictionary<string, string> Attributes = new Dictionary<string, string>();

    public Dictionary<string, LoreNode> Children = new Dictionary<string, LoreNode>();

    public LoreNode(string type, string name)
    {
      Type = type;
      Name = name;
    }
  }
}
