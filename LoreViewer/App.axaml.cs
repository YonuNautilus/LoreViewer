using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Wordprocessing;
using LoreViewer.ViewModels;
using LoreViewer.ViewModels.PrimaryViewModels;
using LoreViewer.Views;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LoreViewer
{
  public partial class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
      if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
      {
        string[] args = desktop.Args;

        string lorePath = string.Empty;
        EStartupType startupType = EStartupType.StartFresh;
        EStartupMode startupMode = EStartupMode.Readonly;

        // If we have an argument associated with a project path, load that, skip the startup window
        if (args.Count() == 0)
        {
          // Set up the start window and its VM
          var startWindow = new StartWindow();

          startWindow.Show();

          var result = await startWindow.StartupAction;
          lorePath = result.Value.Path;
          startupType = result.Value.StartupType;
          startupMode = result.Value.StartupMode;

          Trace.WriteLine(result?.Path ?? "NONE");
        }
        else if (Directory.Exists(args[0]))
        {
          lorePath = args[0];
        }


        if(Directory.Exists(lorePath))
        {
          desktop.MainWindow = new MainWindow();

          LoreViewModel curVM;
          UserControl curView;


          switch (startupMode)
          {
            case EStartupMode.Edit:
              curView = new LoreEditLegacyView();
              curVM = new LoreViewModel(curView);
              curView.DataContext = curVM;
              break;
            case EStartupMode.Readonly:
            default:
              curView = new LoreReadonlyView();
              curVM = new LoreViewModel(curView);
              curView.DataContext = curVM;
              break;
          }

          curVM.ViewMode = startupMode;
          desktop.MainWindow.DataContext = curVM;

          curVM.LoreLibraryFolderPath = lorePath;
          curVM.ReloadLoreFolder();

          desktop.MainWindow.Show();
        }
      }
      base.OnFrameworkInitializationCompleted();
    }
  }
}