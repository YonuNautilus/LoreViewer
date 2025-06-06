using LoreViewer.Exceptions.SettingsParsingExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_4.PositiveTests
{
  public class PositiveEmbeddedTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "PositiveTestData");




    [Test]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle1()
    {
      _parser = new LoreParser();

      // Because the MOST ABSTRACT (ie parent type) embedded node has a title, we don't expect an error to throw.
      Assert.DoesNotThrow(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title1.yaml")));
    }


    [Test]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle2()
    {
      _parser = new LoreParser();

      // Because the MOST ABSTRACT (ie parent type) embedded node has a title, we don't expect an error to throw.
      Assert.DoesNotThrow(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title2.yaml")));
    }

    [Test]
    public void ParsingEmbeddedDefsWithSimilarTypeNoTitle3()
    {
      _parser = new LoreParser();

      // Because the MOST ABSTRACT (ie parent type) embedded node has a title, we don't expect an error to throw.
      Assert.DoesNotThrow(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Embedded_Ancestral_No_Title3.yaml")));
    }
  }
}
