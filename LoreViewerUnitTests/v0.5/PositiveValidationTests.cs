using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Settings;
using NUnit.Framework.Internal;

namespace v0_5.PositiveTests
{
  public class PositiveInheritanceTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "PositiveTestData");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    public void PositiveTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "SimpleTestFile.md"));
      _parser.Validate();

      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(0));

      Assert.That(_parser.Nodes, Has.Count.EqualTo(1));
      Assert.That(_parser.Nodes[0].Collections, Has.Count.EqualTo(1));
      Assert.That(_parser.Nodes[0].Collections[0], Has.Count.EqualTo(2));
    }
  }
}
