using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using LoreViewer.LoreElements;
using LoreViewer.Settings;
using Markdig.Syntax;
using System;
using System.IO;
using System.Linq;

namespace LoreViewer.Exceptions.LoreParsingExceptions
{
  public abstract class LoreParsingException : Exception
  {
    public readonly string MarkdownFilePath;

    static string msgBase;


    public readonly int BlockIndex;
    public readonly int LineNumber;
    public string MarkdownFileName => Path.GetFileName(MarkdownFilePath);

    public LoreParsingException(string filePath, int blockIndex, int lineNumber, string msg) : base(msg)
    {
      MarkdownFilePath = filePath;
      BlockIndex = blockIndex;
      LineNumber = lineNumber;
    }
  }

  #region Nodes
  public abstract class LoreNodeParsingException : LoreParsingException
  {
    public LoreNodeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class NoTagParsingException : LoreNodeParsingException
  {
    static string msgBase = "Error while parsing file {0}: Level 1 heading block is not tagged";
    public NoTagParsingException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, String.Format(msgBase, Path.GetFileName(filePath)))
    { }
  }

  public class FirstHeadingTagException : LoreNodeParsingException
  {
    static string msgBase = "Error while parsing file {0}: First heading MUST be a TYPE or COLLECTION";
    public FirstHeadingTagException(string filePath, int blockIndex, int lineNumber)
      : base(filePath, blockIndex, lineNumber, String.Format(msgBase, Path.GetFileName(filePath)))
    { }
  }

  public class DefinitionNotFoundException : LoreNodeParsingException
  {
    public DefinitionNotFoundException(string filePath, int blockIndex, int lineNumber, string newTitle)
      : base(filePath, blockIndex, lineNumber, $"Could not find section or collection definition for a heading with title {newTitle}")
    {

    }
  }

  public class HeadingLevelErrorException : LoreNodeParsingException
  {
    public HeadingLevelErrorException(string filePath, int blockIndex, int lineNumber, HeadingBlock hb)
      : base(filePath, blockIndex, lineNumber, $"First heading of markdown file {filePath} must be level 1, but it was level {hb.Level}") { }
  }

