using Avalonia.Controls;
using LoreViewer.Settings;
using LoreViewer.ViewModels.SettingsVMs;
using LoreViewer.Views;
using ReactiveUI;

namespace LoreViewer.Dialogs;

public partial class SettingsEditDialog : Window
{
  public SettingsEditDialog()
  {
    InitializeComponent();
  }

  public SettingsEditDialog(LoreSettings _settings)
  {
    InitializeComponent();
    LoreSettingsViewModel _vm = new LoreSettingsViewModel(_settings);
    this.DataContext = _vm;
    DefinitionTreeDataGrid.Source = _vm.TreeSource;
  }
}