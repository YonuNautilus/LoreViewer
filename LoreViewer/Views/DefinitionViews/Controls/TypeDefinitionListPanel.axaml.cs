using Avalonia;
using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.Generic;
using System.Windows.Input;

namespace LoreViewer.Views.DefinitionViews;

public partial class TypeDefinitionListPanel : UserControl
{
  public static readonly StyledProperty<IEnumerable<TypeDefinitionViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<TypeDefinitionListPanel, IEnumerable<TypeDefinitionViewModel>>(nameof(ItemsSource));

  public IEnumerable<TypeDefinitionViewModel> ItemsSource
  {
    get => GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly StyledProperty<ICommand> AddTypeCommandProperty =
        AvaloniaProperty.Register<TypeDefinitionListPanel, ICommand>(nameof(AddTypeCommand));

  public ICommand AddTypeCommand
  {
    get => GetValue(AddTypeCommandProperty);
    set => SetValue(AddTypeCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> DeleteTypeCommandProperty =
        AvaloniaProperty.Register<TypeDefinitionListPanel, ICommand>(nameof(DeleteTypeCommand));

  public ICommand DeleteTypeCommand
  {
    get => GetValue(DeleteTypeCommandProperty);
    set => SetValue(DeleteTypeCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> EditTypeCommandProperty =
        AvaloniaProperty.Register<TypeDefinitionListPanel, ICommand>(nameof(EditTypeCommand));

  public ICommand EditTypeCommand
  {
    get => GetValue(EditTypeCommandProperty);
    set => SetValue(EditTypeCommandProperty, value);
  }
  public TypeDefinitionListPanel()
  {
    InitializeComponent();
  }
}