using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using System.ComponentModel.DataAnnotations;
using UnitsNet.Units;

namespace v0_7.QuantityTests
{
  internal class PositiveQuantityTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "QuantityPositiveParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      LoreViewer.Program.AddCustomAbbreviations();

      _parser = new ParserService();
      _parser = new ParserService();
      _validator = new ValidationService();
      _valStore = new ValidationStore();
      _repository = new LoreRepository();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      Assert.DoesNotThrow(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "QuantityTester1.md")));

      _settings = _parser.Settings;

      _repository.Set(_parser.GetParseResult());

      _valStore.Set(_validator.Validate(_repository));

    }

    [Test]
    [Order(1)]
    public void NoParsingErrors()
    {
      Assert.That(_parser.Nodes, Is.Not.Empty);

      Assert.That(_parser.HadFatalError, Is.False);
      Assert.That(_parser.Errors, Is.Empty);

      Assert.That(_valStore.Result.Errors, Is.Empty);
    }

    [Test]
    [Order(2)]
    // Note that the order of node indices is different than what appears in the markdown file. That is just how it is, for now.
    [TestCase(4, new object[] { 1, LengthUnit.Kilometer, 1 }, new object[] { 2.2, MassUnit.Pound, 2.2 }, new object[] { 3.3, AngleUnit.Radian, 3.3 }, new object[] { "1,000", SpeedUnit.MeterPerSecond, 1000 }, new object[] { 34, PressureUnit.PoundForcePerSquareInch, 34 }, new object[] { 60, TemperatureUnit.DegreeFahrenheit, 60 }, new object[] { 10, DurationUnit.Second, 864000 })]
    [TestCase(3, new object[] { "5'6\"", LengthUnit.Inch, 66 }, new object[] { 0.05, MassUnit.Gram, 0.05 }, new object[] { 32, AngleUnit.Degree, 32 }, new object[] { 66, SpeedUnit.KilometerPerHour, 66 }, new object[] { -6, PressureUnit.NewtonPerSquareMeter, -6 }, new object[] { -3, TemperatureUnit.Kelvin, -3 }, new object[] { 10, DurationUnit.Second, 315576000 })]
    [TestCase(2, new object[] { 7, LengthUnit.Inch, 84 }, new object[] { 6, MassUnit.Ounce, 6 }, new object[] { -0.1, AngleUnit.Degree, -0.1 }, new object[] { 12, SpeedUnit.InchPerSecond, 12 }, new object[] { 60.1, PressureUnit.Pascal, 60.1 }, new object[] { 20, TemperatureUnit.DegreeCelsius, 20 }, new object[] { "1y", DurationUnit.Second, 32853682 })]
    [TestCase(1, new object[] { 2, LengthUnit.Inch, 2 }, new object[] { 99, MassUnit.Tonne, 99 }, new object[] { -22, AngleUnit.Radian, -22 }, new object[] { 100, SpeedUnit.FootPerMinute, 100 }, new object[] { "1,000,000", PressureUnit.Atmosphere, 1000000 }, new object[] { 0.001, TemperatureUnit.DegreeFahrenheit, 0.001 }, new object[] { 6, DurationUnit.Second, 6 })]
    [TestCase(0, new object[] { 2, LengthUnit.Millimeter, 2 }, new object[] { 99, MassUnit.Ounce, 99 }, new object[] { -22, AngleUnit.Arcsecond, -22 }, new object[] { 22, SpeedUnit.MilePerHour, 22 }, new object[] { "-6,000", PressureUnit.NewtonPerSquareMeter, -6000 }, new object[] { "-1,100.22", TemperatureUnit.DegreeCelsius, -1100.22 }, new object[] { "4m", DurationUnit.Second, 250 })]
    public void CheckAttributeValues(int index, object[] lenAttr, object[] massAttr, object[] angleAttr, object[] speedAttr, object[] pressureAttr, object[] tempAttr, object[] durAttr)
    {
      LoreNode n = _parser.Nodes[index] as LoreNode;

      Assert.That(n.Attributes, Has.Count.EqualTo(7));
      Assert.That(n.Attributes[0].HasValue, Is.True);
      Assert.That(n.Attributes[1].HasValue, Is.True);
      Assert.That(n.Attributes[2].HasValue, Is.True);
      Assert.That(n.Attributes[3].HasValue, Is.True);
      Assert.That(n.Attributes[4].HasValue, Is.True);
      Assert.That(n.Attributes[5].HasValue, Is.True);

      Assert.That(n.Attributes[0].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[1].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[2].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[3].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[4].Value, Is.TypeOf(typeof(QuantityAttributeValue)));
      Assert.That(n.Attributes[5].Value, Is.TypeOf(typeof(QuantityAttributeValue)));

      Assert.That(n.Attributes[0].Value.ValueString, Does.StartWith(lenAttr[0].ToString()));
      Assert.That((n.Attributes[0].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(lenAttr[2]));
      Assert.That((n.Attributes[0].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(lenAttr[1]));

      Assert.That(n.Attributes[1].Value.ValueString, Does.StartWith(massAttr[0].ToString()));
      Assert.That((n.Attributes[1].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(massAttr[2]));
      Assert.That((n.Attributes[1].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(massAttr[1]));

      Assert.That(n.Attributes[2].Value.ValueString, Does.StartWith(angleAttr[0].ToString()));
      Assert.That((n.Attributes[2].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(angleAttr[2]));
      Assert.That((n.Attributes[2].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(angleAttr[1]));

      Assert.That(n.Attributes[3].Value.ValueString, Does.StartWith(speedAttr[0].ToString()));
      Assert.That((n.Attributes[3].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(speedAttr[2]));
      Assert.That((n.Attributes[3].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(speedAttr[1]));

      Assert.That(n.Attributes[4].Value.ValueString, Does.StartWith(pressureAttr[0].ToString()));
      Assert.That((n.Attributes[4].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(pressureAttr[2]));
      Assert.That((n.Attributes[4].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(pressureAttr[1]));

      Assert.That(n.Attributes[5].Value.ValueString, Does.StartWith(tempAttr[0].ToString()));
      Assert.That((n.Attributes[5].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(tempAttr[2]));
      Assert.That((n.Attributes[5].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(tempAttr[1]));

      Assert.That(n.Attributes[6].Value.ValueString, Does.StartWith(durAttr[0].ToString()));
      Assert.That((n.Attributes[6].Value as QuantityAttributeValue).Value.Magnitude, Is.EqualTo(durAttr[2]));
      Assert.That((n.Attributes[6].Value as QuantityAttributeValue).Value.Unit.Value, Is.EqualTo(durAttr[1]));
    }
  }
}
