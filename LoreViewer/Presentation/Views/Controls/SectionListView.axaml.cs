using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Views.Controls;

public partial class SectionListView : UserControl
{
  public readonly static StyledProperty<ObservableCollection<LoreSectionViewModel>> SectionsProperty
      = AvaloniaProperty.Register<SectionListView, ObservableCollection<LoreSectionViewModel>>(nameof(Sections));

  public ObservableCollection<LoreSectionViewModel> Sections
  {
    get => GetValue(SectionsProperty);
    set => SetValue(SectionsProperty, value);
  }

  public SectionListView()
  {
    InitializeComponent();
  }
}