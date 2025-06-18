using LoreViewer.Settings;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class SectionDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;

    public override bool UsesTypes { get { return false; } }
    public override bool UsesFields { get { return true; } }
    public override bool UsesSections { get { return true; } }
    public override bool UsesCollections { get { return false; } }
    public override bool UsesEmbeddedNodes { get { return false; } }
    #endregion 

    public bool HasFields { get => secDef.HasFields; }

    public bool IsFreeform { get => secDef.freeform; }


    private LoreSectionDefinition secDef { get => Definition as LoreSectionDefinition; }
    public bool IsRequired { get => secDef.required; set => secDef.required = value; }
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<FieldDefinitionViewModel> Fields => null;
    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }
    public SectionDefinitionViewModel(LoreSectionDefinition definition) : base(definition) { }

    public override void RefreshLists()
    {

    }
  }
}
