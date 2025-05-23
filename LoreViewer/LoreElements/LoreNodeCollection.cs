using LoreViewer.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.LoreElements
{
  public class LoreNodeCollection : LoreNarrativeElement, IList<LoreNode>
  {
    public LoreTypeDefinition Type { get; set; }

    public LoreNodeCollection() { }
    public LoreNodeCollection(LoreTypeDefinition containedType) { Type = containedType; Name = $"Collection of {containedType.name}"; }
    public LoreNodeCollection(LoreTypeDefinition containedType, LoreCollectionDefinition colType)
    { Type = containedType; Name = colType.name; }

    public LoreNodeCollection(LoreTypeDefinition containedType, string name) { Type = containedType; Name = name; }

    private readonly List<LoreNode> _nodes = new List<LoreNode>();
    public LoreNode this[int index] { get => _nodes[index]; set => _nodes[index] = value; }

    public int Count => _nodes.Count();

    public bool IsReadOnly => false;

    public void Add(LoreNode item) => _nodes.Add(item);

    public void Clear() => _nodes.Clear();

    public bool Contains(LoreNode item) => _nodes.Contains(item);

    public void CopyTo(LoreNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

    public IEnumerator<LoreNode> GetEnumerator() => _nodes.GetEnumerator();

    public int IndexOf(LoreNode item) => _nodes.IndexOf(item);

    public void Insert(int index, LoreNode item) => _nodes.Insert(index, item);

    public bool Remove(LoreNode item) => _nodes.Remove(item);

    public void RemoveAt(int index) => _nodes.RemoveAt(index);

    IEnumerator IEnumerable.GetEnumerator() => _nodes.GetEnumerator();
  }
}
