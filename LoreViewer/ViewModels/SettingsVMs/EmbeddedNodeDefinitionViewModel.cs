using DocumentFormat.OpenXml.Presentation;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class EmbeddedNodeDefinitionViewModel : LoreDefinitionViewModel
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

    private LoreEmbeddedNodeDefinition emdDef => Definition as LoreEmbeddedNodeDefinition;
    public string TypeName { get => emdDef.entryTypeName; }

    public bool IsRequired { get => emdDef.required; set => emdDef.required = value; }

    public TypeDefinitionViewModel Type { get; set; }
    public EmbeddedNodeDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase) { Type = new TypeDefinitionViewModel(emdDef.nodeType); }

    public override void RefreshLists()
    {

    }
  }
}
