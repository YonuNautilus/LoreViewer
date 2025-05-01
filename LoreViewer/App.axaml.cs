using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels;
using LoreViewer.Views;

namespace LoreViewer
{
  public partial class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        desktop.MainWindow = new MainWindow();
        LoreEditView readonlyView = new LoreEditView();
        LoreViewModel viewModel = new LoreViewModel(readonlyView);

        readonlyView.DataContext = viewModel;

        ((MainWindow)desktop.MainWindow).AddControl(readonlyView);
      }

      base.OnFrameworkInitializationCompleted();
    }
  }
}