# Slide Design Unlocker

A WinUI 3 application that unlocks the design of PowerPoint slides by modifying shape protection settings.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 18.x](https://visualstudio.microsoft.com/) (or later) with:
  - **.NET desktop development** workload
  - **Windows App SDK** component
  - **MSVC C++ build tools** (for symbol package generation)

## Development

### Debug build

Open `SlideDesignUnlocker.sln` in Visual Studio and press F5, or from the command line:

```shell
dotnet build SlideDesignUnlocker/SlideDesignUnlocker.csproj -c Debug
```

### Release build

Release builds are automatically **self-contained** (the .NET runtime is bundled) to comply
with Microsoft Store policy 10.2.4.1 — no external runtime dependency is required on the
user's machine.

```shell
dotnet build SlideDesignUnlocker/SlideDesignUnlocker.csproj -c Release
```

> **Note:** `PublishTrimmed` only takes effect during `dotnet publish`, not during `dotnet build`.
> Use the `CreateStorePackages` target (below) for fully optimised packages.

## Creating Store packages

The project includes a custom MSBuild target that produces Store-ready MSIX packages for all
three architectures (x64, arm64, x86). Each `.msixupload` contains the `.msix` and its
`.appxsym` symbol package.

```shell
dotnet msbuild SlideDesignUnlocker/SlideDesignUnlocker.csproj -t:CreateStorePackages
```

Output is written to `SlideDesignUnlocker/AppPackages/`:

```plain
AppPackages/
  SlideDesignUnlocker_<version>_x64.msixupload      <- upload to Partner Center
  SlideDesignUnlocker_<version>_arm64.msixupload
  SlideDesignUnlocker_<version>_x86.msixupload
  SlideDesignUnlocker_<version>_x64_Test/            <- sideload test packages
  SlideDesignUnlocker_<version>_arm64_Test/
  SlideDesignUnlocker_<version>_x86_Test/
```

### What the target does

| Step                 | Description                                                                                      |
| -------------------- | ------------------------------------------------------------------------------------------------ |
| **Resolve VC tools** | Finds `mspdbcmf.exe` via `vswhere.exe` so `.appxsym` symbol packages can be generated            |
| **Publish**          | Runs `dotnet publish` per RID with self-contained .NET, IL trimming, and invariant globalization |
| **Package**          | Creates `.msixupload` files (zip of `.msix` + `.appxsym`) for Store submission                   |

### Custom output directory

```shell
dotnet msbuild SlideDesignUnlocker/SlideDesignUnlocker.csproj -t:CreateStorePackages -p:StorePackageDir=C:\output\
```

## Native AOT (experimental)

A separate script produces a Native AOT compiled MSIX for a single architecture:

```shell
.\scripts\publish-native-aot.ps1 -RuntimeIdentifier win-x64
```

This includes a post-build validation that rejects AI/ML payloads (`onnxruntime`,
`ortextensions`, `Microsoft.WindowsAppSDK.AI/ML`) from the MSIX.

## Packaging decisions

| Setting                            | Value            | Reason                                                                      |
| ---------------------------------- | ---------------- | --------------------------------------------------------------------------- |
| `SelfContained`                    | `true` (Release) | Bundles .NET runtime — no external dependency (Store policy 10.2.4.1)       |
| `WindowsAppSDKSelfContained`       | `false`          | Windows App SDK is delivered as a Store framework package (~120 MB savings) |
| `PublishTrimmed` / `TrimMode=full` | `true` (publish) | Removes unused code to minimise package size                                |
| `InvariantGlobalization`           | `true` (Release) | Excludes ICU/locale data (~30 MB savings)                                   |

## Project structure

| File                             | Purpose                                                              |
| -------------------------------- | -------------------------------------------------------------------- |
| `SlideDesignUnlocker.csproj`     | Main project file with build configuration and Release optimisations |
| `StorePackaging.targets`         | Custom MSBuild targets for `CreateStorePackages`                     |
| `scripts/publish-native-aot.ps1` | Native AOT publish script with MSIX validation                       |
| `Package.appxmanifest`           | MSIX package identity, capabilities, and visual assets               |
