using LoreViewer.Exceptions;
using Markdig.Syntax;
using NUnit.Framework.Internal;

namespace PositiveTests
{
  [TestFixture]
  [TestOf(typeof(LoreParser))]
  public class PositiveFieldParsingTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [SetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseFile(fileToTest);
    }

    [Test]
    public void NodesCountTest() { Assert.That(_parser._nodes.Count, Is.EqualTo(3)); }

    [Test]
    public void CharacterFieldsTest()
    {
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
    }

    [Test]
    public void ListFieldsEmphasisTest()
    {
      bool[] hasSingleValue = { true, true, true, true, true, true, false, false, false };

      LoreNode nodeToCheck = null;

      Assert.DoesNotThrow(() => { nodeToCheck = _parser._nodes[1]; });
      Assert.NotNull(nodeToCheck);

      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(9));

      bool[] foundResults = nodeToCheck.Attributes.Select(kvp => kvp.Value.HasValue).ToArray();
      Assert.That(foundResults, Is.EqualTo(hasSingleValue));
    }

    [Test]
    public void ParseFieldEdgeCases_CorrectFormats_ParsesExpectedAttributes()
    {
      var node = _parser._nodes[2];

      var attrs = node.Attributes;

      // Flat and simple emphasis variants
      Assert.That(attrs["FlatField"].Value, Is.EqualTo("Normal value, no formatting"));
      Assert.That(attrs["BoldField"].Value, Is.EqualTo("This value has no bold, just the field label"));
      Assert.That(attrs["ItalicField"].Value, Is.EqualTo("Italicized field name, regular value"));
      Assert.That(attrs["ColonNoSpace"].Value, Is.EqualTo("ThisShouldStillWork"));
      Assert.That(attrs["ColonAfterSpace"].Value, Is.EqualTo("This should work too"));
      Assert.That(attrs["MixedFormatField"].Value, Is.EqualTo("This value contains emphasis and bold"));

      // Nested field parsing
      var nested = attrs["NestedField"].NestedAttributes;
      Assert.That(nested["SubFieldOne"].Value, Is.EqualTo("Value1"));
      Assert.That(nested["SubFieldBold"].Value, Is.EqualTo("Bold-labeled nested value"));
      Assert.That(nested["SubFieldItalic"].Value, Is.EqualTo("Italic-labeled nested value"));

      // Multi-value flat list
      var multi = attrs["MultiField"].Values;
      Assert.That(multi, Has.Count.EqualTo(3));
      Assert.That(multi[0], Is.EqualTo("One"));
      Assert.That(multi[1], Is.EqualTo("Two"));
      Assert.That(multi[2], Is.EqualTo("Three (bolded value)"));

      // Nested multi-value
      var nestedMulti = attrs["NestedMultiField"].NestedAttributes["DetailList"].Values;
      Assert.That(nestedMulti, Has.Count.EqualTo(3));
      Assert.That(nestedMulti[0], Is.EqualTo("Alpha"));
      Assert.That(nestedMulti[1], Is.EqualTo("Beta (italic)"));
      Assert.That(nestedMulti[2], Is.EqualTo("Gamma"));
    }

  }


  [TestFixture]
  [TestOf(typeof(LoreParser))]
  public class PositiveSectionParsingTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "SectionsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [SetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseFile(fileToTest);
    }


    [Test]
    public void NodesCountTest() { Assert.That(_parser._nodes.Count, Is.EqualTo(2)); }

    [Test]
    public void ParseSectionTest_CorrectBlocksAndContent()
    {
      var node = _parser._nodes[0];

      Assert.That(node.Sections.Count, Is.EqualTo(3));

      var topSections = node.Sections;
      Assert.That(topSections.Count, Is.EqualTo(3));

      var personality = topSections.FirstOrDefault(s => s.Name == "Personality");
      var history = topSections.FirstOrDefault(s => s.Name == "History");
      var funFacts = topSections.FirstOrDefault(s => s.Name == "Fun Facts");

      Assert.That(personality, Is.Not.Null);
      Assert.That(history, Is.Not.Null);
      Assert.That(funFacts, Is.Not.Null);

      // Check nested sections under Personality
      Assert.That(personality.SubSections.Count, Is.EqualTo(2));
      var strengths = personality.SubSections.FirstOrDefault(s => s.Name == "Strengths");
      var weaknesses = personality.SubSections.FirstOrDefault(s => s.Name == "Weaknesses");
      Assert.That(strengths, Is.Not.Null);
      Assert.That(weaknesses, Is.Not.Null);
      Assert.That(strengths.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(weaknesses.Text, Does.Contain("Avoids confrontation"));

      // Check nested sections under History
      Assert.That(history.SubSections.Count, Is.EqualTo(2));
      var earlyLife = history.SubSections.FirstOrDefault(s => s.Name == "Early Life");
      var laterYears = history.SubSections.FirstOrDefault(s => s.Name == "Later Years");
      Assert.That(earlyLife, Is.Not.Null);
      Assert.That(laterYears, Is.Not.Null);
      Assert.That(earlyLife.Text, Does.Contain("cloudport colony"));
      Assert.That(earlyLife.Blocks.Any(b => b is QuoteBlock), Is.True);
      Assert.That(laterYears.Text, Does.Contain("archival analytics"));

      // Fun Facts should be flat
      Assert.That(funFacts.SubSections.Count, Is.EqualTo(0));
      Assert.That(funFacts.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(funFacts.Text, Does.Contain("ultraviolet"));
    }

    [Test]
    public void ParseSectionWithFields_ParsesAttributesCorrectly()
    {
      var node = _parser._nodes[1];

      var personality = node.Sections.FirstOrDefault(s => s.Name == "Personality");
      var notes = node.Sections.FirstOrDefault(s => s.Name == "Notes");

      Assert.That(personality, Is.Not.Null);
      Assert.That(notes, Is.Not.Null);

      // Check attribute parsing in Personality
      Assert.That(personality.Attributes, Is.Not.Null);
      Assert.That(personality.Attributes.Count, Is.EqualTo(2));
      Assert.That(personality.Attributes["Tone"].Value, Is.EqualTo("Calm, careful"));
      Assert.That(personality.Attributes["Social Tendencies"].Values, Has.Count.EqualTo(2));
      Assert.That(personality.Attributes["Social Tendencies"].Values[0], Is.EqualTo("Avoids group settings"));
      Assert.That(personality.Attributes["Social Tendencies"].Values[1], Is.EqualTo("Strong one-on-one engagement"));

      // Check freeform paragraph is still preserved
      Assert.That(personality.Text, Does.Contain("She displays openness"));

      // Check Notes section did NOT parse any attributes
      Assert.That(notes.Attributes, Is.Null.Or.Empty);
      Assert.That(notes.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(notes.Text, Does.Contain("Carries a data slate"));
    }

  }
}
