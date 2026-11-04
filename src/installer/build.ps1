# Builds SimpleMarkdown.msi (per-user, no admin needed). Requires: dotnet tool install --global wix
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$publish = Join-Path $root 'obj\publish'
$version = ([xml](Get-Content "$root\..\app\SimpleMarkdown.csproj")).Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1

if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
dotnet publish "$root\..\app\SimpleMarkdown.csproj" -c Release -r win-x64 --self-contained true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }

$wix = Join-Path $env:USERPROFILE '.dotnet\tools\wix.exe'
if (-not (Test-Path $wix)) { $wix = 'wix' }
New-Item -ItemType Directory -Force "$root\out" | Out-Null
& $wix build "$root\Package.wxs" -arch x64 -d "PublishDir=$publish" -d "AppIcon=$root\..\app\app.ico" -d "Version=$version" -o "$root\out\SimpleMarkdown.msi"
if ($LASTEXITCODE -ne 0) { throw 'MSI build failed' }
Write-Host "Built $root\out\SimpleMarkdown.msi"
