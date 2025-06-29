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
    public override ObservableCollection<PicklistDefinitionViewModel> Picklists => null;
    public override ObservableCollection<PicklistEntryDefinitionViewModel> PicklistEntries => null;
    #endregion 

    public bool HasFields { get => secDef.HasFields; }

    public bool IsFreeform { get => secDef.freeform; }

    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();
    private ObservableCollection<SectionDefinitionViewModel> m_cSections = new ObservableCollection<SectionDefinitionViewModel>();

    private LoreSectionDefinition secDef { get => Definition as LoreSectionDefinition; }
    public bool IsRequired { get => secDef.required; set => secDef.required = value; }
    public override ObservableCollection<SectionDefinitionViewModel> Sections { get => m_cSections; }
    public override ObservableCollection<FieldDefinitionViewModel> Fields { get => m_cFields; }
    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }
    public SectionDefinitionViewModel(LoreSectionDefinition definition, LoreSettingsViewModel curSettingsVM) : base(definition, curSettingsVM) { }

    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if (secDef.fields != null)
        foreach (LoreFieldDefinition def in secDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def, CurrentSettingsViewModel));
    }


    private void RefreshSectionDefs()
    {
      m_cSections.Clear();
      if (secDef.sections != null)
        foreach (LoreSectionDefinition def in secDef.sections)
          m_cSections.Add(new SectionDefinitionViewModel(def, CurrentSettingsViewModel));
    }

    public override void BuildLists()
    {
      RefreshFieldDefs();
      RefreshSectionDefs();
    }
  }
}
