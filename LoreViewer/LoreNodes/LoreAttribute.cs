using LoreViewer.Settings;
using System.Collections.Generic;

namespace LoreViewer.LoreNodes
{
  public class LoreAttribute
  {
    public string? Value { get; set; } // Simple "key: value" entry, like a single date or name
    public List<string>? Values { get; set; } // "key: List<value>" entry, like alias for a character
    public Dictionary<string, LoreAttribute>? NestedAttributes { get; set; } // compusite attributes, like a start/end date

    public LoreAttributeDefinition Definition;

    public int SourceIndex { get; set; }

    public bool HasValue => Value != null;
    public bool HasValues => Values != null;
    public bool IsNested => NestedAttributes != null;

    public void Append(string newValue)
    {
      if (!HasValues)
      {
        Values = new List<string> { Value };
        Value = null;
      }

      Values.Add(newValue);
    }

    public void Append(IEnumerable<string> values) { foreach (string newValue in values) Append(newValue); }

    public void Append(LoreAttribute newAttribute)
    {
      if (newAttribute.HasValues) { Append(newAttribute.Values); }
      else { Append(newAttribute.Value); }
    }
  }
}
