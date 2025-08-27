using LoreViewer.Core.Stores;
using LoreViewer.Domain.Entities;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Presentation.Services
{
  public class EntityViewModelProvider
  {
    public ObservableCollection<LoreEntityViewModel> BuildEntities(LoreRepository repo)
    {
      ObservableCollection<LoreEntityViewModel> entities = new();

      foreach(LoreEntity le in repo.Models)
      {
        entities.Add(LoreEntityViewModel.CreateViewModel(le));
      }
      return entities;
    }
  }
}
