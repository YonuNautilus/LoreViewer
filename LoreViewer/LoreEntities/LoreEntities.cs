using DocumentFormat.OpenXml.Bibliography;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Parser;
using LoreViewer.Settings;
using System;

namespace LoreViewer.LoreElements
{
  /// <summary>
  /// The root-level abstract class that all parsed lore information derives from.
  /// Holds a name, definition, and ID (which may be removed later)
  /// </summary>
  public abstract class LoreEntity : ILoreEntity
  {
    public string Name { get; set; } = string.Empty;
    public abstract LoreDefinitionBase Definition { get; set; }

    public virtual T DefinitionAs<T>() where T : LoreDefinitionBase => Definition as T;

    protected LoreTagInfo? tag;

    internal LoreTagInfo? CurrentTag => tag;

    public string ID
    {
      get => tag?.ID ?? $"_{Name}_";
    }

    public LoreEntity(string name, LoreDefinitionBase definition) { Name = name; Definition = definition; }

    public LoreEntity(string name, LoreDefinitionBase definition, LoreTagInfo? tagInfo) : this(name, definition) { tag = tagInfo;}

    public virtual string ErrMsg => $"{Name}";
    public void SetTag(LoreTagInfo? tagInfo) => tag = tagInfo;

    public void SetID(string newID) => tag?.SetID(newID);
  }

  /// <summary>
  /// Abstract base class for all single lore elements.
  /// Holds the file path to the source of the object and the starting block index of the MarkdownDocument
  /// </summary>
  public abstract class LoreElement : LoreEntity
  {
    public LoreElement(string name, LoreDefinitionBase definition) : base(name, definition) { }
    public LoreElement(string name, LoreDefinitionBase definition, string filePath, int blockIndex, int lineNUmber) : base(name, definition)
    {
      SourcePath = filePath;
      BlockIndex = blockIndex;
      LineNumber = lineNUmber;
    }
    public string SourcePath { get; } = string.Empty;
    public int BlockIndex;
    public int LineNumber;

    public override string ErrMsg => $"Go To: {SourcePath}:{LineNumber}";
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
    public LoreNarrativeElement(string name, LoreDefinitionBase definition, string filePath, int blockIndex, int lineNumber) : base(name, definition, filePath, blockIndex, lineNumber) { }
  }
}
