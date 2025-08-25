using Avalonia.Controls;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using LoreViewer.Presentation.ViewModels.Modes;

namespace LoreViewer.Presentation.Views;

public partial class LoreReadonlyView : UserControl
{
  public LoreReadonlyView()
  {
    InitializeComponent();
  }

  private bool ShowErrorSplitter { get => !ErrorsList.ListIsCollapsed && (DataContext as LoreModeViewModel).HasErrors; }
}