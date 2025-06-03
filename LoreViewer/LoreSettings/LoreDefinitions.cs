using DynamicData;
using LoreViewer.Exceptions;
using LoreViewer.Settings.Interfaces;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
  /// The top-level of a lore definition. Can be used to define types like character, organization, race, etc.
  /// </summary>
  public class LoreTypeDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, ICollectionDefinitionContainer, ITypeDefinitionContainer
  {
    #region IFieldDefinitionContainer implementation

    // for fields like Date with Start/End
    public List<LoreFieldDefinition> fields { get; set; }
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
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

    #region ITypeDefinitionContainer Implementation
    public List<LoreTypeDefinition> types { get; set; } = new List<LoreTypeDefinition>();
    public List<string> subTypeReferences { get; set; } = new List<string>();
    public bool HasTypeDefinition(string typeName) => types.Any(t => typeName == t.name);
    public LoreTypeDefinition? GetTypeDefinition(string typeName) => types.FirstOrDefault(t => typeName == t.name);
    #endregion ITypeDefinitionContainer Implementation

    public string extends {  get; set; }
    public LoreTypeDefinition ParentType { get; set; }
    public bool isExtendedType => ParentType != null;

    public override void PostProcess(LoreSettings settings)
    {
      foreach(LoreCollectionDefinition colDef in collections)
        colDef.PostProcess(settings);

      foreach (LoreTypeDefinition typeDefinition in types)
        typeDefinition.PostProcess(settings);

      if (!string.IsNullOrEmpty(extends))
      {
        ParentType = settings.GetTypeDefinition(extends);
        collections = ParentType.collections.Concat(collections).ToList();
        fields = DefinitionMergeManager.MergeFields(ParentType.fields, fields);
        sections = ParentType.sections.Concat(sections).ToList();
      }
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
    public List<LoreFieldDefinition> fields { get; set; }
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    #endregion IFieldDefinitionContainer implementation

    public bool freeform { get; set; } = false;

    public LoreSectionDefinition() { }

    public LoreSectionDefinition(string name, bool isFreeForm) : base(name) { freeform = isFreeForm; }

    public override void PostProcess(LoreSettings settings) { }
  }

  public class LoreFieldDefinition : LoreDefinitionBase, IFieldDefinitionContainer
  {
    #region IFieldDefinitionContainer implementation
    // for fields like Date with Start/End
    public List<LoreFieldDefinition> fields { get; set; } = new List<LoreFieldDefinition>();
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    public EStyle style { get; set; } = EStyle.SingleValue;

    public bool required = false;

    public bool multivalue = false;

    public bool HasRequiredNestedFields => HasFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    public override void PostProcess(LoreSettings settings) { }

    /// <summary>
    /// Takes base definition, adds info like nested fields from the parent and merges them into this child field definition
    /// </summary>
    /// <param name="parentField"></param>
    public void MergeFrom(LoreFieldDefinition parentField)
    {
      this.required |= parentField.required;
      multivalue |= parentField.multivalue;
    }
  }

  public class LoreCollectionDefinition : LoreDefinitionBase
  {
    public string entryTypeName { get; set; } = string.Empty;
    public LoreCollectionDefinition entryCollection { get; set; }
    public LoreDefinitionBase ContainedType { get; set; }
    public bool SortEntries { get; set; }

    public bool IsCollectionOfCollections => ContainedType is LoreCollectionDefinition;

    public override void PostProcess(LoreSettings settings)
    {
      if (entryCollection != null)
        entryCollection.PostProcess(settings);

      if (!string.IsNullOrWhiteSpace(entryTypeName) && entryCollection != null)
        throw new CollectionWithTypeAndCollectionDefined(this);

      if (!string.IsNullOrWhiteSpace(entryTypeName))
      {
        if (settings.HasTypeDefinition(this.entryTypeName))
          ContainedType = settings.GetTypeDefinition(this.entryTypeName);
        else
          throw new System.Exception($"Could not find type ({entryTypeName}) definition for collection {this.name}");
      }
      else if (entryCollection != null)
        ContainedType = entryCollection;

    }
  }

  public static class DefinitionMergeManager
  {
    public static List<LoreFieldDefinition> MergeFields(List<LoreFieldDefinition> baseFields, List<LoreFieldDefinition> childFields)
    {
      List<LoreFieldDefinition> ret = new List<LoreFieldDefinition>(baseFields);

      foreach (LoreFieldDefinition childField in childFields)
      {
        // Look for a match, if this child field has a field of the same name described in the parent list.
        LoreFieldDefinition parentFieldIfDefined = ret.Find(f => f.name == childField.name);

        if (parentFieldIfDefined != null)
        {
          childField.fields = MergeFields(parentFieldIfDefined.fields, childField.fields);
          childField.MergeFrom(parentFieldIfDefined);
          ret.Replace(parentFieldIfDefined, childField);
        }
        else 
          ret.Add(childField);

      }

      return ret;
    }
  }
}
