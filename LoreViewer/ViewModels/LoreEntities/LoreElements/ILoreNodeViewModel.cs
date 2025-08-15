using LoreViewer.Domain.Entities;
using System.Linq;

namespace LoreViewer.ViewModels.LoreEntities
{
  public class ILoreNodeViewModel : LoreEntityViewModel
  {
    private ILoreNode m_oNode { get; }

    public string[] files
    {
      get
      {
        if (m_oNode is LoreCompositeNode lcn)
          return lcn.InternalNodes.Select(ln => ln.SourcePath).ToArray();
        else
          return new string[] { (m_oNode as LoreNode).SourcePath };
      }
    }

    public ILoreNodeViewModel(ILoreNode node) { m_oNode = node; }

  }
}
