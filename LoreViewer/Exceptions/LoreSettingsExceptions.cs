using LoreViewer.Settings;
using System;

namespace LoreViewer.Exceptions.SettingsParsingExceptions
{
  public abstract class SettingsParsingException : Exception
  {
    LoreDefinitionBase definition;

    public SettingsParsingException(LoreDefinitionBase definition, string msg) : base(msg)
    {
      this.definition = definition;
    }
  }

  public class DuplicateDefinitionNamesException : SettingsParsingException
  {
    private static string msgBase = "Found at least one set of definitions with the same name: {0}";
    public DuplicateDefinitionNamesException(LoreDefinitionBase def) : base(def, string.Format(msgBase, def.name)) { }
  }

  public class CollectionWithMultipleEntriesDefined : SettingsParsingException
  {
    private static string msgBase = "Collection {0} was found to have more than one entry defined: entryTypeName (name of a defined type), entryCollectionName (name of a defined collection), entryCollection (a locally defined collection) - this is invalid. Collection can have only one entry .";
    public CollectionWithMultipleEntriesDefined(LoreDefinitionBase definitionBase)
      : base(definitionBase, string.Format(msgBase, definitionBase.name)) { }
  }

  public class CyclicalInheritanceException : SettingsParsingException
  {
    private static string msgBase = "Inheritance cycle detected at type {0}";

    public CyclicalInheritanceException(LoreTypeDefinition typeDef)
      : base(typeDef, string.Format(msgBase, typeDef.name)) { }
  }

  public class InheritingMissingTypeDefinitionException : SettingsParsingException
  {
    private static string msgBase = "Something is trying to inherit from a non-defined type: {0}";

    public InheritingMissingTypeDefinitionException(string typeName)
      : base(null, string.Format(msgBase, typeName)) { }
  }

  public class EmbeddedTypeUnknownException : SettingsParsingException
  {
    private static string msgBase = "Embedded node definition {0} wants a type of {1}, but {1} was not defined";

    public EmbeddedTypeUnknownException(LoreEmbeddedNodeDefinition typeDef, string unknownType)
      : base(typeDef, string.Format(msgBase, typeDef.name, unknownType)) { }
  }

  public class EmbeddedTypeNotGivenException : SettingsParsingException
  {
    private static string msgBase = "Embedded Node definition {0} did not have entryTypeName property defined!";

    public EmbeddedTypeNotGivenException(LoreEmbeddedNodeDefinition embeddedDef)
      : base(embeddedDef, string.Format(msgBase, embeddedDef.name)) { }
  }

  public class EmbeddedNodesWithSameTitleException : SettingsParsingException
  {
    private static string msgBase = "Found two embedded node definitions with the same title: {0}";
    public EmbeddedNodesWithSameTitleException(LoreEmbeddedNodeDefinition def, string name) : base(def, string.Format(msgBase, name)) { }
  }

  public class EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException : SettingsParsingException
  {
    private static string msgBase = "A type definition has two embedded nodes of same or derived type in which at least one does not have the title set!";

    public EmbeddedNodeDefinitionWithAncestralTypeAndNoNameException(LoreEmbeddedNodeDefinition def) : base(def, string.Format(msgBase)) { }
  }
}
