using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v0_7.PositiveTests
{
  public class PositivePicklistTests
  {
    public static LoreSettings _settings;
    public static LoreParser _parser;
    static string ValidFilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "v0.7", "TestData", "PositivePickListData");

    [OneTimeSetUp]
    public void Setup()
    {
      _parser = new LoreParser();

      _parser.ParseSettingsFromFile(Path.Combine(ValidFilesFolder, "Lore_Settings.yaml"));

      _parser.BeginParsingFromFolder(ValidFilesFolder);

      _settings = _parser.Settings;
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

      Assert.That(_parser.validator.ValidationResult.Errors, Has.Count.EqualTo(0));

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
