using LoreViewer.Exceptions.SettingsParsingExceptions;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Domain.Settings.Definitions
{
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
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections?.Any(sec => sectionName.TrimEmoji().Equals(sec.name)) ?? false;
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections?.FirstOrDefault(s => s.name == sectionName.TrimEmoji()) ?? null;
    public bool ShouldSerializesections() { return sections != null && sections.Count > 0; }
    #endregion ISectionDefinitionContainer implementation

    #region ICollectionDefinitionContainer
    [YamlMember(3)]
    public List<LoreCollectionDefinition> collections { get; set; } = new List<LoreCollectionDefinition>();
    public bool HasCollections => collections != null && collections.Count > 0;
    public bool HasCollectionDefinition(string collectionName) => collections?.Any(col => col.name == collectionName) ?? false;
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections?.FirstOrDefault(c => c.name == collectionName) ?? null;
    #endregion ICollectionDefinitionContainer

    #region IEmbeddedNodeDefinitionContainer Implementation
    [YamlMember(4)]
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


    [YamlIgnore]
    public bool HasNestedNodes => embeddedNodeDefs != null && embeddedNodeDefs.Count() > 0;

    #endregion IEmbeddedNodeDefinitionContainer
    public bool AllowsEmbeddedType(LoreTypeDefinition typeDef) => HasTypeDefinition(typeDef) || IsParentOf(typeDef);

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
      collections = DefinitionMergeManager.MergeCollections(ParentType.collections, collections, this);
      embeddedNodeDefs = DefinitionMergeManager.MergeEmbeddedNodeDefs(ParentType.embeddedNodeDefs, embeddedNodeDefs);
    }

    public bool IsParentOf(LoreTypeDefinition subTypeDef) => subTypeDef.isExtendedType && (subTypeDef.ParentType == this || IsParentOf(subTypeDef.ParentType));
    public bool IsATypeOf(LoreTypeDefinition parentTypeDef) => this == parentTypeDef || (parentTypeDef?.IsParentOf(this) ?? false);

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

      if (fields != null)
        foreach (LoreFieldDefinition fDef in fields)
          fDef.PostProcess(settings);

      if (collections != null)
        foreach (LoreCollectionDefinition colDef in collections)
        {
          colDef.OwningDefinition = this;
          colDef.PostProcess(settings);
        }
    }

    public LoreTypeDefinition Clone()
    {
      LoreTypeDefinition typeDef = MemberwiseClone() as LoreTypeDefinition;
      typeDef.fields = fields?.Select(f => f.Clone()).ToList();
      typeDef.sections = sections?.Select(s => s.Clone()).ToList();
      typeDef.collections = collections?.Select(c => c.Clone()).ToList();
      typeDef.embeddedNodeDefs = embeddedNodeDefs?.Select(e => e.Clone()).ToList();

      return typeDef;
    }

    public LoreTypeDefinition CloneFromBase()
    {
      LoreTypeDefinition typeDef = Clone();
      typeDef.Base = this;
      return typeDef;
    }

    internal override void MakeIndependent()
    {
      Base = null;
      ParentType = null;
      extends = null;

      if (HasFields) foreach (LoreFieldDefinition field in fields) field.MakeIndependent();

      if (HasSections) foreach (LoreSectionDefinition section in sections) section.MakeIndependent();

      if (HasCollections) foreach (LoreCollectionDefinition collection in collections) collection.MakeIndependent();

      if (HasNestedNodes) foreach (LoreEmbeddedNodeDefinition embedded in embeddedNodeDefs) embedded.MakeIndependent();
    }
  }

}
