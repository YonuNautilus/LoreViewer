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
    public override ObservableCollection<TypeDefinitionViewModel> Types => throw new NotImplementedException();

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
    private ObservableCollection<SectionDefinitionViewModel> m_cSections = new();
    private ObservableCollection<CollectionDefinitionViewModel> m_cCollections = new();
    private ObservableCollection<EmbeddedNodeDefinitionViewModel> m_cEmbeddeds = new();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get => m_cFields; }
    public override ObservableCollection<SectionDefinitionViewModel> Sections { get => m_cSections; }
    public override ObservableCollection<CollectionDefinitionViewModel> Collections { get => m_cCollections; }
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes { get => m_cEmbeddeds; }

    public TypeDefinitionViewModel(LoreTypeDefinition definition) : base(definition)
    {
    }


    public override void RefreshLists()
    {
      RefreshFieldDefs();
    }
  }
}
