using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using System.Collections.Generic;

namespace LoreViewer.Core.Parsing
{
  public enum EFatalCode
  {
    None, CouldNotFindSettingsFile, CouldNotParseSettingsFile,
  }

  public sealed record ParseResult
  {
    public List<LoreEntity> Models { get; set; }
    public LoreSettings Settings { get; set; }

    public List<ParseError> Errors { get; set; }


    public bool IsFatal = false;
    public string ErrorText;
    public EFatalCode FatalCode;

    public static ParseResult Fatal(EFatalCode code, string msg) { return new ParseResult { ErrorText = msg, FatalCode = code }; }
  }
}
