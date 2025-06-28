using LoreViewer.Exceptions.SettingsParsingExceptions;
using LoreViewer.Settings.Interfaces;
using SharpYaml.Serialization;
using System;
using System.ComponentModel;

namespace LoreViewer.Settings
{
  public class LoreEmbeddedNodeDefinition : LoreDefinitionBase, IRequirable, IDeepCopyable<LoreEmbeddedNodeDefinition>
  {
    private string m_sEntryTypeName = string.Empty;
    public string entryTypeName
    {
      get
      {
        if (nodeType == null) return m_sEntryTypeName;
        else return nodeType.name;
      }
      set => m_sEntryTypeName = value;
    }

    [DefaultValue(false)]
    public bool required { get; set; }

    private LoreTypeDefinition m_oNodeType;

    [YamlIgnore]
    public LoreTypeDefinition nodeType
    {
      get
      {
        return m_oNodeType;
      }
      set
      {
        if (IsInherited && !value.IsATypeOf((Base as LoreEmbeddedNodeDefinition).nodeType)) throw new Exception("Cannot change node type of inherited embedded node definition to an entirely different type");
        else m_oNodeType = value;
      }
    }

    [YamlIgnore]
    public bool hasTitleRequirement => !string.IsNullOrWhiteSpace(name);

    public override void PostProcess(LoreSettings settings)
    {
      if (string.IsNullOrEmpty(entryTypeName))
        throw new EmbeddedTypeNotGivenException(this);

      LoreTypeDefinition foundNodeType = settings.GetTypeDefinition(entryTypeName);

      if (foundNodeType == null)
        throw new EmbeddedTypeUnknownException(this, entryTypeName);
      else
        nodeType = foundNodeType;
    }

    public override bool IsModifiedFromBase
    {
      get
      {
        if (Base == null) return true;

        if (this.required != (Base as LoreEmbeddedNodeDefinition).required) return true;

        if (this.nodeType != (Base as LoreEmbeddedNodeDefinition).nodeType) return true;

        return false;
      }
    }

    public void MergeFrom(LoreEmbeddedNodeDefinition baseEmbedded)
    {
      Base = baseEmbedded;

      // Skip if NodeType has been resolved yet
      if (baseEmbedded.nodeType == null)
      {

      }
      else if (!(this.nodeType as LoreTypeDefinition).IsATypeOf(baseEmbedded.nodeType as LoreTypeDefinition))
        nodeType = (baseEmbedded.nodeType);

      this.required |= baseEmbedded.required;
    }

    public LoreEmbeddedNodeDefinition Clone()
    {
      LoreEmbeddedNodeDefinition emNodeDef = this.MemberwiseClone() as LoreEmbeddedNodeDefinition;
      emNodeDef.Base = this;

      return emNodeDef;
    }

    public LoreEmbeddedNodeDefinition CloneFromBase()
    {
      LoreEmbeddedNodeDefinition enodeDef = Clone();
      enodeDef.Base = this;
      return enodeDef;
    }

    internal override void MakeIndependent()
    {
      this.Base = null;
    }
  }

}
