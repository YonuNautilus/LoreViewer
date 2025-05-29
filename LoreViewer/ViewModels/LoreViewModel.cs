using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using LoreViewer.Settings;
using System.Collections.ObjectModel;
using LoreViewer.LoreElements;
using System.Collections.Generic;
using System.Linq;
using System;

namespace LoreViewer.ViewModels
{
    internal class LoreViewModel : ViewModelBase
  {
    private LoreSettings _settings;
    private LoreParser _parser;

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }
    private List<LoreEntity> _nodes => _parser._nodes.Cast<LoreEntity>().ToList();
    private List<LoreNodeCollection> _collections => _parser._collections.ToList();

    private ObservableCollection<LoreTreeItem> _nodeTreeItems = new ObservableCollection<LoreTreeItem>();
    public ObservableCollection<LoreTreeItem> Nodes { get => _nodeTreeItems; set => this.RaiseAndSetIfChanged(ref _nodeTreeItems, value); }

    private ObservableCollection<LoreTreeItem> _nodeCollectionTreeItems = new ObservableCollection<LoreTreeItem>();
    public ObservableCollection<LoreTreeItem> NodeCollections { get => _nodeCollectionTreeItems; set => this.RaiseAndSetIfChanged(ref _nodeCollectionTreeItems, value); }
    public ObservableCollection<Tuple<string, int, int, Exception>> Errors { get => _parser._errors; set => this.RaiseAndSetIfChanged(ref _parser._errors, value); }
    public ObservableCollection<string> Warnings { get => _parser._warnings; set => this.RaiseAndSetIfChanged(ref _parser._warnings, value); }

    private LoreTreeItem _currentlySelectedTreeNode;
    public LoreTreeItem CurrentlySelectedTreeNode { get => _currentlySelectedTreeNode; set => this.RaiseAndSetIfChanged(ref _currentlySelectedTreeNode, value); }
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

        if (_parser.HadFatalError)
        {

        }

        _nodeTreeItems.Clear();
        _nodeCollectionTreeItems.Clear();

        foreach(LoreEntity e in _nodes) Nodes.Add(new LoreTreeItem(e));

        foreach (LoreEntity e in _collections) NodeCollections.Add(new LoreTreeItem(e));

        this.RaisePropertyChanged("Errors");
        this.RaisePropertyChanged("Warnings");
      }
    }

    private void OpenLoreSettingEditorDialog()
    {

    }
  }
}
