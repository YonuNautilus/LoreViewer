using LoreViewer.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class FieldDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    #endregion

    public static List<EFieldStyle> FieldStyles { get => Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().ToList(); }

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
        fieldDef.style = value;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public override void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(Style));
      this.RaisePropertyChanged(nameof(IsNestedFieldsStyle));
      this.RaisePropertyChanged(nameof(HasSubFields));
      this.RaisePropertyChanged(nameof(NoSubFields));
      this.RaisePropertyChanged(nameof(TooltipText));
      this.RaisePropertyChanged(nameof(CanEditStyle));
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

    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion) { }

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
