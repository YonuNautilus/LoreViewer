using Avalonia;
using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.Generic;
using System.Windows.Input;

namespace LoreViewer.Views.DefinitionViews;

public partial class FieldDefinitionListPanel : UserControl
{
  public static readonly StyledProperty<IEnumerable<FieldDefinitionViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<FieldDefinitionListPanel, IEnumerable<FieldDefinitionViewModel>>(nameof(ItemsSource));

  public IEnumerable<FieldDefinitionViewModel> ItemsSource
  {
    get => GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly StyledProperty<ICommand> AddFieldCommandProperty =
        AvaloniaProperty.Register<FieldDefinitionListPanel, ICommand>(nameof(AddFieldCommand));

  public ICommand AddFieldCommand
  {
    get => GetValue(AddFieldCommandProperty);
    set => SetValue(AddFieldCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> DeleteFieldCommandProperty =
        AvaloniaProperty.Register<FieldDefinitionListPanel, ICommand>(nameof(DeleteFieldCommand));

  public ICommand DeleteFieldCommand
  {
    get => GetValue(DeleteFieldCommandProperty);
    set => SetValue(DeleteFieldCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> EditFieldCommandProperty =
        AvaloniaProperty.Register<FieldDefinitionListPanel, ICommand>(nameof(EditFieldCommand));

  public ICommand EditFieldCommand
  {
    get => GetValue(EditFieldCommandProperty);
    set => SetValue(EditFieldCommandProperty, value);
  }

  public FieldDefinitionListPanel()
  {
    InitializeComponent();
  }
}