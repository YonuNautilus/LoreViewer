using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views.DefinitionViews;

public partial class CollectionDefinitionView : DefinitionView
{
  public CollectionDefinitionView() : base() { }
  public CollectionDefinitionView(CollectionDefinitionViewModel collectionVM) : base(collectionVM) { }
}