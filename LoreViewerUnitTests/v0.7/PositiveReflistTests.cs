using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.LoreElements.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.PositiveReflistTests
{
  [TestFixture]
  internal class PositiveReflistTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "PositiveReflistData");


    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
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

      ILoreNode firstNode  = _parser.GetNode("First Node");
      ILoreNode secondNode = _parser.GetNode("Second Node");
      ILoreNode thirdNode  = _parser.GetNode("Third Node");

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

      // TODO: Make sure when Reference resolution is added, that we make sure these values go to the NODES
    }
  }
}
