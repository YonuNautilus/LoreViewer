using LoreViewer.LoreElements;

namespace LoreViewer.ViewModels.LoreEntities
{
  internal class LoreNodeViewModel : LoreElementViewModel
  {
    internal LoreNode m_oNode => entity as LoreNode;

    private string m_sFileContent;

    private string markdownText;
    public string FileContent
    {
      get
      {
        if (IsDirty) return markdownText;
        else
        {
          // hold on to non-dirt file contents
          markdownText = m_sFileContent = m_oNode.FileContent;
          return markdownText;
        }
      }
      set
      {
        IsDirty = m_sFileContent != value;
        if (IsDirty) markdownText = value;
      }
    }
    
    public LoreNodeViewModel(LoreNode node) { entity = node; }
  }
}
