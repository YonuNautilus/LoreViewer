using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Net.NetworkInformation;

namespace LoreViewer
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }
    
    public void AddControl(Control control)
    {
      MainGrid.Children.Add(control);
    }
  }
}