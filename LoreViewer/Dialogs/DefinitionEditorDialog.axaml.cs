using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System;

namespace LoreViewer.Dialogs;

public partial class DefinitionEditorDialog : Window
{
  public DefinitionEditorDialog()
  {
    InitializeComponent();
  }

  public DefinitionEditorDialog(DefinitionEditView v, LoreDefinitionViewModel vm)
  {
    Name = vm.Name;
    vm.SetView(this);
    this.Content = v;
    this.Title = $"Definition Editor - {vm.Name}";
  }

  public static DefinitionEditorDialog CreateDefinitionEditorDialog(LoreDefinitionViewModel vm)
  {
    switch (vm)
    {
      case TypeDefinitionViewModel typeVM:
        return new DefinitionEditorDialog(new DefinitionEditView(typeVM), typeVM);
      case FieldDefinitionViewModel fieldVM:
        return new(new DefinitionEditView(fieldVM), fieldVM);
      case SectionDefinitionViewModel sectionVM:
        return new(new DefinitionEditView(sectionVM), sectionVM);
      case EmbeddedNodeDefinitionViewModel embeddedNodeVM:
        return new(new DefinitionEditView(embeddedNodeVM), embeddedNodeVM);
      case CollectionDefinitionViewModel collectionVM:
        return new(new DefinitionEditView(collectionVM), collectionVM);
      default:
        throw new Exception($"Unkown Type {vm.GetType().Name} when attempting to create defintition edtior panel");
    }
  }
}