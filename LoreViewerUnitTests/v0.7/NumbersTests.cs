using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.PositiveNumbersTests
{
  [TestFixture]
  public class NumbersTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NumberPositiveData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

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
}
