using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Settings;

namespace v0_7.PositivePicklistTests
{
  public class PositivePicklistTests
  {
    public static LoreSettings _settings;
    public static ParserService _parser;
    public static LoreRepository _repository;
    public static ValidationService _validator;
    public static ValidationStore _valStore;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "PositivePickListData");

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
    public void NoCrayonParsingErrors()
    {
      Assert.That(_settings.picklists, Has.Count.EqualTo(2));
      Assert.That(_settings.picklists[0].HasEntries, Is.True);
      Assert.That(_settings.picklists[0].entries, Has.Count.EqualTo(3));
      Assert.That(_settings.picklists[0].entries[0].HasEntries, Is.False);
      Assert.That(_settings.picklists[0].entries[1].HasEntries, Is.True);
      Assert.That(_settings.picklists[0].entries[1].entries, Has.Count.EqualTo(3));
      Assert.That(_settings.picklists[0].entries[1].entries[1].HasEntries, Is.True);
      Assert.That(_settings.picklists[0].entries[1].entries[1].entries, Has.Count.EqualTo(3));

      Assert.That(_settings.types, Has.Count.EqualTo(4));

      Assert.That(_parser.Nodes, Has.Count.EqualTo(9));

      Assert.That(_valStore.Result.Errors, Has.Count.EqualTo(0));

      Assert.That(_parser.Nodes.Where(node => node.Definition.name == "Red Crayon").ToList(), Has.Count.EqualTo(3));
      Assert.That(_parser.Nodes.Where(node => node.Definition.name == "Green Crayon").ToList(), Has.Count.EqualTo(3));
      Assert.That(_parser.Nodes.Where(node => node.Definition.name == "Blue Crayon").ToList(), Has.Count.EqualTo(3));
    }

    [Test]
    public void ColorsOnlyUsedOnce()
    {
      string[] colorsToCheck = { "burgundy", "crimson", "maroon", "forest", "lime", "neon", "cyan", "azure", "cobalt" };

      foreach (string color in colorsToCheck)
      {
        Assert.That(_parser.Nodes.Where(n => n.GetAttribute("Shade").Value.ValueString == color).ToList(), Has.Count.EqualTo(1));
      }
    }
  }
}
