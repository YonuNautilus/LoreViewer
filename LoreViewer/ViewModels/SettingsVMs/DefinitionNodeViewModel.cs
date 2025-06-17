using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using DynamicData.Binding;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class DefinitionNodeViewModel : ViewModelBase
  {
    public string Name { get; set; }
    public bool IsInherited { get; set; }
    public bool IsModified { get; set; }
    public bool HasErrors { get; set; }

    public ObservableCollection<DefinitionNodeViewModel> Children { get; set; }// = new ObservableCollection<DefinitionNodeViewModel>();
  }

  public abstract class ActualDefinitionNodeViewModel<TDefinitionVM> : DefinitionNodeViewModel where TDefinitionVM : LoreDefinitionViewModel
  {
    public TDefinitionVM DefinitionViewModel { get; set; }
  }

  public class DefinitionExplorerViewModel : DefinitionNodeViewModel
  {

    public DefinitionExplorerViewModel(LoreSettings settings)
    {
      var typesGroup = new GroupNodeViewModel("Types");
      typesGroup.Children = new ObservableCollection<DefinitionNodeViewModel>(
        settings.types.Select(t => new TypeDefinitionNodeViewModel(t)));

      var collectionsGroup = new GroupNodeViewModel("Collections");
      collectionsGroup.Children = new ObservableCollection<DefinitionNodeViewModel>(
        settings.collections.Select(c => new CollectionDefinitionNodeViewModel(c)));

      Children = new ObservableCollection<DefinitionNodeViewModel>([typesGroup, collectionsGroup]);
    }
  }

  public class GroupNodeViewModel : DefinitionNodeViewModel
  {
    public GroupNodeViewModel(string name)
    {
      name = name;
      Children = new ObservableCollection<DefinitionNodeViewModel>();
    }

    public GroupNodeViewModel(IFieldDefinitionContainer container) : this("Fields")
    {
      if (container.HasFields)
        foreach (LoreFieldDefinition fieldDef in container.fields)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.fields.Select(f => new FieldDefinitionNodeViewModel(f)));
    }

    public GroupNodeViewModel(ISectionDefinitionContainer container) : this("Sections")
    {
      if (container.HasSections)
        foreach (LoreSectionDefinition fieldDef in container.sections)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.sections.Select(s => new SectionDefinitionNodeViewModel(s)));
    }

    public GroupNodeViewModel(ICollectionDefinitionContainer container) : this("Collections")
    {
      if(container.HasCollections)
        foreach (LoreCollectionDefinition fieldDef in container.collections)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.collections.Select(s => new CollectionDefinitionNodeViewModel(s)));
    }

    public GroupNodeViewModel(IEmbeddedNodeDefinitionContainer container) : this("Embedded Nodes")
    {
      if(container.HasNestedNodes)
        foreach (LoreEmbeddedNodeDefinition fieldDef in container.embeddedNodeDefs)
          Children = new ObservableCollection<DefinitionNodeViewModel>(container.embeddedNodeDefs.Select(e => new EmbeddedDefinitionNodeViewModel(e)));
    }
  }

  public class TypeDefinitionNodeViewModel : ActualDefinitionNodeViewModel<TypeDefinitionViewModel>
  {
    public TypeDefinitionNodeViewModel(LoreTypeDefinition t)
    {
      DefinitionViewModel = new TypeDefinitionViewModel(t);
      var fieldsGroup = new GroupNodeViewModel(t as IFieldDefinitionContainer);
      var sectionsGroup = new GroupNodeViewModel(t as ISectionDefinitionContainer);
      var collectionGroup = new GroupNodeViewModel(t as ICollectionDefinitionContainer);
      var embeddedGroup = new GroupNodeViewModel(t as IEmbeddedNodeDefinitionContainer);
    }
  }

  public class FieldDefinitionNodeViewModel : ActualDefinitionNodeViewModel<FieldDefinitionViewModel>
  {
    public FieldDefinitionNodeViewModel(LoreFieldDefinition f)
    {
      DefinitionViewModel = new FieldDefinitionViewModel(f);
      var fieldsGroup = new GroupNodeViewModel(f as IFieldDefinitionContainer);
    }
  }

  public class SectionDefinitionNodeViewModel : ActualDefinitionNodeViewModel<SectionDefinitionViewModel>
  {
    public SectionDefinitionNodeViewModel(LoreSectionDefinition s)
    {
      DefinitionViewModel = new SectionDefinitionViewModel(s);
      var fieldsGroup = new GroupNodeViewModel(s as IFieldDefinitionContainer);
      var sectionsGroup = new GroupNodeViewModel(s as ISectionDefinitionContainer);
    }
  }

  public class CollectionDefinitionNodeViewModel : ActualDefinitionNodeViewModel<CollectionDefinitionViewModel>
  {
    public CollectionDefinitionNodeViewModel(LoreCollectionDefinition c)
    {
      DefinitionViewModel = new CollectionDefinitionViewModel(c);
    }
  }

  public class EmbeddedDefinitionNodeViewModel : ActualDefinitionNodeViewModel<EmbeddedNodeDefinitionViewModel>
  {
    public EmbeddedDefinitionNodeViewModel(LoreEmbeddedNodeDefinition e)
    {
      DefinitionViewModel = new EmbeddedNodeDefinitionViewModel(e);
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
            }
      };
    }
  }
}
