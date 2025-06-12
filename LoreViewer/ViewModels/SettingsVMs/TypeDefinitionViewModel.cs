using Avalonia.Interactivity;
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

    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if(typeDef.fields != null)
        foreach (LoreFieldDefinition def in typeDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def));
    }


    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new();

    public ObservableCollection<FieldDefinitionViewModel> Fields { get => m_cFields; }
    public ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get; }

    public TypeDefinitionViewModel(LoreTypeDefinition definition) : base(definition)
    {
      RefreshFieldDefs();
    }
  }
}
