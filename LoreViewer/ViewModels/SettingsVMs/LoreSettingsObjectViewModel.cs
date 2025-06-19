using Avalonia;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

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


    public List<LoreTypeDefinition> AllTypes { get => CurrentSettings.types; }

    protected ObservableCollection<TypeDefinitionViewModel> _allTypeVMs = new ObservableCollection<TypeDefinitionViewModel>();
    public ObservableCollection<TypeDefinitionViewModel> AllTypeVMs 
    {
      get
      {
        return Types;
      }
    }

    public LoreDefinitionViewModel SelectedItem { get; set; }

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




    public List<string> TypeNamesList { get => CurrentSettings.types.Select(n => n.name).ToList(); }



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

      CurrentSettings.PostProcess();
      RefreshLists();
    }




    public abstract void RefreshLists();
  }
}
