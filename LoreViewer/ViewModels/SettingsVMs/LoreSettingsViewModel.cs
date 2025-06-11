using Avalonia;
using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreSettingsViewModel : ViewModelBase
  {

    private LoreSettings m_oLoreSettings;

    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }

    public string LoreLibraryFolderPath { get => m_oLoreSettings.FolderPath; }

    private ObservableCollection<LoreTypeDefinitionViewModel> m_cTypes = new ObservableCollection<LoreTypeDefinitionViewModel>();

    public ObservableCollection<LoreTypeDefinitionViewModel> TypeDefs { get => m_cTypes; }
    
    private void RefreshTypeDefs()
    {
      m_cTypes.Clear();
      foreach (LoreTypeDefinition def in m_oLoreSettings.types)
        m_cTypes.Add(new LoreTypeDefinitionViewModel(def));
    }

    public ObservableCollection<LoreCollectionDefinitionViewModel> ColDefs { get; } = new ObservableCollection<LoreCollectionDefinitionViewModel>();

    private void RefreshColDefs()
    {
      ColDefs.Clear();
      foreach (LoreCollectionDefinition def in m_oLoreSettings.collections)
        ColDefs.Add(new LoreCollectionDefinitionViewModel(def));
    }

    public AppSettingsViewModel ParserSettings { get => new AppSettingsViewModel(m_oLoreSettings.Settings); }


    public LoreSettingsViewModel(LoreSettings _settings)
    {
      m_oLoreSettings = _settings;
      RefreshColDefs();
      RefreshTypeDefs();
    }

    public static LoreDefinitionViewModel CreateViewModel(LoreDefinitionBase def)
    {
      switch (def)
      {
        case LoreTypeDefinition typeDef:
          return new LoreTypeDefinitionViewModel(typeDef);
        case LoreFieldDefinition fieldDef:
          return new LoreFieldDefinitionViewModel(fieldDef);
        case LoreSectionDefinition secDef:
          return new LoreSectionDefinitionViewModel(secDef);
        case LoreCollectionDefinition colDef:
          return new LoreCollectionDefinitionViewModel(colDef);
        default: return null;
      }
    }
  }

  public class AppSettingsViewModel : ViewModelBase
  {
    private AppSettings m_oAppSettings;

    public bool IgnoreCase { get => m_oAppSettings.ignoreCase; }
    public bool SoftLinking { get => m_oAppSettings.softLinking; }
    public ObservableCollection<string> MarkdownExtensions
    {
      get
      {
        ObservableCollection<string> ret = new();
        foreach(string s in m_oAppSettings.markdownExtensions)
          ret.Add(s);
        return ret;
      }
    }
    public ObservableCollection<string> BlockedPaths
    {
      get
      {
        ObservableCollection<string> ret = new();
        foreach(string s in m_oAppSettings.blockedPaths)
          ret.Add(s);
        return ret;
      }
    }

    public AppSettingsViewModel(AppSettings oAppSettings)
    {
      m_oAppSettings = oAppSettings;
    }
  }
}
