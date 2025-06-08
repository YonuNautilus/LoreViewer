using LoreViewer.LoreElements;
using LoreViewer.LoreElements.Interfaces;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization.NamingConventions;

namespace LoreViewer
{
  public enum EValidationState
  {
    Passed,
    Failed,
    ChildFailed
  }

  public class LoreValidationResult
  {
    public Dictionary<LoreEntity, EValidationState> LoreEntityValidationStates { get; set; } = new Dictionary<LoreEntity, EValidationState>();
    public List<LoreValidationError> Errors { get; set; } = new List<LoreValidationError>();

    public void LogError(LoreEntity entity, string message)
    {
      Errors.Add(new LoreValidationError
      {
        Entity = entity,
        Message = message
      });

      if (LoreEntityValidationStates.ContainsKey(entity))
      {
        if (LoreEntityValidationStates[entity] == EValidationState.ChildFailed)
          LoreEntityValidationStates[entity] = EValidationState.Failed;
      }
      else
        LoreEntityValidationStates.Add(entity, EValidationState.Failed);
    }

    public void PropagateDescendentError(LoreEntity parent)
    {
      if (LoreEntityValidationStates.TryGetValue(parent, out var validationState))
      {
        if (validationState != EValidationState.Failed)
          LoreEntityValidationStates[parent] = EValidationState.ChildFailed;
      }
      else
        LoreEntityValidationStates[parent] = EValidationState.ChildFailed;
    }
  }

  public class LoreValidationError
  {
    public LoreEntity Entity;
    public string Message;
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

      // If this is a node with embedding nodes
      if (entity.Definition is IEmbeddedNodeDefinitionContainer defWithEmbedded)
      {
        var defs = ((IEmbeddedNodeDefinitionContainer)entity.Definition).embeddedNodeDefs;

        foreach (var def in defs)
        {
          bool contains = container.ContainsEmbeddedNode(def.nodeType, def.name ?? def.nodeType.name);

          if ((def.required || def.nodeType.HasRequiredEmbeddedNodes) && !contains)
          {
            result.LogError(entity, $"Missing required embedded node '{def.name ?? def.nodeType.name}'");
            result.LoreEntityValidationStates[entity] = EValidationState.Failed;
          }
        }

        // Validate embedded nodes recursively
        foreach (var child in container.Nodes)
        {
          ValidateEntity(child, result);

          if (result.LoreEntityValidationStates.TryGetValue(child, out var state)
              && state != EValidationState.Passed)
          {
            result.PropagateDescendentError(entity);
          }
        }
      }
    }

    internal void ValidateCollections(ICollectionContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

    }

    internal void ValidateAttributes(IAttributeContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      if (entity.Definition is IEmbeddedNodeDefinitionContainer defWithEmbedded)
      {
        var defs = ((IFieldDefinitionContainer)entity.Definition).fields;
        if (defs != null)
        {
          foreach (var def in defs)
          {
            bool contains = container.HasAttribute(def.name);

            if ((def.required || def.HasRequiredNestedFields) && !contains)
            {
              result.LogError(entity, $"Missing required attribute '{def.name}'");
              result.LoreEntityValidationStates[entity] = EValidationState.Failed;
            }
          }
        }
        // Validate nested fields recursively
        foreach (var child in container.Attributes)
        {
          ValidateEntity(child, result);

          if (result.LoreEntityValidationStates.TryGetValue(child, out var state)
              && state != EValidationState.Passed)
          {
            result.PropagateDescendentError(entity);
          }
        }
      }
    }

    internal void ValidateSections(ISectionContainer container, LoreEntity entity, LoreValidationResult result)
    {
      if (entity != container) throw new Exception($"PARSING ERROR ON ENTITY {entity.Name}");

      if (entity.Definition is ISectionDefinitionContainer defWithEmbedded)
      {
        var defs = ((ISectionDefinitionContainer)entity.Definition).sections;
        if (defs != null)
        {
          foreach (var def in defs)
          {
            bool contains = container.HasSection(def.name);

            if ((def.required || def.HasRequiredNestedSections) && !contains)
            {
              result.LogError(entity, $"Missing required attribute '{def.name}'");
              result.LoreEntityValidationStates[entity] = EValidationState.Failed;
            }
          }
          // Validate nested fields recursively
          foreach (var child in container.Sections)
          {
            ValidateEntity(child, result);

            if (result.LoreEntityValidationStates.TryGetValue(child, out var state)
                && state != EValidationState.Passed)
            {
              result.PropagateDescendentError(entity);
            }
          }
        }
      }
    }
  }
}
