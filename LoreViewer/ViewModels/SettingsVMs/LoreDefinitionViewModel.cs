using LoreViewer.Settings;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.SettingsVMs
{
  /// <summary>
  /// A view model for a node of the TreeDataGrid in the Lore settings editor dialog.
  /// Exposes common properties on the contained DefinitionViewModel, like name and inheritance.
  /// </summary>
  public abstract class LoreDefinitionViewModel : ViewModelBase
  {
    public abstract ObservableCollection<TypeDefinitionViewModel> Types { get; }
    public abstract ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public abstract ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public abstract ObservableCollection<CollectionDefinitionViewModel> Collections { get; }
    public abstract ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }
    public abstract ObservableCollection<PicklistDefinitionViewModel> Picklists { get; }
    public abstract ObservableCollection<PicklistEntryDefinitionViewModel> PicklistEntries { get; }


    public LoreDefinitionBase Definition { get; }
    public LoreSettingsViewModel CurrentSettingsViewModel { get; }

    public Guid UniqueID { get => Guid.NewGuid(); }

    public bool IsDeletable
    {
      get
      {
        if(Definition != null) return !Definition.IsInherited;
        else return true;
      }
    }

    public bool IsInherited
    {
      get
      {
        if (Definition != null) return Definition.IsInherited;
        else return false;
      }
    }

    public bool IsNotInherited
    {
      get => !IsInherited;
    }

    public bool IsModifiedFromBase
    {
      get
      {
        if (Definition != null) return Definition.IsModifiedFromBase;
        else return false;
      }
    }

    public string InheritanceModifiedTooltip
    {
      get
      {
        if (IsInherited && IsModifiedFromBase)
          return $"Modified from parent definition";
        else if (IsInherited && !IsModifiedFromBase)
          return $"NOT modified from parent definition";
        else return "";
      }
    }

    public bool CanEditName
    {
      get
      {
        if (Definition is LoreTypeDefinition)
          return true;
        else return !IsInherited;
      }
    }

    public string Name
    {
      get => Definition.name;
      set
      {
        Definition.name = value;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public string InheritanceLabelString
    {
      get
      {
        if (IsInherited) return $"({Definition.Base.name})";
        else return "";
      }
    }

    public virtual bool UsesAny { get { return true; } }

    public virtual void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(IsInherited));
      this.RaisePropertyChanged(nameof(IsDeletable));
      this.RaisePropertyChanged(nameof(IsModifiedFromBase));
      this.RaisePropertyChanged(nameof(IsNotInherited));
      this.RaisePropertyChanged(nameof(Name));
      this.RaisePropertyChanged(nameof(CanEditName));
      this.RaisePropertyChanged(nameof(InheritanceLabelString));

      //SettingsRefresher.Apply(CurrentSettingsViewModel);
    }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase, LoreSettingsViewModel currentSettingsVM)
    {
      CurrentSettingsViewModel = currentSettingsVM;

      Definition = definitionBase;

      if(definitionBase != null)
        BuildLists();
    }

    public virtual void BuildLists() { }

    public override string ToString() => Definition != null ? $"VM of {Definition}" : Name;
  }
}
