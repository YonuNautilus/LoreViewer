using LoreViewer.Core.Parsing;
using LoreViewer.Core.Stores;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Core.Validation
{
  public enum EValidationState
  {
    /* IN ORDER OF PRECEDENCE
     * Meaning, if an element has a warning status but its child element failed, the element's warning status gets changed to ChildFailed.
     */
    None,
    Passed,
    ChildWarning,
    Warning,
    ChildFailed,
    Failed,
  }

  public enum EValidationMessageStatus { Passed, Warning, Failed }



  public struct LoreValidationMessage
  {
    public EValidationMessageStatus Status;
    public string Message { get; set; } = string.Empty;
    public override string ToString() => Message;

    public LoreValidationMessage(EValidationMessageStatus status, string message) { Status = status; Message = message; }
  }

  public class ValidationService
  {


    public LoreValidationResult Validate(LoreRepository repo)
    {
      LoreValidationResult vr = new();

      foreach (LoreEntity entity in repo.Models)
        ValidateEntity(entity, vr);

      return vr;
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

      foreach (LoreNode node in container.Nodes)
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


      // For entities that can contain defined collections (like a Type)
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

      // For collections that contain collections
      else if(entity is LoreCollection lc && (entity as LoreCollection).HasCollections)
      {
        foreach(var childCol in container.Collections)
        {
          ValidateEntity(childCol, result);
          if (result.LoreEntityValidationStates.TryGetValue(childCol, out var state) && state != EValidationState.Passed)
            result.PropagateDescendentState(entity, childCol);
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
            case EFieldContentType.PickList:
              ValidatePicklistAttribute(entity, child, result);
              break;
            case EFieldContentType.Color:
              ValidateColorAttribute(entity, child, result);
              break;
            case EFieldContentType.DateRange:
              ValidateDateRangeAttribute(entity, child, result);
              break;
            case EFieldContentType.Date:
              ValidateDateTimeAttribute(entity, child, result);
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
      foreach (ReferenceAttributeValue refToCheck in refsToCheck)
      {
        // refToCheck.ValueString should be the value written in markdown that was used to resolve the reference.
        if (refToCheck.Value != null)
        {
          LoreTagInfo? valsTag = (refToCheck.Value as LoreEntity).CurrentTag;
          // If the tag was defined, has an ID, and the string of the ReferenceAttributeValue matches the ID, that's good.
          if (valsTag.HasValue && valsTag.Value.HasID && refToCheck.ValueString == valsTag.Value.ID)
          {

          }
          // If the attribute ValueString matches the nodes name, give a warning (two cases, different message for each)
          else if (refToCheck.ValueString == refToCheck.Value.Name)
          {
            string msg = string.Empty;
            if (!valsTag.HasValue)
              msg = "Node referenced by this attribute does not have a tag and should be given one";
            else
              msg = "Node referenced by this attribute has a tag, but the attribute references it by name";

            result.LogWarning(attr, msg);

            if (result.LoreEntityValidationStates.TryGetValue(attr, out var state) && state <= EValidationState.ChildWarning)
              result.LoreEntityValidationStates[attr] = EValidationState.Warning;
          }
        }
      }
    }

    internal void ValidateColorAttribute(LoreEntity parent, LoreAttribute attr, LoreValidationResult result)
    {
      ColorAttributeValue[] valsToCheck;
      if (attr.HasValue) valsToCheck = new ColorAttributeValue[] { (attr.Value as ColorAttributeValue) };
      else valsToCheck = attr.Values.Cast<ColorAttributeValue>().ToArray();

      foreach (ColorAttributeValue cav in valsToCheck)
      {
        // If no value was given, just a name, give warning
        if (string.IsNullOrWhiteSpace(cav.Value.Name))
        {
          result.LogWarning(attr, $"Consider giving this color a name");

          if (result.LoreEntityValidationStates.TryGetValue(attr, out var state) && state <= EValidationState.ChildWarning)
            result.LoreEntityValidationStates[attr] = EValidationState.Warning;
        }
      }
    }

    internal void ValidateDateTimeAttribute(LoreEntity parent, LoreAttribute attr, LoreValidationResult result)
    {
      DateTimeAttributeValue[] valsToCheck;
      if (attr.HasValue) valsToCheck = new DateTimeAttributeValue[] { attr.Value as DateTimeAttributeValue };
      else valsToCheck = attr.Values.Cast<DateTimeAttributeValue>().ToArray();

      foreach (DateTimeAttributeValue dtav in valsToCheck)
      {
        // If using TBD at all, give a warning
        if (dtav.Value.IsTBD) result.LogWarning(attr, $"Using 'TBD' for date/time, consider defining date");


        if (result.LoreEntityValidationStates.TryGetValue(attr, out var state) && state >= EValidationState.ChildWarning)
          result.LoreEntityValidationStates[attr] = EValidationState.Warning;
      }
    }

    internal void ValidateDateRangeAttribute(LoreEntity parent, LoreAttribute attr, LoreValidationResult result)
    {
      DateRangeAttributeValue[] valsToCheck;
      if (attr.HasValue)
        valsToCheck = new DateRangeAttributeValue[] { attr.Value as DateRangeAttributeValue };
      else valsToCheck = attr.Values.Cast<DateRangeAttributeValue>().ToArray();

      foreach (DateRangeAttributeValue drav in valsToCheck)
      {
        // if using TBD at all, give warning.
        if (drav.Value.IsStartTBD || drav.Value.IsEndTBD)
          result.LogWarning(attr, $"Using 'TBD' date/time(s), consider defining dates");

        // If we have two defined dates, give a warning if start date comes AFTER the end date (may be valid in a user's lore, who knows)
        if (drav.Value.IsStartDate && drav.Value.IsEndDate)
        {
          if (drav.Value.StartDateTime > drav.Value.EndDateTime)
            result.LogWarning(attr, "Start date comes AFTER end date. Ignore if valid in lore");
        }


        if (result.LoreEntityValidationStates.TryGetValue(attr, out var state) && state >= EValidationState.ChildWarning)
          result.LoreEntityValidationStates[attr] = EValidationState.Warning;
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
