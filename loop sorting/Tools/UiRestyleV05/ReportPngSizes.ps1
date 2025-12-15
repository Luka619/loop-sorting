param(
    [string]$KitRoot = "Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti",
    [switch]$IncludeWorldSprites,
    [string]$OutFile = ""
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

function Get-PngSize([string]$path) {
    $img = [System.Drawing.Image]::FromFile((Resolve-Path $path))
    try {
        return @{ w = $img.Width; h = $img.Height }
    }
    finally {
        $img.Dispose()
    }
}

function Build-List([string]$subDir) {
    $dir = Join-Path $KitRoot $subDir
    if (!(Test-Path $dir)) {
        throw "Directory not found: $dir"
    }

    Get-ChildItem $dir -Filter *.png | Sort-Object Name | ForEach-Object {
        $s = Get-PngSize $_.FullName
        [PSCustomObject]@{
            dir  = $subDir
            name = $_.Name
            w    = $s.w
            h    = $s.h
        }
    }
}

$items = @()
$items += Build-List "UI_Sprites"
if ($IncludeWorldSprites) { $items += Build-List "World_Sprites" }

if ([string]::IsNullOrWhiteSpace($OutFile)) {
    $items | Format-Table -AutoSize
}
else {
    $outDir = Split-Path $OutFile -Parent
    if ($outDir -and !(Test-Path $outDir)) { New-Item -ItemType Directory -Force -Path $outDir | Out-Null }
    $items | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 $OutFile
    Write-Host "Wrote: $OutFile"
}

