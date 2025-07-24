using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Parser;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using UnitsNet;

namespace LoreViewer.LoreElements
{
  public abstract class LoreAttributeValue 
  {
    public LoreAttribute OwningAttribute { get; }

    private LoreAttributeValue(LoreAttribute owningAttribute)
    {
      OwningAttribute = owningAttribute;
    }

    public LoreAttributeValue(string valStr, LoreAttribute owningAttribute) : this(owningAttribute)
    {
      ValueString = valStr;
    }

    public string ValueString { get; set; }
    public abstract object Value { get; }
  }

  public class StringAttributeValue : LoreAttributeValue
  {
    public StringAttributeValue(string valStr, LoreAttribute owningAttribute) : base(valStr, owningAttribute)
    {
    }

    public override string Value { get; }
  }

  public class ColorAttributeValue : LoreAttributeValue
  {
    const string HEX_COLOR_PATTERN = "(#[0-9A-Fa-f]{8})|(#[0-9A-Fa-f]{6})";
    const string HEX_COLOR_PATTERN_W_NAME = "((#[0-9A-Fa-f]{8}$)|(#[0-9A-Fa-f]{6}$)) (.+)";
    const string HEX_COLOR_PATTERN_NO_ALPHA = "#[0-9A-Fa-f]{6}$";
    const string HEX_COLOR_PATTERN_ALPHA = "#[0-9A-Fa-f]{8}$";
    static ColorConverter conv = new ColorConverter();
    public ColorAttributeValue(string valueToParse, LoreAttribute owningAttribute) : base(valueToParse, owningAttribute)
    {
      // If there is a match in the string for a hex color code, grab it, parse it.
      if (Regex.IsMatch(valueToParse, HEX_COLOR_PATTERN))
      {
        string colorHexString = string.Empty;
        string colorName = string.Empty;

        // If the value to parse has a hex code and a name given
        if (Regex.IsMatch(valueToParse, HEX_COLOR_PATTERN_W_NAME))
        {
          // For HEX_COLOR_PATTERN_W_NAME, group 0 is the 4 byte hex code, group 1 is the 3 byte hex code, group 3 should be the name
          Match m = Regex.Match(valueToParse, HEX_COLOR_PATTERN_W_NAME);
          colorHexString = m.Groups[0].Success ? m.Groups[0].Value : m.Groups[1].Value;
          colorName = m.Groups[3].Value;
        }
        // Otherwise there is no name given, just a color code
        else
        {
          Match m = Regex.Match(valueToParse, HEX_COLOR_PATTERN);
          colorHexString = m.Groups[0].Success ? m.Groups[0].Value : m.Groups[1].Value;
        }
        
        Value = new ColorValue(conv.ConvertFromString(colorHexString) as Color?, colorName);
      }
      else
      {
        Value = new ColorValue(null, valueToParse);
        //throw new ColorCannotParseException(owningAttribute.SourcePath, owningAttribute.BlockIndex, owningAttribute.LineNumber, this);
      }
    }

    public override ColorValue Value { get; }

    /// <summary>
    /// Class wrapper for Color struct. Allows ColorAttributeValue to use covariant return on the Value property.
    /// </summary>
    public class ColorValue
    {
      public string Name { get; set; }

      public Color? DefinedColor { get; set; }

      public ColorValue(Color? color, string name)
      {
        DefinedColor = color;
        Name = name;
      }

    }
  }

  public class NumberAttributeValue : LoreAttributeValue
  {
    public double DefinedNumber { get; set; }

    public override object Value => DefinedNumber;

    public NumberAttributeValue(string numberToParse, LoreAttribute owningAttribute) : base(numberToParse, owningAttribute)
    {
      if(double.TryParse(numberToParse, out double num))
        DefinedNumber = num; 
    }
  }

  public class QuantityAttributeValue : LoreAttributeValue
  {
    public override Quantity Value { get; }

    public QuantityAttributeValue(string quantityToParse, LoreAttribute owningAttribute) : base(quantityToParse, owningAttribute)
    {
      //Value = UnitsNet.Uni
    }
  }

  public class DateAttributeValue : LoreAttributeValue
  {
    public override DateValue Value { get; }

    public DateAttributeValue(string dateToParse, LoreAttribute owningAttribute) : base(dateToParse, owningAttribute)
    {

    }

    /// <summary>
    /// Class wrapper for DateTime struct. Allows DateAttributeValue to use covariant return on the Value property.
    /// </summary>
    public class DateValue
    {
      private DateTime m_oDateTime;

      public DateValue(string dateToParse)
      {
        m_oDateTime = DateTime.Parse(dateToParse);
      }
    }
  }

  public class TimeSpanAttributeValue : LoreAttributeValue
  {
    public override TimeSpanValue Value { get; }

    public TimeSpanAttributeValue(string timespanToParse, LoreAttribute owningAttribute) : base(timespanToParse, owningAttribute)
    {

    }

    /// <summary>
    /// Wrapper class for the TimeSpan struct. Allows TimeSpanAttributeValue to use covariant return on its Value property.
    /// </summary>
    public class TimeSpanValue
    {
      public TimeSpan m_oTimeSpanValue;

      public TimeSpanValue(string timespanToParse)
      {

      }
    }
  }

  public class PicklistAttributeValue : LoreAttributeValue
  {
    public override string Value { get; }

    public PicklistAttributeValue(string value, LoreAttribute owningAttribute) : base(value, owningAttribute) { }
  }

  public class ReferenceAttributeValue : LoreAttributeValue
  {
    private ILoreNode m_oNodeValue;
    public override ILoreNode Value { get => m_oNodeValue; }

    public ReferenceAttributeValue(string value, LoreAttribute owningAttribute) : base(value, owningAttribute) { }

    public void ResolveReferenceToNode(LoreParser parser)
    {
      // First, look for the node by ID
      ILoreNode foundNode = parser.GetNodeByID(ValueString);
      // If not found by ID...
      if(foundNode == null)
      {
        Trace.WriteLine($"ReferenceAttributeValue could not find node with ID {ValueString}");
        // Try to look for the node by name
        foundNode = parser.GetNodeByName(ValueString);
      }

      if (foundNode == null)
        throw new ReflistCannotResovleException(OwningAttribute.SourcePath, OwningAttribute.BlockIndex, OwningAttribute.LineNumber, this);
      else
        m_oNodeValue = foundNode;
    }
  }
}
