using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Domain.Settings.Definitions;
using System.ComponentModel.DataAnnotations;

namespace v0_7.PositiveReflistTests
{
  [TestFixture]
  internal class PositiveReflistTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "PositiveReflistData");


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
    public void RefListSettings()
    {
      Assert.That(_parser.Settings.types, Has.Count.EqualTo(3));

      LoreTypeDefinition type1 = _parser.Settings.types[0];
      LoreTypeDefinition type2 = _parser.Settings.types[1];
      LoreTypeDefinition type3 = _parser.Settings.types[2];

      Assert.That(type1.fields, Is.Not.Null);
      Assert.That(type1.fields, Has.Count.EqualTo(1));

      Assert.That(type2.fields, Is.Null);

      Assert.That(type3.fields, Is.Not.Null);
      Assert.That(type3.fields, Has.Count.EqualTo(2));

      LoreFieldDefinition field = type1.fields[0];
      LoreFieldDefinition infield = type3.fields[0];
      LoreFieldDefinition ofield = type3.fields[1];

      Assert.That(field.contentType, Is.EqualTo(EFieldContentType.ReferenceList));
      Assert.That(field.RefListType, Is.EqualTo(type2));

      Assert.That(infield.Base, Is.SameAs(field));
      Assert.That(infield.RefListType, Is.EqualTo(type2));

      Assert.That(ofield.RefListType, Is.SameAs(type1));
      Assert.That(type3.IsATypeOf(ofield.RefListType), Is.True);
    }

    [Test]
    public void RefListParsed()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(3));

      LoreNode firstNode = _parser.GetNodeByName("First Node");
      LoreNode secondNode = _parser.GetNodeByName("Second Node");
      LoreNode thirdNode = _parser.GetNodeByName("Third Node");

      Assert.That(firstNode.Attributes, Has.Count.EqualTo(1));
      Assert.That(thirdNode.Attributes, Has.Count.EqualTo(2));

      Assert.That(firstNode.Attributes[0].Value, Is.TypeOf(typeof(ReferenceAttributeValue)));
      Assert.That(thirdNode.Attributes[0].Value, Is.TypeOf(typeof(ReferenceAttributeValue)));
      Assert.That(thirdNode.Attributes[1].HasValue, Is.False);
      Assert.That(thirdNode.Attributes[1].HasValues, Is.True);
      Assert.That(thirdNode.Attributes[1].HasAttributes, Is.False);
      Assert.That(thirdNode.Attributes[1].Values, Has.Count.EqualTo(2));
      Assert.That(thirdNode.Attributes[1].Values[0], Is.TypeOf(typeof(ReferenceAttributeValue)));
      Assert.That(thirdNode.Attributes[1].Values[1], Is.TypeOf(typeof(ReferenceAttributeValue)));

      Assert.That((thirdNode.Attributes[0].Value as ReferenceAttributeValue).Value, Is.SameAs(secondNode));
      Assert.That((thirdNode.Attributes[1].Values[0] as ReferenceAttributeValue).Value, Is.SameAs(firstNode));
      Assert.That((thirdNode.Attributes[1].Values[1] as ReferenceAttributeValue).Value, Is.SameAs(thirdNode));

      // Check that referencing a node by name rather than ID results in a warning.
      Assert.That(_valStore.Result.LoreEntityValidationMessages.ContainsKey(thirdNode.Attributes[1]), Is.True);
      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(thirdNode.Attributes[1]), Is.True);
      Assert.That(_valStore.Result.LoreEntityValidationStates[thirdNode.Attributes[1]], Is.EqualTo(EValidationState.Warning));

      Assert.That(_valStore.Result.LoreEntityValidationMessages.ContainsKey(thirdNode.Attributes[0]), Is.True);
      Assert.That(_valStore.Result.LoreEntityValidationMessages[thirdNode.Attributes[0]], Has.Count.EqualTo(1));
      Assert.That(_valStore.Result.LoreEntityValidationMessages[thirdNode.Attributes[0]][0].Status, Is.EqualTo(EValidationMessageStatus.Warning));
      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(thirdNode.Attributes[0]), Is.True);
      Assert.That(_valStore.Result.LoreEntityValidationStates[thirdNode.Attributes[0]], Is.EqualTo(EValidationState.Warning));

      Assert.That(_valStore.Result.LoreEntityValidationStates.ContainsKey(thirdNode as LoreEntity), Is.True);
      Assert.That(_valStore.Result.LoreEntityValidationStates[thirdNode as LoreEntity], Is.EqualTo(EValidationState.ChildWarning));
    }
  }
}
