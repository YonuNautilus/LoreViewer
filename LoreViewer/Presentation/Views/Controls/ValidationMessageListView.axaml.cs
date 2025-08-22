using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Core.Validation;
using ReactiveUI;
using System.Collections;
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

  public ValidationMessageListView()
  {
    InitializeComponent();
  }
}