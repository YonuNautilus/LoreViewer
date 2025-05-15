using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Exceptions
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
    static string msgBase = "Error while parsing file {0}: First heading MUST define a node or collection";
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

  public class LoreSectionParsingException : LoreParsingException
  {
    public LoreSectionParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
 
    public class UnexpectedSectionNameException : LoreSectionParsingException
    {
      static string msgBase = "Section without definiton found. Section title: {0}; Line number {1}";
      public UnexpectedSectionNameException(string filePath, int blockIndex, int lineNumber, string headingTitle, string subHeadingTitle)
        : base(filePath, blockIndex, lineNumber, string.Format(msgBase, subHeadingTitle, lineNumber)) { }
    }

  }

  /*
   LoreAttributeParsingException
│   ├── UnexpectedFieldNameException
│   ├── InvalidNestedStructureException
│   ├── UnexpectedFlatValueException
│   └── UnexpectedMultiStructureException
   */
  public class LoreAttributeParsingException : LoreParsingException
  {
    public LoreAttributeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }


    public class UnexpectedFieldNameException : LoreAttributeParsingException
    {
      static string msgBase = "No definition found for attribute {0}, file line number {1}";
      public UnexpectedFieldNameException(string filePath, int blockIndex, int lineNumber, string info)
        : base(filePath, blockIndex, lineNumber, string.Format(msgBase, info, lineNumber)) { }
    }
    public class NestedBulletsOnSingleValueChildlessAttributeException : LoreAttributeParsingException
    {
      static string msgBase = "Can't have more than one indented bullet on an attribute with a single value or no nested attributes. Attribute: {0}";

      public NestedBulletsOnSingleValueChildlessAttributeException(string filePath, int blockIndex, int lineNumber, string attributeName)
        : base(filePath, blockIndex, lineNumber, string.Format(msgBase, attributeName)) { }
    }
  }
}
