using LoreViewer.Exceptions;
using Markdig.Syntax;
using NUnit.Framework.Internal;

namespace v0_1.PositiveTests
{
  [TestFixture]
  [TestOf(typeof(LoreParser))]
  public class PositiveFieldParsingTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.1", "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseSingleFile(fileToTest);
    }

    [Test]
    public void NodesCountTest() { Assert.That(_parser.Nodes, Has.Count.EqualTo(3)); }

    [Test]
    public void CharacterFieldsTest()
    {
      LoreEntity tempEntity = _parser.Nodes.FirstOrDefault(node => node.Name.Equals("Vela Orion")) as LoreEntity;
      LoreNode nodeToCheck = _parser.Nodes.FirstOrDefault(node => node.Name.Equals("Vela Orion")) as LoreNode;

      Assert.NotNull(nodeToCheck);
      Assert.That(nodeToCheck.Name, Is.EqualTo("Vela Orion"));
      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(2));

      // Now do an in-depth check of all attributes
      LoreAttribute[] nodeAttrs = nodeToCheck.Attributes.ToArray();
      string currAttr = "Aliases";

      Assert.NotNull(nodeToCheck.GetAttribute(currAttr));
      Assert.True(nodeToCheck.GetAttribute(currAttr).HasValues);
      Assert.False(nodeToCheck.GetAttribute(currAttr).HasValue);
      Assert.False(nodeToCheck.GetAttribute(currAttr).IsNested);

      Assert.That(nodeToCheck.GetAttribute(currAttr).Values.Count, Is.EqualTo(3));
      Assert.That(nodeToCheck.GetAttribute(currAttr).Values[0], Is.EqualTo("V"));
      Assert.That(nodeToCheck.GetAttribute(currAttr).Values[1], Is.EqualTo("Orion Ghost"));
      Assert.That(nodeToCheck.GetAttribute(currAttr).Values[2], Is.EqualTo("Silent Flame"));

      currAttr = "Employment History";

      Assert.NotNull(nodeToCheck.GetAttribute(currAttr));

      Assert.True(nodeToCheck.GetAttribute(currAttr).IsNested);
      Assert.False(nodeToCheck.GetAttribute(currAttr).HasValue);
      Assert.False(nodeToCheck.GetAttribute(currAttr).HasValues);

      Assert.Null(nodeToCheck.GetAttribute(currAttr).Values);
      Assert.Null(nodeToCheck.GetAttribute(currAttr).Value);
      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).HasAttributes);

      Assert.That(nodeToCheck.GetAttribute(currAttr).Attributes.Count, Is.EqualTo(3));
      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Organization"));

      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Organization").Value);
      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Organization").Value, Is.EqualTo("Nightfall Syndicate"));

      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles"));
      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles").Values);
      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles").Values.Count, Is.EqualTo(3));

      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles").Values[0], Is.EqualTo("Infiltrator"));
      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles").Values[1], Is.EqualTo("Handler"));
      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Roles").Values[2], Is.EqualTo("Intel Courier"));

      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Duration"));
      Assert.NotNull(nodeToCheck.GetAttribute(currAttr).GetAttribute("Duration").Value);
      Assert.That(nodeToCheck.GetAttribute(currAttr).GetAttribute("Duration").Value, Is.EqualTo("2012–2021"));
    }

    [Test]
    public void ListFieldsEmphasisTest()
    {
      bool[] hasSingleValue = { true, true, true, true, true, true, false, false, false };

      LoreNode nodeToCheck = null;

      Assert.DoesNotThrow(() => { nodeToCheck = _parser.Nodes.FirstOrDefault(node => node.Name.Equals("This is a valid markdown with bullet point fields/attributes")) as LoreNode; } );
      Assert.NotNull(nodeToCheck);

      Assert.That(nodeToCheck.Attributes.Count, Is.EqualTo(9));

      bool[] foundResults = nodeToCheck.Attributes.Select(a => a.HasValue).ToArray();
      Assert.That(foundResults, Is.EqualTo(hasSingleValue));
    }

    [Test]
    public void ParseFieldEdgeCases_CorrectFormats_ParsesExpectedAttributes()
    {
      LoreNode node = _parser.Nodes.FirstOrDefault(node => node.Name.Equals("Field Format Edge Case Test")) as LoreNode;

      var attrs = node.Attributes;

      // Flat and simple emphasis variants
      Assert.That(node.GetAttribute("FlatField").Value, Is.EqualTo("Normal value, no formatting"));
      Assert.That(node.GetAttribute("BoldField").Value, Is.EqualTo("This value has no bold, just the field label"));
      Assert.That(node.GetAttribute("ItalicField").Value, Is.EqualTo("Italicized field name, regular value"));
      Assert.That(node.GetAttribute("ColonNoSpace").Value, Is.EqualTo("ThisShouldStillWork"));
      Assert.That(node.GetAttribute("ColonAfterSpace").Value, Is.EqualTo("This should work too"));
      Assert.That(node.GetAttribute("MixedFormatField").Value, Is.EqualTo("This value contains emphasis and bold"));

      // Nested field parsing
      var nested = node.GetAttribute("NestedField");
      Assert.That(nested.GetAttribute("SubFieldOne").Value, Is.EqualTo("Value1"));
      Assert.That(nested.GetAttribute("SubFieldBold").Value, Is.EqualTo("Bold-labeled nested value"));
      Assert.That(nested.GetAttribute("SubFieldItalic").Value, Is.EqualTo("Italic-labeled nested value"));

      // Multi-value flat list
      var multi = node.GetAttribute("MultiField").Values;
      Assert.That(multi, Has.Count.EqualTo(3));
      Assert.That(multi[0], Is.EqualTo("One"));
      Assert.That(multi[1], Is.EqualTo("Two"));
      Assert.That(multi[2], Is.EqualTo("Three (bolded value)"));

      // Nested multi-value
      var nestedMulti = node.GetAttribute("NestedMultiField").GetAttribute("DetailList").Values;
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

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.1", "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "SectionsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [SetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseSingleFile(fileToTest);
    }


    [Test]
    public void NodesCountTest() { Assert.That(_parser.Nodes.Count, Is.EqualTo(2)); }

    [Test]
    public void ParseSectionTest_CorrectBlocksAndContent()
    {
      LoreNode? node = _parser.GetNode("Section Target Test") as LoreNode;

      Assert.NotNull(node);

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
      Assert.That(personality.Sections.Count, Is.EqualTo(2));
      var strengths = personality.Sections.FirstOrDefault(s => s.Name == "Strengths");
      var weaknesses = personality.Sections.FirstOrDefault(s => s.Name == "Weaknesses");
      Assert.That(strengths, Is.Not.Null);
      Assert.That(weaknesses, Is.Not.Null);
      Assert.That(strengths.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(weaknesses.Summary, Does.Contain("Avoids confrontation"));

      // Check nested sections under History
      Assert.That(history.Sections.Count, Is.EqualTo(2));
      var earlyLife = history.Sections.FirstOrDefault(s => s.Name == "Early Life");
      var laterYears = history.Sections.FirstOrDefault(s => s.Name == "Later Years");
      Assert.That(earlyLife, Is.Not.Null);
      Assert.That(laterYears, Is.Not.Null);
      Assert.That(earlyLife.Summary, Does.Contain("cloudport colony"));
      Assert.That(earlyLife.Blocks.Any(b => b is QuoteBlock), Is.True);
      Assert.That(laterYears.Summary, Does.Contain("archival analytics"));

      // Fun Facts should be flat
      Assert.That(funFacts.Sections.Count, Is.EqualTo(0));
      Assert.That(funFacts.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(funFacts.Summary, Does.Contain("ultraviolet"));
    }

    [Test]
    public void ParseSectionWithFields_ParsesAttributesCorrectly()
    {
      LoreNode? node = _parser.GetNode("Fielded Section Test") as LoreNode;
      Assert.NotNull(node);

      LoreSection? personality = node.GetSection("Personality");
      LoreSection? notes = node.GetSection("Notes");

      Assert.That(personality, Is.Not.Null);
      Assert.That(notes, Is.Not.Null);

      // Check attribute parsing in Personality
      Assert.That(personality.Attributes, Is.Not.Null);
      Assert.That(personality.Attributes.Count, Is.EqualTo(2));
      Assert.That(personality.GetAttribute("Tone").Value, Is.EqualTo("Calm, careful"));
      Assert.That(personality.GetAttribute("Social Tendencies").Values, Has.Count.EqualTo(2));
      Assert.That(personality.GetAttribute("Social Tendencies").Values[0], Is.EqualTo("Avoids group settings"));
      Assert.That(personality.GetAttribute("Social Tendencies").Values[1], Is.EqualTo("Strong one-on-one engagement"));

      // Check freeform paragraph is still preserved
      Assert.That(personality.Summary, Does.Contain("She displays openness"));

      // Check Notes section did NOT parse any attributes
      Assert.That(notes.Attributes, Is.Null.Or.Empty);
      Assert.That(notes.Blocks.First(), Is.InstanceOf<ListBlock>());
      Assert.That(notes.Summary, Does.Contain("Carries a data slate"));
    }

  }
}
