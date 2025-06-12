using Avalonia;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views.DefinitionViews;

public partial class EmbeddedNodeDefinitionView : DefinitionView
{
  public EmbeddedNodeDefinitionView() : base() { }
  public EmbeddedNodeDefinitionView(EmbeddedNodeDefinitionViewModel definitionViewModel) : base(definitionViewModel) { }
}