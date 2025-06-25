using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using LoreViewer.ViewModels;
using Markdig.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LoreViewer.Settings
{
  public class DiffLineVM : ViewModelBase
  {
    public string Text { get; set; } = string.Empty;

    private IBrush _lineColor;

    public IBrush LineColor
    {
      get
      {
        if (_lineColor == null)
        {
          Application.Current.TryFindResource("ThemeForegroundBrush", Application.Current.ActualThemeVariant, out object foregroundBrush);
          if (foregroundBrush != null && foregroundBrush is SolidColorBrush scb)
            _lineColor = scb;
          else
            _lineColor = Brushes.Black;
        }
        return _lineColor;
      }
      set
      {
        _lineColor = value;
      }
    } 
  }

  public class DiffRowVM : ViewModelBase
  {
    public DiffLineVM LeftLine { get; }
    public DiffLineVM RightLine { get; }
    public DiffRowVM(DiffLineVM left, DiffLineVM right)
    {
      LeftLine = left;
      RightLine = right;
    }
  }

  public class DiffVM : ViewModelBase
  {
    public ObservableCollection<DiffRowVM> Rows { get; }

    public DiffVM(SideBySideDiffModel diffModel)
    {
      for (int i = 0; i < Math.Max(diffModel.OldText.Lines.Count, diffModel.NewText.Lines.Count); i++)
      {

      }
    }
  }

  public class SettingsDiffer
  {
    private SideBySideDiffBuilder sideBySide = new SideBySideDiffBuilder(new Differ());


    public SideBySideDiffModel DoSideBySideCompare(string original, string current)
    {
      SideBySideDiffModel model = CompareYAML(original, current);
      return model;
    }

    public DiffVM DoCompare(string original, string current)
    {
      var lists = ToLineVMs(original, current);
      return new DiffVM(lists.left,lists.right);
    }

    public SideBySideDiffModel CompareYAML(string originalYAML, string newYAML) => sideBySide.BuildDiffModel(originalYAML, newYAML);

    public (List<DiffLineVM> left, List<DiffLineVM> right) ToLineVMs(string originalYaml, string newYaml)
    {
      var left = new List<DiffLineVM>();
      var right = new List<DiffLineVM>();

      var diff = InlineDiffBuilder.Diff(originalYaml, newYaml);
      var savedColor = Console.ForegroundColor;

      foreach (var line in diff.Lines)
      {
        switch (line.Type)
        {
          case ChangeType.Deleted:
            left.Add(new DiffLineVM { Text = line.Text, LineColor = Brushes.Red });
            right.Add(new DiffLineVM());
            break;
          case ChangeType.Inserted:
            left.Add(new DiffLineVM());
            right.Add(new DiffLineVM { Text = line.Text, LineColor = Brushes.Green });
            break;
          case ChangeType.Modified:
            left.Add(new DiffLineVM { Text = line.Text, LineColor = Brushes.Blue });
            right.Add(new DiffLineVM { Text = line.Text, LineColor = Brushes.Blue });
            break;
          default:
            left.Add(new DiffLineVM { Text = line.Text });
            right.Add(new DiffLineVM { Text = line.Text });
            break;
        }
      }
      return (left, right);
    }
  }

}
