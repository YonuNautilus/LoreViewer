using DocumentFormat.OpenXml.Drawing.Diagrams;
using LoreViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class LoreCollectionViewModel : LoreElementViewModel
  {
    public LoreCollection trueEntity => entity as LoreCollection;
    public LoreCollectionViewModel(LoreCollection lc) : base(lc)
    {
      if (lc.HasCollections) ShallowChildren = new ObservableCollection<LoreEntityViewModel>(lc.Collections.Select(c => new LoreCollectionViewModel(c)));
      else ShallowChildren = new ObservableCollection<LoreEntityViewModel>(lc.Nodes.Select(n => LoreEntityViewModel.CreateViewModel(n)));
      
    }
  }
}
