using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.Modes;
using System;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Views.Controls;

public partial class LoreElementListView : UserControl
{

  public LoreElementListView()
  {
    InitializeComponent();
  }

  private void TreeDataGrid_DataContextChanged(object sender, EventArgs e)
  {
    if (DataContext is not LoreModeViewModel vm) return;
    OutlineList.Source = vm.EntityTreeData;
    OutlineList.Source.Selection = vm.RowSelection;
  }
}