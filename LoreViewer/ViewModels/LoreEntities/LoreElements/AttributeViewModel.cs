using LoreViewer.LoreElements;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
