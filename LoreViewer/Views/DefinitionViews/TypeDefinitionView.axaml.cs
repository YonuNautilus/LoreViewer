using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using LoreViewer.ViewModels.SettingsVMs;

namespace LoreViewer.Views.DefinitionViews;

public partial class TypeDefinitionView : DefinitionView
{
  public TypeDefinitionView() : base() { }
  public TypeDefinitionView(TypeDefinitionViewModel viewModel) : base(viewModel)
  {
    Name = viewModel.Name;
    InitializeComponent();
  }
}