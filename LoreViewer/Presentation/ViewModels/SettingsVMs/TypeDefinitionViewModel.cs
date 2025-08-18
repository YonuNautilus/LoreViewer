using LoreViewer.Domain.Settings.Definitions;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace LoreViewer.Presentation.ViewModels.SettingsVMs
{
  public class TypeDefinitionViewModel : LoreDefinitionViewModel
  {
    public LoreTypeDefinition typeDef { get => Definition as LoreTypeDefinition; }

    public string ExtendsTypeName { get => typeDef.extends; }

    public ObservableCollection<TypeDefinitionViewModel> AllValidTypes
    {
      get
      {
        if (typeDef.isExtendedType)
          // If this is an extended type,We don't want to see child types or itself
          return new ObservableCollection<TypeDefinitionViewModel>(CurrentSettingsViewModel.Types.Except(CurrentSettingsViewModel.Types.Where(defVM => defVM.typeDef.IsATypeOf(typeDef))).ToList());
        else
          // if this is not an extended type, we want to see all types except child types
          return new ObservableCollection<TypeDefinitionViewModel>(CurrentSettingsViewModel.Types.Except(CurrentSettingsViewModel.Types.Where(defVM => typeDef.IsParentOf(defVM.typeDef))).ToList());
      }
    }


    public TypeDefinitionViewModel ExtendsTypeVM
    {
      get => CurrentSettingsViewModel.Types.FirstOrDefault(tdvm => tdvm.Definition == typeDef.ParentType);
      set
      {
        // try making the definition independent first, might make merging fields easier
        typeDef.MakeIndependent();
        typeDef.ParentType = value.typeDef;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public bool HasEmbeddedNodes { get => typeDef.HasNestedNodes; }
    public bool HasCollections { get => typeDef.collections != null && typeDef.collections.Count > 0; }


    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new();
    private ObservableCollection<SectionDefinitionViewModel> m_cSections = new();
    private ObservableCollection<CollectionDefinitionViewModel> m_cCollections = new();
    private ObservableCollection<EmbeddedNodeDefinitionViewModel> m_cEmbeddeds = new();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get => m_cFields; }
    public override ObservableCollection<SectionDefinitionViewModel> Sections { get => m_cSections; }
    public override ObservableCollection<CollectionDefinitionViewModel> Collections { get => m_cCollections; }
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get => m_cEmbeddeds; }
    public override ObservableCollection<PicklistDefinitionViewModel> Picklists => null;
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    public override ObservableCollection<PicklistEntryDefinitionViewModel> PicklistEntries => null;


    public ReactiveCommand<Unit, Unit> MakeIndependentCommand { get; set; }


    public TypeDefinitionViewModel(LoreTypeDefinition definition, LoreSettingsViewModel curSettingsVM) : base(definition, curSettingsVM)
    {


      MakeIndependentCommand = ReactiveCommand.Create(() =>
      {
        if (Definition != null)
          typeDef.MakeIndependent();

        SettingsRefresher.Apply(CurrentSettingsViewModel);
      });
    }


    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if (typeDef.fields != null)
        foreach (LoreFieldDefinition def in typeDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def, CurrentSettingsViewModel));
    }
    private void RefreshSectionDefs()
    {
      m_cSections.Clear();
      if (typeDef.sections != null)
        foreach (LoreSectionDefinition def in typeDef.sections)
          m_cSections.Add(new SectionDefinitionViewModel(def, CurrentSettingsViewModel));
    }
    private void RefreshEmbeddedDefs()
    {
      m_cEmbeddeds.Clear();
      if (typeDef.embeddedNodeDefs != null)
        foreach (LoreEmbeddedNodeDefinition def in typeDef.embeddedNodeDefs)
          m_cEmbeddeds.Add(new EmbeddedNodeDefinitionViewModel(def, CurrentSettingsViewModel));
    }
    private void RefreshCollectionDefs()
    {
      m_cCollections.Clear();
      if (typeDef.collections != null)
        foreach (LoreCollectionDefinition def in typeDef.collections)
          m_cCollections.Add(new CollectionDefinitionViewModel(def, CurrentSettingsViewModel));
    }


    public override void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(ExtendsTypeVM));
      this.RaisePropertyChanged(nameof(ExtendsTypeName));
      base.RefreshUI();
      //this.RaisePropertyChanged(nameof(AllValidTypes));
    }

    public override void BuildLists()
    {
      RefreshFieldDefs();
      RefreshSectionDefs();
      RefreshEmbeddedDefs();
      RefreshCollectionDefs();
    }
  }
}
