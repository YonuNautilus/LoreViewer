using Avalonia;
using Avalonia.Controls;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Views.DefinitionViews;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreSettingsViewModel : ViewModelBase
  {
    private Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; }

    private LoreSettings m_oLoreSettings;

    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }

    public string LoreLibraryFolderPath { get => m_oLoreSettings.FolderPath; }

    private ObservableCollection<TypeDefinitionViewModel> m_cTypes = new ObservableCollection<TypeDefinitionViewModel>();

    public ObservableCollection<TypeDefinitionViewModel> TypeDefs { get => m_cTypes; }
    
    private void RefreshTypeDefs()
    {
      m_cTypes.Clear();
      foreach (LoreTypeDefinition def in m_oLoreSettings.types)
        m_cTypes.Add(new TypeDefinitionViewModel(def));
    }

    public ObservableCollection<CollectionDefinitionViewModel> ColDefs { get; } = new ObservableCollection<CollectionDefinitionViewModel>();

    private void RefreshColDefs()
    {
      ColDefs.Clear();
      foreach (LoreCollectionDefinition def in m_oLoreSettings.collections)
        ColDefs.Add(new CollectionDefinitionViewModel(def));
    }

    public AppSettingsViewModel ParserSettings { get => new AppSettingsViewModel(m_oLoreSettings.Settings); }


    public LoreSettingsViewModel(LoreSettings _settings)
    {
      DeleteDefinitionCommand = ReactiveCommand.Create<LoreDefinitionViewModel>(DeleteDefinition);
      EditDefinitionCommand = ReactiveCommand.CreateFromTask<LoreDefinitionViewModel>(EditDefinition);
      m_oLoreSettings = _settings;
      RefreshColDefs();
      RefreshTypeDefs();
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
      if(vm != null)
      {
          var dialog = DefinitionEditorDialog.CreateDefinitionEditorDialog(vm);
          await dialog.ShowDialog(TopLevel.GetTopLevel(m_oView) as Window);
      }
    }

    public void DeleteDefinition(LoreDefinitionViewModel vm)
    {
      Trace.WriteLine($"DELETING DEFINITION {vm.Definition.name} OF TYPE {vm.Definition.GetType().Name}");
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
