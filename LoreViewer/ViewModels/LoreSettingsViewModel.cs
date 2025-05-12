using ReactiveUI;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoreViewer.ViewModels
{
    internal class LoreSettingsViewModel : ViewModelBase
  {

    private string m_sLoreLibraryFolderPath = string.Empty;
    public string LoreLibraryFolderPath { get => m_sLoreLibraryFolderPath; set => this.RaiseAndSetIfChanged(ref m_sLoreLibraryFolderPath, value); }

    private LoreSettings m_oLoreSettings;
    public LoreSettings LoreSettings { get => m_oLoreSettings; set => this.RaiseAndSetIfChanged(ref m_oLoreSettings, value); }

    public LoreSettingsViewModel() { }

    
  }
}
