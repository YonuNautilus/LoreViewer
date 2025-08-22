using Avalonia.Controls;
using Avalonia.Controls.Selection;
using LoreViewer.Core.Outline;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.ViewModels.Modes
{
  public abstract class LoreModeViewModel : ViewModelBase
  {
    protected IOutlineProvider m_oOutlineProvider;

    protected LoreRepository m_oLoreRepo;
    protected ValidationStore m_oValidationRepo;

    private Dictionary<LoreEntity, LoreEntityViewModel> m_dEntityVMCache = new();

    public ObservableCollection<ParseErrorViewModel> ParseErrors { get; protected set; } = new ObservableCollection<ParseErrorViewModel>();

    public bool HasErrors { get => m_oLoreRepo?.Errors != null && m_oLoreRepo.Errors.Any(); }




    private HierarchicalTreeDataGridSource<OutlineItemViewModel> m_oOutlineTreeData;
    public HierarchicalTreeDataGridSource<OutlineItemViewModel> OutlineTreeData
    {
      get
      {
        return m_oOutlineTreeData;
      }
      protected set
      {
        this.RaiseAndSetIfChanged(ref m_oOutlineTreeData, value, nameof(OutlineTreeData));
      }
    }

    public TreeDataGridRowSelectionModel<OutlineItemViewModel> RowSelection { get; protected set; }

    public IEnumerable<LoreValidationMessage> ValidationMessagesForCurrentOutline
    {
      get
      {
        
        if (SelectedOutlineItem != null)
          return m_oValidationRepo.Result.GetValidationMessagesForOutline(SelectedOutlineItem.model, this is LoreReadonlyViewModel);
        else return null;
      }
    }

    private OutlineItemViewModel m_oSelectedOutlineItem;
    public OutlineItemViewModel SelectedOutlineItem
    {
      get { return m_oSelectedOutlineItem; }
      set
      {
        this.RaiseAndSetIfChanged(ref m_oSelectedOutlineItem, value, nameof(SelectedOutlineItem));
        this.RaisePropertyChanged(nameof(ValidationMessagesForCurrentOutline));

        if (!m_dEntityVMCache.ContainsKey(m_oSelectedOutlineItem.model.entity))
          m_dEntityVMCache[m_oSelectedOutlineItem.model.entity] = LoreEntityViewModel.CreateViewModel(m_oSelectedOutlineItem.model.entity);
        
        CurrentEntityVM = m_dEntityVMCache[m_oSelectedOutlineItem.model.entity];
      }
    }

    private LoreEntityViewModel m_oCurrentEntityVM;
    public LoreEntityViewModel CurrentEntityVM
    {
      get { return m_oCurrentEntityVM; }
      set
      {
        this.RaiseAndSetIfChanged(ref m_oCurrentEntityVM, value, nameof(CurrentEntityVM));
      }
    }

    public LoreModeViewModel(LoreRepository loreRepo, ValidationStore validationRepo)
    {
      m_oLoreRepo = loreRepo;
      m_oLoreRepo.LoreRepoUpdated += LoreRepoUpdated;

      m_oValidationRepo = validationRepo;
      m_oValidationRepo.ValidationUpdated += ValidationRepoUpdated;
    }

    protected abstract void LoreRepoUpdated(object? sender, EventArgs e);
    protected abstract void ValidationRepoUpdated(object? sender, EventArgs e);
  }
}
