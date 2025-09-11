using DynamicData;
using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Domain.Entities;
using LoreViewer.Presentation.Services;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using ReactiveUI;
using System;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.Modes
{
  internal class LoreReadonlyViewModel : LoreModeViewModel
  {

    public LoreReadonlyViewModel(LoreRepository loreRepo, ValidationStore validationRepo) : base(loreRepo, validationRepo)
    {
      m_oEntityVMProvider = new EntityViewModelProvider();
    }
    

    protected override void ValidationRepoUpdated(object? sender, EventArgs e)
    {
      //foreach(LoreEntityViewModel vm in OutlineItems)
        //UpdateValidationStateOnItem(vm);
    }

    protected override void LoreRepoUpdated(object? sender, EventArgs e)
    {
      Entities = m_oEntityVMProvider.BuildEntities(m_oLoreRepo);

      var newSrc = EntityTreeDataGridSourceBuilder.BuildShallowTreeSource(Entities, m_oValidationRepo);
      RowSelection = new Avalonia.Controls.Selection.TreeDataGridRowSelectionModel<LoreEntityViewModel>(newSrc);

      RowSelection.SelectionChanged += (e, _) =>
      {
        SelectedElement = RowSelection.SelectedItem;
      };

      newSrc.Selection = RowSelection;

      EntityTreeData = newSrc;

      ParseErrors.Clear();

      if (m_oLoreRepo.Errors != null && m_oLoreRepo.Errors.Any())
      {
        foreach(ParseError pe in m_oLoreRepo.Errors)
          ParseErrors.Add(new ParseErrorViewModel(pe));

      }

      this.RaisePropertyChanged(nameof(HasErrors));
    }

    protected override void GoToEntity(LoreEntity entityToGoTo)
    {
      LoreEntityViewModel vmToSelect = null;

      foreach(LoreEntityViewModel levm in Entities)
      {
        LoreEntityViewModel v = levm.GetChildVM(entityToGoTo);
        if (v != null)
        { 
          vmToSelect = v;
          break;
        }
      }

      if (vmToSelect == null) return;

      RowSelection.Select(EntityTreeData.Items.IndexOf(vmToSelect));
    }
  }
}
