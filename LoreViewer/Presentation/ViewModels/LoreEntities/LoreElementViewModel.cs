using LoreViewer.Domain.Entities;

namespace LoreViewer.Presentation.ViewModels.LoreEntities
{
  public class LoreElementViewModel : LoreEntityViewModel
  {
    public string SourcePath { get => (entity as LoreElement).SourcePath; }

    protected LoreElementViewModel(LoreElement le) : base(le)
    {

    }

  }
}
