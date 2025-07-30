using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Validation;

namespace v0_7.DateRangeTests
{
  [TestFixture]
  [TestOf(typeof(DateRangeAttributeValue))]
  [TestOf(typeof(DateRangeAttributeValue.DateRangeValue))]
  public class PositiveDateRangeTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "DateRangePositiveParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    [Order(0)]
    public void NoDateRangeParsingErrors()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(1));

      Assert.That(_parser.Errors, Has.Count.EqualTo(0));

      ILoreNode rangeTester = _parser.Nodes[0];

      Assert.True(rangeTester.HasAttributes);
      Assert.That(rangeTester.Attributes, Has.Count.EqualTo(2));
    }

    [Test]
    [Order(1)]
    public void DateRangeCalculations()
    {
      Assert.DoesNotThrow(() => { var t = (_parser.Nodes[0].Attributes[0].Value as DateRangeAttributeValue).Value.TimeSpan; });
      Assert.That((_parser.Nodes[0].Attributes[0].Value as DateRangeAttributeValue).Value.TimeSpan, Is.EqualTo(new TimeSpan(days: 4783, hours: 0, minutes: 0, seconds: 0)));

      var multiValueRanges = _parser.Nodes[0].Attributes[1].Values;

      // Roud total days and hours to an int, since the millisecond differences between the calculated values will make this comparison normally fail.
      Assert.NotNull((multiValueRanges[0] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That(((int)(multiValueRanges[0] as DateRangeAttributeValue).Value.TimeSpan.Value.TotalDays), Is.EqualTo((int)(DateTime.Now - new DateTime(2012, 4, 10)).TotalDays));
      Assert.That(((int)(multiValueRanges[0] as DateRangeAttributeValue).Value.TimeSpan.Value.TotalHours), Is.EqualTo((int)(DateTime.Now - new DateTime(2012, 4, 10)).TotalHours));
      Assert.That((multiValueRanges[0] as DateRangeAttributeValue).Value.Duration, Contains.Substring(" +"));

      Assert.That((multiValueRanges[1] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("Indeterminate"));
      Assert.Null((multiValueRanges[1] as DateRangeAttributeValue).Value.TimeSpan);

      Assert.NotNull((multiValueRanges[2] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[2] as DateRangeAttributeValue).Value.TimeSpan, Is.EqualTo(new TimeSpan(days: 4820, hours: 0, minutes: 0, seconds: 0)));
      Assert.That((multiValueRanges[2] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("4820.00:00:00"));

      Assert.Null((multiValueRanges[3] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[3] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("Indeterminate"));
      Assert.DoesNotThrow(() => { var v = (multiValueRanges[3] as DateRangeAttributeValue).Value.TimeSpan; });

      Assert.Null((multiValueRanges[4] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[4] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("Indeterminate"));
      Assert.That((multiValueRanges[4] as DateRangeAttributeValue).Value.IsStartTBD);
      Assert.That((multiValueRanges[4] as DateRangeAttributeValue).Value.IsEndTBD);

      Assert.Null((multiValueRanges[5] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[5] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("Indeterminate"));
      Assert.That((multiValueRanges[5] as DateRangeAttributeValue).Value.IsStartTBD);
      Assert.That((multiValueRanges[5] as DateRangeAttributeValue).Value.IsEndTBD);

      Assert.Null((multiValueRanges[6] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[6] as DateRangeAttributeValue).Value.Duration, Is.EqualTo("Indeterminate"));
      Assert.That((multiValueRanges[6] as DateRangeAttributeValue).Value.IsStartDate);
      Assert.That((multiValueRanges[6] as DateRangeAttributeValue).Value.StartDateTime, Is.EqualTo(new DateTime(2018, 3, 16)));
      Assert.That((multiValueRanges[6] as DateRangeAttributeValue).Value.IsEndTBD);

      Assert.NotNull((multiValueRanges[7] as DateRangeAttributeValue).Value.TimeSpan);
      Assert.That((multiValueRanges[7] as DateRangeAttributeValue).Value.Duration, Contains.Substring("-"));
      Assert.That((multiValueRanges[7] as DateRangeAttributeValue).Value.IsStartDate);
      Assert.That((multiValueRanges[7] as DateRangeAttributeValue).Value.IsEndDate);
    }

    [Test]
    [Order(2)]
    public void DateRangeValidations()
    {
      LoreValidationResult result = _parser.validator.ValidationResult;

      Assert.That(_parser.validator.ValidationResult.Errors, Is.Empty);
      Assert.That(_parser.validator.ValidationResult.LoreEntityValidationMessages, Is.Not.Empty);

      var multiValueRanges = _parser.Nodes[0].Attributes[1].Values;

      Assert.That(result.LoreEntityValidationStates[_parser.Nodes[0] as LoreEntity], Is.EqualTo(EValidationState.ChildWarning));
      Assert.That(result.LoreEntityValidationStates[(_parser.Nodes[0] as LoreNode).Attributes[0]], Is.EqualTo(EValidationState.Passed));


      // Check that start date that is later than an end date gives warning
      var attr2 = _parser.Nodes[0].Attributes[1];

      Assert.That(result.LoreEntityValidationStates[attr2], Is.EqualTo(EValidationState.Warning));
    }

  }
  public class NegativeDateRangeTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "DateRangeNegativeParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    public void DateRangeParsingExceptions()
    {
      Assert.Throws<DateRangeCannotParseStartDateException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeStartError.md")); });
      Assert.Throws<DateRangeCannotParseEndDateException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeEndError.md")); });

      Assert.Throws<DateRangeCannotParseStartDateException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangePresentStart.md")); });
      Assert.Throws<DateRangeCannotParseStartDateException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeOngoingStart.md")); });

      Assert.Throws<DateRangeNoPipeCharacterException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeNoPipe.md")); });

      Assert.Throws<DateRangeTooManyPipeCharactersException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeTooMuchPipe1.md")); });
      Assert.Throws<DateRangeTooManyPipeCharactersException>(() => { _parser.ParseFile(Path.Combine(ValidFilesFolder, "DateRangeTooMuchPipe2.md")); });
    }
  }
}
