using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.OpenGL;
using Avalonia.Platform;
using LoreViewer.Core.Validation;
using LoreViewer.Domain.Entities;
using LoreViewer.Domain.Settings.Definitions;
using LoreViewer.Presentation;
using LoreViewer.Presentation.ViewModels;
using LoreViewer.Presentation.ViewModels.PrimaryViewModels;
using LoreViewer.Presentation.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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


  public class ViewModeToViewConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is LoreViewModel lvm)
      {
        switch (lvm.ViewMode)
        {
          case EStartupMode.Readonly:
            return new LoreReadonlyView();
          case EStartupMode.Edit:
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

        if (isLeftRight is bool lr && lr)
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

  public class ValidationStateToImagePathConverter : IValueConverter
  {

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      // Value will be the datagrid node (LoreTreeItem)

      // if there is a LoreEntity associated with the selected DataGrid node...
      if (value != null && value is EValidationState vs)
      {
        string image = string.Empty;
        //EValidationState elementState = lvr.LoreEntityValidationStates.ContainsKey(e) ?
        //  lvr.LoreEntityValidationStates[e] : EValidationState.Passed;

        //EValidationMessageStatus cumulativeMessageStatus = EValidationMessageStatus.Passed;
        //if (lvr.LoreEntityValidationMessages.ContainsKey(e) && lvr.LoreEntityValidationMessages[e].Count > 0)
        //  cumulativeMessageStatus = lvr.LoreEntityValidationMessages[e].Select(m => m.Status).Distinct().Max();

        if (vs == EValidationState.Failed || vs == EValidationState.ChildFailed)
          image = "avares://LoreViewer/Resources/close.png";
        else if (vs == EValidationState.Warning)
          image = "avares://LoreViewer/Resources/warning.png";
        //else if (vs == EValidationState.ChildWarning)
        //{
        //  if (cumulativeMessageStatus == EValidationMessageStatus.Failed)
        //    image = "avares://LoreViewer/Resources/failedChildWarning.png";
        //  else if (cumulativeMessageStatus == EValidationMessageStatus.Passed)
        //    image = "avares://LoreViewer/Resources/childWarning.png";
        //}
        else if (vs == EValidationState.Passed)
          image = "avares://LoreViewer/Resources/valid.png";
        else
          return null;
        return new Bitmap(AssetLoader.Open(new Uri(image)));
      }
      return null;
    }

    public object? ConvertBack(object? values, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }
  public class LoreEntityToValidationMessageListConverter : IMultiValueConverter
  {
    public object? Convert(IList<object>? values, Type targetType, object? parameter, CultureInfo culture)
    {
      if (values[0] is LoreEntity e && values[1] is LoreValidationResult lvr)
        return lvr.LoreEntityValidationMessages.ContainsKey(e) ? new ObservableCollection<LoreValidationMessage>(lvr.LoreEntityValidationMessages[e]) : null;
      return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }
  }

  public class HasErrorsToHiddenGridLengthConverter : IMultiValueConverter
  {
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
      if (values[0] is bool bHasErrors && values[1] is bool bIsShown && values[2] is GridLength g && bool.TryParse(parameter as string, out var bHideOnCollapse))
      {
        if(bHasErrors && bIsShown)
          return g;
        else if (bHasErrors && !bIsShown)
        {
          if (bHideOnCollapse)
            return new GridLength(0);
          else 
            return new GridLength(1, GridUnitType.Auto);
        }
        else if (!bHasErrors)
          return new GridLength(0);
      }
      return new GridLength(1, GridUnitType.Star);
    }
  }
}
