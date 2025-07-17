using LoreViewer.Exceptions.SettingsParsingExceptions;
using NUnit.Framework.Internal;

namespace v0_4.NegativeTests
{
  [TestFixture]
  [TestOf(typeof(LoreSettings))]
  public class NegativeInheritanceTets
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "NegativeTestData");

    [OneTimeSetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();
    }

    [Test]
    [TestOf(typeof(CyclicalInheritanceException))]
    public void CyclicInheritance()
    {
      Assert.Throws<CyclicalInheritanceException>(
        () => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Cyclic.yaml")));
    }

    [Test]
    [TestOf(typeof(InheritingMissingTypeDefinitionException))]
    public void MissingInheritanceType()
    {
      Assert.Throws<InheritingMissingTypeDefinitionException>(
        () => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Missing.yaml")));
    }

    [Test]
    [TestOf(typeof(EmbeddedTypeUnknownException))]
    public void MissingEmbeddedType()
    {
      Assert.Throws<EmbeddedTypeUnknownException>(
        () => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Missing_Embedded.yaml")));
    }
  }
}