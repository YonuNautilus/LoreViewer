using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using System.Collections.ObjectModel;

namespace LoreViewer.ViewModels.LoreEntities
{
  public class AttributeViewModel : LoreElementViewModel
  {
    public string FieldName { get => entity.Name; }

    public EFieldContentType FieldContentType { get => (entity as LoreAttribute).DefinitionAs<LoreFieldDefinition>().contentType; }

    public ObservableCollection<AttributeValueViewModel> AttributeValues { get; set; }

    public AttributeViewModel(LoreAttribute attr) { entity = attr; }
  }
}
