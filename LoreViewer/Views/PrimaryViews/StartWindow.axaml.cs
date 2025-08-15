using Avalonia.Controls;
using LoreViewer.ViewModels.PrimaryViewModels;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LoreViewer;

public partial class StartWindow : Window
{
  private TaskCompletionSource<StartupAction?> m_oStartupTaskCompletionSource = new();
  public Task<StartupAction?> StartupAction => m_oStartupTaskCompletionSource.Task;
  public StartWindow()
  {
    InitializeComponent();
    var vm = new StartWindowViewModel(OnOptionSelected);
    vm.SetView(this);
    this.DataContext = vm;
  }

  private void OnOptionSelected(StartupAction action)
  {
    Trace.WriteLine(action.Path);
    m_oStartupTaskCompletionSource.TrySetResult(action);
    Close();
  }

  public void DoClose()
  {
    this.Close();
  }
}