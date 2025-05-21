using LoreViewer.LoreNodes;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;

namespace LoreViewer.Interfaces
{
  public interface ISectionContainer
  {
    ObservableCollection<LoreSection> Sections { get; }

    public bool HasSection(string sectionName) => Sections.Any(s => s.Name == sectionName);

    public LoreSection? GetSection(string sectionName) => Sections.FirstOrDefault(s => s.Name == sectionName);
  }
}
