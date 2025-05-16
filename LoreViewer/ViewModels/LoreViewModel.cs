using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using LoreViewer.Settings;
using System.Collections.ObjectModel;
using LoreViewer.LoreNodes;

namespace LoreViewer.ViewModels
{
    internal class LoreViewModel : ViewModelBase
  {
    private LoreSettings _settings;
    private LoreParser _parser;

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }
    public ObservableCollection<LoreNode> Nodes { get => _parser._nodes; set => this.RaiseAndSetIfChanged(ref _parser._nodes, value); }
    public ObservableCollection<LoreNodeCollection> NodeCollections { get => _parser._collections; set => this.RaiseAndSetIfChanged(ref _parser._collections, value); }
    public ObservableCollection<string> Errors { get => _parser._errors; set => this.RaiseAndSetIfChanged(ref _parser._errors, value); }
    public ObservableCollection<string> Warnings { get => _parser._warnings; set => this.RaiseAndSetIfChanged(ref _parser._warnings, value); }
    public ReactiveCommand<Unit, Unit> OpenLibraryFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenLoreSettingsEditor {  get; }

    private Visual m_oView;

    public LoreViewModel(Visual view)
    {
      m_oView = view;
      OpenLibraryFolderCommand = ReactiveCommand.CreateFromTask(HandleOpenLibraryCommandAsync);
      OpenLoreSettingsEditor = ReactiveCommand.Create(OpenLoreSettingEditorDialog);

      _settings = new LoreSettings();
      _parser = new LoreParser(_settings);
    }

    private async Task HandleOpenLibraryCommandAsync()
    {
      _parser._nodes.Clear();
      _parser._collections.Clear();
      _parser._errors.Clear();
      _parser._warnings.Clear();

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

        Nodes = _parser._nodes;
        NodeCollections = _parser._collections;
        Errors = _parser._errors;
        Warnings = _parser._warnings;
      }
    }

    private void OpenLoreSettingEditorDialog()
    {

    }
  }
}
