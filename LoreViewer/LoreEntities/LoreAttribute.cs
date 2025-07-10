using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LoreViewer.LoreElements
{
  /// <summary>
  /// An Attribute a single value for a lore element's field (i.e. name, species, magic type, biome).
  /// <para/>
  /// Definition Type: LoreFieldDefinition
  /// <br/>
  /// <br/>
  /// Attributes can contain:
  /// <list type="bullet">
  /// <item>Attributes</item>
  /// </list>
  /// </summary>
  public class LoreAttribute : LoreElement, IAttributeContainer
  {
    public override LoreDefinitionBase Definition { get => _definition as LoreFieldDefinition; set { _definition = value as LoreFieldDefinition; } }
    private LoreFieldDefinition _definition;

    #region IFieldContainer Implementation
    public ObservableCollection<LoreAttribute> Attributes { get; } = new ObservableCollection<LoreAttribute>();
    public bool HasAttribute(string attrName) => Attributes.Any(a => a.Name == attrName);
    public LoreAttribute? GetAttribute(string attrName) => Attributes.FirstOrDefault(a => a.Name == attrName);
    public bool HasAttributes => Attributes.Any();
    #endregion

    public LoreAttributeValue? Value { get; set; } // Simple "key: value" entry, like a single date or name
    public List<LoreAttributeValue>? Values { get; set; } // "key: List<value>" entry, like alias for a character

    public LoreAttribute(string name, LoreFieldDefinition definition) : base(name, definition) { }
    public LoreAttribute(string name, LoreFieldDefinition definition, string filePath, int blockIndex, int lineNumber) : base(name, definition, filePath, blockIndex, lineNumber) { }

    public int SourceIndex { get; set; }

    public bool HasValue => Value != null && !string.IsNullOrWhiteSpace(Value.ValueString);

    /// <summary>
    /// True if this LoreAttribute has multiple (nested) values
    /// </summary>
    public bool HasValues => Values != null && Values.Count > 0;
    public bool IsNested => Attributes.Count() > 1;

    public void Append(string newValue)
    {
      if (!HasValues)
      {
        Values = new List<LoreAttributeValue>();
        if (HasValue)
        {
          Values.Add(CreateNewAttributeValue(newValue));
          Value = null;
        }
      }

      Values.Add(CreateNewAttributeValue(newValue));
    }

    public void Append(IEnumerable<string> values)
    {
      if (values.Count() > 1 || HasValues)
        foreach (string newValue in values) Append(newValue);
      else if (values.Count() == 1)
      {
        Values = null;
        Value = CreateNewAttributeValue(values.ToArray()[0]);
      }

    }

    public override string ToString()
    {
      return Name;
    }

    private LoreAttributeValue CreateNewAttributeValue(string valueToParse)
    {
      switch (_definition.contentType)
      {
        case EFieldContentType.String:
          return new StringAttributeValue(valueToParse, this);
        case EFieldContentType.Color:
          return new ColorAttributeValue(valueToParse, this);
        case EFieldContentType.Number:
          return new NumberAttributeValue(valueToParse, this);
        case EFieldContentType.Quantity:
          return new QuantityAttributeValue(valueToParse, this);
        case EFieldContentType.Date:
          return new DateAttributeValue(valueToParse, this);
        case EFieldContentType.Timespan:
          return new TimeSpanAttributeValue(valueToParse, this);
        case EFieldContentType.Picklist:
          return new PicklistAttributeValue(valueToParse, this);
        case EFieldContentType.ReferenceList:
          return new ReferenceAttributeValue(valueToParse, this);
        default:
          return null;
      }
    }
  }
}
