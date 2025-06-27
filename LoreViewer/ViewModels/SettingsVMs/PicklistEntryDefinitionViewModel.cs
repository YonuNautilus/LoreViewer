using DynamicData;
using LoreViewer.Settings;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class PicklistEntryDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    public override ObservableCollection<FieldDefinitionViewModel> Fields => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    public override ObservableCollection<PicklistDefinitionViewModel> Picklists => null;
    #endregion overrides


    private ObservableCollection<PicklistEntryDefinitionViewModel> m_cEntries = new ObservableCollection<PicklistEntryDefinitionViewModel>();
    public override ObservableCollection<PicklistEntryDefinitionViewModel> PicklistEntries { get => m_cEntries; }


    public LorePicklistEntryDefinition pickEntryDef => Definition as LorePicklistEntryDefinition;


    public PicklistEntryDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase)
    {

    }

    public void AddNewSubOption()
    {

    }

    private void RefreshPicklists()
    {
      m_cEntries.Clear();
      if (pickEntryDef.HasEntries)
        foreach (var pl in pickEntryDef.entries)
          m_cEntries.Add(new PicklistEntryDefinitionViewModel(pl));
    }

    public override void RefreshLists()
    {
      RefreshPicklists();
    }


  }
}
