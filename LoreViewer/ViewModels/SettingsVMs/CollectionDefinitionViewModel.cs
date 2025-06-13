using DynamicData.Alias;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    private LoreCollectionDefinition colDef { get => Definition as LoreCollectionDefinition; }

    public bool IsRequired { get => colDef.required; }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; }
    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition) { }

    public override void RefreshLists()
    {

    }
  }
}
