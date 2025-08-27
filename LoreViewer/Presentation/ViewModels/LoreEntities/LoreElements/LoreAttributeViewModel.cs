using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class LoreAttributeViewModel : LoreElementViewModel
  {
    public string FieldName { get => entity.Name; }

    public EFieldContentType FieldContentType { get => (entity as LoreAttribute).DefinitionAs<LoreFieldDefinition>().contentType; }

    public ObservableCollection<LoreAttributeViewModel> NestedAttributes { get; set; }

    public ObservableCollection<AttributeValueViewModel> AttributeValues { get; set; }

    public LoreAttributeViewModel(LoreAttribute attr) : base(attr)
    {
      if (attr.HasAttributes)
      {
        NestedAttributes = new(attr.Attributes.Select(a => new LoreAttributeViewModel(a)));
      }
      else
      {

        if (attr.HasValue)
        {
          AttributeValues = [AttributeValueViewModel.CreateValueVM(attr.Value)];
        }
        else
        {
          AttributeValues = new(attr.Values.Select(av => AttributeValueViewModel.CreateValueVM(av)).ToList());
        }
      }
    }
  }
}
