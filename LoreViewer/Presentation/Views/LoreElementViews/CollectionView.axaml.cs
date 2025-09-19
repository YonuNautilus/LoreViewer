using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using LoreViewer.Presentation.ViewModels.LoreEntities.LoreElements;
using System;
using System.Globalization;

namespace LoreViewer.Presentation.Views.LoreElementViews;

public partial class CollectionView : UserControl
{
  public CollectionView()
  {
    InitializeComponent();
  }

}
public class CollectionTemplateConverter : IValueConverter
{
  public IDataTemplate? CollectionsTemplate { get; set; }
  public IDataTemplate? NodesTemplate { get; set; }
  public IDataTemplate? ErrorTemplate { get; set; }

  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is not LoreCollectionViewModel) return null;

    LoreCollectionViewModel col = value as LoreCollectionViewModel;

    if (col.ContainsNodes) return NodesTemplate;
    if (col.ContainsCollections) return CollectionsTemplate;
    return ErrorTemplate;
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    throw new NotImplementedException();
  }
}