using LoreViewer.Core.Outline;
using LoreViewer.Core.Validation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.ViewModels
{
  public class OutlineItemViewModel : ViewModelBase
  {
    public OutlineItemViewModel(OutlineItem model) {
      m_oOutlineItem = model;
      if (m_oOutlineItem.children != null)
        Children = new(m_oOutlineItem.children.Select(o => new OutlineItemViewModel(o)));
    }

    private OutlineItem m_oOutlineItem;

    public OutlineItem model => m_oOutlineItem;

    public bool HasChildren => Children != null && Children.Count > 0;

    public ObservableCollection<OutlineItemViewModel> Children { get; private set; }

    public string DisplayName { get => m_oOutlineItem.displayName; }

    public EValidationState ValidationState { get => m_oOutlineItem.validationState; }
  }
}
