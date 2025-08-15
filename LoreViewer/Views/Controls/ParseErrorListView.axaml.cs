using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using LoreViewer.ViewModels;
using System.Collections.ObjectModel;

namespace LoreViewer.Views;

public partial class ParseErrorListView : UserControl
{
  public static readonly StyledProperty<ObservableCollection<ParseErrorViewModel>> ParseErrorsProperty
    = AvaloniaProperty.Register<ParseErrorListView, ObservableCollection<ParseErrorViewModel>>(nameof(ParseErrors));

  public ObservableCollection<ParseErrorViewModel> ParseErrors
  {
    get => GetValue(ParseErrorsProperty);
    set => SetValue(ParseErrorsProperty, value);
  }

  public ParseErrorListView()
  {
    InitializeComponent();
  }

  private void Button_Tapped(object? sender, TappedEventArgs args) { ErrorsList.IsVisible = !ErrorsList.IsVisible; }
}