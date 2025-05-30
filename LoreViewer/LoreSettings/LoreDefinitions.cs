using LoreViewer.LoreElements;
using LoreViewer.Settings.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;

namespace LoreViewer.Settings
{
  public enum EStyle
  {
    SingleValue,
    MultiValue,
    Textual
  }

  public enum ECollectionMode
  {
    Nodes,
    Collections
  }

  public abstract class LoreDefinitionBase
  {
    public string name { get; set; } = string.Empty;

    public LoreDefinitionBase() { }

    public LoreDefinitionBase(string name) { this.name = name; }

    public abstract void PostProcess(LoreSettings settings);
  }

  /// <summary>
  /// The top-level of a lore definition. Can be used to define characters, organizations, races, etc.
  /// </summary>
  public class LoreTypeDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, ICollectionDefinitionContainer
  {
    #region IFieldDefinitionContainer implementation

    // for fields like Date with Start/End
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
    #endregion ISectionDefinitionContainer implementation

    #region ICollectionDefinitionContainer
    public List<LoreCollectionDefinition> collections { get; set; } = new List<LoreCollectionDefinition>();
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => col.name == collectionName);
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
    #endregion ICollectionDefinitionContainer

    public string extends {  get; set; }

    private List<string> RelevantFilePaths = new List<string>();

    public override void PostProcess(LoreSettings settings)
    {
      foreach(LoreCollectionDefinition colDef in collections)
        colDef.PostProcess(settings);
    }
  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer
  {

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
    #endregion ISectionDefinitionContainer implementation

    #region IFieldDefinitionContainer implementation
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    #endregion IFieldDefinitionContainer implementation

    public bool freeform { get; set; } = false;

    public LoreSectionDefinition() { }

    public LoreSectionDefinition(string name, bool isFreeForm) : base(name) { freeform = isFreeForm; }

    public override void PostProcess(LoreSettings settings) { }
  }

  public class LoreAttributeDefinition : LoreDefinitionBase, IFieldDefinitionContainer
  {
    #region IFieldDefinitionContainer implementation
    // for fields like Date with Start/End
    public List<LoreAttributeDefinition> fields { get; set; }
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    public EStyle style { get; set; } = EStyle.SingleValue;

    public bool required = false;

    public bool multivalue = false;

    public bool HasNestedFields => fields != null;

    public bool HasRequiredNestedFields => HasNestedFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    public override void PostProcess(LoreSettings settings) { }
  }

  public class LoreCollectionDefinition : LoreDefinitionBase
  {
    public string entryTypeName { get; set; } = string.Empty;
    public LoreDefinitionBase ContainedType { get; set; }
    public bool SortEntries { get; set; }

    public override void PostProcess(LoreSettings settings)
    {
      if (settings.HasTypeDefinition(this.entryTypeName))
        ContainedType = settings.GetTypeDefinition(this.entryTypeName);
      else
        throw new System.Exception($"Could not find type ({entryTypeName}) definition for collection {this.name}");
    }
  }
}
