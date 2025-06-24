using Avalonia.Controls;
using LoreViewer.Settings;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LoreViewer.ViewModels.SettingsVMs;

public class LoreSettingsViewModel : LoreSettingsObjectViewModel
{

  public string OriginalYAML { get => m_oLoreSettings.OriginalYAML; }
  public string NewYAML { get => m_oLoreSettings.CurrentYAML; }

  public LoreSettings m_oLoreSettings;

  public TreeDataGrid curTree;

  public bool IgnoreCase
  { 
    get => m_oLoreSettings.settings.ignoreCase;
    set
    {
      m_oLoreSettings.settings.ignoreCase = value;
      SettingsRefresher.Apply(this);
    }
  }
  public bool SoftLinking
  { 
    get => m_oLoreSettings.settings.softLinking;
    set
    {
      m_oLoreSettings.settings.softLinking = value;
      SettingsRefresher.Apply(this);
    }
  }
  public bool EnablePruningForSerialization
  { 
    get => m_oLoreSettings.settings.EnableSerializationPruning;
    set
    {
      m_oLoreSettings.settings.EnableSerializationPruning = value;
      SettingsRefresher.Apply(this);
    }
  }
  public string MarkdownExtensions
  {
    get => string.Join("\r\n", m_oLoreSettings.settings.markdownExtensions);
    set
    {
      m_oLoreSettings.settings.markdownExtensions = value.Split("\r\n").ToList();
      SettingsRefresher.Apply(this);
    }
  }
  public string BlockedPaths
  {
    get => string.Join("\r\n", m_oLoreSettings.settings.blockedPaths);
    set
    {
      m_oLoreSettings.settings.blockedPaths = value.Split("\r\n").ToList();
      SettingsRefresher.Apply(this);
    }
  }



  private DefinitionTreeNodeViewModel? _selectedNode;
  public DefinitionTreeNodeViewModel? SelectedNode
  {
    get => _selectedNode;
    set
    {
      SelectedDefinition = value?.DefinitionVM;
      this.RaiseAndSetIfChanged(ref _selectedNode, value);
    }
  }

  public LoreDefinitionViewModel? _selectedDef;

  public LoreDefinitionViewModel? SelectedDefinition
  {
    get => _selectedDef;
    set
    {
      _selectedDef = value;
      this.RaisePropertyChanged(nameof(SelectedDefinition));
    }
  }


  public string LoreLibraryFolderPath { get => m_oLoreSettings.FolderPath; }

  private ObservableCollection<TypeDefinitionViewModel> m_cTypes = new ObservableCollection<TypeDefinitionViewModel>();
  private ObservableCollection<CollectionDefinitionViewModel> m_cCollections = new ObservableCollection<CollectionDefinitionViewModel>();

  private void ConstructTypeDefinitionViewModels()
  {
    foreach (LoreTypeDefinition def in m_oLoreSettings.types)
      Types.Add(new TypeDefinitionViewModel(def));
  }

  public void GoToNodeOfDefinition(LoreDefinitionBase definition)
  {
    DefinitionTreeNodeViewModel dtvm = FindNodeOfDefinition(definition);

    //if(dtvm != null && curTree != null)
      //TreeRootNodes[0] = dtvm;
  }

  private DefinitionTreeNodeViewModel FindNodeOfDefinition(LoreDefinitionBase definition)
  {
    DefinitionTreeNodeViewModel dtvm;

    foreach(DefinitionTreeNodeViewModel node in TreeRootNodes)
    {
      dtvm = node.FindNodeOfDefinition(definition);
      if (dtvm != null) return dtvm;
    }

    return null;
  }


  private void ConstructCollectionDefinitionViewModels()
  {
    foreach (LoreCollectionDefinition def in m_oLoreSettings.collections)
      Collections.Add(new CollectionDefinitionViewModel(def));
  }

  public AppSettingsViewModel ParserSettings { get => new AppSettingsViewModel(m_oLoreSettings.settings); }
  public override ObservableCollection<TypeDefinitionViewModel> Types { get => m_cTypes; }
  public override ObservableCollection<FieldDefinitionViewModel> Fields => null;
  public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
  public override ObservableCollection<CollectionDefinitionViewModel> Collections { get => m_cCollections; }
  public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;


  public ObservableCollection<DefinitionTreeNodeViewModel> TreeRootNodes { get; set; } = new ObservableCollection<DefinitionTreeNodeViewModel>();

  public LoreSettingsViewModel(LoreSettings _settings)
  { 
    m_oLoreSettings = _settings;
    ConstructCollectionDefinitionViewModels();
    ConstructTypeDefinitionViewModels();

    CurrentSettings = _settings;


    var typesGroup = new DefinitionTreeNodeViewModel("Types", this, typeof(LoreTypeDefinition));
    foreach (var type in m_oLoreSettings.types)
    {
      var typeVM = new TypeDefinitionViewModel(type);
      var node = new DefinitionTreeNodeViewModel(typeVM);
      //node.BuildChildren();
      typesGroup.AddChild(node);
    }

    var collectionsGroup = new DefinitionTreeNodeViewModel("Collections", this, typeof(LoreCollectionDefinition));
    foreach (var collection in m_oLoreSettings.collections)
    {
      var colVM = new CollectionDefinitionViewModel(collection);
      var node = new DefinitionTreeNodeViewModel(colVM);
      collectionsGroup.AddChild(node);
    }

    TreeRootNodes.Add(typesGroup);
    TreeRootNodes.Add(collectionsGroup);
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


  public void NotifyYAMLChanged()
  {
    this.RaisePropertyChanged(nameof(NewYAML));
  }

  public void RefreshTreeNodes()
  {
    foreach (DefinitionTreeNodeViewModel nodeVM in TreeRootNodes)
    {
      nodeVM.RefreshTreeNode();
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

public static class SettingsRefresher
{
  public static void Apply(LoreSettingsViewModel vm)
  {
    if (vm?.m_oLoreSettings == null) return;

    vm?.m_oLoreSettings.PostProcess();

    vm.NotifyYAMLChanged();

    vm.RefreshTreeNodes();
  }
}