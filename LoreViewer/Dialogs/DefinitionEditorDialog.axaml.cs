using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Views.DefinitionViews;
using LoreViewer.ViewModels.SettingsVMs;
using System.Runtime.CompilerServices;

namespace LoreViewer.Dialogs;

public partial class DefinitionEditorDialog : Window
{
  public DefinitionEditorDialog()
  {
    InitializeComponent();
  }

  public DefinitionEditorDialog(DefinitionView v, LoreDefinitionViewModel vm)
  {
    Name = vm.Name;
    vm.SetView(this);
    this.Content = v;
  }

  public static DefinitionEditorDialog CreateDefinitionEditorDialog(LoreDefinitionViewModel vm)
  {
    switch (vm)
    {
      case TypeDefinitionViewModel typeVM:
        return new DefinitionEditorDialog(new TypeDefinitionView(typeVM), typeVM);
        break;
      case FieldDefinitionViewModel fieldVM:
        return new(new FieldDefinitionView(fieldVM), fieldVM);
        break;
      case SectionDefinitionViewModel sectionVM:
        return new(new SectionDefinitionView(sectionVM), sectionVM);
        break;
      case EmbeddedNodeDefinitionViewModel embeddedNodeVM:
        return new(new EmbeddedNodeDefinitionView(embeddedNodeVM), embeddedNodeVM);
        break;
      case CollectionDefinitionViewModel collectionVM:
        return new(new CollectionDefinitionView(collectionVM), collectionVM);
        break;
      default: return null;
    }
  }
}