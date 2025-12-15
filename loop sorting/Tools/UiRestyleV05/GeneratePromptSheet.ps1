param(
    [string]$KitRoot = "Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti",

    [ValidateSet("Hud", "Meta", "All")]
    [string]$Scope = "Hud",

    [string]$ConfigPath = "Assets/Resources/LoopSortingUIKitConfig.json",

    [string]$OutFile = "",

    [switch]$IncludeExtraResources,

    [switch]$IncludeKitWorldAndTextures
)

$ErrorActionPreference = "Stop"

function Fail([string]$msg) { throw $msg }

if (!(Test-Path $KitRoot)) { Fail "KitRoot not found: $KitRoot" }
if (!(Test-Path $ConfigPath)) { Fail "ConfigPath not found: $ConfigPath" }

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

function Ensure-OutDir([string]$path) {
    $dir = Split-Path $path -Parent
    if ($dir -and !(Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
}

function Normalize-Role([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return "" }
    return ($s.Substring(0, 1).ToUpperInvariant() + $s.Substring(1))
}

function Get-ButtonState([string]$fileName) {
    if ($fileName -like "*_pressed.png") { return "Pressed" }
    if ($fileName -like "*_disabled.png") { return "Disabled" }
    return "Normal"
}

$cfg = Get-Content $ConfigPath -Raw | ConvertFrom-Json
$nineSliceRules = @()
if ($cfg -and $cfg.nineSliceRules) { $nineSliceRules = @($cfg.nineSliceRules) }

function Get-NineSliceBorder([string]$fileName) {
    foreach ($r in $nineSliceRules) {
        if ($fileName -like $r.pattern) {
            if ($r.border -and $r.border.Count -eq 4) {
                return "{0},{1},{2},{3}" -f $r.border[0], $r.border[1], $r.border[2], $r.border[3]
            }
            return ""
        }
    }
    return ""
}

$STYLE_CORE = "soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view"
$NEGATIVE_CORE = "photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, isometric, 3D scene, perspective skew, background scene, checkerboard background, alpha grid, transparency grid, tight framing, cropped, cut off, clipped, truncated edges, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts"
$EXPORT_CORE = "centered, no cropping, leave generous transparent padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG"

function Get-TemplateInfo([string]$fileName) {
    $state = Get-ButtonState $fileName

    if ($fileName -eq "bg_main.png") {
        return @{
            template = "BG_MAIN"
            prompt = "warm creamy background gradient, subtle bokeh accents in mint and pink, soft vignette, clean and minimal, no text, no characters"
            background = "opaque"
        }
    }
    if ($fileName -eq "overlay_dim.png") {
        return @{
            template = "OVERLAY_DIM"
            prompt = "full-screen dim overlay for mobile UI, smooth dark gradient, subtle noise, no hard edges, no text"
            background = "opaque"
        }
    }

    if ($fileName -like "shop_card_*.png") {
        $tone = "warm beige cream"
        if ($fileName -like "*purple*") { $tone = "soft lavender purple" }
        if ($fileName -like "*yellow*") { $tone = "soft butter yellow" }
        return @{
            template = "SHOP_CARD"
            prompt = "shop item card background, wide rounded rectangle, $tone fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, no text, no icons"
            background = "transparent"
        }
    }

    if ($fileName -like "shop_row_*.png") {
        $tone = "warm beige cream"
        if ($fileName -like "*purple*") { $tone = "soft lavender purple" }
        if ($fileName -like "*yellow*") { $tone = "soft butter yellow" }
        return @{
            template = "SHOP_ROW"
            prompt = "shop row background, wide rounded rectangle, $tone fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, no text, no icons"
            background = "transparent"
        }
    }

    if ($fileName -eq "shop_group_bar.png") {
        return @{
            template = "SHOP_GROUP_BAR"
            prompt = "shop group header bar background, rounded pill, dark chocolate fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "shop_scroll_fade_*.png") {
        $dir = if ($fileName -like "*top*") { "top fade" } else { "bottom fade" }
        return @{
            template = "SCROLL_FADE"
            prompt = "scroll view edge fade overlay, $dir, smooth alpha gradient from transparent to semi-opaque cream tint, no hard edges, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "shop_topbar_scallop_tile_512x128.png") {
        return @{
            template = "SCALLOP_TILE"
            prompt = "seamless scallop decorative trim tile, horizontally tileable, creamy plastic material, soft inner shadow, subtle highlight, no text, no icons, no background"
            background = "transparent"
        }
    }

    if ($fileName -like "panel_*.png") {
        $fill = "cream or blue"
        $frame = "gold or white"
        if ($fileName -eq "panel_thick_gold_blue.png") { $fill = "blue"; $frame = "gold" }
        return @{
            template = "PANEL_BASE"
            prompt = "UI panel background, rounded rectangle, $fill fill with soft inner gradient, $frame thick frame, beveled edges, inner shadow, gentle highlight, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "*_square_*.png") {
        $role = Normalize-Role (($fileName -split "_square_")[0])
        return @{
            template = "BTN_SQUARE"
            prompt = "square rounded button base, $role candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, $state state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "*_long_*.png") {
        $role = Normalize-Role (($fileName -split "_long_")[0])
        return @{
            template = "BTN_LONG"
            prompt = "long pill button base, $role candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, $state state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "btn_small_*_*.png") {
        $parts = $fileName -replace "\.png$", "" -split "_"
        $role = if ($parts.Length -ge 3) { Normalize-Role $parts[2] } else { "Blue" }
        return @{
            template = "BTN_SMALL"
            prompt = "small rounded button base, $role candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, $state state, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "btn_price_green_*.png") {
        return @{
            template = "BTN_PRICE"
            prompt = "price button background, green candy plastic, rounded rectangle, thick outline, top-left highlight, soft inner shadow, $state state, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "btn_close_red_*.png") {
        return @{
            template = "BTN_CLOSE"
            prompt = "close button background, orange-red candy plastic, rounded square, thick outline, top-left highlight, soft inner shadow, $state state, no text (X glyph is separate icon layer if needed)"
            background = "transparent"
        }
    }

    if ($fileName -like "hud_pill_dark*.png") {
        return @{
            template = "HUD_PILL"
            prompt = "HUD pill background, dark chocolate or dark navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "hud_level_label_bg.png") {
        return @{
            template = "HUD_LABEL"
            prompt = "HUD label background, creamy fill, subtle bevel, thin outline, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "tag_fast_*_bg.png") {
        $mood = if ($fileName -like "*danger*") { "danger (orange-red)" } else { "info (mint/blue)" }
        return @{
            template = "TAG_PILL"
            prompt = "pill tag background, $mood mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "tag_small_*_bg.png") {
        return @{
            template = "TAG_PILL_SMALL"
            prompt = "small pill tag background, info mood, rounded capsule, thin outline, subtle highlight, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "badge_red_bg.png") {
        return @{
            template = "BADGE_BG"
            prompt = "small notification badge background, red candy plastic, round shape, thick outline, glossy highlight, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "pill_bg*.png") {
        return @{
            template = "PILL_BG"
            prompt = "long pill button background, warm beige cream candy plastic, thick outline, top-left highlight, soft inner shadow, bottom shadow, $state state (Pressed=slightly darker + shorter shadow, Disabled=desaturated + reduced contrast), no icon, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "pill_timer_beige.png") {
        return @{
            template = "PILL_TIMER"
            prompt = "timer pill background, warm beige cream candy plastic, rounded capsule, subtle bevel, thin outline, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "lock_chip_plate.png") {
        return @{
            template = "LOCK_CHIP_PLATE"
            prompt = "lock chip plate background, rounded rectangle, creamy plastic, subtle bevel, medium outline, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "lock_node_base.png") {
        return @{
            template = "LOCK_NODE_BASE"
            prompt = "lock node base plate, rounded square with thick soft frame, creamy plastic, subtle bevel, inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "lock_node_label_bg.png") {
        return @{
            template = "LOCK_NODE_LABEL_BG"
            prompt = "lock node label background, small rounded pill, creamy plastic, subtle bevel, thin outline, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "lock_node_lock.png") {
        return @{
            template = "LOCK_NODE_LOCK"
            prompt = "UI lock icon glyph, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "digit_?.png") {
        $d = ($fileName -replace "^digit_", "") -replace "\.png$", ""
        return @{
            template = "DIGIT"
            prompt = "single digit glyph '$d', bold rounded toy font, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background"
            background = "transparent"
        }
    }

    if ($fileName -like "icon_*.png") {
        $subject = ($fileName -replace "^icon_", "") -replace "\.png$", ""
        $subject = $subject -replace "_128$", ""
        $subject = $subject -replace "_", " "
        return @{
            template = "ICON_GLYPH"
            prompt = "UI icon glyph of $subject, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "heart_big.png") {
        return @{
            template = "ICON_GLYPH"
            prompt = "UI icon glyph of heart, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean silhouette, no text"
            background = "transparent"
        }
    }

    if ($fileName -like "toggle_*.png") {
        return @{
            template = "TOGGLE"
            prompt = "toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text"
            background = "transparent"
        }
    }

    if ($fileName -eq "card_setting_row.png") {
        return @{
            template = "CARD_ROW"
            prompt = "settings row card background, creamy plastic, rounded rectangle, subtle bevel, soft inner shadow, thin outline, no text"
            background = "transparent"
        }
    }

    return @{
        template = "GENERIC"
        prompt = "UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text"
        background = "transparent"
    }
}

$uiSpritesDir = Join-Path $KitRoot "UI_Sprites"
if (!(Test-Path $uiSpritesDir)) { Fail "UI_Sprites not found: $uiSpritesDir" }

$allFiles = Get-ChildItem $uiSpritesDir -Filter *.png | Sort-Object Name

if ($IncludeKitWorldAndTextures) {
    $worldDir = Join-Path $KitRoot "World_Sprites"
    if (Test-Path $worldDir) {
        $worldFiles = Get-ChildItem $worldDir -Filter *.png | ForEach-Object {
            $_ | Add-Member -NotePropertyName DirName -NotePropertyValue "World_Sprites" -PassThru
        }
    }
    else {
        $worldFiles = @()
    }

    $beltDir = Join-Path $KitRoot "conveyor_belt_texture_v02_candy"
    if (Test-Path $beltDir) {
        $beltFiles = Get-ChildItem $beltDir -Filter *.png | ForEach-Object {
            $_ | Add-Member -NotePropertyName DirName -NotePropertyValue "conveyor_belt_texture_v02_candy" -PassThru
        }
    }
    else {
        $beltFiles = @()
    }
}
else {
    $worldFiles = @()
    $beltFiles = @()
}

if ($IncludeExtraResources) {
    $extraDirs = @("BoosterPurchase", "setting_page_assets")
    $extraFiles = @()
    foreach ($d in $extraDirs) {
        $dirPath = Join-Path "Assets/Resources" $d
        if (Test-Path $dirPath) {
            $extraFiles += Get-ChildItem $dirPath -Filter *.png | ForEach-Object {
                $_ | Add-Member -NotePropertyName DirName -NotePropertyValue $d -PassThru
            }
        }
    }

    $rootPng = Join-Path "Assets/Resources" "setting_page.png"
    if (Test-Path $rootPng) {
        $extraFiles += (Get-Item $rootPng) | ForEach-Object {
            $_ | Add-Member -NotePropertyName DirName -NotePropertyValue "ResourcesRoot" -PassThru
        }
    }
}
else {
    $extraFiles = @()
}

function Is-HudFile([string]$name) {
    $patterns = @(
        "*_square_*.png",
        "hud_pill_dark*.png",
        "hud_level_label_bg.png",
        "tag_fast_*.png",
        "badge_red_bg.png",
        "digit_*.png",
        "icon_*.png"
    )
    foreach ($p in $patterns) { if ($name -like $p) { return $true } }
    return $false
}

function In-Scope([string]$name) {
    if ($Scope -eq "All") { return $true }

    $isHud = Is-HudFile $name
    if ($Scope -eq "Hud") { return $isHud }
    if ($Scope -eq "Meta") { return -not $isHud }
    return $true
}

# Annotate UI_Sprites dir for uniform handling later.
$uiFiles = $allFiles | ForEach-Object { $_ | Add-Member -NotePropertyName DirName -NotePropertyValue "UI_Sprites" -PassThru }
$files = $uiFiles | Where-Object { In-Scope $_.Name }
$files += $worldFiles
$files += $beltFiles
$files += $extraFiles

if ([string]::IsNullOrWhiteSpace($OutFile)) {
    $OutFile = if ($Scope -eq "All") {
        "Tools/UiRestyleV05/_prompt_sheet_all_v05.md"
    }
    elseif ($Scope -eq "Meta") {
        "Tools/UiRestyleV05/_prompt_sheet_meta_v05.md"
    }
    else {
        "Tools/UiRestyleV05/_prompt_sheet_hud_v05.md"
    }
}

$sb = New-Object System.Text.StringBuilder
$null = $sb.AppendLine("# UI Prompt Sheet (v0.5 / Creamy Plastic) - $Scope")
$null = $sb.AppendLine("")
$null = $sb.AppendLine('Usage: copy each item prompt to generate a PNG with the exact same filename and pixel size, then run `Tools/UiRestyleV05/ReplacePngs.ps1` to overwrite Unity assets (only `.png`, keep `.meta`).')
$null = $sb.AppendLine("")
$null = $sb.AppendLine("Global constants (recommended):")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("**STYLE_CORE**")
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine($STYLE_CORE)
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("**NEGATIVE_CORE**")
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine($NEGATIVE_CORE)
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("**EXPORT_CORE**")
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine($EXPORT_CORE)
$null = $sb.AppendLine("~~~")
$null = $sb.AppendLine("")
$null = $sb.AppendLine("---")
$null = $sb.AppendLine("")

foreach ($f in $files) {
    $size = Get-PngSize $f.FullName
    $dirName = if ($f.PSObject.Properties.Match("DirName").Count -gt 0) { $f.DirName } else { "UI_Sprites" }

    $border = ""
    if ($dirName -eq "UI_Sprites") {
        $border = Get-NineSliceBorder $f.Name
    }

    $info = Get-TemplateInfo $f.Name

    $bgRule = if ($info.background -eq "opaque") { "opaque background" } else { "transparent background" }
    $positive = $info.prompt + ", exact $($size.w)x$($size.h)px, $bgRule, " + $EXPORT_CORE + ", " + $STYLE_CORE

    $null = $sb.AppendLine("## $dirName/$($f.Name) ($($size.w)x$($size.h))")
    $null = $sb.AppendLine("- template: $($info.template)")
    if (![string]::IsNullOrWhiteSpace($border)) {
        $null = $sb.AppendLine("- nine-slice border: $border")
    }
    $null = $sb.AppendLine("")
    $null = $sb.AppendLine("**Positive prompt**")
    $null = $sb.AppendLine("~~~")
    $null = $sb.AppendLine($positive)
    $null = $sb.AppendLine("~~~")
    $null = $sb.AppendLine("")
    $null = $sb.AppendLine("**Negative prompt**")
    $null = $sb.AppendLine("~~~")
    $null = $sb.AppendLine($NEGATIVE_CORE)
    $null = $sb.AppendLine("~~~")
    $null = $sb.AppendLine("")
}

Ensure-OutDir $OutFile
$sb.ToString() | Set-Content -Encoding UTF8 $OutFile
Write-Host "Wrote: $OutFile"
