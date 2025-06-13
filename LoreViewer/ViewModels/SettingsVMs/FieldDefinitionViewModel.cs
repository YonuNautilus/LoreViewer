using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class FieldDefinitionViewModel : LoreDefinitionViewModel
  {
    #region overrides
    public override ObservableCollection<TypeDefinitionViewModel> Types => null;
    public override ObservableCollection<SectionDefinitionViewModel> Sections => null;
    public override ObservableCollection<CollectionDefinitionViewModel> Collections => null;
    public override ObservableCollection<EmbeddedNodeDefinitionViewModel> EmbeddedNodes => null;
    #endregion

    public static List<EFieldStyle> FieldStyles { get => Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().ToList(); }

    private LoreFieldDefinition fieldDef { get => Definition as LoreFieldDefinition; }
    public bool IsRequired { get => fieldDef.required; }

    public bool HasSubFields { get => fieldDef.HasFields; }
    public bool NoSubFields { get => !fieldDef.HasFields; }


    public bool IsThreeState { get => fieldDef.HasRequiredNestedFields; }

    public bool? RequiredState
    {
      get
      {
        if (fieldDef.required) return true;
        else if (fieldDef.HasRequiredNestedFields && !fieldDef.required) return null;
        else return false;
      }
      set
      {
        // It seems that clicking on a check while it is in the third state will try to set the bool to null.
        fieldDef.required = (value != null);
      }
    }


    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get =>  m_cFields; }

    public EFieldStyle Style { get => fieldDef.style; set => fieldDef.style = value; }

    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion) { }

    private void RefreshFieldDefs()
    {
      m_cFields.Clear();
      if (fieldDef.fields != null)
        foreach (LoreFieldDefinition def in fieldDef.fields)
          m_cFields.Add(new FieldDefinitionViewModel(def));
    }

    public override void RefreshLists()
    {
      RefreshFieldDefs();
    }
  }
}
