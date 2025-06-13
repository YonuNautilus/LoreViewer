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

    public bool HasEmbeddedNodes { get => typeDef.HasNestedNodes; }
    public bool HasCollections { get => typeDef.collections != null && typeDef.collections.Count > 0; }


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


    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if (typeDef.fields != null)
        foreach (LoreFieldDefinition def in typeDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def));
    }
    private void RefreshSectionDefs()
    {
      m_cSections.Clear();
      if (typeDef.sections != null)
        foreach (LoreSectionDefinition def in typeDef.sections)
          m_cSections.Add(new SectionDefinitionViewModel(def));
    }
    private void RefreshEmbeddedDefs()
    {
      m_cEmbeddeds.Clear();
      if (typeDef.embeddedNodeDefs != null)
        foreach (LoreEmbeddedNodeDefinition def in typeDef.embeddedNodeDefs)
          m_cEmbeddeds.Add(new EmbeddedNodeDefinitionViewModel(def));
    }
    private void RefreshCollectionDefs()
    {
      m_cCollections.Clear();
      if (typeDef.collections != null)
        foreach (LoreCollectionDefinition def in typeDef.collections)
          m_cCollections.Add(new CollectionDefinitionViewModel(def));
    }

    public override void RefreshLists()
    {
      RefreshFieldDefs();
      RefreshSectionDefs();
      RefreshEmbeddedDefs();
      RefreshCollectionDefs();
    }
  }
}
