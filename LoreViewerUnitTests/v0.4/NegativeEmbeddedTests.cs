using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_4.NegativeTests
{
  public class NegativeEmbeddedTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "NegativeTestData");

    [OneTimeSetUp]
    public static void SetupLoreSettings()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings_Invalid_Embedded_Type.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    public void InvalidEmbeddedNodeType()
    {
      Assert.Throws<EmbeddedNodeTypeNotAllowedException>(() => _parser.ParseFile(Path.Combine(ValidFilesFolder, "Invalid_Embedded_Type.md")));
    }
  }
}
