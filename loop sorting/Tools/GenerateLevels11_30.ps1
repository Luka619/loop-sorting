param(
    [string]$ProjectRoot = (Resolve-Path ".").Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Globalization | Out-Null

$levelsDir = Join-Path $ProjectRoot "Assets/Levels"
if (!(Test-Path $levelsDir)) {
    throw "Assets/Levels not found at: $levelsDir"
}

$levelLayoutScriptGuid = "65e1c79ae65a8454faa53ad7a19d56a1"
$beltCapacity = 50
$beltSlotSpacing = 0
$smoothCorners = 1
$cornerSmoothTension = 1
$cornerSubdivisions = 10
$blockSize = 0.3
$beltWidth = 20

function F([double]$v) {
    return $v.ToString("0.###", [System.Globalization.CultureInfo]::InvariantCulture)
}

function Vec2Line([double]$x, [double]$y, [int]$indent = 4) {
    $pad = " " * $indent
    return ("{0}- {{x: {1}, y: {2}}}" -f $pad, (F $x), (F $y))
}

function OpeningInt([string]$opening) {
    switch ($opening) {
        "Left" { return 0 }
        "Right" { return 1 }
        "Top" { return 2 }
        "Bottom" { return 3 }
        default { throw "Unknown opening: $opening" }
    }
}

function CenterFromMouth([double]$mouthX, [double]$mouthY, [string]$opening, [int]$cols, [int]$rows) {
    $sx = $cols * $blockSize
    $sy = $rows * $blockSize
    $hx = $sx / 2.0
    $hy = $sy / 2.0

    switch ($opening) {
        "Left"   { return @{ x = $mouthX + $hx; y = $mouthY } }
        "Right"  { return @{ x = $mouthX - $hx; y = $mouthY } }
        "Top"    { return @{ x = $mouthX; y = $mouthY - $hy } }
        "Bottom" { return @{ x = $mouthX; y = $mouthY + $hy } }
        default  { throw "Unknown opening: $opening" }
    }
}

function BuildColorCounts([int[]]$colors, [int]$boxIndex, [int]$mode) {
    $c = $colors.Count
    if ($mode -eq 2) {
        $outer = $colors[$boxIndex % $c]
        $inner = $colors[($boxIndex + 1) % $c]
        return @(
            @{ color = $outer; count = 15; hidden = 0 },
            @{ color = $inner; count = 15; hidden = 1 }
        )
    }
    if ($mode -eq 3) {
        $a = $colors[$boxIndex % $c]
        $b = $colors[($boxIndex + 1) % $c]
        $d = $colors[($boxIndex + 2) % $c]
        return @(
            @{ color = $a; count = 10; hidden = 0 },
            @{ color = $b; count = 10; hidden = 1 },
            @{ color = $d; count = 10; hidden = 1 }
        )
    }
    throw "Unknown mode: $mode"
}

function NewLevelYaml(
    [int]$levelNumber,
    [double[][]]$points,
    [bool]$loop,
    [bool]$smooth,
    [double]$tension,
    [int]$subdivisions,
    [hashtable[]]$boxes
) {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("%YAML 1.1")
    $lines.Add("%TAG !u! tag:unity3d.com,2011:")
    $lines.Add("--- !u!114 &11400000")
    $lines.Add("MonoBehaviour:")
    $lines.Add("  m_ObjectHideFlags: 0")
    $lines.Add("  m_CorrespondingSourceObject: {fileID: 0}")
    $lines.Add("  m_PrefabInstance: {fileID: 0}")
    $lines.Add("  m_PrefabAsset: {fileID: 0}")
    $lines.Add("  m_GameObject: {fileID: 0}")
    $lines.Add("  m_Enabled: 1")
    $lines.Add("  m_EditorHideFlags: 0")
    $lines.Add("  m_Script: {fileID: 11500000, guid: $levelLayoutScriptGuid, type: 3}")
    $lines.Add("  m_Name: $levelNumber")
    $lines.Add("  m_EditorClassIdentifier: ")
    $lines.Add("  beltCapacity: $beltCapacity")
    $lines.Add("  beltSlotSpacing: $beltSlotSpacing")
    $lines.Add("  smoothCorners: " + ($(if ($smooth) { "1" } else { "0" })))
    $lines.Add("  cornerSmoothTension: " + (F $tension))
    $lines.Add("  cornerSubdivisions: $subdivisions")
    $lines.Add("  blockSize: " + (F $blockSize))
    $lines.Add("  conveyors:")
    $lines.Add("  - name: Conveyor")
    $lines.Add("    points:")
    foreach ($p in $points) {
        $lines.Add((Vec2Line $p[0] $p[1] 4))
    }
    $lines.Add("    loop: " + ($(if ($loop) { "1" } else { "0" })))
    $lines.Add("    width: $beltWidth")
    $lines.Add("  boxes:")

    foreach ($b in $boxes) {
        $lines.Add("  - name: $($b.name)")
        $lines.Add(("    position: {{x: {0}, y: {1}}}" -f (F $b.position.x), (F $b.position.y)))
        $lines.Add(("    size: {{x: {0}, y: {1}}}" -f (F $b.size.x), (F $b.size.y)))
        $lines.Add("    color: {r: 0, g: 0, b: 0, a: 0}")
        $lines.Add("    columns: $($b.columns)")
        $lines.Add("    rows: $($b.rows)")
        $lines.Add("    opening: $($b.opening)")
        $lines.Add("    autoAlignSlot: 1")
        if ($b.colorCounts -ne $null -and $b.colorCounts.Count -gt 0) {
            $lines.Add("    colorCounts:")
            foreach ($cc in $b.colorCounts) {
                $lines.Add("    - color: $($cc.color)")
                $lines.Add("      count: $($cc.count)")
                $lines.Add("      hidden: $($cc.hidden)")
            }
        }
        else {
            $lines.Add("    colorCounts: []")
        }
        $lines.Add("    initialBlocks: ")
        $lines.Add("    beltSlotIndex: 0")
        $lines.Add("    locked: " + ($(if ($b.locked) { "1" } else { "0" })))
        $lines.Add("    unlockColor: $($b.unlockColor)")
    }

    return ($lines -join "`r`n") + "`r`n"
}

function WriteUtf8NoBom([string]$path, [string]$content) {
    $enc = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($path, $content, $enc)
}

$templates = @{}

function AddTemplate([string]$key, [double[][]]$points, [bool]$loop, [hashtable[]]$anchors) {
    $templates[$key] = @{
        points = $points
        loop = $loop
        anchors = $anchors
    }
}

# Each anchor is a mouth point on the belt plus a box shape and opening direction.
AddTemplate "RectSmall" @(
    @(-6, -3), @(6, -3), @(6, 3), @(-6, 3), @(-6, -3)
) $true @(
    @{ mouth = @(-3, -3); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(0, -3); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(3, -3); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-3, 3); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(0, 3); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(3, 3); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(-6, -1); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(-6, 1); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(6, -1); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(6, 1); opening = "Right"; cols = 10; rows = 3 }
)

