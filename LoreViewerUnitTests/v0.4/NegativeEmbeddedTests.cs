using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Settings;
using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.Exceptions.SettingsParsingExceptions;

namespace v0_4.NegativeTests
{
  public class NegativeEmbeddedTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "NegativeTestData");

    #region Lore Parsing Tests

    [Test]
    [TestOf(typeof(EmbeddedNodeTypeNotAllowedException))]
    public void InvalidEmbeddedNodeType()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Invalid_Embedded_Type.yaml"));

      _settings = _parser.Settings;

      Assert.Throws<EmbeddedNodeTypeNotAllowedException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Invalid_Embedded_Type.md")));
    }

    [Test]
    [TestOf(typeof(EmbeddedNodeInvalidNameException))]
    public void InvalidEmbeddedNodeTitle()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Wrong_Title.yaml"));

      _settings = _parser.Settings;

      Assert.Throws<EmbeddedNodeInvalidNameException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Embedded_Node_Wrong_Title.md")));
    }

    [Test]
    [TestOf(typeof(EmbeddedNodeAlreadyAddedException))]
    public void EmbeddedAlreadyAddedSameType()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Embedded_Already_Added.yaml"));

      Assert.Throws<EmbeddedNodeAlreadyAddedException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Embedded_Already_Added_Same.md")));
      Assert.Throws<EmbeddedNodeAlreadyAddedException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Embedded_Already_Added_Parent.md")));
      Assert.Throws<EmbeddedNodeAlreadyAddedException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Embedded_Already_Added_Child.md")));
    }
    #endregion Lore Parsing Tests

    #region Settings Parsing Tests

    [Test]
    [TestOf(typeof(EmbeddedNodesWithSameTitleException))]
    public void ParsingEmbeddedDefsWithSameTitle()
    {
      _parser = new ParserService();

      Assert.Throws<EmbeddedNodesWithSameTitleException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Emb_Defs_Same_Title.yaml")));
    }

    [Test]
    [TestOf(typeof(EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException))]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle1()
    {
      _parser = new ParserService();

      Assert.Throws<EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title1.yaml")));
    }


    [Test]
    [TestOf(typeof(EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException))]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle2()
    {
      _parser = new ParserService();

      Assert.Throws<EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title2.yaml")));
    }

    [Test]
    [TestOf(typeof(EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException))]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle3()
    {
      _parser = new ParserService();

      Assert.Throws<EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title3.yaml")));
    }

    [Test]
    [TestOf(typeof(EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException))]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle4()
    {
      _parser = new ParserService();

      Assert.Throws<EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title4.yaml")));
    }
    #endregion Settings Parsing Tests
  }
}
