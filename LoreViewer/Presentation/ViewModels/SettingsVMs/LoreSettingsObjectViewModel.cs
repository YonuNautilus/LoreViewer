using LoreViewer.Domain.Settings;
using LoreViewer.Presentation.ViewModels;
using System.Collections.Generic;

namespace LoreViewer.Presentation.ViewModels.SettingsVMs
{
  public abstract class LoreSettingsObjectViewModel : ViewModelBase
  {
    public static LoreSettings CurrentSettings { get; }
  }

  public static class CollectionHelpers
  {
    public static IEnumerable<PicklistEntryDefinitionViewModel> FlattenPicklistEntryViewModels(this IEnumerable<PicklistEntryDefinitionViewModel> source)
    {
      foreach (var item in source)
      {
        yield return item;
        foreach (var child in item.PicklistEntries.FlattenPicklistEntryViewModels())
        {
          yield return child;
        }
      }
    }
  }
}
