param([Parameter(Mandatory=$true)][string]$Repository)
$ErrorActionPreference = 'Stop'
if ($Repository -notmatch '^[A-Za-z0-9-]+/[A-Za-z0-9_.-]+$') { throw 'Use owner/repository for Repository.' }
dotnet test "$PSScriptRoot/tests/FreeGuide.Tests.csproj" -c Release
if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
New-Item -ItemType Directory -Force "$PSScriptRoot/dist" | Out-Null
$zipPath = "$PSScriptRoot/dist/HDHomeRunFreeGuide_1.0.0.0.zip"
Compress-Archive -Path "$PSScriptRoot/src/bin/Release/net10.0/FreeGuide.dll","$PSScriptRoot/src/bin/Release/net10.0/FreeGuide.deps.json" -DestinationPath $zipPath -Force
$manifest = @(@{
 guid='94079c51-6e85-4483-8b34-85b162638f75';name='HDHomeRun Free Guide';description='Automatic HDHomeRun XMLTV guide using tuner authorization. No email required.';overview='Free two-day guide with fresh DeviceAuth, gzip support, automatic registration and randomized background downloads.';owner=$Repository.Split('/')[0];category='Live TV';imageUrl="https://raw.githubusercontent.com/$Repository/main/assets/logo.png";versions=@(@{version='1.0.0.0';targetAbi='12.1.0.0';sourceUrl="https://github.com/$Repository/releases/download/v1.0.0/HDHomeRunFreeGuide_1.0.0.0.zip";checksum=(Get-FileHash $zipPath -Algorithm MD5).Hash.ToLowerInvariant();timestamp=[DateTime]::UtcNow.ToString('O');changelog='Initial Jellyfin 12.1 release.'})
})
ConvertTo-Json -InputObject $manifest -Depth 8 | Set-Content "$PSScriptRoot/manifest.json" -Encoding utf8
Write-Output "Package: $zipPath"
Write-Output "Repository URL: https://raw.githubusercontent.com/$Repository/main/manifest.json"
