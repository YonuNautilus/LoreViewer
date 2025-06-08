namespace v0_5.NegativeTests
{
  public class NegativeValidationTests
  {
    public LoreSettings _settings;
    public LoreParser _parser;
    string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "NegativeTestData");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    public void MissingFieldsOnFirstNodeTest()
    {
      _parser.ParseFile(Path.Combine(ValidFilesFolder, "MissingFieldsOnFirst.md"));
      _parser.Validate();

      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(2));

      Assert.That(_parser.validator.ValidationResult.Errors[0].Message, Contains.Substring("required field"));
      Assert.That(_parser.validator.ValidationResult.Errors[1].Message, Contains.Substring("grandparent of required field"));

      Assert.True(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(_parser.GetNode("First Simple Node") as LoreNode));

      _parser._nodes.Clear();
      _parser._collections.Clear();
    }

    [Test]
    public void MissingNestedNodeTest()
    {
      _parser.ParseFile(Path.Combine(ValidFilesFolder, "MissingNestedNode.md"));
      _parser.Validate();

      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(1));

      Assert.That(_parser.validator.ValidationResult.Errors[0].Message, Contains.Substring("Required Nested Node"));

      Assert.True(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(_parser.GetNode("First Simple Node") as LoreNode));

      _parser._nodes.Clear();
      _parser._collections.Clear();
    }

    [Test]
    public void MissingNestedChildNodeTest()
    {
      _parser.ParseFile(Path.Combine(ValidFilesFolder, "MissingNestedChildNode.md"));
      _parser.Validate();

      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(1));

      Assert.That(_parser.validator.ValidationResult.Errors[0].Message, Contains.Substring("Required Grandchild"));

      Assert.True(_parser.validator.ValidationResult.LoreEntityValidationStates.ContainsKey(_parser.GetNode("First Simple Node").GetNode("Parent Of Required Nested Node") as LoreNode));

      _parser._nodes.Clear();
      _parser._collections.Clear();
    }
  }
}
