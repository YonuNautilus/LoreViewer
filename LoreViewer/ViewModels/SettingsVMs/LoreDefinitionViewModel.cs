using LoreViewer.Settings;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class LoreDefinitionViewModel : LoreSettingsObjectViewModel
  {
    public static LoreSettingsViewModel CurrentSettingsViewModel { get; set; }

    public ObservableCollection<CollectionDefinitionViewModel> locallyDefinedCollectionDefs = new ObservableCollection<CollectionDefinitionViewModel>();

    public ObservableCollection<CollectionDefinitionViewModel> AllCollectionVMs
    {
      get
      {
        return new ObservableCollection<CollectionDefinitionViewModel>(
          CurrentSettings.collections.Select(c => new CollectionDefinitionViewModel(c)).Concat(
            locallyDefinedCollectionDefs
          )
        );
      }
    }

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

    public bool CanEditName { get => false; }

    public string Name { get => Definition.name; set => Definition.name = value; }

    public virtual bool UsesAny { get { return true; } }

    public abstract bool UsesTypes { get; }
    public string TypesTabTitle
    {
      get
      {
        if (Types == null || Types.Count == 0)
          return $"Types";
        else
          return $"Types ({Types.Count})";
      }
    }

    public abstract bool UsesFields { get; }

    public string FieldsTabTitle
    {
      get
      {
        if (Fields == null || Fields.Count == 0)
          return $"Fields";
        else
          return $"Fields ({Fields.Count})";
      }
    }

    public abstract bool UsesSections { get; }

    public  string SectionsTabTitle
    {
      get
      {
        if (Sections == null || Sections.Count == 0)
          return $"Sections";
        else
          return $"Sections ({Sections.Count})";
      }
    }

    public abstract bool UsesEmbeddedNodes { get; }
    public string EmbeddedNodesTabTitle
    {
      get
      {
        if (EmbeddedNodes == null || EmbeddedNodes.Count == 0)
          return $"Embedded Nodes";
        else
          return $"Embedded Nodes ({EmbeddedNodes.Count})";
      }
    }
    
    public abstract bool UsesCollections { get; }

    public string CollectionsTabTitle
    {
      get
      {
        if (Collections == null || Collections.Count == 0)
          return $"Collections";
        else
          return $"Collections ({Collections.Count})";
      }
    }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase)
    {
      Definition = definitionBase;

      if(definitionBase != null)
        RefreshLists();
    }

  }
}
