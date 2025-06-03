using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using Markdig.Syntax;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements
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
  public class LoreSection : LoreNarrativeElement, IFieldContainer, ISectionContainer
  {
    public override LoreDefinitionBase Definition { get => _definition; set { _definition = value as LoreSectionDefinition; } }
    private LoreSectionDefinition _definition;

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

    private List<Block> _blocks;

    public List<Block> Blocks
    {
      get { if (_blocks == null) _blocks = new List<Block>(); return _blocks; }
      set { _blocks = value; }
    }

    public LoreSection(string name, LoreSectionDefinition definition) : base(name, definition) { }
  }
}
