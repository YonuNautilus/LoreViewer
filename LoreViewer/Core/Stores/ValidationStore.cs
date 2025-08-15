using LoreViewer.Core.Validation;

namespace LoreViewer.Core.Stores
{
  public class ValidationStore
  {
    private LoreValidationResult m_oResult;

    public LoreValidationResult Result;

    public void Set(LoreValidationResult res) { m_oResult = res; }
  }
}
