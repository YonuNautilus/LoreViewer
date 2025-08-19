using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Core.Outline
{
  public interface IOutlineProvider
  {
    IReadOnlyList<OutlineItem> BuildOutlineItems(IEnumerable<LoreEntity> models);
  }

  public class OutlineProvider : IOutlineProvider
  {
    IReadOnlyList<OutlineItem> IOutlineProvider.BuildOutlineItems(IEnumerable<LoreEntity> models)
    {
      var ret = new List<OutlineItem>();


      return ret;
    }
  }

  public class ShallowOutlineProvider : IOutlineProvider
  {
    public IReadOnlyList<OutlineItem> BuildOutlineItems(IEnumerable<LoreEntity> models)
    {
      var ret = new List<OutlineItem>();

      foreach (var element in models)
      {
        if(element is LoreCollection lc)
        {
          List<LoreElement> childrenOfCollection = lc.HasCollections ? lc.Collections.ToList<LoreElement>() : lc.Nodes.ToList<LoreElement>();
          IReadOnlyList<OutlineItem> childOutlines = BuildOutlineItems(childrenOfCollection);
          OutlineItem colParent = new OutlineItem(lc, lc.Name, childOutlines);
          ret.Add(colParent);
        }
        // If not a collection, must be a node (composite or regular)
        else
        {
          ret.Add(new OutlineItem(element, element.Name, null));
        }
      }

      return ret;
    }
  }
}
