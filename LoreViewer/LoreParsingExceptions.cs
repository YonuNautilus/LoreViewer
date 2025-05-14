using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer
{
  public abstract class LoreParsingException : Exception
  {
    public readonly string MarkdownFilePath;

    static string msgBase;


    public readonly int BlockIndex;
    public readonly int LineNumber;
    public string MarkdownFileName => Path.GetFileName(MarkdownFilePath);

    public LoreParsingException(string filePath, int blockIndex, int lineNumber, string msg) : base(msg)
    {
      MarkdownFilePath = filePath;
      BlockIndex = blockIndex;
      LineNumber = lineNumber;
    }
  }

  public abstract class LoreNodeParsingException : LoreParsingException
  {
    public LoreNodeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class NoTagParsingException : LoreNodeParsingException
  {
    static string msgBase = "Error while parsing file {0}: First heading block found is not tagged";
    public NoTagParsingException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, String.Format(msgBase, Path.GetFileName(filePath)))
    { }
  }

  public class FirstHeadingTagException : LoreNodeParsingException
  {
    static string msgBase = "Error while parsing file {Path.GetFileName(filePath)}: First heading MUST define a node or collection";
    public FirstHeadingTagException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, String.Format(msgBase, Path.GetFileName(filePath)))
    { }
  }

  public class UnexpectedBlockException : LoreNodeParsingException
  {
    public UnexpectedBlockException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, $"")
    {

    }
  }

  public class HeadingLevelErrorException : LoreNodeParsingException
  {
    public HeadingLevelErrorException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg)
    {

    }
  }

  /*
   LoreAttributeParsingException
│   ├── MissingFieldDefinitionException
│   ├── InvalidNestedStructureException
│   ├── UnexpectedFlatValueException
│   └── UnexpectedMultiStructureException
   */
  public class LoreAttributeParsingException : LoreParsingException
  {
    public LoreAttributeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }
  public class MissingFieldDefinitionException : LoreAttributeParsingException
  {

    static string msgBase = "no definition found for attribute {0}, file line number {1}";
    public MissingFieldDefinitionException(string filePath, int blockIndex, int lineNumber, string info)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, info, lineNumber))
    {

    }
  }
}
