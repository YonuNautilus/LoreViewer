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

    private ObservableCollection<FieldDefinitionViewModel> m_cFields = new ObservableCollection<FieldDefinitionViewModel>();

    public override ObservableCollection<FieldDefinitionViewModel> Fields { get =>  m_cFields; }

    public EFieldStyle Style { get => fieldDef.style; set => fieldDef.style = value; }

    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion) { }


    public override void RefreshLists()
    {
      
    }
  }
}
