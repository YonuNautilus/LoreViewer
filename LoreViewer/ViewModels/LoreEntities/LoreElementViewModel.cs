using LoreViewer.LoreElements;

namespace LoreViewer.ViewModels.LoreEntities
{
  public class LoreElementViewModel : LoreEntityViewModel
  {
    public string SourcePath { get => (entity as LoreElement).SourcePath; }

  }
}
