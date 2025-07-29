using DocumentFormat.OpenXml.Drawing;
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
    private const string VERIFICATION_PATTERN = @"^(((#[0-9A-Fa-f]{8})|(#[0-9A-Fa-f]{6})) )(.*)";

    const string HEX_COLOR_PATTERN = @"^((#[0-9A-Fa-f]{8})|(#[0-9A-Fa-f]{6}))(?= )";
    const string NAME_PATTERN = @"(?<=(^((#[0-9A-Fa-f]{8})|(#[0-9A-Fa-f]{6}))) )(.*)";
    static ColorConverter conv = new ColorConverter();
    public ColorAttributeValue(string valueToParse, LoreAttribute owningAttribute) : base(valueToParse, owningAttribute)
    {
      // If there is a match in the string for a hex color code, grab it, parse it.
      if (Regex.IsMatch(valueToParse, VERIFICATION_PATTERN))
      {
        string colorHexString = Regex.Match(valueToParse, HEX_COLOR_PATTERN).Value;
        string colorName = Regex.Match(valueToParse, NAME_PATTERN).Value;

        Color? c = conv.ConvertFromString(colorHexString) as Color?;
        if (!c.HasValue) throw new Exception($"COULD NOT PARSE \"{colorHexString}\" INTO A COLOR");
        Value = new ColorValue(c.Value, colorName);
      }
      else
      {
        throw new ColorCannotParseException(owningAttribute.SourcePath, owningAttribute.BlockIndex, owningAttribute.LineNumber, this);
      }
    }

    public override ColorValue Value { get; }

    /// <summary>
    /// Class wrapper for Color struct. Allows ColorAttributeValue to use covariant return on the Value property.
    /// </summary>
    public class ColorValue
    {
      public string Name { get; set; }

      public Color DefinedColor { get; set; }

      public ColorValue(Color color, string name)
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

  /// <summary>
  /// Quantity encompasses numbers with a unit, like:
  /// <list type="bullet">
  /// <item>distance</item>
  /// <item>time (timespan)</item>
  /// <item>velocity</item>
  /// <item>volume</item>
  /// <item>mass</item>
  /// <item>weight</item>
  /// </list>
  /// </summary>
  public class QuantityAttributeValue : LoreAttributeValue
  {
    public override Quantity Value { get; }

    public QuantityAttributeValue(string quantityToParse, LoreAttribute owningAttribute) : base(quantityToParse, owningAttribute)
    {
      //Value = UnitsNet.Uni
    }
  }

  public class DateTimeAttributeValue : LoreAttributeValue
  {
    public override DateValue Value { get; }

    public DateTimeAttributeValue(string dateToParse, LoreAttribute owningAttribute) : base(dateToParse, owningAttribute)
    {

    }

    /// <summary>
    /// Class wrapper for DateTime struct. Allows DateTimeAttributeValue to use covariant return on the Value property.
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

  public class DateRangeAttributeValue : LoreAttributeValue
  {
    public override DateRangeValue Value { get; }

    public DateRangeAttributeValue(string spanToParse, LoreAttribute owningAttribute) : base(spanToParse, owningAttribute)
    {
      Value = new DateRangeValue(spanToParse, this);
    }

    /// <summary>
    /// Class wrapper for a combo TimeSpan/DateTime struct. Allows DateTimeRangeAttributeValue to use covariant return on the Value property.
    /// </summary>
    public class DateRangeValue
    {
      public enum EDateValueStatus
      {
        Unknown, Date, Present, TBD
      }

      private const string PRESENT_PATTERN = @"present";
      private const string ONGOING_PATTERN = @"ongoing";
      private const string UNKNOWN_PATTERN = @"unknown";
      private const string TBD_PATTERN     = @"tbd";

      private const string VALID_END_KEYWORDS_PATTERN = @"(present)|(ongoing)|(unknown)|(tbd)";
      private const string VALID_START_KEYWORDS_PATTERN = @"(unknown)|(tbd)";

      private EDateValueStatus m_eStartDateStatus = EDateValueStatus.Unknown;
      private EDateValueStatus m_eEndDateStatus   = EDateValueStatus.Unknown;

      public EDateValueStatus StartDateStatus => m_eStartDateStatus;
      public EDateValueStatus EndDateStatus => m_eEndDateStatus;

      private DateTime m_oStartDateTime;
      private DateTime m_oEndDateTime;

      public DateTime StartDateTime => m_oStartDateTime;
      public DateTime EndDateTime => m_oEndDateTime;


      public TimeSpan? TimeSpan
      {
        get
        {
          if (m_eEndDateStatus == EDateValueStatus.Unknown || m_eEndDateStatus == EDateValueStatus.TBD || m_eStartDateStatus == EDateValueStatus.TBD || m_eStartDateStatus == EDateValueStatus.Unknown)
            return null;
          if (m_eEndDateStatus == EDateValueStatus.Present && m_eStartDateStatus == EDateValueStatus.Date)
            return DateTime.Now - m_oStartDateTime;
          return m_oEndDateTime - m_oStartDateTime;
        }
      }

      public string Duration
      {
        get
        {
          if (m_eStartDateStatus == EDateValueStatus.Date && m_eEndDateStatus == EDateValueStatus.Date && TimeSpan.HasValue)
            return TimeSpan.Value.ToString();
          else if (m_eStartDateStatus == EDateValueStatus.Date && m_eEndDateStatus == EDateValueStatus.Present)
            return (DateTime.Now - m_oStartDateTime).ToString() + " +";
          else
            return "Indeterminate";
        }
      }

      /// <summary>
      /// Start and end date/times must be separated by a pipe character '|'
      /// <br/>
      /// <br/>
      /// <list type="bullet">
      /// <listheader>Special Keywords</listheader>
      /// <item><c>unknown</c>: (start or end) - means the start or end time is unkown.</item>
      /// <item><c>present</c>: (end) - the date range is ongoing</item>
      /// <item><c>ongoing</c>: (end) - synonym for <c>present</c></item>
      /// <item><c>TBD</c>: (start/end) - 'to be determined'</item>
      /// </list>
      /// </summary>
      /// <param name="spanToParse"></param>
      public DateRangeValue(string spanToParse, DateRangeAttributeValue owner)
      {

        if (!spanToParse.Contains('|'))
          throw new DateRangeNoPipeCharacterException(owner);

        string[] separatedElements = spanToParse.Split('|');

        if (separatedElements.Length > 2)
          throw new DateRangeTooManyPipeCharactersException(owner);

        separatedElements[0] = separatedElements[0].Trim();
        separatedElements[1] = separatedElements[1].Trim();

        // Then check if either is not parsable to datetime, if not, see if it is using a keyword. If NOT, then throw error
        if (!DateTime.TryParse(separatedElements[0], out var parsedStartDate))
        {
          if (!Regex.IsMatch(separatedElements[0], VALID_START_KEYWORDS_PATTERN, RegexOptions.IgnoreCase))
            throw new DateRangeCannotParseStartDateException(owner);
          else if (Regex.IsMatch(separatedElements[0], UNKNOWN_PATTERN, RegexOptions.IgnoreCase))
            m_eStartDateStatus = EDateValueStatus.Unknown;
          else if (Regex.IsMatch(separatedElements[0], TBD_PATTERN, RegexOptions.IgnoreCase))
            m_eStartDateStatus = EDateValueStatus.TBD;
          else
            Trace.WriteLine($"Could not parse start Date Range keyword? {separatedElements[0]}");
        }
        else
        {
          m_oStartDateTime = parsedStartDate;
          m_eStartDateStatus = EDateValueStatus.Date;
        }

        if (!DateTime.TryParse(separatedElements[1], out var parsedEndDate))
        {
          if (!Regex.IsMatch(separatedElements[1], VALID_END_KEYWORDS_PATTERN, RegexOptions.IgnoreCase))
            throw new DateRangeCannotParseEndDateException(owner);
          else if (Regex.IsMatch(separatedElements[1], UNKNOWN_PATTERN, RegexOptions.IgnoreCase))
            m_eEndDateStatus = EDateValueStatus.Unknown;
          else if (Regex.IsMatch(separatedElements[1], TBD_PATTERN, RegexOptions.IgnoreCase))
            m_eEndDateStatus = EDateValueStatus.TBD;
          else if (Regex.IsMatch(separatedElements[1], ONGOING_PATTERN, RegexOptions.IgnoreCase) || Regex.IsMatch(separatedElements[1], PRESENT_PATTERN, RegexOptions.IgnoreCase))
          {
            m_eEndDateStatus = EDateValueStatus.Present;
            m_oEndDateTime = DateTime.Now;
          }
          else
            Trace.WriteLine($"Could not parse end Date Range keyword? {separatedElements[1]}");
        }
        else
        {
          m_oEndDateTime = parsedEndDate;
          m_eEndDateStatus = EDateValueStatus.Date;
        }

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
