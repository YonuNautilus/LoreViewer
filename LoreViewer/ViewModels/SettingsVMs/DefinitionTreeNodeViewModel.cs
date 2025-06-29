using Avalonia;
using Avalonia.Controls;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Wordprocessing;
using DynamicData;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs;

public enum ETreeNodeType
{
  RootTypeGroupingNode,
  RootCollectionGroupingNode,
  RootPicklistGroupingNode,

  FieldGroupingNode,
  SectionGroupingNode,
  CollectionGroupingNode,
  EmbeddedNodeGroupingNode,

  TypeDefinitionNode,
  FieldDefinitionNode,
  SectionDefinitionNode,
  CollectionDefinitionNode,
  EmbeddedNodeDefinitionNode,
  PicklistDefinitionNode,
  PicklistEntryDefinitionNode,
}


/// <summary>
/// A viewmodel for any node representing a definition or grouping of definitions in the lore settings edit dialog's TreeDataGrid.
/// This class exposes properties like name, inheritance status, and commands for the add and delete buttons that appear on the node.
/// </summary>
public class DefinitionTreeNodeViewModel : ViewModelBase
{

  private ETreeNodeType[] GroupingNodeTypes = {
    ETreeNodeType.RootTypeGroupingNode,
    ETreeNodeType.RootCollectionGroupingNode,
    ETreeNodeType.RootPicklistGroupingNode,

    ETreeNodeType.FieldGroupingNode,
    ETreeNodeType.SectionGroupingNode,
    ETreeNodeType.CollectionGroupingNode,
    ETreeNodeType.EmbeddedNodeGroupingNode };

  private ETreeNodeType[] RootGroupingNodeTypes = {
    ETreeNodeType.RootTypeGroupingNode,
    ETreeNodeType.RootCollectionGroupingNode,
    ETreeNodeType.RootPicklistGroupingNode };

  private ETreeNodeType[] DefinitionLevelGroupingNodeType = {
    ETreeNodeType.FieldGroupingNode,
    ETreeNodeType.SectionGroupingNode,
    ETreeNodeType.CollectionGroupingNode,
    ETreeNodeType.EmbeddedNodeGroupingNode };


  private string TypeGroupName => Children.Any() ? $"Types ({Children.Count})" : "Types";
  private string FieldGroupName => Children.Any() ? $"Fields ({Children.Count})" : "Fields";
  private string SectionGroupName => Children.Any() ? $"Sections ({Children.Count})" : "Sections";
  private string CollectionGroupName => Children.Any() ? $"Collections ({Children.Count})" : "Collections";
  private string EmbeddedNodesGroupName => Children.Any() ? $"Embedded Nodes ({Children.Count})" : "Embedded Nodes";
  private string PicklistGroupName => Children.Any() ? $"Picklists ({Children.Count})" : "Picklists";


  private LoreSettingsViewModel _settings;
  private Type _addType;

  public ETreeNodeType TreeNodeType { get; }

  public ReactiveCommand<LoreDefinitionBase, Unit> GoToNodeOfDefinitionCommand { get; }

  public LoreSettingsViewModel CurrentSettingsVM => _settings;

  public DefinitionTreeNodeViewModel? Parent { get; private set; }
  public ObservableCollection<DefinitionTreeNodeViewModel> Children { get; } = new();

  public LoreDefinitionViewModel? DefinitionVM { get; }
  public string GroupName { get; } = string.Empty;
  public bool IsGroupNode { get { return GroupingNodeTypes.Contains(TreeNodeType); } }

  public string InheritanceLabelString => DefinitionVM?.InheritanceLabelString;

  public ReactiveCommand<Unit, Unit> DeleteDefinitionCommand { get; }
  public ReactiveCommand<Unit, Unit> AddDefinitionCommand { get; }

