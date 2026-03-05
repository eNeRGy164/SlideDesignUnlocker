# Copilot Instructions

## Project Guidelines
- Prefers `var x = new Type()` over target-typed `new()` syntax (Type x = new())

## Architecture
- WinUI 3 desktop app (.NET 10, Windows App SDK 1.8)
- Single-project MSIX packaging (`EnablePreviewMsixTooling`)
- MVVM via CommunityToolkit.Mvvm; PowerPoint files via DocumentFormat.OpenXml

## Build & packaging
- Release builds are self-contained (no external .NET runtime dependency) per Store policy 10.2.4.1
- `PublishTrimmed` with `TrimMode=full` and `InvariantGlobalization` — only effective during `dotnet publish`
- Windows App SDK is NOT self-contained; it's delivered as a Store framework package
- Store packages are created via: `dotnet msbuild -t:CreateStorePackages`
- Custom MSBuild targets live in `StorePackaging.targets` (imported by the csproj)
- Native AOT builds use `scripts/publish-native-aot.ps1`

## Trim safety
- Do NOT use `WeakReferenceMessenger.Default.RegisterAll(this)` — it is not trim-safe
- Use explicit `WeakReferenceMessenger.Default.Register<TMessage>(this)` instead
