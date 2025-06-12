using Avalonia.Interactivity;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels.SettingsVMs
{
  public class SectionDefinitionViewModel : LoreDefinitionViewModel
  {
    private LoreSectionDefinition secDef { get => Definition as LoreSectionDefinition; }
    public bool IsRequired { get => secDef.required; set => secDef.required = value; }
    public ObservableCollection<SectionDefinitionViewModel> Sections { get; }
    public ObservableCollection<FieldDefinitionViewModel> Fields { get; }
    public LoreDefinitionViewModel CurrentlySelectedDefinition { get; set; }
    public SectionDefinitionViewModel(LoreSectionDefinition definition) : base(definition) { }

  }
}
