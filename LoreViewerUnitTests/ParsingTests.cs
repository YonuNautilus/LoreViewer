using LoreViewer;

namespace LoreViewerUnitTests
{
  [TestFixture]
  public class ParsingTets
  {
    LoreSettings _settings;
    LoreParser _parser;

    string ErrorFilesFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "ErrorFiles");
    string ValidFilesFolder = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "ValidFiles");

    [SetUp]
    public void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    [TestOf(typeof(LoreParser))]
    public void ValidFieldFormatsTest()
    {
      bool[] hasSingleValue = { true, true, true, true, true, true, false, false, false };
      string fileToTest = Path.Combine(ValidFilesFolder, "FieldsTest.md");

      _parser.ParseFile(fileToTest);

      Assert.That(_parser._nodes.Count, Is.EqualTo(2));

      LoreNode nodeToCheck = _parser._nodes[0];

      Assert.NotNull(nodeToCheck);
      Assert.That(nodeToCheck.Name, Is.EqualTo("This is a valid markdown with bullet point fields/attributes"));
      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(9));

      Assert.That(nodeToCheck.Attributes.Select(kvp => kvp.Value.HasValue).ToArray(), Is.EqualTo(hasSingleValue));

      Assert.DoesNotThrow(() => { nodeToCheck = _parser._nodes[1]; });
      Assert.NotNull(nodeToCheck);

      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(2));
    }

    [Test]
    [TestOf(typeof(LoreParser))]
    public void UntaggedFirstHeadingTest()
    {
      string fileToTest = Path.Combine(ErrorFilesFolder, "TopHeadingUntagged.md");

      Assert.Throws<NoTagParsingException>(() =>
      {
        _parser.ParseFile(fileToTest);
      });
    }

    [Test]
    [TestOf(typeof(LoreParser))]
    public void FirstHeadingIsSectionTest()
    {
      string fileToTest =  Path.Combine(ErrorFilesFolder, "FirstHeadingIsSection.md");

      Assert.Throws<FirstHeadingTagException>(() =>
      {
        _parser.ParseFile(fileToTest);
      });
    }
  }
}
