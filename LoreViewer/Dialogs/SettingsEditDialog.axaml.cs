using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.ObjectModel;

namespace LoreViewer.Dialogs;

public partial class SettingsEditDialog : Window
{
  public SettingsEditDialog()
  {
    InitializeComponent();
  }

  public SettingsEditDialog(LoreSettings settings)
  {
    InitializeComponent();

    var vm = new LoreSettingsViewModel(settings);
    vm.SetView(this);
    LoreDefinitionViewModel.CurrentSettingsViewModel = vm;
    this.DataContext = vm;

    var source = DefinitionTreeDataGridBuilder.BuildTreeSource(vm.TreeRootNodes);
    DefinitionTreeDataGrid.Source = source;

    DefinitionTreeDataGrid.RowSelection.SelectionChanged += (_, e) =>
    {
      if (DefinitionTreeDataGrid.RowSelection?.SelectedItem is DefinitionTreeNodeViewModel selected)
      {
        vm.SelectedNode = selected;
      }
    };

    vm.curTree = DefinitionTreeDataGrid;

    foreach (DefinitionTreeNodeViewModel nodeVM in source.Items)
      nodeVM.RefreshTreeNode();
  }

  private void SaveButtonClick(object sender, RoutedEventArgs e)
  {
    Close((DataContext as LoreSettingsViewModel).m_oLoreSettings);
  }
}


public static class DefinitionTreeDataGridBuilder
{
  public static HierarchicalTreeDataGridSource<DefinitionTreeNodeViewModel> BuildTreeSource(
      ObservableCollection<DefinitionTreeNodeViewModel> roots)
  {
    return new HierarchicalTreeDataGridSource<DefinitionTreeNodeViewModel>(roots)
    {
      Columns = {
        new HierarchicalExpanderColumn<DefinitionTreeNodeViewModel>(
          new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Name",
            width: new GridLength(1, GridUnitType.Star),
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if (node == null) return new Panel();

              TextBox retBox = new TextBox
              {
                [!!TextBox.TextProperty] = new Binding("DisplayName")
                {
                  UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                },
                [!TextBox.BorderThicknessProperty] = new Binding("TextboxBorderThickness"),
                [!TextBox.PaddingProperty] = new Binding("TextboxBorderThickness"),
                [!TextBox.IsReadOnlyProperty] = new Binding("NameIsReadOnly"),
                [!TextBox.IsHitTestVisibleProperty] = new Binding("CanEditName"),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
              };

              return retBox;

            })
          ),
          childSelector: x => x.Children
        ),

      new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Inherited",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if (node == null || node.DefinitionVM == null) return new StackPanel();

              string imgURI = node.DefinitionVM?.Definition is LoreTypeDefinition ? "avares://LoreViewer/Resources/arrow_down.png" : "avares://LoreViewer/Resources/link.png";
              string toolTipText = node.DefinitionVM?.Definition is LoreTypeDefinition ? $"Extends from {node.DefinitionVM?.Definition.Base}" : "Inherited from the type's parent";

              StackPanel ret = new StackPanel
              {
                [!StackPanel.IsVisibleProperty] = new Binding("IsInherited"),
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(5,0,0,0)
              };

              ToolTip.SetTip(ret, toolTipText);

              var img = new Image
              {
                Source = new Bitmap(AssetLoader.Open(new System.Uri(imgURI))),
                Width = 24,
                [!Image.IsVisibleProperty] = new Binding(nameof(node.IsInherited))
              };


              ret.Children.Add(img);

              var label = new Label { [!Label.ContentProperty] = new Binding(nameof(node.InheritanceLabelString)) };
              ret.Children.Add(label);

              ContextMenu inheritanceMenu = new ContextMenu
              {
                [!ContextMenu.IsEnabledProperty] = new Binding(nameof(node.IsInherited))
              };

              inheritanceMenu.Items.Add(new MenuItem
              {
                Header = "Go to base definition",
                Command = node.GoToNodeOfDefinitionCommand,
                CommandParameter = node.DefinitionVM.Definition.Base
              });

              ret.ContextMenu = inheritanceMenu;


              return ret;
            })),

      new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Required",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if (node == null) return new Panel();

              return new CheckBox
              {
                // Note for me: in this, the "!" is an Avalonia thing:
                // !  : one-way binding (viewmodel to view)
                // !! : two-way binding
                // ~  : one-way-to-source binding (view to viewmodel)
                // ?  : set local value only not already set

                [!!ToggleButton.IsCheckedProperty] = new Binding("IsRequired"),
                [!ToggleButton.IsEnabledProperty] = new Binding("CanEditRequired"),
                IsVisible = (node.DefinitionVM?.Definition is IRequirable && node.Parent?.Parent != null),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center

              };
            })),

      new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Actions",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if (node == null)
                return new StackPanel();

              var buttonGrid = new Grid
              {
                ColumnDefinitions = new ColumnDefinitions { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(1, GridUnitType.Star) },
              };

              if (!node.IsGroupNode)
              {
                var deleteButton = new Button
                {
                  Content = new Image() { Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/trash_can.png"))), Margin = new Thickness(-10) },
                  Width = 24,
                  Padding = new Thickness(-5),
                  [!Button.IsEnabledProperty] = new Binding("CanDelete")
                };
                deleteButton.Bind(Button.CommandProperty, new Binding(nameof(node.DeleteCommand)));

                ToolTip.SetTip(deleteButton, "Delete this definition");

                Grid.SetColumn(deleteButton, 0);
                buttonGrid.Children.Add(deleteButton);
              }

              if(node.IsGroupNode ||
                (node.DefinitionVM != null && (
                      (node.DefinitionVM is FieldDefinitionViewModel) || 
                      (node.DefinitionVM is PicklistDefinitionViewModel) ||
                      (node.DefinitionVM is PicklistEntryDefinitionViewModel))))
              {
                // Optional: add "+" add button for group nodes
                var addButton = new Button
                {
                  Content = new Image() { Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/add.png"))), Margin = new Thickness(-10) },
                  Width = 24,
                };
                if (node.Parent != null)
                  addButton.Bind(Button.CommandProperty, new Binding(nameof(node.AddDefinitionCommand)));
                else
                  addButton.Bind(Button.CommandProperty, new Binding(nameof(node.AddDefinitionCommand)));

                if(node.DefinitionVM is FieldDefinitionViewModel fdvm)
                {
                  addButton.Bind(Button.IsEnabledProperty, new Binding("IsNestedFieldsStyle"));
                }

                ToolTip.SetTip(addButton, "Add a new definition within this definition/group");

                Grid.SetColumn(addButton, 1);
                buttonGrid.Children.Add(addButton);
              }

              buttonGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;

              return buttonGrid;
            }))
    }
    };
  }
}