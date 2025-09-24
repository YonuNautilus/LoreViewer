using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Domain.Entities
{
  /* When a LoreNarrativeElement has narrative content (ie text just below the heading that defines the lore element),
   * it should be allowed to contain not just text, but images, hyperlinks, etc. It should also be allowed to span
   * multiple 'parapgraphs.'
   * 
   * Narrative content is organized as such:
   * 
   * Paragraph Blocks (in markdown, separated by two newlines)
   *  └─ Paragraph Line (in markdown, separated by a singe newline)
   *      └─ Paragraph Inline (in markdown, separated by a single space, or nothing, I guess?)
   *      
   * Paragraph Inline can contain a number of things, like text, image, hyperlinks and such.
   * A paragraph can contain just a single lie with a single inline element.
   */

  public class LoreNarrativeBlock
  {
    public List<LoreNarrativeLine> Lines { get; set; } = new List<LoreNarrativeLine>();

    public void AddNarrativeLine(LoreNarrativeLine line) => Lines.Add(line);
  }

  public class LoreNarrativeLine
  {
    public List<LoreNarrativeInline> Inlines { get; set; } = new List<LoreNarrativeInline>();

    public void AddNarrativeInline(LoreNarrativeInline inline) => Inlines.Add(inline);
  }


  #region Inlines
  public abstract class LoreNarrativeInline
  {
  }

  public class LoreNarrativeTextInline : LoreNarrativeInline
  {
    string Text { get; set; }

    public LoreNarrativeTextInline(string content) { Text = content; }
  }

  public class LoreNarrativeImageInline : LoreNarrativeInline
  {
    public string ImagePath { get; set; }
  }

  public class LoreNarrativeLinkInline : LoreNarrativeInline
  {
    public string Path { get; set; }

    public string Label { get; set; }
  }

  #endregion Inlines
}
