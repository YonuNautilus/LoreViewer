using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Wordprocessing;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class LoreDefinitionViewModel
  {
    private Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; }
    public LoreDefinitionBase Definition { get; }

    public abstract ObservableCollection<TypeDefinitionViewModel> Types { get; }
    public abstract ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public abstract ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public abstract ObservableCollection<CollectionDefinitionViewModel> Collections { get; }
    public abstract ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }

    public string Name { get => Definition.name; set => Definition.name = value; }


    public LoreDefinitionViewModel SelectedItem { get; set; }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase)
    {
      DeleteDefinitionCommand = ReactiveCommand.Create<LoreDefinitionViewModel>(DeleteDefinition);
      EditDefinitionCommand = ReactiveCommand.CreateFromTask<LoreDefinitionViewModel>(EditDefinition);

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
  }
}
