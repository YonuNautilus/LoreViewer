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
    List<LoreAttributeDefinition> fields { get; }

    public bool HasFieldDefinition(string fieldName) => fields.Any(f => fieldName.Contains(f.name));

    public LoreAttributeDefinition? GetFieldDefinition(string fieldName) => fields.FirstOrDefault(f => f.name == fieldName);

  }

  public interface ISectionDefinitionContainer
  {
    List<LoreSectionDefinition> sections { get; }
    public bool HasSectionDefinition(string sectionName) => sections.Any(sec => sectionName.Contains(sec.name));
    public LoreSectionDefinition? GetSectionDefinition(string sectionName) => sections.FirstOrDefault(s => s.name == sectionName);
  }

  public interface ICollectionDefinitionContainer
  {
    List<LoreCollectionDefinition> collections { get; }
    public bool HasCollectionDefinition(string collectionName) => collections.Any(col => collectionName.Contains(col.name));
    public LoreCollectionDefinition? GetCollectionDefinition(string collectionName) => collections.FirstOrDefault(c => c.name == collectionName);
  }
}
