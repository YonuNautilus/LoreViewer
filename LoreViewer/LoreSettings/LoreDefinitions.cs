﻿using Avalonia.Dialogs;
using Avalonia.Styling;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using DocumentFormat.OpenXml.Spreadsheet;
using DynamicData;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace LoreViewer.Settings
{
  public enum EFieldStyle
  {
    [Description("Nested Fields")]
    NestedValues = -1,
    [Description("Single Value")]
    SingleValue = 0,
    [Description("Multiple Values")]
    MultiValue = 1,
    [Description("Purely Textual")]
    Textual = 2,
    [Description("List")]
    PickList = 3
  }

  public enum ECollectionMode
  {
    Nodes,
    Collections
  }

  public abstract class LoreDefinitionBase
  {
    [YamlIgnore]
    /// The definition this definition inherits from
    public LoreDefinitionBase Base { get; protected set; }

    [DefaultValue("")]
    [YamlMember(-100)]
    public string name { get; set; } = string.Empty;

    public LoreDefinitionBase() { }

    public LoreDefinitionBase(string name) { this.name = name; }

    public abstract void PostProcess(LoreSettings settings);

    public override string ToString() => name;

    [YamlIgnore]
    public bool processed = false;


    public abstract bool IsModifiedFromBase { get; }

  }

  /// <summary>
  /// The top-level of a lore definition. Can be used to define types like character, organization, race, etc.
  /// </summary>
  public class LoreTypeDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, ICollectionDefinitionContainer, IEmbeddedNodeDefinitionContainer, IDeepCopyable<LoreTypeDefinition>
  {
    #region IFieldDefinitionContainer implementation

    [YamlMember(1)]
    // for fields like Date with Start/End
    public List<LoreFieldDefinition> fields { get; set; }

    [YamlIgnore]
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields?.FirstOrDefault(f => f.name == fieldName) ?? null;

    public bool ShouldSerializefields() { return fields != null && fields.Any(); }
    #endregion IFieldDefinitionContainer implementation

    #region ISectionDefinitionContainer implementation
    [YamlMember(2)]
    public List<LoreSectionDefinition> sections { get; set; }
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections?.Any(sec => sectionName.Contains(sec.name)) ?? false;
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections?.FirstOrDefault(s => s.name == sectionName) ?? null;
    public bool ShouldSerializesections() { return sections != null && sections.Count > 0; }
    #endregion ISectionDefinitionContainer implementation

    #region ICollectionDefinitionContainer
    [YamlMember(3)]
    public List<LoreCollectionDefinition> collections { get; set; }
    public bool HasCollectionDefinition(string collectionName) => collections?.Any(col => col.name == collectionName) ?? false;
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections?.FirstOrDefault(c => c.name == collectionName) ?? null;
    #endregion ICollectionDefinitionContainer

    #region IEmbeddedNodeDefinitionContainer Implementation
    [YamlMember(4)]
    public List<LoreEmbeddedNodeDefinition> embeddedNodeDefs { get; set; }
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


    [YamlIgnore]
    public bool HasNestedNodes => embeddedNodeDefs != null && embeddedNodeDefs.Count() > 0;

    #endregion IEmbeddedNodeDefinitionContainer
    public bool AllowsEmbeddedType(LoreTypeDefinition typeDef) => HasTypeDefinition(typeDef) || this.IsParentOf(typeDef);

    [YamlIgnore]
    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null)
          return true;  // Always serialize fully-defined root fields

        if (fields.Any(f => f.IsModifiedFromBase)) return true;

        if (sections.Any(s => s.IsModifiedFromBase)) return true;

        if (collections.Any(c => c.IsModifiedFromBase)) return true;

        if (embeddedNodeDefs.Any(e => e.IsModifiedFromBase)) return true;

        return false;
      }
    }

    [YamlIgnore]
    private string m_sExtends = string.Empty;

    [YamlMember(0)]
    public string extends
    {
      get
      {
        if (ParentType == null)
        {
          if (string.IsNullOrEmpty(m_sExtends)) return null;
          else return m_sExtends;
        }
        else return ParentType.name;
      }
      set
      {
        m_sExtends = value;
      }
    }

    [YamlIgnore]
    public LoreTypeDefinition ParentType { get => Base as LoreTypeDefinition; set => Base = value; }

    [YamlIgnore]
    public bool isExtendedType => ParentType != null;

    [YamlIgnore]
    public bool HasRequiredEmbeddedNodes => HasNestedNodes ? embeddedNodeDefs.Aggregate(false, (sum, next) => sum || next.required || next.nodeType.HasRequiredEmbeddedNodes, r => r) : false;

    public void MergeFrom(LoreTypeDefinition type)
    {
      ParentType = type;

      fields = DefinitionMergeManager.MergeFields(ParentType.fields, fields);
      sections = DefinitionMergeManager.MergeSections(ParentType.sections, sections);
      collections = DefinitionMergeManager.MergeCollections(ParentType.collections, collections);
      embeddedNodeDefs = DefinitionMergeManager.MergeEmbeddedNodeDefs(ParentType.embeddedNodeDefs, embeddedNodeDefs);
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
      if (embeddedNodeDefs != null)
      {

        IEnumerable<LoreEmbeddedNodeDefinition> dupedTitles = embeddedNodeDefs?.Where(d => d.hasTitleRequirement).GroupBy(d => d.name).Where(c => c.Count() > 1).Select(name => name.First());
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
      }

      processed = true;

      if (collections != null)
        foreach (LoreCollectionDefinition colDef in collections)
          if (!colDef.processed)
            colDef.PostProcess(settings);
    }

    public LoreTypeDefinition Clone()
    {
      LoreTypeDefinition typeDef = this.MemberwiseClone() as LoreTypeDefinition;
      typeDef.fields = this.fields?.Select(f => f.Clone()).ToList();
      typeDef.sections = this.sections?.Select(s => s.Clone()).ToList();
      typeDef.collections = this.collections?.Select(c => c.Clone()).ToList();
      typeDef.embeddedNodeDefs = this.embeddedNodeDefs?.Select(e => e.Clone()).ToList();

      return typeDef;
    }
  }

  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, IRequirable, IDeepCopyable<LoreSectionDefinition>
  {

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; }
    [YamlIgnore]
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
    #endregion ISectionDefinitionContainer implementation

    #region IFieldDefinitionContainer implementation
    public List<LoreFieldDefinition> fields { get; set; }

    [YamlIgnore]
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

    #endregion IFieldDefinitionContainer implementation

    [DefaultValue(false)]
    public bool required { get; set; }

    [YamlIgnore]
    public bool HasRequiredNestedSections => HasSections ? sections.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedSections, r => r) : false;

    [DefaultValue(false)]
    public bool freeform { get; set; } = false;

    public LoreSectionDefinition() { }

    public LoreSectionDefinition(string name, bool isFreeForm) : base(name) { freeform = isFreeForm; }


    /// <summary>
    /// Takes base definition, adds info like nested fields from the parent and merges them into this child field definition
    /// </summary>
    /// <param name="parentSection"></param>
    public void MergeFrom(LoreSectionDefinition parentSection)
    {
      Base = parentSection;

      this.freeform |= parentSection.freeform;
    }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null)
          return true;  // Always serialize fully-defined root fields

        if (this.freeform != (Base as LoreSectionDefinition).freeform)
          return true;

        if (this.required != (Base as LoreSectionDefinition).required)
          return true;

        if (this.HasFields)
          if (this.fields.Any(f => f.IsModifiedFromBase)) return true;

        return false;
      }
    }


    public override void PostProcess(LoreSettings settings) { }

    public LoreSectionDefinition Clone()
    {
      LoreSectionDefinition typeDef = this.MemberwiseClone() as LoreSectionDefinition;
      typeDef.fields = this.fields?.Select(f => f.Clone()).ToList();
      typeDef.sections = this.sections?.Select(s => s.Clone()).ToList();
      //typeDef.Base = this;

      return typeDef;
    }
  }

  public class LoreFieldDefinition : LoreDefinitionBase, IFieldDefinitionContainer, IRequirable, IDeepCopyable<LoreFieldDefinition>
  {

    #region IFieldDefinitionContainer implementation
    public List<LoreFieldDefinition> fields { get; set; }

    [YamlIgnore]
    // for fields like Date with Start/End
    public bool HasFields => fields != null && fields.Count > 0;
    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));
    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);
    #endregion IFieldDefinitionContainer implementation

    private EFieldStyle m_eStyle = EFieldStyle.SingleValue;

    [YamlMember(1)]
    [DefaultValue(EFieldStyle.SingleValue)]
    public EFieldStyle style
    {
      get
      {
        if (HasFields) m_eStyle = EFieldStyle.NestedValues;
        return m_eStyle;
      }
      set => m_eStyle = value; 
    }

    [YamlMember(0)]
    [DefaultValue(false)]
    public bool required { get; set; }

    [YamlIgnore]
    public bool multivalue => style == EFieldStyle.MultiValue;

    [YamlIgnore]
    public bool HasRequiredNestedFields => HasFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    [YamlIgnore]
    public bool HasOwnFields => HasFields ? fields.All(f => !f.IsInherited) : false;

    [YamlIgnore]
    public bool IsInherited => Base != null;

    public override void PostProcess(LoreSettings settings) { }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreFieldDefinition).required) return true;

        if (this.style != (Base as LoreFieldDefinition).style) return true;

        if (this.HasFields)
          if (this.fields.Any(f => f.IsModifiedFromBase)) return true;

        return false;
      }
    }

    /// <summary>
    /// Takes base definition, adds info like nested fields from the parent and merges them into this child field definition
    /// </summary>
    /// <param name="parentField"></param>
    public void MergeFrom(LoreFieldDefinition parentField)
    {
      Base = parentField;

      this.required |= parentField.required;
    }

    public LoreFieldDefinition Clone()
    {
      LoreFieldDefinition fieldDef = this.MemberwiseClone() as LoreFieldDefinition;
      fieldDef.fields = this.fields?.Select(f => f.Clone()).ToList();
      //fieldDef.Base = this;

      return fieldDef;
    }
  }

  public class LoreCollectionDefinition : LoreDefinitionBase, IRequirable, IDeepCopyable<LoreCollectionDefinition>
  {
    [DefaultValue("")]
    public string entryTypeName { get; set; } = string.Empty;

    public LoreCollectionDefinition entryCollection { get; set; }

    [YamlIgnore]
    public LoreDefinitionBase ContainedType { get; set; }

    [DefaultValue(false)]
    public bool SortEntries { get; set; }

    [DefaultValue(false)]
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

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreCollectionDefinition).required) return true;

        return false;
      }
    }

    public void MergeFrom(LoreCollectionDefinition parentCollection)
    {
      Base = parentCollection;

    }

    public LoreCollectionDefinition Clone()
    {
      // Keep ContainedType as a reference
      LoreCollectionDefinition colDef = this.MemberwiseClone() as LoreCollectionDefinition;
      colDef.entryCollection = this.entryCollection?.Clone();

      return colDef;
    }
  }

  public class LoreEmbeddedNodeDefinition : LoreDefinitionBase, IRequirable, IDeepCopyable<LoreEmbeddedNodeDefinition>
  {
    private string m_sEntryTypeName = string.Empty;
    public string entryTypeName
    {
      get
      {
        if (nodeType == null) return m_sEntryTypeName;
        else return nodeType.name;
      }
      set => m_sEntryTypeName = value;
    }

    [DefaultValue(false)]
    public bool required { get; set; }

    [YamlIgnore]
    public LoreTypeDefinition nodeType { get; set; }

    [YamlIgnore]
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

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreEmbeddedNodeDefinition).required) return true;

        return false;
      }
    }

    public void MergeFrom(LoreEmbeddedNodeDefinition parentEmdedded)
    {
      Base = parentEmdedded;

    }

    public LoreEmbeddedNodeDefinition Clone()
    {
      LoreEmbeddedNodeDefinition emNodeDef = this.MemberwiseClone() as LoreEmbeddedNodeDefinition;
      emNodeDef.Base = this;

      return emNodeDef;
    }
  }

  public static class DefinitionMergeManager
  {
    public static List<LoreFieldDefinition> MergeFields(List<LoreFieldDefinition> baseFields, List<LoreFieldDefinition> childFields)
    {
      if (baseFields == null && childFields == null) return null;

      if (baseFields == null && childFields != null) return childFields;

      if (baseFields != null && childFields == null) return baseFields = baseFields.Select(f => f.Clone()).ToList();

      List<LoreFieldDefinition> ret = new();

      string[] _allFieldNames = baseFields.Select(f => f.name).Concat(childFields.Select(f => f.name)).Distinct().ToArray();


      foreach (string fieldName in _allFieldNames)
      {

        LoreFieldDefinition resultingField = null;
        LoreFieldDefinition childField = childFields.FirstOrDefault(f => f.name == fieldName);
        LoreFieldDefinition baseField = baseFields.FirstOrDefault(f => f.name == fieldName);

        if (childField != null && baseField != null)
        {
          resultingField = childField;
          resultingField.MergeFrom(baseField);
          resultingField.fields = MergeFields(baseField.fields, resultingField.fields);
        }
        else if (childField == null && baseField != null)
        {
          resultingField = baseField.Clone() as LoreFieldDefinition;
          resultingField.MergeFrom(baseField);
        }
        else if (childField != null && baseField == null)
        {
          resultingField = childField;
        }

        ret.Add(resultingField);
      }

      return ret;
    }
    public static List<LoreSectionDefinition> MergeSections(List<LoreSectionDefinition> baseSections, List<LoreSectionDefinition> childSections)
    {
      if (baseSections == null && childSections == null) return null;

      if (baseSections == null && childSections != null) return childSections;

      if (baseSections != null && childSections == null) return baseSections.Select(s => s.Clone() as LoreSectionDefinition).ToList();

      List<LoreSectionDefinition> ret = new List<LoreSectionDefinition>();

      string[] _allSectionNames = baseSections.Select(f => f.name).Concat(childSections.Select(f => f.name)).Distinct().ToArray();

      foreach (string sectionName in _allSectionNames)
      {

        LoreSectionDefinition resultingSection = null;
        LoreSectionDefinition childSection = childSections.FirstOrDefault(s => s.name == sectionName);
        LoreSectionDefinition baseSection = baseSections.FirstOrDefault(s => s.name == sectionName);

        if (childSection != null && baseSection != null)
        {
          resultingSection = childSection;
          resultingSection.MergeFrom(baseSection);
          resultingSection.fields = MergeFields(baseSection.fields, resultingSection.fields);
          resultingSection.sections = MergeSections(baseSection.sections, resultingSection.sections);
        }
        else if (childSection == null && baseSection != null)
        {
          resultingSection = baseSection.Clone() as LoreSectionDefinition;
          resultingSection.MergeFrom(baseSection);
        }
        else if (childSection != null && baseSection == null)
        {
          resultingSection = childSection;
        }

        ret.Add(resultingSection);
      }

      return ret;
    }

    internal static List<LoreCollectionDefinition>? MergeCollections(List<LoreCollectionDefinition>? baseCols, List<LoreCollectionDefinition>? childCols)
    {
      if (baseCols == null && childCols == null) return null;

      if (baseCols == null && childCols != null) return childCols;

      if (baseCols != null && childCols == null) return baseCols.Select(c => c.Clone() as LoreCollectionDefinition).ToList();

      List<LoreCollectionDefinition> ret = new List<LoreCollectionDefinition>();

      string[] _allCollectionNames = baseCols.Select(f => f.name).Concat(childCols.Select(f => f.name)).Distinct().ToArray();

      foreach (string collectionName in _allCollectionNames)
      {

        LoreCollectionDefinition resultingCollection = null;
        LoreCollectionDefinition childCollection = childCols.FirstOrDefault(c => c.name == collectionName);
        LoreCollectionDefinition baseCollection = baseCols.FirstOrDefault(c => c.name == collectionName);

        if (childCollection != null && baseCollection != null)
        {
          resultingCollection = childCollection;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection == null && baseCollection != null)
        {
          resultingCollection = baseCollection.Clone() as LoreCollectionDefinition;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection != null && baseCollection == null)
        {
          resultingCollection = childCollection;
        }

        ret.Add(resultingCollection);
      }

      return ret;
    }

    internal static List<LoreEmbeddedNodeDefinition> MergeEmbeddedNodeDefs(List<LoreEmbeddedNodeDefinition> baseEmbds, List<LoreEmbeddedNodeDefinition> childEmbds)
    {
      if (baseEmbds == null && childEmbds == null) return null;

      if (baseEmbds == null && childEmbds != null) return childEmbds;

      if (baseEmbds != null && childEmbds == null) return baseEmbds.Select(e => e.Clone() as LoreEmbeddedNodeDefinition).ToList();

      List<LoreEmbeddedNodeDefinition> ret = new List<LoreEmbeddedNodeDefinition>();

      string[] _allCollectionNames = baseEmbds.Select(f => f.name).Concat(childEmbds.Select(f => f.name)).Distinct().ToArray();

      foreach (string collectionName in _allCollectionNames)
      {

        LoreEmbeddedNodeDefinition resultingCollection = null;
        LoreEmbeddedNodeDefinition childCollection = childEmbds.FirstOrDefault(e => e.name == collectionName);
        LoreEmbeddedNodeDefinition baseCollection = baseEmbds.FirstOrDefault(e => e.name == collectionName);

        if (childCollection != null && baseCollection != null)
        {
          resultingCollection = childCollection;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection == null && baseCollection != null)
        {
          resultingCollection = baseCollection.Clone() as LoreEmbeddedNodeDefinition;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection != null && baseCollection == null)
        {
          resultingCollection = childCollection;
        }

        ret.Add(resultingCollection);
      }

      return ret;
    }
  }
}
