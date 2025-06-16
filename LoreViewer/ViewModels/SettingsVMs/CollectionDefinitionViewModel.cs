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
          if (AllCollections.Count > 0) colDef.ContainedType = AllCollections.FirstOrDefault();
          else
            UseNewCollectionDefinition(new LoreCollectionDefinition() { name = "New Collection" });

        }
        this.RaisePropertyChanged("ContainedType");
        this.RaisePropertyChanged("EntryCollection");
        this.RaisePropertyChanged("IsCollectionOfCollections");
        this.RaisePropertyChanged("IsCollectionOfNodes");
        this.RaisePropertyChanged("AllTypes");

      }
    }

    public bool IsRequired { get => colDef.required; }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; }

    public LoreCollectionDefinition EntryCollection { get => colDef.entryCollection; }

    public CollectionDefinitionViewModel EntryCollectionVM { get => new CollectionDefinitionViewModel(EntryCollection); }

    public LoreTypeDefinition EntryType { get => colDef.ContainedType as LoreTypeDefinition; }

    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition) { }

    public void UseNewCollectionDefinition(LoreCollectionDefinition newColDef)
    {
      locallyDefinedCollectionDefs.Add(newColDef);
      colDef.ContainedType = newColDef;
      this.RaisePropertyChanged("AllTypes");
      this.RaisePropertyChanged("AllCollections");
      this.RaisePropertyChanged("ContainedType");
      this.RaisePropertyChanged("IsCollectionOfNodes");
      this.RaisePropertyChanged("IsCollectionOfCollections");
      this.RaisePropertyChanged("EntryCollection");
    }

    public override void RefreshLists()
    {

    }
  }
}
