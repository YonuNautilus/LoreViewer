using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.Exceptions.SettingsParsingExceptions;

namespace v0_7.NegativeReflistTests
{
  [TestFixture]
  internal class NegativeReflistTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativeReflistSettings");

    [Test]
    [TestOf(typeof(FieldRefListNameNotGivenException))]
    public void MissingRefTypeName()
    {
      _parser = new LoreParser();

      Assert.Throws<FieldRefListNameNotGivenException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "MissingTypeName.yaml")));
    }

    [Test]
    [TestOf(typeof(ReferenceListTypeNotFoundException))]
    public void InvalidRefTypeName()
    {
      _parser = new LoreParser();

      Assert.Throws<ReferenceListTypeNotFoundException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "InvalidTypeName.yaml")));
    }
  }

  [TestFixture]
  internal class NegativeReflistParsingTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativeReflistParsingData");

    [Test]
    [TestOf(typeof(ReflistCannotResovleException))]
    public void ReflistCannotResolve()
    {
      Assert.Throws<ReflistCannotResovleException>(() =>
      {
        LoreParser _parser = new LoreParser();
        _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));
        _parser.BeginParsingFromFolder(ValidFilesFolder);
      });
    }
  }
}
