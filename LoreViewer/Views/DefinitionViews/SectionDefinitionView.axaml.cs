using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views.DefinitionViews;

public partial class SectionDefinitionView : DefinitionView
{
  public SectionDefinitionView() : base() { }
  public SectionDefinitionView(SectionDefinitionViewModel sectionVM) : base(sectionVM) { }
}