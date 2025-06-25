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
    public ReactiveCommand<Unit, Unit> RevertContainedTypeCommand { get; set; }

    private LoreCollectionDefinition colDef { get => Definition as LoreCollectionDefinition; }

    public ObservableCollection<CollectionDefinitionViewModel> AllCollectionVMs
    {
      get
      {
        if(CurrentSettingsViewModel != null)
        {
          var ret = new ObservableCollection<CollectionDefinitionViewModel>(CurrentSettingsViewModel.Collections);
          if (locallyDefinedCollectionVM != null)
            ret.Add(locallyDefinedCollectionVM);
          return ret;
        }
        return new ObservableCollection<CollectionDefinitionViewModel>();
      }
    }
    
    public ObservableCollection<TypeDefinitionViewModel> ValidTypeVMs
    {
      get
      {
        if (IsLocallyDefinedDef && IsInherited)
        {
          var containedTypeOfBase = (colDef.Base as LoreCollectionDefinition)?.ContainedType as LoreTypeDefinition;
          return new ObservableCollection<TypeDefinitionViewModel>(TypesFromSettings.Where(tdvm => tdvm.typeDef.IsATypeOf(containedTypeOfBase)));
        }
        return TypesFromSettings;
      }
    }

    public ObservableCollection<TypeDefinitionViewModel> TypesFromSettings { get => CurrentSettingsViewModel.Types; }


    private CollectionDefinitionViewModel locallyDefinedCollectionVM { get; set; }

    public bool IsCollectionOfCollections { get => colDef.IsCollectionOfCollections; }
    public bool IsCollectionOfNodes { get => !colDef.IsCollectionOfCollections; }

    public bool IsLocallyDefinedDef { get => colDef.IsLocallyDefined; }
    
    public bool IsUsingLocalCollectionDef { get => colDef.IsUsingLocallyDefinedCollection; }

    public bool IsNotUsingLocalCollectionDef { get => !colDef.IsUsingLocallyDefinedCollection; }

    public bool CanRevertContainedType { get => IsNotUsingLocalCollectionDef && IsInherited; }

    public bool NotUsingParentDefinedType { get => CanRevertContainedType && (colDef.ContainedType != (colDef.Base as LoreCollectionDefinition).Base); }

    public bool CanChangeRequired
    {
      get
      {
        if (IsInherited) return !(colDef.Base as LoreCollectionDefinition).required;
        else return true;
      }
    }

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
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public bool IsRequired
    {
      get => colDef.required;
      set
      {
        colDef.required = value;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
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
            return AllCollectionVMs.FirstOrDefault(cvm => cvm.Definition == colDef);
          default:
            return null;
        }
      }
      set
      {
        if(value != null)
        {
          colDef.SetContainedType(value.Definition);
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
      {
        var newVM = new CollectionDefinitionViewModel(colDef.entryCollection);
        locallyDefinedCollectionVM = newVM;
        ContainedTypeVM = newVM;
      }

      //Default to a type (ie node) collection if no contained type is specified
      if (ContainedType == null)
        definition.SetContainedType(CurrentSettings.types.FirstOrDefault());

      RevertContainedTypeCommand = ReactiveCommand.Create(() =>
      {
        if (colDef.IsCollectionOfCollections)
          colDef.SetContainedType(AllCollectionVMs.First(cvm => cvm.Name == (colDef.Base as LoreCollectionDefinition).ContainedType.name)?.ContainedType);
        else
          colDef.SetContainedType(CurrentSettingsViewModel.Types.First(tvm => tvm.Name == (colDef.Base as LoreCollectionDefinition).ContainedType.name).Definition);

        SettingsRefresher.Apply(CurrentSettingsViewModel);
      });
    }


    private void CreateNewLocalCollection()
    {
      UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });
    }

    public override void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(TypesFromSettings));
      this.RaisePropertyChanged(nameof(AllCollectionVMs));
      this.RaisePropertyChanged(nameof(IsCollectionOfNodes));
      this.RaisePropertyChanged(nameof(IsCollectionOfCollections));
      this.RaisePropertyChanged(nameof(EntryCollection));
      this.RaisePropertyChanged(nameof(ContainedTypeVM));
      this.RaisePropertyChanged(nameof(IsRequired));
      this.RaisePropertyChanged(nameof(CanChangeRequired));
      base.RefreshUI();
    }

    public void UseNewCollectionDefinition(LoreCollectionDefinition newColDef)
    {
      var newVM = new CollectionDefinitionViewModel(newColDef);
      locallyDefinedCollectionVM = newVM;
      ContainedTypeVM = newVM;

      SettingsRefresher.Apply(CurrentSettingsViewModel);
    }
  }
}
