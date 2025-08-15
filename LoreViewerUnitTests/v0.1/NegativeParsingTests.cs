using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Settings;
using LoreViewer.Exceptions.LoreParsingExceptions;

namespace v0_1.NegativeTests
{
  [TestFixture]
  [TestOf(typeof(ParserService))]
  public class NegativeFieldParsingTests
  {
    LoreSettings _settings;
    ParserService _parser;

    string ErrorFilesFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "v0.1", "TestData", "NegativeTestData");

    [SetUp]
    public void SetupLoreSettings()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ErrorFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    [TestOf(typeof(ParserService))]
    [TestOf(typeof(NoTagParsingException))]
    public void UntaggedFirstHeadingTest()
    {
      string fileToTest = Path.Combine(ErrorFilesFolder, "TopHeadingUntagged.md");

      Assert.Throws<NoTagParsingException>(() =>
      {
        _parser.ParseSingleFile(fileToTest);
      });
    }

    [Test]
    [TestOf(typeof(ParserService))]
    [TestOf(typeof(FirstHeadingTagException))]
    public void FirstHeadingIsSectionTest()
    {
      string fileToTest = Path.Combine(ErrorFilesFolder, "FirstHeadingIsSection.md");

      Assert.Throws<FirstHeadingTagException>(() =>
      {
        _parser.ParseSingleFile(fileToTest);
      });
    }
  }

  [TestFixture]
  [TestOf(typeof(ParserService))]
  public class NegativeSectionParsingTests
  {
    LoreSettings _settings;
    ParserService _parser;

    string ErrorFilesFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "v0.1", "TestData", "NegativeTestData", "Section");

    [SetUp]
    public void SetupLoreSettings()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ErrorFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }


    [Test]
    [TestOf(typeof(DefinitionNotFoundException))]
    public void ParseFile_ThrowsOn_UnknownSectionName()
    {
      Assert.Throws<DefinitionNotFoundException>(() => _parser.ParseSingleFile(Path.Combine(ErrorFilesFolder, "UnknownSectionName.md")));
    }
  }

}