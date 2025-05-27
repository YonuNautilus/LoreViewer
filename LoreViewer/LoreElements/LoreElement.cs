using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreElements
{
  /// <summary>
  /// Abstract base class for all Lore elements. Simply implements name, source file path
  /// and starting block index of the MarkdownDocument
  /// </summary>
  public abstract class LoreElement
  {
    public string Name {  get; set; } = string.Empty;

    public string SourcePath = string.Empty;
    public int BlockIndex;
    public Guid Id { get; set; }
  }

  /// <summary>
  /// Abstract class for a LoreElement that can contain plain text, ie 'narrative text'
  /// </summary>
  public abstract class LoreNarrativeElement : LoreElement
  {
    public string Summary { get; set; } = string.Empty;

    public void AddNarrativeText(string textToAdd)
    {
      if (String.IsNullOrWhiteSpace(Summary))
        Summary = textToAdd;
      else Summary += "\r\n" + textToAdd;
    }

    public bool HasNarrativeText => Summary != string.Empty;
  }
}
