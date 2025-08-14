using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.ViewModels.LoreEntities;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.ViewModels
{
  internal class LoreNodeViewModel : LoreEntityViewModel
  {
    private ILoreNode Node => entity as ILoreNode;

    private string m_sModifiedContent;

    public string FileContent
    {
      get
      {
        if (Node is LoreNode n) return string.IsNullOrWhiteSpace(m_sModifiedContent) ? n.FileContent : m_sModifiedContent;
        else return "MULITPLE FILES";
      }
      set
      {
        m_sModifiedContent = value;
      }
    }

    private ObservableCollection<AttributeViewModel> m_oAttributes;
    public ObservableCollection<AttributeViewModel> Attributes { get => m_oAttributes; }

    private int m_iCursorIndex;

    public int CursorIndex
    {
      get
      {
        if (Node is LoreNode n) return n.LineNumber;
        else return 0;
      }
    }

    public void ClearModifications() => m_sModifiedContent = string.Empty;

    public LoreNodeViewModel(ILoreNode node) 
    { 
      entity = node as LoreEntity;
      m_oAttributes = new ObservableCollection<AttributeViewModel>(node.Attributes.Select(s => new AttributeViewModel(s)));
    }
  }
}
