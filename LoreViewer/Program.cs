using Avalonia;
using Avalonia.ReactiveUI;
using System;
using UnitsNet.Units;

namespace LoreViewer
{
  public class Program
  {
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
      BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();

    public static void AddCustomAbbreviations()
    {
      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<AngleUnit>(AngleUnit.Degree, "°");
      
      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<MassUnit>(MassUnit.Ounce, "ounces");
      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<MassUnit>(MassUnit.Ounce, "ounce");
      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<MassUnit>(MassUnit.Tonne, "tonnes");

      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<TemperatureUnit>(TemperatureUnit.DegreeFahrenheit, "°F");
      UnitsNet.UnitAbbreviationsCache.Default.MapUnitToAbbreviation<TemperatureUnit>(TemperatureUnit.DegreeCelsius, "°C");
    }
  }
}
