using LoreViewer.Core.Outline;
using LoreViewer.Core.Stores;
using LoreViewer.Domain.Entities;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.Modes
{
  internal class LoreReadonlyViewModel : LoreModeViewModel
  {
    public ObservableCollection<OutlineItemViewModel> OutlineItems { get; } = new();

    public LoreReadonlyViewModel(LoreRepository loreRepo, ValidationStore validationRepo) : base(loreRepo, validationRepo)
    {
      m_oOutlineProvider = new ShallowOutlineProvider();
    }

    

    protected override void ValidationRepoUpdated(object? sender, EventArgs e)
    {
      foreach(OutlineItemViewModel vm in OutlineItems)
        UpdateValidationStateOnItem(vm);
    }

    protected override void LoreRepoUpdated(object? sender, EventArgs e)
    {
      OutlineItems.Clear();

      var newOutlines = m_oOutlineProvider.BuildOutlineItems(m_oLoreRepo.Models);

      foreach (var outlineItem in newOutlines)
      {
        OutlineItems.Add(new OutlineItemViewModel(outlineItem));
      }
    }

    private void UpdateValidationStateOnItem(OutlineItemViewModel vm)
    {
      vm.model.validationState = m_oValidationRepo.Result.GetValidationStateForOutline(vm.model);
      if(vm.HasChildren)
        foreach (OutlineItemViewModel childvm in vm.Children)
          UpdateValidationStateOnItem(childvm);
    }


  }
}
