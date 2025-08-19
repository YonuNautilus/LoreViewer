using LoreViewer.Domain.Settings.Definitions;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Domain.Entities
{
  /// <summary>
  /// A Section represents a descriptive or narrative unit of a Node (i.e. Character's background, spell's effects, location's origin, faction's goals).
  /// <para/>
  /// Supports narrative/descriptive text.
  /// <para/>
  /// Definition Type: LoreSectionDefinition
  /// <br/>
  /// <br/>
  /// Sections can contain:
  /// <list type="bullet">
  /// <item>Sections</item>
  /// <item>Attributes</item>
  /// </list>
  /// </summary>
  public class LoreSection : LoreNarrativeElement, IAttributeContainer, ISectionContainer
  {
    public override LoreDefinitionBase Definition { get => _definition; set { _definition = value as LoreSectionDefinition; } }
    private LoreSectionDefinition _definition;

    #region IFieldContainer Implementation
    public List<LoreAttribute> Attributes { get; set; } = new List<LoreAttribute>();
    public LoreAttribute? GetAttribute(string name) => Attributes.FirstOrDefault(a => a.Name == name);
    public bool HasAttribute(string name) => Attributes.Any(a => a.Name == name);
    #endregion
    #region ISectionContainer Implementation
    public List<LoreSection> Sections { get; set; } = new List<LoreSection>();
    public LoreSection? GetSection(string name) => Sections.FirstOrDefault(s => s.Name == name);
    public bool HasSection(string name) => Sections.Any(s => s.Name == name);
    #endregion

    private List<Block> _blocks;

    public List<Block> Blocks
    {
      get { if (_blocks == null) _blocks = new List<Block>(); return _blocks; }
      set { _blocks = value; }
    }

    public LoreSection(string name, LoreSectionDefinition definition) : base(name, definition) { }
    public LoreSection(string name, LoreSectionDefinition definition, string filePath, int blockIndex, int lineNumber) : base(name, definition, filePath, blockIndex, lineNumber) { }
  }
}
