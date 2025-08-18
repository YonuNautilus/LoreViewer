using LoreViewer.Core.Validation;
using System;

namespace LoreViewer.Core.Stores
{
  public class ValidationStore
  {
    public event EventHandler? ValidationUpdated;

    public LoreValidationResult Result;

    public void Set(LoreValidationResult res)
    {
      Result = res;
      ValidationUpdated?.Invoke(this, EventArgs.Empty);
    }
  }
}
