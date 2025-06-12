using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class CollectionDefinitionViewModel : LoreDefinitionViewModel
  {
    private LoreCollectionDefinition colDef { get => Definition as LoreCollectionDefinition; }

    public string ContainedTypeName { get => colDef.entryTypeName; }

    public LoreDefinitionBase ContainedType { get => colDef.ContainedType; }
    public CollectionDefinitionViewModel(LoreCollectionDefinition definition) : base(definition) { }
  }
}
