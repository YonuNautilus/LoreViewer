using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml.InkML;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreSettingsViewModel : ViewModelBase
  {
    private Visual m_oView;

    public string OriginalYAML { get; }
    public string NewYAML { get => m_oLoreSettings.CurrentYAML; }
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; }

    private LoreSettings m_oLoreSettings;

    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }

    public string LoreLibraryFolderPath { get => m_oLoreSettings.FolderPath; }

    private ObservableCollection<TypeDefinitionViewModel> m_cTypes = new ObservableCollection<TypeDefinitionViewModel>();
    private ObservableCollection<CollectionDefinitionViewModel> m_cCollections = new ObservableCollection<CollectionDefinitionViewModel>();

    public ObservableCollection<TypeDefinitionViewModel> TypeDefs { get => m_cTypes; }
    public ObservableCollection<CollectionDefinitionViewModel> ColDefs { get => m_cCollections; }
    
    private void RefreshTypeDefs()
    {
      m_cTypes.Clear();
      foreach (LoreTypeDefinition def in m_oLoreSettings.types)
        m_cTypes.Add(new TypeDefinitionViewModel(def));
    }


    private void RefreshColDefs()
    {
      ColDefs.Clear();
      foreach (LoreCollectionDefinition def in m_oLoreSettings.collections)
        ColDefs.Add(new CollectionDefinitionViewModel(def));
    }

    public AppSettingsViewModel ParserSettings { get => new AppSettingsViewModel(m_oLoreSettings.settings); }


    public LoreSettingsViewModel(LoreSettings _settings)
    {
      DeleteDefinitionCommand = ReactiveCommand.Create<LoreDefinitionViewModel>(DeleteDefinition);
      EditDefinitionCommand = ReactiveCommand.CreateFromTask<LoreDefinitionViewModel>(EditDefinition);
      m_oLoreSettings = _settings;
      RefreshColDefs();
      RefreshTypeDefs();

      OriginalYAML = _settings.OriginalYAML;
    }

    public static LoreDefinitionViewModel CreateViewModel(LoreDefinitionBase def)
    {
      switch (def)
      {
        case LoreTypeDefinition typeDef:
          return new TypeDefinitionViewModel(typeDef);
        case LoreFieldDefinition fieldDef:
          return new FieldDefinitionViewModel(fieldDef);
        case LoreSectionDefinition secDef:
          return new SectionDefinitionViewModel(secDef);
        case LoreCollectionDefinition colDef:
          return new CollectionDefinitionViewModel(colDef);
        default: return null;
      }
    }

    public async Task EditDefinition(LoreDefinitionViewModel vm)
    {
      if (vm != null)
      {
        var dialog = DefinitionEditorDialog.CreateDefinitionEditorDialog(vm);
        await dialog.ShowDialog(TopLevel.GetTopLevel(m_oView) as Window);
      }
    }

    public void DeleteDefinition(LoreDefinitionViewModel vm)
    {
      switch (vm)
      {
        case TypeDefinitionViewModel typeDefVM:
          m_oLoreSettings.types.Remove(typeDefVM.Definition as LoreTypeDefinition);
          TypeDefs.Remove(typeDefVM);
          break;
        case CollectionDefinitionViewModel collectionDefVM:
          m_oLoreSettings.collections.Remove(collectionDefVM.Definition as LoreCollectionDefinition);
          ColDefs.Remove(collectionDefVM);
          break;
        default: return;
      }
    }
  }

  public class AppSettingsViewModel : ViewModelBase
  {
    private AppSettings m_oAppSettings;

    public bool IgnoreCase { get => m_oAppSettings.ignoreCase; set => m_oAppSettings.ignoreCase = value; }
    public bool SoftLinking { get => m_oAppSettings.softLinking; set => m_oAppSettings.softLinking = value; }
    public bool EnablePruningForSerialization { get => m_oAppSettings.EnableSerializationPruning; set => m_oAppSettings.EnableSerializationPruning = value; }
    public string MarkdownExtensions
    {
      get => string.Join("\r\n", m_oAppSettings.markdownExtensions);
      set => m_oAppSettings.markdownExtensions = value.Split("\r\n").ToList();
    }
    public string BlockedPaths
    {
      get => string.Join("\r\n", m_oAppSettings.blockedPaths);
      set => m_oAppSettings.blockedPaths = value.Split("\r\n").ToList();
    }

    public AppSettingsViewModel(AppSettings oAppSettings)
    {
      m_oAppSettings = oAppSettings;
    }
  }
}
