using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views.DefinitionViews
{
  public abstract class DefinitionView : UserControl
  {
    public DefinitionView() { }
    public DefinitionView(LoreDefinitionViewModel definitionViewModel)
    {
      DataContext = definitionViewModel;
      //definitionViewModel.SetView(this);
    }
  }
}
