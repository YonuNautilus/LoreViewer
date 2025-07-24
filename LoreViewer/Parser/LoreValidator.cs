using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Parser;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Validation
{
  public enum EValidationState
  {
    /* IN ORDER OF PRECEDENCE
     * Meaning, if an element has a warning status but its child element failed, the element's warning status gets changed to ChildFailed.
     */

    Passed,
    ChildWarning,
    Warning,
    ChildFailed,
    Failed,
  }

  public enum EValidationMessageStatus { Passed, Warning, Failed }

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
          if(suggestedNewState > validationState) LoreEntityValidationStates[parent] = suggestedNewState;
        }
      }
    }
  }

  public struct LoreValidationMessage
  {
    public EValidationMessageStatus Status;
    public string Message { get; set; } = string.Empty;
    public override string ToString() => Message;

    public LoreValidationMessage(EValidationMessageStatus status, string message) { Status = status; Message = message; }
  }

  public class LoreValidator
  {
    public LoreValidationResult ValidationResult;

    public LoreValidationResult Validate(IEnumerable<LoreEntity> entities)
    {
      ValidationResult = new();

      foreach (LoreEntity entity in entities)
        ValidateEntity(entity, ValidationResult);

      return ValidationResult;
    }

    private void ValidateEntity(LoreEntity entity, LoreValidationResult result)
    {
      result.LoreEntityValidationStates[entity] = EValidationState.Passed;

      if (entity is IEmbeddedNodeContainer embNodeCont)
        ValidateEmbeddedNodes(embNodeCont, entity, result);
      if (entity is IAttributeContainer attrCont)
        ValidateAttributes(attrCont, entity, result);
      if (entity is ISectionContainer secCont)
        ValidateSections(secCont, entity, result);
      if (entity is ICollectionContainer colCont)
        ValidateCollections(colCont, entity, result);
      if (entity is INodeContainer nodeCont)
        ValidateNodes(nodeCont, entity, result);
    }

    private void ValidateNodes(INodeContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");
      
      foreach(LoreNode node in container.Nodes)
      {
        ValidateEntity(node, result);

        if (result.LoreEntityValidationStates.TryGetValue(node, out var state) && state != EValidationState.Passed)
          result.PropagateDescendentState(entity, node);
      }
    }

    private void ValidateEmbeddedNodes(IEmbeddedNodeContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      // Validate embedded nodes recursively
      foreach (var child in container.Nodes)
      {
        ValidateEntity(child, result);

        if (result.LoreEntityValidationStates.TryGetValue(child, out var state) && state != EValidationState.Passed)
        {
          result.PropagateDescendentState(entity, child);
        }
      }

      // If this is a node with embedding nodes
      if (entity.Definition is IEmbeddedNodeDefinitionContainer defWithEmbedded && defWithEmbedded.HasNestedNodes)
      {
        var defs = ((IEmbeddedNodeDefinitionContainer)entity.Definition).embeddedNodeDefs;

        foreach (var def in defs)
        {
          bool contains = container.ContainsEmbeddedNode(def.nodeType, def.name ?? def.nodeType.name);

          if ((def.required) && !contains)
          {
            result.LogError(entity, $"Missing required embedded node '{def.name ?? def.nodeType.name}'");
            result.LoreEntityValidationStates[entity] = EValidationState.Failed;
          }
        }
      }
    }

    internal void ValidateCollections(ICollectionContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      if (entity.Definition is ICollectionDefinitionContainer defWithFields)
      {
        var defs = ((ICollectionDefinitionContainer)entity.Definition).collections;

        // Validate nested fields recursively
        foreach (var child in container.Collections)
        {
          ValidateEntity(child, result);

          if (result.LoreEntityValidationStates.TryGetValue(child, out var state) && state != EValidationState.Passed)
          {
            result.PropagateDescendentState(entity, child);
          }
        }

        if (defs != null)
        {
          foreach (var def in defs)
          {
            bool contains = container.HasCollection(def.name);

            if ((def.required) && !contains)
            {
              result.LogError(entity, $"Missing required collection '{def.name}'");
              result.LoreEntityValidationStates[entity] = EValidationState.Failed;
            }
          }
        }
      }
    }

    /// <summary>
    /// Validates attributes contained within this IAttributeContainer.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="entity"></param>
    /// <param name="result"></param>
    /// <exception cref="Exception"></exception>
    internal void ValidateAttributes(IAttributeContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      if (entity.Definition is IFieldDefinitionContainer defWithFields)
      {
        var defs = ((IFieldDefinitionContainer)entity.Definition).fields;

        // Validate nested fields recursively
        foreach (var child in container.Attributes)
        {
          // Run this container's children though the validation process to catch basic things like missing required subattributes, etc
          ValidateEntity(child, result);


          // Validate the attributes by content type
          switch (child.DefinitionAs<LoreFieldDefinition>().contentType)
          {
            case EFieldContentType.ReferenceList:
              ValidateReferenceAttribute(entity, child, result);
              break;
            case EFieldContentType.Picklist:
              ValidatePicklistAttribute(entity, child, result);
              break;
          }


          if (result.LoreEntityValidationStates.TryGetValue(child, out var state) && state != EValidationState.Passed)
          {
            result.PropagateDescendentState(entity, child);
          }
        }

        // Check for missing required fields
        if (defs != null)
        {
          foreach (var def in defs)
          {
            bool contains = container.HasAttribute(def.name);

            if ((def.required) && !contains)
            {
              result.LogError(entity, $"Missing required attribute '{def.name}'");
              result.LoreEntityValidationStates[entity] = EValidationState.Failed;
            }
          }
        }
      }
    }

    internal void ValidatePicklistAttribute(LoreEntity parent, LoreAttribute attr, LoreValidationResult result)
    {
      LoreFieldDefinition def = attr.DefinitionAs<LoreFieldDefinition>();
      string[] valuesToCheck;

      if (attr.HasValue) valuesToCheck = new string[] { attr.Value.ValueString };
      else valuesToCheck = attr.Values.Select(v => v.ValueString).ToArray();

      var options = def.GetPicklistOptions();
      foreach (string valueToCheck in valuesToCheck)
      {
        if (!options.Contains(valueToCheck))
        {
          result.LogError(attr, $"Attribute {def.name} of style Picklist has invalid value {valueToCheck}. Valid Values are {string.Join(", ", options)}");
          result.LoreEntityValidationStates[attr] = EValidationState.Failed;
          result.PropagateDescendentState(parent, attr);
        }
      }
    }

    internal void ValidateReferenceAttribute(LoreEntity parent, LoreAttribute attr, LoreValidationResult result)
    {
      ReferenceAttributeValue[] refsToCheck;
      if (attr.HasValue) refsToCheck = new ReferenceAttributeValue[] { (attr.Value as ReferenceAttributeValue) };
      else refsToCheck = attr.Values.Cast<ReferenceAttributeValue>().ToArray();

      // Check if the reference was resolved by ID or by name. If by name, create a warning.
      foreach(ReferenceAttributeValue refToCheck in refsToCheck)
      {
        // refToCheck.ValueString should be the value written in markdown that was used to resolve the reference.
        if(refToCheck.Value != null)
        {
          LoreTagInfo? valsTag = (refToCheck.Value as LoreEntity).CurrentTag;
          // If the tag was defined, has an ID, and the string of the ReferenceAttributeValue matches the ID, that's good.
          if (valsTag.HasValue && valsTag.Value.HasID && refToCheck.ValueString == valsTag.Value.ID)
          {

          }
          // If the attribute ValueString matches the nodes name, give a warning (two cases, different message for each)
          else if(refToCheck.ValueString == refToCheck.Value.Name)
          {
            string msg = string.Empty;
            if (!valsTag.HasValue)
              msg = "Node referenced by this attribute does not have a tag and should be given one";
            else
              msg = "Node referenced by this attribute has a tag, but the attribute references it by name";

            result.LogWarning(attr, msg);

            if (result.LoreEntityValidationStates.TryGetValue(attr, out var state) && state == EValidationState.Passed)
              result.LoreEntityValidationStates[attr] = EValidationState.Warning;
          }
        }
      }
    }


    internal void ValidateSections(ISectionContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      // Validate nested fields recursively
      foreach (var child in container.Sections)
      {
        ValidateEntity(child, result);

        if (result.LoreEntityValidationStates.TryGetValue(child, out var state) && state != EValidationState.Passed)
        {
          result.PropagateDescendentState(entity, child);
        }
      }

      if (entity.Definition is ISectionDefinitionContainer defWithSections)
      {
        var defs = ((ISectionDefinitionContainer)entity.Definition).sections;
        if (defs != null)
        {
          foreach (var def in defs)
          {
            bool contains = container.HasSection(def.name);

            if ((def.required) && !contains)
            {
              result.LogError(entity, $"Missing required attribute '{def.name}'");
              result.LoreEntityValidationStates[entity] = EValidationState.Failed;
            }
          }
        }
      }
    }
  }
}
