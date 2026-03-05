param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$RuntimeIdentifier = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectDir = Join-Path $repoRoot "SlideDesignUnlocker"
$projectPath = Join-Path $projectDir "SlideDesignUnlocker.csproj"
$appxPackageDir = (Join-Path $projectDir ("AppPackages\NativeAot-{0}" -f $RuntimeIdentifier)) + "\"

New-Item -ItemType Directory -Force -Path $appxPackageDir | Out-Null

dotnet publish $projectPath `
    -c Release `
    -r $RuntimeIdentifier `
    -p:PublishAot=true `
    -p:GenerateAppxPackageOnBuild=true `
    -p:AppxPackageDir=$appxPackageDir `
    -p:AppxSymbolPackageEnabled=false `
    -p:BuildAppxUploadPackageForUap=false

$msix = Get-ChildItem -Path $appxPackageDir -Recurse -File |
    Where-Object { $_.Extension -eq ".msix" -and $_.Name -like "SlideDesignUnlocker_*.msix" } |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1

if (-not $msix) {
    throw "MSIX was not created under $appxPackageDir"
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($msix.FullName)

try {
    $blockedEntries = $archive.Entries |
        Where-Object {
            $_.FullName -match "(?i)(onnxruntime|ortextensions|Microsoft\.WindowsAppSDK\.(AI|ML))"
        } |
        Select-Object -ExpandProperty FullName -Unique
}
finally {
    $archive.Dispose()
}

if ($blockedEntries) {
    throw "MSIX contains blocked AI/ML payloads:`n$($blockedEntries -join "`n")"
}

Write-Host "Created MSIX: $($msix.FullName)"
Write-Host "Verified that the MSIX does not contain blocked AI/ML payloads."
