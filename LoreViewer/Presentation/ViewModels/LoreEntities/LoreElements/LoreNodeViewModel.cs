using LoreViewer.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
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

    private ObservableCollection<LoreAttributeViewModel> m_oAttributes;
    public ObservableCollection<LoreAttributeViewModel> Attributes { get => m_oAttributes; }


    private ObservableCollection<LoreSectionViewModel> m_oSections;
    public ObservableCollection<LoreSectionViewModel> Sections { get => m_oSections; }

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

    public LoreNodeViewModel(ILoreNode node) : base(node as LoreEntity)
    {
      m_oAttributes = new ObservableCollection<LoreAttributeViewModel>(node.Attributes.Select(s => new LoreAttributeViewModel(s)));
      m_oSections = new ObservableCollection<LoreSectionViewModel>(node.Sections.Select(s => new LoreSectionViewModel(s)));
    }
  }
}
