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
      Assert.That(nodeToCheck.Name, Is.EqualTo("Vela Orion"));
      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(2));

      // Now do an in-depth check of all attributes
      Dictionary<string, LoreAttribute> nodeAttr = nodeToCheck.Attributes;
      string currAttr = "Aliases";

      Assert.NotNull(nodeAttr[currAttr]);
      Assert.True(nodeAttr[currAttr].HasValues);
      Assert.False(nodeAttr[currAttr].HasValue);
      Assert.False(nodeAttr[currAttr].IsNested);

      Assert.That(nodeAttr[currAttr].Values.Count, Is.EqualTo(3));
      Assert.That(nodeAttr[currAttr].Values[0], Is.EqualTo("V"));
      Assert.That(nodeAttr[currAttr].Values[1], Is.EqualTo("Orion Ghost"));
      Assert.That(nodeAttr[currAttr].Values[2], Is.EqualTo("Silent Flame"));

      currAttr = "Employment History";

      Assert.NotNull(nodeAttr[currAttr]);

      Assert.True(nodeAttr[currAttr].IsNested);
      Assert.False(nodeAttr[currAttr].HasValue);
      Assert.False(nodeAttr[currAttr].HasValues);

      Assert.Null(nodeAttr[currAttr].Values);
      Assert.Null(nodeAttr[currAttr].Value);
      Assert.NotNull(nodeAttr[currAttr].NestedAttributes);

      Assert.That(nodeAttr[currAttr].NestedAttributes.Count, Is.EqualTo(3));
      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Organization"]);

      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Organization"].Value);
      Assert.That(nodeAttr[currAttr].NestedAttributes["Organization"].Value, Is.EqualTo("Nightfall Syndicate"));

      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Roles"]);
      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Roles"].Values);
      Assert.That(nodeAttr[currAttr].NestedAttributes["Roles"].Values.Count, Is.EqualTo(3));

      Assert.That(nodeAttr[currAttr].NestedAttributes["Roles"].Values[0], Is.EqualTo("Infiltrator"));
      Assert.That(nodeAttr[currAttr].NestedAttributes["Roles"].Values[1], Is.EqualTo("Handler"));
      Assert.That(nodeAttr[currAttr].NestedAttributes["Roles"].Values[2], Is.EqualTo("Intel Courier"));

      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Duration"]);
      Assert.NotNull(nodeAttr[currAttr].NestedAttributes["Duration"].Value);
      Assert.That(nodeAttr[currAttr].NestedAttributes["Duration"].Value, Is.EqualTo("2012–2021"));




      Assert.DoesNotThrow(() => { nodeToCheck = _parser._nodes[1]; });
      Assert.NotNull(nodeToCheck);

      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(9));

      bool[] foundResults = nodeToCheck.Attributes.Select(kvp => kvp.Value.HasValue).ToArray();
      Assert.That(foundResults, Is.EqualTo(hasSingleValue));
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
