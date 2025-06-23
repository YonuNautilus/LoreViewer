using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

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

    public ReactiveCommand<Unit, Unit> RevertContainedTypeCommand { get; set; }

    #endregion
    private LoreEmbeddedNodeDefinition embDef => Definition as LoreEmbeddedNodeDefinition;

    public bool CanRevertContainedType { get => IsInherited; }

    public bool IsRequired
    {
      get => embDef.required;
      set
      {
        embDef.required = value;
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      }
    }

    public bool CanChangeRequired
    {
      get
      {
        return IsInherited && !(embDef.Base as LoreEmbeddedNodeDefinition).required;
      }
    }

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

    public ObservableCollection<TypeDefinitionViewModel> ValidTypeVMs
    {
      get
      {
        if (IsInherited)
        {
          var containedTypeOfBase = (embDef.Base as LoreEmbeddedNodeDefinition)?.nodeType as LoreTypeDefinition;
          return new ObservableCollection<TypeDefinitionViewModel>(TypesFromSettings.Where(tdvm => tdvm.typeDef.IsATypeOf(containedTypeOfBase)));
        }
        return TypesFromSettings;
      }
    }

    public ObservableCollection<TypeDefinitionViewModel> TypesFromSettings { get => CurrentSettingsViewModel.Types; }

    public EmbeddedNodeDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase)
    {
      if (CurrentSettingsViewModel != null && embDef != null && embDef.nodeType == null)
        embDef.nodeType = CurrentSettingsViewModel.Types.FirstOrDefault()?.Definition as LoreTypeDefinition;

      RevertContainedTypeCommand = ReactiveCommand.Create(() =>
      {
        embDef.nodeType = (TypesFromSettings.First(cvm => cvm.Name == (embDef.Base as LoreEmbeddedNodeDefinition).nodeType.name).typeDef);
        SettingsRefresher.Apply(CurrentSettingsViewModel);
      });
    }

    public override void RefreshUI()
    {
      this.RaisePropertyChanged(nameof(TypesFromSettings));
      this.RaisePropertyChanged(nameof(NodeTypeVM));
      this.RaisePropertyChanged(nameof(IsRequired));
      this.RaisePropertyChanged(nameof(IsInherited));
      this.RaisePropertyChanged(nameof(CanEditName));
      this.RaisePropertyChanged(nameof(CanChangeRequired));
      this.RaisePropertyChanged(nameof(CanRevertContainedType));
      base.RefreshUI();
    }

    public override void RefreshLists()
    {

    }
  }
}
