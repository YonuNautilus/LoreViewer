using LoreViewer.Settings.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Settings
{
  public enum EStyle
  {
    SingleValue,
    MultiValue,
    Textual
  }

  public abstract class LoreDefinitionBase
  {
    public string name { get; set; } = string.Empty;

    public LoreDefinitionBase() { }

    public LoreDefinitionBase(string name) { this.name = name; }
  }

  /// <summary>
  /// The top-level of a lore definition. Can be used to define characters, organizations, races, etc.
  /// </summary>
  public class LoreTypeDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, ICollectionDefinitionContainer
  {
    #region IFieldDefinitionContainer implementation

    // for fields like Date with Start/End
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
    #endregion ISectionDefinitionContainer implementation

    #region ICollectionDefinitionContainer
    public List<LoreCollectionDefinition> collections { get; set; } = new List<LoreCollectionDefinition>();
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => col.name == collectionName);
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
    #endregion ICollectionDefinitionContainer

    private List<string> RelevantFilePaths = new List<string>();
    /// <summary>
    /// Collection of field definitions that should be visible at the top of the markdown on a declared object of a type.
    /// For example, a character will have a field for name, a field for race, etc.
    /// </summary>

    /// <summary>
    /// A collection of sections in an object's file (or section). These will contain more infomation, usually paragraphs,
    /// that expand upon base level fields, or add new information. Sections can contain their own fields and subsections.
    /// </summary>

  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer
  {

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
    #endregion ISectionDefinitionContainer implementation

    #region IFieldDefinitionContainer implementation
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    #endregion IFieldDefinitionContainer implementation

    public bool freeform { get; set; } = false;

    public bool HasFields => fields != null && fields.Count > 0;

    public LoreSectionDefinition() { }

    public LoreSectionDefinition(string name, bool isFreeForm) : base(name) { freeform = isFreeForm; }
  }

  public class LoreAttributeDefinition : LoreDefinitionBase, IFieldDefinitionContainer
  {
    public EStyle style { get; set; } = EStyle.SingleValue;

    public bool required = false;

    public bool multivalue = false;

    public bool HasNestedFields => fields != null;

    public bool HasRequiredNestedFields => HasNestedFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    #region IFieldDefinitionContainer implementation

    // for fields like Date with Start/End
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    #endregion IFieldDefinitionContainer implementation
  }

  public class LoreCollectionDefinition : LoreDefinitionBase, ICollectionDefinitionContainer
  {
    #region ICollectionDefinitionContainer
    public List<LoreCollectionDefinition> collections { get; set; } = new List<LoreCollectionDefinition>();
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => collectionName.Contains(col.name));
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
    #endregion ICollectionDefinitionContainer

    public string entryType { get; set; } = string.Empty;
    public string EntryStyle { get; set; } = string.Empty;
    public bool SortEntries { get; set; }

  }
}
