using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using LoreViewer.Presentation.ViewModels.LoreEntities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public class LoreAttributeViewModel : LoreElementViewModel
  {
    public string FieldName { get => entity.Name; }

    public EFieldContentType FieldContentType { get => (entity as LoreAttribute).DefinitionAs<LoreFieldDefinition>().contentType; }

    public ObservableCollection<LoreAttributeViewModel> NestedAttributes { get; set; }

    public ObservableCollection<AttributeValueViewModel> AttributeValues { get; set; }

    public bool HasSingleValue { get => (entity as LoreAttribute).HasValue; }
    public bool HasMultipleValues { get => (entity as LoreAttribute).HasValues; }
    public bool HasNestedAttributes { get => (entity as LoreAttribute).IsNested; }

    public override LoreEntityViewModel GetChildVM(LoreEntity eToGet)
    {
      if (entity == eToGet) return this;

      if (HasNestedAttributes)
        foreach (LoreAttributeViewModel nested in NestedAttributes)
        {
          LoreEntityViewModel n = nested.GetChildVM(eToGet);
          if (n != null) return n;
        }

      return null;
    }

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
