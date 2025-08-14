using LoreViewer.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
  public class ParseErrorViewModel : ViewModelBase
  {
    private ParseError m_oError;

    public string FilePath { get => m_oError.FilePath; }
    public int BlockIndex { get => m_oError.BlockIndex; }
    public int LineNumber { get => m_oError.LineNumber; }
    public Exception ParseException { get => m_oError.ParseException; }
    public string Message { get => m_oError.ParseException.Message; }

    public ParseErrorViewModel(ParseError pe) { m_oError = pe; }
  }
}
