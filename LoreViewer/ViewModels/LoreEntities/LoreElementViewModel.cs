using LoreViewer.LoreElements;

namespace LoreViewer.ViewModels.LoreEntities
{
  internal class LoreElementViewModel : LoreEntityViewModel
  {
    public string SourcePath { get => (entity as LoreElement).SourcePath; }

  }
}
