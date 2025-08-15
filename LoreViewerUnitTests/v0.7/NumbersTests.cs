using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Settings;
using LoreViewer.Exceptions.LoreParsingExceptions;

namespace v0_7.NumbersTests
{
  [TestFixture]
  public class PositiveNumbersTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NumberPositiveData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      //_parser.BeginParsingFromFolder(ValidFilesFolder);

      //_settings = _parser.Settings;
    }

    [Test]
    public void NoParsingExceptions()
    {
      Assert.DoesNotThrow(() => _parser.BeginParsingFromFolder(ValidFilesFolder));
    }
  }

  [TestFixture]
  public class NegativeNumbersTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NumberNegativeData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      //_parser.BeginParsingFromFolder(ValidFilesFolder);

      //_settings = _parser.Settings;
    }

    [Test]
    public void ParsingException1()
    {
      Assert.Throws<NumberCannotParseIntoNumericException>(() => _parser.ParseFile(Path.Combine(ValidFilesFolder, "NonparsableNumbers.md")));
    }

    [Test]
    public void ParsingException2()
    {
      Assert.Throws<NumberInvalidModifiersException>(() => _parser.ParseFile(Path.Combine(ValidFilesFolder, "NonparsableNumbers2.md")));
    }
  }
}
