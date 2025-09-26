using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Domain.Settings.Definitions;

namespace v0_2.PositiveTests
{
  [TestFixture]
  [TestOf(typeof(ParserService))]
  public class PositiveNarrativeParsingTets
  {
    public static LoreSettings _settings;
    public static ParserService _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.2", "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "SectionsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public static void SetupLoreSettings()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseSingleFile(fileToTest);
    }

    [Test]
    public void SectionsCountTest()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(1));
      Assert.That(_parser.Nodes[0], Is.TypeOf(typeof(LoreNode)));
      Assert.That((_parser.Nodes[0] as LoreNode).Sections, Has.Count.EqualTo(4));
      Assert.IsTrue((_parser.Nodes[0] as LoreNode).HasNarrativeContent);
    }

    [Test]
    public void FreeformWithFreeformChild()
    {
      LoreNode nodeToTest = _parser.Nodes[0] as LoreNode;
      LoreSection sectionToTest = nodeToTest.GetSection("Freeform With Freeform Child");
      Assert.NotNull(sectionToTest);
      Assert.That(sectionToTest, Is.SameAs(nodeToTest.Sections[0]));
      Assert.IsTrue(sectionToTest.HasNarrativeText);
      Assert.That(sectionToTest.Summary.Trim(), Is.EqualTo("This is the freeform parent section\r\n\r\n- Here is a list\r\n  - here is a subvalue"));
      Assert.IsFalse((sectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasSections);
      Assert.That(sectionToTest.Sections, Has.Count.EqualTo(1));
      LoreSection subsectionToTest = sectionToTest.Sections[0];
      Assert.That(sectionToTest.GetSection("Freeform Child"), Is.SameAs(subsectionToTest));
      Assert.That(subsectionToTest.Summary.Trim(), Is.EqualTo("- Here is a list\r\n- in a freeform subsection.\r\n  - This list should be parsed as text"));
    }

    [Test]
    public void FieldsSectionWithFreeformChild()
    {
      LoreNode nodeToTest = _parser.Nodes[0] as LoreNode;
      LoreSection sectionToTest = nodeToTest.GetSection("Fields-Section With Freeform Child");
      Assert.NotNull(sectionToTest);
      Assert.That(nodeToTest.Sections[1], Is.SameAs(sectionToTest));
      Assert.IsTrue(sectionToTest.HasNarrativeText);
      Assert.That(sectionToTest.Summary.Trim(), Is.EqualTo("This section has fields and a freeform subsection. This text should be parsed after the fields and placed in the Summary property of the LoreSection.\r\n\r\nYou know, we have that freeform bool on the LoreSectionDefinition,  but it isn't really used for anything...\r\n\r\nI suppose this is a test to see if a section with fields can parse paragraph blocks after a list of defined fields."));
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasSections);

      Assert.That(sectionToTest.Attributes, Has.Count.EqualTo(2));
      LoreAttribute attributeToTest = sectionToTest.Attributes[1];
      Assert.NotNull(attributeToTest);
      Assert.That(sectionToTest.GetAttribute("Field 2"), Is.SameAs(attributeToTest));
      Assert.IsFalse(attributeToTest.HasValue);
      Assert.IsTrue(attributeToTest.HasValues);
      Assert.That(attributeToTest.Values, Has.Count.EqualTo(2));

      Assert.That(sectionToTest.Sections, Has.Count.EqualTo(1));
      LoreSection subsectionToTest = sectionToTest.Sections[0];
      Assert.That(sectionToTest.GetSection("Freeform Child"), Is.SameAs(subsectionToTest));
      Assert.That(subsectionToTest.Summary.Trim(), Is.EqualTo("- this\r\n- is\r\n- a\r\n- freeform\r\n- child\r\n\r\nBecause this subsection is freeform, that list above should be parsed as text."));
    }

    [Test]
    public void FieldsSectionWithFieldsChild()
    {
      LoreNode nodeToTest = _parser.Nodes[0] as LoreNode;
      LoreSection sectionToTest = nodeToTest.GetSection("Fields-Section With Fields-Child");
      Assert.NotNull(sectionToTest);
      Assert.That(nodeToTest.Sections[2], Is.SameAs(sectionToTest));
      Assert.IsFalse(sectionToTest.HasNarrativeText);
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasSections);

      Assert.That(sectionToTest.Attributes, Has.Count.EqualTo(2));
      LoreAttribute attributeToTest = sectionToTest.Attributes[1];
      Assert.NotNull(attributeToTest);
      Assert.That(sectionToTest.GetAttribute("Field 4"), Is.SameAs(attributeToTest));
      Assert.IsFalse(attributeToTest.HasValue);
      Assert.IsTrue(attributeToTest.HasValues);
      Assert.That(attributeToTest.Values, Has.Count.EqualTo(2));

      Assert.That(sectionToTest.Sections, Has.Count.EqualTo(1));
      LoreSection subsectionToTest = sectionToTest.Sections[0];
      Assert.That(sectionToTest.GetSection("Fields-Child"), Is.SameAs(subsectionToTest));
      Assert.IsTrue((subsectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.That((subsectionToTest.Definition as LoreSectionDefinition).fields, Has.Count.EqualTo(2));
      Assert.IsFalse(subsectionToTest.Attributes[1].HasValue);
      Assert.IsTrue(subsectionToTest.Attributes[1].HasValues);
    }

    [Test]
    public void FreeformSectionWithFieldsChild()
    {
      LoreNode nodeToTest = _parser.Nodes[0] as LoreNode;
      LoreSection sectionToTest = nodeToTest.GetSection("Freeform With Fields-Child");
      Assert.NotNull(sectionToTest);
      Assert.That(nodeToTest.Sections[3], Is.SameAs(sectionToTest));
      Assert.IsTrue(sectionToTest.HasNarrativeText);
      Assert.IsFalse((sectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.IsTrue((sectionToTest.Definition as LoreSectionDefinition).HasSections);

      Assert.That(sectionToTest.Attributes, Is.Empty);

      Assert.That(sectionToTest.Sections, Has.Count.EqualTo(1));
      LoreSection subsectionToTest = sectionToTest.Sections[0];
      Assert.That(sectionToTest.GetSection("Fields-Child"), Is.SameAs(subsectionToTest));
      Assert.IsFalse(subsectionToTest.HasNarrativeText);
      Assert.IsTrue((subsectionToTest.Definition as LoreSectionDefinition).HasFields);
      Assert.That(subsectionToTest.Attributes, Has.Count.EqualTo(2));
      Assert.IsFalse(subsectionToTest.Attributes[0].HasValue);
      Assert.IsTrue(subsectionToTest.Attributes[0].HasValues);
      Assert.That(subsectionToTest.Attributes[0].Values, Has.Count.EqualTo(3));
      Assert.IsFalse(subsectionToTest.Attributes[1].HasValues);
      Assert.IsTrue(subsectionToTest.Attributes[1].HasValue);
      Assert.That(subsectionToTest.GetAttribute("Field 8"), Is.EqualTo(subsectionToTest.Attributes[1]));
      Assert.That(subsectionToTest.Attributes[1].Value.ValueString, Is.EqualTo("I don't care."));
    }
  }
}
