using Avalonia;
using Avalonia.Controls;

namespace LoreViewer.Presentation.Views.LoreElementViews;

public partial class AttributeValueView : UserControl
{
  public static readonly StyledProperty<bool> IsReadOnlyProperty =
  AvaloniaProperty.Register<NodeViewBasic, bool>(nameof(IsReadOnly), defaultValue: false);

  public bool IsReadOnly
  {
    get => GetValue(IsReadOnlyProperty);
    set => SetValue(IsReadOnlyProperty, value);
  }

  public bool IsEditMode
  {
    get => !IsReadOnly;
  }

  public AttributeValueView()
  {
    InitializeComponent();
  }
}