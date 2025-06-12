using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.DefinitionViews;

public partial class TypeDefinitionView : UserControl
{
  public TypeDefinitionView()
  {
    InitializeComponent();
  }

  public void FieldList_DoubleClicked(object sender, RoutedEventArgs e)
  {
    if (DataContext != null && DataContext is LoreDefinitionViewModel ldvm)
    {
      //ldvm.ListDoubleClicked()
    }
  }
}