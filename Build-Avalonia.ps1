# Build-Avalonia.ps1
# Builds and packs Avalonia, TreeDataGrid, and DataGrid into local NuGet folder

$ErrorActionPreference = "Stop"

# Paths (relative to LoreViewer)
$root = ".."
$avaloniaRepo = "$root/Avalonia"
$treeGridRepo = "$root/Avalonia.Controls.TreeDataGrid"
$dataGridRepo = "$root/Avalonia.Controls.DataGrid"
$outputDir = "NuGet/Avalonia"

$projects = @(
    #@{ Name = "Avalonia.Build.Tasks"; Path = "$avaloniaRepo/src/Avalonia.Build.Tasks/Avalonia.Build.Tasks.csproj" },
    #@{ Name = "Avalonia.Base"; Path = "$avaloniaRepo/src/Avalonia.Base/Avalonia.Base.csproj" },
    #@{ Name = "Avalonia.Markup"; Path = "$avaloniaRepo/src/Markup/Avalonia.Markup/Avalonia.Markup.csproj" },
    #@{ Name = "Avalonia.Markup.Xaml"; Path = "$avaloniaRepo/src/Markup/Avalonia.Markup.Xaml/Avalonia.Markup.Xaml.csproj" },
    #@{ Name = "Avalonia.Controls.ColorPicker"; Path = "$avaloniaRepo/src/Avalonia.Controls.ColorPicker/Avalonia.Controls.ColorPicker.csproj" },
    #@{ Name = "Avalonia"; Path = "$avaloniaRepo/packages/Avalonia/Avalonia.csproj" },
    #@{ Name = "Avalonia.Native"; Path = "$avaloniaRepo/src/Avalonia.Native/Avalonia.Native.csproj" },
    #@{ Name = "Avalonia.Vulkan"; Path = "$avaloniaRepo/src/Avalonia.Vulkan/Avalonia.Vulkan.csproj" },
    #@{ Name = "Avalonia.Diagnostics"; Path = "$avaloniaRepo/src/Avalonia.Diagnostics/Avalonia.Diagnostics.csproj" },
    #@{ Name = "Avalonia.FreeDesktop"; Path = "$avaloniaRepo/src/Avalonia.FreeDesktop/Avalonia.FreeDesktop.csproj" },
    #@{ Name = "Avalonia.Dialogs"; Path = "$avaloniaRepo/src/Avalonia.Dialogs/Avalonia.Dialogs.csproj" },
    #@{ Name = "Avalonia.Skia"; Path = "$avaloniaRepo/src/Skia/Avalonia.Skia/Avalonia.Skia.csproj" },
    #@{ Name = "Avalonia.Win32"; Path = "$avaloniaRepo/src/Windows/Avalonia.Win32/Avalonia.Win32.csproj" },
    #@{ Name = "Avalonia.X11"; Path = "$avaloniaRepo/src/Avalonia.X11/Avalonia.X11.csproj" }
    #@{ Name = "Avalonia.DesignerSupport"; Path = "$avaloniaRepo/src/Avalonia.DesignerSupport/Avalonia.DesignerSupport.csproj" },
    #@{ Name = "Avalonia.Remote.Protocol"; Path = "$avaloniaRepo/src/Avalonia.Remote.Protocol/Avalonia.Remote.Protocol.csproj" },
    #@{ Name = "Avalonia.OpenGL"; Path = "$avaloniaRepo/src/Avalonia.OpenGL/Avalonia.OpenGL.csproj" },
    #@{ Name = "Avalonia.Controls"; Path = "$avaloniaRepo/src/Avalonia.Controls/Avalonia.Controls.csproj" },
    #@{ Name = "Avalonia.Desktop"; Path = "$avaloniaRepo/src/Avalonia.Desktop/Avalonia.Desktop.csproj" },
    #@{ Name = "Avalonia.Themes.Fluent"; Path = "$avaloniaRepo/src/Avalonia.Themes.Fluent/Avalonia.Themes.Fluent.csproj" },
    #@{ Name = "Avalonia.Themes.Simple"; Path = "$avaloniaRepo/src/Avalonia.Themes.Simple/Avalonia.Themes.Simple.csproj" },
    #@{ Name = "Avalonia.ReactiveUI"; Path = "$avaloniaRepo/src/Avalonia.ReactiveUI/Avalonia.ReactiveUI.csproj" },
    #@{ Name = "Avalonia.Metal"; Path = "$avaloniaRepo/src/Avalonia.Metal/Avalonia.Metal.csproj" },
    #@{ Name = "Avalonia.MicroCom"; Path = "$avaloniaRepo/src/Avalonia.MicroCom/Avalonia.MicroCom.csproj" },
    @{ Name = "TreeDataGrid"; Path = "$treeGridRepo/src/Avalonia.Controls.TreeDataGrid/Avalonia.Controls.TreeDataGrid.csproj" }
    #@{ Name = "DataGrid"; Path = "$dataGridRepo/src/Avalonia.Controls.DataGrid/Avalonia.Controls.DataGrid.csproj" }

)


# Ensure output exists
$outputFullPath = Resolve-Path $outputDir -ErrorAction SilentlyContinue
if (-not $outputFullPath) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

Write-Host "📦 Building and packing Avalonia components..." -ForegroundColor Cyan

foreach ($proj in $projects) {
    $csproj = $proj.Path
    $name = $proj.Name

    if (-not (Test-Path $csproj)) {
        Write-Warning "⚠️  Skipping $name — not found at $csproj"
        continue
    }

    Write-Host "🔧 Packing $name..."
    dotnet pack $csproj -c Release -o "$PSScriptRoot/$outputDir" --nologo
    

}

Write-Host "`n✅ Packages placed in: $outputDir" -ForegroundColor Green
