using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media.Imaging;
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
      m_oInlines = new ObservableCollection<NarrativeInlineViewModel>(line.Inlines.Select(i => NarrativeInlineViewModel.CreateVM(i)));
    }
  }

  public abstract class NarrativeInlineViewModel : ViewModelBase
  {
    public LoreNarrativeInline m_oInline;
    public NarrativeInlineViewModel(LoreNarrativeInline inline) { m_oInline = inline; }

    public static NarrativeInlineViewModel CreateVM(LoreNarrativeInline inline)
    {
      switch (inline)
      {
        case LoreNarrativeTextInline t:
          return new NarrativeInlineTextVM(t);
        case LoreNarrativeLinkInline l:
          return new NarrativeInlineLinkVM(l);
        case LoreNarrativeImageInline i:
          return new NarrativeInlineImageVM(i);
        default:
          return null;
      }
    }
  }

  public class NarrativeInlineTextVM : NarrativeInlineViewModel
  {
    public string Text { get => (m_oInline as LoreNarrativeTextInline).Text; }
    public NarrativeInlineTextVM(LoreNarrativeTextInline inline) : base(inline) { }
  }

  public class NarrativeInlineImageVM : NarrativeInlineViewModel
  {
    public Bitmap Image { get => new Bitmap(ImagePath); }
    public string ImagePath { get => (m_oInline as LoreNarrativeImageInline).ImagePath; }
    public NarrativeInlineImageVM(LoreNarrativeImageInline inline) : base(inline) { }
  }

  public class NarrativeInlineLinkVM : NarrativeInlineViewModel
  {
    public string DisplayText { get => (m_oInline as LoreNarrativeLinkInline).Label; }
    public string Path { get => (m_oInline as LoreNarrativeLinkInline).Path; }
    public NarrativeInlineLinkVM(LoreNarrativeLinkInline inline) : base(inline) { }
  }
}
