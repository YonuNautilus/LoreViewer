using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

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

    public override bool UsesAny { get { return false; } }
    public override bool UsesTypes { get { return false; } }
    public override bool UsesFields { get { return false; } }
    public override bool UsesSections { get { return false; } }
    public override bool UsesCollections { get { return false; } }
    public override bool UsesEmbeddedNodes { get { return false; } }
    #endregion

    private LoreCollectionDefinition colDef { get => Definition as LoreCollectionDefinition; }

    public bool IsCollectionOfCollections { get => colDef.IsCollectionOfCollections; }
    public bool IsCollectionOfNodes { get => !colDef.IsCollectionOfCollections; }

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
          colDef.ContainedType = AllTypes.FirstOrDefault();
        }
        else
        {
          if (AllCollectionVMs.Count > 0) colDef.ContainedType = AllCollectionVMs.FirstOrDefault().Definition;
          else
            UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });

        }
        this.RaisePropertyChanged("ContainedType");
        this.RaisePropertyChanged("EntryCollection");
        this.RaisePropertyChanged("IsCollectionOfCollections");
        this.RaisePropertyChanged("IsCollectionOfNodes");
        this.RaisePropertyChanged("AllTypes");
        this.RaisePropertyChanged("AllTypeVMs");

      }
    }

    public bool IsRequired { get => colDef.required; }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; set => colDef.ContainedType = value; }
    public LoreDefinitionViewModel ContainedTypeVM
    {
      get
      {
        switch (ContainedType)
        {
          case LoreTypeDefinition typeDef:
            return AllTypeVMs.FirstOrDefault(tvm => tvm.Definition == typeDef);
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
          ContainedType = value.Definition;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }
    }

    public LoreCollectionDefinition EntryCollection { get => colDef.entryCollection; }

    public CollectionDefinitionViewModel EntryCollectionVM { get => new CollectionDefinitionViewModel(EntryCollection); }

    public LoreTypeDefinition EntryType { get => colDef.ContainedType as LoreTypeDefinition; }

    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition)
    {
      AddCollectionCommand = ReactiveCommand.Create(CreateNewLocalCollection);
    }


    private void CreateNewLocalCollection()
    {
      UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });
    }

    public void UseNewCollectionDefinition(LoreCollectionDefinition newColDef)
    {
      var newVM = new CollectionDefinitionViewModel(newColDef);
      locallyDefinedCollectionDefs.Add(newVM);
      this.RaisePropertyChanged("AllTypeVMs");
      this.RaisePropertyChanged("AllCollectionVMs");
      this.RaisePropertyChanged("IsCollectionOfNodes");
      this.RaisePropertyChanged("IsCollectionOfCollections");
      this.RaisePropertyChanged("EntryCollection");
      ContainedTypeVM = newVM;
      this.RaisePropertyChanged("ContainedTypeVM");
    }

    public override void RefreshLists()
    {

    }
  }
}
