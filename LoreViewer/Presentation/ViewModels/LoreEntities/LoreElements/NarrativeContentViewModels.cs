using Avalonia.Controls;
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

    public LoreNarrativeBlock m_oBlock;

    public NarrativeBlockViewModel(LoreNarrativeBlock block)
    {
      m_oBlock = block;
      m_oLines = new ObservableCollection<NarrativeLineViewModel>(block.Lines.Select(l => new NarrativeLineViewModel(l)));
    }
  }

  public class NarrativeLineViewModel : ViewModelBase
  {
    private ObservableCollection<NarrativeInlineViewModel> m_oInlines;
    public ObservableCollection<NarrativeInlineViewModel> Inlines { get => m_oInlines; }

    public LoreNarrativeLine m_oLine;

    public NarrativeLineViewModel(LoreNarrativeLine line)
    {
      m_oLine = line;
      m_oInlines = new ObservableCollection<NarrativeInlineViewModel>(line.Inlines.Select(i => new NarrativeInlineViewModel(i)));
    }
  }

  public class NarrativeInlineViewModel : ViewModelBase
  {
    public LoreNarrativeInline m_oInline;

    public Control InlineView;

    public NarrativeInlineViewModel(LoreNarrativeInline inline)
    {
      m_oInline = inline;

      switch (inline)
      {
        case LoreNarrativeTextInline t:
          InlineView = new TextBlock()
          {
            Text = t.Text,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
          };
          break;
      }

    }

  }

}
