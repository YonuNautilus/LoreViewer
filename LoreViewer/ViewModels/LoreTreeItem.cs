using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.ViewModels
{
  /// <summary>
  /// A ViewModel for LoreElements to be displayed
  /// </summary>
  public class LoreTreeItem : ReactiveObject
  {
    public string DisplayName { get; set; }
    public string Summary { get; set; } = string.Empty;

    public string CurrentNodeInfoText
    {
      get
      {
        switch (element)
        {
          case LoreNarrativeElement lne:
            return lne.Summary;
          case null:
            return DisplayName;
          default:
            return element.Name;
        }
      }
    }

    public LoreEntity element { get; set; }

    public ObservableCollection<LoreTreeItem> Children { get; set; } = new();
    public LoreTreeItem(LoreEntity e)
    {
      if (e == null) return;

      this.element = e;

      if (e is LoreCollection)
        DisplayName = $"{e.Name} - {((LoreCollection)e).Count} items";
      else
        DisplayName = e.Name;

      if(e is IAttributeContainer ife && ife.HasAttributes)
        Children.Add(CreateParentItem(ife.Attributes, "Attributes"));

      if(e is ISectionContainer isc && isc.HasSections)
        Children.Add(CreateParentItem(isc.Sections, "Sections"));

      if (e is ILoreNode)
      {
        ILoreNode ln = e as ILoreNode;
        if (ln.Collections != null && ln.Collections.Count() > 0)
          foreach (LoreCollection childCollection in ln.Collections)
            Children.Add(new LoreTreeItem(childCollection));

        if(ln is LoreNode)
          if (ln.Nodes != null && ln.Nodes.Count() > 0)
            //Children.AddNode(CreateParentItem(ln.Nodes, "Nodes"));
            foreach(LoreNode node in ln.Nodes)
              Children.Add(new LoreTreeItem(node));
      }

      if(e is LoreCollection lnc && lnc != null && lnc.Count > 0)
        foreach(LoreElement elem in lnc)
          Children.Add(new LoreTreeItem(elem));

      if (e is LoreNarrativeElement lne)
        Summary = lne.Summary;

      if(e is LoreAttribute)
      {
        LoreAttribute la = e as LoreAttribute;

        if (la.HasAttributes)
          foreach (LoreAttribute nestedAttr in la.Attributes)
            Children.Add(new LoreTreeItem(nestedAttr));

        if (la.HasValue && !la.HasValues)
          DisplayName = DisplayName += $": {la.Value}";
        else if (la.HasValues)
          foreach (string v in la.Values) Children.Add(new LoreTreeItem(v));
      }
    }

    public LoreTreeItem(string name) { DisplayName = name.Trim(); }

    private LoreTreeItem? CreateParentItem(IEnumerable<LoreElement> elems, string parentName)
    {
      if (elems == null || elems.Count() < 1) return null;

      LoreTreeItem lti = new LoreTreeItem(parentName);
      lti.Children = new ObservableCollection<LoreTreeItem>();

      foreach(LoreElement elem in elems)
      {
        lti.Children.Add(new LoreTreeItem(elem));
      }

      return lti;
    }
  }
}
