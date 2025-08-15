using Avalonia.Controls;
using Avalonia.Interactivity;
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

  private void SaveButtonClick(object sender, RoutedEventArgs e)
  {
    //if (Entity != null) Close(Entity.GetSaveContent());
  }

  private void CancelButtonClick(object sender, RoutedEventArgs e)
  {
    Close(null);
  }
}