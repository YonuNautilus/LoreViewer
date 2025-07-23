using LoreViewer.Exceptions.LoreParsingExceptions;
using LoreViewer.Exceptions.SettingsParsingExceptions;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.MiscExceptionTests
{

  [TestFixture]
  internal class MiscExceptions
  {
    static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "MiscExceptionsParsingData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();
      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "MiscExceptions.yaml"));
    }

    [Test]
    public void TypeNotDefined()
    {
      Assert.Throws<TypeNotDefinedxception>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "TypeNotDefined.md")));
    }

    [Test]
    public void UnexpectedFieldName()
    {
      Assert.Throws<UnexpectedFieldNameException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "UnexpectedField.md")));
    }

    [Test]
    public void MultipleEntriesInCollectionDefinition()
    {
      Assert.Throws<CollectionWithMultipleEntriesDefined>(() =>
      {
        LoreParser tempParser = new LoreParser();
        tempParser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "CollectionWithMultipleEntriesDefined.yaml"));
      });
    }

    [Test]
    public void HeadingLevelErrorException()
    {
      Assert.Throws<HeadingLevelErrorException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "HeadingLevelErrorException.md")));
    }

    [Test]
    public void TagParsingException()
    {
      Assert.Throws<LoreTagParsingException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "ImproperlyFormattedHTMLTag.md")));
    }

    [Test]
    public void NestedAttributesOnSingleVal()
    {
      Assert.Throws<NestedBulletsOnSingleValueChildlessAttributeException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "NestedBulletsOnSingle.md")));
    }

    [Test]
    public void UnexpectedTagType()
    {
      Assert.Throws<UnexpectedTagTypeException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "UnexpectedTagType.md")));
    }

    [Test]
    public void PicklistNameNotGiven()
    {
      Assert.Throws<FieldPicklistNameNotGivenException>(() =>
      {
        LoreParser tempParser = new LoreParser();
        tempParser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "PicklistNameNotGiven.yaml"));
      });
    }

    [Test]
    public void UnexpectedSectionName()
    {
      Assert.Throws<UnexpectedSectionNameException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "UnexpectedSectionName.md")));
    }

    [Test]
    public void EmbdNodeTypeNotGiven()
    {
      LoreParser tempParser = new LoreParser();
      Assert.Throws<EmbeddedTypeNotGivenException>(() => tempParser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "EmbeddedTypeNodeGiven.yaml")));
    }
  }
}
