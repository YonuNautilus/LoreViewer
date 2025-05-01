using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreNodes
{
  public abstract class LoreNode
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public Dictionary<string, string> Attributes { get; set; }
  }
}
