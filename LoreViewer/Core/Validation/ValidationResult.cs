using LoreViewer.Domain.Entities;
using System.Collections.Generic;

namespace LoreViewer.Core.Validation
{
  public class LoreValidationResult
  {
    public Dictionary<LoreEntity, List<LoreValidationMessage>> LoreEntityValidationMessages { get; set; } = new();
    public Dictionary<LoreEntity, List<LoreValidationMessage>> Errors { get; set; } = new();
    public Dictionary<LoreEntity, EValidationState> LoreEntityValidationStates { get; set; } = new Dictionary<LoreEntity, EValidationState>();

    private void AddMessage(LoreEntity entity, LoreValidationMessage message)
    {
      if (LoreEntityValidationMessages.ContainsKey(entity))
      {
        if (LoreEntityValidationMessages[entity] == null) LoreEntityValidationMessages[entity] = new();

        LoreEntityValidationMessages[entity].Add(message);
      }
      else
      {
        LoreEntityValidationMessages.Add(entity, new List<LoreValidationMessage>(new LoreValidationMessage[] { message }));
      }
    }

    public void LogError(LoreEntity entity, string message)
    {
      LoreValidationMessage newErr = new LoreValidationMessage(EValidationMessageStatus.Failed, message);

      AddMessage(entity, newErr);


      if (Errors.ContainsKey(entity)) Errors[entity].Add(newErr);
      else Errors.Add(entity, [.. new LoreValidationMessage[] { newErr }]);

      if (LoreEntityValidationStates.ContainsKey(entity))
      {
        if (LoreEntityValidationStates[entity] == EValidationState.ChildFailed)
          LoreEntityValidationStates[entity] = EValidationState.Failed;
      }
      else
        LoreEntityValidationStates.Add(entity, EValidationState.Failed);
    }

    public void LogWarning(LoreEntity entity, string message)
    {
      LoreValidationMessage newWarning = new LoreValidationMessage(EValidationMessageStatus.Warning, message);

      AddMessage(entity, newWarning);

      if (LoreEntityValidationStates.ContainsKey(entity))
        if (LoreEntityValidationStates[entity] <= EValidationState.ChildWarning)
          LoreEntityValidationStates[entity] = EValidationState.Warning;
    }

    public void PropagateDescendentState(LoreEntity parent, LoreEntity child)
    {
      if (LoreEntityValidationStates.TryGetValue(parent, out var validationState) && LoreEntityValidationStates.TryGetValue(child, out var childValidationState))
      {
        // If the child did anything but pass, and the current element has anything but failed, we *may* need to update this element's status
        // But if the child passed, the parent won't need to be updated, and if the parent failed, well, failed takes precedence over all other statuses
        if (childValidationState > EValidationState.Passed && validationState < EValidationState.Failed)
        {
          // the EValidationState enum is in order of precendence, so if any children have a status greater than Passed, the parent CANNOT have Passed status

          // Based on this child element, determine what this parent would become (assuming the parent has Passed status)
          EValidationState suggestedNewState;

          if (childValidationState == EValidationState.Warning || childValidationState == EValidationState.ChildWarning) suggestedNewState = EValidationState.ChildWarning;
          else if (childValidationState == EValidationState.Failed || childValidationState == EValidationState.ChildFailed) suggestedNewState = EValidationState.ChildFailed;
          else return;

          // Now take that suggestion and see if it can be applied to the parent or not.
          if (suggestedNewState > validationState) LoreEntityValidationStates[parent] = suggestedNewState;
        }
      }
    }
  }
}
