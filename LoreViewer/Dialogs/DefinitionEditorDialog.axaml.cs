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

  public DefinitionEditorDialog(DefinitionView v)
  {
    this.Content = v;
  }

  public static DefinitionEditorDialog CreateDefinitionEditorDialog(LoreDefinitionViewModel vm)
  {
    switch (vm)
    {
      case TypeDefinitionViewModel typeVM:
        var ret = new DefinitionEditorDialog(new TypeDefinitionView(typeVM));
        typeVM.SetView(ret);
        return ret;
        break;
      case FieldDefinitionViewModel fieldVM:
        return new(new FieldDefinitionView(fieldVM));
        break;
      case SectionDefinitionViewModel sectionVM:
        return new(new SectionDefinitionView(sectionVM));
        break;
      case EmbeddedNodeDefinitionViewModel embeddedNodeVM:
        return new(new EmbeddedNodeDefinitionView(embeddedNodeVM));
        break;
      case CollectionDefinitionViewModel collectionVM:
        return new(new CollectionDefinitionView(collectionVM));
        break;
      default: return null;
    }
  }
}