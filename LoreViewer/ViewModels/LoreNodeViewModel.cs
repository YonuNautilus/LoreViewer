using LoreViewer.LoreElements;
using System.Reflection.Metadata.Ecma335;

namespace LoreViewer.ViewModels
{
  internal class LoreNodeViewModel
  {
    private LoreNode Node;

    private string m_sModifiedContent;

    public string FileContent
    {
      get
      {
        return string.IsNullOrWhiteSpace(m_sModifiedContent) ? Node.FileContent : m_sModifiedContent;
      }
      set
      {
        m_sModifiedContent = value;
      }
    }

    private int m_iCursorIndex;

    public int CursorIndex
    {
      get => Node.LineNumber;
    }

    public void ClearModifications() => m_sModifiedContent = string.Empty;

    public LoreNodeViewModel(LoreNode node) { Node = node; }
  }
}
