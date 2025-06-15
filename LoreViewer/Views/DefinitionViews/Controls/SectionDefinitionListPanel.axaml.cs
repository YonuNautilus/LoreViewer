using Avalonia;
using Avalonia.Controls;
using LoreViewer.ViewModels.SettingsVMs;
using System.Collections.Generic;
using System.Windows.Input;

namespace LoreViewer.Views.DefinitionViews;

public partial class SectionDefinitionListPanel : UserControl
{
  public static readonly StyledProperty<IEnumerable<SectionDefinitionViewModel>> ItemsSourceProperty =
        AvaloniaProperty.Register<SectionDefinitionListPanel, IEnumerable<SectionDefinitionViewModel>>(nameof(ItemsSource));

  public IEnumerable<SectionDefinitionViewModel> ItemsSource
  {
    get => GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
  }

  public static readonly StyledProperty<ICommand> AddSectionCommandProperty =
        AvaloniaProperty.Register<SectionDefinitionListPanel, ICommand>(nameof(AddSectionCommand));

  public ICommand AddSectionCommand
  {
    get => GetValue(AddSectionCommandProperty);
    set => SetValue(AddSectionCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> DeleteSectionCommandProperty =
        AvaloniaProperty.Register<SectionDefinitionListPanel, ICommand>(nameof(DeleteSectionCommand));

  public ICommand DeleteSectionCommand
  {
    get => GetValue(DeleteSectionCommandProperty);
    set => SetValue(DeleteSectionCommandProperty, value);
  }

  public static readonly StyledProperty<ICommand> EditSectionCommandProperty =
        AvaloniaProperty.Register<SectionDefinitionListPanel, ICommand>(nameof(EditSectionCommand));

  public ICommand EditSectionCommand
  {
    get => GetValue(EditSectionCommandProperty);
    set => SetValue(EditSectionCommandProperty, value);
  }

  public SectionDefinitionListPanel()
  {
    InitializeComponent();
  }
}