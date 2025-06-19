using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DocumentFormat.OpenXml.Vml.Office;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.ObjectModel;
using Tmds.DBus.Protocol;

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
            cellTemplate: new FuncDataTemplate<DefinitionTreeNodeViewModel>((node, _) =>
            {
              if (node == null) return new Panel();
              if (node.IsGroupNode)
                return new Label { [!Label.ContentProperty] = new Binding("DisplayName") };
              else
                return new TextBox
                {
                  [!!TextBox.TextProperty] = new Binding("DisplayName"),
                  [!TextBox.IsReadOnlyProperty] = new Binding("NameIsReadOnly")
                };
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
                IsVisible = node.IsInherited,
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Margin = new Thickness(5, 0,0,0)
              };

              ToolTip.SetTip(ret, toolTipText);

              var img = new Image
              {
                Source = new Bitmap(AssetLoader.Open(new System.Uri(imgURI))),
                Width = 24,
              };


              ret.Children.Add(img);

              if(node.DefinitionVM?.Definition is LoreTypeDefinition && node.DefinitionVM.Definition.IsInherited)
              {
                var label = new Label { Content = $"({node.DefinitionVM.Definition.Base.name})", };
                ret.Children.Add(label);
              }

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
                  Content = new Image() { Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/trash_can.png"))), Margin = new Thickness(-10) },
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
                  Content = new Image() { Source = new Bitmap(AssetLoader.Open(new System.Uri("avares://LoreViewer/Resources/add.png"))), Margin = new Thickness(-10) },
                  Width = 24,
                };
                if (node.Parent != null)
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