using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LoreViewer.Presentation.Views.Controls;

public partial class CollectionListView : UserControl
{
  public readonly static StyledProperty<ObservableCollection<LoreCollectionViewModel>> CollectionsProperty
      = AvaloniaProperty.Register<CollectionListView, ObservableCollection<LoreCollectionViewModel>>(nameof(Collections));

  public ObservableCollection<LoreCollectionViewModel> Collections
  {
    get => GetValue(CollectionsProperty);
    set => SetValue(CollectionsProperty, value);
  }

  public CollectionListView()
  {
    InitializeComponent();
  }
}