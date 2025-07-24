using LoreViewer.LoreElements;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace LoreViewer.ViewModels.LoreEntities
{

  public class LoreEntityViewModel : ViewModelBase
  {
    public bool IsDirty { get; set; } = false;

    internal LoreEntity entity;


    public string ID { get => entity.ID; set => entity.SetID(value); }

    public static LoreEntityViewModel CreateViewModel(LoreEntity e)
    {
      switch (e)
      {
        case LoreNode node:
          return new LoreNodeViewModel(node);
        case LoreCompositeNode compositeNode:
          return new LoreCompositeNodeViewModel(compositeNode);
        case LoreAttribute attr:
          //return new LoreAttributeViewModel(attr);
        default:
          return null;
      }
    }

    public string Name { get => $"Editing: {entity.Name}"; }

    public Dictionary<string, string> GetSaveContent()
    {
      Dictionary<string, string> saveContent = new();
      switch (this)
      {
        case LoreNodeViewModel node:
          if (node.IsDirty)
          {
            Trace.WriteLine($"STAGING FILE FOR SAVE: {node.SourcePath}");
            saveContent[node.SourcePath] = node.FileContent;
          }

          break;
        case LoreCompositeNodeViewModel compNode:
          foreach (LoreNodeViewModel node in compNode.InternalNodes)
          {
            if (node.IsDirty)
            {
              saveContent[node.SourcePath] = node.FileContent;
              Trace.WriteLine($"FROM COMPOSITE NODE, STAGING FILE FOR SAVE: {node.SourcePath}");
            }
          }
          break;
        default:
          break;
      }
      return saveContent;
    }
  }
}
