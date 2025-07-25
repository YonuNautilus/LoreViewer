﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using LoreViewer.Dialogs;
using LoreViewer.LoreElements;
using LoreViewer.Parser;
using LoreViewer.Settings;
using LoreViewer.Validation;
using LoreViewer.ViewModels.LoreEntities;
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
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
  internal class LoreViewModel : ViewModelBase
  {
    // Models for this ViewModel:
    public static LoreSettings _settings;
    public static LoreParser _parser;

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }
    public ObservableCollection<LoreTreeItem> Nodes { get; } = new();

    public ObservableCollection<LoreTreeItem> Collections { get; } = new();
    public ObservableCollection<Tuple<string, int, int, Exception>> Errors { get; } = new();
    public ObservableCollection<string> Warnings { get; } = new();


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
      if (_settings != null)
      {
        var dialog = new SettingsEditDialog(_settings);
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
      Warnings.Clear();
      Errors.Clear();

      try
      {
        _settings = LoreSettings.ParseSettingsFromFolder(LoreLibraryFolderPath);
        HadSettingsParsingError = false;
      }
      catch (Exception e)
      {
        Errors.Add(new Tuple<string, int, int, Exception>(e.Message, -1, -1, e));
        HadSettingsParsingError = true;
        IsParsing = false;
        return;
      }
      finally
      {
        this.RaisePropertyChanged(nameof(NoSettingsParsingError));
        this.RaisePropertyChanged(nameof(HadSettingsParsingError));
      }

      _parser = new LoreParser(_settings);

      FileCount = _parser.ParsableFiles(LoreLibraryFolderPath, _settings).Count();

      var _progress = new Progress<int>(i => ParsingProgress = i);

      await _parser.ParseFolderAsync(LoreLibraryFolderPath, _progress);

      RefreshAllFromParser();

      FileCount = 0;
      IsParsing = false;
    }

    #region Refreshing
    private void RefreshAllFromParser()
    {
      RefreshNodesFromParser();
      RefreshCollectionsFromParser();
      RefreshErrorsFromParser();
      RefreshWarningsFromParser();
    }

    private void RefreshNodesFromParser() { foreach (LoreEntity e in _parser.Nodes) Nodes.Add(new LoreTreeItem(e)); }

    private void RefreshCollectionsFromParser() { foreach (LoreEntity e in _parser.Collections) Collections.Add(new LoreTreeItem(e)); }

    private void RefreshErrorsFromParser() { foreach (Tuple<string, int, int, Exception> e in _parser.Errors) Errors.Add(e); }

    private void RefreshWarningsFromParser() { foreach (string e in _parser.Warnings) Warnings.Add(e); }
    #endregion 
  }

  public class ValidationErrorToImagePathConverter : IValueConverter
  {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      // Value will be the datagrid node (LoreTreeItem)

      // if there is a LoreEntity associated with the selected DataGrid node...
      if (value is LoreTreeItem lti)
      {
        if (lti.element is LoreEntity e)
        {
          LoreValidationResult valRes = LoreViewModel._parser.validator.ValidationResult;

          string image = string.Empty;
          EValidationState elementState = valRes.LoreEntityValidationStates.ContainsKey(e) ?
            valRes.LoreEntityValidationStates[e] : EValidationState.Passed;

          EValidationMessageStatus cumulativeMessageStatus = EValidationMessageStatus.Passed;
          if (valRes.LoreEntityValidationMessages.ContainsKey(e) && valRes.LoreEntityValidationMessages[e].Count > 0)
            cumulativeMessageStatus = valRes.LoreEntityValidationMessages[e].Select(m => m.Status).Distinct().Max();

          if (elementState == EValidationState.Failed || elementState == EValidationState.ChildFailed)
            image = "avares://LoreViewer/Resources/close.png";
          else if (elementState == EValidationState.Warning)
            image = "avares://LoreViewer/Resources/warning.png";
          else if (elementState == EValidationState.ChildWarning)
          {
            if (cumulativeMessageStatus == EValidationMessageStatus.Failed)
              image = "avares://LoreViewer/Resources/failedChildWarning.png";
            else if (cumulativeMessageStatus == EValidationMessageStatus.Passed)
              image = "avares://LoreViewer/Resources/childWarning.png";
          }
          else if (elementState == EValidationState.Passed)
            image = "avares://LoreViewer/Resources/valid.png";
          else
            return null;
          return new Bitmap(AssetLoader.Open(new Uri(image)));
        }
        // If there isn't a LoreEntity associated with the selected DataGrid node
        // (ie, a node that contains all the attributes or sections of a node),
        // get a cummulative validation state to display
        else
        {
          if(lti.Children != null)
          {
            string image = string.Empty;

            EValidationState containerItemState = EValidationState.Passed;
            EValidationState maxChildItemState = lti.Children.Select(
              childLti =>
              childLti.element != null ?
                  LoreViewModel._parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(childLti.element) ?
                  LoreViewModel._parser.validator.ValidationResult.LoreEntityValidationStates[childLti.element] : EValidationState.Passed : EValidationState.Passed)
              .Distinct().Max();

            switch (maxChildItemState)
            {
              case EValidationState.ChildFailed:
              case EValidationState.Failed:
                image = "avares://LoreViewer/Resources/close.png";
                break;
              case EValidationState.ChildWarning:
              case EValidationState.Warning:
                image = "avares://LoreViewer/Resources/childWarning.png";
                break;
              default:
                image = "avares://LoreViewer/Resources/valid.png";
                break;
            }

            if (!string.IsNullOrEmpty(image))
              return new Bitmap(AssetLoader.Open(new Uri(image)));
            else return null;
          }
        }
      }
      return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
  public class LoreEntityToValidationMessageListConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is LoreEntity e)
        return LoreViewModel._parser.validator.ValidationResult.LoreEntityValidationMessages.ContainsKey(e) ? new ObservableCollection<LoreValidationMessage>(LoreViewModel._parser.validator.ValidationResult.LoreEntityValidationMessages[e]) : null;
      return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

}
