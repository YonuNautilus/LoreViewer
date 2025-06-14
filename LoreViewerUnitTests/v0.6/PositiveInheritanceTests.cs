using NUnit.Framework.Internal;

namespace v0_6.PositiveTests
{
  public class PositiveInheritanceTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.6", "TestData", "PositiveTestData");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "SettingsWithNewInheritance.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    public void PositiveFieldTest()
    {
      LoreSettings settings = _parser.Settings;
      Assert.That(settings.types, Has.Count.EqualTo(2));


      LoreTypeDefinition typea = settings.types[0];
      LoreTypeDefinition typeb = settings.types[1];

      Assert.That(typea.fields, Has.Count.EqualTo(3));
      Assert.That(typeb.fields, Has.Count.EqualTo(3));


      Assert.That(typeb.Base, Is.EqualTo(typea));

      LoreFieldDefinition fielda = typea.fields[0];
      LoreFieldDefinition fieldb = typeb.fields[0];

      Assert.That(fielda.style, Is.EqualTo(EFieldStyle.SingleValue));
      Assert.That(fieldb.style, Is.EqualTo(EFieldStyle.NestedValues));

      Assert.That(fielda.fields, Is.Null);
      Assert.That(fieldb.fields, Is.Not.Null);

      Assert.That(fielda, Is.Not.SameAs(fieldb));
      Assert.That(fielda, Is.SameAs(fieldb.Base as LoreFieldDefinition));

      Assert.That(fieldb.fields[0].Base, Is.Null);

      Assert.That(typea.fields[1], Is.Not.Null);
      Assert.That(typea.fields[1].Base, Is.Null);
      Assert.That(typeb.fields[1], Is.Not.Null);
      Assert.That(typeb.fields[1].Base, Is.Not.Null.And.SameAs(typea.fields[1]));
      Assert.That(typea.fields[1].required, Is.False);
      Assert.That(typeb.fields[1].required, Is.True);
      Assert.That(typea.fields[1].style, Is.EqualTo(EFieldStyle.MultiValue));
      Assert.That(typeb.fields[1].style, Is.EqualTo(EFieldStyle.Textual));

      Assert.That(typea.fields[2], Is.Not.SameAs(typeb.fields[2]));
      Assert.That(typea.fields[2].Base, Is.Null);
      Assert.That(typeb.fields[2].Base, Is.SameAs(typea.fields[2]).And.Not.Null);
    }

    [Test]
    public void PositiveSectionTest()
    {
      LoreSettings settings = _parser.Settings;

      LoreTypeDefinition typea = settings.types[0];
      LoreTypeDefinition typeb = settings.types[1];

      Assert.That(typea.sections, Is.Not.Null);
      Assert.That(typea.sections, Has.Count.EqualTo(1));
      Assert.That(typeb.sections, Is.Not.Null);
      Assert.That(typeb.sections, Has.Count.EqualTo(1));

      LoreSectionDefinition seca = typea.sections[0];
      LoreSectionDefinition secb = typeb.sections[0];

      Assert.That(secb.Base, Is.SameAs(seca).And.Not.Null);
      Assert.That(seca.Base, Is.Null);

      Assert.That(seca.HasFields, Is.True);
      Assert.That(secb.HasFields, Is.True);
      Assert.That(seca.fields, Has.Count.EqualTo(1));
      Assert.That(secb.fields, Has.Count.EqualTo(2));

      Assert.That(seca.fields[0].Base, Is.Null);
      Assert.That(secb.fields[0], Is.Not.Null.And.Not.SameAs(seca.fields[0]));
      Assert.That(secb.fields[0].Base, Is.Not.Null.And.SameAs(seca.fields[0]));

      Assert.That(secb.fields[1].Base, Is.Null);
    }

    [Test]
    public void PositiveCollectionTest()
    {
      LoreSettings settings = _parser.Settings;

      LoreTypeDefinition typea = settings.types[0];
      LoreTypeDefinition typeb = settings.types[1];

      Assert.That(typea.collections, Is.Not.Null);
      Assert.That(typea.collections, Has.Count.EqualTo(1));
      Assert.That(typeb.collections, Is.Not.Null);
      Assert.That(typeb.collections, Has.Count.EqualTo(2));

      LoreCollectionDefinition cola = typea.collections[0];
      LoreCollectionDefinition colb = typeb.collections[0];
      LoreCollectionDefinition colb2 = typeb.collections[1];

      Assert.That(colb.Base, Is.SameAs(cola).And.Not.Null);
      Assert.That(cola.Base, Is.Null);

      Assert.That(cola.IsCollectionOfCollections, Is.False);
      Assert.That(cola.ContainedType, Is.Not.Null.And.SameAs(typeb).And.SameAs(colb.ContainedType));
      Assert.That(colb.IsCollectionOfCollections, Is.False);
      Assert.That(colb2.IsCollectionOfCollections, Is.False);
      Assert.That(colb2.ContainedType, Is.Not.Null.And.SameAs(typea));

      /*
      Assert.That(cola.fields[0].Base, Is.Null);
      Assert.That(colb.fields[0], Is.Not.Null.And.Not.SameAs(cola.fields[0]));
      Assert.That(colb.fields[0].Base, Is.Not.Null.And.SameAs(cola.fields[0]));

      Assert.That(colb.fields[1].Base, Is.Null);
      */
    }
  }
}
