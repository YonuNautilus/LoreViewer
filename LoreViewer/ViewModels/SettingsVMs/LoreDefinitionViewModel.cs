using Avalonia;
using Avalonia.Interactivity;
using LoreViewer.Settings;
using System;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreDefinitionViewModel
  {
    private Visual m_oView;
    public void SetView(Visual visual) => m_oView = visual;

    public LoreDefinitionBase Definition { get; }

    public string Name { get => Definition.name; set => Definition.name = value; }

    public LoreDefinitionViewModel SelectedItem { get; set; }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase) { Definition = definitionBase; }

    public void ListDoubleClicked(LoreDefinitionViewModel vm)
    {

    }
  }
}
