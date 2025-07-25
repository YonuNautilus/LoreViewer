using DocumentFormat.OpenXml.Bibliography;
using LoreViewer.Validation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.ColorsTests
{
  public class PositiveColorsTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "ColorPositiveParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Color_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    public void NoCrayonParsingErrors()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(3));

      Assert.That(_parser.Errors, Has.Count.EqualTo(0));

      LoreNode RedCrayon = _parser.GetNodeByName("Red Crayon") as LoreNode;

      Assert.NotNull(RedCrayon);
      Assert.That(RedCrayon.Attributes, Has.Count.EqualTo(1));
      Assert.That(RedCrayon.Attributes[0].HasValue, Is.True);
      Assert.That(RedCrayon.Attributes[0].HasValues, Is.False);
      Assert.That(RedCrayon.Attributes[0].Value, Is.TypeOf(typeof(ColorAttributeValue)));
      Assert.That((RedCrayon.Attributes[0].Value as ColorAttributeValue).Value.DefinedColor, Is.EqualTo(Color.Red));
      Assert.That((RedCrayon.Attributes[0].Value as ColorAttributeValue).Value.Name, Is.EqualTo("Plain Ol' Red"));

      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(RedCrayon));
      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates[RedCrayon], Is.EqualTo(EValidationState.Passed));

      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(RedCrayon.Attributes[0]));
      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates[RedCrayon.Attributes[0]], Is.EqualTo(EValidationState.Passed));


      LoreNode UnknownCrayon = _parser.GetNodeByName("Unknown Crayon") as LoreNode;

      Assert.NotNull(UnknownCrayon);
      Assert.That(UnknownCrayon.Attributes, Has.Count.EqualTo(1));
      Assert.That(UnknownCrayon.Attributes[0].HasValue, Is.True);
      Assert.That(UnknownCrayon.Attributes[0].HasValues, Is.False);
      Assert.That(UnknownCrayon.Attributes[0].Value, Is.TypeOf(typeof(ColorAttributeValue)));

      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(UnknownCrayon));
      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates[UnknownCrayon], Is.EqualTo(EValidationState.Passed));

      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(UnknownCrayon.Attributes[0]));
      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationStates[UnknownCrayon.Attributes[0]], Is.EqualTo(EValidationState.Passed));


      LoreNode PencilBox = _parser.GetNodeByName("PencilBox") as LoreNode;

      Assert.NotNull(PencilBox);
      Assert.That(PencilBox.Attributes, Has.Count.EqualTo(1));
      Assert.That(PencilBox.Attributes[0].HasValue, Is.False);
      Assert.That(PencilBox.Attributes[0].HasValues, Is.True);

      Assert.That(PencilBox.Attributes[0].Values[0], Is.TypeOf(typeof(ColorAttributeValue)));
      Assert.IsNotEmpty((PencilBox.Attributes[0].Values[0] as ColorAttributeValue).Value.Name);
      Assert.That((PencilBox.Attributes[0].Values[0] as ColorAttributeValue).Value.Name, Is.EqualTo("idk"));

      Assert.That(PencilBox.Attributes[0].Values[1], Is.TypeOf(typeof(ColorAttributeValue)));
      Assert.IsNotEmpty((PencilBox.Attributes[0].Values[1] as ColorAttributeValue).Value.Name);
      Assert.That((PencilBox.Attributes[0].Values[1] as ColorAttributeValue).Value.Name, Is.EqualTo("Invisible"));
    }
  }
  public class NegativeColorsTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "ColorNegativeParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Color_Settings.yaml"));


      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    public void CrayonParsingErrors()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(0));

      Assert.That(_parser.Errors, Has.Count.EqualTo(8));
    }
  }
}
