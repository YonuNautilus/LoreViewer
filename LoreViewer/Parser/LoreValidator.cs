using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Validation
{
  public enum EValidationState
  {
    Passed,
    ChildFailed,
    Failed,
  }

  public class LoreValidationResult
  {
    public Dictionary<LoreEntity, EValidationState> LoreEntityValidationStates { get; set; } = new Dictionary<LoreEntity, EValidationState>();
    public Dictionary<LoreEntity, List<LoreValidationError>> Errors { get; set; } = new();

    public void LogError(LoreEntity entity, string message)
    {
      LoreValidationError newErr = new LoreValidationError
      {
        Entity = entity,
        Message = message
      };

      if (Errors.ContainsKey(entity)) Errors[entity].Add(newErr);
      else Errors.Add(entity, [.. new LoreValidationError[] { newErr }]);

      if (LoreEntityValidationStates.ContainsKey(entity))
      {
        if (LoreEntityValidationStates[entity] == EValidationState.ChildFailed)
          LoreEntityValidationStates[entity] = EValidationState.Failed;
      }
      else
        LoreEntityValidationStates.Add(entity, EValidationState.Failed);
    }

    public void PropagateDescendentError(LoreEntity parent, LoreEntity child)
    {
      if (LoreEntityValidationStates.TryGetValue(parent, out var validationState) && LoreEntityValidationStates.TryGetValue(child, out var childValidationState))
      {
        if (childValidationState > EValidationState.Passed && validationState < EValidationState.Failed)
          LoreEntityValidationStates[parent] = EValidationState.ChildFailed;
      }
    }
  }

  public class LoreValidationError
  {
    public LoreEntity Entity;
    public string Message { get; set; } = string.Empty;
    public override string ToString() => Message;
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

      if (entity is IEmbeddedNodeContainer nodeCont)
        ValidateEmbeddedNodes(nodeCont, entity, result);
      if (entity is IAttributeContainer attrCont)
        ValidateAttributes(attrCont, entity, result);
      if (entity is ISectionContainer secCont)
        ValidateSections(secCont, entity, result);
      if (entity is ICollectionContainer colCont)
        ValidateCollections(colCont, entity, result);
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
          result.PropagateDescendentError(entity, child);
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
            result.PropagateDescendentError(entity, child);
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

    internal void ValidateAttributes(IAttributeContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      if (entity.Definition is IFieldDefinitionContainer defWithFields)
      {
        var defs = ((IFieldDefinitionContainer)entity.Definition).fields;

        // Validate nested fields recursively
        foreach (var child in container.Attributes)
        {
          ValidateEntity(child, result);

          if (result.LoreEntityValidationStates.TryGetValue(child, out var state) && state != EValidationState.Passed)
          {
            result.PropagateDescendentError(entity, child);
          }
        }

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

            // Check if an attribute follows its options if field is picklist style
            if(contains && def.style == EFieldStyle.PickList)
            {
              LoreAttribute attr = container.GetAttribute(def.name);
              string attrVal = attr.Value;
              var options = def.GetPicklistOptions();
              if (!options.Contains(attrVal))
              {
                result.LogError(attr, $"Attribute {def.name} of style Picklist has invalid value {attrVal}. Valid Values are {string.Join(", ", options)}");
                result.LoreEntityValidationStates[attr] = EValidationState.Failed;
                result.PropagateDescendentError(entity, attr);
              }
            }
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
          result.PropagateDescendentError(entity, child);
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
