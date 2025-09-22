using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Views.Controls;

public partial class NodeListView : UserControl
{
  internal readonly static StyledProperty<ObservableCollection<LoreNodeViewModel>> NodesProperty
      = AvaloniaProperty.Register<NodeListView, ObservableCollection<LoreNodeViewModel>>(nameof(Nodes));

  internal ObservableCollection<LoreNodeViewModel> Nodes
  {
    get { return GetValue(NodesProperty); }
    set { SetValue(NodesProperty, value); }
  }

  public NodeListView()
  {
    InitializeComponent();
  }
}