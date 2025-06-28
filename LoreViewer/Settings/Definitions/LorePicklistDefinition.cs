using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LoreViewer.Settings
{

  public class LorePicklistDefinition : LoreDefinitionBase, IPicklistEntryDefinitionContainer, IDeepCopyable<LorePicklistDefinition>
  {
    [YamlIgnore]
    public bool HasEntries => entries != null && entries.Count() > 0;

    public List<LorePicklistEntryDefinition> entries { get; set; }
    public override bool IsModifiedFromBase => true;

    [YamlIgnore]
    public bool isBranch = false;

    public override void PostProcess(LoreSettings settings)
    {
      if (HasEntries)
        entries.ForEach(entry => entry.PostProcess(settings));
    }

    internal override void MakeIndependent() { }

    public LorePicklistDefinition Clone()
    {
      LorePicklistDefinition pDef = new();
      pDef.name = this.name;
      pDef.entries = this.entries?.Select(ple => ple.Clone()).ToList();
      return pDef;
    }

    public LorePicklistDefinition CloneFromBase() => throw new NotImplementedException();
  }
}
