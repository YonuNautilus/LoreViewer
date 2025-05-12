using System;
using System.Collections.Generic;
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

  public class NoTagParsingException : LoreParsingException
  {
    public NoTagParsingException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, $"Error while parsing file {Path.GetFileName(filePath)}: First heading block found is not tagged")
    { }
  }

  public class FirstHeadingTagException : LoreParsingException
  {
    public FirstHeadingTagException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, $"Error while parsing file {Path.GetFileName(filePath)}: First heading MUST define a node or collection")
    { }
  }

  public class UnexpectedBlockException : LoreParsingException
  {
    public UnexpectedBlockException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, $"")
    {

    }
  }

  public class HeadingLevelErrorException : LoreParsingException
  {
    public HeadingLevelErrorException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg)
    {

    }
  }
}
