using DynamicData;
using LoreViewer.LoreElements;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.LoreEntities
{
  internal class LoreCompositeNodeViewModel : LoreEntityViewModel
  {
    private LoreCompositeNode m_oCompNode => entity as LoreCompositeNode;

    public ObservableCollection<LoreNodeViewModel> InternalNodes { get; } = new();

    public LoreCompositeNodeViewModel(LoreCompositeNode oCompNode)
    {
      entity = oCompNode;
      foreach (LoreNode node in oCompNode.InternalNodes)
        InternalNodes.Add(LoreEntityViewModel.CreateViewModel(node) as LoreNodeViewModel);
    }
  }
}
