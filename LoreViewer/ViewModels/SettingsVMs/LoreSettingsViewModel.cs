using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DocumentFormat.OpenXml.Bibliography;
using LoreViewer.Dialogs;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs;

/// <summary>
/// The View Model for the LoreSettings model object.
/// This View Model holds references to view models for all Types, globally-defined Collections, and Picklists from the LoreSettings object.
/// It also creates the Hierarchy collection object that is used to display the definition tree.
/// </summary>
public class LoreSettingsViewModel : ViewModelBase
{
  private class NewNameTracker
  {
    private int number = 1;
    private string prefix = string.Empty;
    public NewNameTracker(string namePrefix) { prefix = namePrefix; }

    public string GetName() => $"{prefix}_{number++}";
  }

  private NewNameTracker tpyeNamer = new NewNameTracker("NewType");
  private NewNameTracker fieldNamer = new NewNameTracker("NewField");
  private NewNameTracker sectionNamer = new NewNameTracker("NewSection");
  private NewNameTracker collectionNamer = new NewNameTracker("NewCollection");
  private NewNameTracker embeddedNamer = new NewNameTracker("NewEmbedded");
  private NewNameTracker picklistNamer = new NewNameTracker("NewPicklist");
  private NewNameTracker picklistEntryNamer = new NewNameTracker("NewPicklistEntry");

  protected Visual m_oView;
  public void SetView(Visual visual) => m_oView = visual;

  public string OriginalYAML { get => m_oLoreSettings.OriginalYAML; }
  public string NewYAML { get => m_oLoreSettings.CurrentYAML; }

  public LoreSettings m_oLoreSettings;

  public TreeDataGrid curTree;

  public ReactiveCommand<Unit, Unit> SaveSettingsCommand { get; }
  public ReactiveCommand<Unit, Unit> SaveSettingsWithCompareCommand { get; }

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



  private FlatTreeDataGridSource<DiffRowVM> m_oDiffRowsSource;
  public FlatTreeDataGridSource<DiffRowVM> DiffRowsSource
  {
    get => m_oDiffRowsSource;
    set => this.RaiseAndSetIfChanged(ref m_oDiffRowsSource, value);
  }





