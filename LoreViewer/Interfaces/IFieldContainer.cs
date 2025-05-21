using LoreViewer.LoreNodes;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.Interfaces
{
  public interface IFieldContainer
  {
    ObservableCollection<LoreAttribute> Attributes { get; }

    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);

    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
  }
}
