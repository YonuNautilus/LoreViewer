using LoreViewer.LoreElements;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements.Interfaces
{
  public interface IFieldContainer
  {
    ObservableCollection<LoreAttribute> Attributes { get; }

    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);

    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
  }

  public interface ISectionContainer
  {
    ObservableCollection<LoreSection> Sections { get; }

    public bool HasSection(string sectionName) => Sections.Any(s => s.Name == sectionName);

    public LoreSection? GetSection(string sectionName) => Sections.FirstOrDefault(s => s.Name == sectionName);
  }
}
