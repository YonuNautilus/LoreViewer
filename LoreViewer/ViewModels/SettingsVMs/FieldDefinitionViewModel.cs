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
    public bool Required { get => (Definition as LoreFieldDefinition).required; }
    public FieldDefinitionViewModel(LoreFieldDefinition defintion) : base(defintion) { }
  }
}
