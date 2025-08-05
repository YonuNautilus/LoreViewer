using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet.Units;

namespace v0_7.QuantityTests
{
  internal class PositiveQuantityTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "QuantityPositiveParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      LoreViewer.Program.AddCustomAbbreviations();

      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      Assert.DoesNotThrow(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "QuantityTester1.md")));

      _settings = _parser.Settings;

      _parser.Validate();

    }

    [Test]
    [Order(1)]
    public void NoParsingErrors()
    {
      Assert.That(_parser.Nodes, Is.Not.Empty);

      Assert.That(_parser.HadFatalError, Is.False);
      Assert.That(_parser.Errors, Is.Empty);

      Assert.That(_parser.validator.ValidationResult.Errors, Is.Empty);
    }

    [Test]
    [Order(2)]
    // Note that the order of node indices is different than what appears in the markdown file. That is just how it is, for now.
    [TestCase(4, new object[] { 1, LengthUnit.Kilometer, 1 }, new object[] { 2.2, MassUnit.Pound, 2.2 }, new object[] { 3.3, AngleUnit.Radian, 3.3 } )]
    [TestCase(3, new object[] {"5'6\"", LengthUnit.Inch, 66}, new object[] { 0.05, MassUnit.Gram, 0.05 }, new object[] { 32, AngleUnit.Degree, 32 })]
    [TestCase(2, new object[] { 7, LengthUnit.Inch, 84 }, new object[] { 6, MassUnit.Ounce, 6 }, new object[] { -0.1, AngleUnit.Degree, -0.1})]
    [TestCase(1, new object[] { 2, LengthUnit.Inch, 2 }, new object[] { 99, MassUnit.Tonne, 99 }, new object[] { -22, AngleUnit.Radian, -22 })]
    [TestCase(0, new object[] { 2, LengthUnit.Millimeter, 2 }, new object[] { 99, MassUnit.Ounce, 99 }, new object[] { -22, AngleUnit.Arcsecond, -22 })]
    public void CheckAttributeValues(int index, object[] firstAttr, object[] secondAttr, object[] thirdAttr)
    {
      LoreNode n = _parser.Nodes[index] as LoreNode;

      Assert.That(n.Attributes, Has.Count.EqualTo(3));
      Assert.That(n.Attributes[0].HasValue, Is.True);
      Assert.That(n.Attributes[1].HasValue, Is.True);
      Assert.That(n.Attributes[2].HasValue, Is.True);

      Assert.That(n.Attributes[0].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[1].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[2].Value, Is.TypeOf(typeof(QuantityAttributeValue)));

      Assert.That(n.Attributes[0].Value.ValueString, Does.StartWith(firstAttr[0].ToString()));
      Assert.That((n.Attributes[0].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(firstAttr[2]));
      Assert.That((n.Attributes[0].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(firstAttr[1]));

      Assert.That(n.Attributes[1].Value.ValueString, Does.StartWith(secondAttr[0].ToString()));
      Assert.That((n.Attributes[1].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(secondAttr[2]));
      Assert.That((n.Attributes[1].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(secondAttr[1]));

      Assert.That(n.Attributes[2].Value.ValueString, Does.StartWith(thirdAttr[0].ToString()));
      Assert.That((n.Attributes[2].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(thirdAttr[2]));
      Assert.That((n.Attributes[2].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(thirdAttr[1]));
    }
  }
}
