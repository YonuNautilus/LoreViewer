using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
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
            new TextColumn<DefinitionTreeNodeViewModel, string>(
                header: "Name",
                getter: x => x.DisplayName,
                setter: (x, v) =>
                {
                    if (x.CanEditName && x.DefinitionVM != null)
                        x.DefinitionVM.Name = v;
                }),
            x => x.Children),

        new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Inherited",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if(node == null) return new StackPanel();

              var img = new Image
              {
                Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/link.png"))),
                Width = 24,
                Margin = new Thickness(-10),
                IsVisible = node.IsInherited
              };

              return img;
            })),

        new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Required",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if(node == null) return new Panel();

              return new CheckBox
              {
                // Note for me: in this, the "!" is an Avalonia thing:
                // !  : one-way binding (viewmodel to view)
                // !! : two-way binding
                // ~  : one-way-to-source binding (view to viewmodel)
                // ?  : set local value only not already set

                [!!ToggleButton.IsCheckedProperty] = new Binding("IsRequired"),
                [!ToggleButton.IsEnabledProperty] = new Binding("CanEditRequired"),
                IsVisible = (node.DefinitionVM?.Definition is IRequirable)

              };
            })),

        new TemplateColumn<DefinitionTreeNodeViewModel>(
            header: "Actions",
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if(node == null)
                return new StackPanel();

              var panel = new StackPanel
              {
                  Orientation = Avalonia.Layout.Orientation.Horizontal,
                  Spacing = 4,
                  Height = 20
              };

              if (!node.IsGroupNode)
              {
                var deleteButton = new Button
                {
                    Content = new Image(){ Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/trash_can.png"))), Margin = new Thickness(-10)},
                    Width = 24,
                    Padding = new Thickness(-5),
                    [!Button.IsEnabledProperty] = new Binding("CanDelete")
                };
                deleteButton.Bind(Button.CommandProperty, new Binding(nameof(node.DeleteCommand)));
                panel.Children.Add(deleteButton);
              }
              else
              {
                // Optional: add "+" add button for group nodes
                var addButton = new Button
                {
                    Content = new Image(){ Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/add.png"))), Margin = new Thickness(-10) },
                    Width = 24,
                };
                if(node.Parent != null)
                  addButton.Bind(Button.CommandProperty, new Binding(nameof(node.AddDefinitionCommand)));
                else
                  addButton.Bind(Button.CommandProperty, new Binding(nameof(node.AddDefinitionCommand)));

                panel.Children.Add(addButton);
              }

                return panel;
            }))
      }
    };
  }
}