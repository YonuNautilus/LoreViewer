using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Views.Controls;

public partial class NarrativeView : UserControl
{
  public readonly static StyledProperty<ObservableCollection<NarrativeBlockViewModel>> BlocksProperty
    = AvaloniaProperty.Register<NarrativeView, ObservableCollection<NarrativeBlockViewModel>>(nameof(Blocks));

  public ObservableCollection<NarrativeBlockViewModel> Blocks
  {
    get => GetValue(BlocksProperty);
    set => SetValue(BlocksProperty, value);
  }

  public NarrativeView()
  {
    InitializeComponent();
  }
}