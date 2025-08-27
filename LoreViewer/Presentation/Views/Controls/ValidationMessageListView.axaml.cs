using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using LoreViewer.Core.Validation;
using LoreViewer.Presentation.ViewModels.Modes;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive;

namespace LoreViewer.Presentation.Views.Controls;

public partial class ValidationMessageListView : UserControl
{
  public static readonly StyledProperty<IEnumerable<LoreValidationMessage>> ItemsProperty =
    AvaloniaProperty.Register<ValidationMessageListView, IEnumerable<LoreValidationMessage>?>(nameof(Items));

  public static readonly StyledProperty<ReactiveCommand<string, Unit>> OpenFileAtCommandProperty =
    AvaloniaProperty.Register<ValidationMessageListView, ReactiveCommand<string, Unit>?> (nameof(OpenFileAtCommand));

  public IEnumerable<LoreValidationMessage>? Items
  {
    get => GetValue(ItemsProperty);
    set => SetValue(ItemsProperty, value);
  }

  public ReactiveCommand<string, Unit> OpenFileAtCommand
  {
    get => GetValue(OpenFileAtCommandProperty);
    set => SetValue(OpenFileAtCommandProperty, value);
  }

  public bool ListIsCollapsed { get => !ValidationList.IsVisible; }

  public ValidationMessageListView()
  {
    InitializeComponent();
  }

  private void Button_Tapped(object? sender, TappedEventArgs args) { ValidationList.IsVisible = !ValidationList.IsVisible; }
}