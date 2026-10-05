param(
    [Parameter(Mandatory)] [string] $Repository,
    [string] $Changelog
)
$ErrorActionPreference = 'Stop'
if ($Repository -notmatch '^[A-Za-z0-9-]+/[A-Za-z0-9_.-]+$') { throw 'Use owner/repository for Repository.' }
. "$PSScriptRoot/scripts/ManifestVersions.ps1"

$project = [xml](Get-Content "$PSScriptRoot/src/FreeGuide.csproj" -Raw)
$releaseVersion = [string]$project.Project.PropertyGroup.Version
if ($releaseVersion -notmatch '^\d+\.\d+\.\d+$') { throw 'The project version must have three parts, such as 1.0.1.' }
$pluginVersion = "$releaseVersion.0"
if ([string]::IsNullOrWhiteSpace($Changelog)) {
    $notesPath = "$PSScriptRoot/release-notes/$releaseVersion.md"
    if (-not (Test-Path $notesPath)) { throw "Add release notes at $notesPath or provide -Changelog." }
    $Changelog = (Get-Content $notesPath -Raw).Trim()
}
$manifestPath = "$PSScriptRoot/manifest.json"
$pluginId = '94079c51-6e85-4483-8b34-85b162638f75'
$existingPlugins = if (Test-Path $manifestPath) { @(Get-Content $manifestPath -Raw | ConvertFrom-Json) } else { @() }
$existingPlugin = $existingPlugins | Where-Object guid -eq $pluginId | Select-Object -First 1

# Published versions keep the checksum of their original installer.
if (@($existingPlugin.versions | Where-Object version -eq $pluginVersion).Count -gt 0) {
    throw "Version $pluginVersion is already published. Increase the project version before packaging a new release."
}

dotnet test "$PSScriptRoot/tests/FreeGuide.Tests.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
New-Item -ItemType Directory -Force "$PSScriptRoot/dist" | Out-Null
$zipPath = "$PSScriptRoot/dist/HDHomeRunFreeGuide_$pluginVersion.zip"
Compress-Archive -Path "$PSScriptRoot/src/bin/Release/net10.0/FreeGuide.dll","$PSScriptRoot/src/bin/Release/net10.0/FreeGuide.deps.json" -DestinationPath $zipPath -Force
$newVersion = @{
    version = $pluginVersion
    targetAbi = '12.1.0.0'
    sourceUrl = "https://github.com/$Repository/releases/download/v$releaseVersion/HDHomeRunFreeGuide_$pluginVersion.zip"
    checksum = (Get-FileHash $zipPath -Algorithm MD5).Hash.ToLowerInvariant()
    timestamp = [DateTime]::UtcNow.ToString('O')
    changelog = $Changelog
}
$plugin = @{
    guid = $pluginId
    name = 'HDHomeRun Free Guide'
    description = 'Automatic HDHomeRun XMLTV guide using tuner authorization. No email required.'
    overview = 'Free two-day guide with fresh DeviceAuth, gzip support, automatic registration and randomized background downloads.'
    owner = $Repository.Split('/')[0]
    category = 'Live TV'
    imageUrl = "https://raw.githubusercontent.com/$Repository/main/assets/logo.png"
    versions = @(Merge-PluginVersions -NewVersion $newVersion -ExistingVersions @($existingPlugin.versions))
}
$manifest = @($plugin) + @($existingPlugins | Where-Object guid -ne $pluginId)
ConvertTo-Json -InputObject $manifest -Depth 8 | Set-Content $manifestPath -Encoding utf8
Write-Output "Package: $zipPath"
Write-Output "Repository URL: https://raw.githubusercontent.com/$Repository/main/manifest.json"
