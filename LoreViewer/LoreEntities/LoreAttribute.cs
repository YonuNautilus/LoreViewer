using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements
{
  public class LoreAttribute : LoreElement, IFieldContainer
  {
    public override LoreDefinitionBase Definition { get => _definition as LoreFieldDefinition; set { _definition = value as LoreFieldDefinition; } }
    private LoreFieldDefinition _definition;

    #region IFieldContainer Implementation
    public ObservableCollection<LoreAttribute> Attributes { get; } = new ObservableCollection<LoreAttribute>();
    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);
    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
    public bool HasAttributes => Attributes.Any();
    #endregion

    public string? Value { get; set; } // Simple "key: value" entry, like a single date or name
    public List<string>? Values { get; set; } // "key: List<value>" entry, like alias for a character

    public LoreAttribute(string name, LoreFieldDefinition definition) : base(name, definition) { }

    public int SourceIndex { get; set; }

    public bool HasValue => Value != null && !string.IsNullOrWhiteSpace(Value);
    public bool HasValues => Values != null && Values.Count > 0;
    public bool IsNested => Attributes.Count() > 1;

    public void Append(string newValue)
    {
      if (!HasValues)
      {
        Values = new List<string>();
        if (HasValue)
        {
          Values.Add(Value);
          Value = null;
        }
      }

      Values.Add(newValue);
    }

    public void Append(IEnumerable<string> values)
    {
      if(values.Count() > 1 || HasValues)
        foreach (string newValue in values) Append(newValue);
      else if(values.Count() == 1)
      {
        Values = null;
        Value = values.ToArray()[0];
      }

    }

    public void Append(LoreAttribute newAttribute)
    {
      if (newAttribute.HasValues) { Append(newAttribute.Values); }
      else { Append(newAttribute.Value); }
    }

    public override string ToString()
    {
      return Name;
    }
  }
}
