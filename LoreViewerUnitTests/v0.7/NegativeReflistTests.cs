using LoreViewer.Exceptions.SettingsParsingExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.NegativeReflistTests
{
  [TestFixture]
  internal class NegativeReflistTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativeReflistSettings");

    [Test]
    public void MissingRefTypeName()
    {
      _parser = new LoreParser();

      Assert.Throws<FieldRefListNameNotGivenException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "MissingTypeName.yaml")));
    }

    [Test]
    public void InvalidRefTypeName()
    {
      _parser = new LoreParser();

      Assert.Throws<ReferenceListTypeNotFoundException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "InvalidTypeName.yaml")));
    }
  }
}
