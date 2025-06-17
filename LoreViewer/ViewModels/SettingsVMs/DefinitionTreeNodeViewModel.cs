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

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class DefinitionTreeNodeViewModel : ReactiveObject
  {
    public DefinitionTreeNodeViewModel? Parent { get; private set; }
    public ObservableCollection<DefinitionTreeNodeViewModel> Children { get; } = new();

    public LoreDefinitionViewModel? DefinitionVM { get; }
    public string GroupName { get; } = string.Empty;
    public bool IsGroupNode { get; }

    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

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

    // Group node constructor
    public DefinitionTreeNodeViewModel(string groupName)
    {
      IsGroupNode = true;
      GroupName = groupName;
    }

    // Leaf node constructor
    public DefinitionTreeNodeViewModel(LoreDefinitionViewModel vm)
    {
      DefinitionVM = vm;
      IsGroupNode = false;

      DeleteCommand = ReactiveCommand.Create(() =>
      {
        if (Parent is DefinitionTreeNodeViewModel parentNode)
        {
          // Attempt to remove from parent model container
          if (parentNode.DefinitionVM?.Definition is IFieldDefinitionContainer fCont &&
              vm.Definition is LoreFieldDefinition fld)
          {
            fCont.fields.Remove(fld);
          }
          else if (parentNode.DefinitionVM?.Definition is ISectionDefinitionContainer sCont &&
                   vm.Definition is LoreSectionDefinition sec)
          {
            sCont.sections.Remove(sec);
          }
          else if (parentNode.DefinitionVM?.Definition is ICollectionDefinitionContainer cCont &&
                   vm.Definition is LoreCollectionDefinition col)
          {
            cCont.collections.Remove(col);
          }
          else if (parentNode.DefinitionVM?.Definition is IEmbeddedNodeDefinitionContainer eCont &&
                   vm.Definition is LoreEmbeddedNodeDefinition emb)
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
}