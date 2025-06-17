using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DynamicData.Binding;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public abstract class DefinitionNodeViewModel : ViewModelBase
  {
    public virtual string Name { get; set; }
    public virtual bool IsInherited { get; set; }
    public virtual bool IsModified { get; set; }
    public virtual bool HasErrors { get; set; }

    public ObservableCollection<DefinitionNodeViewModel> Children { get; set; }// = new ObservableCollection<DefinitionNodeViewModel>();
  }

  public abstract class ActualDefinitionNodeViewModel<TDefinitionVM> : DefinitionNodeViewModel where TDefinitionVM : LoreDefinitionViewModel
  {
    public override string Name { get => DefinitionViewModel.Name; }
    public override bool IsInherited { get => DefinitionViewModel.IsInherited; }
    public override bool IsModified { get => DefinitionViewModel.IsModifiedFromBase; }
    public TDefinitionVM DefinitionViewModel { get; set; }

    public ReactiveCommand<Unit, Unit> DeleteDefinitionCommand { get; }

    public ActualDefinitionNodeViewModel(LoreDefinitionViewModel vm)
    {
      DefinitionViewModel = vm as TDefinitionVM;
      Children = new ObservableCollection<DefinitionNodeViewModel>();
    }
  }

  public class GroupNodeViewModel : DefinitionNodeViewModel
  {
    public override string Name { get; set; }

    private Type containedDefinitionType;



    public GroupNodeViewModel(string name)
    {
      Name = name;
      Children = new ObservableCollection<DefinitionNodeViewModel>();
    }

    public GroupNodeViewModel(string name, Type definitionType) : this(name)
    {
      containedDefinitionType = definitionType;
    }

    public GroupNodeViewModel(IEnumerable<LoreDefinitionBase> defs, string name, Type definitionType) : this(name, definitionType)
    {
      Type contained = defs.GetType().GetGenericArguments()[0];
      foreach (LoreDefinitionBase def in defs)
      {
        if (def is LoreTypeDefinition typeDef)
          Children.Add(new TypeDefinitionNodeViewModel(typeDef));
        else if (def is LoreCollectionDefinition colDef)
          Children.Add(new CollectionDefinitionNodeViewModel(colDef));
      }

    }

    public GroupNodeViewModel(IFieldDefinitionContainer container) : this("Fields", typeof(LoreFieldDefinition))
    {
      if (container.HasFields)
        foreach (LoreFieldDefinition fieldDef in container.fields)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.fields.Select(f => new FieldDefinitionNodeViewModel(f)));
    }

    public GroupNodeViewModel(ISectionDefinitionContainer container) : this("Sections", typeof(LoreFieldDefinition))
    {
      if (container.HasSections)
        foreach (LoreSectionDefinition fieldDef in container.sections)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.sections.Select(s => new SectionDefinitionNodeViewModel(s)));
    }

    public GroupNodeViewModel(ICollectionDefinitionContainer container) : this("Collections", typeof(LoreCollectionDefinition))
    {
      if (container.HasCollections)
        foreach (LoreCollectionDefinition fieldDef in container.collections)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.collections.Select(s => new CollectionDefinitionNodeViewModel(s)));
    }

    public GroupNodeViewModel(IEmbeddedNodeDefinitionContainer container) : this("Embedded Nodes", typeof(LoreEmbeddedNodeDefinition))
    {
      if (container.HasNestedNodes)
        foreach (LoreEmbeddedNodeDefinition fieldDef in container.embeddedNodeDefs)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.embeddedNodeDefs.Select(e => new EmbeddedDefinitionNodeViewModel(e)));
    }
  }

  public class TypeDefinitionNodeViewModel : ActualDefinitionNodeViewModel<TypeDefinitionViewModel>
  {
    public TypeDefinitionNodeViewModel(LoreTypeDefinition t) : base(new TypeDefinitionViewModel(t))
    {
      Children.Add(new GroupNodeViewModel(t as IFieldDefinitionContainer));
      Children.Add(new GroupNodeViewModel(t as ISectionDefinitionContainer));
      Children.Add(new GroupNodeViewModel(t as ICollectionDefinitionContainer));
      Children.Add(new GroupNodeViewModel(t as IEmbeddedNodeDefinitionContainer));
    }
  }

  public class FieldDefinitionNodeViewModel : ActualDefinitionNodeViewModel<FieldDefinitionViewModel>
  {
    public FieldDefinitionNodeViewModel(LoreFieldDefinition f) : base(new FieldDefinitionViewModel(f))
    {
      Children.Add(new GroupNodeViewModel(f as IFieldDefinitionContainer));
    }
  }

  public class SectionDefinitionNodeViewModel : ActualDefinitionNodeViewModel<SectionDefinitionViewModel>
  {
    public SectionDefinitionNodeViewModel(LoreSectionDefinition s) : base(new SectionDefinitionViewModel(s))
    {
      if (s.HasFields)
        Children.Add(new GroupNodeViewModel(s as IFieldDefinitionContainer));
      if (s.HasSections)
        Children.Add(new GroupNodeViewModel(s as ISectionDefinitionContainer));
    }
  }

  public class CollectionDefinitionNodeViewModel : ActualDefinitionNodeViewModel<CollectionDefinitionViewModel>
  {
    public CollectionDefinitionNodeViewModel(LoreCollectionDefinition c) : base(new CollectionDefinitionViewModel(c))
    {

    }
  }

  public class EmbeddedDefinitionNodeViewModel : ActualDefinitionNodeViewModel<EmbeddedNodeDefinitionViewModel>
  {
    public EmbeddedDefinitionNodeViewModel(LoreEmbeddedNodeDefinition e) : base(new EmbeddedNodeDefinitionViewModel(e))
    {
    }
  }


  public static class DefinitionTreeDataGridFactory
  {

    public static ITreeDataGridSource<DefinitionNodeViewModel> Build(IEnumerable<DefinitionNodeViewModel> rootNodes)
    {
      return new HierarchicalTreeDataGridSource<DefinitionNodeViewModel>(rootNodes)
      {
        Columns =
            {
                new HierarchicalExpanderColumn<DefinitionNodeViewModel>(
                  new TextColumn<DefinitionNodeViewModel, string>("Name", x => x.Name),
                  x => x.Children),

                new TextColumn<DefinitionNodeViewModel, string>(
                    header: "Inherited",
                    getter: x => x.IsInherited ? "Yes" : ""),

                new TextColumn<DefinitionNodeViewModel, string>(
                    header: "Modified",
                    getter: x => x.IsModified ? "*" : ""),

                new TextColumn<DefinitionNodeViewModel, string>(
                    header: "Validation",
                    getter: x => x.HasErrors ? "Error" : "OK"),

                new TemplateColumn<DefinitionNodeViewModel>(
                  header: "Actions",
                  cellTemplate: new FuncDataTemplate<DefinitionNodeViewModel>((node, _) =>
                  {
                    var panel = new StackPanel{ Orientation = Avalonia.Layout.Orientation.Horizontal, Height = 20 };

                    if(node != null)
                    {

                      // Only show delete for nodes representing actual definitions
                      if (IsActualDefinitionNode(node))
                      {
                        var deleteButton = new Button{ Content = new Image{ Source = new Bitmap(AssetLoader.Open(new Uri("avares://LoreViewer/Resources/pencil.png"))) } };
                        deleteButton.Bind(Button.CommandProperty, new Binding("DeleteDefinitionCommand") );
                        deleteButton.Bind(Button.CommandParameterProperty, new Binding(".") );
                        //deleteButton.Bind(Button.IsVisibleProperty, )
                        panel.Children.Add(deleteButton);
                      }

                      // only show add button for GROUPING nodes.
                      if(node is GroupNodeViewModel)
                      {
                        var addButton = new Button{ Content = "+" };
                        addButton.Bind(Button.CommandProperty, new Binding("AddDefinitionCommand") );
                        addButton.Bind(Button.CommandParameterProperty, new Binding(".") );
                        panel.Children.Add(addButton);
                      }
                    }
                    return panel;
                  })),

            }
      };
    }


    public static bool IsActualDefinitionNode(DefinitionNodeViewModel vm)
    {
      var currentType = vm?.GetType();

      while (currentType != null)
      {
        if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(ActualDefinitionNodeViewModel<>))
          return true;

        currentType = currentType.BaseType;
      }
      return false;
    }
  }
}
