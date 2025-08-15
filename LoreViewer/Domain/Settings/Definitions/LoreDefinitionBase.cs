using SharpYaml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace LoreViewer.Domain.Settings.Definitions
{

  public abstract class LoreDefinitionBase
  {
    [YamlIgnore]
    /// The definition this definition inherits from
    public LoreDefinitionBase Base { get; protected set; }

    private string m_sName = string.Empty;

    [DefaultValue("")]
    [YamlMember(-100)]
    public string name
    {
      get
      {
        if (this is LoreTypeDefinition) return m_sName;
        if (Base == null) return m_sName;
        else return Base.name;
      }
      set
      {
        if (this is LoreTypeDefinition) m_sName = value;
        if (Base != null)
          Trace.WriteLine("Trying to set name for inheriting Definition");
        else
          m_sName = value;
      }
    }

    private bool m_bIsDeleted;
    [YamlIgnore]
    public bool IsDeleted = false;
    [YamlIgnore]
    public bool WasDeleted
    {
      get
      {
        if (IsDeleted) return true;

        if (Base != null) return Base.WasDeleted;

        return false;
      }
    }

    public LoreDefinitionBase() { }

    public LoreDefinitionBase(string name) { this.name = name; }

    public abstract void PostProcess(LoreSettings settings);

    public override string ToString() => name;

    [YamlIgnore]
    public bool processed = false;

    [YamlIgnore]
    public bool IsInherited => Base != null;

    public abstract bool IsModifiedFromBase { get; }


    internal abstract void MakeIndependent();
  }

  public static class DefinitionMergeManager
  {
    public static List<LoreFieldDefinition> MergeFields(List<LoreFieldDefinition> baseFields, List<LoreFieldDefinition> childFields)
    {
      if (baseFields == null && childFields == null) return null;

      if (baseFields == null && childFields != null) return childFields;

      if (baseFields != null && childFields == null) return baseFields = baseFields.Select(f => f.CloneFromBase()).ToList();

      List<LoreFieldDefinition> ret = new();

      string[] _allFieldNames = baseFields.Select(f => f.name).Concat(childFields.Select(f => f.name)).Distinct().ToArray();


      foreach (string fieldName in _allFieldNames)
      {

        LoreFieldDefinition resultingField = null;
        LoreFieldDefinition childField = childFields.FirstOrDefault(f => f.name == fieldName);
        LoreFieldDefinition baseField = baseFields.FirstOrDefault(f => f.name == fieldName);


        if (childField != null && baseField != null)
        {
          resultingField = childField;
          resultingField.MergeFrom(baseField);
          resultingField.fields = MergeFields(baseField.fields, resultingField.fields);
        }
        else if (childField == null && baseField != null)
        {
          resultingField = baseField.CloneFromBase();
          resultingField.MergeFrom(baseField);
        }
        else if (childField != null && baseField == null)
        {
          // For re-processing -- if the child field has a Base set (ie IsInherited is true), but there is no corresponding baseField, we can assume that the base has been deleted.
          // Therefore, we can skip adding the child field
          if (childField.IsInherited)
            continue;
          else
            resultingField = childField;
        }

        ret.Add(resultingField);
      }

      return ret;
    }
    public static List<LoreSectionDefinition> MergeSections(List<LoreSectionDefinition> baseSections, List<LoreSectionDefinition> childSections)
    {
      if (baseSections == null && childSections == null) return null;

      if (baseSections == null && childSections != null) return childSections;

      if (baseSections != null && childSections == null) return baseSections.Select(s => s.CloneFromBase() as LoreSectionDefinition).ToList();

      List<LoreSectionDefinition> ret = new List<LoreSectionDefinition>();

      string[] _allSectionNames = baseSections.Select(f => f.name).Concat(childSections.Select(f => f.name)).Distinct().ToArray();

      foreach (string sectionName in _allSectionNames)
      {

        LoreSectionDefinition resultingSection = null;
        LoreSectionDefinition childSection = childSections.FirstOrDefault(s => s.name == sectionName);
        LoreSectionDefinition baseSection = baseSections.FirstOrDefault(s => s.name == sectionName);

        if (childSection != null && baseSection != null)
        {
          resultingSection = childSection;
          resultingSection.MergeFrom(baseSection);
          resultingSection.fields = MergeFields(baseSection.fields, resultingSection.fields);
          resultingSection.sections = MergeSections(baseSection.sections, resultingSection.sections);
        }
        else if (childSection == null && baseSection != null)
        {
          resultingSection = baseSection.CloneFromBase();
          resultingSection.MergeFrom(baseSection);
        }
        else if (childSection != null && baseSection == null)
        {
          // For re-processing -- if the child section has a Base set (ie IsInherited is true), but there is no corresponding baseSection, we can assume that the base has been deleted.
          // Therefore, we can skip adding the child section
          if (childSection.IsInherited)
            continue;
          resultingSection = childSection;
        }

        ret.Add(resultingSection);
      }

      return ret;
    }

    internal static List<LoreCollectionDefinition>? MergeCollections(List<LoreCollectionDefinition>? baseCols, List<LoreCollectionDefinition>? childCols, LoreDefinitionBase baseOwningDefinition = null)
    {
      if (CollectionHelpers.CollectionNullOrEmpty(baseCols) && CollectionHelpers.CollectionNullOrEmpty(childCols)) return null;

      if (CollectionHelpers.CollectionNullOrEmpty(baseCols) && !CollectionHelpers.CollectionNullOrEmpty(childCols)) return childCols;

      if (!CollectionHelpers.CollectionNullOrEmpty(baseCols) && CollectionHelpers.CollectionNullOrEmpty(childCols)) return baseCols.Select(c => c.CloneFromBaseWithOwner(baseOwningDefinition) as LoreCollectionDefinition).ToList();

      List<LoreCollectionDefinition> ret = new List<LoreCollectionDefinition>();

      string[] _allCollectionNames = baseCols.Select(f => f.name).Concat(childCols.Select(f => f.name)).Distinct().ToArray();

      foreach (string collectionName in _allCollectionNames)
      {

        LoreCollectionDefinition resultingCollection = null;
        LoreCollectionDefinition childCollection = childCols.FirstOrDefault(c => c.name == collectionName);
        LoreCollectionDefinition baseCollection = baseCols.FirstOrDefault(c => c.name == collectionName);

        if (childCollection != null && baseCollection != null)
        {
          resultingCollection = childCollection;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection == null && baseCollection != null)
        {
          resultingCollection = baseCollection.CloneFromBase() as LoreCollectionDefinition;
          resultingCollection.MergeFrom(baseCollection);
        }
        else if (childCollection != null && baseCollection == null)
        {
          // For re-processing -- if the child collection has a Base set (ie IsInherited is true), but there is no corresponding baseEmbedded, we can assume that the base has been deleted.
          // Therefore, we can skip adding the child collection
          if (childCollection.IsInherited)
            continue;
          resultingCollection = childCollection;
        }

        ret.Add(resultingCollection);
      }

      return ret;
    }

    internal static List<LoreEmbeddedNodeDefinition> MergeEmbeddedNodeDefs(List<LoreEmbeddedNodeDefinition> baseEmbds, List<LoreEmbeddedNodeDefinition> childEmbds)
    {
      if (baseEmbds == null && childEmbds == null) return null;

      if (baseEmbds == null && childEmbds != null) return childEmbds;

      if (baseEmbds != null && childEmbds == null) return baseEmbds.Select(e => e.CloneFromBase() as LoreEmbeddedNodeDefinition).ToList();

      List<LoreEmbeddedNodeDefinition> ret = new List<LoreEmbeddedNodeDefinition>();

      string[] _allEmbeddedNames = baseEmbds.Select(f => f.name).Concat(childEmbds.Select(f => f.name)).Distinct().ToArray();

      foreach (string embeddedName in _allEmbeddedNames)
      {

        LoreEmbeddedNodeDefinition resultingEmbedded = null;
        LoreEmbeddedNodeDefinition childEmbedded = childEmbds.FirstOrDefault(e => e.name == embeddedName);
        LoreEmbeddedNodeDefinition baseEmbedded = baseEmbds.FirstOrDefault(e => e.name == embeddedName);

        if (childEmbedded != null && baseEmbedded != null)
        {
          resultingEmbedded = childEmbedded;
          resultingEmbedded.MergeFrom(baseEmbedded);
        }
        else if (childEmbedded == null && baseEmbedded != null)
        {
          resultingEmbedded = baseEmbedded.CloneFromBase();
          resultingEmbedded.MergeFrom(baseEmbedded);
        }
        else if (childEmbedded != null && baseEmbedded == null)
        {
          // For re-processing -- if the child embedded has a Base set (ie IsInherited is true), but there is no corresponding base, we can assume that the base has been deleted.
          // Therefore, we can skip adding the child embedded
          if (childEmbedded.IsInherited)
            continue;
          resultingEmbedded = childEmbedded;
        }

        ret.Add(resultingEmbedded);
      }

      return ret;
    }
  }

  public static class CollectionHelpers
  {
    public static bool CollectionNullOrEmpty(IEnumerable<object> col) => col == null || !col.Any();

    public static IEnumerable<LorePicklistEntryDefinition> FlattenPicklistEntries(this IEnumerable<LorePicklistEntryDefinition> entries)
    {
      foreach (var entry in entries)
      {
        yield return entry;

        if (entry.HasEntries)
          foreach (var child in entry.entries.FlattenPicklistEntries())
            yield return child;
      }
    }
  }

  public static class StringHelpers
  {
    public const string EMOJI_PATTERN = @"\p{Cs}|[^\u0000-\u007F]";
    public static string TrimEmoji(this string text) => Regex.Replace(text, EMOJI_PATTERN, "").Trim();
  }
}