  public class EmbeddedNodeTypeNotAllowedException : LoreNodeParsingException
  {
    private static string msgBase = "Node type {0} does not allow node type {1} as an embedded node.";
    public EmbeddedNodeTypeNotAllowedException(string filePath, int blockIndex, int lineNumber, string typeNameParent, string typeNameChild)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, typeNameParent, typeNameChild)) { }
  }

  public class TypeNotDefinedxception : LoreNodeParsingException
  {
    private static string msgBase = "Markdown defined a node of type {0}, but that definition was not found in the schema.";
    public TypeNotDefinedxception(string filePath, int blockIndex, int lineNumber, string nodeType)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, nodeType)) { }
  }
  #endregion


  #region Sections
  public abstract class LoreSectionParsingException : LoreParsingException
  {
    public LoreSectionParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class UnexpectedSectionNameException : LoreSectionParsingException
  {
    static string msgBase = "Section without definiton found. Section title: {0}; Line number {1}";
    public UnexpectedSectionNameException(string filePath, int blockIndex, int lineNumber, string headingTitle, string subHeadingTitle)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, subHeadingTitle, lineNumber)) { }
  }

  public class UnexpectedTagTypeException : LoreSectionParsingException
  {
    static string msgBase = "Section had a subheading with tag type {2}, but only <section> tags are allowed. Section Title: {0}; Line number {1}";
    public UnexpectedTagTypeException(string filePath, int blockIndex, int lineNumber, string tagType)
      : base(filePath, blockIndex, lineNumber, String.Format(msgBase, Path.GetFileName(filePath), lineNumber, tagType))
    { }
  }
  #endregion

  /*
   LoreAttributeParsingException
│   ├── UnexpectedFieldNameException
│   ├── InvalidNestedStructureException
│   ├── UnexpectedFlatValueException
│   └── UnexpectedMultiStructureException
   */

  #region Attributes
  public abstract class LoreAttributeParsingException : LoreParsingException
  {
    public LoreAttributeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class UnexpectedFieldNameException : LoreAttributeParsingException
  {
    static string msgBase = "No definition found for attribute {0}, file line number {1}";
    public UnexpectedFieldNameException(string filePath, int blockIndex, int lineNumber, string info)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, info, lineNumber)) { }
  }
  public class NestedBulletsOnSingleValueChildlessAttributeException : LoreAttributeParsingException
  {
    static string msgBase = "Can't have more than one indented bullet on an attribute that does not have style NestedFields or MultiValue. Attribute: {0}, Style: {1}";

    public NestedBulletsOnSingleValueChildlessAttributeException(string filePath, int blockIndex, int lineNumber, LoreFieldDefinition field)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, field.name, field.structure)) { }
  }

  public class ReflistCannotResovleException : LoreAttributeParsingException
  {
    static string msgBase = "Reflist Attribute '{0}' cannot find a node of type {1} with name or ID equal to '{2}'";

    public ReflistCannotResovleException(string filePath, int blockIndex, int lineNumber, ReferenceAttributeValue attrVal)
      : base(filePath, blockIndex, lineNumber,
          string.Format(msgBase,
            attrVal.OwningAttribute.Name,
            attrVal.OwningAttribute.DefinitionAs<LoreFieldDefinition>().RefListType.name,
            attrVal.ValueString))
    { }
  }

  public class ColorCannotParseException : LoreAttributeParsingException
  {
    static string msgBase = "Color attribute '{0}' cannot parse string \"{1}\" into a color, it must start with the '#' character followed by 6 or 8 hexadecimal digits";

    public ColorCannotParseException(string filePath, int blockIndex, int lineNumber, ColorAttributeValue attrVal) 
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, attrVal.OwningAttribute.Name, attrVal.ValueString)) { }
  }

  public class DateRangeCannotParseException : LoreAttributeParsingException
  {
    protected DateRangeCannotParseException(string filePath, int blockIndex, int lineNumber, DateRangeAttributeValue attrVal, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class DateRangeCannotParseStartDateException : DateRangeCannotParseException
  {
    static string msgBase = "Invalid date format or keyword on Start Date of DateRange attribute: '{0}'";
    public DateRangeCannotParseStartDateException(DateRangeAttributeValue attrVal)
      : base(attrVal.OwningAttribute.SourcePath, attrVal.OwningAttribute.BlockIndex, attrVal.OwningAttribute.LineNumber,
          attrVal, string.Format(msgBase, attrVal.ValueString)) { }
  }

  public class DateRangeCannotParseEndDateException : DateRangeCannotParseException
  {
    static string msgBase = "Invalid date format or keyword on End Date of DateRange attribute: '{0}'";
    public DateRangeCannotParseEndDateException(DateRangeAttributeValue attrVal)
      : base(attrVal.OwningAttribute.SourcePath, attrVal.OwningAttribute.BlockIndex, attrVal.OwningAttribute.LineNumber,
          attrVal, string.Format(msgBase, attrVal.ValueString)) { }
  }

  public class DateRangeNoPipeCharacterException : DateRangeCannotParseException
  {
    static string msgBase = "No pipe character '|' found in value {0} - Need a pipe to separate start and end date!";
    public DateRangeNoPipeCharacterException(DateRangeAttributeValue attrVal)
      : base(attrVal.OwningAttribute.SourcePath, attrVal.OwningAttribute.BlockIndex, attrVal.OwningAttribute.LineNumber,
          attrVal, string.Format(msgBase, attrVal.ValueString)){ }
  }

  public class DateRangeTooManyPipeCharactersException : DateRangeCannotParseException
  {
    static string msgBase = "Too many pipe characters '|' fonud in value {0} - Only use ONE to separate start date from end date, couted {1}";
    public DateRangeTooManyPipeCharactersException(DateRangeAttributeValue attrVal)
      : base(attrVal.OwningAttribute.SourcePath, attrVal.OwningAttribute.BlockIndex, attrVal.OwningAttribute.LineNumber,
          attrVal, string.Format(msgBase, attrVal.ValueString, attrVal.ValueString.Count(p => p == '|'))) { }
  }

  public class DateTimeCannotParseException : LoreAttributeParsingException
  {
    public DateTimeCannotParseException(DateTimeAttributeValue attrVal)
      : base(attrVal.OwningAttribute.SourcePath, attrVal.OwningAttribute.BlockIndex, attrVal.OwningAttribute.LineNumber,
          $"Could not parse date or keyword: {attrVal.ValueString}") { }
  }
  #endregion

  #region Collections
  public abstract class LoreCollectionParsingException : LoreParsingException
  {
    public LoreCollectionParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class UnknownTypeInCollectionException : LoreCollectionParsingException
  {
    static string msgBase = "Tried to parse a node of UNKNOWN type {0} int a collection of type {1}!";
    public UnknownTypeInCollectionException(string filePath, int blockIndex, int lineNumber, string unknownTypeTag, LoreDefinitionBase containedType)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, unknownTypeTag, containedType)) { }
  }

  public class InvalidTypeInCollectionException : LoreCollectionParsingException
  {
    static string msgBase = "Tried to parse a node of type {0} into a collection of type {1}. Should only add nodes of the defined type {1} or derived types!";
    public InvalidTypeInCollectionException(string filePath, int blockIndex, int lineNumber, string invalidTypeTag, LoreDefinitionBase containedType)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, invalidTypeTag, containedType)) { }
  }

  public class CollectionWithUnknownTypeException : LoreCollectionParsingException
  {
    static string msgBase = "Tried to parse a collection with an undefined type {0}.";
    public CollectionWithUnknownTypeException(string filePath, int blockIndex, int lineNumber, string type)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, type)) { }
  }
  #endregion

  #region Embedded Nodes
  public abstract class EmbeddedNodeParsingException : LoreParsingException
  {
    public EmbeddedNodeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class EmbeddedNodeInvalidNameException : EmbeddedNodeParsingException
  {
    static string msgBase = "Tried to add embedded node of type {1} to a node of type {0} - but embedded node title '{2}' is not allowed!";

    public EmbeddedNodeInvalidNameException(string filePath, int blockIndex, int lineNumber, LoreTypeDefinition parentNodeType, LoreTypeDefinition embeddedNodeType, string actualEmbeddedNodeTitle)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, parentNodeType.name, embeddedNodeType.name, actualEmbeddedNodeTitle)) { }
  }

  public class EmbeddedNodeAlreadyAddedException : EmbeddedNodeParsingException
  {
    static string msgBase = "Tried to add embedded node '{0}' of type {1} to parent node {2} (type: {3}), but the embedded node could not be added because existing embedded nodes satisfied all applicable embedded node definitions";
    public EmbeddedNodeAlreadyAddedException(string filePath, int blockIndex, int lineNumber, LoreNode parentNode, LoreTypeDefinition newNodeType, string newNodeTitle)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, newNodeType, newNodeType.name, parentNode.Name, parentNode.Definition.name)) { }
  }



  #endregion

  #region Tags

  public class LoreTagParsingException : LoreParsingException
  {
    static string msgBase = "There was an issue trying to parse the HTML tag {0}. Please check that the HTML is formatted correctly. Parsing exception: {1}";

    public LoreTagParsingException(string filePath, int blockIndex, int lineNumber, string htmlTag, Exception e)
    : base(filePath, blockIndex, lineNumber, string.Format(msgBase, htmlTag, e.Message)) { }
  }
  #endregion
}
