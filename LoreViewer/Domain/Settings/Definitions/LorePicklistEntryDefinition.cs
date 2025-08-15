using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Domain.Settings.Definitions
{
  public class LorePicklistEntryDefinition : LoreDefinitionBase, IPicklistEntryDefinitionContainer, IDeepCopyable<LorePicklistEntryDefinition>
  {
    #region IPicklistDefinitionContainer
    [YamlMember(1)]
    public List<LorePicklistEntryDefinition> entries { get; set; }

    public bool HasPicklistDefinition(string listItemName) => entries.Any(t => listItemName == t.name);

    public bool HasEntries => entries != null && entries.Count() > 0;

    public void AddPicklistDefinition(LorePicklistEntryDefinition picklistDefinition)
    {
      if (entries == null) entries = new List<LorePicklistEntryDefinition>();
      if (!entries.Contains(picklistDefinition)) entries.Add(picklistDefinition);
    }
    #endregion IPicklistDefinitionContainer

    public override bool IsModifiedFromBase => true;

    public LorePicklistEntryDefinition() { }

    /// <summary>
    /// Turn this entry and its subentries into a separate Picklist, used for value selection when a field has Picklist branch restricted
    /// </summary>
    /// <returns></returns>
    public LorePicklistDefinition MakePicklistFromEntry()
    {
      LorePicklistDefinition retList = new LorePicklistDefinition();
      // Dont clone the items in the list, just make a new list
      retList.entries = entries?.Select(e => e).ToList() ?? new List<LorePicklistEntryDefinition>();
      retList.entries.Insert(0, this);
      retList.isBranch = true;
      return retList;
    }

    [YamlIgnore]
    LorePicklistEntryDefinition? OwningEntry;

    public override void PostProcess(LoreSettings settings)
    {
      if (HasEntries)
      {
        entries.ForEach(entry => entry.OwningEntry = this);
        entries.ForEach(entry => entry.PostProcess(settings));
      }
    }

    internal override void MakeIndependent()
    {

    }

    public LorePicklistEntryDefinition Clone()
    {
      LorePicklistEntryDefinition oClone = new LorePicklistEntryDefinition();
      oClone.name = name;
      if (HasEntries)
        oClone.entries = entries.Select(o => o.Clone()).ToList();
      return oClone;
    }

    public LorePicklistEntryDefinition CloneFromBase()
    {
      LorePicklistEntryDefinition oClone = Clone();
      oClone.Base = this;
      return oClone;
    }
  }
}
