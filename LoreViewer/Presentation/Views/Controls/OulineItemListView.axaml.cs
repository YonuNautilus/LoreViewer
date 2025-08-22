using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.Modes;
using System;
using System.Collections.ObjectModel;

namespace LoreViewer.Presentation.Views.Controls;

public partial class OulineItemListView : UserControl
{
  //public static readonly StyledProperty<string> LabelTextProperty
  //  = AvaloniaProperty.Register<OulineItemListView, string>(nameof(LabelText), defaultValue: "Entities");

  //public static readonly StyledProperty<ObservableCollection<OutlineItemViewModel>> OutlineItemsProperty
  //  = AvaloniaProperty.Register<OulineItemListView, ObservableCollection<OutlineItemViewModel>>(nameof(OutlineItems));

  //public string LabelText
  //{
  //  get => GetValue(LabelTextProperty);
  //  set => SetValue(LabelTextProperty, value);
  //}

  //public ObservableCollection<OutlineItemViewModel> OutlineItems
  //{
  //  get => GetValue(OutlineItemsProperty);
  //  set => SetValue(OutlineItemsProperty, value);
  //}

  public OulineItemListView()
  {
    InitializeComponent();
  }

  private void TreeDataGrid_DataContextChanged(object sender, EventArgs e)
  {
    if (DataContext is not LoreModeViewModel vm) return;
    OutlineList.Source = vm.OutlineTreeData;
    OutlineList.Source.Selection = vm.RowSelection;
  }
}