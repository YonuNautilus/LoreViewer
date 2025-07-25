﻿using Avalonia;
using Avalonia.Controls;
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

  private ETreeNodeType[] DefinitionLevelGroupingNodeTypes = {
    ETreeNodeType.FieldGroupingNode,
    ETreeNodeType.SectionGroupingNode,
    ETreeNodeType.CollectionGroupingNode,
    ETreeNodeType.EmbeddedNodeGroupingNode };

  private ETreeNodeType[] DefinitionNodeTypes = {
    ETreeNodeType.TypeDefinitionNode,
    ETreeNodeType.FieldDefinitionNode,
    ETreeNodeType.SectionDefinitionNode,
    ETreeNodeType.CollectionDefinitionNode,
    ETreeNodeType.EmbeddedNodeDefinitionNode,
    ETreeNodeType.PicklistDefinitionNode,
    ETreeNodeType.PicklistEntryDefinitionNode,
  };

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
  public bool IsRootGroupNode { get { return RootGroupingNodeTypes.Contains(TreeNodeType); } }
  public bool IsDefinitionLevelGroupingNode { get { return DefinitionLevelGroupingNodeTypes.Contains(TreeNodeType); } }
  public bool IsDefinitionNode { get { return DefinitionNodeTypes.Contains(TreeNodeType); } }

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
        case ETreeNodeType.CollectionGroupingNode:
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

  public bool IsNestedFieldsStructure => DefinitionVM is FieldDefinitionViewModel fdvm ? fdvm.IsNestedFieldsStructure : false;

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
      if (DefinitionVM != null)
      {
        return DefinitionVM.CanEditRequired;
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

  private void BuildChildren()
  {
    // For Picklist, Picklist Entries
    if(TreeNodeType == ETreeNodeType.PicklistDefinitionNode || TreeNodeType == ETreeNodeType.PicklistEntryDefinitionNode)
      foreach(PicklistEntryDefinitionViewModel pledvm in DefinitionVM.PicklistEntries)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.PicklistEntryDefinitionNode, CurrentSettingsVM, pledvm));

    // For Fields with nested fields
    if (TreeNodeType == ETreeNodeType.FieldDefinitionNode && (DefinitionVM as FieldDefinitionViewModel).HasSubFields)
      foreach(FieldDefinitionViewModel fdvm in DefinitionVM.Fields)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, fdvm));


    // For Locally Defined Collection
    if (TreeNodeType == ETreeNodeType.CollectionDefinitionNode && DefinitionVM is CollectionDefinitionViewModel lcdvm && lcdvm.IsUsingLocalCollectionDef)
      AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, lcdvm.ContainedTypeVM as CollectionDefinitionViewModel));


    // Now for grouping nodes
    if (TreeNodeType == ETreeNodeType.FieldGroupingNode && DefinitionVM.Fields != null)
      foreach (FieldDefinitionViewModel f in DefinitionVM.Fields)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, f));

    if(TreeNodeType == ETreeNodeType.SectionGroupingNode && DefinitionVM.Sections != null)
      foreach(SectionDefinitionViewModel s in DefinitionVM.Sections)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.SectionDefinitionNode, CurrentSettingsVM, s));

    if (TreeNodeType == ETreeNodeType.CollectionGroupingNode && DefinitionVM.Collections != null)
      foreach (CollectionDefinitionViewModel c in DefinitionVM.Collections)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, c));

    if(TreeNodeType == ETreeNodeType.EmbeddedNodeDefinitionNode && DefinitionVM.EmbeddedNodes != null)
      foreach (EmbeddedNodeDefinitionViewModel e in DefinitionVM.EmbeddedNodes)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeDefinitionNode, CurrentSettingsVM, e));

    // Handle locally defined collection definition
    if(DefinitionVM is CollectionDefinitionViewModel cdvm && cdvm.IsUsingLocalCollectionDef)
    {
      AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, cdvm.ContainedTypeVM));
    }



    // For definition-level nodes that need grouping nodes as children (Type nodes, Section Nodes)
    if (IsDefinitionNode)
    {
      // Do not make a Field grouping nodes under field nodes -- if a field has nested fields, they are NOT put in a grouping node
      if (DefinitionVM.Definition is IFieldDefinitionContainer && DefinitionVM is not FieldDefinitionViewModel)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldGroupingNode, CurrentSettingsVM, DefinitionVM));

      if (DefinitionVM.Definition is ISectionDefinitionContainer)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.SectionGroupingNode, CurrentSettingsVM, DefinitionVM));

      if (DefinitionVM.Definition is ICollectionDefinitionContainer)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionGroupingNode, CurrentSettingsVM, DefinitionVM));

      if (DefinitionVM.Definition is IEmbeddedNodeDefinitionContainer)
        AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeGroupingNode, CurrentSettingsVM, DefinitionVM));
    }
  }

  private void BuildFieldChildren(DefinitionTreeNodeViewModel parentNode, ObservableCollection<FieldDefinitionViewModel> fieldsToAdd)
  {
    foreach (FieldDefinitionViewModel fdvm in fieldsToAdd)
      parentNode.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, fdvm));
  }

  private void BuildSectionChildren(DefinitionTreeNodeViewModel parentNode, ObservableCollection<SectionDefinitionViewModel> sectionsToAdd)
  {
    foreach (SectionDefinitionViewModel sdvm in sectionsToAdd)
      parentNode.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.SectionDefinitionNode, CurrentSettingsVM, sdvm));
  }

  private void BuildCollectionChildren(DefinitionTreeNodeViewModel parentNode, ObservableCollection<CollectionDefinitionViewModel> collectionsToAdd)
  {
    foreach (CollectionDefinitionViewModel cdvm in collectionsToAdd)
      parentNode.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, cdvm));
  }

  private void BuildEmbededNodeChildren(DefinitionTreeNodeViewModel parentNode, ObservableCollection<EmbeddedNodeDefinitionViewModel> embeddedNodesToAdd)
  {
    foreach (EmbeddedNodeDefinitionViewModel endvm in embeddedNodesToAdd)
      parentNode.AddChild(new DefinitionTreeNodeViewModel(ETreeNodeType.EmbeddedNodeDefinitionNode, CurrentSettingsVM, endvm));
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
          for (int i = 0; i < DefinitionVM.Fields?.Count(); i++)
          {
            FieldDefinitionViewModel fdvmToAdd = DefinitionVM.Fields[i];
            if (!Children.Any(node => node.DefinitionVM as FieldDefinitionViewModel == fdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.FieldDefinitionNode, CurrentSettingsVM, fdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.SectionGroupingNode:
          for (int i = 0; i < DefinitionVM.Sections?.Count(); i++)
          {
            SectionDefinitionViewModel sdvmToAdd = DefinitionVM.Sections[i];
            if (!Children.Any(node => node.DefinitionVM as SectionDefinitionViewModel == sdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.SectionDefinitionNode, CurrentSettingsVM, sdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.CollectionGroupingNode:
          for (int i = 0; i < DefinitionVM.Collections?.Count(); i++)
          {
            CollectionDefinitionViewModel cdvmToAdd = DefinitionVM.Collections[i];
            if (!Children.Any(node => node.DefinitionVM as CollectionDefinitionViewModel == cdvmToAdd))
            {
              InsertChild(i, new DefinitionTreeNodeViewModel(ETreeNodeType.CollectionDefinitionNode, CurrentSettingsVM, cdvmToAdd));
            }
          }
          break;
        case ETreeNodeType.EmbeddedNodeGroupingNode:
          for (int i = 0; i < DefinitionVM.EmbeddedNodes?.Count(); i++)
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
    if(TreeNodeType == ETreeNodeType.FieldDefinitionNode && DefinitionVM is FieldDefinitionViewModel fdvm && fdvm.InputStructure == EFieldInputStructure.NestedValues)
    {
      // Handle deletions
      for (int i = Children.Count() - 1; i >= 0; i--)
      {
        DefinitionTreeNodeViewModel fieldNodeToDel = Children[i];
        if (fieldNodeToDel.DefinitionVM != null && fieldNodeToDel.DefinitionVM.Definition.WasDeleted)
        {
          DefinitionVM.Fields.Remove(fieldNodeToDel.DefinitionVM as FieldDefinitionViewModel);
          RemoveChild(fieldNodeToDel);
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
          RemoveChild(picklistEntryToDel);
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

      foreach (DefinitionTreeNodeViewModel childNode in Children)
        childNode.RefreshTreeNode();
    }

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
    this.RaisePropertyChanged(nameof(IsNestedFieldsStructure));
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