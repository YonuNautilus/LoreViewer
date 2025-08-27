using LoreViewer.Domain.Entities;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.ViewModels.LoreEntities
{
  internal class LoreCompositeNodeViewModel : ILoreNodeViewModel
  {
    private LoreCompositeNode m_oCompNode => entity as LoreCompositeNode;

    public ObservableCollection<LoreNodeViewModel> InternalNodes { get; } = new();

    public LoreCompositeNodeViewModel(LoreCompositeNode oCompNode) : base(oCompNode)
    {

      foreach (LoreNode node in oCompNode.InternalNodes)
        InternalNodes.Add(CreateViewModel(node) as LoreNodeViewModel);
    }
  }
}
