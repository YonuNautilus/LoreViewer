using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.LoreParsing
{
  public class LoreParsingContext
  {
    public string FilePath;
    public LoreParsingContext(string filePath) { FilePath = filePath; }
  }
}
