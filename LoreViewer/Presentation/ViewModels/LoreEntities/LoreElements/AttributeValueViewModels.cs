using DocumentFormat.OpenXml.Office2010.ExcelAc;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Color = Avalonia.Media.Color;

namespace LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements
{
  public abstract class AttributeValueViewModel : ViewModelBase
  {
    protected LoreAttributeValue m_oAttrVal;

    public string ValueString { get => m_oAttrVal.ValueString; }
    public AttributeValueViewModel(LoreAttributeValue attrVal) { m_oAttrVal = attrVal; }

    public static AttributeValueViewModel CreateValueVM(LoreAttributeValue attrVal)
    {
      switch(attrVal)
      {
        case StringAttributeValue sav:
          return new StringAttributeValueViewModel(sav);
        case ColorAttributeValue cav:
          return new ColorAttributeValueViewModel(cav);
        case PicklistAttributeValue pav:
          return new PicklistAttributeValueViewModel(pav);
        case ReferenceAttributeValue rav:
          return new ReferenceAttributeValueViewModel(rav);
        case NumberAttributeValue nav:
          return new NumberAttributeValueViewModel(nav);
        case QuantityAttributeValue qav:
          return new QuantityAttributeValueViewModel(qav);
        case DateTimeAttributeValue dtav:
          return new DateTimeAttributeViewModel(dtav);
        case DateRangeAttributeValue drav:
          return new DateRangeAttributeViewModel(drav);
      }
      return null;
    }
  }

  public class StringAttributeValueViewModel : AttributeValueViewModel
  {
    public StringAttributeValueViewModel(StringAttributeValue attrVal) : base(attrVal) { }
  }

  public class ColorAttributeValueViewModel : AttributeValueViewModel
  {
    public Color Color { get => (m_oAttrVal as ColorAttributeValue).Value.BrushColor; }
    public Color InverseColor { get => (m_oAttrVal as ColorAttributeValue).Value.InverseBrushColor; }
    
    public string ColorHex { get => (m_oAttrVal as ColorAttributeValue).Value.ColorHex; }

    public string ColorName { get => (m_oAttrVal as ColorAttributeValue).Value.Name; }
    public ColorAttributeValueViewModel(ColorAttributeValue attrVal) : base(attrVal) { }
  }

  public class PicklistAttributeValueViewModel : AttributeValueViewModel
  {
    public List<string> PicklistValues { get => m_oAttrVal.OwningAttribute.DefinitionAs<LoreFieldDefinition>().GetPicklistOptions(); }
    public PicklistAttributeValueViewModel(PicklistAttributeValue attrVal) : base(attrVal) { }
  }

  public class ReferenceAttributeValueViewModel : AttributeValueViewModel
  {
    public string ReferencedID { get => (m_oAttrVal as ReferenceAttributeValue).ValueString; }
    public string NodeName { get => (m_oAttrVal as ReferenceAttributeValue).Value.Name; }

    public LoreNode ReferencedNode { get => (m_oAttrVal as ReferenceAttributeValue).Value; }

    public string DisplayText { get => $"{NodeName} [{ReferencedID}]"; }
    public ReferenceAttributeValueViewModel(ReferenceAttributeValue attrVal) : base(attrVal) { }
  }

  public class NumberAttributeValueViewModel : AttributeValueViewModel
  {
    public string Number { get => (m_oAttrVal as NumberAttributeValue).Value.ToString(); }

    public string NumberType { get => (m_oAttrVal as NumberAttributeValue).OwningAttribute.DefinitionAs<LoreFieldDefinition>().numericType.ToString(); }

    public NumberAttributeValueViewModel(NumberAttributeValue attrVal) : base(attrVal) { }
  }

  public class QuantityAttributeValueViewModel : AttributeValueViewModel
  {
    public string UnitString { get => (m_oAttrVal as QuantityAttributeValue).Value.Unit.PluralName; }
    public string Magnitude { get => (m_oAttrVal as QuantityAttributeValue).Value.Magnitude.ToString(); }
    
    public string GetQuantityString
    {
      get => (m_oAttrVal as QuantityAttributeValue).GetQuantityString();
    }
    public QuantityAttributeValueViewModel(QuantityAttributeValue attrVal) : base(attrVal) { }
  }

  public class DateTimeAttributeViewModel : AttributeValueViewModel
  {
    public string DateTime { get => (m_oAttrVal as DateTimeAttributeValue).Value.ToString(); }

    public DateTimeAttributeViewModel(DateTimeAttributeValue attrVal) : base(attrVal) { }
  }

  public class DateRangeAttributeViewModel : AttributeValueViewModel
  {
    public string SpanText
    {
      get
      {
        var v = (m_oAttrVal as DateRangeAttributeValue).Value;
        return $"{v.StartString} | {v.EndString}";
      }
    }

    public string DurationText { get => (m_oAttrVal as DateRangeAttributeValue).Value.Duration; }
    public DateRangeAttributeViewModel(DateRangeAttributeValue attrVal) : base(attrVal) { }
  }
}
