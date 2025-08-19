using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Core.Outline
{
  /// <summary>
  /// A small immutable wrapper used to display infor about loreentities in a very basic sense.
  /// OutlineItem is wrapped by VMs for actually displaying.
  /// </summary>
  public record OutlineItem(LoreEntity entity, string displayName, IReadOnlyList<OutlineItem> children)
  {
    public EValidationState validationState { get; set; }
  }
}
