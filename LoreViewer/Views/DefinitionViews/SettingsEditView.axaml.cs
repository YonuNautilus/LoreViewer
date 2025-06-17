using Avalonia.Controls;
using LoreViewer.Settings;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views;

public partial class SettingsEditView : UserControl
{
  LoreSettings Settings { get; set; }
  LoreSettingsViewModel SettingsViewModel { get; set; }

  public SettingsEditView()
  {
    InitializeComponent();
  }

  public SettingsEditView(LoreSettings _loreSettings) : this()
  {
    Settings = _loreSettings;
    SettingsViewModel = new LoreSettingsViewModel(Settings);

    DefinitionTreeDataGrid.Source = DefinitionTreeDataGridFactory.Build([new DefinitionExplorerViewModel(Settings)]);
  }

}


