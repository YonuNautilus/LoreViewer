using LoreViewer.Domain.Entities;

namespace LoreViewer.Presentation.ViewModels.LoreEntities
{
  public abstract class LoreElementViewModel : LoreEntityViewModel
  {
    public string SourcePath { get => (entity as LoreElement).Provenance[0].SourceFilePath; }

    protected LoreElementViewModel(LoreElement le) : base(le)
    {

    }

  }
}
