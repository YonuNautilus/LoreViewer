using LoreViewer.Exceptions.SettingsParsingExceptions;

namespace v0_4.NegativeTests
{
  internal class NegativeGeneralTest
  {
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "NegativeTestData");

    [Test]
    [TestOf(typeof(DuplicateDefinitionNamesException))]
    public void DuplicateNameTest()
    {
      LoreParser _parser = new LoreParser();

      Assert.Throws<DuplicateDefinitionNamesException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Duplicate_Name.yaml")));

    }
  }
}
