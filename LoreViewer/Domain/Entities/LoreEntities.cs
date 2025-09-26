using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Settings.Definitions;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Domain.Entities
{
  /// <summary>
  /// The root-level abstract class that all parsed lore information derives from.
  /// Holds a name, definition, and ID (which may be removed later)
  /// </summary>
  public abstract class LoreEntity
  {
    public string Name { get; set; } = string.Empty;
    public abstract LoreDefinitionBase Definition { get; set; }

    public abstract LoreDefinitionBase GetDefinition();

    public virtual T DefinitionAs<T>() where T : LoreDefinitionBase => Definition as T;

    protected LoreTagInfo? tag;

    internal LoreTagInfo? CurrentTag => tag;

    public string ID
    {
      get => tag?.ID ?? $"_{Name}_";
    }

    public LoreEntity(string name, LoreDefinitionBase definition) { Name = name; Definition = definition; }

    public LoreEntity(string name, LoreDefinitionBase definition, LoreTagInfo? tagInfo) : this(name, definition) { tag = tagInfo; }

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
    public LoreElement(string name, LoreDefinitionBase definition, string filePath, int blockIndex, int lineNumber) : base(name, definition)
    {
      Provenance.Add(new Provenance { BlockIndex = blockIndex, LineNumber = lineNumber, SourceFilePath = filePath });
    }
    public string SourcePath { get; } = string.Empty;
    public int BlockIndex;
    public int LineNumber;

    List<Provenance> Provenance { get; } = new List<Provenance>();

    public override string ErrMsg => $"Go To: {SourcePath}:{LineNumber}";
  }

  public sealed record Provenance
  {
    public string SourceFilePath = string.Empty;
    public int BlockIndex = -1;
    public int LineNumber = -1;

  }

  /// <summary>
  /// Abstract class for a LoreElement that can contain plain text, ie 'narrative text'
  /// </summary>
  public abstract class LoreNarrativeElement : LoreElement
  {
    public virtual string Summary { get; set; } = string.Empty;
    public virtual bool HasNarrativeText => Summary != string.Empty;


    public virtual List<LoreNarrativeBlock> NarrativeContent { get; set; } = new List<LoreNarrativeBlock>();
    public virtual bool HasNarrativeContent => NarrativeContent.Any();

    public void AddNarrativeText(string textToAdd)
    {
      if (string.IsNullOrWhiteSpace(Summary))
        Summary = textToAdd;
      else Summary += "\r\n" + textToAdd;
    }

    public void AddNarrativeContent(LoreNarrativeBlock contentBlock) => NarrativeContent.Add(contentBlock);

    public LoreNarrativeElement(string name, LoreDefinitionBase definition) : base(name, definition) { }
    public LoreNarrativeElement(string name, LoreDefinitionBase definition, string filePath, int blockIndex, int lineNumber) : base(name, definition, filePath, blockIndex, lineNumber) { }
  }
}
