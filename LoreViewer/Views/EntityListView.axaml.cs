using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels;
using System.Collections;
using System.Collections.ObjectModel;

namespace LoreViewer.Views;

public partial class EntityListView : UserControl
{
  public static readonly StyledProperty<string> LabelTextProperty
    = AvaloniaProperty.Register<EntityListView, string>(nameof(LabelText), defaultValue: "Entities");

  public static readonly StyledProperty<ObservableCollection<LoreTreeItem>> TreeItemsProperty
    = AvaloniaProperty.Register<EntityListView, ObservableCollection<LoreTreeItem>>(nameof(TreeItems));

  public string LabelText
  {
    get => GetValue(LabelTextProperty);
    set => SetValue(LabelTextProperty, value);
  }

  public ObservableCollection<LoreTreeItem> TreeItems
  {
    get => GetValue(TreeItemsProperty);
    set => SetValue(TreeItemsProperty, value);
  }

  public EntityListView()
  {
    InitializeComponent();
  }
}