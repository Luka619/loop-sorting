param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDir,

    [string]$KitRoot = "Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti",

    [string]$ResourcesRoot = "Assets/Resources",

    [switch]$IncludeWorldSprites,

    [switch]$IncludeExtraResources,

    [switch]$Backup,

    [switch]$DryRun,

    [switch]$AllowPartial,

    # Back-compat: when provided, size mismatches will be warned but not blocked.
    # Default behavior is now also to allow mismatches unless -StrictSize is set.
    [switch]$AllowSizeMismatch,

    # Enforce exact pixel-size match (old default behavior).
    [switch]$StrictSize
)

$ErrorActionPreference = "Stop"

function Fail([string]$msg) {
    throw $msg
}

if (!(Test-Path $SourceDir)) {
    Fail "SourceDir not found: $SourceDir"
}
if (!(Test-Path $KitRoot)) {
    Fail "KitRoot not found: $KitRoot"
}
if ($IncludeExtraResources -and !(Test-Path $ResourcesRoot)) {
    Fail "ResourcesRoot not found: $ResourcesRoot"
}

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

function Ensure-Dir([string]$dir) {
    if (!(Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
}

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupRoot = Join-Path "Tools/UiRestyleV05/_backup" $timestamp

if ($AllowSizeMismatch -and $StrictSize) {
    Fail "Invalid args: -AllowSizeMismatch and -StrictSize cannot be used together."
}

function Resolve-SourceFile([string]$subDir, [string]$fileName) {
    $candidate1 = Join-Path (Join-Path $SourceDir $subDir) $fileName
    if (Test-Path $candidate1) { return $candidate1 }

    $candidate2 = Join-Path $SourceDir $fileName
    if (Test-Path $candidate2) { return $candidate2 }

    return $null
}

function Replace-InTargetDir([string]$sourceSubDir, [string]$targetDir) {
    if (!(Test-Path $targetDir)) {
        Fail "Target directory not found: $targetDir"
    }

    $targetFiles = Get-ChildItem $targetDir -Filter *.png | Sort-Object Name
    if ($targetFiles.Count -eq 0) {
        Write-Host "No PNGs found in $targetDir"
        return @{ replaced = 0; skipped = 0 }
    }

    $replaced = 0
    $skipped = 0

    foreach ($t in $targetFiles) {
        $src = Resolve-SourceFile $sourceSubDir $t.Name
        if ($null -eq $src) {
            if ($AllowPartial) {
                $skipped++
                continue
            }
            Fail "Missing source file for $sourceSubDir/$($t.Name) (expected at '$SourceDir/$sourceSubDir/$($t.Name)' or '$SourceDir/$($t.Name)')"
        }

        $exp = Get-PngSize $t.FullName
        $act = Get-PngSize $src
        if ($exp.w -ne $act.w -or $exp.h -ne $act.h) {
            $msg = "Size mismatch for $sourceSubDir/$($t.Name): expected ${($exp.w)}x${($exp.h)}, got ${($act.w)}x${($act.h)}"
            if ($StrictSize) {
                Fail $msg
            }
            Write-Host "[warn] $msg"
        }

        if ($Backup) {
            $backupDir = Join-Path $backupRoot $sourceSubDir
            Ensure-Dir $backupDir
            Copy-Item -Force $t.FullName (Join-Path $backupDir $t.Name)
        }

        if ($DryRun) {
            Write-Host "[DryRun] $sourceSubDir/$($t.Name) <= $src"
        }
        else {
            Copy-Item -Force $src $t.FullName
        }

        $replaced++
    }

    return @{ replaced = $replaced; skipped = $skipped }
}

function Replace-InDir([string]$subDir) {
    $targetDir = Join-Path $KitRoot $subDir
    return Replace-InTargetDir $subDir $targetDir
}

Write-Host "SourceDir: $SourceDir"
Write-Host "KitRoot:   $KitRoot"
if ($IncludeExtraResources) { Write-Host "Resources: $ResourcesRoot" }
Write-Host "DryRun:    $DryRun"
Write-Host "Backup:    $Backup"
Write-Host "Partial:   $AllowPartial"
Write-Host "SizeCheck: $(if ($StrictSize) { 'STRICT' } else { 'ALLOW_MISMATCH' })"
Write-Host "Extras:    $IncludeExtraResources"
if ($Backup -and !$DryRun) { Write-Host "BackupDir: $backupRoot" }

$summary = @()
$summary += [PSCustomObject]@{ dir = "UI_Sprites"; result = Replace-InDir "UI_Sprites" }
if ($IncludeWorldSprites) {
    $summary += [PSCustomObject]@{ dir = "World_Sprites"; result = Replace-InDir "World_Sprites" }
}
if ($IncludeExtraResources) {
    $summary += [PSCustomObject]@{ dir = "BoosterPurchase"; result = Replace-InTargetDir "BoosterPurchase" (Join-Path $ResourcesRoot "BoosterPurchase") }
    $summary += [PSCustomObject]@{ dir = "setting_page_assets"; result = Replace-InTargetDir "setting_page_assets" (Join-Path $ResourcesRoot "setting_page_assets") }
    $summary += [PSCustomObject]@{ dir = "ResourcesRoot"; result = Replace-InTargetDir "ResourcesRoot" $ResourcesRoot }
}

Write-Host ""
Write-Host "Done."
foreach ($row in $summary) {
    Write-Host ("- {0}: replaced={1}, skipped={2}" -f $row.dir, $row.result.replaced, $row.result.skipped)
}
