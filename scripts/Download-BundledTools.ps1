#Requires -Version 5.1
<#
  Downloads FFmpeg, Pandoc, ImageMagick (portable), 7-Zip CLI, and optionally LibreOffice MSI
  into src/Cofrox.App/Tools so the app runs offline. Tools/ is gitignored.

  Ghostscript: official Windows installers are not simple archives; install GPL Ghostscript once
  (winget install Ghostscript.Ghostscript) or the app will use Program Files\gs\*\bin\gswin64c.exe.

  After this script + a Release publish with the Portable profile, the output folder is self-contained
  (.NET 10 runtime included) with bundled engines — copy the whole publish folder to another PC.

  Usage:
    .\scripts\Download-BundledTools.ps1
    .\scripts\Download-BundledTools.ps1 -IncludeLibreOffice
    .\scripts\Download-BundledTools.ps1 -ToolsRoot D:\path\to\Tools
#>
param(
    [string] $ToolsRoot = "",
    [switch] $IncludeLibreOffice,
    [string] $LibreOfficeVersion = "25.8.5"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

$repoRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($ToolsRoot)) {
    $ToolsRoot = Join-Path $repoRoot "src\Cofrox.App\Tools"
}

$ghHeaders = @{ "User-Agent" = "Cofrox-Download-BundledTools/1.0" }

function Get-Bootstrap7za {
    param([string] $WorkDir)
    $sevenZr = Join-Path $WorkDir "7zr.exe"
    $extra = Join-Path $WorkDir "7z2500-extra.7z"
    Invoke-WebRequest -Uri "https://www.7-zip.org/a/7zr.exe" -OutFile $sevenZr -UseBasicParsing
    Invoke-WebRequest -Uri "https://www.7-zip.org/a/7z2500-extra.7z" -OutFile $extra -UseBasicParsing
    $extraOut = Join-Path $WorkDir "extra"
    & $sevenZr x $extra "-o$extraOut" -y | Out-Null
    $sevenZa = Join-Path $extraOut "x64\7za.exe"
    if (-not (Test-Path $sevenZa)) {
        throw "7za.exe not found after extracting 7z2500-extra.7z"
    }
    return $sevenZa
}

function Expand-7zArchive {
    param(
        [string] $SevenZa,
        [string] $Archive,
        [string] $Destination
    )
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    & $SevenZa x $Archive "-o$Destination" -y
    if ($LASTEXITCODE -ne 0) {
        throw "7za failed on $Archive (exit $LASTEXITCODE)"
    }
}

if (-not (Test-Path $ToolsRoot)) {
    New-Item -ItemType Directory -Force -Path $ToolsRoot | Out-Null
}

$tempDir = Join-Path $env:TEMP ("cofrox-tools-" + [guid]::NewGuid().ToString("n"))
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

