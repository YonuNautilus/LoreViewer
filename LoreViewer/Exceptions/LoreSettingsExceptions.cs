using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

  public class CollectionWithTypeAndCollectionDefined : SettingsParsingException
  {
    private static string msgBase = "Collection {0} was found to have both entryTypeName and entryCollection defined - this is invalid. Collection can have EITHER type contents OR collection contents.";
    public CollectionWithTypeAndCollectionDefined(LoreDefinitionBase definitionBase)
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
