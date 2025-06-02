using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.Exceptions
{
  #region Parsing Exceptions
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

  public abstract class LoreNodeParsingException : LoreParsingException
  {
    public LoreNodeParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class NoTagParsingException : LoreNodeParsingException
  {
    static string msgBase = "Error while parsing file {0}: Level 1 heading block found is not tagged";
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

  public class UnexpectedTypeNameException : LoreNodeParsingException
  {
    public UnexpectedTypeNameException(string filePath, int blockIndex, int lineNumber, string typeName)
      : base(filePath, blockIndex, lineNumber, $"Found reference to a node type that was not defined: {typeName}") { }
  }

  public class HeadingLevelErrorException : LoreNodeParsingException
  {
    public HeadingLevelErrorException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg)
    {

    }
  }

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


  /*
   LoreAttributeParsingException
│   ├── UnexpectedFieldNameException
│   ├── InvalidNestedStructureException
│   ├── UnexpectedFlatValueException
│   └── UnexpectedMultiStructureException
   */
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
    static string msgBase = "Can't have more than one indented bullet on an attribute with a single value or no nested attributes. Attribute: {0}";

    public NestedBulletsOnSingleValueChildlessAttributeException(string filePath, int blockIndex, int lineNumber, string attributeName)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, attributeName)) { }
  }

  public class LoreCollectionParsingException : LoreParsingException
  {
    public LoreCollectionParsingException(string filePath, int blockIndex, int lineNumber, string msg)
      : base(filePath, blockIndex, lineNumber, msg) { }
  }

  public class InvalidContainedTypeDefinitionException: LoreCollectionParsingException
  {
    static string msgBase = "Tried to parse a collection with an invalid type {0}. Type can only be Type (node) or Collection";
    public InvalidContainedTypeDefinitionException(string filePath, int blockIndex, int lineNumber, LoreDefinitionBase containedType)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, containedType)) { }
  }

  public class CollectionWithUnknownTypeException: LoreCollectionParsingException
  {
    static string msgBase = "Tried to parse a collection with an undefined type {0}.";
    public CollectionWithUnknownTypeException(string filePath, int blockIndex, int lineNumber, string type)
      : base(filePath, blockIndex, lineNumber, string.Format(msgBase, type)) { }
  }
  #endregion

  #region Settings parsing Exceptions
  public abstract class SettingsParsingException : Exception
  {
    LoreDefinitionBase definition;

    public SettingsParsingException(LoreDefinitionBase definition, string msg) : base(msg)
    {
      this.definition = definition;
    }
  }

  public class CollectionWithTypeAndCollectionDefined : SettingsParsingException
  {
    public static string msgBase = "Collection {0} was found to have both entryTypeName and entryCollection defined - this is invalid. Collection can have EITHER type contents OR collection contents.";
    public CollectionWithTypeAndCollectionDefined(LoreDefinitionBase definitionBase)
      : base(definitionBase, string.Format(msgBase, definitionBase.name)) { }
  }
  #endregion 
}
