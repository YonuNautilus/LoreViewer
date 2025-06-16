using Avalonia;
using Avalonia.Controls;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class LoreDefinitionViewModel : LoreSettingsObjectViewModel
  { 

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

    public LoreDefinitionViewModel SelectedItem { get; set; }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase)
    {
      DeleteDefinitionCommand = ReactiveCommand.Create<LoreDefinitionViewModel>(DeleteDefinition);
      EditDefinitionCommand = ReactiveCommand.CreateFromTask<LoreDefinitionViewModel>(EditDefinition);
      AddFieldCommand = ReactiveCommand.Create(AddField);
      AddSectionCommand = ReactiveCommand.Create(AddSection);
      AddCollectionCommand = ReactiveCommand.Create(AddCollection);
      AddEmbeddedCommand = ReactiveCommand.Create(AddEmbedded);

      Definition = definitionBase;

      if(definitionBase != null)
        RefreshLists();
    }

  }
}
