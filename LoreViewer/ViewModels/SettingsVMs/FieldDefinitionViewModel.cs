using DocumentFormat.OpenXml.Drawing;
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

    public static List<EFieldStyle> FieldStyles { get => Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().ToList(); }

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
    
    public bool IsNestedFieldsStyle { get => fieldDef.style == EFieldStyle.NestedValues; }

    public bool IsPicklistFieldStyle { get => fieldDef.style == EFieldStyle.PickList; }

    public bool HasPicklistSelected { get => Picklist != null; }
    public bool HasRestrictionSelected { get => PicklistBranchRestriction != null; }

    public bool HasSubFields { get => fieldDef.HasFields; }
    public bool NoSubFields { get => !fieldDef.HasFields; }

    public bool CanEditStyle
    {
      get
      {
        // Don't allow style changing if there are subfields
        // If there are subfields, style should be locked at NestedFields
        if (fieldDef.HasFields)
          return false;

        // Whether inherited or not, as long as no subfields, user can edit the style.
        return true;
      }
    }


    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get =>  m_cFields; }

    public EFieldStyle Style
    {
      get => fieldDef.style;
      set
      {
        if(fieldDef.style == EFieldStyle.PickList && value != EFieldStyle.PickList)
        {
          fieldDef.Picklist = null;
        }
        fieldDef.style = value;
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

    public PicklistEntryDefinitionViewModel PicklistBranchRestriction
    {
      get
      {
        return Picklist?.GetBranch(fieldDef.PicklistBranchConstraint);
      }
      set
      {
        fieldDef.PicklistBranchConstraint = value?.pickEntryDef;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public override void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(Style));
      this.RaisePropertyChanged(nameof(IsNestedFieldsStyle));
      this.RaisePropertyChanged(nameof(IsPicklistFieldStyle));
      this.RaisePropertyChanged(nameof(HasSubFields));
      this.RaisePropertyChanged(nameof(NoSubFields));
      this.RaisePropertyChanged(nameof(TooltipText));
      this.RaisePropertyChanged(nameof(CanEditStyle));
      this.RaisePropertyChanged(nameof(Picklist));
      this.RaisePropertyChanged(nameof(Picklists));
      this.RaisePropertyChanged(nameof(PicklistBranchRestriction));
      this.RaisePropertyChanged(nameof(ValidPickListBranchChoices));
      this.RaisePropertyChanged(nameof(HasPicklistSelected));
      this.RaisePropertyChanged(nameof(HasRestrictionSelected));
      base.RefreshUI();
    }

    public string TooltipText
    {
      get
      {
        if (Style == EFieldStyle.NestedValues && HasSubFields)
          return "Cannot change field style from Nested Values until this field's nested values are cleared.\r\nIf this field is inherited, the parent's nested fields must be cleared.";
        return null;
      }
    }

    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion)
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
          m_cFields.Add(new FieldDefinitionViewModel(def));
    }

    public override void RefreshLists()
    {
      RefreshFieldDefs();
    }


  }
}
