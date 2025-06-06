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

  public class CollectionWithTypeAndCollectionDefined : SettingsParsingException
  {
    public static string msgBase = "Collection {0} was found to have both entryTypeName and entryCollection defined - this is invalid. Collection can have EITHER type contents OR collection contents.";
    public CollectionWithTypeAndCollectionDefined(LoreDefinitionBase definitionBase)
      : base(definitionBase, string.Format(msgBase, definitionBase.name)) { }
  }

  public class CyclicalInheritanceException : SettingsParsingException
  {
    public static string msgBase = "Inheritance cycle detected at type {0}";

    public CyclicalInheritanceException(LoreTypeDefinition typeDef)
      : base(typeDef, string.Format(msgBase, typeDef.name)) { }
  }

  public class InheritingMissingTypeDefinitionException : SettingsParsingException
  {
    public static string msgBase = "Something is trying to inherit from a non-defined type: {0}";

    public InheritingMissingTypeDefinitionException(string typeName)
      : base(null, string.Format(msgBase, typeName)) { }
  }

  public class EmbeddedTypeUnknownException : SettingsParsingException
  {
    public static string msgBase = "Embedded node definition {0} wants a type of {1}, but {1} was not defined";

    public EmbeddedTypeUnknownException(LoreEmbeddedNodeDefinition typeDef, string unknownType)
      : base(typeDef, string.Format(msgBase, typeDef.name, unknownType)) { }
  }

  public class EmbeddedTypeNotGivenException : SettingsParsingException
  {
    public static string msgBase = "Embedded Node definition {0} did not have entryTypeName property defined!";

    public EmbeddedTypeNotGivenException(LoreEmbeddedNodeDefinition embeddedDef)
      : base(embeddedDef, string.Format(msgBase, embeddedDef.name)) { }
  }
}
