using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreNodes
{
  public class LoreAttribute
  {
    public string? Value { get; set; } // Simple "key: value" entry, like a single date or name
    public List<string>? Values { get; set; } // "key: List<value>" entry, like alias for a character
    public Dictionary<string, string>? NestedValues { get; set; } // compusite attributes, like a start/end date

    public int SourceIndex { get; set; }

    public bool HasValue => Value != null;
    public bool IsList => Values != null;
    public bool IsNested => NestedValues != null;

    public void Append(string newValue)
    {
      if (!IsList)
      {
        Values = new List<string> { Value };
        Value = null;
      }

      Values.Add(newValue);
    }

    public void Append(IEnumerable<string> values) { foreach (string newValue in values) Append(newValue); }

    public void Append(LoreAttribute newAttribute)
    {
      if (newAttribute.IsList) { Append(newAttribute.Values); }
      else { Append(newAttribute.Value); }
    }
  }
}
