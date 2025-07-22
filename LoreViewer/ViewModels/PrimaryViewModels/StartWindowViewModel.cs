using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using LoreViewer.Util;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.PrimaryViewModels
{
  public enum EStartupType
  {
    OpenExisting, StartFresh, OpenTemplate, OpenSample
  }
  public enum EStartupMode
  {
    Readonly, Edit
  }

  public struct StartupAction
  {
    public readonly EStartupType StartupType;
    public readonly EStartupMode StartupMode;
    public readonly string Path;

    public StartupAction(EStartupType startupType, EStartupMode startupMode, string path)
    {
      StartupType = startupType; StartupMode = startupMode; Path = path; }
  }

  public class StartWindowViewModel : ViewModelBase
  {
    private StartWindow m_oView;

    public StartupAction Result;
    public void SetView(StartWindow view) => m_oView = view;

    public List<string> RecentProjectPaths { get => RegistryHelper.GetRecentProjects(); }

    private ReactiveCommand<Unit, Unit> m_oOpenLoreLibraryCommand;
    private ReactiveCommand<string, Unit> m_oOpenReadonlyFromRecentCommand;
    private ReactiveCommand<string, Unit> m_oOpenEditFromRecentCommand;
    private ReactiveCommand<string, Unit> m_oOpenNewFromSampleCommand;
    private ReactiveCommand<string, Unit> m_oOpenNewFromTemplateCommand;
    private ReactiveCommand<Unit, Unit> m_oExitCommand;

    public ReactiveCommand<Unit, Unit> OpenLoreLibaryCommand { get => m_oOpenLoreLibraryCommand; }
    public ReactiveCommand<string, Unit> OpenReadonlyFromRecentCommand { get => m_oOpenReadonlyFromRecentCommand; }
    public ReactiveCommand<string, Unit> OpenEditFromRecentCommand { get => m_oOpenEditFromRecentCommand; }
    public ReactiveCommand<string, Unit> OpenSampleCommand { get => m_oOpenNewFromSampleCommand; }
    public ReactiveCommand<string, Unit> OpenTemplateCommand { get => m_oOpenNewFromTemplateCommand; }
    public ReactiveCommand<Unit, Unit> ExitCommand { get => m_oExitCommand; }

    private readonly Action<StartupAction> m_oStartupCallback;

    public StartWindowViewModel(Action<StartupAction> callback)
    {
      m_oStartupCallback = callback;

      m_oOpenLoreLibraryCommand = ReactiveCommand.CreateFromTask(HandleOpenLibraryCommandAsync);
      m_oOpenReadonlyFromRecentCommand = ReactiveCommand.Create<string>(OpenRecentReadonly);
      m_oOpenEditFromRecentCommand = ReactiveCommand.Create<string>(OpenRecentEdit);
      m_oOpenNewFromSampleCommand = ReactiveCommand.Create<string>(OpenSample);
      m_oOpenNewFromTemplateCommand = ReactiveCommand.Create<string>(OpenTemplate);


    }

    private async Task HandleOpenLibraryCommandAsync()
    {
      var topLevel = TopLevel.GetTopLevel(m_oView);
      var folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
      {
        Title = "Select Lore Library Base Folder",
        AllowMultiple = false
      });

      if (folderPath != null && folderPath.Count > 0)
      {
        string pathToOpen = folderPath[0].TryGetLocalPath();
        Dispatcher.UIThread.Invoke(() => RegistryHelper.SaveRecentProject(pathToOpen));
        m_oStartupCallback.Invoke(new StartupAction(EStartupType.OpenExisting, EStartupMode.Edit, pathToOpen));
      }
    }

    private void OpenRecentReadonly(string path) => m_oStartupCallback.Invoke(new StartupAction(EStartupType.OpenExisting, EStartupMode.Readonly, path));
    private void OpenRecentEdit(string path) => m_oStartupCallback.Invoke(new StartupAction(EStartupType.OpenExisting, EStartupMode.Edit, path));

    private void OpenSample(string sample) => m_oStartupCallback.Invoke(new StartupAction(EStartupType.OpenSample, EStartupMode.Readonly, sample));
    private void OpenTemplate(string template) => m_oStartupCallback.Invoke(new StartupAction(EStartupType.OpenTemplate, EStartupMode.Edit, template));
  }
}
