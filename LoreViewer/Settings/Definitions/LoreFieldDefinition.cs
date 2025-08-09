using DocumentFormat.OpenXml.Drawing.Diagrams;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LoreViewer.Settings
{
  public enum EFieldCardinality
  {
    [Description("Single Value")]
    SingleValue = 0,
    [Description("Multiple Values")]
    MultiValue = 1,
  }

  public enum EFieldContentType
  {
    [Description("String of text")]
    String = 0,
    [Description("Color (hex code)")]
    Color = 1,
    [Description("Numeric value")]
    Number = 2,
    [Description("Numeric value with units")]
    Quantity = 3,
    [Description("Date/Time")]
    Date = 4,
    [Description("Range of Dates/Times")]
    DateRange = 5,
    [Description("List of Preset Values")]
    PickList = 6,
    [Description("List of Objects")]
    ReferenceList = 7,
  }

  public enum EFieldInputStructure
  {
    [Description("Normal (Default)")]
    Normal = 0,
    [Description("Purely Textual")]
    Textual = 1,
    [Description("Nested Fields")]
    NestedValues = 2,
  }

  public enum ENumericType
  {
    [Description("Natural (positive integer)")]
    Natural,
    [Description("Integer")]
    Integer,
    [Description("Fractional")]
    Float,
  }

  public enum EQuantityUnitType
  {
    Length,
    [Description("Mass/Weight")]
    Mass,
    Velocity,
    Acceleration,
    [Description("Time duration")]
    TimeDuration,
    Angle,
    //Coordinates,
    Temperature,
    Pressure,
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


    private EFieldInputStructure m_eStructure = EFieldInputStructure.Normal;

    /// <summary>
    /// Input structure overrides any contentType or Cardinality if it is a value other than Normal
    /// </summary>
    [YamlMember(1)]
    [DefaultValue(EFieldInputStructure.Normal)]
    public EFieldInputStructure structure
    {
      get
      {
        if (HasFields) m_eStructure = EFieldInputStructure.NestedValues;
        return m_eStructure;
      }
      set
      {
        if (m_eStructure != EFieldInputStructure.Normal && m_eStructure != value)
        {
          Picklist = null;
          PicklistBranchConstraint = null;
          RefListType = null;
          reflistTypeName = string.Empty;

          contentType = EFieldContentType.String;
          cardinality = EFieldCardinality.SingleValue;
        }
        m_eStructure = value;
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


    private EFieldCardinality m_eCardinality = EFieldCardinality.SingleValue;

    [YamlMember(3)]
    [DefaultValue(EFieldCardinality.SingleValue)]
    public EFieldCardinality cardinality
    {
      get
      {
        return m_eCardinality;
      }
      set
      {
        m_eCardinality = value;
      }
    }


    private string m_sPicklistName;

    [YamlMember(4)]
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

    [YamlMember(5)]
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


    private ENumericType m_eNumericType = ENumericType.Natural;
    [YamlMember(6)]
    [DefaultValue(ENumericType.Natural)]
    public ENumericType numericType
    {
      get { return m_eNumericType; }
      set { m_eNumericType = value; }
    }

    private EQuantityUnitType m_eQuantityType = EQuantityUnitType.Length;
    [YamlMember(7)]
    [DefaultValue(EQuantityUnitType.Length)]
    public EQuantityUnitType quantityUnitType
    {
      get { return m_eQuantityType; }
      set { m_eQuantityType = value; }
    }



    private LoreTypeDefinition m_oRefListType;
    [YamlIgnore]
    public LoreTypeDefinition RefListType
    {
      get
      {
        return this.m_oRefListType;
      }
      set
      {
        m_oRefListType = value;
        m_sReferenceListTypeName = value?.name;
      }
    }

    private string m_sReferenceListTypeName;

    [YamlMember(3)]
    [DefaultValue("")]
    public string reflistTypeName
    {
      get
      {
        if (RefListType != null) return RefListType.name;
        else return m_sReferenceListTypeName;
      }
      set
      {
        m_sReferenceListTypeName = value;
      }
    }


    [YamlIgnore]
    public bool multivalue => cardinality == EFieldCardinality.MultiValue;

    [YamlIgnore]
    public bool HasRequiredNestedFields => HasFields ? fields.Aggregate(false, (sum, next) => sum || next.required || next.HasRequiredNestedFields, r => r) : false;

    [YamlIgnore]
    public bool HasOwnFields => HasFields ? fields.All(f => !f.IsInherited) : false;


    public override void PostProcess(LoreSettings settings)
    {
      if (contentType == EFieldContentType.PickList)
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
      else if (contentType == EFieldContentType.ReferenceList)
      {
        if (string.IsNullOrWhiteSpace(reflistTypeName)) throw new FieldRefListNameNotGivenException(this);

        if (settings.types.Any(t => t.name == reflistTypeName))
        {
          RefListType = settings.types.FirstOrDefault(t => t.name == reflistTypeName);
        }
        else
        {
          throw new ReferenceListTypeNotFoundException(this);
        }
      }
      else if (structure == EFieldInputStructure.NestedValues || structure == EFieldInputStructure.Textual)
      {
        contentType = EFieldContentType.String;
        cardinality = EFieldCardinality.SingleValue;

        if(structure == EFieldInputStructure.NestedValues && HasFields)
          foreach (LoreFieldDefinition lfd in fields)
            lfd.PostProcess(settings);
      }
    }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreFieldDefinition).required) return true;

        if (this.structure != (Base as LoreFieldDefinition).structure) return true;

        if (this.HasFields)
          if (this.fields.Any(f => f.IsModifiedFromBase)) return true;

        if (this.contentType == EFieldContentType.PickList)
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

      // inherited fields always use parent's content type
      contentType = parentField.contentType;
      cardinality = parentField.cardinality;
      structure = parentField.structure;

      if (this.Picklist == null && parentField.Picklist != null)
        Picklist = parentField.Picklist;

      if (PicklistBranchConstraint == null && parentField.PicklistBranchConstraint != null)
        PicklistBranchConstraint = parentField.PicklistBranchConstraint;

      if(this.RefListType == null && parentField.RefListType != null)
        this.RefListType = parentField.RefListType;

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

      if (fieldDef.structure == EFieldInputStructure.NestedValues)
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
      if (contentType != EFieldContentType.PickList) return new List<string>();

      List<string> ret = new();

      if (PicklistBranchConstraint != null)
        return PicklistBranchConstraint.entries.FlattenPicklistEntries().Select(e => e.name).ToList();

      else
        return Picklist.entries.FlattenPicklistEntries().Select(e => e.name).ToList();
    }
  }
}
