using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class TypeDefinitionViewModel : LoreDefinitionViewModel
  {
    private LoreTypeDefinition typeDef { get => Definition as LoreTypeDefinition; }

    public string ExtendsTypeName { get => typeDef.extends; }

    public LoreTypeDefinition ExtendsType { get => typeDef.ParentType; }


    public ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }

    public TypeDefinitionViewModel(LoreTypeDefinition definition) : base(definition) { }

  }
}
