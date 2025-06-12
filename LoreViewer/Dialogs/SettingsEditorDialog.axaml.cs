using Avalonia.Controls;
using Avalonia.Threading;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Dialogs;

public partial class SettingsEditorDialog : Window
{
  public LoreSettingsViewModel viewModel { get; }

  public SettingsEditorDialog(LoreSettingsViewModel _settingsVM)
  {
    InitializeComponent();
    viewModel = _settingsVM;
    viewModel.SetView(this);
    // Need to defer this until AFTER, because DataGrid does not want to show it's data at the right time! Data virtualization or something...
    Dispatcher.UIThread.Post(() => DataContext = _settingsVM);
  }

  public SettingsEditorDialog()
  {
    InitializeComponent();
  }
}