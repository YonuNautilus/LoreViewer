using Avalonia.Interactivity;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class SectionDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    #endregion 

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
