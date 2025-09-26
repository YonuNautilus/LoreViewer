using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LoreViewer.Domain.Entities;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;

namespace LoreViewer.Presentation.ViewModels.LoreEntities
{
  public abstract class LoreEntityViewModel : ViewModelBase
  {
    public bool IsDirty { get; set; } = false;

    internal LoreEntity entity;

    internal LoreEntity trueEntity;

    public string ID { get => entity.ID; set => entity.SetID(value); }

    protected LoreEntityViewModel(LoreEntity le)
    {
      entity = le;
    }

    public static LoreEntityViewModel CreateViewModel(LoreEntity e)
    {
      switch (e)
      {
        case LoreNode node:
          return new LoreNodeViewModel(node);
        case LoreCollection col:
          return new LoreCollectionViewModel(col);
        case LoreAttribute attr:
          return new LoreAttributeViewModel(attr);
        default:
          return null;
      }
    }

    public string Name { get => $"Editing: {entity.Name}"; }

    public string DisplayName { get => entity.Name; }

    public virtual ObservableCollection<LoreEntityViewModel> ShallowChildren { get; set; } = new();

    public abstract LoreEntityViewModel GetChildVM(LoreEntity eToGet);


    //public Dictionary<string, string> GetSaveContent()
    //{
    //  Dictionary<string, string> saveContent = new();
    //  switch (this)
    //  {
    //    case LoreNodeViewModel node:
    //      if (node.IsDirty)
    //      {
    //        Trace.WriteLine($"STAGING FILE FOR SAVE: {node.SourcePath}");
    //        saveContent[node.SourcePath] = node.FileContent;
    //      }

    //      break;
    //    case LoreCompositeNodeViewModel compNode:
    //      foreach (LoreNodeViewModel node in compNode.InternalNodes)
    //      {
    //        if (node.IsDirty)
    //        {
    //          saveContent[node.SourcePath] = node.FileContent;
    //          Trace.WriteLine($"FROM COMPOSITE NODE, STAGING FILE FOR SAVE: {node.SourcePath}");
    //        }
    //      }
    //      break;
    //    default:
    //      break;
    //  }
    //  return saveContent;
    //}
  }
}
