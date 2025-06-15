using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer;

public partial class DefinitionEditView : UserControl
{
  public DefinitionEditView()
  {
    InitializeComponent();
  }

  public DefinitionEditView(LoreDefinitionViewModel vm) : this()
  {
    DataContext = vm;
  }
}