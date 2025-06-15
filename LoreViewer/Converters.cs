using Avalonia.Controls;
using Avalonia.Data.Converters;
using System;
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

}
