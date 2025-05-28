using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using System;

namespace LoreViewer.LoreElements
{
  /// <summary>
  /// The root-level abstract class that all parsed lore information derives from.
  /// Holds a name and an ID (which may be removed later)
  /// </summary>
  public abstract class LoreEntity : ILoreEntity
  {
    public string Name { get; set; } = string.Empty;
    public abstract LoreDefinitionBase Definition { get; set; }
    public Guid Id { get; set; }
    public LoreEntity(String name, LoreDefinitionBase definition) { Name = name; Definition = definition; }
  }

  /// <summary>
  /// Abstract base class for all Lore elements. Simply implements name, source file path
  /// and starting block index of the MarkdownDocument
  /// </summary>
  public abstract class LoreElement : LoreEntity
  {
    public LoreElement(string name, LoreDefinitionBase definition) : base(name, definition) { }
    public string SourcePath = string.Empty;
    public int BlockIndex;
  }

  /// <summary>
  /// Abstract class for a LoreElement that can contain plain text, ie 'narrative text'
  /// </summary>
  public abstract class LoreNarrativeElement : LoreElement
  {
    public string Summary { get; set; } = string.Empty;
    public bool HasNarrativeText => Summary != string.Empty;

    public void AddNarrativeText(string textToAdd)
    {
      if (String.IsNullOrWhiteSpace(Summary))
        Summary = textToAdd;
      else Summary += "\r\n" + textToAdd;
    }

    public LoreNarrativeElement(string name, LoreDefinitionBase definition) : base(name, definition) { }
  }
}
