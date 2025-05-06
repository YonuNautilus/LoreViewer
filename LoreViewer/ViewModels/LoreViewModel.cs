using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
  internal class LoreViewModel : ViewModelBase
  {
    private LoreSettings _settings;
    private LoreParser _parser;

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }
    public ReactiveCommand<Unit, Unit> OpenLibraryFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenLoreSettingsEditor {  get; }
    public ReactiveCommand<Unit, Unit> TestParsingCommand {  get; }

    private Visual m_oView;

    public LoreViewModel(Visual view)
    {
      m_oView = view;
      OpenLibraryFolderCommand = ReactiveCommand.CreateFromTask(HandleOpenLibraryCommandAsync);
      OpenLoreSettingsEditor = ReactiveCommand.Create(OpenLoreSettingEditorDialog);
      TestParsingCommand = ReactiveCommand.CreateFromTask(TestParsing);

      _settings = new LoreSettings();
      _parser = new LoreParser(_settings);
    }

    private async Task HandleOpenLibraryCommandAsync()
    {
      var topLevel = TopLevel.GetTopLevel(m_oView);
      var folderPath = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
      {
        Title = "Select Lore Archive Base Folder",
        AllowMultiple = false
      });

      if (folderPath != null && folderPath.Count > 0)
      {
        LoreLibraryFolderPath = folderPath[0].TryGetLocalPath();
        _parser.BeginParsingFromFolder(LoreLibraryFolderPath);
      }
    }

    private void OpenLoreSettingEditorDialog()
    {

    }

    private async Task TestParsing()
    {
      var topLevel = TopLevel.GetTopLevel(m_oView);
      var filePath = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
      {
        Title = "Select Lore Markdown File To Test Parsing",
        AllowMultiple = false,
      });

      if (filePath != null && filePath.Count > 0)
      {
        _parser.ParseFile(filePath[0].TryGetLocalPath());
      }
    }
  }
}
