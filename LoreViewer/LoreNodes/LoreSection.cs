using LoreViewer.Settings;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreNodes
{
  public class LoreSection
  {
    public string Name;

    private List<Block> _blocks;

    public List<Block> Blocks
    {
      get { if (_blocks == null) _blocks = new List<Block>(); return _blocks; }
      set { _blocks = value; }
    }

    public ObservableCollection<LoreAttribute> Attributes = new ObservableCollection<LoreAttribute>();

    private List<LoreSection> _subSections;

    public List<LoreSection> SubSections
    {
      get {
        if (_subSections == null) _subSections = new List<LoreSection>();
        return _subSections;
      }
      set { SubSections = value; }
    }

    public string Text = string.Empty;

    public string MarkdownBody;


    public LoreSection() { }

    public LoreSection(string name) { Name = name; }

    public LoreSection(List<Block> block) { Blocks = block; }

    public LoreSectionDefinition Definition;


    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);

    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
  }
}
