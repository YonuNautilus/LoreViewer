using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace LoreViewer.LoreElements
{
  public abstract class LoreAttributeValue 
  {
    public LoreAttribute OwningAttribute { get; }

    public LoreAttributeValue(LoreAttribute owningAttribute)
    {
      OwningAttribute = owningAttribute;
    }

    public abstract object Value { get; }
  }

  public class ColorAttributeValue : LoreAttributeValue
  {
    public ColorAttributeValue(string valueToParse, LoreAttribute owningAttribute) : base(owningAttribute)
    {

    }

    public override ColorValue Value { get; }

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

    public NumberAttributeValue(string numberToParse, LoreAttribute owningAttribute) : base(owningAttribute)
    {
      if(double.TryParse(numberToParse, out double num))
        DefinedNumber = num; 
    }
  }

  public class QuantityAttributeValue : LoreAttributeValue
  {
    public override Quantity Value { get; }

    public QuantityAttributeValue(string quantityToParse, LoreAttribute owningAttribute) : base(owningAttribute)
    {
      Value = UnitsNet.Uni
    }
  }
}
