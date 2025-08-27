using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Settings;
using LoreViewer.Exceptions.SettingsParsingExceptions;

namespace v0_7.NegativePicklistTests
{
  [TestFixture]
  public class NegativePicklistSettingsTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativePicklistSettings");

    [Test]
    [TestOf(typeof(DuplicatePicklistEntryNameException))]
    public void DuplicateNameInPicklist()
    {
      _parser = new ParserService();

      Assert.Throws<DuplicatePicklistEntryNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "DuplicateListItemNames.yaml")));
    }

    [Test]
    [TestOf(typeof(DuplicatePicklistNameException))]
    public void DuplicateNameOfPicklists()
    {
      _parser = new ParserService();

      Assert.Throws<DuplicatePicklistNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "DuplicateListName.yaml")));
    }

    [Test]
    [TestOf(typeof(PicklistsDefinitionNotFoundException))]
    public void UnknownPicklistName()
    {
      _parser = new ParserService();
      Assert.Throws<PicklistsDefinitionNotFoundException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "MissingPicklist.yaml")));
    }
  }

  [TestFixture]
  public class NegativePicklistParsingTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativePicklistParsingData");

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
    [TestOf(typeof(ValidationService))]
    public void InvalidChoiceOnConstrainedField()
    {
      // This should parse, but there should be validation errors
      Assert.DoesNotThrow(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Crayons.md")));
      _repository.Set(_parser.GetParseResult());
      _valStore.Set(_validator.Validate(_repository));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(1));
      Assert.That(_valStore.Result.Errors.First().Value[0].Message, Contains.Substring("Attribute Mystery field of style Picklist has invalid value 1-3. Valid Values are 1-3-1, 1-3-2"));
    }
  }
}