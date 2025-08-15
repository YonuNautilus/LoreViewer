using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using LoreViewer.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace LoreViewer.Core.Settings
{
  public class DiffRowVM : ViewModelBase
  {
    public string LeftLine { get; }
    public string RightLine { get; }
    public IBrush LeftBrush { get; }
    public IBrush RightBrush { get; }
    public DiffRowVM(string leftText, string rightText, IBrush leftColor, IBrush rightColor)
    {
      LeftLine = leftText;
      LeftBrush = leftColor;
      RightLine = rightText;
      RightBrush = rightColor;
    }
  }

  public class SettingsDiffer
  {
    private SideBySideDiffBuilder sideBySide = new SideBySideDiffBuilder(new Differ());


    public ObservableCollection<DiffRowVM> DoSideBySideCompare(string original, string current)
    {
      SideBySideDiffModel model = CompareYAML(original, current);
      return MakeRows(model);
    }

    public SideBySideDiffModel CompareYAML(string originalYAML, string newYAML) => sideBySide.BuildDiffModel(originalYAML, newYAML);

    public static ObservableCollection<DiffRowVM> MakeRows(SideBySideDiffModel diffModel)
    {
      IBrush b;
      var p = Application.Current.TryFindResource("ThemeForegroundBrush", Application.Current.ActualThemeVariant, out object foregroundBrush);
      if (foregroundBrush != null && foregroundBrush is SolidColorBrush scb)
        b = scb;
      else
        b = Brushes.Black;

      ObservableCollection<DiffRowVM> ret = new ObservableCollection<DiffRowVM>();

      for (int i = 0; i < Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count); i++)
      {
        var oldLine = i < diffModel.OldText.Lines.Count ? diffModel.OldText.Lines[i] : null;
        var newLine = i < diffModel.NewText.Lines.Count ? diffModel.NewText.Lines[i] : null;

        IBrush leftBrush = oldLine?.Type switch
        {
          ChangeType.Deleted => Brushes.Red,
          ChangeType.Modified => Brushes.Blue,
          _ => b
        };

        IBrush rightBrush = newLine?.Type switch
        {
          ChangeType.Inserted => Brushes.Green,
          ChangeType.Modified => Brushes.Blue,
          _ => b
        };

        ret.Add(new DiffRowVM
          (
          leftText: oldLine != null ? oldLine.Text : string.Empty,
          rightText: newLine != null ? newLine.Text : string.Empty,
          leftColor: leftBrush,
          rightColor: rightBrush
          ));

      }
      return ret;
    }
  }

}