  private DefinitionTreeNodeViewModel? _selectedNode;
  public DefinitionTreeNodeViewModel? SelectedNode
  {
    get => _selectedNode;
    set
    {
      SelectedDefinition = value?.DefinitionVM;
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
  private ObservableCollection<PicklistDefinitionViewModel> m_cPicklists = new ObservableCollection<PicklistDefinitionViewModel>();

  public void GoToNodeOfDefinition(LoreDefinitionBase definition)
  {
    DefinitionTreeNodeViewModel dtvm = FindNodeOfDefinition(definition, out var pathToNode);

    if (dtvm != null && curTree != null)
    {
      (curTree.Rows as HierarchicalRows<DefinitionTreeNodeViewModel>).Expand(pathToNode);
      curTree.RowSelection.SelectedIndex = pathToNode;
    }
  }

  private DefinitionTreeNodeViewModel FindNodeOfDefinition(LoreDefinitionBase definition, out IndexPath pathToVM)
  {
    DefinitionTreeNodeViewModel dtvm;

    foreach(DefinitionTreeNodeViewModel node in TreeRootNodes)
    {
      dtvm = node.FindNodeOfDefinition(definition, out var resultingPath);
      if (dtvm != null)
      {
        pathToVM = new IndexPath(new int[] { TreeRootNodes.IndexOf(node) }.Concat(resultingPath.ToArray()));
        return dtvm;
      }
    }

    pathToVM = new IndexPath();
    return null;
  }

  public AppSettingsViewModel ParserSettings { get => new AppSettingsViewModel(m_oLoreSettings.settings); }
  public ObservableCollection<TypeDefinitionViewModel> Types { get => m_cTypes; }
  public ObservableCollection<CollectionDefinitionViewModel> Collections { get => m_cCollections; }
  public ObservableCollection<PicklistDefinitionViewModel> Picklists { get => m_cPicklists; }


  public ObservableCollection<DefinitionTreeNodeViewModel> TreeRootNodes { get; set; } = new ObservableCollection<DefinitionTreeNodeViewModel>();

  /// <summary>
  /// Creates the LoreSettingsViewModel for the input LoreSettings object.
  /// This creates definition view models for all definitions in the settings and the root grouping nodes for the DataTreeGrid.
  /// Because those two processes are recursive, the entire structure for both definition view models and tree node view models is created.
  /// 
  /// 1. for each type, collection, or picklist defined in the settings, create a viewmodel for it.
  /// 2. That calls the DefinitionViewModel base constructor, which calls the BuildList method overriden on all definitionVM classes, creating viewmodels for all of its contained definitions.
  /// 3. after each type, collection, or picklist view model has been created, create a new DefinitionTreeNodeViewModel with it, which will create nodes for all of the newly created definition view models as well.
  /// </summary>
  /// <param name="_settings">The LoreSettings object this View Model is built for.</param>
  public LoreSettingsViewModel(LoreSettings _settings)
  { 
    m_oLoreSettings = _settings;

    SaveSettingsWithCompareCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);
    SaveSettingsCommand = ReactiveCommand.Create(SaveSettings);

    var typesGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.RootTypeGroupingNode, this);
    foreach (var type in m_oLoreSettings.types)
    {
      var typeVM = new TypeDefinitionViewModel(type, this);
      var node = new DefinitionTreeNodeViewModel(ETreeNodeType.TypeDefinitionNode, this, typeVM);
      Types.Add(typeVM);
      typesGroup.AddChild(node);
    }

    var collectionsGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.RootCollectionGroupingNode, this);
    foreach (var collection in m_oLoreSettings.collections)
    {
      var colVM = new CollectionDefinitionViewModel(collection, this);
      var node = new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, this, colVM);
      Collections.Add(colVM);
      collectionsGroup.AddChild(node);
    }

    var picklistGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.RootPicklistGroupingNode, this);
    foreach (var picklist in m_oLoreSettings.picklists)
    {
      var pickVM = new PicklistDefinitionViewModel(picklist, this);
      var node = new DefinitionTreeNodeViewModel(ETreeNodeType.PicklistDefinitionNode, this, pickVM);
      Picklists.Add(pickVM);
      picklistGroup.AddChild(node);
    }

    TreeRootNodes.Add(typesGroup);
    TreeRootNodes.Add(collectionsGroup);
    TreeRootNodes.Add(picklistGroup);

    RefreshYAMLComparison();
  }

  private void SaveSettings()
  {
    m_oLoreSettings.WriteSettingsToFile();
    var win = TopLevel.GetTopLevel(m_oView) as Window;
    win.Close(m_oLoreSettings);
  }

  private async Task SaveSettingsAsync()
  {
    SettingsDiffer dfr = new SettingsDiffer();
    var output = dfr.DoSideBySideCompare(OriginalYAML, NewYAML);
    DiffRowsSource = SettingsDiffTreeDataGridBuilder.BuildTreeSource(output);
    CompareDialog cd = new CompareDialog(this);
    bool confirm = await cd.ShowDialog<bool>(TopLevel.GetTopLevel(m_oView) as Window);

    if (confirm)
    {
      m_oLoreSettings.WriteSettingsToFile();
      var win = TopLevel.GetTopLevel(m_oView) as Window;
      win.Close(m_oLoreSettings);
    }
  }

  public void RefreshYAMLComparison()
  {
    SettingsDiffer dfr = new SettingsDiffer();
    var output = dfr.DoSideBySideCompare(OriginalYAML, NewYAML);
    DiffRowsSource = SettingsDiffTreeDataGridBuilder.BuildTreeSource(output);
    this.RaisePropertyChanged(nameof(DiffRowsSource));
  }


  public void NotifyYAMLChanged()
  {
    this.RaisePropertyChanged(nameof(NewYAML));
  }

  public void RefreshTreeNodes()
  {
    // First check for nodes that have been added (at the top level) but have not yet been added to 


    foreach (DefinitionTreeNodeViewModel nodeVM in TreeRootNodes)
    {
      nodeVM.RefreshTreeNode();
    }
  }


  /// <summary>
  /// Central logic for adding a new definition to the settings model.
  /// Depending on the node type of the input tree node (the one on which the button was clicked),
  /// this method will add a new definition of the proper type to the correct definition
  /// </summary>
  /// <param name="treeVM"></param>
  internal void AddDefinition(DefinitionTreeNodeViewModel treeVM)
  {
    switch (treeVM.TreeNodeType)
    {
      case ETreeNodeType.RootTypeGroupingNode:
        m_oLoreSettings.types.Add(new LoreTypeDefinition { name = tpyeNamer.GetName() });
        return;
      case ETreeNodeType.RootCollectionGroupingNode:
        m_oLoreSettings.collections.Add(new LoreCollectionDefinition { name = collectionNamer.GetName() });
        return;
      case ETreeNodeType.RootPicklistGroupingNode:
        m_oLoreSettings.picklists.Add(new LorePicklistDefinition { name = picklistEntryNamer.GetName() });
        return;

      case ETreeNodeType.FieldGroupingNode:
        (treeVM.Parent.DefinitionVM.Definition as IFieldDefinitionContainer).AddField(new LoreFieldDefinition { name = fieldNamer.GetName() });
        return;
      case ETreeNodeType.SectionGroupingNode:
        (treeVM.Parent.DefinitionVM.Definition as ISectionDefinitionContainer).AddSection(new LoreSectionDefinition { name = sectionNamer.GetName() });
        return;
      case ETreeNodeType.CollectionGroupingNode:
        (treeVM.Parent.DefinitionVM.Definition as ICollectionDefinitionContainer).AddCollection(new LoreCollectionDefinition { name = collectionNamer.GetName() });
        return;
      case ETreeNodeType.EmbeddedNodeGroupingNode:
        (treeVM.Parent.DefinitionVM.Definition as IEmbeddedNodeDefinitionContainer).AddEmbedded(new LoreEmbeddedNodeDefinition { name = embeddedNamer.GetName() });
        return;

      case ETreeNodeType.FieldDefinitionNode:
        (treeVM.DefinitionVM.Definition as IFieldDefinitionContainer).AddField(new LoreFieldDefinition { name = fieldNamer.GetName() });
        return;
      case ETreeNodeType.PicklistEntryDefinitionNode:
      case ETreeNodeType.PicklistDefinitionNode:
        (treeVM.DefinitionVM.Definition as IPicklistEntryDefinitionContainer).AddPicklistDefinition(new LorePicklistEntryDefinition { name = picklistEntryNamer.GetName() });
        return;
      default:
        throw new Exception($"CANNOT ADD A NEW DEFINITION TO THIS DEFINITION: {treeVM.DefinitionVM?.Name}");
    }
  }

  /// <summary>
  /// Central logic for deleting an existing definition from the settings model.
  /// Depending on the node type of the input tree node, this method will remove the
  /// definition from its containing definition, or from the settings itself if its a globally defined definition
  /// </summary>
  /// <param name="definitionTreeNodeViewModel"></param>
  internal void DeleteDefinition(DefinitionTreeNodeViewModel definitionTreeNodeViewModel)
  {
    
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
  static bool isRefreshing = false;
  public static void Apply(LoreSettingsViewModel vm)
  {
    if (isRefreshing) return;
    if (vm?.m_oLoreSettings == null) return;
    isRefreshing = true;
    try
    {
      vm?.m_oLoreSettings.PostProcess();
      vm?.RefreshYAMLComparison();
      vm?.NotifyYAMLChanged();
      vm?.RefreshTreeNodes();
    }
    catch(Exception e)
    {
      Trace.TraceError(e.Message);
      Trace.TraceError(e.StackTrace);
    }
    finally
    {
      isRefreshing = false;
    }

  }
}