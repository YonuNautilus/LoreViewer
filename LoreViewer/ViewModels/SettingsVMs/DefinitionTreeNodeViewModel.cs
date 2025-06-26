using Avalonia;
using Avalonia.Controls;
using DynamicData;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using Markdig.Syntax;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs;

public class DefinitionTreeNodeViewModel : ReactiveObject
{
  private class NewNameTracker
  {
    private int number = 1;
    private string prefix = string.Empty;
    public NewNameTracker(string namePrefix) { prefix = namePrefix; }

    public string GetName() => $"{prefix}_{number++}";
  }

  private const string TypeGroupName = "Types";
  private const string FieldGroupName = "Fields";
  private const string SectionGroupName = "Sections";
  private const string CollectionGroupName = "Collections";
  private const string EmbeddedNodsGroupName = "Embedded Nodes";
  private const string PicklistGroupName = "Picklists";

  private static NewNameTracker tpyeNamer = new NewNameTracker("NewType");
  private static NewNameTracker fieldNamer = new NewNameTracker("NewField");
  private static NewNameTracker sectionNamer = new NewNameTracker("NewSection");
  private static NewNameTracker collectionNamer = new NewNameTracker("NewCollection");
  private static NewNameTracker embeddedNamer = new NewNameTracker("NewEmbedded");
  private static NewNameTracker picklistNamer = new NewNameTracker("NewPicklist");
  private LoreSettingsViewModel _settings;
  private Type _addType;

  public ReactiveCommand<LoreDefinitionBase, Unit> GoToNodeOfDefinitionCommand { get; }

  public LoreSettingsViewModel CurrentSettingsVM => _settings;

  public DefinitionTreeNodeViewModel? Parent { get; private set; }
  public ObservableCollection<DefinitionTreeNodeViewModel> Children { get; } = new();

  public LoreDefinitionViewModel? DefinitionVM { get; }
  public string GroupName { get; } = string.Empty;
  public bool IsGroupNode { get; }

  public string InheritanceLabelString => DefinitionVM?.InheritanceLabelString;

  public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
  public ReactiveCommand<Unit, Unit> AddDefinitionCommand { get; }

