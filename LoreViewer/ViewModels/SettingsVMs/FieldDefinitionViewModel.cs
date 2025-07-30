using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using LoreViewer.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class FieldDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    public override ObservableCollection<PicklistEntryDefinitionViewModel> PicklistEntries => null;
    #endregion

    public ReactiveCommand<Unit, Unit> ClearUsedPicklistConstraintCommand { get; }

    public ObservableCollection<TypeDefinitionViewModel> TypesInSettings { get => CurrentSettingsViewModel.Types; }

    public List<EFieldContentType> ContentTypes { get => Enum.GetValues(typeof(EFieldContentType)).Cast<EFieldContentType>().ToList(); }
    public List<EFieldCardinality> FieldCardinalities { get => Enum.GetValues(typeof(EFieldCardinality)).Cast<EFieldCardinality>().ToList(); }
    public List<EFieldInputStructure> InputStructures { get => Enum.GetValues(typeof(EFieldInputStructure)).Cast<EFieldInputStructure>().ToList(); }
    public List<ENumericType> NumericTypes { get => Enum.GetValues(typeof(ENumericType)).Cast<ENumericType>().ToList(); }
    public List<EQuantityUnitType> QuantityTypes { get => Enum.GetValues(typeof(EQuantityUnitType)).Cast<EQuantityUnitType>().ToList(); }

    public override ObservableCollection<PicklistDefinitionViewModel> Picklists { get => CurrentSettingsViewModel.Picklists; }

    public ObservableCollection<PicklistEntryDefinitionViewModel> ValidPickListBranchChoices { get => Picklist?.ValidBranchRestrictionChoices; }

    private LoreFieldDefinition fieldDef { get => Definition as LoreFieldDefinition; }
    public bool IsRequired
    {
      get => fieldDef.required;
      set
      {
        fieldDef.required = value;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public bool IsNestedFieldsStructure { get => fieldDef.structure == EFieldInputStructure.NestedValues; }
    public bool IsNotNestedFieldsStructure { get => !IsNestedFieldsStructure; }

    public bool IsPicklistContentType { get => fieldDef.contentType == EFieldContentType.PickList; }
    public bool IsReferencelistContentType { get => fieldDef.contentType == EFieldContentType.ReferenceList; }

    public bool IsNumericContentType { get => fieldDef.contentType == EFieldContentType.Number; }

    public bool HasPicklistSelected { get => Picklist != null; }
    public bool HasRestrictionSelected { get => PicklistBranchRestriction != null; }

    public bool HasRefListTypeSelected { get => RefListType != null; }

    public bool HasSubFields { get => fieldDef.HasFields; }
    public bool NoSubFields { get => !fieldDef.HasFields; }

    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get => m_cFields; }

    public EFieldInputStructure InputStructure
    {
      get { return fieldDef.structure; }
      set
      {
        if (fieldDef.structure != value)
        {
          fieldDef.structure = value;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }
    }


    public EFieldContentType ContentType
    {
      get { return fieldDef.contentType; }
      set
      {
        if (value == null) return;
        if (fieldDef.contentType != value)
        {
          fieldDef.contentType = value;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }
    }

    public EFieldCardinality Cardinality
    {
      get { return fieldDef.cardinality; }
      set
      {
        if (fieldDef.cardinality != value)
        {
          fieldDef.cardinality = value;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }
    }

    public ENumericType NumericType
    {
      get { return fieldDef.numericType; }
      set
      {
        if(fieldDef.numericType != value)
        {
          fieldDef.numericType = value;
          SettingsRefresher.Apply(CurrentSettingsViewModel) ;
        }
      }
    }

    private TypeDefinitionViewModel m_oRefListType;
    public TypeDefinitionViewModel RefListType
    {
      get
      {
        if(IsReferencelistContentType && m_oRefListType == null)
          m_oRefListType = CurrentSettingsViewModel.Types.FirstOrDefault(tvm => tvm.typeDef == fieldDef.RefListType);
        return m_oRefListType;
      }
      set
      {
        m_oRefListType = value;
        fieldDef.RefListType = value?.typeDef;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }


    private PicklistDefinitionViewModel m_oPicklist;
    public PicklistDefinitionViewModel Picklist
    {
      get
      {
        return m_oPicklist;
      }
      set
      {
        m_oPicklist = value;
        fieldDef.Picklist = value?.pickDef;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    private PicklistEntryDefinitionViewModel m_oPicklistConstraint;
    public PicklistEntryDefinitionViewModel PicklistBranchRestriction
    {
      get
      {
        return m_oPicklistConstraint;
      }
      set
      {
        m_oPicklistConstraint = value;
        fieldDef.PicklistBranchConstraint = value?.pickEntryDef;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public bool CanBreakPicklistBranchConstraint
    {
      get
      {
        if (m_oPicklistConstraint == null) return false;
        if (ValidPickListBranchChoices.Count() == 0) return false;
        return true;
      }
    }


    public override void RefreshUI()
    {
      if (m_oPicklist == null && fieldDef.Picklist != null)
      {
        m_oPicklist = CurrentSettingsViewModel.Picklists.FirstOrDefault(pl => pl.Definition == fieldDef.Picklist);
      }

      if ((m_oPicklistConstraint != null && m_oPicklistConstraint.Definition != fieldDef.PicklistBranchConstraint) || (m_oPicklistConstraint == null && fieldDef.PicklistBranchConstraint != null))
        if (ValidPickListBranchChoices != null)
          m_oPicklistConstraint = ValidPickListBranchChoices.FirstOrDefault(vc => vc.Definition == fieldDef.PicklistBranchConstraint);

      this.RaisePropertyChanged(nameof(InputStructure));
      this.RaisePropertyChanged(nameof(ContentType));
      this.RaisePropertyChanged(nameof(FieldCardinalities));
      this.RaisePropertyChanged(nameof(IsNestedFieldsStructure));
      this.RaisePropertyChanged(nameof(IsPicklistContentType));
      this.RaisePropertyChanged(nameof(IsReferencelistContentType));
      this.RaisePropertyChanged(nameof(HasSubFields));
      this.RaisePropertyChanged(nameof(NoSubFields));
      this.RaisePropertyChanged(nameof(TooltipText));
      this.RaisePropertyChanged(nameof(RefListType));
      this.RaisePropertyChanged(nameof(Picklist));
      this.RaisePropertyChanged(nameof(Picklists));
      this.RaisePropertyChanged(nameof(PicklistBranchRestriction));
      this.RaisePropertyChanged(nameof(ValidPickListBranchChoices));
      this.RaisePropertyChanged(nameof(HasPicklistSelected));
      this.RaisePropertyChanged(nameof(HasRestrictionSelected));
      this.RaisePropertyChanged(nameof(CanBreakPicklistBranchConstraint));

      base.RefreshUI();
    }

    public string TooltipText
    {
      get
      {
        if (InputStructure == EFieldInputStructure.NestedValues && HasSubFields)
          return "Cannot change field style from Nested Values until this field's nested values are cleared.\r\nIf this field is inherited, the parent's nested fields must be cleared.";
        return null;
      }
    }

    public FieldDefinitionViewModel(LoreFieldDefinition defintion, LoreSettingsViewModel curSettingsVM) : base(defintion, curSettingsVM)
    {
      ClearUsedPicklistConstraintCommand = ReactiveCommand.Create(ClearUsedPicklistConstraint);
    }

    private void ClearUsedPicklistConstraint()
    {
      PicklistBranchRestriction = null;
    }

    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if (fieldDef.fields != null)
        foreach (LoreFieldDefinition def in fieldDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def, CurrentSettingsViewModel));
    }

    public override void BuildLists()
    {
      RefreshFieldDefs();
    }
  }
}
