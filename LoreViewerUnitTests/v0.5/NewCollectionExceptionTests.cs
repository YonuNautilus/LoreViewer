using LoreViewer.Exceptions.LoreParsingExceptions;
using NUnit.Framework.Internal;

namespace v0_5.NewCollectionExceptions
{
  public class NewExceptionTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "NewExceptionsTest");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);
    }

    [Test]
    [TestOf(typeof(InvalidTypeInCollectionException))]
    public void BadType()
    {
      Assert.Throws<InvalidTypeInCollectionException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "CollectionExceptionBadType.md")));
    }


    [Test]
    [TestOf(typeof(UnknownTypeInCollectionException))]
    public void UnknownType()
    {
      Assert.Throws<UnknownTypeInCollectionException>(() => _parser.ParseSingleFile(Path.Combine(ValidFilesFolder, "CollectionExceptionUnknownType.md")));
    }
  }
}
