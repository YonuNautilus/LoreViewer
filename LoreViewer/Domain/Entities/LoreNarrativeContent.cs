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
    public void AddNarrativeLines(LoreNarrativeLine[] lines) => Lines.AddRange(lines);
  }

  public class LoreNarrativeLine
  {
    public List<LoreNarrativeInline> Inlines { get; set; } = new List<LoreNarrativeInline>();

    public void AddNarrativeInline(LoreNarrativeInline inline) => Inlines.Add(inline);
    public void AddNarrativeInlines(LoreNarrativeInline[] inlines) => Inlines.AddRange(inlines);
  }


  #region Inlines
  public abstract class LoreNarrativeInline
  {
  }

  [Flags]
  public enum ETextStyle { Normal = 0, Italics = 1, Oblique = 2, Bold = 4, Strike = 8, Code = 16, Inserted = 32, Marked = 64 }

  public enum ETextAlignment { Base, Subscript, Superscript }

  public class LoreNarrativeTextInline : LoreNarrativeInline
  {
    public string Text { get; set; }

    public ETextStyle TextStyle { get; set; }

    public ETextAlignment TextAlignment { get; set; }

    public LoreNarrativeTextInline(string content) { Text = content; }

    public LoreNarrativeTextInline(string content, ETextStyle style, ETextAlignment alignment = ETextAlignment.Base) : this(content) { TextStyle = style; TextAlignment = alignment; }
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
