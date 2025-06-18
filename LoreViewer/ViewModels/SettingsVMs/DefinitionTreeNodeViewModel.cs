using DocumentFormat.OpenXml.Spreadsheet;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

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





  private static NewNameTracker tpyeNamer = new NewNameTracker("NewType");
  private static NewNameTracker fieldNamer = new NewNameTracker("NewField");
  private static NewNameTracker sectionNamer = new NewNameTracker("NewSection");
  private static NewNameTracker collectionNamer = new NewNameTracker("NewCollection");
  private static NewNameTracker embeddedNamer = new NewNameTracker("NewEmbedded");
  private LoreSettingsViewModel _settings;
  private Type _addType;

  public DefinitionTreeNodeViewModel? Parent { get; private set; }
  public ObservableCollection<DefinitionTreeNodeViewModel> Children { get; } = new();

  public LoreDefinitionViewModel? DefinitionVM { get; }
  public string GroupName { get; } = string.Empty;
  public bool IsGroupNode { get; }

  public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
  public ReactiveCommand<Unit, Unit> AddDefinitionCommand { get; }

  public string DisplayName => IsGroupNode
      ? GroupName
      : DefinitionVM?.Name ?? "(Unnamed)";

  public bool IsInherited => DefinitionVM?.IsInherited ?? false;

  public bool? IsRequired => DefinitionVM switch
  {
    FieldDefinitionViewModel f => f.IsRequired,
    SectionDefinitionViewModel s => s.IsRequired,
    _ => null
  };

  public bool CanEditRequired => DefinitionVM switch
  {
    FieldDefinitionViewModel f => !f.IsInherited,
    SectionDefinitionViewModel s => !s.IsInherited,
    _ => false
  };

  public bool CanEditName => DefinitionVM?.CanEditName ?? false;


  public DefinitionTreeNodeViewModel()
  {

  }

  // Group node constructor
  public DefinitionTreeNodeViewModel(string groupName, LoreSettingsViewModel svm = null, Type addType = null)
  {
    IsGroupNode = true;
    GroupName = groupName;
    _settings = svm;
    _addType = addType;

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

        SettingsRefresher.Apply(LoreDefinitionViewModel.CurrentSettingsViewModel);
      });
    }
    else
    {
      AddDefinitionCommand = ReactiveCommand.Create(() =>
      {
        if (Parent?.DefinitionVM?.Definition is IFieldDefinitionContainer fCont && groupName == "Fields")
        {
          var newDef = new LoreFieldDefinition { name = DefinitionTreeNodeViewModel.fieldNamer.GetName() };
          fCont.fields.Add(newDef);
          var newVM = new FieldDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is ISectionDefinitionContainer sCont && groupName == "Sections")
        {
          var newDef = new LoreSectionDefinition { name = DefinitionTreeNodeViewModel.sectionNamer.GetName() };
          sCont.sections.Add(newDef);
          var newVM = new SectionDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is ICollectionDefinitionContainer cCont && groupName == "Collections")
        {
          var newDef = new LoreCollectionDefinition { name = DefinitionTreeNodeViewModel.collectionNamer.GetName() };
          cCont.collections.Add(newDef);
          var newVM = new CollectionDefinitionViewModel(newDef);
          AddChild(new DefinitionTreeNodeViewModel(newVM));
        }
        else if (Parent?.DefinitionVM?.Definition is IEmbeddedNodeDefinitionContainer eCont && groupName == "Embedded Nodes")
        {
          var newDef = new LoreEmbeddedNodeDefinition { name = DefinitionTreeNodeViewModel.embeddedNamer.GetName() };
          eCont.embeddedNodeDefs.Add(newDef);
          var newVM = new EmbeddedNodeDefinitionViewModel(newDef);
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

    DeleteCommand = ReactiveCommand.Create(() =>
    {
      if (Parent is DefinitionTreeNodeViewModel parentNode)
      {
        // Attempt to remove from parent model container
        if (parentNode.DefinitionVM?.Definition is IFieldDefinitionContainer fCont && vm.Definition is LoreFieldDefinition fld)
        {
          fCont.fields.Remove(fld);
        }
        else if (parentNode.DefinitionVM?.Definition is ISectionDefinitionContainer sCont && vm.Definition is LoreSectionDefinition sec)
        {
          sCont.sections.Remove(sec);
        }
        else if (parentNode.DefinitionVM?.Definition is ICollectionDefinitionContainer cCont && vm.Definition is LoreCollectionDefinition col)
        {
          cCont.collections.Remove(col);
        }
        else if (parentNode.DefinitionVM?.Definition is IEmbeddedNodeDefinitionContainer eCont && vm.Definition is LoreEmbeddedNodeDefinition emb)
        {
          eCont.embeddedNodeDefs.Remove(emb);
        }

        parentNode.RemoveChild(this);
      }
    });
  }

  public void AddChild(DefinitionTreeNodeViewModel child)
  {
    child.Parent = this;
    Children.Add(child);
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

    if (DefinitionVM is TypeDefinitionViewModel typeVM)
    {
      if (typeVM.Fields?.Any() == true)
      {
        var fieldGroup = new DefinitionTreeNodeViewModel("Fields");
        foreach (var f in typeVM.Fields)
          fieldGroup.AddChild(new DefinitionTreeNodeViewModel(f));
        AddChild(fieldGroup);
      }

      if (typeVM.Sections?.Any() == true)
      {
        var sectionGroup = new DefinitionTreeNodeViewModel("Sections");
        foreach (var s in typeVM.Sections)
          sectionGroup.AddChild(new DefinitionTreeNodeViewModel(s));
        AddChild(sectionGroup);
      }

      if (typeVM.Collections?.Any() == true)
      {
        var colGroup = new DefinitionTreeNodeViewModel("Collections");
        foreach (var c in typeVM.Collections)
          colGroup.AddChild(new DefinitionTreeNodeViewModel(c));
        AddChild(colGroup);
      }

      if (typeVM.EmbeddedNodes?.Any() == true)
      {
        var embedGroup = new DefinitionTreeNodeViewModel("Embedded Nodes");
        foreach (var e in typeVM.EmbeddedNodes)
          embedGroup.AddChild(new DefinitionTreeNodeViewModel(e));
        AddChild(embedGroup);
      }
    }
  }
}