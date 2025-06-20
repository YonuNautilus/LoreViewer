using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class LoreDefinitionViewModel : LoreSettingsObjectViewModel
  {
    public static LoreSettingsViewModel CurrentSettingsViewModel { get; set; }

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

    public string Name { get => Definition.name; set => Definition.name = value; }

    public virtual bool UsesAny { get { return true; } }

    public virtual void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(IsInherited));
      this.RaisePropertyChanged(nameof(IsDeletable));
      this.RaisePropertyChanged(nameof(IsModifiedFromBase));
      this.RaisePropertyChanged(nameof(IsNotInherited));
      this.RaisePropertyChanged(nameof(Name));
      this.RaisePropertyChanged(nameof(CanEditName));

      SettingsRefresher.Apply(CurrentSettingsViewModel);
    }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase)
    {
      Definition = definitionBase;

      if(definitionBase != null)
        RefreshLists();
    }

  }
}
