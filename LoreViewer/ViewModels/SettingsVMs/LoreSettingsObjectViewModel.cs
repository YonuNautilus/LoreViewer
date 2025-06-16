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
  public abstract class LoreSettingsObjectViewModel : ViewModelBase
  {
    public ReactiveCommand<Unit, Unit> AddTypeCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddFieldCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddSectionCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddCollectionCommand { get; set; }
    public ReactiveCommand<Unit, Unit> AddEmbeddedCommand { get; set; }
    public LoreDefinitionBase Definition { get; set; }

    public static LoreSettings CurrentSettings { get; set; }

    protected Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; set; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; set; }





    public abstract ObservableCollection<TypeDefinitionViewModel> Types { get; }
    public abstract ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public abstract ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public abstract ObservableCollection<CollectionDefinitionViewModel> Collections { get; }
    public abstract ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }




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
            if (Fields != null)
              Fields.Remove(fieldDefVM);
          }
          break;
        case SectionDefinitionViewModel sectionDefVM:
          if (Definition is ISectionDefinitionContainer isdc)
          {
            isdc.sections.Remove(sectionDefVM.Definition as LoreSectionDefinition);
            if (Sections != null)
              Sections.Remove(sectionDefVM);
          }
          break;
        case CollectionDefinitionViewModel collectionDefVM:
          if (Definition is ICollectionDefinitionContainer icdc)
          {
            icdc.collections.Remove(collectionDefVM.Definition as LoreCollectionDefinition);
            if (Collections != null)
              Collections.Remove(collectionDefVM);
          }
          break;
        case EmbeddedNodeDefinitionViewModel embeddedNodeDefVM:
          if (Definition is IEmbeddedNodeDefinitionContainer iendc)
          {
            iendc.embeddedNodeDefs.Remove(embeddedNodeDefVM.Definition as LoreEmbeddedNodeDefinition);
            if (EmbeddedNodes != null)
              EmbeddedNodes.Remove(embeddedNodeDefVM);
          }
          break;
        default: return;
      }
    }




    public abstract void RefreshLists();



    public async Task EditDefinition(LoreDefinitionViewModel vm)
    {
      var dialog = DefinitionEditorDialog.CreateDefinitionEditorDialog(vm);
      await dialog.ShowDialog(TopLevel.GetTopLevel(m_oView) as Window);
      RefreshLists();
    }

    public void AddField()
    {
      if (Definition is IFieldDefinitionContainer fieldContainer)
      {
        if (fieldContainer.fields == null) fieldContainer.fields = new List<LoreFieldDefinition>();
        fieldContainer.fields.Add(new LoreFieldDefinition());
        RefreshLists();
      }
    }

    public void AddSection()
    {
      if (Definition is ISectionDefinitionContainer sectionContainer)
      {
        if (sectionContainer.sections == null) sectionContainer.sections = new List<LoreSectionDefinition>();
        sectionContainer.sections.Add(new LoreSectionDefinition());
        RefreshLists();
      }
    }

    public void AddCollection()
    {
      if (Definition is ICollectionDefinitionContainer collectionContainer)
      {
        if (collectionContainer.collections == null) collectionContainer.collections = new List<LoreCollectionDefinition>();
        collectionContainer.collections.Add(new LoreCollectionDefinition());
        RefreshLists();
      }
    }

    public void AddEmbedded()
    {
      if (Definition is IEmbeddedNodeDefinitionContainer embeddedContainer)
      {
        if (embeddedContainer.embeddedNodeDefs == null) embeddedContainer.embeddedNodeDefs = new List<LoreEmbeddedNodeDefinition>();
        embeddedContainer.embeddedNodeDefs.Add(new LoreEmbeddedNodeDefinition());
        RefreshLists();
      }
    }
  }
}
