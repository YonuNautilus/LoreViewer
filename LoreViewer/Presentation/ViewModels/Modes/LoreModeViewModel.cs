using LoreViewer.Core.Outline;
using LoreViewer.Core.Stores;
using System;
using System.Collections.Generic;
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
