using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Exceptions.LoreParsingExceptions;
using System.ComponentModel.DataAnnotations;

namespace v0_7.DateTimeTests
{
  [TestFixture]
  [Order(0)]
  [TestOf(typeof(DateTimeAttributeValue))]
  [TestOf(typeof(DateTimeAttributeValue.DateValue))]
  public class PositiveDateTimeTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "DateTimePositiveParsingData");

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
    [Order(0)]
    public void NoDateTimeParsingErrors()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(1));

      Assert.That(_parser.Errors, Has.Count.EqualTo(0));

      LoreNode dateTester = _parser.Nodes[0];

      Assert.True(dateTester.HasAttributes);
      Assert.That(dateTester.Attributes, Has.Count.EqualTo(2));
    }

    [Test]
    [Order(1)]
    public void DateTimeCalculations()
    {
      Assert.DoesNotThrow(() => { var t = (_parser.Nodes[0].Attributes[0].Value as DateTimeAttributeValue).Value.Date; });
      Assert.That((_parser.Nodes[0].Attributes[0].Value as DateTimeAttributeValue).Value.Date, Is.EqualTo(new DateTime(2012, 4, 10, 10, 30, 0)));

      var multiValueDates = _parser.Nodes[0].Attributes[1].Values;

      // Roud total days and hours to an int, since the millisecond differences between the calculated values will make this comparison normally fail.
      Assert.NotNull((multiValueDates[0] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[0] as DateTimeAttributeValue).Value.Date, Is.EqualTo(new DateTime(2012, 4, 10)));

      Assert.Null((multiValueDates[1] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[1] as DateTimeAttributeValue).Value.IsUnknown);

      Assert.NotNull((multiValueDates[2] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[2] as DateTimeAttributeValue).Value.IsDateValid);

      Assert.Null((multiValueDates[3] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[3] as DateTimeAttributeValue).Value.IsTBD);
      Assert.Throws<InvalidOperationException>(() => { var v = (multiValueDates[3] as DateTimeAttributeValue).Value.Date.Value.TimeOfDay; });

      Assert.NotNull((multiValueDates[4] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[4] as DateTimeAttributeValue).Value.IsDateValid);

      Assert.NotNull((multiValueDates[5] as DateTimeAttributeValue).Value.Date);
      Assert.That((multiValueDates[5] as DateTimeAttributeValue).Value.Date, Is.EqualTo(DateTime.Today));
      Assert.That((multiValueDates[5] as DateTimeAttributeValue).Value.IsPresent);
    }

    [Test]
    [Order(2)]
    public void DateTimeValidations()
    {
      LoreValidationResult result = _valStore.Result;

      Assert.That(_valStore.Result.Errors, Is.Empty);
      Assert.That(_valStore.Result.LoreEntityValidationMessages, Is.Not.Empty);

      var multiValueDates = _parser.Nodes[0].Attributes[1].Values;

      Assert.That(result.LoreEntityValidationStates[_parser.Nodes[0] as LoreEntity], Is.EqualTo(EValidationState.ChildWarning));
      Assert.That(result.LoreEntityValidationStates[(_parser.Nodes[0] as LoreNode).Attributes[0]], Is.EqualTo(EValidationState.Passed));


      // Check that start date that is later than an end date gives warning
      var attr2 = _parser.Nodes[0].Attributes[1];

      Assert.That(result.LoreEntityValidationStates[attr2], Is.EqualTo(EValidationState.Warning));
    }

  }

  [Order(1)]
  public class NegativeDateTimeTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "DateTimeNegativeParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    public void DateTimeParsingExceptions()
    {
      Assert.Throws<DateTimeCannotParseException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateTimeBadFormat.md")); });
      Assert.Throws<DateTimeCannotParseException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateTimeBC.md")); });

      Assert.Throws<DateTimeCannotParseException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateTimeInvalidKeyword.md")); });
      // Yes, just a time is valid for DateTime
      //Assert.Throws<DateTimeCannotParseException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateTimeTimeOnly.md")); });
    }
  }
}
