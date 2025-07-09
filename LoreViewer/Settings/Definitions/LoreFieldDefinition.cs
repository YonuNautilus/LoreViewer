using DocumentFormat.OpenXml.Drawing.Diagrams;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
    [Description("Picklist Options")]
    PickList = 3,
    [Description("Color")]
    Color = 4,
  }

  public enum EFieldContentType
  {
    [Description("String of text")]
    String = 0,
    [Description("Color (hex code)")]
    Color = 1,
    [Description("Number value (integer or floating point)")]
    Number = 2,
    [Description("Number with units")]
    Quantity = 3,
    [Description("Date")]
    Date = 4,
    [Description("Span of time")]
    Timespan = 5,
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

    [YamlMember(0)]
    [DefaultValue(false)]
    public bool required { get; set; }


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
      set
      {
        if (m_eStyle == EFieldStyle.NestedValues && m_eStyle != value)
        {
          Picklist = null;
          PicklistBranchConstraint = null;
        }
        m_eStyle = value;
      }
    }


    private EFieldContentType m_eContentType = EFieldContentType.String;
    [YamlMember(2)]
    [DefaultValue(EFieldContentType.String)]
    public EFieldContentType contentType
    {
      get
      {
        return m_eContentType;
      }
      set
      {
        m_eContentType = value;
      }
    }


    private string m_sPicklistName;

    [YamlMember(3)]
    [DefaultValue("")]
    public string picklistName
    {
      get
      {
        if (Picklist != null) return Picklist.name;
        else return m_sPicklistName;
      }
      set
      {
        m_sPicklistName = value;
      }
    }

    private string m_sPicklistBranchRestriction;

    [YamlMember(4)]
    [DefaultValue("")]
    public string picklistBranchRestriction
    {
      get
      {
        if (PicklistBranchConstraint != null) return PicklistBranchConstraint.name;
        else return m_sPicklistBranchRestriction;
      }
      set
      {
        m_sPicklistBranchRestriction = value;
      }
    }

    private LorePicklistDefinition m_oPicklist;
    [YamlIgnore]
    public LorePicklistDefinition Picklist
    {
      get
      {
        return this.m_oPicklist;
      }
      set
      {
        m_oPicklist = value;
        m_sPicklistName = value?.name;
        //if (m_oPicklist.isBranch) picklistBranchRestriction = m_oPicklist.name;
      }
    }

    private LorePicklistEntryDefinition m_oPicklistBranchConstraint;
    [YamlIgnore]
    public LorePicklistEntryDefinition PicklistBranchConstraint
    {
      get => m_oPicklistBranchConstraint;
      set
      {
        if (value != null && value != m_oPicklistBranchConstraint)
        {
          m_oPicklistBranchConstraint = value;
        }
        else
        {
          m_oPicklistBranchConstraint = null;
          picklistBranchRestriction = null;
        }
      }
    }


    [YamlIgnore]
    public bool multivalue => style == EFieldStyle.MultiValue;

    [YamlIgnore]
    public bool HasRequiredNestedFields => HasFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    [YamlIgnore]
    public bool HasOwnFields => HasFields ? fields.All(f => !f.IsInherited) : false;


    public override void PostProcess(LoreSettings settings)
    {
      if (style == EFieldStyle.PickList)
      {
        if (string.IsNullOrWhiteSpace(picklistName)) throw new FieldPicklistNameNotGivenException(this);

        // At this point, a Picklist name was given. Check if it is valid.
        if (settings.picklists.Any(p => p.name == picklistName))
        {
          Picklist = settings.picklists.FirstOrDefault(p => p.name == picklistName);

          // If there's a picklist branch constraint:
          if (Picklist.HasEntries && Picklist.entries.FlattenPicklistEntries().Any(p => p.name == picklistBranchRestriction))
            PicklistBranchConstraint = Picklist.entries.FlattenPicklistEntries().First(p => p.name == picklistBranchRestriction);
        }
        // NO Picklists found in definition
        else
        {
          throw new PicklistsDefinitionNotFoundException(this);
        }
      }
    }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreFieldDefinition).required) return true;

        if (this.style != (Base as LoreFieldDefinition).style) return true;

        if (this.HasFields)
          if (this.fields.Any(f => f.IsModifiedFromBase)) return true;

        if (this.style == EFieldStyle.PickList)
          if (this.picklistBranchRestriction != (Base as LoreFieldDefinition).picklistBranchRestriction) return true;

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

      // Notes on what inherited field styles can be to override their base.
      // inherited can be Multivalue or purely textual to override base's single value
      // Otherwise, inherited must match base type

      if(parentField.style == EFieldStyle.SingleValue)
      {
        if (style != EFieldStyle.MultiValue && style != EFieldStyle.Textual)
          style = EFieldStyle.SingleValue;
      }
      else
      {
        style = parentField.style;
      }

      // inherited fields always use parent's content type
      contentType = parentField.contentType;

      if (this.Picklist == null && parentField.Picklist != null)
        Picklist = parentField.Picklist;

      if (PicklistBranchConstraint == null && parentField.PicklistBranchConstraint != null)
        PicklistBranchConstraint = parentField.PicklistBranchConstraint;

      this.required |= parentField.required;
    }

    public LoreFieldDefinition Clone()
    {
      LoreFieldDefinition fieldDef = this.MemberwiseClone() as LoreFieldDefinition;
      fieldDef.fields = this.fields?.Select(f => f.Clone()).ToList();

      fieldDef.picklistName = this.picklistName;
      fieldDef.picklistBranchRestriction = this.picklistBranchRestriction;

      return fieldDef;
    }

    public LoreFieldDefinition CloneFromBase()
    {
      LoreFieldDefinition fieldDef = Clone();
      fieldDef.Base = this;

      if (fieldDef.style == EFieldStyle.NestedValues)
        fieldDef.fields = DefinitionMergeManager.MergeFields(fields, fieldDef.fields);

      return fieldDef;
    }

    internal override void MakeIndependent()
    {
      this.Base = null;

      if (HasFields) foreach (LoreFieldDefinition field in fields) field.MakeIndependent();
    }


    public List<string> GetPicklistOptions()
    {
      if (style != EFieldStyle.PickList) return new List<string>();

      List<string> ret = new();

      if (PicklistBranchConstraint != null)
        return PicklistBranchConstraint.entries.FlattenPicklistEntries().Select(e => e.name).ToList();

      else
        return Picklist.entries.FlattenPicklistEntries().Select(e => e.name).ToList();
    }
  }
}
