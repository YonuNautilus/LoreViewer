using LoreViewer.Core.Parsing;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings;
using System;
using System.Collections.Generic;

namespace LoreViewer.Core.Stores
{
  public sealed class LoreRepository
  {
    public IEnumerable<LoreEntity> Models;

    public IEnumerable<ParseError> Errors;

    public LoreSettings Settings { get; private set; }

    public event EventHandler? LoreRepoUpdated;

    public void Set(ParseResult res)
    {
      Models = res.Models;
      Errors = res.Errors;
      Settings = res.Settings;
      LoreRepoUpdated?.Invoke(this, EventArgs.Empty);
    }


  }
}
