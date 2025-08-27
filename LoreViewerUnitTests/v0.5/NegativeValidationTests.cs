using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using System.ComponentModel.DataAnnotations;

namespace v0_5.NegativeTests
{
  public class NegativeValidationTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "NegativeTestData");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();
      _validator = new ValidationService();
      _valStore = new ValidationStore();
      _repository = new LoreRepository();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      _repository.Set(_parser.GetParseResult());

      _valStore.Set(_validator.Validate(_repository));
    }

    [Test]
    public void MissingFieldsOnFirstNodeTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "MissingFieldsOnFirst.md"));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(2));

      Assert.That(_valStore.Result.Errors.Values.ToArray()[0][0].Message, Contains.Substring("double-nested required field"));
      Assert.That(_valStore.Result.Errors.Values.ToArray()[1][0].Message, Contains.Substring("required field"));

      Assert.True(_valStore.Result.LoreEntityValidationStates.ContainsKey(_parser.GetNodeByName("First Simple Node") as LoreNode));

      _parser.Clear();
    }

    [Test]
    public void MissingNestedNodeTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "MissingNestedNode.md"));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(1));

      Assert.That(_valStore.Result.Errors.Values.ToArray()[0][0].Message, Contains.Substring("Required Nested Node"));

      Assert.True(_valStore.Result.LoreEntityValidationStates.ContainsKey(_parser.GetNodeByName("First Simple Node") as LoreNode));

      _parser.Clear();
    }

    [Test]
    public void MissingNestedChildNodeTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "MissingNestedChildNode.md"));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(1));

      Assert.That(_valStore.Result.Errors.Values.ToArray()[0][0].Message, Contains.Substring("Required Grandchild"));

      Assert.True(_valStore.Result.LoreEntityValidationStates.ContainsKey(_parser.GetNodeByName("First Simple Node").GetNode("Parent Of Required Nested Node") as LoreNode));

      _parser.Clear();
    }

    [Test]
    public void MissingCollectionTest()
    {
      _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "MissingCollection.md"));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(1));

      Assert.That(_valStore.Result.Errors.Values.ToArray()[0][0].Message, Contains.Substring("Required Collection"));

      Assert.True(_valStore.Result.LoreEntityValidationStates.ContainsKey(_parser.GetNodeByName("First Simple Node") as LoreNode));

      _parser.Clear();
    }
  }
}