try {
    Write-Host "Bootstrapping 7-Zip CLI..."
    $sevenZa = Get-Bootstrap7za -WorkDir $tempDir

    Write-Host "Downloading FFmpeg (BtbN win64 GPL)..."
    $ffmpegZip = Join-Path $tempDir "ffmpeg.zip"
    Invoke-WebRequest -Uri "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip" `
        -OutFile $ffmpegZip -UseBasicParsing
    $ffStage = Join-Path $tempDir "ffmpeg-stage"
    Expand-Archive -Path $ffmpegZip -DestinationPath $ffStage -Force
    $ffRoot = Get-ChildItem $ffStage -Directory | Select-Object -First 1
    if (-not $ffRoot) { throw "FFmpeg zip layout unexpected." }
    $destFf = Join-Path $ToolsRoot "ffmpeg"
    Remove-Item $destFf -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $destFf | Out-Null
    Copy-Item -Path (Join-Path $ffRoot.FullName "bin\*") -Destination $destFf -Recurse -Force

    Write-Host "Downloading Pandoc (latest release)..."
    $pandocRel = Invoke-RestMethod -Uri "https://api.github.com/repos/jgm/pandoc/releases/latest" -Headers $ghHeaders
    $pandocAsset = $pandocRel.assets | Where-Object { $_.name -match "windows-x86_64\.zip$" } | Select-Object -First 1
    if (-not $pandocAsset) { throw "Pandoc windows-x86_64.zip not found in latest release." }
    $pandocZip = Join-Path $tempDir "pandoc.zip"
    Invoke-WebRequest -Uri $pandocAsset.browser_download_url -OutFile $pandocZip -UseBasicParsing
    $pStage = Join-Path $tempDir "pandoc-stage"
    Expand-Archive -Path $pandocZip -DestinationPath $pStage -Force
    $destP = Join-Path $ToolsRoot "pandoc"
    Remove-Item $destP -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $destP | Out-Null
    Copy-Item -Path (Join-Path $pStage "*") -Destination $destP -Recurse -Force

    Write-Host "Downloading ImageMagick portable (latest Q16 x64)..."
    $imRel = Invoke-RestMethod -Uri "https://api.github.com/repos/ImageMagick/ImageMagick/releases/latest" -Headers $ghHeaders
    $imAsset = $imRel.assets | Where-Object { $_.name -match "portable-Q16-x64\.7z$" } | Select-Object -First 1
    if (-not $imAsset) { throw "ImageMagick portable Q16 x64 .7z not found in latest release." }
    $im7z = Join-Path $tempDir $imAsset.name
    Invoke-WebRequest -Uri $imAsset.browser_download_url -OutFile $im7z -UseBasicParsing
    $imStage = Join-Path $tempDir "im-stage"
    Expand-7zArchive -SevenZa $sevenZa -Archive $im7z -Destination $imStage
    $magick = Get-ChildItem $imStage -Recurse -Filter "magick.exe" | Select-Object -First 1
    if (-not $magick) { throw "magick.exe not found in ImageMagick archive." }
    $imRoot = $magick.Directory.FullName
    $destIm = Join-Path $ToolsRoot "imagemagick"
    Remove-Item $destIm -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $destIm | Out-Null
    Copy-Item -Path (Join-Path $imRoot "*") -Destination $destIm -Recurse -Force

    Write-Host "Installing 7-Zip CLI as Tools\7zip\7z.exe (7za build)..."
    $dest7 = Join-Path $ToolsRoot "7zip"
    Remove-Item $dest7 -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $dest7 | Out-Null
    Copy-Item -Path $sevenZa -Destination (Join-Path $dest7 "7z.exe") -Force

    if ($IncludeLibreOffice) {
        Write-Host "Downloading LibreOffice $LibreOfficeVersion MSI (large)..."
        $msiName = "LibreOffice_{0}_Win_x86-64.msi" -f $LibreOfficeVersion
        $msiUrl = "https://download.documentfoundation.org/libreoffice/stable/{0}/win/x86_64/{1}" -f $LibreOfficeVersion, $msiName
        $msiPath = Join-Path $tempDir $msiName
        Invoke-WebRequest -Uri $msiUrl -OutFile $msiPath -UseBasicParsing
        $loStage = Join-Path $tempDir "lo-msi"
        Expand-7zArchive -SevenZa $sevenZa -Archive $msiPath -Destination $loStage
        $soffice = Get-ChildItem $loStage -Recurse -Filter "soffice.exe" | Select-Object -First 1
        if (-not $soffice) { throw "soffice.exe not found after extracting LibreOffice MSI." }
        $programDir = $soffice.Directory.FullName
        if ($programDir -notmatch '[\\/]program$') {
            throw "Unexpected LibreOffice layout (expected ...\program\soffice.exe): $programDir"
        }
        $libreOfficeRoot = Split-Path $programDir -Parent
        $destLo = Join-Path $ToolsRoot "libreoffice"
        Remove-Item $destLo -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $destLo | Out-Null
        Copy-Item -Path (Join-Path $libreOfficeRoot "*") -Destination $destLo -Recurse -Force
        if (-not (Test-Path (Join-Path $destLo "program\soffice.exe"))) {
            throw "LibreOffice copy failed: missing libreoffice\program\soffice.exe"
        }
    }

    Write-Host ""
    Write-Host "Done. Tools installed under: $ToolsRoot"
    Write-Host "Ghostscript: install separately (winget install Ghostscript.Ghostscript) or rely on Program Files detection."
    Write-Host "Publish portable (includes .NET 10 runtime):"
    Write-Host "  dotnet publish `"$repoRoot\src\Cofrox.App\Cofrox.App.csproj`" -c Release -p:PublishProfile=Portable"
}
finally {
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}
