using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Charts;
using LoreViewer.ViewModels;
using LoreViewer.Views;
using System.IO;

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
        LoreEditView startingView = new LoreEditView();
        LoreViewModel viewModel = new LoreViewModel(startingView);

        startingView.DataContext = viewModel;

        ((MainWindow)desktop.MainWindow).AddControl(startingView);
        
        string[] args = desktop.Args;
        if (Directory.Exists(args[0]))
        {
          viewModel.LoreLibraryFolderPath = args[0];
          viewModel.ReloadLoreFolder();
        }
      }

      base.OnFrameworkInitializationCompleted();
    }
  }
}