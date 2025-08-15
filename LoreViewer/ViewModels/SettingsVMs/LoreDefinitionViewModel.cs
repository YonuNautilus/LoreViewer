using LoreViewer.Domain.Settings.Definitions;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
        if (Definition != null) return !Definition.IsInherited;
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

    public bool CanEditRequired
    {
      get
      {
        if (Definition is IRequirable iReq)
        {
          if (IsInherited && (Definition.Base as IRequirable).required)
            return false;
        }
        return true;
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
      this.RaisePropertyChanged(nameof(CanEditRequired));
      this.RaisePropertyChanged(nameof(InheritanceLabelString));

      //SettingsRefresher.Apply(CurrentSettingsViewModel);
    }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase, LoreSettingsViewModel currentSettingsVM)
    {
      CurrentSettingsViewModel = currentSettingsVM;

      Definition = definitionBase;

      if (definitionBase != null)
        BuildLists();
    }

    public virtual void BuildLists() { }


    /// <summary>
    /// Central logic method for refreshing the definition view models.
    /// Deletes contained viewmodels if deleted on the model, or adds a new contained viewmodel if added in the model
    /// </summary>
    public void RefreshChildren()
    {
      // Fields
      if (Definition is IFieldDefinitionContainer ifdc && ifdc.HasFields)
      {
        // Check for deletions
        for (int i = Fields.Count - 1; i >= 0; i--)
        {
          FieldDefinitionViewModel lfd = Fields[i];
          if (lfd.Definition.WasDeleted) Fields.Remove(lfd);
        }

        // Check additions
        for (int i = ifdc.fields.Count - 1; i >= 0; i--)
        {
          LoreFieldDefinition lfd = ifdc.fields[i];
          if (!Fields.Any(fdvm => fdvm.Definition == lfd))
            Fields.Insert(i, new FieldDefinitionViewModel(lfd, CurrentSettingsViewModel));
        }

        // Propagate refresh down to field children
        foreach (FieldDefinitionViewModel fd in Fields)
          fd.RefreshChildren();
      }


      // Sections
      if (Definition is ISectionDefinitionContainer isdc && isdc.HasSections)
      {
        // Check for deletions
        for (int i = Sections.Count - 1; i >= 0; i--)
        {
          SectionDefinitionViewModel lsd = Sections[i];
          if (lsd.Definition.WasDeleted) Sections.Remove(lsd);
        }

        // Check additions
        for (int i = isdc.sections.Count - 1; i >= 0; i--)
        {
          LoreSectionDefinition lsd = isdc.sections[i];
          if (!Sections.Any(fdvm => fdvm.Definition == lsd))
            Sections.Insert(i, new SectionDefinitionViewModel(lsd, CurrentSettingsViewModel));
        }

        // Propagate refresh down to section children
        foreach (SectionDefinitionViewModel sd in Sections)
          sd.RefreshChildren();
      }


      // Collections
      if (Definition is ICollectionDefinitionContainer icdc && icdc.HasCollections)
      {
        // Check for deletions
        for (int i = Collections.Count - 1; i >= 0; i--)
        {
          CollectionDefinitionViewModel lcd = Collections[i];
          if (lcd.Definition.WasDeleted) Collections.Remove(lcd);
        }

        // Check additions
        for (int i = icdc.collections.Count - 1; i >= 0; i--)
        {
          LoreCollectionDefinition lcd = icdc.collections[i];
          if (!Collections.Any(fdvm => fdvm.Definition == lcd))
            Collections.Insert(i, new CollectionDefinitionViewModel(lcd, CurrentSettingsViewModel));
        }

        // Propagate refresh down to collection children
        foreach (CollectionDefinitionViewModel cd in Collections)
          cd.RefreshChildren();
      }


      // EmbeddedNodes
      if (Definition is IEmbeddedNodeDefinitionContainer iedc && iedc.HasNestedNodes)
      {
        // Check for deletions
        for (int i = EmbeddedNodes.Count - 1; i >= 0; i--)
        {
          EmbeddedNodeDefinitionViewModel led = EmbeddedNodes[i];
          if (led.Definition.WasDeleted) EmbeddedNodes.Remove(led);
        }

        // Check additions
        for (int i = iedc.embeddedNodeDefs.Count - 1; i >= 0; i--)
        {
          LoreEmbeddedNodeDefinition lcd = iedc.embeddedNodeDefs[i];
          if (!EmbeddedNodes.Any(fdvm => fdvm.Definition == lcd))
            EmbeddedNodes.Insert(i, new EmbeddedNodeDefinitionViewModel(lcd, CurrentSettingsViewModel));
        }

        // Propagate refresh down to embeddedNode children
        foreach (EmbeddedNodeDefinitionViewModel ed in EmbeddedNodes)
          ed.RefreshChildren();
      }


      if (Definition is IPicklistEntryDefinitionContainer ipedc && ipedc.HasEntries)
      {
        // Check for deletions
        for (int i = PicklistEntries.Count - 1; i >= 0; i--)
        {
          PicklistEntryDefinitionViewModel pledvm = PicklistEntries[i];
          if (pledvm.Definition.WasDeleted) PicklistEntries.Remove(pledvm);
        }

        // Check for additions
        for (int i = ipedc.entries.Count - 1; i >= 0; i--)
        {
          LorePicklistEntryDefinition ple = ipedc.entries[i];
          if (!PicklistEntries.Any(plevm => plevm.Definition == ple))
            PicklistEntries.Insert(i, new PicklistEntryDefinitionViewModel(ple, CurrentSettingsViewModel));
        }

        // propagate refresh down to picklist entry childre
        foreach (PicklistEntryDefinitionViewModel pledvm in PicklistEntries)
          pledvm.RefreshChildren();
      }



      RefreshUI();
    }

    public override string ToString() => Definition != null ? $"VM of {Definition}" : Name;
  }
}
