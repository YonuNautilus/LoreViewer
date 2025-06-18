using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData.Binding;
using LoreViewer.Settings;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.ObjectModel;

namespace LoreViewer.Dialogs;

public partial class SettingsEditDialog : Window
{
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

        new CheckBoxColumn<DefinitionTreeNodeViewModel>(
            header: "Inherited",
            getter: x => x.IsInherited),

        new CheckBoxColumn<DefinitionTreeNodeViewModel>(
            header: "Required",
            getter: x => x.IsRequired ?? false,
            setter: (x, v) =>
            {
                if (x.CanEditRequired && x.DefinitionVM is FieldDefinitionViewModel f)
                    f.IsRequired = v;
                else if (x.CanEditRequired && x.DefinitionVM is SectionDefinitionViewModel s)
                    s.IsRequired = v;
            }),

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
                      Height = 20,
                      Padding = new Thickness(-5)
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
                    Height = 20
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