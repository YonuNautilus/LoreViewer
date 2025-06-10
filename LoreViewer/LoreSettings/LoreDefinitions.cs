using DynamicData;
using LoreViewer.Exceptions.SettingsParsingExceptions;
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
    Textual,
    PickList
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

    public override string ToString() => name;

    public bool processed = false;
  }

  /// <summary>
  /// The top-level of a lore definition. Can be used to define types like character, organization, race, etc.
  /// </summary>
  public class LoreTypeDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, ICollectionDefinitionContainer, IEmbeddedNodeDefinitionContainer
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

    #region IEmbeddedNodeDefinitionContainer Implementation
    public List<LoreEmbeddedNodeDefinition> embeddedNodeDefs { get; set; } = new List<LoreEmbeddedNodeDefinition>();
    public bool HasTypeDefinition(string typeName) => embeddedNodeDefs.Any(t => typeName == t.name);
    public bool HasTypeDefinition(LoreTypeDefinition typeDef) => embeddedNodeDefs.Any(t => t.nodeType.IsParentOf(typeDef));

    // Check if the TYPE, regardless of title, is allowed as an embedded node.
    public bool IsAllowedEmbeddedType(LoreTypeDefinition typeDefinition) => embeddedNodeDefs.Any(t => t.nodeType == typeDefinition || t.nodeType.IsParentOf(typeDefinition));

    // If the node type we found is the same or extends the LoreEmbeddedNodeDefinition's node type definition, it is allowed.
    // If the LoreEmbeddedNodeDefinition does not have a title defined, it can have any title. Otherwise, title must match.
    public bool IsAllowedEmbeddedNode(LoreTypeDefinition typeDef, string nodeTitle) => embeddedNodeDefs.Any(
          t =>
          (t.nodeType == typeDef || t.nodeType.IsParentOf(typeDef)) &&
          (!string.IsNullOrWhiteSpace(t.name) ? t.name == nodeTitle : true));

    public bool HasNestedNodes => embeddedNodeDefs != null && embeddedNodeDefs.Count() > 0;

    #endregion IEmbeddedNodeDefinitionContainer
    public bool AllowsEmbeddedType(LoreTypeDefinition typeDef) => HasTypeDefinition(typeDef) || this.IsParentOf(typeDef);


    public string extends { get; set; }
    public LoreTypeDefinition ParentType { get; set; }
    public bool isExtendedType => ParentType != null;

    public bool HasRequiredEmbeddedNodes => HasNestedNodes ? embeddedNodeDefs.Aggregate(false, (sum, next) => sum || next.required || next.nodeType.HasRequiredEmbeddedNodes, r => r) : false;

    public void SetParent(LoreTypeDefinition type)
    {
      ParentType = type;
      collections = ParentType.collections.Concat(collections).ToList();
      fields = DefinitionMergeManager.MergeFields(ParentType.fields, fields);
      sections = DefinitionMergeManager.MergeSections(ParentType.sections, sections);
    }

    public bool IsParentOf(LoreTypeDefinition subTypeDef) => subTypeDef.isExtendedType && (subTypeDef.ParentType == this || this.IsParentOf(subTypeDef.ParentType));
    public bool IsATypeOf(LoreTypeDefinition parentTypeDef) => this == parentTypeDef || parentTypeDef.IsParentOf(this);

    public override void PostProcess(LoreSettings settings)
    {

      /* Check for embedded node rule violations:
       *   1. Two embedded nodes under the same parent node cannot have the same title
       *   2. Regarding multiple embedded nodes on the same type inheritance tree:
       *      1. The LEAST ABSTRACT type of embedded node does not nead a title
       *      2. Any nodes types that are MORE ABSTRACT must have a title
       */

      // Check rule 1 before any postprocessing
      IEnumerable<LoreEmbeddedNodeDefinition> dupedTitles = embeddedNodeDefs.Where(d => d.hasTitleRequirement).GroupBy(d => d.name).Where(c => c.Count() > 1).Select(name => name.First());
      if (dupedTitles.Any())
        throw new EmbeddedNodesWithSameTitleException(dupedTitles.First(), dupedTitles.First().name);

      foreach (LoreEmbeddedNodeDefinition lend in embeddedNodeDefs)
        lend.PostProcess(settings);


      // Now check for rule 2 violations.
      // Iterate through each embedded node def. Assuming it is the least abstract, get all embedded node defs whose type is a parent
      foreach (LoreEmbeddedNodeDefinition lend in embeddedNodeDefs)
      {
        IEnumerable<LoreEmbeddedNodeDefinition> matchingTypeOrParentType = embeddedNodeDefs.Except(new LoreEmbeddedNodeDefinition[] { lend }).Where(d => lend.nodeType.IsATypeOf(d.nodeType));
        LoreEmbeddedNodeDefinition firstWithoutNameReq = matchingTypeOrParentType.FirstOrDefault(d => !d.hasTitleRequirement);
        if (firstWithoutNameReq != null)
          throw new EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException(firstWithoutNameReq);
      }

      processed = true;

      foreach (LoreCollectionDefinition colDef in collections)
        if (!colDef.processed)
          colDef.PostProcess(settings);
    }
  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, IRequirable
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

    public bool required { get; set; }
    public bool HasRequiredNestedSections => HasSections ? sections.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedSections, r => r) : false;


    public bool freeform { get; set; } = false;

    public LoreSectionDefinition() { }

    public LoreSectionDefinition(string name, bool isFreeForm) : base(name) { freeform = isFreeForm; }


    /// <summary>
    /// Takes base definition, adds info like nested fields from the parent and merges them into this child field definition
    /// </summary>
    /// <param name="parentField"></param>
    public void MergeFrom(LoreSectionDefinition parentField)
    {
      this.freeform |= parentField.freeform;
    }

    public override void PostProcess(LoreSettings settings) { }
  }

  public class LoreFieldDefinition : LoreDefinitionBase, IFieldDefinitionContainer, IRequirable
  {
    #region IFieldDefinitionContainer implementation
    // for fields like Date with Start/End
    public List<LoreFieldDefinition> fields { get; set; } = new List<LoreFieldDefinition>();
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    public EStyle style { get; set; } = EStyle.SingleValue;

    public bool required { get; set; }

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

  public class LoreCollectionDefinition : LoreDefinitionBase, IRequirable
  {
    public string entryTypeName { get; set; } = string.Empty;
    public LoreCollectionDefinition entryCollection { get; set; }
    public LoreDefinitionBase ContainedType { get; set; }
    public bool SortEntries { get; set; }
    public bool required { get; set; }

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

  public class LoreEmbeddedNodeDefinition : LoreDefinitionBase, IRequirable
  {
    public string entryTypeName { get; set; }
    public LoreTypeDefinition nodeType { get; set; }
    public bool required { get; set; }

    public bool hasTitleRequirement => !string.IsNullOrWhiteSpace(name);

    public override void PostProcess(LoreSettings settings)
    {
      if (string.IsNullOrEmpty(entryTypeName))
        throw new EmbeddedTypeNotGivenException(this);

      LoreTypeDefinition foundNodeType = settings.GetTypeDefinition(entryTypeName);

      if (foundNodeType == null)
        throw new EmbeddedTypeUnknownException(this, entryTypeName);
      else
        nodeType = foundNodeType;
    }
  }

  public static class DefinitionMergeManager
  {
    public static List<LoreFieldDefinition> MergeFields(List<LoreFieldDefinition> baseFields, List<LoreFieldDefinition> childFields)
    {
      if (baseFields == null && childFields == null) return null;

      if (baseFields == null && childFields != null) return childFields;

      if (baseFields != null && childFields == null) return baseFields;

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
    public static List<LoreSectionDefinition> MergeSections(List<LoreSectionDefinition> baseSections, List<LoreSectionDefinition> childSections)
    {
      if (baseSections == null && childSections == null) return null;

      if (baseSections == null && childSections != null) return childSections;

      if (baseSections != null && childSections == null) return baseSections;

      List<LoreSectionDefinition> ret = new List<LoreSectionDefinition>(baseSections);

      foreach (LoreSectionDefinition childSection in childSections)
      {
        // Look for a match, if this child field has a field of the same name described in the parent list.
        LoreSectionDefinition parentSectionIfDefined = ret.Find(f => f.name == childSection.name);

        if (parentSectionIfDefined != null)
        {
          childSection.fields = MergeFields(parentSectionIfDefined.fields, childSection.fields);
          childSection.sections = MergeSections(parentSectionIfDefined.sections, childSection.sections);
          childSection.MergeFrom(parentSectionIfDefined);
          ret.Replace(parentSectionIfDefined, childSection);
        }
        else
          ret.Add(childSection);

      }

      return ret;
    }
  }
}
