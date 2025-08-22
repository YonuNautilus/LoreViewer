using LoreViewer.Exceptions.SettingsParsingExceptions;
using SharpYaml.Serialization;
using System;
using System.ComponentModel;
using System.Linq;

namespace LoreViewer.Domain.Settings.Definitions
{
  public class LoreCollectionDefinition : LoreDefinitionBase, IRequirable, IDeepCopyable<LoreCollectionDefinition>
  {

    [DefaultValue("")]
    public string entryTypeName { get; set; } = string.Empty;

    // This is used if the contained type is a globally defined collection
    [DefaultValue("")]
    public string entryCollectionName { get; set; } = string.Empty;

    // This is THE locally defined collection, if used
    public LoreCollectionDefinition entryCollection { get; set; }

    private LoreDefinitionBase m_oContainedGlobalDef;

    [YamlIgnore]
    public LoreDefinitionBase ContainedType
    {
      get
      {
        if (entryCollection != null) return entryCollection;
        else return m_oContainedGlobalDef;
      }
    }

    public void SetContainedType(LoreDefinitionBase def)
    {
      if (def is LoreCollectionDefinition c)
      {
        if (c.OwningDefinition != null)
        {
          // It's inline
          entryCollection = c;
          entryTypeName = "";
          entryCollectionName = "";
        }
        else
        {
          // It's global
          entryCollection = null;
          entryTypeName = "";
          entryCollectionName = c.name;
          m_oContainedGlobalDef = def;
        }
      }
      else if (def is LoreTypeDefinition t)
      {
        entryTypeName = t.name;
        entryCollectionName = "";
        entryCollection = null;
        m_oContainedGlobalDef = def;
      }
    }



    /// <summary>
    /// Originally returned true if OwningDefinition was set -- Collections definitions could only be local if owned by some other definition
    /// (ie a type, or other collection). But locally defined collections at the level 1 heading would end up not being considered 'locally defined'
    /// because they had no owning definition set.
    /// Now this is manually set when creating locally defined collection definitions of any kind.
    /// </summary>
    [YamlIgnore]
    public bool IsLocallyDefined { get; set; } = false;

    [YamlIgnore]
    public LoreDefinitionBase? OwningDefinition { get; set; }

    [YamlIgnore]
    public bool IsUsingLocallyDefinedCollection
    {
      get
      {
        return entryCollection != null ? entryCollection.IsLocallyDefined : false;
      }
    }

    [DefaultValue(false)]
    public bool SortEntries { get; set; }

    [DefaultValue(false)]
    public bool required { get; set; }

    public bool IsCollectionOfCollections => ContainedType is LoreCollectionDefinition;

    private bool MoreThanOneTrue(params bool[] booleans) => booleans.Where(b => b == true).Count() > 1;

    public override void PostProcess(LoreSettings settings)
    {
      // If using a locally defined collection definition
      if (entryCollection != null)
      {
        entryCollection.OwningDefinition = this;
        if (IsInherited)
        {
          if (entryCollection == (Base as LoreCollectionDefinition).entryCollection)
            entryCollection = (Base as LoreCollectionDefinition).entryCollection.CloneFromBase();
          entryCollection.MergeFrom((Base as LoreCollectionDefinition).entryCollection);
        }
        entryCollection.PostProcess(settings);
      }

      if (MoreThanOneTrue(!string.IsNullOrWhiteSpace(entryTypeName), entryCollection != null, !string.IsNullOrWhiteSpace(entryCollectionName)))
        throw new CollectionWithMultipleEntriesDefined(this);

      // Globally defined collection
      if (!string.IsNullOrWhiteSpace(entryTypeName))
      {
        if (settings.HasTypeDefinition(entryTypeName))
          SetContainedType(settings.GetTypeDefinition(entryTypeName));
        else
          throw new Exception($"Could not find type ({entryTypeName}) definition for collection {name}");
      }
      else if (!string.IsNullOrWhiteSpace(entryCollectionName))
      {
        if (settings.HasCollectionDefinition(entryCollectionName))
          SetContainedType(settings.GetCollectionDefinition(entryCollectionName));
        else
          throw new Exception($"Could not find type ({entryCollectionName} definition for collection {name}");
      }
    }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (required != (Base as LoreCollectionDefinition).required) return true;

        if (IsUsingLocallyDefinedCollection)
        {
          if (ContainedType.IsModifiedFromBase) return true;
        }
        else if (ContainedType != (Base as LoreCollectionDefinition).ContainedType) return true;

        return false;
      }
    }

    public void MergeFrom(LoreCollectionDefinition baseCollection)
    {
      Base = baseCollection;

      // make sure to skip if ContainedType hasn't yet been resolved
      if (baseCollection.ContainedType == null)
      {

      }

      else if (ContainedType == null)
      {
        if (baseCollection.IsUsingLocallyDefinedCollection) SetContainedType((baseCollection.ContainedType as LoreCollectionDefinition).CloneFromBaseWithOwner(this));
      }
      else if (!baseCollection.IsCollectionOfCollections && !IsCollectionOfCollections)
      {
        if (!(ContainedType as LoreTypeDefinition).IsATypeOf(baseCollection.ContainedType as LoreTypeDefinition))
          SetContainedType(baseCollection.ContainedType);
      }
      else if (baseCollection.IsCollectionOfCollections && IsCollectionOfCollections && (ContainedType as LoreCollectionDefinition).IsLocallyDefined)
      {
        (ContainedType as LoreCollectionDefinition).MergeFrom(baseCollection.ContainedType as LoreCollectionDefinition);
      }

      required |= baseCollection.required;
    }

    public LoreCollectionDefinition Clone()
    {
      // Keep ContainedType as a reference
      LoreCollectionDefinition colDef = MemberwiseClone() as LoreCollectionDefinition;
      colDef.SetContainedType(ContainedType);
      return colDef;
    }

    public LoreCollectionDefinition CloneFromBase()
    {
      LoreCollectionDefinition colDef = MemberwiseClone() as LoreCollectionDefinition;

      // If entryCollectionName is NOT null or empty, then we know 'this' collection definition's contents are a GLOBALLY DEFINED collection, and should NOT have BASE SET
      //if (string.IsNullOrEmpty(entryCollectionName))
      colDef.Base = this;

      if (IsUsingLocallyDefinedCollection)
        colDef.entryCollection = entryCollection.CloneFromBase();

      return colDef;
    }

    public LoreCollectionDefinition CloneFromBaseWithOwner(LoreDefinitionBase owner)
    {
      LoreCollectionDefinition colDef = CloneFromBase();
      colDef.OwningDefinition = this;
      return colDef;
    }

    internal override void MakeIndependent()
    {
      Base = null;

      if (IsCollectionOfCollections && IsUsingLocallyDefinedCollection) ContainedType.MakeIndependent();
    }
  }
}
