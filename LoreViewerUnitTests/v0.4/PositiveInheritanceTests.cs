using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using LoreViewer.Domain.Settings.Definitions;

namespace v0_4.PositiveTests
{
  [TestFixture]
  [TestOf(typeof(LoreSettings))]
  internal class PositiveInheritanceTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;

    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.4", "TestData", "PositiveTestData");

    static string[] testFiles => new string[] { "Inheritance.md", "InheritanceAndEmbedded.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public static void SetupLoreSettings()
    {
      _parser = new ParserService();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _settings = _parser.Settings;

      foreach (string fileToTest in testFiles)
        _parser.ParseSingleFile(fileToTest);
    }

    [Test]
    public void Settings_BaseCounts()
    {
      Assert.That(_settings.types, Has.Count.EqualTo(12));
      Assert.That(_settings.collections, Has.Count.EqualTo(0));
    }

    [Test]
    public void Settings_SectionsDefinintionsCount()
    {
      // Sections
      Assert.That(_settings.GetTypeDefinition("TypeBase").sections, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("TypeChild").sections, Has.Count.EqualTo(2));
      Assert.That(_settings.GetTypeDefinition("TypeGrandchild").sections, Has.Count.EqualTo(3));
      Assert.That(_settings.GetTypeDefinition("Uncle").sections, Has.Count.EqualTo(2));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem2").sections, Has.Count.EqualTo(2));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections, Has.Count.EqualTo(3));
      Assert.That(_settings.GetTypeDefinition("CollectionItem4").sections, Has.Count.EqualTo(2));

      // Subsections
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections[0].sections, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections[0].sections[0].name, Is.EqualTo("CI1 Subinfo"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].sections, Has.Count.EqualTo(2));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].sections[0].name, Is.EqualTo("CI1 Subinfo"));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].sections[1].name, Is.EqualTo("CI1 Subinfo 2"));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].sections[0].sections, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].sections[0].sections[0].name, Is.EqualTo("CI1 Subsubinfo"));
    }

    [Test]
    public void Settings_FieldsDefinitionsCount()
    {
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").GetSectionDefinition("CI1 Info").fields,
        Is.EqualTo(_settings.GetTypeDefinition("CollectionItem1").sections[0].fields));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections[0].fields, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections[0].fields[0].name, Is.EqualTo("CI1 Field"));
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").sections[0].fields[0].multivalue, Is.EqualTo(false));
      Assert.That(_settings.GetTypeDefinition("CollectionItem1").fields, Is.Null);

      Assert.That(_settings.GetTypeDefinition("CollectionItem2").sections[0].fields, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem2").sections[0].fields[0].name, Is.EqualTo("CI1 Field"));
      // This used to be true -- but now it is false, because we don't allow cardinality overrides for inherited fields anymore
      Assert.That(_settings.GetTypeDefinition("CollectionItem2").sections[0].fields[0].multivalue, Is.EqualTo(false));
      Assert.That(_settings.GetTypeDefinition("CollectionItem2").fields, Is.Null);

      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].fields, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].fields[0].name, Is.EqualTo("CI1 Field"));
      // This used to be true -- but now it is false, because we don't allow cardinality overrides for inherited fields anymore
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").sections[0].fields[0].multivalue, Is.EqualTo(false));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").fields, Has.Count.EqualTo(1));
      Assert.That(_settings.GetTypeDefinition("CollectionItem3").fields[0].name, Is.EqualTo("CI3 Field"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem4").fields, Is.Null);
    }

    [Test]
    public void Settings_FamilyInheritance()
    {
      LoreTypeDefinition typeBase = _settings.GetTypeDefinition("TypeBase");
      LoreTypeDefinition typeChild = _settings.GetTypeDefinition("TypeChild");
      LoreTypeDefinition typeGrandchild = _settings.GetTypeDefinition("TypeGrandchild");
      LoreTypeDefinition typeUncle = _settings.GetTypeDefinition("Uncle");

      // Check Inheritance
      Assert.NotNull(typeBase);
      Assert.NotNull(typeChild);
      Assert.NotNull(typeGrandchild);
      Assert.NotNull(typeUncle);

      Assert.Null(typeBase.ParentType);

      Assert.NotNull(typeChild.ParentType);
      Assert.AreSame(typeChild.ParentType, typeBase);

      Assert.NotNull(typeGrandchild.ParentType);
      Assert.That(typeGrandchild.ParentType, Is.EqualTo(typeChild));
      Assert.AreSame(typeGrandchild.ParentType.ParentType, typeChild.ParentType);

      Assert.NotNull(typeUncle.ParentType);
      Assert.AreSame(typeUncle.ParentType, typeBase);
    }

    [Test]
    public void Settings_DoesNotBreak_Grandparent()
    {
      LoreTypeDefinition typeBase = _settings.GetTypeDefinition("TypeBase");
      Assert.NotNull(typeBase);
      // GRANDPARENT:
      // Check Fields
      Assert.That(typeBase.fields, Has.Count.EqualTo(3));
      Assert.NotNull(typeBase.GetFieldDefinition("Awards"));
      Assert.That(typeBase.GetFieldDefinition("Awards").fields, Has.Count.EqualTo(1));
      Assert.NotNull(typeBase.GetFieldDefinition("Awards").GetFieldDefinition("Oscar"));
      Assert.NotNull(typeBase.GetFieldDefinition("Who"));
      Assert.NotNull(typeBase.GetFieldDefinition("What"));
      // Check Sections
      Assert.That(typeBase.sections, Has.Count.EqualTo(1));
      Assert.NotNull(typeBase.GetSectionDefinition("First Section"));
      // Check Collections
      Assert.That(typeBase.collections, Has.Count.EqualTo(1));
      Assert.NotNull(typeBase.GetCollectionDefinition("First Collection"));
      Assert.NotNull(typeBase.GetCollectionDefinition("First Collection").ContainedType);
      Assert.That(typeBase.GetCollectionDefinition("First Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeBase.GetCollectionDefinition("First Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(typeBase.GetCollectionDefinition("First Collection").ContainedType as LoreTypeDefinition));
    }

    [Test]
    public void Settings_DoesNotBreak_Parent()
    {
      LoreTypeDefinition typeChild = _settings.GetTypeDefinition("TypeChild");
      Assert.NotNull(typeChild);
      // GRANDPARENT:
      // Check Fields
      Assert.That(typeChild.fields, Has.Count.EqualTo(5));
      Assert.NotNull(typeChild.GetFieldDefinition("Awards"));
      Assert.That(typeChild.GetFieldDefinition("Awards").fields, Has.Count.EqualTo(2));
      Assert.NotNull(typeChild.GetFieldDefinition("Awards").GetFieldDefinition("Oscar"));
      Assert.NotNull(typeChild.GetFieldDefinition("Awards").GetFieldDefinition("Tony"));
      Assert.NotNull(typeChild.GetFieldDefinition("Who"));
      Assert.NotNull(typeChild.GetFieldDefinition("What"));
      Assert.NotNull(typeChild.GetFieldDefinition("When"));
      Assert.NotNull(typeChild.GetFieldDefinition("Where"));
      // Check Sections
      Assert.That(typeChild.sections, Has.Count.EqualTo(2));
      Assert.NotNull(typeChild.GetSectionDefinition("First Section"));
      Assert.NotNull(typeChild.GetSectionDefinition("Second Section"));
      // Check Collections
      Assert.That(typeChild.collections, Has.Count.EqualTo(2));
      Assert.NotNull(typeChild.GetCollectionDefinition("First Collection"));
      Assert.NotNull(typeChild.GetCollectionDefinition("First Collection").ContainedType);
      Assert.That(typeChild.GetCollectionDefinition("First Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeChild.GetCollectionDefinition("First Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(typeChild.GetCollectionDefinition("First Collection").ContainedType as LoreTypeDefinition));
      Assert.NotNull(typeChild.GetCollectionDefinition("Second Collection"));
      Assert.NotNull(typeChild.GetCollectionDefinition("Second Collection").ContainedType);
      Assert.That(typeChild.GetCollectionDefinition("Second Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeChild.GetCollectionDefinition("Second Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(typeChild.GetCollectionDefinition("Second Collection").ContainedType as LoreTypeDefinition));
    }

    [Test]
    public void Settings_DoesNotBreak_Uncle()
    {
      LoreTypeDefinition typeUncle = _settings.GetTypeDefinition("Uncle");
      Assert.NotNull(typeUncle);
      // GRANDPARENT:
      // Check Fields
      Assert.That(typeUncle.fields, Has.Count.EqualTo(4));
      Assert.NotNull(typeUncle.GetFieldDefinition("Awards"));
      Assert.That(typeUncle.GetFieldDefinition("Awards").fields, Has.Count.EqualTo(3));
      Assert.NotNull(typeUncle.GetFieldDefinition("Awards").GetFieldDefinition("Oscar"));
      Assert.NotNull(typeUncle.GetFieldDefinition("Awards").GetFieldDefinition("Pulitzer"));
      Assert.NotNull(typeUncle.GetFieldDefinition("Awards").GetFieldDefinition("Nobel"));
      Assert.NotNull(typeUncle.GetFieldDefinition("Who"));
      Assert.NotNull(typeUncle.GetFieldDefinition("What"));
      Assert.NotNull(typeUncle.GetFieldDefinition("How"));
      // Check Sections
      Assert.That(typeUncle.sections, Has.Count.EqualTo(2));
      Assert.NotNull(typeUncle.GetSectionDefinition("First Section"));
      Assert.NotNull(typeUncle.GetSectionDefinition("Stinky Section"));
      // Check Collections
      Assert.That(typeUncle.collections, Has.Count.EqualTo(2));
      Assert.NotNull(typeUncle.GetCollectionDefinition("First Collection"));
      Assert.NotNull(typeUncle.GetCollectionDefinition("First Collection").ContainedType);
      Assert.That(typeUncle.GetCollectionDefinition("First Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeUncle.GetCollectionDefinition("First Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(typeUncle.GetCollectionDefinition("First Collection").ContainedType as LoreTypeDefinition));
      Assert.NotNull(typeUncle.GetCollectionDefinition("Stinky Collection"));
      Assert.NotNull(typeUncle.GetCollectionDefinition("Stinky Collection").ContainedType);
      Assert.That(typeUncle.GetCollectionDefinition("Stinky Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeUncle.GetCollectionDefinition("Stinky Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem4"), Is.SameAs(typeUncle.GetCollectionDefinition("Stinky Collection").ContainedType as LoreTypeDefinition));
    }

    [Test]
    public void Settings_DoesNotBreak_Child()
    {
      LoreTypeDefinition typeGrandchild = _settings.GetTypeDefinition("TypeGrandchild");
      Assert.NotNull(typeGrandchild);
      // GRANDPARENT:
      // Check Fields
      Assert.That(typeGrandchild.fields, Has.Count.EqualTo(6));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Awards"));
      Assert.That(typeGrandchild.GetFieldDefinition("Awards").fields, Has.Count.EqualTo(3));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Awards").GetFieldDefinition("Oscar"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Awards").GetFieldDefinition("Tony"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Awards").GetFieldDefinition("Emmy"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Who"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("What"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("When"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Where"));
      Assert.NotNull(typeGrandchild.GetFieldDefinition("Why"));
      // Check Sections
      Assert.That(typeGrandchild.sections, Has.Count.EqualTo(3));
      Assert.NotNull(typeGrandchild.GetSectionDefinition("First Section"));
      Assert.NotNull(typeGrandchild.GetSectionDefinition("Second Section"));
      Assert.NotNull(typeGrandchild.GetSectionDefinition("Third Section"));
      // Check Collections
      Assert.That(typeGrandchild.collections, Has.Count.EqualTo(3));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("First Collection"));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("First Collection").ContainedType);
      Assert.That(typeGrandchild.GetCollectionDefinition("First Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeGrandchild.GetCollectionDefinition("First Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(typeGrandchild.GetCollectionDefinition("First Collection").ContainedType as LoreTypeDefinition));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("Second Collection"));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("Second Collection").ContainedType);
      Assert.That(typeGrandchild.GetCollectionDefinition("Second Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeGrandchild.GetCollectionDefinition("Second Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(typeGrandchild.GetCollectionDefinition("Second Collection").ContainedType as LoreTypeDefinition));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("Third Collection"));
      Assert.NotNull(typeGrandchild.GetCollectionDefinition("Third Collection").ContainedType);
      Assert.That(typeGrandchild.GetCollectionDefinition("Third Collection").ContainedType, Is.TypeOf<LoreTypeDefinition>());
      Assert.False(typeGrandchild.GetCollectionDefinition("Third Collection").IsCollectionOfCollections);
      Assert.That(_settings.GetTypeDefinition("CollectionItem3"), Is.SameAs(typeGrandchild.GetCollectionDefinition("Third Collection").ContainedType as LoreTypeDefinition));
    }


    [Test]
    public void Parsed_BaseCounts()
    {
      Assert.That(_parser.Nodes, Has.Count.EqualTo(5));
      Assert.That(_parser.Collections, Has.Count.EqualTo(0));
    }

    [Test]
    public void Parsed_InheritanceTest()
    {
      LoreNode grandmaINode = _parser.GetNodeByName("Grandparent");
      Assert.NotNull(grandmaINode);
      Assert.That(grandmaINode, Is.TypeOf<LoreNode>());
      LoreNode grandma = grandmaINode as LoreNode;
      Assert.That(grandma.Definition, Is.TypeOf<LoreTypeDefinition>());
      Assert.That(grandma.DefinitionAs<LoreTypeDefinition>().name, Is.EqualTo("TypeBase"));
      Assert.That(grandma.DefinitionAs<LoreTypeDefinition>().ParentType, Is.Null);

      LoreNode parentINode = _parser.GetNodeByName("Parent");
      Assert.NotNull(parentINode);
      Assert.That(parentINode, Is.TypeOf<LoreNode>());
      LoreNode parent = parentINode as LoreNode;
      Assert.That(parent.Definition, Is.TypeOf<LoreTypeDefinition>());
      Assert.That(parent.DefinitionAs<LoreTypeDefinition>().name, Is.EqualTo("TypeChild"));
      Assert.That(parent.DefinitionAs<LoreTypeDefinition>().ParentType, Is.Not.Null);
      Assert.That(parent.DefinitionAs<LoreTypeDefinition>().ParentType, Is.EqualTo(grandma.DefinitionAs<LoreTypeDefinition>()));

      LoreNode childINode = _parser.GetNodeByName("Daughter");
      Assert.NotNull(childINode);
      Assert.That(childINode, Is.TypeOf<LoreNode>());
      LoreNode child = childINode as LoreNode;
      Assert.That(child.Definition, Is.TypeOf<LoreTypeDefinition>());
      Assert.That(child.DefinitionAs<LoreTypeDefinition>().name, Is.EqualTo("TypeGrandchild"));
      Assert.That(child.DefinitionAs<LoreTypeDefinition>().ParentType, Is.Not.Null);
      Assert.That(child.DefinitionAs<LoreTypeDefinition>().ParentType, Is.EqualTo(parent.DefinitionAs<LoreTypeDefinition>()));
      Assert.That(child.DefinitionAs<LoreTypeDefinition>().ParentType.ParentType, Is.EqualTo(parent.DefinitionAs<LoreTypeDefinition>().ParentType));

      LoreNode uncleINode = _parser.GetNodeByName("Uncle");
      Assert.NotNull(uncleINode);
      Assert.That(uncleINode, Is.TypeOf<LoreNode>());
      LoreNode uncle = uncleINode as LoreNode;
      Assert.That(uncle.Definition, Is.TypeOf<LoreTypeDefinition>());
      Assert.That(uncle.DefinitionAs<LoreTypeDefinition>().name, Is.EqualTo("Uncle"));
      Assert.That(uncle.DefinitionAs<LoreTypeDefinition>().ParentType, Is.Not.Null);
      Assert.That(uncle.DefinitionAs<LoreTypeDefinition>().ParentType, Is.EqualTo(grandma.DefinitionAs<LoreTypeDefinition>()));
    }

    [Test]
    public void Parsed_FieldsTest_Grandparent()
    {
      LoreNode grandmaNode = _parser.GetNodeByName("Grandparent") as LoreNode;

      Assert.That(grandmaNode.Attributes, Has.Count.EqualTo(3));
      Assert.NotNull(grandmaNode.GetAttribute("Awards"));
      Assert.NotNull(grandmaNode.GetAttribute("Awards").Attributes);
      Assert.That(grandmaNode.GetAttribute("Awards").Attributes, Has.Count.EqualTo(1));
      Assert.That(grandmaNode.GetAttribute("Awards").GetAttribute("Oscar").Value.ValueString, Is.EqualTo("Nominated"));

      Assert.NotNull(grandmaNode.GetAttribute("Who"));
      Assert.That(grandmaNode.GetAttribute("Who").Value.ValueString, Is.EqualTo("Grandma"));
      Assert.That(grandmaNode.GetAttribute("What").Value.ValueString, Is.EqualTo("A person"));
    }

    [Test]
    public void Parsed_SectionsTest_Grandparent()
    {
      LoreNode grandmaNode = _parser.GetNodeByName("Grandparent") as LoreNode;

      Assert.That(grandmaNode.Sections, Has.Count.EqualTo(1));
      Assert.NotNull(grandmaNode.GetSection("First Section"));
      Assert.That(grandmaNode.GetSection("First Section").Summary, Does.Contain("This is the first section."));
    }

    [Test]
    public void Parsed_CollectionsTest_Grandparent()
    {
      LoreNode grandmaNode = _parser.GetNodeByName("Grandparent") as LoreNode;

      Assert.That(grandmaNode.Collections, Has.Count.EqualTo(1));
      Assert.NotNull(grandmaNode.GetCollection("First Collection"));
      Assert.That(grandmaNode.GetCollection("First Collection").Summary, Does.Contain("Grandma's Collection"));
      Assert.False(grandmaNode.GetCollection("First Collection").ContainsCollections);
      Assert.That(grandmaNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((grandmaNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem1"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(grandmaNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));

      Assert.That(grandmaNode.GetCollection("First Collection"), Has.Count.EqualTo(2));

      LoreNode collectionNode = grandmaNode.GetCollection("First Collection").GetNode("Decorative Spoon");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("It's just a decortive spoon."));

      collectionNode = grandmaNode.GetCollection("First Collection").GetNode("Rusty Spoon");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("Grandma refuses to throw it away"));
    }



    [Test]
    public void Parsed_FieldsTest_Parent()
    {
      LoreNode parentNode = _parser.GetNodeByName("Parent") as LoreNode;

      Assert.That(parentNode.Attributes, Has.Count.EqualTo(5));
      Assert.NotNull(parentNode.GetAttribute("Awards"));
      Assert.NotNull(parentNode.GetAttribute("Awards").Attributes);
      Assert.That(parentNode.GetAttribute("Awards").Attributes, Has.Count.EqualTo(2));
      Assert.That(parentNode.GetAttribute("Awards").GetAttribute("Oscar").Value.ValueString, Is.EqualTo("Achieved"));
      Assert.That(parentNode.GetAttribute("Awards").GetAttribute("Tony").Value.ValueString, Is.EqualTo("Achieved"));

      Assert.NotNull(parentNode.GetAttribute("Who"));
      Assert.That(parentNode.GetAttribute("Who").Value.ValueString, Is.EqualTo("Mother"));
      Assert.That(parentNode.GetAttribute("What").Value.ValueString, Is.EqualTo("A Robot"));
      Assert.That(parentNode.GetAttribute("When").Value.ValueString, Is.EqualTo("Never"));
      Assert.That(parentNode.GetAttribute("Where").Value.ValueString, Is.EqualTo("Kentucky"));
    }

    [Test]
    public void Parsed_SectionsTest_Parent()
    {
      LoreNode parentNode = _parser.GetNodeByName("Parent") as LoreNode;

      Assert.That(parentNode.Sections, Has.Count.EqualTo(2));
      Assert.NotNull(parentNode.GetSection("First Section"));
      Assert.That(parentNode.GetSection("First Section").Summary, Does.Contain("Inherited from Grandma"));
      Assert.NotNull(parentNode.GetSection("Second Section"));
      Assert.That(parentNode.GetSection("Second Section").Summary, Does.Contain("Mom's own section"));
    }

    [Test]
    public void Parsed_CollectionsTest_Parent()
    {
      LoreNode parentNode = _parser.GetNodeByName("Parent") as LoreNode;

      Assert.That(parentNode.Collections, Has.Count.EqualTo(2));
      Assert.NotNull(parentNode.GetCollection("First Collection"));
      Assert.That(parentNode.GetCollection("First Collection").Summary, Does.Contain("Mom's Collection"));
      Assert.False(parentNode.GetCollection("First Collection").ContainsCollections);
      Assert.That(parentNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((parentNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem1"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(parentNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));


      Assert.That(parentNode.GetCollection("First Collection"), Has.Count.EqualTo(2));

      LoreNode collectionNode = parentNode.GetCollection("First Collection").GetNode("A Silver Platter");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("A platter made of silver"));

      collectionNode = parentNode.GetCollection("First Collection").GetNode("Depression Glass");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("This doesn't make me feel good."));



      Assert.NotNull(parentNode.GetCollection("Second Collection"));
      Assert.IsEmpty(parentNode.GetCollection("Second Collection").Summary);
      Assert.False(parentNode.GetCollection("Second Collection").ContainsCollections);
      Assert.That(parentNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That(((LoreTypeDefinition)parentNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType).name,
        Is.EqualTo("CollectionItem2"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(parentNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));
      Assert.That(parentNode.GetCollection("Second Collection"), Has.Count.EqualTo(4));


      collectionNode = parentNode.GetCollection("Second Collection").GetNode("Something Old");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(2));
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("It's gross..."));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("It's old..."));


      collectionNode = parentNode.GetCollection("Second Collection").GetNode("Something New");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("I don't know"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("It's new..."));


      collectionNode = parentNode.GetCollection("Second Collection").GetNode("Something Borrowed");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("Hey, that was mine!"));


      collectionNode = parentNode.GetCollection("Second Collection").GetNode("Something Blue");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("It's blue..."));

    }




    [Test]
    public void Parsed_FieldsTest_Child()
    {
      LoreNode childNode = _parser.GetNodeByName("Daughter") as LoreNode;

      Assert.That(childNode.Attributes, Has.Count.EqualTo(6));
      Assert.NotNull(childNode.GetAttribute("Awards"));
      Assert.NotNull(childNode.GetAttribute("Awards").Attributes);
      Assert.That(childNode.GetAttribute("Awards").Attributes, Has.Count.EqualTo(3));
      Assert.That(childNode.GetAttribute("Awards").GetAttribute("Oscar").Value.ValueString, Is.EqualTo("Lost"));
      Assert.That(childNode.GetAttribute("Awards").GetAttribute("Tony").Value.ValueString, Is.EqualTo("Lost"));
      Assert.That(childNode.GetAttribute("Awards").GetAttribute("Emmy").Value.ValueString, Is.EqualTo("Nominated"));

      Assert.That(childNode.GetAttribute("Who").Value.ValueString, Is.EqualTo("Child"));
      Assert.That(childNode.GetAttribute("What").Value.ValueString, Is.EqualTo("Fleshy blob"));
      Assert.That(childNode.GetAttribute("When").Value.ValueString, Is.EqualTo("Always"));
      Assert.That(childNode.GetAttribute("Where").Value.ValueString, Is.EqualTo("North Dakota"));
      Assert.That(childNode.GetAttribute("Why").Value.ValueString, Is.EqualTo("No particular reason"));
    }

    [Test]
    public void Parsed_SectionsTest_Child()
    {
      LoreNode childNode = _parser.GetNodeByName("Daughter") as LoreNode;

      Assert.That(childNode.Sections, Has.Count.EqualTo(3));
      Assert.NotNull(childNode.GetSection("First Section"));
      Assert.That(childNode.GetSection("First Section").Summary, Does.Contain("Inherited from Grandma through mom"));
      Assert.NotNull(childNode.GetSection("Second Section"));
      Assert.That(childNode.GetSection("Second Section").Summary, Does.Contain("Inherited from mom."));
      Assert.NotNull(childNode.GetSection("Third Section"));
      Assert.That(childNode.GetSection("Third Section").Summary, Does.Contain("Baby's first section"));
    }


    [Test]
    public void Parsed_CollectionsTest_Child()
    {
      LoreNode childNode = _parser.GetNodeByName("Daughter") as LoreNode;

      Assert.That(childNode.Collections, Has.Count.EqualTo(3));
      Assert.NotNull(childNode.GetCollection("First Collection"));
      Assert.That(childNode.GetCollection("First Collection").Summary, Does.Contain("Child's Collection"));
      Assert.False(childNode.GetCollection("First Collection").ContainsCollections);
      Assert.That(childNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((childNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem1"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(childNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));


      Assert.That(childNode.GetCollection("First Collection"), Has.Count.EqualTo(2));

      LoreNode collectionNode = childNode.GetCollection("First Collection").GetNode("A Green Slime");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("A slime that's green."));

      collectionNode = childNode.GetCollection("First Collection").GetNode("A Blue Slime");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("A slime that's blue."));



      Assert.NotNull(childNode.GetCollection("Second Collection"));
      Assert.IsEmpty(childNode.GetCollection("Second Collection").Summary);
      Assert.False(childNode.GetCollection("Second Collection").ContainsCollections);
      Assert.That(childNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((childNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem2"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(childNode.GetCollection("Second Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));
      Assert.That(childNode.GetCollection("Second Collection"), Has.Count.EqualTo(4));


      collectionNode = childNode.GetCollection("Second Collection").GetNode("Education");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("Never went to Oovoo Javer"));


      collectionNode = childNode.GetCollection("Second Collection").GetNode("Employment");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("Now works at Oovoo Javer"));


      collectionNode = childNode.GetCollection("Second Collection").GetNode("Hobbies");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("Loves Oovoo Javer"));


      collectionNode = childNode.GetCollection("Second Collection").GetNode("Dislikes");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem2"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("I hate being away from Oovoo Javer"));




      Assert.NotNull(childNode.GetCollection("Third Collection"));
      Assert.IsEmpty(childNode.GetCollection("Third Collection").Summary);
      Assert.False(childNode.GetCollection("Third Collection").ContainsCollections);
      Assert.That(childNode.GetCollection("Third Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((childNode.GetCollection("Third Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem3"));

      Assert.AreSame(childNode.GetCollection("Third Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, _settings.GetTypeDefinition("CollectionItem3"));
      Assert.That(childNode.GetCollection("Third Collection"), Has.Count.EqualTo(2));


      collectionNode = childNode.GetCollection("Third Collection").GetNode("D20");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem3"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(2));
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.Null(collectionNode.GetSection("CI2 Info"));
      Assert.NotNull(collectionNode.GetSection("CI3 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("I don't know"));
      Assert.That(collectionNode.GetSection("CI3 Info").Summary, Does.Contain("Rolled a 6"));


      collectionNode = childNode.GetCollection("Third Collection").GetNode("D6");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem3"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(3));
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI2 Info"));
      Assert.NotNull(collectionNode.GetSection("CI3 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("Just testing inheritance"));
      Assert.That(collectionNode.GetSection("CI2 Info").Summary, Does.Contain("I don't know"));
      Assert.That(collectionNode.GetSection("CI3 Info").Summary, Does.Contain("Also rolled a 6"));
    }




    [Test]
    public void Parsed_FieldsTest_Uncle()
    {
      LoreNode UncleNode = _parser.GetNodeByName("Uncle") as LoreNode;

      Assert.That(UncleNode.Attributes, Has.Count.EqualTo(4));
      Assert.NotNull(UncleNode.GetAttribute("Awards"));
      Assert.NotNull(UncleNode.GetAttribute("Awards").Attributes);
      Assert.That(UncleNode.GetAttribute("Awards").Attributes, Has.Count.EqualTo(2));
      Assert.That(UncleNode.GetAttribute("Awards").GetAttribute("Oscar").Value.ValueString, Is.EqualTo("Won"));
      Assert.That(UncleNode.GetAttribute("Awards").GetAttribute("Pulitzer").Value.ValueString, Is.EqualTo("Won"));

      Assert.NotNull(UncleNode.GetAttribute("Who"));
      Assert.That(UncleNode.GetAttribute("Who").Value.ValueString, Is.EqualTo("Uncle"));
      Assert.That(UncleNode.GetAttribute("What").Value.ValueString, Is.EqualTo("An Uncle"));
      Assert.That(UncleNode.GetAttribute("How").Value.ValueString, Is.EqualTo("Nobody Knows"));
    }

    [Test]
    public void Parsed_SectionsTest_Uncle()
    {
      LoreNode UncleNode = _parser.GetNodeByName("Uncle") as LoreNode;

      Assert.That(UncleNode.Sections, Has.Count.EqualTo(2));
      Assert.NotNull(UncleNode.GetSection("First Section"));
      Assert.That(UncleNode.GetSection("First Section").Summary, Does.Contain("This is the first section."));
      Assert.Null(UncleNode.GetSection("Second Section"));
      Assert.NotNull(UncleNode.GetSection("Stinky Section"));
      Assert.That(UncleNode.GetSection("Stinky Section").Summary, Does.Contain("Uncle has an appreciation for unpleasant odors.\r\n\r\nHe's a strange guy..."));
    }

    [Test]
    public void Parsed_CollectionsTest_Uncle()
    {
      LoreNode UncleNode = _parser.GetNodeByName("Uncle") as LoreNode;

      Assert.That(UncleNode.Collections, Has.Count.EqualTo(2));
      Assert.NotNull(UncleNode.GetCollection("First Collection"));
      Assert.That(UncleNode.GetCollection("First Collection").Summary, Does.Contain("Uncle's Collection"));
      Assert.False(UncleNode.GetCollection("First Collection").ContainsCollections);
      Assert.That(UncleNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((UncleNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem1"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(UncleNode.GetCollection("First Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));

      Assert.That(UncleNode.GetCollection("First Collection").Summary, Is.EqualTo("Uncle's Collection."));


      Assert.That(UncleNode.GetCollection("First Collection"), Has.Count.EqualTo(3));

      LoreNode collectionNode = UncleNode.GetCollection("First Collection").GetNode("Rotten Banana Peel");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("It's slippery and stinky"));

      collectionNode = UncleNode.GetCollection("First Collection").GetNode("Skunks Under The Shed");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("They make it extra stinky"));

      collectionNode = UncleNode.GetCollection("First Collection").GetNode("Corpse Flower");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem1"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("You don't need to see it to know it's blooming."));



      Assert.NotNull(UncleNode.GetCollection("Stinky Collection"));
      Assert.IsEmpty(UncleNode.GetCollection("Stinky Collection").Summary);
      Assert.False(UncleNode.GetCollection("Stinky Collection").ContainsCollections);
      Assert.That(UncleNode.GetCollection("Stinky Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType, Is.TypeOf<LoreTypeDefinition>());

      Assert.That((UncleNode.GetCollection("Stinky Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType as LoreTypeDefinition).name,
        Is.EqualTo("CollectionItem4"));

      Assert.That(_settings.GetTypeDefinition("CollectionItem4"), Is.SameAs(UncleNode.GetCollection("Stinky Collection").DefinitionAs<LoreCollectionDefinition>().ContainedType));
      Assert.That(UncleNode.GetCollection("Stinky Collection"), Has.Count.EqualTo(3));


      collectionNode = UncleNode.GetCollection("Stinky Collection").GetNode("Manure Pile");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem4"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(2));
      Assert.NotNull(collectionNode.GetSection("CI1 Info"));
      Assert.Null(collectionNode.GetSection("CI2 Info"));
      Assert.NotNull(collectionNode.GetSection("CI4 Info"));
      Assert.That(collectionNode.GetSection("CI1 Info").Summary, Does.Contain("Making sure CollectionItem4 still gets the CollectionItem1 sections"));
      Assert.That(collectionNode.GetSection("CI4 Info").Summary, Does.Contain("I don't need to explain this..."));


      collectionNode = UncleNode.GetCollection("Stinky Collection").GetNode("Pig Farm");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem4"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.NotNull(collectionNode.GetSection("CI4 Info"));
      Assert.That(collectionNode.GetSection("CI4 Info").Summary, Does.Contain("I hear pig slurry is the absolute worst smell."));


      collectionNode = UncleNode.GetCollection("Stinky Collection").GetNode("Open Septic Tank");
      Assert.NotNull(collectionNode);
      Assert.That(_settings.GetTypeDefinition("CollectionItem4"), Is.SameAs(collectionNode.DefinitionAs<LoreTypeDefinition>()));
      Assert.IsEmpty(collectionNode.Summary);

      Assert.NotNull(collectionNode.Sections);
      Assert.That(collectionNode.Sections, Has.Count.EqualTo(1));
      Assert.Null(collectionNode.GetSection("CI1 Info"));
      Assert.Null(collectionNode.GetSection("CI2 Info"));
      Assert.Null(collectionNode.GetSection("CI3 Info"));
      Assert.NotNull(collectionNode.GetSection("CI4 Info"));
      Assert.That(collectionNode.GetSection("CI4 Info").Summary, Does.Contain("I'll pass, thanks."));
    }

    [Test]
    public void Settings_Inher_And_Embed()
    {
      Assert.NotNull(_settings.GetTypeDefinition("BothTestType"));
      Assert.That(_settings.GetTypeDefinition("BothTestType").embeddedNodeDefs, Has.Count.EqualTo(2));

      Assert.NotNull(_settings.GetTypeDefinition("EmbeddedType1"));
      Assert.That(_settings.GetTypeDefinition("EmbeddedType1").embeddedNodeDefs, Has.Count.EqualTo(2));
      Assert.That(_settings.GetTypeDefinition("EmbeddedType1").embeddedNodeDefs[0].nodeType, Is.SameAs(_settings.GetTypeDefinition("EmbeddedType1")));
      Assert.That(_settings.GetTypeDefinition("EmbeddedType1").embeddedNodeDefs[1].nodeType, Is.SameAs(_settings.GetTypeDefinition("EmbeddedType1")));

      Assert.NotNull(_settings.GetTypeDefinition("EmbeddedType2"));
      Assert.That(_settings.GetTypeDefinition("EmbeddedType2"), Is.SameAs(_settings.GetTypeDefinition("BothTestType").embeddedNodeDefs[1].nodeType));

      Assert.NotNull(_settings.GetTypeDefinition("EmbeddedType3"));
      Assert.That(_settings.GetTypeDefinition("EmbeddedType3").ParentType, Is.SameAs(_settings.GetTypeDefinition("EmbeddedType1")));

    }

    [Test]
    public void Parsed_Inher_And_Embed()
    {
      Assert.That(_parser.GetNodeByName("Test Node"), Is.Not.Null);
      Assert.That(_parser.GetNodeByName("Test Node").Nodes, Has.Count.EqualTo(2));

      LoreNode node = (LoreNode)_parser.GetNodeByName("Test Node");

      Assert.NotNull(node.GetNode("First Embedded Node"));
      Assert.That(node.GetNode("First Embedded Node").Nodes, Has.Count.EqualTo(0));

      Assert.NotNull(node.GetNode("Second Embedded Node"));
      Assert.That(node.GetNode("Second Embedded Node").Nodes, Has.Count.EqualTo(2));

      node = node.GetNode("Second Embedded Node");

      Assert.NotNull(node.GetNode("A Deeply Embedded Node Of The Defined Type"));

      Assert.NotNull(node.GetNode("A Deeply Embedded Node Derived From The Defined Type"));

    }
  }
}
