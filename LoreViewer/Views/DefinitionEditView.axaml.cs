using Avalonia.Controls;
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