AddTemplate "RectTall" @(
    @(-5, -5), @(5, -5), @(5, 5), @(-5, 5), @(-5, -5)
) $true @(
    @{ mouth = @(-2.5, -5); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(2.5, -5); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-2.5, 5); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(2.5, 5); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(-5, -2); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(-5, 2); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(5, -2); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(5, 2); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(0, -5); opening = "Bottom"; cols = 6; rows = 5 },
    @{ mouth = @(0, 5); opening = "Top"; cols = 5; rows = 6 }
)

AddTemplate "Diamond" @(
    @(0, 6), @(6, 0), @(0, -6), @(-6, 0), @(0, 6)
) $true @(
    @{ mouth = @(0, 6); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(0, -6); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-6, 0); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(6, 0); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(-3, 3); opening = "Left"; cols = 6; rows = 5 },
    @{ mouth = @(3, 3); opening = "Right"; cols = 6; rows = 5 },
    @{ mouth = @(-3, -3); opening = "Left"; cols = 6; rows = 5 },
    @{ mouth = @(3, -3); opening = "Right"; cols = 6; rows = 5 }
)

AddTemplate "Hex" @(
    @(-4, 5), @(4, 5), @(6, 0), @(4, -5), @(-4, -5), @(-6, 0), @(-4, 5)
) $true @(
    @{ mouth = @(0, 5); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(0, -5); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-6, 0); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(6, 0); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(-4, 5); opening = "Top"; cols = 5; rows = 6 },
    @{ mouth = @(4, 5); opening = "Top"; cols = 5; rows = 6 },
    @{ mouth = @(-4, -5); opening = "Bottom"; cols = 5; rows = 6 },
    @{ mouth = @(4, -5); opening = "Bottom"; cols = 5; rows = 6 }
)

