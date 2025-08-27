using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Presentation.ViewModels;
using System.Drawing;

namespace v0_7.ColorsTests
{
  public class PositiveColorsTests
  {
    public static LoreViewModel _lore = new LoreViewModel(null);
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _loreRepo;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "ColorPositiveParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();
      _validator = new ValidationService();
      _loreRepo = new LoreRepository();
      _valStore = new ValidationStore();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Color_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;

      _loreRepo.Set(_parser.GetParseResult());

      _valStore.Set(_validator.Validate(_loreRepo));

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

      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(RedCrayon));
      Assert.That(_valStore.Result.LoreEntityValidationStates[RedCrayon], Is.EqualTo(EValidationState.Passed));

      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(RedCrayon.Attributes[0]));
      Assert.That(_valStore.Result.LoreEntityValidationStates[RedCrayon.Attributes[0]], Is.EqualTo(EValidationState.Passed));


      LoreNode UnknownCrayon = _parser.GetNodeByName("Unknown Crayon") as LoreNode;

      Assert.NotNull(UnknownCrayon);
      Assert.That(UnknownCrayon.Attributes, Has.Count.EqualTo(1));
      Assert.That(UnknownCrayon.Attributes[0].HasValue, Is.True);
      Assert.That(UnknownCrayon.Attributes[0].HasValues, Is.False);
      Assert.That(UnknownCrayon.Attributes[0].Value, Is.TypeOf(typeof(ColorAttributeValue)));

      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(UnknownCrayon));
      Assert.That(_valStore.Result.LoreEntityValidationStates[UnknownCrayon], Is.EqualTo(EValidationState.Passed));

      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(UnknownCrayon.Attributes[0]));
      Assert.That(_valStore.Result.LoreEntityValidationStates[UnknownCrayon.Attributes[0]], Is.EqualTo(EValidationState.Passed));


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
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "ColorNegativeParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();

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
