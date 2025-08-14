using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LoreViewer.LoreElements;
using LoreViewer.Settings;
using LoreViewer.Validation;
using LoreViewer.ViewModels;
using LoreViewer.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LoreViewer.Converters
{
  /// <summary>
  /// <see href="https://code.4noobz.net/wpf-enum-binding-with-description-in-a-combobox/">SOURCE</see>
  /// </summary>
  public class EnumDescriptionConverter : IValueConverter
  {
    private string GetEnumDescription(Enum enumObj)
    {
      if (enumObj == null) return "!!!";
      FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());
      object[] attribArray = fieldInfo.GetCustomAttributes(false);

      if (attribArray.Length == 0)
        return enumObj.ToString();
      else
      {
        DescriptionAttribute attrib = null;

        foreach (var att in attribArray)
        {
          if (att is DescriptionAttribute)
            attrib = att as DescriptionAttribute;
        }

        if (attrib != null)
          return attrib.Description;

        return enumObj.ToString();
      }
    }

    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Enum myEnum = (Enum)value;
      string description = GetEnumDescription(myEnum);
      return description;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return string.Empty;
    }
  }

  public class BoolToGridLengthConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool b)
        return b ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
      return new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  public class AllowedStyleConverter : IMultiValueConverter
  {
    public object Convert(IList<object?> values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Count < 2)
        return false;

      if (values[0] is EFieldContentType style && values[1] is IEnumerable allowedStyles)
      {
        foreach (var allowed in allowedStyles)
        {
          if (allowed is EFieldContentType allowedStyle && allowedStyle == style)
            return true;
        }
      }

      return false;
    }

    public object ConvertBack(IList values, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  public class ViewModeToViewConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if(value is LoreViewModel lvm)
      {
        switch (lvm.ViewMode)
        {
          case ViewModels.PrimaryViewModels.EStartupMode.Readonly:
            return new LoreReadonlyView();
          case ViewModels.PrimaryViewModels.EStartupMode.Edit:
            return new LoreEditView();
        }
      }
      return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  public class GreaterThanZero : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is IList ie) return ie.Count > 0;
      else if (value is int i) return i > 0;
      else return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  public class CollapseExpandIconConverter : IValueConverter
  {
    public object? Convert(object? value, Type targetType, object? isLeftRight, CultureInfo culture)
    {
      // Value will be a bool, true for 'is visible', false for 'is collapsed'
      // If currently visible, show the 'collapse icon', if currently collapsed, show the 'expand icon'

      // Parameter is a bool as well. True for left-right, false for up-down

      if (value is bool bVisible)
      {
        string image = string.Empty;

        if(isLeftRight is bool lr && lr)
        {
          if (bVisible) image = "avares://LoreViewer/Resources/ChevronRight24.png";
          else image = "avares://LoreViewer/Resources/ChevronLeft24.png";
        }
        else
        {
          if (bVisible) image = "avares://LoreViewer/Resources/ChevronDown24.png";
          else image = "avares://LoreViewer/Resources/ChevronUp24.png";
        }
        return new Bitmap(AssetLoader.Open(new Uri(image)));
        
      }
      return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
}
