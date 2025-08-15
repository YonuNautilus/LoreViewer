using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Dialogs;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.ViewModels.LoreEntities;
using LoreViewer.ViewModels.PrimaryViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
  public class LoreViewModel : ViewModelBase
  {
    private LoreRepository m_oLoreRepo = new LoreRepository();
    private ValidationStore m_oValidationRepo = new ValidationStore();

    public ValidationStore ValStore { get => m_oValidationRepo; }

    private ParserService _parser = new ParserService();
    private ValidationService _validator = new ValidationService();

    private Dictionary<ILoreNode, ILoreNodeViewModel> m_cNodeVMCache = new Dictionary<ILoreNode, ILoreNodeViewModel>();

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }
    public ObservableCollection<LoreTreeItem> Nodes { get; } = new();
    public ObservableCollection<LoreTreeItem> Collections { get; } = new();
    public ObservableCollection<ParseErrorViewModel> Errors { get; } = new();
    public ObservableCollection<string> Warnings { get; } = new();

    public ObservableCollection<ItemOutlineViewModel> ItemOutlines { get; } = new();

    private ItemOutlineViewModel m_oCurrentlySelectedOutline;
    public ItemOutlineViewModel CurrentlySelectedOutline
    {
      get => m_oCurrentlySelectedOutline;
      set
      {
        this.RaiseAndSetIfChanged(ref m_oCurrentlySelectedOutline, value);

        ActualNodeIsSelected = m_oCurrentlySelectedOutline.IsElementANode;
      }
    }

    private ILoreNodeViewModel m_oCurrentlySelectedLoreNode;
    public ILoreNodeViewModel CurrentlySelectedLoreNode
    {
      get => m_oCurrentlySelectedLoreNode;
      set
      {
        this.RaiseAndSetIfChanged(ref m_oCurrentlySelectedLoreNode, value);
      }
    }

    public EStartupMode ViewMode { get; set; }

    public bool IsEditMode { get => ViewMode == EStartupMode.Edit; }
    public bool IsReadOnlyMode { get => ViewMode == EStartupMode.Readonly; }


    private int m_iFilesParsed;
    public int ParsingProgress { get => m_iFilesParsed; set => this.RaiseAndSetIfChanged(ref m_iFilesParsed, value); }

    private int m_iFileCount;
    public int FileCount { get => m_iFileCount; set => this.RaiseAndSetIfChanged(ref m_iFileCount, value); }


    private bool m_bIsParsing;
    public bool IsParsing { get => m_bIsParsing; set => this.RaiseAndSetIfChanged(ref m_bIsParsing, value); }

    private bool m_bActualEntityIsSelected = false;
    public bool ActualNodeIsSelected
    {
      get => m_bActualEntityIsSelected;
      set => this.RaiseAndSetIfChanged(ref m_bActualEntityIsSelected, value);
    }

    private bool m_bNPppExists;

    public bool NotepadPPIsInstalled { get => m_bNPppExists; }

    private bool m_bUseNPpp;
    public bool UseNPpp
    {
      get => m_bNPppExists ? m_bUseNPpp : false;
      set
      {
        if (m_bNPppExists)
          this.RaiseAndSetIfChanged(ref m_bUseNPpp, value);
      }
    }

    public bool HadSettingsParsingError { get; set; }
    public bool NoSettingsParsingError { get => !HadSettingsParsingError; }


    private LoreTreeItem _currentlySelectedTreeNode;
    public LoreTreeItem CurrentlySelectedTreeNode
    {
      get => _currentlySelectedTreeNode;
      set
      {
        this.RaiseAndSetIfChanged(ref _currentlySelectedTreeNode, value);
        ActualNodeIsSelected = CurrentlySelectedTreeNode?.element is LoreEntity;
      }
    }

    public ReactiveCommand<Unit, Unit> OpenLibraryFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> ReloadLibraryCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenLoreSettingsEditor { get; }
    public ReactiveCommand<LoreEntity, Unit> OpenFileToLine { get; }
    public ReactiveCommand<Tuple<string, int, int, Exception>, Unit> OpenErrorFileToLine { get; }

    private Visual m_oView;

    private string m_sPathToNPppx64 = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles%"), "Notepad++", "notepad++.exe");
    private string m_sPathToNPppx86 = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "Notepad++", "notepad++.exe");
    private string m_sPathToNPpp;

    public LoreViewModel(Visual view)
    {

      if (Path.Exists(m_sPathToNPppx86))
      {
        m_bNPppExists = true;
        m_sPathToNPpp = m_sPathToNPppx86;
      }
      else if (Path.Exists(m_sPathToNPppx64))
      {

        m_bNPppExists = true;
        m_sPathToNPpp = m_sPathToNPppx64;
      }
      m_oView = view;
      OpenLibraryFolderCommand = ReactiveCommand.CreateFromTask(HandleOpenLibraryCommandAsync);
      OpenLoreSettingsEditor = ReactiveCommand.CreateFromTask(OpenLoreSettingEditorDialog);
      ReloadLibraryCommand = ReactiveCommand.Create(ReloadLoreFolder);
      OpenFileToLine = ReactiveCommand.CreateFromTask<LoreEntity>(GoToFileAtLine);
      OpenErrorFileToLine = ReactiveCommand.Create<Tuple<string, int, int, Exception>>(GoToFileAtLine);
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
        await LoadLoreFromFolder();
      }
    }

    private async Task GoToFileAtLine(LoreEntity e)
    {
      if (e is LoreElement elem)
      {
        if (UseNPpp)
          Process.Start(m_sPathToNPpp, $"{elem.SourcePath} -n{elem.LineNumber}");
        else
        {
          var dialog = new EntityEditDialog(LoreEntityViewModel.CreateViewModel(e));
          Dictionary<string, string>? result = await dialog.ShowDialog<Dictionary<string, string>?>(TopLevel.GetTopLevel(m_oView) as Window);

          if (result != null && result.Count > 0)
          {
            DoFileSaves(result);
            ReloadLoreFolder();
          }
        }
      }
    }

    private void DoFileSaves(Dictionary<string, string> saveContents)
    {
      foreach (string path in saveContents.Keys)
      {
        string markdown = saveContents[path];
        try
        {
          File.WriteAllBytes(path, Encoding.UTF8.GetBytes(markdown));
          Trace.WriteLine($"Successfully wrote markdown to file: {path}");
        }
        catch (Exception ex)
        {
          Trace.WriteLine($"Error while writing to file: {path}\r\n\t{ex.Message}");
        }
      }
      ReloadLoreFolder();
    }

    private void GoToFileAtLine(Tuple<string, int, int, Exception> dat)
    {
      string fullPathToFile;

      // If this was a settings parsing error, get the path to the settings file
      if (HadSettingsParsingError)
        fullPathToFile = Path.Combine(LoreLibraryFolderPath, LoreSettings.LoreSettingsFileName);
      else
        // If this was a Lore parsing error, get the path to the markdown file from the lore parsing exception
        fullPathToFile = Path.Combine(LoreLibraryFolderPath, dat.Item1);

      if (Path.Exists(fullPathToFile))
      {
        if (m_bNPppExists)
          Process.Start(m_sPathToNPpp, $"{fullPathToFile} -n{dat.Item3}");
        else
          Trace.WriteLine("NO IN-APP EDITOR AVAILABLE");
      }
      else
        Trace.WriteLine("EDITING FAILED FILE, NPP NOT INSTALLED");
    }
    public async void ReloadLoreFolder()
    {
      await LoadLoreFromFolder();
    }

    private async Task OpenLoreSettingEditorDialog()
    {
      if (m_oLoreRepo?.Settings != null)
      {
        var dialog = new SettingsEditDialog(m_oLoreRepo.Settings);
        LoreSettings newSettings = await dialog.ShowDialog<LoreSettings>(TopLevel.GetTopLevel(m_oView) as Window);

        if (newSettings != null)
          Trace.WriteLine("GOT NEW SETTINGS");
        else
          Trace.WriteLine("Got NULL from Settings Editor dialog, assuming changes where cancelled");
      }
    }

    private async Task LoadLoreFromFolder()
    {
      IsParsing = true;

      ParsingProgress = 0;

      Nodes.Clear();
      Collections.Clear();

      ItemOutlines.Clear();

      Warnings.Clear();
      Errors.Clear();

      ParseResult pr;

      try
      {
        var _progress = new Progress<int>(i => ParsingProgress = i);
        CancellationToken ct = new CancellationToken();
        pr = await _parser.ParseFolderAsync(LoreLibraryFolderPath, _progress, ct);


        if (pr.IsFatal)
          return;
        
        
        m_oLoreRepo.Set(pr);
        LoreValidationResult v = _validator.Validate(m_oLoreRepo);
        m_oValidationRepo.Set(v);
      }
      catch (Exception e)
      {
        Errors.Add(new ParseErrorViewModel(new ParseError(e.Message, -1, -1, e)));
        HadSettingsParsingError = true;
        IsParsing = false;
        return;
      }
      finally
      {
        this.RaisePropertyChanged(nameof(NoSettingsParsingError));
        this.RaisePropertyChanged(nameof(HadSettingsParsingError));
      }

      RefreshAllFromLoreRepo();

      FileCount = 0;
      IsParsing = false;
    }

    #region Refreshing
    private void RefreshAllFromLoreRepo()
    {
      RefreshNodesFromRepo();
      RefreshCollectionsFromRepo();
      RefreshOutlinesFromRepo();
      RefreshErrorsFromRepo();
      RefreshWarningsFromRepo();
    }

    private void RefreshOutlinesFromRepo()
    {
      foreach (LoreEntity e in _parser.Nodes) ItemOutlines.Add(new ItemOutlineViewModel(e));
      foreach (LoreCollection c in _parser.Collections) ItemOutlines.Add(new ItemOutlineViewModel(c));
    }

    private void RefreshNodesFromRepo() { foreach (LoreEntity e in _parser.Nodes) Nodes.Add(new LoreTreeItem(e)); }

    private void RefreshCollectionsFromRepo() { foreach (LoreEntity e in _parser.Collections) Collections.Add(new LoreTreeItem(e)); }

    private void RefreshErrorsFromRepo() { foreach (ParseError pe in _parser.Errors) Errors.Add(new ParseErrorViewModel(pe)); }

    private void RefreshWarningsFromRepo() { foreach (string e in _parser.Warnings) Warnings.Add(e); }
    #endregion 
  }

}
