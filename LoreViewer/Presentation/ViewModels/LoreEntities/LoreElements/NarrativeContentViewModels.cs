using LoreViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class NarrativeBlockViewModel : ViewModelBase
  {
    private ObservableCollection<NarrativeLineViewModel> m_oLines;

    public ObservableCollection<NarrativeLineViewModel> Lines { get => m_oLines; }

    public NarrativeBlockViewModel(LoreNarrativeBlock block)
    {

    }
  }

  public class NarrativeLineViewModel : ViewModelBase
  {
    private ObservableCollection<NarrativeInlineViewModel> m_oInlines;
    public ObservableCollection<NarrativeInlineViewModel> Inlines { get => m_oInlines; }

    public NarrativeLineViewModel(LoreNarrativeLine line)
    {

    }
  }

  public class NarrativeInlineViewModel : ViewModelBase
  {
    
  }

}
