using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LoreViewer.Settings
{
  /// <summary>
  /// A section of information owned by a lore object. A section can contain subsections or additional fields
  /// </summary>
  public class LoreSectionDefinition : LoreDefinitionBase, ISectionDefinitionContainer, IFieldDefinitionContainer, IRequirable, IDeepCopyable<LoreSectionDefinition>
  {

    #region ISectionDefinitionContainer implementation
    public List<LoreSectionDefinition> sections { get; set; } = new List<LoreSectionDefinition>();
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

    public LoreSectionDefinition CloneFromBase()
    {
      LoreSectionDefinition secDef = Clone();
      secDef.Base = this;
      return secDef;
    }

    internal override void MakeIndependent()
    {
      this.Base = null;

      if (HasFields) foreach (LoreFieldDefinition field in fields) field.MakeIndependent();

      if (HasSections) foreach (LoreSectionDefinition section in sections) section.MakeIndependent();
    }
  }

}
