using LoreViewer;

namespace LoreViewerUnitTests
{
  [TestFixture]
  public class ParsingTets
  {
    LoreSettings _settings;
    LoreParser _parser;

    string FolderName = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "ErrorTests");

    [SetUp]
    public void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    public void ValidFieldFormatsTest()
    {
      bool[] hasSingleValue = { true, true, true, true, true, true, false, false, false };
      string fileToTest = Path.Combine(FolderName, "FieldsTest.md");

      _parser.ParseFile(fileToTest);

      Assert.AreEqual(1, _parser._nodes.Count);

      LoreNode nodeToCheck = _parser._nodes[0];

      Assert.NotNull(nodeToCheck);
      Assert.AreEqual("This is a valid markdown with bullet point fields/attributes", nodeToCheck.Name);
      Assert.AreEqual(9, nodeToCheck.Attributes.Count);

      Assert.AreEqual(hasSingleValue, nodeToCheck.Attributes.Select(kvp => kvp.Value.HasValue).ToArray());
    }

    [Test]
    public void UntaggedFirstHeadingTest()
    {
      string fileToTest = Path.Combine(FolderName, "TopHeadingUntagged.md");

      Assert.Throws<NoTagParsingException>(() =>
      {
        _parser.ParseFile(fileToTest);
      });
    }

    [Test]
    public void FirstHeadingIsSectionTest()
    {
      string fileToTest =  Path.Combine(FolderName, "FirstHeadingIsSection.md");

      Assert.Throws<FirstHeadingTagException>(() =>
      {
        _parser.ParseFile(fileToTest);
      });
    }
  }
}