  public string DisplayName
  {
    get
    {
      switch (TreeNodeType)
      {
        case ETreeNodeType.RootTypeGroupingNode:
          return TypeGroupName;
        case ETreeNodeType.RootCollectionGroupingNode:
        case ETreeNodeType.CollectionDefinitionNode:
          return CollectionGroupName;
        case ETreeNodeType.RootPicklistGroupingNode:
          return PicklistGroupName;
        case ETreeNodeType.FieldGroupingNode:
          return FieldGroupName;
        case ETreeNodeType.SectionGroupingNode:
          return SectionGroupName;
        case ETreeNodeType.EmbeddedNodeGroupingNode:
          return EmbeddedNodesGroupName;
        default:
          return DefinitionVM?.Name ?? "(Unnamed)";
      }
    }
    set
    {
      if(DefinitionVM != null)
        DefinitionVM.Name = value;
    }
  }

  public bool IsNestedFieldsStyle => DefinitionVM is FieldDefinitionViewModel fdvm ? fdvm.IsNestedFieldsStyle : false;

  public bool IsInherited => DefinitionVM?.IsInherited ?? false;
  public bool CanDelete
  {
    get
    {
      if (DefinitionVM?.Definition is LoreTypeDefinition typeDef)
        return true;

      else return !IsInherited;
    }
  }

  public bool? IsRequired
  {
    get
    {
      if (DefinitionVM?.Definition is IRequirable iReq)
        return iReq.required;
      else return false;
    }
    set
    {
      if (DefinitionVM?.Definition is IRequirable iReq && value.HasValue)
      {
        iReq.required = value.Value;
        SettingsRefresher.Apply(CurrentSettingsVM);
      }
    }
  }

  public bool CanEditRequired
  {
    get
    {
      if (DefinitionVM?.Definition is IRequirable iReq)
      {
        if (DefinitionVM.IsInherited && (DefinitionVM.Definition.Base as IRequirable).required)
          return false;
      }
      return true;
    }
  }

  public bool NameIsReadOnly { get => !CanEditName; }

  public bool CanEditName
  {
    get
    {
      if (IsGroupNode) return false;
      else return DefinitionVM?.CanEditName ?? false;
    }
  }

  public Thickness TextboxBorderThickness
  {
    get
    {
      if (IsGroupNode || !CanEditName) return new Thickness(0);
      else return new Thickness(1);
    }
  }


  private DefinitionTreeNodeViewModel(LoreSettingsViewModel curSettingsVM)
  {
    _settings = curSettingsVM;
    GoToNodeOfDefinitionCommand = ReactiveCommand.Create<LoreDefinitionBase>(GoToNodeOfDefinition);
    AddDefinitionCommand = ReactiveCommand.Create(() => curSettingsVM.AddDefinition(this));
    DeleteDefinitionCommand = ReactiveCommand.Create(() => curSettingsVM.DeleteDefinition(this));
  }


  public DefinitionTreeNodeViewModel(ETreeNodeType thisNodeType, LoreSettingsViewModel curSettingsVM, LoreDefinitionViewModel defVM = null) : this(curSettingsVM)
  {
    TreeNodeType = thisNodeType;
    DefinitionVM = defVM;

    BuildChildren();
  }

  public void GoToNodeOfDefinition(LoreDefinitionBase definition) => DefinitionVM.CurrentSettingsViewModel.GoToNodeOfDefinition(definition);

  public void AddChild(DefinitionTreeNodeViewModel child)
  {
    child.Parent = this;
    Children.Add(child);
  }

  public void InsertChild(int index, DefinitionTreeNodeViewModel child)
  {
    child.Parent = this;
    Children.Insert(index, child);
  }

  public void RemoveChild(DefinitionTreeNodeViewModel child)
  {
    if (Children.Remove(child))
      child.Parent = null;
  }

