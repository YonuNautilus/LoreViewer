using LoreViewer.Settings;
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
    #endregion

    private LoreEmbeddedNodeDefinition embDef => Definition as LoreEmbeddedNodeDefinition;
    public string TypeName { get => embDef?.entryTypeName; set => ApplyNewType(value); }

    public bool IsRequired { get => embDef.required; set => embDef.required = value; }

    public TypeDefinitionViewModel NodeTypeVM
    {
      get
      {
        return CurrentSettingsViewModel.Types.FirstOrDefault(tvm => tvm.Definition == embDef.nodeType);
      }
      set
      {
        if(value != null)
        {
          embDef.nodeType = value.Definition as LoreTypeDefinition;
          SettingsRefresher.Apply(CurrentSettingsViewModel);
        }
      }

    }

    public TypeDefinitionViewModel Type { get; set; }
    public EmbeddedNodeDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase)
    {
      if (CurrentSettingsViewModel != null && embDef != null)
      {
        Type = CurrentSettingsViewModel.Types.FirstOrDefault(tvm => tvm.Definition == definitionBase);
      }
      //Type = new TypeDefinitionViewModel(embDef.nodeType);
    }

    public override void RefreshLists()
    {

    }

    public void ApplyNewType(string typeName)
    {
      LoreTypeDefinition newType = CurrentSettings.GetTypeDefinition(typeName);
      if (newType != null)
      {
        embDef.nodeType = newType;
        Type = new TypeDefinitionViewModel(newType);
      }
    }
  }
}