AddTemplate "UOpen" @(
    @(-6, 4), @(6, 4), @(6, -4), @(-6, -4)
) $false @(
    @{ mouth = @(-3, 4); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(0, 4); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(3, 4); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(-3, -4); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(0, -4); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(3, -4); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(6, 0); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(-6, 0); opening = "Left"; cols = 10; rows = 3 }
)

AddTemplate "SOpen" @(
    @(-6, 4), @(6, 4), @(6, 1), @(-6, 1), @(-6, -2), @(6, -2), @(6, -5)
) $false @(
    @{ mouth = @(-3, 4); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(3, 4); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(-3, 1); opening = "Bottom"; cols = 6; rows = 5 },
    @{ mouth = @(3, 1); opening = "Bottom"; cols = 6; rows = 5 },
    @{ mouth = @(-3, -2); opening = "Top"; cols = 6; rows = 5 },
    @{ mouth = @(3, -2); opening = "Top"; cols = 6; rows = 5 },
    @{ mouth = @(6, -3.5); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(-6, -0.5); opening = "Left"; cols = 10; rows = 3 }
)

AddTemplate "SpiralOpen" @(
    @(-5, 5), @(5, 5), @(5, -5), @(-2, -5), @(-2, 2), @(2, 2), @(2, -2), @(-5, -2)
) $false @(
    @{ mouth = @(0, 5); opening = "Top"; cols = 3; rows = 10 },
    @{ mouth = @(5, 0); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(0, -5); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-2, -5); opening = "Bottom"; cols = 5; rows = 6 },
    @{ mouth = @(-2, 2); opening = "Left"; cols = 6; rows = 5 },
    @{ mouth = @(2, 2); opening = "Right"; cols = 6; rows = 5 },
    @{ mouth = @(2, -2); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(-5, -2); opening = "Left"; cols = 10; rows = 3 }
)

AddTemplate "NotchedLoop" @(
    @(-6, -4), @(6, -4), @(6, 0), @(2, 0), @(2, 4), @(-6, 4), @(-6, -4)
) $true @(
    @{ mouth = @(-2, -4); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(2, -4); opening = "Bottom"; cols = 3; rows = 10 },
    @{ mouth = @(-6, -1); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(-6, 2); opening = "Left"; cols = 10; rows = 3 },
    @{ mouth = @(6, -2); opening = "Right"; cols = 10; rows = 3 },
    @{ mouth = @(2, 4); opening = "Top"; cols = 5; rows = 6 },
    @{ mouth = @(-2, 4); opening = "Top"; cols = 5; rows = 6 },
    @{ mouth = @(2, 0); opening = "Right"; cols = 6; rows = 5 },
    @{ mouth = @(6, 0); opening = "Right"; cols = 10; rows = 3 }
)

