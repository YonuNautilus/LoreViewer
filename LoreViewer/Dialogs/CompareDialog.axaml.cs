using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using LoreViewer.Core.Settings;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace LoreViewer.Dialogs;

public partial class CompareDialog : Window
{

  public CompareDialog()
  {
    InitializeComponent();
  }

  public CompareDialog(LoreSettingsViewModel vm)
  {
    InitializeComponent();

    this.DataContext = vm;

    this.MinWidth = SettingsDiffTreeDataGridBuilder.CurrentMaxWidthLeft + SettingsDiffTreeDataGridBuilder.CurrentMaxWidthRight + 60;
    this.Width = this.MaxWidth = this.MinWidth;
  }

  private async void SaveButton_Click(object sender, RoutedEventArgs e)
  {
    await Dispatcher.UIThread.InvokeAsync(() => this.Close(true), DispatcherPriority.Background);
  }

  private async void CancelButton_Click(object sender, RoutedEventArgs e)
  {
    await Dispatcher.UIThread.InvokeAsync(() => this.Close(false), DispatcherPriority.Background);
  }
}

public static class SettingsDiffTreeDataGridBuilder
{
  public static int CurrentMaxWidthLeft { get; private set; }
  public static int CurrentMaxWidthRight { get; private set; }

  public static FlatTreeDataGridSource<DiffRowVM> BuildTreeSource(ObservableCollection<DiffRowVM> rows)
  {
    double fontSize = 13;
    string font = "Consolas";

    var maxLeftWidth = rows.ToList()
    .Select(r => MeasureStringWidth(r.LeftLine, fontSize, font))
    .DefaultIfEmpty(0)
    .Max();
    CurrentMaxWidthLeft = (int)maxLeftWidth;

    var maxRightWidth = rows
        .Select(r => MeasureStringWidth(r.RightLine, fontSize, font))
        .DefaultIfEmpty(0)
        .Max();
    CurrentMaxWidthRight = (int)maxRightWidth;

    return new FlatTreeDataGridSource<DiffRowVM>(rows)
    {
      Columns =
      {
        new TemplateColumn<DiffRowVM>(
          header: "Original",
          cellTemplate: new FuncDataTemplate<DiffRowVM>((row, _) =>
            new TextBlock{
              [!TextBlock.TextProperty] = new Binding(nameof(DiffRowVM.LeftLine)),
              [!TextBlock.ForegroundProperty] = new Binding(nameof(DiffRowVM.LeftBrush)),
              Padding = new Thickness(0),
              Margin = new Thickness(0),
              FontFamily = new FontFamily("Consolas"),
              FontSize = fontSize,
              VerticalAlignment = VerticalAlignment.Center
          }),
          options: new TemplateColumnOptions<DiffRowVM> { MinWidth = new GridLength(maxLeftWidth) }
        ),
        new TemplateColumn<DiffRowVM>(
          header: "Modified",
          cellTemplate: new FuncDataTemplate<DiffRowVM>((row, _) =>
          new TextBlock{
              [!TextBlock.TextProperty] = new Binding(nameof(DiffRowVM.RightLine)),
              [!TextBlock.ForegroundProperty] = new Binding(nameof(DiffRowVM.RightBrush)),
              Padding = new Thickness(0),
              Margin = new Thickness(0),
              FontFamily = new FontFamily("Consolas"),
              FontSize = fontSize,
              VerticalAlignment = VerticalAlignment.Center
          }),
          options: new TemplateColumnOptions<DiffRowVM> { MinWidth = new GridLength(maxRightWidth) }
        )
      }
    };
  }

  static double MeasureStringWidth(string text, double fontSize, string fontFamily = "Consolas")
  {
    IBrush b;
    var p = Application.Current.TryFindResource("ThemeForegroundBrush", Application.Current.ActualThemeVariant, out object foregroundBrush);
    if (foregroundBrush != null && foregroundBrush is SolidColorBrush scb)
      b = scb;
    else
      b = Brushes.Black;

    var typeface = new Typeface(fontFamily);
    var formatted = new FormattedText(text ?? string.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, fontSize, b);

    return formatted.Width;
  }
}