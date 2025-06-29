using Avalonia;
using LoreViewer.Settings;
using LoreViewer.Settings.Interfaces;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

namespace LoreViewer.ViewModels.SettingsVMs
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
