using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Parser;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using UnitsNet;
using UnitsNet.Units;

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

    public override string Value { get => ValueString; }
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
    private NumberValue DefinedNumber { get; set; }

    public override NumberValue Value => DefinedNumber;

    public NumberAttributeValue(string numberToParse, LoreAttribute owningAttribute) : base(numberToParse, owningAttribute)
    {
      DefinedNumber = new NumberValue(numberToParse, owningAttribute.DefinitionAs<LoreFieldDefinition>(), this);
    }

    public class NumberValue
    {
      private ulong natural;
      private long integer;
      private double fractional;
      private ENumberModifier m_eNumMod = ENumberModifier.None;

      private const string NUMBER_FORMAT_PATTERN = @"^([~<>]{0,1})([-.,\d]+)+(\+|)$";

      public ENumberModifier NumMod { get => m_eNumMod; }

      public enum ENumberModifier
      {
        None, Approx, LessThan, GreaterThan, Plus
      }

      private LoreFieldDefinition owningFieldDef;

      public NumberValue(string numberToParse, LoreFieldDefinition fieldDefToRef, NumberAttributeValue owner)
      {
        owningFieldDef = fieldDefToRef;
        if (Regex.IsMatch(numberToParse.Trim(), NUMBER_FORMAT_PATTERN, RegexOptions.IgnoreCase))
        {
          Match m = Regex.Match(numberToParse.Trim(), NUMBER_FORMAT_PATTERN);

          if (!string.IsNullOrWhiteSpace(m.Groups[1].Value) && !string.IsNullOrWhiteSpace(m.Groups[3].Value))
            throw new NumberInvalidModifiersException(owner, m.Groups[1].Value, m.Groups[3].Value);

          else if (!string.IsNullOrWhiteSpace(m.Groups[1].Value))
          {
            if (m.Groups[1].Value == "~") m_eNumMod = ENumberModifier.Approx;
            else if (m.Groups[1].Value == "~") m_eNumMod = ENumberModifier.Approx;
            else if (m.Groups[1].Value == "<") m_eNumMod = ENumberModifier.LessThan;
            else if (m.Groups[1].Value == ">") m_eNumMod = ENumberModifier.LessThan;
          }

          else if (!string.IsNullOrWhiteSpace(m.Groups[3].Value))
          {
            if (m.Groups[3].Value == "+") m_eNumMod = ENumberModifier.Plus;
          }

          string num = m.Groups[2].Value;

          if (owningFieldDef.numericType == ENumericType.Natural)
          {
            if (!ulong.TryParse(num, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out natural) && !ulong.TryParse(num, out natural)) throw new NumberCannotParseIntoNumericException(owner);
          }
          else if (owningFieldDef.numericType == ENumericType.Float)
          {
            if (!double.TryParse(num, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out fractional) && !double.TryParse(num, out fractional)) throw new NumberCannotParseIntoNumericException(owner);
          }
          else if (owningFieldDef.numericType == ENumericType.Integer)
          {
            if (!long.TryParse(num, NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out integer) && !long.TryParse(num, out integer)) throw new NumberCannotParseIntoNumericException(owner);
          }
        }
        else throw new NumberCannotParseIntoNumericException(owner);
      }

      public override string ToString()
      {
        string ret = string.Empty;
        switch (owningFieldDef.numericType)
        {
          case ENumericType.Natural: ret = natural.ToString(); break;
          case ENumericType.Float: ret = fractional.ToString(); break;
          case ENumericType.Integer: ret = integer.ToString(); break;
        }

        switch (m_eNumMod)
        {
          case ENumberModifier.Approx: ret = "~" + ret; break;
          case ENumberModifier.GreaterThan: ret = ">" + ret; break;
          case ENumberModifier.LessThan: ret = "<" + ret; break;
          case ENumberModifier.Plus: ret = ret + "+"; break;
          default: break;
        }
        return ret;
      }
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
    public override QuantityAttributeValue.QuantityValue Value { get; }

    public QuantityAttributeValue(string quantityToParse, LoreAttribute owningAttribute) : base(quantityToParse, owningAttribute)
    {
      Value = new QuantityValue(this, quantityToParse);
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Holds a UnitsNet UnitInfo, and a magnitude as a double.
    /// </summary>
    public class QuantityValue
    {
      private const string REGULAR_MAGNITUDE_PATTERN = @"^((-){0,1}[\d,\.]+)";
      private const string REGULAR_UNIT_PATTERN = @"(?<=^((-){0,1}[\d\.]+)\w).+$";

      private const string FEET_INCHES_PATTERN = @"^((\d+) {0,1}('|foot|feet|ft) {0,1}){0,1}((\d{1,2}) {0,1}(""|inches|inch|in)){0,1}$";
      private const string YEARS_DAYS_HOURS_MINUTES_SECONDS_PATTERN = @"(?:(\d+)\s*y(?:ears?)?)?\s*(?:(\d+)\s*d(?:ays?)?)?\s*(?:(\d+)\s*h(?:ours?)?)?\s*(?:(\d+)\s*m(?:in(?:uts?)?)?)?\s*(?:(\d+)\s*s(?:ec(?:onds?)?)?)?\s*";


      private UnitInfo m_oUnit;
      private double m_dMagnitude;
      private QuantityInfo m_oQuantityInfo;
      private IQuantity m_oQuantity;

      public UnitInfo Unit => m_oUnit;
      public string unitText = "";
      public double Magnitude => m_oQuantity.As(m_oQuantity.Unit);

      public QuantityValue(QuantityAttributeValue owner, string quantityToParse)
      {

        // First check the field's quantity unit type, and see if we can parse a segment with units from the string (and verify it is valid
        // for the specified unit quantity type).

        // First parse the magnitude, which should be the easiest part.


        // Handle the special case of feet-inches format.
        if (owner.OwningAttribute.DefinitionAs<LoreFieldDefinition>().quantityUnitType == EQuantityUnitType.Length &&
          Regex.IsMatch(quantityToParse, FEET_INCHES_PATTERN, RegexOptions.IgnoreCase))
        {
          Match feetAndInchesMatch = Regex.Match(quantityToParse, FEET_INCHES_PATTERN);
          var feet = feetAndInchesMatch.Groups[2].Value;
          var inches = feetAndInchesMatch.Groups[5].Value;

          double.TryParse(feet, out double dFeet);
          double.TryParse(inches, out double dInches);

          m_oQuantity = Length.FromFeetInches(dFeet, dInches);
          m_dMagnitude = Length.FromFeetInches(dFeet, dInches).Value;

          m_oUnit = Quantity.GetUnitInfo(LengthUnit.Inch);
        }

        else if(owner.OwningAttribute.DefinitionAs<LoreFieldDefinition>().quantityUnitType == EQuantityUnitType.TimeDuration &&
          Regex.IsMatch(quantityToParse, YEARS_DAYS_HOURS_MINUTES_SECONDS_PATTERN, RegexOptions.IgnoreCase))
        {
          Match m = Regex.Match(quantityToParse, YEARS_DAYS_HOURS_MINUTES_SECONDS_PATTERN, RegexOptions.IgnoreCase);
          int years, days, hours, minutes, seconds;
          int.TryParse(m.Groups[1].Value, out years);
          int.TryParse(m.Groups[2].Value, out days);
          int.TryParse(m.Groups[3].Value, out hours);
          int.TryParse(m.Groups[4].Value, out minutes);
          int.TryParse(m.Groups[5].Value, out seconds);

          Duration d = Duration.From(seconds, DurationUnit.Second);
          d += Duration.From(minutes, DurationUnit.Minute);
          d += Duration.From(hours, DurationUnit.Hour);
          d += Duration.From(days, DurationUnit.Day);
          d += Duration.From(years, DurationUnit.JulianYear);

          m_oQuantity = d;
          m_dMagnitude = d.Value;
          m_oUnit = Quantity.GetUnitInfo(d.Unit);
        }

        else
        {
          var magnitude = Regex.Match(quantityToParse, REGULAR_MAGNITUDE_PATTERN).Value;
          var units = Regex.Match(quantityToParse, REGULAR_UNIT_PATTERN).Value;

          units = quantityToParse.Substring(magnitude.Length).Trim();
          unitText = units;

          try
          {
#if USE_QUANTITY_REFLECTION
            Type unitType = Type.GetType($"UnitsNet.{owner.OwningAttribute.DefinitionAs<LoreFieldDefinition>().quantityUnitType}, UnitsNet");

            MethodInfo parseMethod = unitType.GetMethod("Parse", new[] {typeof(string)});

            object parsed = parseMethod.Invoke(null, new object[] { quantityToParse.Trim() });

            m_oQuantity = parsed;
#else

            switch (owner.OwningAttribute.DefinitionAs<LoreFieldDefinition>().quantityUnitType)
            {
              case EQuantityUnitType.Length:
                LengthUnit lu = UnitParser.Default.Parse<LengthUnit>(units);
                Length l = Length.Parse(quantityToParse);
                m_oQuantity = l;
                m_dMagnitude = l.Value;
                m_oUnit = Quantity.GetUnitInfo(lu);
                break;
              case EQuantityUnitType.Mass:
                MassUnit mu = UnitParser.Default.Parse<MassUnit>(units);
                Mass m = Mass.Parse(quantityToParse);
                m_oQuantity = m;
                m_dMagnitude = m.Value;
                m_oUnit = Quantity.GetUnitInfo(mu);
                break;
              case EQuantityUnitType.Angle:
                AngleUnit au = UnitParser.Default.Parse<AngleUnit>(units);
                Angle a = Angle.Parse(quantityToParse);
                m_oQuantity = a;
                m_dMagnitude = a.Value;
                m_oUnit = Quantity.GetUnitInfo(au);
                break;
              case EQuantityUnitType.Velocity:
                SpeedUnit su = UnitParser.Default.Parse<SpeedUnit>(units);
                Speed s = Speed.Parse(quantityToParse);
                m_oQuantity = s;
                m_dMagnitude = s.Value;
                m_oUnit = Quantity.GetUnitInfo(su);
                break;
              case EQuantityUnitType.Acceleration:
                AccelerationUnit acu = UnitParser.Default.Parse<AccelerationUnit>(units);
                Acceleration ac = Acceleration.Parse(quantityToParse);
                m_oQuantity = ac;
                m_dMagnitude = ac.Value;
                m_oUnit = Quantity.GetUnitInfo(acu);
                break;
              case EQuantityUnitType.TimeDuration:
                DurationUnit du = UnitParser.Default.Parse<DurationUnit>(units);
                Duration d = Duration.Parse(quantityToParse);
                m_oQuantity = d;
                m_dMagnitude = d.Value;
                m_oUnit = Quantity.GetUnitInfo(du);
                break;
              case EQuantityUnitType.Temperature:
                TemperatureUnit tu = UnitParser.Default.Parse<TemperatureUnit>(units);
                Temperature t = Temperature.Parse(quantityToParse);
                m_oQuantity = t;
                m_dMagnitude = t.Value;
                m_oUnit = Quantity.GetUnitInfo(tu);
                break;
              case EQuantityUnitType.Pressure:
                PressureUnit pu = UnitParser.Default.Parse<PressureUnit>(units);
                Pressure p = Pressure.Parse(quantityToParse);
                m_oQuantity = p;
                m_dMagnitude = p.Value;
                m_oUnit = Quantity.GetUnitInfo(pu);
                break;
              default:
                throw new QuantityUnitTypeUnsupportException(owner, owner.OwningAttribute.DefinitionAs<LoreFieldDefinition>().quantityUnitType);
            }
#endif
          }
          catch (UnitsNet.UnitNotFoundException e)
          {
            throw new QuantityUnknownUnitException(owner, unitText, e);
          }
          catch (QuantityCannotParseException qe)
          {
            throw qe;
          }
          catch (Exception ex)
          {
            throw new QuantityCannotParseException(owner, $"Could not parse {quantityToParse} into a quantity");
          }
        }
      }

      public override string ToString() => m_oQuantityInfo?.ToString();
    }
  }

  public enum EDateValueStatus
  {
    Unknown, Date, Present, TBD
  }

  public class DateTimeAttributeValue : LoreAttributeValue
  {
    public override DateValue Value { get; }

    public DateTimeAttributeValue(string dateToParse, LoreAttribute owningAttribute) : base(dateToParse, owningAttribute)
    {
      Value = new DateValue(dateToParse, this);
    }

    /// <summary>
    /// Class wrapper for DateTime struct. Allows DateTimeAttributeValue to use covariant return on the Value property.
    /// Only allows Present, TBD, and Unknown as keywords
    /// </summary>
    public class DateValue
    {
      private const string VALID_KEYWORDS_PATTERN = @"(unknown)|(tbd)|(present)";
      private const string PRESENT_PATTERN = @"present";
      private const string UNKNOWN_PATTERN = @"unknown";
      private const string TBD_PATTERN = @"tbd";

      private DateTime m_oDateTime;
      private EDateValueStatus m_eDateStatus = EDateValueStatus.Unknown;

      public DateTime? Date
      {
        get
        {
          if (IsDateValid) return m_oDateTime;
          else if (IsPresent) return DateTime.Today;
          else return null;
        }
      }

      public bool IsTBD => m_eDateStatus == EDateValueStatus.TBD;
      public bool IsUnknown => m_eDateStatus == EDateValueStatus.Unknown;
      public bool IsPresent => m_eDateStatus == EDateValueStatus.Present;
      public bool IsDateValid => m_eDateStatus == EDateValueStatus.Date;

      public DateValue(string dateToParse, DateTimeAttributeValue attrVal)
      {
        if (!DateTime.TryParse(dateToParse.Trim(), out var outDate))
        {
          if (!Regex.IsMatch(dateToParse.Trim(), VALID_KEYWORDS_PATTERN, RegexOptions.IgnoreCase))
            throw new DateTimeCannotParseException(attrVal);
          else if (Regex.IsMatch(dateToParse.Trim(), UNKNOWN_PATTERN, RegexOptions.IgnoreCase))
            m_eDateStatus = EDateValueStatus.Unknown;
          else if (Regex.IsMatch(dateToParse.Trim(), TBD_PATTERN, RegexOptions.IgnoreCase))
            m_eDateStatus = EDateValueStatus.TBD;
          else if (Regex.IsMatch(dateToParse.Trim(), PRESENT_PATTERN, RegexOptions.IgnoreCase))
            m_eDateStatus = EDateValueStatus.Present;
        }
        else
        {
          m_oDateTime = outDate;
          m_eDateStatus = EDateValueStatus.Date;
        }
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
      private const string TBD_PATTERN = @"tbd";

      private const string VALID_END_KEYWORDS_PATTERN = @"(present)|(ongoing)|(unknown)|(tbd)";
      private const string VALID_START_KEYWORDS_PATTERN = @"(unknown)|(tbd)";

      private EDateValueStatus m_eStartDateStatus = EDateValueStatus.Unknown;
      private EDateValueStatus m_eEndDateStatus = EDateValueStatus.Unknown;

      public bool IsStartUnknown => m_eStartDateStatus == EDateValueStatus.Unknown;
      public bool IsStartTBD => m_eStartDateStatus == EDateValueStatus.TBD;
      public bool IsStartDate => m_eStartDateStatus == EDateValueStatus.Date;
      public bool IsEndUnknown => m_eEndDateStatus == EDateValueStatus.Unknown;
      public bool IsEndTBD => m_eEndDateStatus == EDateValueStatus.TBD;
      public bool IsEndPresent => m_eEndDateStatus == EDateValueStatus.Present;
      public bool IsEndDate => m_eEndDateStatus == EDateValueStatus.Date;

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
            throw new DateRangeCannotParseStartDateException(owner);
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
            throw new DateRangeCannotParseEndDateException(owner);
        }
        else
        {
          m_oEndDateTime = parsedEndDate;
          m_eEndDateStatus = EDateValueStatus.Date;
        }

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
      if (foundNode == null)
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
