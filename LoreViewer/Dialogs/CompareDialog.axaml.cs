using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using LoreViewer.Settings;
using LoreViewer.ViewModels.SettingsVMs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LoreViewer.Dialogs;

public partial class CompareDialog : Window
{
  public CompareDialog()
  {
    InitializeComponent();
  }

  public CompareDialog(List<DiffLineVM> left, List<DiffLineVM> right)
  {
    InitializeComponent();
    //LeftBox.ItemsSource = new ObservableCollection<DiffLineVM>(left);
    //RightBox.ItemsSource = new ObservableCollection<DiffLineVM>(right);
  }
}

public static class SettingsDiffTreeDataGridBuilder
{
  public static FlatTreeDataGridSource<DiffRowVM> BuildTreeSource(
}