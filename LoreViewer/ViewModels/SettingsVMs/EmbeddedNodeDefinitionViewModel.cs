using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class EmbeddedNodeDefinitionViewModel : LoreDefinitionViewModel
  {
    private LoreEmbeddedNodeDefinition emdDef => Definition as LoreEmbeddedNodeDefinition;
    public string TypeName { get => emdDef.entryTypeName; }

    public bool IsRequired { get => emdDef.required; set => emdDef.required = value; }

    public TypeDefinitionViewModel Type { get; set; }
    protected EmbeddedNodeDefinitionViewModel(LoreDefinitionBase definitionBase) : base(definitionBase) { Type = new TypeDefinitionViewModel(emdDef.nodeType); }
  }
}
