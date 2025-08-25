using LoreViewer.Core.Parsing;
using System;

namespace LoreViewer.Presentation.ViewModels
{
  public class ParseErrorViewModel : ViewModelBase
  {
    private ParseError m_oError;

    public string FilePath { get => m_oError.FilePath; }
    public int BlockIndex { get => m_oError.BlockIndex; }
    public int LineNumber { get => m_oError.LineNumber; }
    public Exception ParseException { get => m_oError.ParseException; }
    public string Message { get => m_oError.ParseException.Message; }

    public Tuple<string, int, int, Exception> OpenAtParam
    {
      get => new Tuple<string, int, int, Exception>(FilePath, BlockIndex, LineNumber, ParseException);
    }

    public ParseErrorViewModel(ParseError pe) { m_oError = pe; }
  }
}
