using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using System.Linq;
using YamlDotNet.Serialization.NamingConventions;

namespace LoreViewer.ViewModels.LoreEntities
{
  public class ILoreNodeViewModel : ViewModelBase
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
