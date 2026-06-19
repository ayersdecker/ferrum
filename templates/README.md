# Ferrum .NET MAUI Project Template

This directory contains a `dotnet new` template for creating .NET MAUI apps preconfigured with the Ferrum framework for native C/C++ interop.

## Installing the Template

From the repository root:

```bash
dotnet new install templates/maui-ferrum
```

Or install from a NuGet package (when published):

```bash
dotnet new install Ferrum.Templates
```

## Using the Template

Create a new Ferrum-enabled MAUI app:

```bash
dotnet new maui-ferrum -n MyApp
cd MyApp
```

Optional parameters:

```bash
dotnet new maui-ferrum -n MyApp \
  --ApplicationId com.mycompany.myapp \
  --FerrumVersion 0.1.0-alpha
```

## What's Included

The template creates a .NET MAUI project with:

- ✅ Ferrum.Framework NuGet package reference
- ✅ NativeAOT enabled for iOS
- ✅ `AllowUnsafeBlocks` configured
- ✅ Example `NativeBuffer<T>` usage
- ✅ Placeholder for native library references
- ✅ README with next steps

## Uninstalling the Template

```bash
dotnet new uninstall Ferrum.Templates
```

or if installed locally:

```bash
dotnet new uninstall templates/maui-ferrum
```

## Template Development

To test changes to the template:

```bash
# Uninstall old version
dotnet new uninstall templates/maui-ferrum

# Install updated version
dotnet new install templates/maui-ferrum

# Create test project
dotnet new maui-ferrum -n TestApp -o ../test-output
```

## Publishing the Template

To package and publish to NuGet:

```bash
# Create NuGet package
dotnet pack templates/Ferrum.Templates.csproj

# Publish to NuGet.org
dotnet nuget push bin/Release/Ferrum.Templates.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Files in This Template

- `FerrumApp.csproj` - Project file with Ferrum.Framework reference
- `App.xaml[.cs]` - Application entry point
- `AppShell.xaml[.cs]` - Shell navigation structure
- `MainPage.xaml[.cs]` - Example page with NativeBuffer demo
- `MauiProgram.cs` - MAUI configuration
- `Platforms/` - Platform-specific initialization
- `.template.config/template.json` - Template metadata
- `README.md` - Getting started guide for template users

## Learn More

- [Ferrum Framework](https://github.com/ayersdecker/ferrum)
- [.NET MAUI Templates](https://learn.microsoft.com/en-us/dotnet/core/tools/custom-templates)
