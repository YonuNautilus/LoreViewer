using Avalonia.Controls;
using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreSettingsViewModel : LoreSettingsObjectViewModel
  {

    public string OriginalYAML { get; }
    public string NewYAML { get => m_oLoreSettings.CurrentYAML; }

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
    public override ObservableCollection<TypeDefinitionViewModel> Types { get => m_cTypes; }
    public override ObservableCollection<FieldDefinitionViewModel> Fields => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections { get => m_cCollections; }
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;


    public ITreeDataGridSource<DefinitionNodeViewModel> TreeSource { get; set; }

    public LoreSettingsViewModel(LoreSettings _settings)
    {
      DeleteDefinitionCommand = ReactiveCommand.Create<LoreDefinitionViewModel>(DeleteDefinition);
      EditDefinitionCommand = ReactiveCommand.CreateFromTask<LoreDefinitionViewModel>(EditDefinition);
      AddTypeCommand = ReactiveCommand.Create(AddType);
      AddCollectionCommand = ReactiveCommand.Create(AddCollection);
      m_oLoreSettings = _settings;
      RefreshColDefs();
      RefreshTypeDefs();

      CurrentSettings = _settings;

      OriginalYAML = _settings.OriginalYAML;

      TreeSource = DefinitionTreeDataGridFactory.Build([new DefinitionExplorerViewModel(_settings)]);
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

    public void RefreshYaml()
    {
      this.RaisePropertyChanged("NewYAML");
    }

    public override void RefreshLists()
    {
      RefreshTypeDefs();
      RefreshColDefs();
    }

    public new void AddType()
    {
      CurrentSettings.types.Add(new LoreTypeDefinition() { name = "New Type" });
      RefreshTypeDefs();
      RefreshYaml();
    }

    public new void AddCollection()
    {
      CurrentSettings.collections.Add(new LoreCollectionDefinition() { name = "New Collection" });
      RefreshColDefs();
      RefreshYaml();
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
