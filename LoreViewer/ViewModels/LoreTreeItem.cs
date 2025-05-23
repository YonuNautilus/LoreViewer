﻿using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using ReactiveUI;
using Splat.ApplicationPerformanceMonitoring;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
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

    public LoreElement element { get; set; }

    public ObservableCollection<LoreTreeItem> Children { get; set; } = new();
    public LoreTreeItem(LoreElement e)
    {
      if (e == null) return;

      this.element = e;

      if (e is LoreNodeCollection)
        DisplayName = $"{e.Name} - {((LoreNodeCollection)e).Count} items";
      else
        DisplayName = e.Name;

      if(e is IFieldContainer ife && ife.Attributes != null && ife.Attributes.Count() > 0)
        Children.Add(CreateParentItem(ife.Attributes, "Attributes"));

      if(e is ISectionContainer isc && isc.Sections != null && isc.Sections.Count() > 0)
        Children.Add(CreateParentItem(isc.Sections, "Sections"));

      if (e is LoreNode)
      {
        LoreNode ln = e as LoreNode;
        if (ln.CollectionChildren != null && ln.CollectionChildren.Count() > 0)
          foreach (LoreNodeCollection childCollection in ln.CollectionChildren)
            Children.Add(new LoreTreeItem(childCollection));

        if (ln.Children != null && ln.Children.Count() > 0)
          Children.Add(CreateParentItem(ln.Children, "Nodes"));
      }

      if(e is LoreNodeCollection lnc && lnc != null && lnc.Count > 0)
        foreach(LoreElement elem in lnc)
          Children.Add(new LoreTreeItem(elem));

      if (e is LoreNarrativeElement lne)
        Summary = lne.Summary;

      if(e is LoreAttribute)
      {
        LoreAttribute la = e as LoreAttribute;

        if (la.HasNestedAttributes)
          foreach (LoreAttribute nestedAttr in la.NestedAttributes)
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
