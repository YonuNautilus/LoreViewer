using DynamicData;
using LoreViewer.Settings;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class PicklistDefinitionViewModel : LoreDefinitionViewModel
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


    public LorePicklistDefinition pickDef => Definition as LorePicklistDefinition;


    public PicklistDefinitionViewModel(LoreDefinitionBase definitionBase, LoreSettingsViewModel curSettingsVM) : base(definitionBase, curSettingsVM)
    {

    }

    public ObservableCollection<PicklistEntryDefinitionViewModel> ValidBranchRestrictionChoices
    {
      get
      {
        var entriesWithChildren = PicklistEntries.FlattenPicklistEntryViewModels().Where(evm => evm.pickEntryDef.HasEntries);
        return new ObservableCollection<PicklistEntryDefinitionViewModel>(entriesWithChildren);
      }
    }

    public PicklistEntryDefinitionViewModel GetBranch(LorePicklistEntryDefinition branchDef)
    {
      foreach (PicklistEntryDefinitionViewModel pledvm in PicklistEntries)
      {
        var ret = pledvm.GetBranch(branchDef);
        if (ret != null) return ret;
      }
      return null;
    }

    public override void RefreshUI()
    {
      
    }

    private void RefreshPicklists()
    {
      m_cEntries.Clear();
      if (pickDef.HasEntries)
        foreach (var ple in pickDef.entries)
          m_cEntries.Add(new PicklistEntryDefinitionViewModel(ple, CurrentSettingsViewModel));
    }

    public override void BuildLists()
    {
      RefreshPicklists();
    }


  }
}
