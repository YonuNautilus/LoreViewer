using LoreViewer.Domain.Entities;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.LoreEntities
{
  /// <summary>
  /// Used for displaying BASIC info in a listview consisting of LoreNodes/LoreCompositeNodes and Collections.
  /// </summary>
  public class ItemOutlineViewModel : ViewModelBase
  {
    private LoreEntity m_oEntity;
    public string DisplayName { get => m_oEntity.Name; }

    public bool IsElementANode { get => m_oEntity is ILoreNode; }

    public ObservableCollection<ItemOutlineViewModel> Items { get; } = new ObservableCollection<ItemOutlineViewModel>();

    public ItemOutlineViewModel(LoreEntity entity)
    {
      m_oEntity = entity;
      if (m_oEntity is LoreCollection col) foreach (LoreEntity e in col) Items.Add(new ItemOutlineViewModel(e));
    }
  }
}
