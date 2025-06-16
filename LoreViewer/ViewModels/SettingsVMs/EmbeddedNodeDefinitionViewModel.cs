using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LoreViewer.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
    public string TypeName { get => emdDef?.entryTypeName; set => ApplyNewType(value); }

    public bool IsRequired { get => emdDef.required; set => emdDef.required = value; }

    public List<string> TypeNamesList { get => CurrentSettings.types.Select(n => n.name).ToList(); }

    public TypeDefinitionViewModel Type { get; set; }
    public EmbeddedNodeDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase) { Type = new TypeDefinitionViewModel(emdDef.nodeType); }

    public override void RefreshLists()
    {

    }

    public void ApplyNewType(string typeName)
    {
      LoreTypeDefinition newType = CurrentSettings.GetTypeDefinition(typeName);
      if (newType != null)
      {
        emdDef.nodeType = newType;
        Type = new TypeDefinitionViewModel(newType);
      }
    }
  }
}
