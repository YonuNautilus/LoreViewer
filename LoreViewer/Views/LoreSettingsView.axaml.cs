using Avalonia.Controls;
using Avalonia.Interactivity;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views;

public partial class LoreSettingsView : UserControl
{
  public LoreSettingsView()
  {
    InitializeComponent();
  }

  public void TypeListDoubleClicked(object sender, RoutedEventArgs e)
  {
    if(DataContext != null && DataContext is LoreSettingsViewModel vm)
    {
      vm.DoubleClicked((sender as DataGrid).SelectedItem as LoreDefinitionViewModel);
    }
  }
}