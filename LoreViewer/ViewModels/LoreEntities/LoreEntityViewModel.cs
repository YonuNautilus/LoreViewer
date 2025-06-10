using LoreViewer.LoreElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.LoreEntities
{

  public class LoreEntityViewModel : ViewModelBase
  {
    public bool IsDirty { get; set; } = false;

    internal LoreEntity entity;
    public static LoreEntityViewModel CreateViewModel(LoreEntity e)
    {
      switch (e)
      {
        case LoreNode node:
          return new LoreNodeViewModel(node);
        case LoreCompositeNode compositeNode:
          return new LoreCompositeNodeViewModel(compositeNode);
        default:
          return null;
      }
    }
  }
}
