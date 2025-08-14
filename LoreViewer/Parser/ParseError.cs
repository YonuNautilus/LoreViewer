using System;

namespace LoreViewer.Parser
{
  public class ParseError
  {
    public readonly string FilePath;
    public readonly int BlockIndex;
    public readonly int LineNumber;
    public readonly Exception ParseException;

    public ParseError(string filePath, int blockIndex, int lineNumber, Exception ex)
    {
      FilePath = filePath; BlockIndex = blockIndex; LineNumber = lineNumber; ParseException = ex;
    }
  }
}
