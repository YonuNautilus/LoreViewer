using Avalonia;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Wordprocessing;
using LoreViewer.Settings;
using ReactiveUI;
using System;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreDefinitionViewModel
  {
    private Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;
    public ReactiveCommand<LoreDefinitionViewModel, Unit> DeleteDefinitionCommand { get; }
    public ReactiveCommand<LoreDefinitionViewModel, Unit> EditDefinitionCommand { get; }
    public LoreDefinitionBase Definition { get; }

    public string Name { get => Definition.name; set => Definition.name = value; }

    public LoreDefinitionViewModel SelectedItem { get; set; }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase)
    {
      Definition = definitionBase;
    }

    public void DeleteDefinition(LoreDefinitionViewModel viewModel)
    {
      switch (viewModel)
      {
        case TypeDefinitionViewModel typeDefVM:
          break;
        case FieldDefinitionViewModel fieldDefVM:
          break;
        case SectionDefinitionViewModel sectionDefVM:
          break;
        case CollectionDefinitionViewModel collectionDefVM:
          break;
        case EmbeddedNodeDefinitionViewModel embeddedNodeDefVM:
          break;
        default: return;
      }
    }

    public void EditDefinition(LoreDefinitionViewModel viewModel)
    {

    }
  }
}
