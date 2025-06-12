using LoreViewer.Settings;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class LoreDefinitionViewModel
  {
    public LoreDefinitionBase Definition { get; }

    public string Name { get => Definition.name; set => Definition.name = value; }

    public LoreDefinitionViewModel SelectedItem { get; set; }

    protected LoreDefinitionViewModel(LoreDefinitionBase definitionBase) { Definition = definitionBase; }

    public void ListDoubleClicked(LoreDefinitionViewModel vm)
    {

    }
  }
}
