using LoreViewer.Domain.Entities;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  internal class LoreNodeViewModel : ILoreNodeViewModel
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

    public LoreNodeViewModel(LoreNode node) : base(node) { }
  }
}
