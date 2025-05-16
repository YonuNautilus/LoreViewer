using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Settings
{
  public enum EStyle
  {
    BulletPoint,
    Table,
    Freeform,
    Hybrid
  }

  /// <summary>
  /// The top-level of a lore definition. Can be used to define characters, organizations, races, etc.
  /// </summary>
  public class LoreTypeDefinition
  {
    public bool HasSectionName(string sectionName) => sections.FirstOrDefault(sec => sectionName.Contains(sec.name)) != null;

    private List<string> RelevantFilePaths = new List<string>();

    public EStyle field_style { get; set; }

    /// <summary>
    /// Collection of field definitions that should be visible at the top of the markdown on a declared object of a type.
    /// For example, a character will have a field for name, a field for race, etc.
    /// </summary>
    public List<LoreAttributeDefinition> fields { get; set; } = new List<LoreAttributeDefinition>();

    /// <summary>
    /// A collection of sections in an object's file (or section). These will contain more infomation, usually paragraphs,
    /// that expand upon base level fields, or add new information. Sections can contain their own fields and subsections.
    /// </summary>
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();

    public List<LoreCollectionDefinition> collections { get; set; }
  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition
  {
    public string name { get; set; }
    public EStyle type { get; set; }

    public List<LoreSectionDefinition> sections { get; set; }
    public List<LoreAttributeDefinition> fields { get; set; }

    public bool freeform { get; set; } = false;

    public bool HasFields => fields != null && fields.Count > 0;
  }

  public class LoreAttributeDefinition
  {
    public string name { get; set; } = string.Empty;
    public EStyle style { get; set; } // bullet_point, bullet_list, heading_paragraph

    public bool required = false;

    public bool multivalue = false;

    public bool HasNestedFields => nestedFields != null;

    public bool HasRequiredNestedFields => HasNestedFields ? nestedFields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    // for fields like Date with Start/End
    public List<LoreAttributeDefinition> nestedFields { get; set; }
  }

  public class LoreCollectionDefinition
  {
    public string entryType { get; set; } = string.Empty;
    public string EntryStyle { get; set; } = string.Empty;
    public bool SortEntries { get; set; }
  }

}
