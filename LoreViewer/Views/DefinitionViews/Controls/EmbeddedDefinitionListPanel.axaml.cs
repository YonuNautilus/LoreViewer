using Avalonia;
using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.Generic;
using System.Windows.Input;

namespace LoreViewer.Views.DefinitionViews;

public partial class EmbeddedDefinitionListPanel : UserControl
{
  public static readonly StyledProperty<IEnumerable<EmbeddedNodeDefinitionViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<EmbeddedDefinitionListPanel, IEnumerable<EmbeddedNodeDefinitionViewModel>>(nameof(ItemsSource));

  public IEnumerable<EmbeddedNodeDefinitionViewModel> ItemsSource
  {
    get => GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly StyledProperty<ICommand> AddEmbeddedCommandProperty =
        AvaloniaProperty.Register<EmbeddedDefinitionListPanel, ICommand>(nameof(AddEmbeddedCommand));

  public ICommand AddEmbeddedCommand
  {
    get => GetValue(AddEmbeddedCommandProperty);
    set => SetValue(AddEmbeddedCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> DeleteEmbeddedCommandProperty =
        AvaloniaProperty.Register<EmbeddedDefinitionListPanel, ICommand>(nameof(DeleteEmbeddedCommand));

  public ICommand DeleteEmbeddedCommand
  {
    get => GetValue(DeleteEmbeddedCommandProperty);
    set => SetValue(DeleteEmbeddedCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> EditEmbeddedCommandProperty =
        AvaloniaProperty.Register<EmbeddedDefinitionListPanel, ICommand>(nameof(EditEmbeddedCommand));

  public ICommand EditEmbeddedCommand
  {
    get => GetValue(EditEmbeddedCommandProperty);
    set => SetValue(EditEmbeddedCommandProperty, value);
  }

  public EmbeddedDefinitionListPanel()
  {
    InitializeComponent();
  }
}