  public string DisplayName
  {
    get
    {
      if (IsGroupNode) return Children.Any() ? $"{GroupName} ({Children.Count})" : GroupName;
      else return DefinitionVM?.Name ?? "(Unnamed)";
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
        SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
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


  public DefinitionTreeNodeViewModel()
  {
    GoToNodeOfDefinitionCommand = ReactiveCommand.Create<LoreDefinitionBase>(GoToNodeOfDefinition);
  }

  // Group node constructor
  public DefinitionTreeNodeViewModel(string groupName, LoreSettingsViewModel svm = null, Type addType = null)
  {
    IsGroupNode = true;
    GroupName = groupName;
    _settings = svm;
    _addType = addType;

    // For adding type definitions and collections directly at the settings level
    if (_settings != null && _addType != null)
    {
      AddDefinitionCommand = ReactiveCommand.Create(() =>
      {
        if (_addType == typeof(LoreTypeDefinition))
        {
          var newDef = new LoreTypeDefinition { name = DefinitionTreeNodeViewModel.tpyeNamer.GetName() };
          _settings.m_oLoreSettings.types.Add(newDef);
          var newVM = new TypeDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (_addType == typeof(LoreCollectionDefinition))
        {
          var newDef = new LoreCollectionDefinition { name = DefinitionTreeNodeViewModel.collectionNamer.GetName() };
          _settings.m_oLoreSettings.collections.Add(newDef);
          var newVM = new CollectionDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (_addType == typeof(LorePicklistDefinition))
        {
          var newDef = new LorePicklistDefinition { name = DefinitionTreeNodeViewModel.picklistNamer.GetName() };
          _settings.m_oLoreSettings.picklists.Add(newDef);
          var newVM = new PicklistDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }

          SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
      });
    }
    // For adding field, section, collection and embedded node definitions at the type level
    else
    {
      AddDefinitionCommand = ReactiveCommand.Create(() =>
      {

        if (Parent?.DefinitionVM?.Definition is IFieldDefinitionContainer fCont && groupName == FieldGroupName)
        {
          var newDef = new LoreFieldDefinition { name = DefinitionTreeNodeViewModel.fieldNamer.GetName() };
          fCont.AddField(newDef);
          var newVM = new FieldDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is ISectionDefinitionContainer sCont && groupName == SectionGroupName)
        {
          var newDef = new LoreSectionDefinition { name = DefinitionTreeNodeViewModel.sectionNamer.GetName() };
          sCont.AddSection(newDef);
          var newVM = new SectionDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is ICollectionDefinitionContainer cCont && groupName == CollectionGroupName)
        {
          var newDef = new LoreCollectionDefinition { name = DefinitionTreeNodeViewModel.collectionNamer.GetName() };
          cCont.AddCollection(newDef);
          var newVM = new CollectionDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is IEmbeddedNodeDefinitionContainer eCont && groupName == EmbeddedNodsGroupName)
        {
          var newDef = new LoreEmbeddedNodeDefinition { name = DefinitionTreeNodeViewModel.embeddedNamer.GetName() };
          eCont.AddEmbedded(newDef);
          var newVM = new EmbeddedNodeDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM.Definition is IPicklistDefinitionContainer pCont && groupName == PicklistGroupName)
        {
          var newDef = new LorePicklistDefinition { name = DefinitionTreeNodeViewModel.picklistNamer.GetName() };
          pCont.AddPicklistDefinition(newDef);
          var newVM = new PicklistDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }

          SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
      });
    }
  }

  // Leaf node constructor
  public DefinitionTreeNodeViewModel(LoreDefinitionViewModel vm) : this()
  {
    DefinitionVM = vm;
    IsGroupNode = false;

    BuildChildren();

    if(vm is FieldDefinitionViewModel fdvm)
    {
      AddDefinitionCommand = ReactiveCommand.Create(() =>
      {
        if (DefinitionVM != null && DefinitionVM.Definition is LoreFieldDefinition fDef && fDef.style == EFieldStyle.NestedValues)
        {
          IFieldDefinitionContainer fCont = DefinitionVM.Definition as IFieldDefinitionContainer;
          var newDef = new LoreFieldDefinition { name = DefinitionTreeNodeViewModel.fieldNamer.GetName() };
          fCont.AddField(newDef);
          var newVM = new FieldDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));

          SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
        }
      });
    }

    if (vm is PicklistDefinitionViewModel pldvm)
    {
      AddDefinitionCommand = ReactiveCommand.Create(() =>
      {
        if (DefinitionVM != null && DefinitionVM.Definition is LorePicklistDefinition pickDef)
        {
          IPicklistDefinitionContainer pCont = DefinitionVM.Definition as IPicklistDefinitionContainer;
          var newDef = new LorePicklistDefinition { name = DefinitionTreeNodeViewModel.picklistNamer.GetName() };
          pCont.AddPicklistDefinition(newDef);
          var newVM = new PicklistDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));

          SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
        }
      });
    }

    DeleteCommand = ReactiveCommand.Create(() =>
    {
      if (Parent is DefinitionTreeNodeViewModel parentNode)
      {
        // Attempt to remove from parent model container
        if (parentNode.Parent?.DefinitionVM?.Definition is IFieldDefinitionContainer fCont && vm.Definition is LoreFieldDefinition fld)
        {
          fCont.fields.Remove(fld);
          fld.IsDeleted = true;
        }
        else if (parentNode.Parent?.DefinitionVM?.Definition is ISectionDefinitionContainer sCont && vm.Definition is LoreSectionDefinition sec)
        {
          sCont.sections.Remove(sec);
          sec.IsDeleted = true;
        }
        else if (parentNode.Parent?.DefinitionVM?.Definition is ICollectionDefinitionContainer cCont && vm.Definition is LoreCollectionDefinition col)
        {
          cCont.collections.Remove(col);
          col.IsDeleted = true;
        }
        else if (parentNode.Parent?.DefinitionVM?.Definition is IEmbeddedNodeDefinitionContainer eCont && vm.Definition is LoreEmbeddedNodeDefinition emb)
        {
          eCont.embeddedNodeDefs.Remove(emb);
          emb.IsDeleted = true;
        }
        // Otherwise, remove a nested field definition
        else if (parentNode.DefinitionVM?.Definition is IFieldDefinitionContainer nestedFCont && nestedFCont.HasFields && vm.Definition is LoreFieldDefinition fd)
        {
          nestedFCont.fields.Remove(fd);
          fd.IsDeleted = true;
        }

        parentNode.RemoveChild(this);

        SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
      }
    });
  }

