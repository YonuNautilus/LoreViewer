using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreNodes
{
  public class LoreSection
  {
    public string Name;

    public List<Block> Blocks;

    public string MarkdownBody;

    public LoreSection() { }

    public LoreSection(string name) { Name = name; }

    public LoreSection(List<Block> block) { Blocks = block; }
  }
}
