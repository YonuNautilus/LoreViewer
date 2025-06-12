using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class FieldDefinitionViewModel : LoreDefinitionViewModel
  {
    public static List<EFieldStyle> FieldStyles { get => Enum.GetValues(typeof(EFieldStyle)).Cast<EFieldStyle>().ToList(); }

    private LoreFieldDefinition fieldDef { get => Definition as LoreFieldDefinition; }
    public bool IsRequired { get => fieldDef.required; }

    public EFieldStyle Style { get => fieldDef.style; set => fieldDef.style = value; }

    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion) { }
  }
}
