using Avalonia;
using Avalonia.Controls;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class LoreDefinitionViewModel
  {
    private Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; }
    public ReactiveCommand<Unit, Unit> AddFieldCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSectionCommand { get; }
    public ReactiveCommand<Unit, Unit> AddCollectionCommand { get; }
    public ReactiveCommand<Unit, Unit> AddEmbeddedCommand { get; }
    public LoreDefinitionBase Definition { get; }

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

    public abstract ObservableCollection<TypeDefinitionViewModel> Types { get; }
    public abstract ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public abstract ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public abstract ObservableCollection<CollectionDefinitionViewModel> Collections { get; }
    public abstract ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }

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
      RefreshLists();
    }

    public abstract void RefreshLists();

    public void DeleteDefinition(LoreDefinitionViewModel viewModel)
    {
      switch (viewModel)
      {
        case TypeDefinitionViewModel typeDefVM:
          break;
        case FieldDefinitionViewModel fieldDefVM:
          if (Definition is IFieldDefinitionContainer ifdc)
          {
            ifdc.fields.Remove(fieldDefVM.Definition as LoreFieldDefinition);
            if(Fields != null)
              Fields.Remove(fieldDefVM);
          }
          break;
        case SectionDefinitionViewModel sectionDefVM:
          if (Definition is ISectionDefinitionContainer isdc)
          {
            isdc.sections.Remove(sectionDefVM.Definition as LoreSectionDefinition);
            if(Sections != null)
              Sections.Remove(sectionDefVM);
          }
          break;
        case CollectionDefinitionViewModel collectionDefVM:
          if (Definition is ICollectionDefinitionContainer icdc)
          {
            icdc.collections.Remove(collectionDefVM.Definition as LoreCollectionDefinition);
            if(Collections != null)
              Collections.Remove(collectionDefVM);
          }
          break;
        case EmbeddedNodeDefinitionViewModel embeddedNodeDefVM:
          if (Definition is IEmbeddedNodeDefinitionContainer iendc)
          {
            iendc.embeddedNodeDefs.Remove(embeddedNodeDefVM.Definition as LoreEmbeddedNodeDefinition);
            if(EmbeddedNodes != null)
              EmbeddedNodes.Remove(embeddedNodeDefVM);
          }
          break;
        default: return;
      }
    }

    public async Task EditDefinition(LoreDefinitionViewModel vm)
    {
      var dialog = DefinitionEditorDialog.CreateDefinitionEditorDialog(vm);
      await dialog.ShowDialog(TopLevel.GetTopLevel(m_oView) as Window);
    }

    public void AddField()
    {
      if(Definition is IFieldDefinitionContainer fieldContainer)
      {
        if (fieldContainer.fields == null) fieldContainer.fields = new List<LoreFieldDefinition>();
        fieldContainer.fields.Add(new LoreFieldDefinition());
      }
    }

    public void AddSection()
    {
      if(Definition is ISectionDefinitionContainer sectionContainer)
      {
        if (sectionContainer.sections == null) sectionContainer.sections = new List<LoreSectionDefinition>();
        sectionContainer.sections.Add(new LoreSectionDefinition());
      }
    }

    public void AddCollection()
    {
      if(Definition is ICollectionDefinitionContainer collectionContainer)
      {
        if (collectionContainer.collections == null) collectionContainer.collections = new List<LoreCollectionDefinition>();
        collectionContainer.collections.Add(new LoreCollectionDefinition());
      }
    }

    public void AddEmbedded()
    {
      if(Definition is IEmbeddedNodeDefinitionContainer embeddedContainer)
      {
        if (embeddedContainer.embeddedNodeDefs == null) embeddedContainer.embeddedNodeDefs = new List<LoreEmbeddedNodeDefinition>();
        embeddedContainer.embeddedNodeDefs.Add(new LoreEmbeddedNodeDefinition());
      }
    }
  }
}
