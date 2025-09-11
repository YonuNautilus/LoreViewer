using DocumentFormat.OpenXml.Drawing.Diagrams;
using LoreViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoreViewer.Domain.Settings.Definitions;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class LoreCollectionViewModel : LoreElementViewModel
  {
    public LoreCollection trueEntity => entity as LoreCollection;
    
    public string NarrativeText
    {
      get => trueEntity.Summary;
    }

    public ObservableCollection<LoreEntityViewModel> ContainedItems { get; }

    public string DisplayContainedType { get => trueEntity.DefinitionAs<LoreCollectionDefinition>().DisplayContainedTypeTag; }


    public override LoreEntityViewModel GetChildVM(LoreEntity eToGet)
    {
      if (entity == eToGet) return this;

      foreach(LoreEntityViewModel vm in ContainedItems)
      {
        LoreEntityViewModel evm = vm.GetChildVM(eToGet);
        if (evm != null) return evm;
      }

      return null;
    }

    
    public LoreCollectionViewModel(LoreCollection lc) : base(lc)
    {
      if (lc.HasCollections)
      {
        ShallowChildren = new ObservableCollection<LoreEntityViewModel>(lc.Collections.Select(c => new LoreCollectionViewModel(c)));
      }
      else
      {
        ShallowChildren = new ObservableCollection<LoreEntityViewModel>(lc.Nodes.Select(n => LoreEntityViewModel.CreateViewModel(n)));
      }

      ContainedItems = ShallowChildren;
      
    }
  }
}
