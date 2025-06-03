using LoreViewer.LoreElements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using static System.Collections.Specialized.BitVector32;

namespace LoreViewer.Settings.Interfaces
{
  public interface IFieldDefinitionContainer
  {
    List<LoreFieldDefinition> fields { get; }
    public bool HasFields => fields != null && fields.Count > 0;

    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));

    public LoreFieldDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

  }

  public interface ISectionDefinitionContainer
  {
    List<LoreSectionDefinition> sections { get; }
    public bool HasSections => sections != null && sections.Count > 0;
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
  }

  public interface ICollectionDefinitionContainer
  {
    List<LoreCollectionDefinition> collections { get; }
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => collectionName == col.name);
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
  }

  public interface ITypeDefinitionContainer
  {
    List<LoreTypeDefinition> types { get; }
    List<string> subTypeReferences { get; }
    public bool HasTypeDefinition(string typeName) => types.Any(t => typeName == t.name);
    public LoreTypeDefinition? GetTypeDefinition(string typeName) => types.FirstOrDefault(t => typeName == t.name);

  }
}
