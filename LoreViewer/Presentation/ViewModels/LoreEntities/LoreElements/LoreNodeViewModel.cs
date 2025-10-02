using LoreViewer.Domain.Entities;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  internal class LoreNodeViewModel : LoreNarrativeElementViewModel
  {
    private LoreNode Node => entity as LoreNode;

    public bool IsCompositeNode => entity is LoreCompositeNode;

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

    public string NarrativeText { get => Node.Summary; }
    public bool HasNarrativeText { get => !string.IsNullOrWhiteSpace(NarrativeText); }


    private ObservableCollection<LoreAttributeViewModel> m_oAttributes;
    public ObservableCollection<LoreAttributeViewModel> Attributes { get => m_oAttributes; }

    public bool HasAttributes { get => m_oAttributes != null && m_oAttributes.Count > 0; }


    private ObservableCollection<LoreSectionViewModel> m_oSections;
    public ObservableCollection<LoreSectionViewModel> Sections { get => m_oSections; }
    public bool HasSections { get => m_oSections != null && m_oSections.Count > 0; }


    private ObservableCollection<LoreCollectionViewModel> m_oCollections;
    public ObservableCollection<LoreCollectionViewModel> Collections { get => m_oCollections; }
    public bool HasCollections { get => m_oCollections != null && m_oCollections.Count > 0; }


    private ObservableCollection<LoreNodeViewModel> m_oEmbeddedNodes;
    public ObservableCollection<LoreNodeViewModel> EmbeddedNodes { get => m_oEmbeddedNodes; }
    public bool HasEmbeddedNodes { get => m_oEmbeddedNodes != null && m_oEmbeddedNodes.Count > 0; }


    private int m_iCursorIndex;

    public int CursorIndex
    {
      get
      {
        if (Node is LoreNode n) return n.Provenance[0].LineNumber;
        else return 0;
      }
    }

    public override LoreEntityViewModel GetChildVM(LoreEntity eToGet)
    {
      if (entity == eToGet) return this;

      if (m_oAttributes != null)
      {
        foreach (LoreAttributeViewModel a in m_oAttributes)
        {
          LoreEntityViewModel evm = a.GetChildVM(eToGet);
          if (evm != null) return evm;
        }
      }

      if(m_oSections != null)
      {
        foreach(LoreSectionViewModel s in m_oSections)
        {
          LoreEntityViewModel evm = s.GetChildVM(eToGet);
          if (evm != null) return evm;
        }
      }

      if(m_oCollections != null)
      {
        foreach(LoreCollectionViewModel c in m_oCollections)
        {
          LoreEntityViewModel evm = c.GetChildVM(eToGet);
          if (evm != null) return evm;
        }
      }
      

      if(m_oEmbeddedNodes != null)
      {
        foreach(LoreNodeViewModel e in m_oEmbeddedNodes)
        {
          LoreEntityViewModel evm = e.GetChildVM(eToGet);
          if (evm != null) return evm;
        }
      }

      return null;
    }

    public void ClearModifications() => m_sModifiedContent = string.Empty;

    public LoreNodeViewModel(LoreNode node) : base(node as LoreNarrativeElement)
    {
      m_oAttributes = new ObservableCollection<LoreAttributeViewModel>(node.Attributes.Select(s => new LoreAttributeViewModel(s)));
      m_oSections = new ObservableCollection<LoreSectionViewModel>(node.Sections.Select(s => new LoreSectionViewModel(s)));
      m_oCollections = new ObservableCollection<LoreCollectionViewModel>(node.Collections.Select(c => new LoreCollectionViewModel(c)));
      m_oEmbeddedNodes = new ObservableCollection<LoreNodeViewModel>(node.Nodes.Select(n => new LoreNodeViewModel(n)));
    }
  }
}
