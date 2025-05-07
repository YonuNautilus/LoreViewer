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
    public bool IsList => Value != null;
    public bool IsNested => NestedValues != null;
  }
}