  public void BuildChildren()
  {

    if (DefinitionVM.Definition is IFieldDefinitionContainer ifcont)
    {
      // If this is a field definition, but it's NOT a nested values field, break out of this method
      if (ifcont is LoreFieldDefinition fd)
      {
        if (fd.style != EFieldStyle.NestedValues) return;

        if (DefinitionVM.Fields != null)
        {
          foreach (var f in DefinitionVM.Fields)
            AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, f));
        }
      }
      else
      {
        var fieldGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.FieldGroupingNode, CurrentSettingsVM, DefinitionVM);
        if (DefinitionVM.Fields != null)
        {
          foreach (var f in DefinitionVM.Fields)
            fieldGroup.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, f));
        }
        AddChild(fieldGroup);
      }
    }

    if (DefinitionVM.Definition is ISectionDefinitionContainer iscont)
    {
      var sectionGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.SectionGroupingNode, CurrentSettingsVM, DefinitionVM);
      if (DefinitionVM.Sections != null)
      {
        foreach (var s in DefinitionVM.Sections)
          sectionGroup.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.SectionDefinitionNode, CurrentSettingsVM, s));
      }
      AddChild(sectionGroup);
    }

    if (DefinitionVM.Definition is ICollectionDefinitionContainer iccont)
    {
      var colGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionGroupingNode, CurrentSettingsVM, DefinitionVM);
      if (DefinitionVM.Sections != null)
      {
        foreach (var c in DefinitionVM.Collections)
          colGroup.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, c));
      }
      AddChild(colGroup);
    }


    if (DefinitionVM.Definition is IEmbeddedNodeDefinitionContainer iecont)
    {
      var embedGroup = new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeGroupingNode, CurrentSettingsVM, DefinitionVM);
      if (DefinitionVM.EmbeddedNodes != null)
      {
        foreach (var e in DefinitionVM.EmbeddedNodes)
          embedGroup.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeDefinitionNode, CurrentSettingsVM, e));
      }
      AddChild(embedGroup);
    }

    // Handle locally defined collection definition
    if(DefinitionVM is CollectionDefinitionViewModel cdvm && cdvm.IsUsingLocalCollectionDef)
    {
      AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode,CurrentSettingsVM, cdvm.ContainedTypeVM));
    }

    if (DefinitionVM.Definition is IPicklistEntryDefinitionContainer ipcont && ipcont.HasEntries)
    {
      foreach (var p in DefinitionVM.PicklistEntries)
      {
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.PicklistEntryDefinitionNode, CurrentSettingsVM, p));
      }
    }
  }

  internal void RefreshTreeNode()
  {
    // When a definition is added or deleted, that one definition (model), its viewmodel, and the tree node view model are deleted (in LoreSettingsViewModel).
    // But that does not delete inherited versions of that definition (where it applies).
    // Example, We have a base Type Being, and a subtype Character. If we delete field Appearance on Being in the settings editor dialog, this won't delete the Appearance
    // field on Character type -- the Field definition on the Character type will be marked as deleted on its WasDeleted boolean property -- during this check (RefreshTreeNode),
    // if a node with a definition has been found with WasDeleted true, it will remove the definition view model and tree node.

    // If this is a grouping node, we need to ensure this group contains the correct nodes for the corresponding contained fields on the definition model.
    // i.e. if this is a field grouping node under a type node, we need to make sure this grouping of fields isn't missing any fields on the parent or that it doesn't contain extra fields

    // Handle additions/deletions for grouping nodes.
    if (IsGroupNode)
    {
      // Handle deletions
      for (int i = Children.Count() - 1; i >= 0; i--)
      {
        DefinitionTreeNodeViewModel dtnvm = Children[i];
        if(dtnvm.DefinitionVM != null && dtnvm.DefinitionVM.Definition.WasDeleted)
        {
          switch (dtnvm.DefinitionVM)
          {
            case FieldDefinitionViewModel fdvmToDel:
              DefinitionVM.Fields.Remove(fdvmToDel);
              break;
            case SectionDefinitionViewModel sdvmToDel:
              DefinitionVM.Sections.Remove(sdvmToDel);
              break;
            case CollectionDefinitionViewModel cdvmToDel:
              DefinitionVM.Collections.Remove(cdvmToDel);
              break;
            case EmbeddedNodeDefinitionViewModel endvmToDel:
              DefinitionVM.EmbeddedNodes.Remove(endvmToDel);
              break;
          }

          Children.Remove(dtnvm);
        }
      }

      // Handle additions
      ObservableCollection<LoreDefinitionViewModel> listToCheck;
      Type typeToAdd;
      switch (TreeNodeType)
      {
        case ETreeNodeType.FieldGroupingNode:
          for (int i = 0; i < DefinitionVM.Fields.Count(); i++)
          {
            FieldDefinitionViewModel fdvmToAdd = DefinitionVM.Fields[i];
            if (!Children.Any(node => node.DefinitionVM as FieldDefinitionViewModel == fdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, fdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.SectionGroupingNode:
          for (int i = 0; i < DefinitionVM.Sections.Count(); i++)
          {
            SectionDefinitionViewModel sdvmToAdd = DefinitionVM.Sections[i];
            if (!Children.Any(node => node.DefinitionVM as SectionDefinitionViewModel == sdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.SectionDefinitionNode, CurrentSettingsVM, sdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.CollectionGroupingNode:
          for (int i = 0; i < DefinitionVM.Collections.Count(); i++)
          {
            CollectionDefinitionViewModel cdvmToAdd = DefinitionVM.Collections[i];
            if (!Children.Any(node => node.DefinitionVM as CollectionDefinitionViewModel == cdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, cdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.EmbeddedNodeGroupingNode:
          for (int i = 0; i < DefinitionVM.EmbeddedNodes.Count(); i++)
          {
            EmbeddedNodeDefinitionViewModel endvmToAdd = DefinitionVM.EmbeddedNodes[i];
            if (!Children.Any(node => node.DefinitionVM as EmbeddedNodeDefinitionViewModel == endvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeDefinitionNode, CurrentSettingsVM, endvmToAdd));
            }
          }
          break;
      }
    }


    // Now handle non-grouping nodes (ie picklist and picklist definition nodes, and fields with nested fields
    if(TreeNodeType == ETreeNodeType.FieldDefinitionNode && DefinitionVM is FieldDefinitionViewModel fdvm && fdvm.Style == EFieldStyle.NestedValues)
    {
      // Handle deletions
      for (int i = Children.Count() - 1; i >= 0; i--)
      {
        DefinitionTreeNodeViewModel fieldNodeToDel = Children[i];
        if (fieldNodeToDel.DefinitionVM != null && fieldNodeToDel.DefinitionVM.Definition.WasDeleted)
        {
          DefinitionVM.Fields.Remove(fieldNodeToDel.DefinitionVM as FieldDefinitionViewModel);
          Children.Remove(fieldNodeToDel);
        }
      }

      // Handle additions
      for(int i = 0; i < DefinitionVM.Fields.Count(); i++)
      {
        FieldDefinitionViewModel fieldDefToAdd = DefinitionVM.Fields[i];
        if (!Children.Any(node => node.DefinitionVM as FieldDefinitionViewModel == fieldDefToAdd))
        {
          InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, fieldDefToAdd));
        }
      }
    }
    else if(TreeNodeType == ETreeNodeType.PicklistDefinitionNode || TreeNodeType == ETreeNodeType.PicklistEntryDefinitionNode)
    {
      // Handle deletions
      for (int i = Children.Count() - 1; i >= 0; i--)
      {
        DefinitionTreeNodeViewModel picklistEntryToDel = Children[i];
        if (picklistEntryToDel.DefinitionVM != null && picklistEntryToDel.DefinitionVM.Definition.WasDeleted)
        {
          DefinitionVM.PicklistEntries.Remove(picklistEntryToDel.DefinitionVM as PicklistEntryDefinitionViewModel);
          Children.Remove(picklistEntryToDel);
        }
      }

      // Handle additions
      for (int i = 0; i < DefinitionVM.PicklistEntries.Count(); i++)
      {
        PicklistEntryDefinitionViewModel picklistEntryToAdd = DefinitionVM.PicklistEntries[i];
        if (!Children.Any(node => node.DefinitionVM as PicklistEntryDefinitionViewModel == picklistEntryToAdd))
        {
          InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.PicklistEntryDefinitionNode, CurrentSettingsVM, picklistEntryToAdd));
        }
      }
    }

    /*


    // Parent in this case is the preceeding node in the tree, which should hold the type or section or whatever definition -- unless it's a ROOT grouping node
    if (this.IsGroupNode && Parent != null && Parent.DefinitionVM != null)
    {
      LoreDefinitionViewModel curContainingDvm = Parent.DefinitionVM;
      LoreDefinitionBase curContainingModel = curContainingDvm.Definition;

      // Check if we need to remove a node (that's an inherited definition) due to the base definition being deleted.
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        // Check if definition or its base was deleted.
        DefinitionTreeNodeViewModel treeNode = Children[i];
        if ((bool)(treeNode.DefinitionVM?.Definition.WasDeleted))
        {
          Children.Remove(treeNode);
        }
      }

      if (GroupName == FieldGroupName)
      {
        IFieldDefinitionContainer curContainingFieldContainerModel = curContainingModel as IFieldDefinitionContainer;

        if (curContainingFieldContainerModel.HasFields)
        {

          // Check if we need to add new definitions
          for (int i = 0; i < curContainingFieldContainerModel.fields.Count; i++)
          {
            LoreFieldDefinition curFieldModel = curContainingFieldContainerModel.fields[i];


            // Do some reordering, keeping the order the SAME as the model-level list of fields.
            // This prevents 'duplicate' nodes from being created by the code below because I'm dumb
            var nodeOfName = Children.FirstOrDefault(node => node.DefinitionVM.Definition == curFieldModel);
            if (nodeOfName != null && Children.IndexOf(nodeOfName) != i)
            {
              Children.Move(Children.IndexOf(nodeOfName), i);
            }

            // If Children is empty, we want to avoid an out of range exception, shortcut to add new field
            if (Children.Count < 1)
            {
              AddChild(new DefinitionTreeNodeViewModel(new FieldDefinitionViewModel(curFieldModel)));
              continue;
            }

            // If the index int i is going to cause an out-of-range exception, I think we can assume we need to add a node for the current field definition
            if (i >= Children.Count)
            {
              AddChild(new DefinitionTreeNodeViewModel(new FieldDefinitionViewModel(curFieldModel)));
              continue;
            }

            DefinitionTreeNodeViewModel curChildUnderThisGroup = Children[i];

            if (curChildUnderThisGroup.DefinitionVM.Definition != curFieldModel)
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(new FieldDefinitionViewModel(curFieldModel)));
            }
          }

        }
      }
      else if (GroupName == SectionGroupName)
      {
        ISectionDefinitionContainer curContainingSectionContainerModel = curContainingModel as ISectionDefinitionContainer;

        if (curContainingSectionContainerModel.HasSections)
        {
          for (int i = 0; i < curContainingSectionContainerModel.sections.Count; i++)
          {
            LoreSectionDefinition curSectionModel = curContainingSectionContainerModel.sections[i];


            // Do some reordering, keeping the order the SAME as the model-level list of sections.
            // This prevents 'duplicate' nodes from being created by the code below because I'm dumb
            var nodeOfName = Children.FirstOrDefault(node => node.DefinitionVM.Definition == curSectionModel);
            if (nodeOfName != null && Children.IndexOf(nodeOfName) != i)
            {
              Children.Move(Children.IndexOf(nodeOfName), i);
            }


            // If Children is empty, we want to avoid an out of range exception, shortcut to add new section
            if (Children.Count < 1)
            {
              AddChild(new DefinitionTreeNodeViewModel(new SectionDefinitionViewModel(curSectionModel)));
              continue;
            }


            // If the index int i is going to cause an out-of-range exception, I think we can assume we need to add a node for the current section definition
            if (i >= Children.Count)
            {
              AddChild(new DefinitionTreeNodeViewModel(new SectionDefinitionViewModel(curSectionModel)));
              continue;
            }


            DefinitionTreeNodeViewModel curChildUnderThisGroup = Children[i];

            if (curChildUnderThisGroup.DefinitionVM.Definition != curSectionModel)
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(new SectionDefinitionViewModel(curSectionModel)));
            }
          }
        }
      }
      else if (GroupName == CollectionGroupName)
      {
        ICollectionDefinitionContainer curContainingCollectionContainerModel = curContainingModel as ICollectionDefinitionContainer;

        if (curContainingCollectionContainerModel.HasCollections)
        {
          for (int i = 0; i < curContainingCollectionContainerModel.collections.Count; i++)
          {
            LoreCollectionDefinition curCollectionModel = curContainingCollectionContainerModel.collections[i];

            // Do some reordering, keeping the order the SAME as the model-level list of fields.
            // This prevents 'duplicate' nodes from being created by the code below because I'm dumb
            var nodeOfName = Children.FirstOrDefault(node => node.DefinitionVM.Definition == curCollectionModel);
            if (nodeOfName != null && Children.IndexOf(nodeOfName) != i)
            {
              Children.Move(Children.IndexOf(nodeOfName), i);
            }

            // If Children is empty, we want to avoid an out of range exception, shortcut to add new collection
            if (Children.Count < 1)
            {
              AddChild(new DefinitionTreeNodeViewModel(new CollectionDefinitionViewModel(curCollectionModel)));
              continue;
            }

            // If the index int i is going to cause an out-of-range exception, I think we can assume we need to add a node for the current collectoin definition
            if (i >= Children.Count)
            {
              AddChild(new DefinitionTreeNodeViewModel(new CollectionDefinitionViewModel(curCollectionModel)));
              continue;
            }


            DefinitionTreeNodeViewModel curChildUnderThisGroup = Children[i];

            if (curChildUnderThisGroup.DefinitionVM.Definition != curCollectionModel)
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(new CollectionDefinitionViewModel(curCollectionModel)));
            }
          }
        }
      }
      else if (GroupName == EmbeddedNodesGroupName)
      {
        IEmbeddedNodeDefinitionContainer curContainingEmbeddedContainerModel = curContainingModel as IEmbeddedNodeDefinitionContainer;

        if (curContainingEmbeddedContainerModel.HasNestedNodes)
        {
          for (int i = 0; i < curContainingEmbeddedContainerModel.embeddedNodeDefs.Count; i++)
          {
            LoreEmbeddedNodeDefinition curEmbeddedModel = curContainingEmbeddedContainerModel.embeddedNodeDefs[i];


            // Do some reordering, keeping the order the SAME as the model-level list of fields.
            // This prevents 'duplicate' nodes from being created by the code below because I'm dumb
            var nodeOfName = Children.FirstOrDefault(node => node.DefinitionVM.Definition == curEmbeddedModel);
            if (nodeOfName != null && Children.IndexOf(nodeOfName) != i)
            {
              Children.Move(Children.IndexOf(nodeOfName), i);
            }

            // If Children is empty, we want to avoid an out of range exception, shortcut to add new embedded node
            if (Children.Count < 1)
            {
              AddChild(new DefinitionTreeNodeViewModel(new EmbeddedNodeDefinitionViewModel(curEmbeddedModel)));
              continue;
            }

            // If the index int i is going to cause an out-of-range exception, I think we can assume we need to add a node for the current embedded definition
            if (i >= Children.Count)
            {
              AddChild(new DefinitionTreeNodeViewModel(new EmbeddedNodeDefinitionViewModel(curEmbeddedModel)));
              continue;
            }


            DefinitionTreeNodeViewModel curChildUnderThisGroup = Children[i];

            if (curChildUnderThisGroup.DefinitionVM.Definition != curEmbeddedModel)
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(new EmbeddedNodeDefinitionViewModel(curEmbeddedModel)));
            }
          }
        }
      }
    }

    // Handle a local collection definition being added, etc
    if (this.DefinitionVM is CollectionDefinitionViewModel cdvm)
    {
      if (Children.Count == 0)
      {

        if (cdvm.IsUsingLocalCollectionDef)
        {
          AddChild(new DefinitionTreeNodeViewModel(cdvm.ContainedTypeVM));
        }
      }
      else if (Children[0].DefinitionVM != cdvm)
      {
        Children.Clear();
        AddChild(new DefinitionTreeNodeViewModel(cdvm.ContainedTypeVM));
      }
    }

    // Handle a field's nested fields being deleted or added
    if (this.DefinitionVM?.Definition is LoreFieldDefinition lfd && lfd.style == EFieldStyle.NestedValues && lfd.HasFields)
    {
      FieldDefinitionViewModel curContainingDvm = DefinitionVM as FieldDefinitionViewModel;


      // Check if we need to remove a node (that's an inherited definition) due to the base definition being deleted.
      for (int i = Children.Count - 1; i >= 0; i--)
      {
        LoreFieldDefinition curFieldModel = (curContainingDvm.Definition as LoreFieldDefinition).fields[i];

        // Do some reordering, keeping the order the SAME as the model-level list of fields.
        // This prevents 'duplicate' nodes from being created by the code below because I'm dumb
        var nodeOfName = Children.FirstOrDefault(node => node.DefinitionVM.Definition == curFieldModel);
        if (nodeOfName != null && Children.IndexOf(nodeOfName) != i)
        {
          Children.Move(Children.IndexOf(nodeOfName), i);
        }


        DefinitionTreeNodeViewModel treeNode = Children[i];
        if ((bool)(treeNode.DefinitionVM?.Definition.WasDeleted))
        {
          Children.Remove(treeNode);
        }
      }
    }
    */

    if (DefinitionVM != null)
      DefinitionVM.RefreshUI();

    foreach (DefinitionTreeNodeViewModel vm in Children)
      vm.RefreshTreeNode();

    RaiseTreePropertiesChanged();
  }

  private void RaiseTreePropertiesChanged()
  {
    this.RaisePropertyChanged(nameof(IsRequired));
    this.RaisePropertyChanged(nameof(IsInherited));
    this.RaisePropertyChanged(nameof(DisplayName));
    this.RaisePropertyChanged(nameof(CanEditRequired));
    this.RaisePropertyChanged(nameof(CanEditName));
    this.RaisePropertyChanged(nameof(CanDelete));
    this.RaisePropertyChanged(nameof(NameIsReadOnly));
    this.RaisePropertyChanged(nameof(IsNestedFieldsStyle));
    this.RaisePropertyChanged(nameof(TextboxBorderThickness));
    this.RaisePropertyChanged(nameof(InheritanceLabelString));
  }

  internal DefinitionTreeNodeViewModel? FindNodeOfDefinition(LoreDefinitionBase definition, out IndexPath pathToVM)
  {
    DefinitionTreeNodeViewModel dtvm;

    if (!IsGroupNode)
    {
      if (this.DefinitionVM.Definition == definition)
      {
        pathToVM = new IndexPath();
        return this;
      }
    }

    foreach (DefinitionTreeNodeViewModel node in Children)
    {
      dtvm = node.FindNodeOfDefinition(definition, out var resultingPath);
      if (dtvm != null)
      {
        if (resultingPath.Count == 0)
          pathToVM = new IndexPath(Children.IndexOf(node));
        else
          pathToVM = new IndexPath(new int[] { Children.IndexOf(node) }.Concat(resultingPath.ToArray()));
        return dtvm;
      }
    }

    pathToVM = new IndexPath();
    return null;
  }

  public override string ToString() => DisplayName;
}