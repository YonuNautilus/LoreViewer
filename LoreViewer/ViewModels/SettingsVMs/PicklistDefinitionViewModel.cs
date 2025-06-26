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
    #endregion overrides


    private ObservableCollection<PicklistDefinitionViewModel> m_cOptions = new ObservableCollection<PicklistDefinitionViewModel>();

    public override ObservableCollection<PicklistDefinitionViewModel> PicklistOptions { get => m_cOptions; }


    public LorePicklistDefinition pickDef => Definition as LorePicklistDefinition;


    public PicklistDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase)
    {

    }

    public void AddNewSubOption()
    {

    }

    private void RefreshPicklists()
    {
      m_cOptions.Clear();
      if (pickDef.HasOptions)
        foreach (var pl in pickDef.options)
          m_cOptions.Add(new PicklistDefinitionViewModel(pl));
    }

    public override void RefreshLists()
    {
      RefreshPicklists();
    }


  }
}
