using Avalonia.Controls;
using Avalonia.Threading;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Dialogs;

public partial class SettingsEditorDialogOld : Window
{
  public LoreSettingsViewModel viewModel { get; }

  public SettingsEditorDialogOld(LoreSettingsViewModel _settingsVM)
  {
    InitializeComponent();
    viewModel = _settingsVM;
    viewModel.SetView(this);
    // Need to defer this until AFTER, because DataGrid does not want to show it's data at the right time! Data virtualization or something...
    Dispatcher.UIThread.Post(() => DataContext = _settingsVM);
  }

  public SettingsEditorDialogOld()
  {
    InitializeComponent();
  }
}