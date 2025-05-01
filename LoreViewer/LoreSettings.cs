using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer
{
  internal class LoreSettings
  {
    public Dictionary<string, LoreTypeDefinition> Types { get; set; }
    public Dictionary<string, LoreCollectionDefinition> Collections { get; set; }
    public AppSettings Settings { get; set; }

    public LoreSettings()
    {
      Types = new Dictionary<string, LoreTypeDefinition>();
      Collections = new Dictionary<string, LoreCollectionDefinition>();
      Settings = new AppSettings();
    }
  }

  public class LoreTypeDefinition
  {
    public string Folder { get; set; } = string.Empty;
    public bool PrimaryHeading { get; set; }
    public List<LoreFieldDefinition> Fields { get; set; } = new List<LoreFieldDefinition>();
  }

  public class LoreFieldDefinition
  {
    public string Name { get; set; } = string.Empty;
    public string Style { get; set; } = string.Empty; // bullet_point, bullet_list, heading_paragraph
  }

  public class LoreCollectionDefinition
  {
    public string File { get; set; } = string.Empty;
    public string EntryStyle { get; set; } = string.Empty;
    public bool SortEntries { get; set; }
  }

  public class AppSettings
  {
    public bool IgnoreCase { get; set; }
    public bool SoftLinking { get; set; }
    public string DefaultSort { get; set; } = string.Empty;
    public List<string> MarkdownExtensions { get; set; } = new List<string>();
  }
}