  public void GoToNodeOfDefinition(LoreDefinitionBase definition) => LoreDefinitionViewModel.CurrentSettingsViewModel.GoToNodeOfDefinition(definition);

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
    if (DefinitionVM == null)
      return;

    if (DefinitionVM.Definition is IFieldDefinitionContainer ifcont)
    {
      // If this is a field definition, but it's NOT a nested values field, break out of this method
      if (ifcont is LoreFieldDefinition fd)
      {
        if (fd.style != EFieldStyle.NestedValues) return;

        if (DefinitionVM.Fields != null)
        {
          foreach (var f in DefinitionVM.Fields)
            AddChild(new DefinitionTreeNodeViewModel(f));
        }
      }
      else
      {
        var fieldGroup = new DefinitionTreeNodeViewModel(FieldGroupName, addType: typeof(LoreFieldDefinition));
        if (DefinitionVM.Fields != null)
        {
          foreach (var f in DefinitionVM.Fields)
            fieldGroup.AddChild(new DefinitionTreeNodeViewModel(f));
        }
        AddChild(fieldGroup);
      }
    }

    if (DefinitionVM.Definition is ISectionDefinitionContainer iscont)
    {
      var sectionGroup = new DefinitionTreeNodeViewModel(SectionGroupName, addType: typeof(LoreSectionDefinition));
      if (DefinitionVM.Sections != null)
      {
        foreach (var s in DefinitionVM.Sections)
          sectionGroup.AddChild(new DefinitionTreeNodeViewModel(s));
      }
      AddChild(sectionGroup);
    }

    if (DefinitionVM.Definition is ICollectionDefinitionContainer iccont)
    {
      var colGroup = new DefinitionTreeNodeViewModel(CollectionGroupName, addType: typeof(LoreCollectionDefinition));
      if (DefinitionVM.Sections != null)
      {
        foreach (var c in DefinitionVM.Collections)
          colGroup.AddChild(new DefinitionTreeNodeViewModel(c));
      }
      AddChild(colGroup);
    }


    if (DefinitionVM.Definition is IEmbeddedNodeDefinitionContainer iecont)
    {
      var embedGroup = new DefinitionTreeNodeViewModel(EmbeddedNodsGroupName, addType: typeof(LoreEmbeddedNodeDefinition));
      if (DefinitionVM.EmbeddedNodes != null)
      {
        foreach (var e in DefinitionVM.EmbeddedNodes)
          embedGroup.AddChild(new DefinitionTreeNodeViewModel(e));
      }
      AddChild(embedGroup);
    }

    // Handle locally defined collection definition
    if(DefinitionVM is CollectionDefinitionViewModel cdvm && cdvm.IsUsingLocalCollectionDef)
    {
      AddChild(new DefinitionTreeNodeViewModel(cdvm.ContainedTypeVM));
    }

    if (DefinitionVM.Definition is IPicklistDefinitionContainer ipcont && ipcont.HasOptions)
    {
      foreach (var p in DefinitionVM.PicklistOptions)
      {
        AddChild(new DefinitionTreeNodeViewModel(p));
      }
    }
  }

  internal void RefreshTreeNode()
  {

    // If this is a grouping node, we need to ensure this group contains the correct nodes for the corresponding contained fields on the definition model.
    // i.e. if this is a field grouping node under a type node, we need to make sure this grouping of fields isn't missing any fields on the parent or that it doesn't contain extra fields

    // Parent in this case is the preceeding node in the tree, which should hold the type or section or whatever definition.
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
            if(nodeOfName != null && Children.IndexOf(nodeOfName) != i)
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
      else if (GroupName == EmbeddedNodsGroupName)
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