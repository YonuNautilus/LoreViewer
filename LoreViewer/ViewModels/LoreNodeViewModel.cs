using LoreViewer.LoreElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
  internal class LoreNodeViewModel
  {
    private LoreNode Node;

    private string m_sModifiedContent;

    public string FileContent {
      get
      {
        return string.IsNullOrWhiteSpace(m_sModifiedContent) ? Node.FileContent : m_sModifiedContent;
      }
      set
      {
        m_sModifiedContent = value;
      }
    }

    public void ClearModifications() => m_sModifiedContent = string.Empty;

    public LoreNodeViewModel(LoreNode node) { Node = node; }
  }
}
