using Avalonia.Controls;
using Avalonia.Controls.Selection;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Presentation.Services;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.Modes
{
  public abstract class LoreModeViewModel : ViewModelBase
  {
    protected EntityViewModelProvider m_oEntityVMProvider;

    protected LoreRepository m_oLoreRepo;
    protected ValidationStore m_oValidationRepo;

    public ObservableCollection<LoreEntityViewModel> Entities { get; protected set; } = new ObservableCollection<LoreEntityViewModel>();

    public ObservableCollection<ParseErrorViewModel> ParseErrors { get; protected set; } = new ObservableCollection<ParseErrorViewModel>();

    public bool HasErrors { get => m_oLoreRepo?.Errors != null && m_oLoreRepo.Errors.Any(); }

    private bool m_bIsErrorListExpanded = true;
    public bool IsErrorListExpanded
    {
      get
      {
        return m_bIsErrorListExpanded;
      }
      set
      {
        this.RaiseAndSetIfChanged(ref m_bIsErrorListExpanded, value, nameof(IsErrorListExpanded));
      }
    }

    public GridLength RowHeight3 { get; } = new GridLength(3);
    public GridLength RowHeight1Star { get; } = new GridLength(200);


    private HierarchicalTreeDataGridSource<LoreEntityViewModel> m_oEntityTreeData;
    public HierarchicalTreeDataGridSource<LoreEntityViewModel> EntityTreeData
    {
      get
      {
        return m_oEntityTreeData;
      }
      protected set
      {
        this.RaiseAndSetIfChanged(ref m_oEntityTreeData, value, nameof(EntityTreeData));
      }
    }

    public TreeDataGridRowSelectionModel<LoreEntityViewModel> RowSelection { get; protected set; }
    public IEnumerable<LoreValidationMessage> ValidationMessagesForCurrentElement
    {
      get
      {
        
        if (SelectedElement != null)
          return m_oValidationRepo.Result.GetValidationMessagesForOutline(SelectedElement.entity, this is LoreReadonlyViewModel);
        else return null;
      }
    }

    private LoreEntityViewModel m_oSelectedElement;
    public LoreEntityViewModel SelectedElement
    {
      get { return m_oSelectedElement; }
      set
      {
        this.RaiseAndSetIfChanged(ref m_oSelectedElement, value, nameof(SelectedElement));
        this.RaisePropertyChanged(nameof(ValidationMessagesForCurrentElement));
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
