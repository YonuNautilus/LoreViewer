using Avalonia.Controls;
using LoreViewer.ViewModels.LoreEntities;

namespace LoreViewer;

public partial class EntityEditDialog : Window
{

  public LoreEntityViewModel Entity { get; }

  public EntityEditDialog()
  {
    InitializeComponent();
  }

  public EntityEditDialog(LoreEntityViewModel e)
  {
    Entity = e;
    DataContext = Entity;
    InitializeComponent();
  }
}