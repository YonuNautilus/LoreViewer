using Avalonia.Data.Converters;
using DocumentFormat.OpenXml;
using LoreViewer.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
}
