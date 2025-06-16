using LoreViewer.Settings;
using System.Collections.ObjectModel;

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

    public bool IsRequired { get => colDef.required; }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; }
    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition) { }

    public override void RefreshLists()
    {

    }
  }
}