$levels = @(
    @{ n = 11; template = "RectSmall"; colors = @(0,1,2); empties = 1; mode = 2; locks = @() },
    @{ n = 12; template = "SOpen"; colors = @(0,1,2); empties = 1; mode = 2; locks = @() },
    @{ n = 13; template = "Diamond"; colors = @(0,1,2,3); empties = 1; mode = 2; locks = @() },
    @{ n = 14; template = "UOpen"; colors = @(0,1,2,4); empties = 1; mode = 2; locks = @() },
    @{ n = 15; template = "Hex"; colors = @(0,1,2,5); empties = 2; mode = 2; locks = @() },
    @{ n = 16; template = "NotchedLoop"; colors = @(0,1,2,3,4); empties = 2; mode = 3; locks = @() },
    @{ n = 17; template = "SpiralOpen"; colors = @(0,1,2,3,4); empties = 2; mode = 3; locks = @() },
    @{ n = 18; template = "RectTall"; colors = @(0,1,2,3,5); empties = 2; mode = 3; locks = @() },
    @{ n = 19; template = "Diamond"; colors = @(0,1,2,4,5); empties = 2; mode = 3; locks = @() },

    @{ n = 20; template = "RectSmall"; colors = @(0,1,2,3); empties = 2; mode = 2; locks = @(@{ idx = 1; unlock = 0 }) },
    @{ n = 21; template = "UOpen"; colors = @(0,1,4,5); empties = 2; mode = 2; locks = @(@{ idx = 1; unlock = 0 }) },
    @{ n = 22; template = "Hex"; colors = @(0,1,2,3,4); empties = 2; mode = 3; locks = @(@{ idx = 1; unlock = 0 }) },
    @{ n = 23; template = "SOpen"; colors = @(0,1,2,3,5); empties = 2; mode = 3; locks = @(@{ idx = 2; unlock = 0 }) },
    @{ n = 24; template = "NotchedLoop"; colors = @(0,1,2,3,4); empties = 2; mode = 3; locks = @(@{ idx = 1; unlock = 0 }, @{ idx = 2; unlock = 0 }) },
    @{ n = 25; template = "SpiralOpen"; colors = @(0,1,2,4,5); empties = 2; mode = 3; locks = @(@{ idx = 3; unlock = 2 }, @{ idx = 4; unlock = 2 }) },
    @{ n = 26; template = "RectTall"; colors = @(0,1,2,3,4,5); empties = 2; mode = 3; locks = @(@{ idx = 1; unlock = 0 }, @{ idx = 2; unlock = 0 }) },
    @{ n = 27; template = "Diamond"; colors = @(0,1,2,3,4,5); empties = 2; mode = 3; locks = @(@{ idx = 3; unlock = 2 }, @{ idx = 4; unlock = 2 }) },
    @{ n = 28; template = "Hex"; colors = @(0,1,2,3,4,5); empties = 2; mode = 3; locks = @(@{ idx = 1; unlock = 0 }, @{ idx = 2; unlock = 0 }, @{ idx = 3; unlock = 0 }) },
    @{ n = 29; template = "SOpen"; colors = @(0,1,2,3,4,5); empties = 2; mode = 3; locks = @(@{ idx = 3; unlock = 2 }, @{ idx = 4; unlock = 2 }, @{ idx = 5; unlock = 2 }) },
    @{ n = 30; template = "NotchedLoop"; colors = @(0,1,2,3,4,5); empties = 3; mode = 3; locks = @(@{ idx = 1; unlock = 0 }, @{ idx = 2; unlock = 0 }, @{ idx = 3; unlock = 0 }) }
)

foreach ($lvl in $levels) {
    $tpl = $templates[$lvl.template]
    if ($null -eq $tpl) { throw "Missing template: $($lvl.template)" }

    $colors = [int[]]$lvl.colors
    $nonEmptyCount = $colors.Count
    $emptyCount = [int]$lvl.empties
    $totalBoxes = $nonEmptyCount + $emptyCount
    $anchors = $tpl.anchors
    if ($anchors.Count -lt $totalBoxes) {
        throw "Template $($lvl.template) has only $($anchors.Count) anchors, needs $totalBoxes."
    }

    $lockMap = @{}
    foreach ($l in $lvl.locks) {
        $lockMap[[int]$l.idx] = [int]$l.unlock
    }

    $boxes = @()
    for ($i = 0; $i -lt $totalBoxes; $i++) {
        $a = $anchors[$i]
        $opening = [string]$a.opening
        $cols = [int]$a.cols
        $rows = [int]$a.rows

        $mouthX = [double]$a.mouth[0]
        $mouthY = [double]$a.mouth[1]
        $center = CenterFromMouth $mouthX $mouthY $opening $cols $rows
        $sx = $cols * $blockSize
        $sy = $rows * $blockSize

        $cc = @()
        if ($i -lt $nonEmptyCount) {
            $cc = BuildColorCounts $colors $i ([int]$lvl.mode)
        }

        $locked = $false
        $unlockColor = 0
        if ($lockMap.ContainsKey($i)) {
            $locked = $true
            $unlockColor = $lockMap[$i]
        }

        $boxes += @{
            name = ("Box {0}" -f ($i + 1))
            position = @{ x = $center.x; y = $center.y }
            size = @{ x = $sx; y = $sy }
            columns = $cols
            rows = $rows
            opening = (OpeningInt $opening)
            colorCounts = $cc
            locked = $locked
            unlockColor = $unlockColor
        }
    }

    $yaml = NewLevelYaml `
        -levelNumber ([int]$lvl.n) `
        -points ([double[][]]$tpl.points) `
        -loop ([bool]$tpl.loop) `
        -smooth $true `
        -tension $cornerSmoothTension `
        -subdivisions $cornerSubdivisions `
        -boxes $boxes

    $outPath = Join-Path $levelsDir ("{0}.asset" -f $lvl.n)
    WriteUtf8NoBom $outPath $yaml
    Write-Host ("Wrote {0}" -f $outPath)
}

Write-Host "Done."
