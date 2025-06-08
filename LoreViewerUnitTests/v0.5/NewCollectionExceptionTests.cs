using LoreViewer.Exceptions.LoreParsingExceptions;
using NUnit.Framework.Internal;

namespace v0_5.NewCollectionExceptions
{
  public class NewExceptionTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.5", "TestData", "NewExceptionsTest");
    //static string[] testFiles => new string[] { "FieldsTest.md" }.Select(s => Path.Combine(ValidFilesFolder, s)).ToArray();

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
    }

    [Test]
    public void BadType()
    {
      Assert.Throws<InvalidTypeInCollectionException>(() => _parser.ParseFile(Path.Combine(ValidFilesFolder, "CollectionExceptionBadType.md")));
    }


    [Test]
    public void UnknownType()
    {
      Assert.Throws<UnknownTypeInCollectionException>(() => _parser.ParseFile(Path.Combine(ValidFilesFolder, "CollectionExceptionUnknownType.md")));
    }
  }
}
