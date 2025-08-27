using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Settings;
using NUnit.Framework.Internal;
using System.ComponentModel.DataAnnotations;

namespace v0_5.PositiveTests
{
  public class PositiveInheritanceTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "PositiveTestData");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();
      _validator = new ValidationService();
      _valStore = new ValidationStore();
      _repository = new LoreRepository();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;

      _repository.Set(_parser.GetParseResult());

      _valStore.Set(_validator.Validate(_repository));
    }

    [Test]
    public void PositiveTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "SimpleTestFile.md"));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(0));

      Assert.That(_parser.Nodes, Has.Count.EqualTo(1));
      Assert.That(_parser.Nodes[0].Collections, Has.Count.EqualTo(1));
      Assert.That(_parser.Nodes[0].Collections[0], Has.Count.EqualTo(2));
    }
  }
}
