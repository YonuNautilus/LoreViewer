using System.Linq;

namespace LoreViewer.Settings
{
  public static class LorePruner
  {
    public static void Prune(LoreSettings settings)
    {
      foreach (var typeDef in settings.types)
      {
        PruneTypeDefinition(typeDef);
      }
      
      settings.collections = settings.collections.Where(c => c.IsModifiedFromBase).ToList();
    }


    private static void PruneTypeDefinition(LoreTypeDefinition typeDef)
    {
      typeDef.fields = typeDef.fields?.Where(field => field.IsModifiedFromBase).ToList();

      typeDef.sections = typeDef.sections?.Where(section => section.IsModifiedFromBase).ToList();

      typeDef.collections = typeDef.collections?.Where(collection => collection.IsModifiedFromBase).ToList();

      typeDef.embeddedNodeDefs = typeDef.embeddedNodeDefs?.Where(embedded => embedded.IsModifiedFromBase).ToList();

      // Recursively prune subfields, subsections, etc
      if (typeDef.fields != null)
      {
        foreach (var field in typeDef.fields)
          PruneField(field);
        if (typeDef.fields.Count == 0) typeDef.fields = null;
      }

      if (typeDef.sections != null)
      {
        foreach (var section in typeDef.sections)
          PruneSection(section);
        if (typeDef.sections.Count == 0) typeDef.sections = null;
      }

      if(typeDef.collections != null)
      {
        foreach (var collection in typeDef.collections)
          PruneCollection(collection);
        if (typeDef.collections.Count == 0) typeDef.collections = null;
      }

    }


    private static void PruneField(LoreFieldDefinition field)
    {
      if (field.fields != null)
      {
        field.fields = field.fields.Where(subfield => subfield.IsModifiedFromBase).ToList();

        foreach (var subfield in field.fields)
          PruneField(subfield);
      }
    }

    private static void PruneSection(LoreSectionDefinition section)
    {
      if (section.sections != null)
      {
        section.sections = section.sections
            .Where(subsection => subsection.IsModifiedFromBase)
            .ToList();

        foreach (var subsection in section.sections)
          PruneSection(subsection);
      }
    }

    private static void PruneCollection(LoreCollectionDefinition collection)
    {
      
    }
  }
}
