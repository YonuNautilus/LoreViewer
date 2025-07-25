﻿using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Validation;

namespace v0_7.NegativePicklistTests
{
  [TestFixture]
  public class NegativePicklistSettingsTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativePicklistSettings");

    [Test]
    [TestOf(typeof(DuplicatePicklistEntryNameException))]
    public void DuplicateNameInPicklist()
    {
      _parser = new LoreParser();

      Assert.Throws<DuplicatePicklistEntryNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "DuplicateListItemNames.yaml")));
    }

    [Test]
    [TestOf(typeof(DuplicatePicklistNameException))]
    public void DuplicateNameOfPicklists()
    {
      _parser = new LoreParser();

      Assert.Throws<DuplicatePicklistNameException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "DuplicateListName.yaml")));
    }

    [Test]
    [TestOf(typeof(PicklistsDefinitionNotFoundException))]
    public void UnknownPicklistName()
    {
      _parser = new LoreParser();
      Assert.Throws<PicklistsDefinitionNotFoundException>(() => _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "MissingPicklist.yaml")));
    }
  }

  [TestFixture]
  public class NegativePicklistParsingTests
  {

    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "NegativePicklistParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;
    }

    [Test]
    [TestOf(typeof(LoreValidator))]
    public void InvalidChoiceOnConstrainedField()
    {
      // This should parse, but there should be validation errors
      Assert.DoesNotThrow(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "Crayons.md")));
      _parser.Validate();
      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(1));
      Assert.That(_parser.validator.ValidationResult.Errors.First().Value[0].Message, Contains.Substring("Attribute Mystery field of style Picklist has invalid value 1-3. Valid Values are 1-3-1, 1-3-2"));
    }
  }
}