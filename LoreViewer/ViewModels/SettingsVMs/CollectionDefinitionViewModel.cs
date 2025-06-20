using DocumentFormat.OpenXml.Presentation;
using DynamicData;
using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class CollectionDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<FieldDefinitionViewModel> Fields => null;
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    #endregion

    public ReactiveCommand<Unit, Unit> AddLocalCollectionCommand { get; set; }

    private LoreCollectionDefinition colDef { get => Definition as LoreCollectionDefinition; }

    public ObservableCollection<CollectionDefinitionViewModel> AllCollectionVMs
    {
      get
      {
        var ret = new ObservableCollection<CollectionDefinitionViewModel>(CurrentSettingsViewModel.Collections);
        if(locallyDefinedCollectionVM != null)
          ret.Add(locallyDefinedCollectionVM);
        return ret;
      }
    }

    public ObservableCollection<TypeDefinitionViewModel> TypesFromSettings { get => CurrentSettingsViewModel.Types; }


    private CollectionDefinitionViewModel locallyDefinedCollectionVM { get; set; }

    public bool IsCollectionOfCollections { get => colDef.IsCollectionOfCollections; }
    public bool IsCollectionOfNodes { get => !colDef.IsCollectionOfCollections; }
    
    public bool IsUsingLocalCollectionDef { get => colDef.IsUsingLocallyDefinedCollection; }

    public bool IsNotUsingLocalCollectionDef { get => !colDef.IsUsingLocallyDefinedCollection; }

    public bool UsesTypesOrNull
    {
      get
      {
        if (colDef.ContainedType == null || colDef.ContainedType is LoreTypeDefinition) return true;
        else return false;
      }
      set
      {
        if (value)
        {
          colDef.entryCollection = null;
          ContainedTypeVM = CurrentSettingsViewModel.Types.FirstOrDefault();
        }
        else
        {
          if (AllCollectionVMs.Count > 0) ContainedTypeVM = AllCollectionVMs.FirstOrDefault();
          else
            UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });

        }
        RefreshUI();

      }
    }

    public bool IsRequired
    {
      get => colDef.required;
      set
      {
        colDef.required = value;
        RefreshUI();
      }
    }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; }
    public LoreDefinitionViewModel ContainedTypeVM
    {
      get
      {
        Trace.Write("");
        switch (ContainedType)
        {
          case LoreTypeDefinition typeDef:
            return CurrentSettingsViewModel.Types.FirstOrDefault(tvm => tvm.Definition == typeDef);
          case LoreCollectionDefinition colDef:
            if (this.IsUsingLocalCollectionDef)
              return locallyDefinedCollectionVM;
            return AllCollectionVMs.FirstOrDefault(cvm => cvm.Definition == colDef);
          default:
            return null;
        }
      }
      set
      {
        if(value != null)
        {
          colDef.ContainedType = value.Definition;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }
    }

    public LoreCollectionDefinition EntryCollection { get => colDef.entryCollection; set => colDef.entryCollection = value; }

    public CollectionDefinitionViewModel EntryCollectionVM
    {
      get
      {
        if (IsCollectionOfCollections)
          return AllCollectionVMs.FirstOrDefault(cdvm => cdvm.Definition == EntryCollection);
        else return null;
      }
    }

    public LoreTypeDefinition EntryType { get => colDef.ContainedType as LoreTypeDefinition; }

    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition)
    {
      AddLocalCollectionCommand = ReactiveCommand.Create(CreateNewLocalCollection);
      if (colDef.IsUsingLocallyDefinedCollection)
        UseNewCollectionDefinition(colDef.entryCollection);
    }


    private void CreateNewLocalCollection()
    {
      UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });
    }

    public override void RefreshUI()
    {
      this.RaisePropertyChanged("AllTypeVMs");
      this.RaisePropertyChanged("AllCollectionVMs");
      this.RaisePropertyChanged("IsCollectionOfNodes");
      this.RaisePropertyChanged("IsCollectionOfCollections");
      this.RaisePropertyChanged("EntryCollection");
      this.RaisePropertyChanged("ContainedTypeVM");
      base.RefreshUI();
    }

    public void UseNewCollectionDefinition(LoreCollectionDefinition newColDef)
    {
      var newVM = new CollectionDefinitionViewModel(newColDef);
      locallyDefinedCollectionVM = newVM;
      ContainedTypeVM = newVM;

      RefreshUI();
    }
  }
}
