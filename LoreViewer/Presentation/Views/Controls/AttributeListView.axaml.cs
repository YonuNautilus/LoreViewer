using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;

namespace LoreViewer.Presentation.Views.Controls
{
  public partial class AttributeListView : UserControl
  {
    public readonly static StyledProperty<ObservableCollection<LoreAttributeViewModel>> AttributesProperty
      = AvaloniaProperty.Register<AttributeListView, ObservableCollection<LoreAttributeViewModel>>(nameof(Attributes));

    public ObservableCollection<LoreAttributeViewModel> Attributes
    {
      get => GetValue(AttributesProperty);
      set => SetValue(AttributesProperty, value);
    }

    public AttributeListView()
    {
      InitializeComponent();
    }
  }

  public class AttributeTemplateConverter : IValueConverter
  {
    public IDataTemplate? SingleValue { get; set; }
    public IDataTemplate? MultiValue { get; set; }
    public IDataTemplate? NestedAttrs { get; set; }
    
    public IDataTemplate? ErrorAttr { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      if (value is not LoreAttributeViewModel) return null;
      
      LoreAttributeViewModel attribute = value as LoreAttributeViewModel;
      
      if (attribute.HasSingleValue) return SingleValue;
      if (attribute.HasMultipleValues) return MultiValue;
      if (attribute.HasNestedAttributes) return NestedAttrs;
      return ErrorAttr;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}