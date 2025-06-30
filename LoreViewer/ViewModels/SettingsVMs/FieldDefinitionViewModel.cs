using LoreViewer.Settings;
using ReactiveUI;
using System;
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

    private ObservableCollection<SelectableFieldStyleViewModel> m_oSelectableStyles;
    public ObservableCollection<SelectableFieldStyleViewModel> FieldStyles { get => m_oSelectableStyles; }

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

        if (fieldDef.IsInherited)
        {
          if ((fieldDef.Base as LoreFieldDefinition).style == EFieldStyle.SingleValue)
            return true;
          else
            return false;
        }

        // Whether inherited or not, as long as no subfields, user can edit the style.
        return true;
      }
    }


    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get =>  m_cFields; }

    public EFieldStyle Style
    {
      get { return fieldDef.style; }
    }



    private SelectableFieldStyleViewModel? m_oSelectedFieldStyle;
    public SelectableFieldStyleViewModel? SelectedFieldStyle
    {
      get
      {
        return m_oSelectedFieldStyle;
      }

      set
      {
        if (value == null)
          return;

        if (m_oSelectedFieldStyle != value)
        {
          m_oSelectedFieldStyle = value;
          fieldDef.style = value.Style;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
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
      if(m_oPicklist == null && fieldDef.Picklist != null)
      {
        m_oPicklist = CurrentSettingsViewModel.Picklists.FirstOrDefault(pl => pl.Definition == fieldDef.Picklist);
      }

      if (m_oSelectedFieldStyle.Style != Style) m_oSelectedFieldStyle = m_oSelectableStyles.FirstOrDefault(s => s.Style == Style);

      if ((m_oPicklistConstraint != null && m_oPicklistConstraint.Definition != fieldDef.PicklistBranchConstraint) || (m_oPicklistConstraint == null && fieldDef.PicklistBranchConstraint != null))
        m_oPicklistConstraint = ValidPickListBranchChoices.FirstOrDefault(vc => vc.Definition == fieldDef.PicklistBranchConstraint);

      this.RaisePropertyChanged(nameof(Style));
      this.RaisePropertyChanged(nameof(FieldStyles));
      this.RaisePropertyChanged(nameof(SelectedFieldStyle));
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
      this.RaisePropertyChanged(nameof(CanBreakPicklistBranchConstraint));

      foreach (SelectableFieldStyleViewModel fstylevm in m_oSelectableStyles)
        fstylevm.RefreshUI();

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

    public FieldDefinitionViewModel(LoreFieldDefinition defintion, LoreSettingsViewModel curSettingsVM) : base(defintion, curSettingsVM)
    {
      ClearUsedPicklistConstraintCommand = ReactiveCommand.Create(ClearUsedPicklistConstraint);
      var temp = Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().Select(fs => new SelectableFieldStyleViewModel(fs, this));
      m_oSelectableStyles = new ObservableCollection<SelectableFieldStyleViewModel>(Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().Select(fs => new SelectableFieldStyleViewModel(fs, this)));
      m_oSelectedFieldStyle = m_oSelectableStyles.FirstOrDefault(m => m.Style == fieldDef.style);
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


  public class SelectableFieldStyleViewModel : ViewModelBase
  {
    FieldDefinitionViewModel m_oParentVM;

    public EFieldStyle Style { get; }
    public bool IsAllowed
    {
      get
      {
        if (m_oParentVM.IsInherited)
        {
          if ((m_oParentVM.Definition.Base as LoreFieldDefinition).style == EFieldStyle.SingleValue)
          {
            switch (Style)
            {
              case EFieldStyle.SingleValue:
              case EFieldStyle.MultiValue:
              case EFieldStyle.Textual:
                return true;
              default: return false;
            }
          }
          else return (m_oParentVM.Definition.Base as LoreFieldDefinition).style == m_oParentVM.Style;
        }
        return true;
      }
    }

    public SelectableFieldStyleViewModel(EFieldStyle style, FieldDefinitionViewModel parentVM)
    {
      Style = style;
      m_oParentVM = parentVM;

      // Subscribe to parent ViewModel changes that affect IsAllowed
      m_oParentVM.WhenAnyValue(
          vm => vm.Style,
          vm => vm.IsInherited
      ).Subscribe(_ => this.RaisePropertyChanged(nameof(IsAllowed)));
    }

    public void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(IsAllowed));
    }
  }
}
