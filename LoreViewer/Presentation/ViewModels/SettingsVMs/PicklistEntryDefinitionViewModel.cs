using LoreViewer.Domain.Settings.Definitions;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.ViewModels.SettingsVMs
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


    public PicklistEntryDefinitionViewModel(LoreDefinitionBase definitionBase, LoreSettingsViewModel curSettingsVM) : base(definitionBase, curSettingsVM)
    {

    }

    public PicklistEntryDefinitionViewModel GetBranch(LorePicklistEntryDefinition branchDef)
    {
      if (pickEntryDef == branchDef) return this;
      else
      {
        foreach (PicklistEntryDefinitionViewModel pledvm in PicklistEntries)
        {
          var ret = pledvm.GetBranch(branchDef);
          if (ret != null) return ret;
        }
      }
      return null;
    }

    private void BuildPicklists()
    {
      m_cEntries.Clear();
      if (pickEntryDef.HasEntries)
        foreach (var pl in pickEntryDef.entries)
          m_cEntries.Add(new PicklistEntryDefinitionViewModel(pl, CurrentSettingsViewModel));
    }

    public override void BuildLists()
    {
      BuildPicklists();
    }


  }
}
