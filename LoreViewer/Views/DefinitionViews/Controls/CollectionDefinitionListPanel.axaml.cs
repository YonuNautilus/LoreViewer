using Avalonia;
using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.Generic;
using System.Windows.Input;

namespace LoreViewer.Views.DefinitionViews;

public partial class CollectionDefinitionListPanel : UserControl
{
  public static readonly StyledProperty<IEnumerable<CollectionDefinitionViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<CollectionDefinitionListPanel, IEnumerable<CollectionDefinitionViewModel>>(nameof(ItemsSource));

  public IEnumerable<CollectionDefinitionViewModel> ItemsSource
  {
    get => GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly StyledProperty<ICommand> AddCollectionCommandProperty =
        AvaloniaProperty.Register<CollectionDefinitionListPanel, ICommand>(nameof(AddCollectionCommand));

  public ICommand AddCollectionCommand
  {
    get => GetValue(AddCollectionCommandProperty);
    set => SetValue(AddCollectionCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> DeleteCollectionCommandProperty =
        AvaloniaProperty.Register<CollectionDefinitionListPanel, ICommand>(nameof(DeleteCollectionCommand));

  public ICommand DeleteCollectionCommand
  {
    get => GetValue(DeleteCollectionCommandProperty);
    set => SetValue(DeleteCollectionCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> EditCollectionCommandProperty =
        AvaloniaProperty.Register<CollectionDefinitionListPanel, ICommand>(nameof(EditCollectionCommand));

  public ICommand EditCollectionCommand
  {
    get => GetValue(EditCollectionCommandProperty);
    set => SetValue(EditCollectionCommandProperty, value);
  }

  public CollectionDefinitionListPanel()
  {
    InitializeComponent();
  }
}