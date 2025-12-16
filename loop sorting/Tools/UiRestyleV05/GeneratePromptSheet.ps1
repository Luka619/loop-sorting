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
$EXPORT_CORE = "centered, no cropping, leave generous padding on all sides (at least ~8% of canvas), ensure the full silhouette and all shadows are fully inside the frame, no element touches the image border, crisp edges, sRGB, PNG, straight alpha"

function Strip-PngSuffix([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return "" }
    # Handle odd names like "icon_shuffle_noframe.png.png"
    return ($s -replace "(?i)(\\.png)+$", "")
}

function Get-StatePhrase([string]$state) {
    switch ($state) {
        "Pressed" { return "Pressed state: slightly darker (6-10%), shorter shadow (40-60%), weaker highlight; same shape" }
        "Disabled" { return "Disabled state: desaturated (25-45%), reduced contrast, lighter shadow; same shape" }
        default { return "Normal state: brightest highlight, longest shadow, highest contrast" }
    }
}

function Get-IconSubject([string]$fileName) {
    $base = Strip-PngSuffix($fileName)
    $base = $base -replace "^icon_", ""
    $base = $base -replace "_128$", ""
    $key = $base.ToLowerInvariant()
    $map = @{
        "retry" = "retry arrow (refresh)"
        "pause" = "pause symbol ||"
        "shop" = "shop storefront"
        "gear" = "gear"
        "loop" = "loop ring"
        "lock" = "padlock"
        "shuffle" = "shuffle arrows"
        "sort" = "sort icon"
        "next" = "right arrow"
        "plus" = "plus sign"
        "close" = "X cross"
        "clock" = "clock"
        "music" = "music note"
        "vibrate" = "vibration waves"
        "video" = "video/play icon"
        "heart" = "heart"
        "fill" = "filled rounded square"
        "no_ads_tv" = "TV with a no-ads prohibition symbol"
        "coin" = "coin"
        "coin_stack" = "coin stack"
        "coin_bag" = "coin bag"
        "coin_chest" = "coin chest"
        "coin_safe" = "coin safe"
    }
    if ($map.ContainsKey($key)) { return $map[$key] }

    # Fallback: turn into readable words.
    return ($key -replace "_", " ")
}

function Get-TemplateInfo([string]$fileName) {
    $state = Get-ButtonState $fileName

    if ($dirName -eq "UI_Sprites" -and $fileName -eq "bg_main.png") {
        return @{
            template = "BG_MAIN"
            prompt = "full-screen background for mobile game UI, warm creamy gradient, subtle mint/pink bokeh accents, soft vignette, clean and minimal, no UI frames, no buttons, no text, no characters"
            background = "opaque"
            layer = "background (full-screen)"
            negative = "UI panels, UI frames, buttons, icons, text blocks"
        }
    }
    if ($dirName -eq "UI_Sprites" -and $fileName -eq "overlay_dim.png") {
        return @{
            template = "OVERLAY_DIM"
            prompt = "modal dim overlay mask for mobile UI (behind popups), soft dark vignette and shadow, center-weighted darkening with gentle falloff, no hard edges, no text"
            background = "transparent"
            layer = "overlay mask (dim/shadow)"
            negative = "button, panel frame, icon, text"
        }
    }

    if ($fileName -like "shop_card_*.png") {
        $tone = "warm beige cream"
        if ($fileName -like "*purple*") { $tone = "soft lavender purple" }
        if ($fileName -like "*yellow*") { $tone = "soft butter yellow" }
        return @{
            template = "SHOP_CARD"
            prompt = "shop item card background plate (content container), wide rounded rectangle, $tone fill, subtle inner gradient, medium outline, soft bevel, gentle inner shadow, faint highlight at top-left, flat clean center area for content, no text, no icons"
            background = "transparent"
            layer = "panel/card background"
            negative = "embedded text, embedded icons, ornate corner decorations, patterned embossing"
        }
    }

    if ($fileName -like "shop_row_*.png") {
        $tone = "warm beige cream"
        if ($fileName -like "*purple*") { $tone = "soft lavender purple" }
        if ($fileName -like "*yellow*") { $tone = "soft butter yellow" }
        return @{
            template = "SHOP_ROW"
            prompt = "shop row background plate (list row container), wide rounded rectangle, $tone fill, subtle inner gradient, thin outline, soft bevel, gentle inner shadow, flat clean center area for row content, no text, no icons"
            background = "transparent"
            layer = "panel/row background"
            negative = "embedded text, embedded icons, ornate corner decorations, patterned embossing"
        }
    }

    if ($fileName -eq "shop_group_bar.png") {
        return @{
            template = "SHOP_GROUP_BAR"
            prompt = "shop group header bar background plate, rounded pill, dark chocolate fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, reserve clean center area for header text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons, ornate decorations"
        }
    }

    if ($fileName -like "shop_scroll_fade_*.png") {
        $dir = if ($fileName -like "*top*") { "top fade" } else { "bottom fade" }
        return @{
            template = "SCROLL_FADE"
            prompt = "scroll view edge fade overlay, $dir, smooth vertical opacity gradient to 0 alpha at the edge, soft cream tint, no hard edges, no text"
            background = "transparent"
            layer = "overlay fade"
            negative = "text, icons, hard cutoff, banding"
        }
    }

    if ($fileName -eq "shop_topbar_scallop_tile_512x128.png") {
        return @{
            template = "SCALLOP_TILE"
            prompt = "seamless scallop decorative trim tile, horizontally tileable (seamless at left/right edges), creamy plastic material, soft inner shadow, subtle highlight, no text, no icons"
            background = "transparent"
            layer = "decorative tile (tileable)"
            negative = "visible seams, non-tileable edges, text, icons"
        }
    }

    if ($fileName -like "panel_*.png") {
        $fill = "cream or blue"
        $frame = "gold or white"
        if ($fileName -eq "panel_thick_gold_blue.png") { $fill = "blue"; $frame = "gold" }
        return @{
            template = "PANEL_BASE"
            prompt = "UI panel background (content container), rounded rectangle, $fill fill with soft inner gradient, $frame thick frame, beveled edges, inner shadow, gentle highlight, flat clean center area for content, no text"
            background = "transparent"
            layer = "panel background"
            negative = "embedded text, embedded icons, ornate patterns, corner ornaments, noisy texture"
        }
    }

    if ($fileName -like "*_square_*.png") {
        $role = Normalize-Role (($fileName -split "_square_")[0])
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "BTN_SQUARE"
            prompt = "pressable square rounded button base for mobile UI, $role candy plastic, thick outline using darker tone, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, flat clean center area for overlay icon/text, $statePhrase, base-only (no embedded icon, no embedded text)"
            background = "transparent"
            layer = "base-only (icon/text is separate)"
            negative = "embedded text, embedded icon, corner ornaments, asymmetric decorations, embossed patterns"
        }
    }

    if ($fileName -like "*_long_*.png") {
        $role = Normalize-Role (($fileName -split "_long_")[0])
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "BTN_LONG"
            prompt = "pressable long pill button base for mobile UI, $role candy plastic, thick outline, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, flat clean center area for overlay text/icon, $statePhrase, base-only (no embedded icon, no embedded text)"
            background = "transparent"
            layer = "base-only (icon/text is separate)"
            negative = "embedded text, embedded icon, corner ornaments, asymmetric decorations, embossed patterns"
        }
    }

    if ($fileName -like "btn_small_*_*.png") {
        $parts = $fileName -replace "\.png$", "" -split "_"
        $role = if ($parts.Length -ge 3) { Normalize-Role $parts[2] } else { "Blue" }
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "BTN_SMALL"
            prompt = "pressable small rounded button base for mobile UI, $role candy plastic, thick outline, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, flat clean center area for overlay label, $statePhrase, base-only (no embedded text)"
            background = "transparent"
            layer = "base-only (text is separate)"
            negative = "embedded text, embedded icon, ornate edges, asymmetric decorations"
        }
    }

    if ($fileName -like "btn_price_green_*.png") {
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "BTN_PRICE"
            prompt = "price button background plate (label container), green candy plastic, rounded rectangle, thick outline, soft bevel, top-left highlight, soft inner shadow, $statePhrase, reserve clean flat center area for price text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, currency symbols, embossed patterns"
        }
    }

    if ($fileName -like "btn_close_red_*.png") {
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "BTN_CLOSE"
            prompt = "pressable close button base, orange-red candy plastic, rounded square, thick outline, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, $statePhrase, base-only (X glyph is separate icon layer), no embedded text"
            background = "transparent"
            layer = "base-only (icon is separate)"
            negative = "embedded X, embedded text, corner ornaments"
        }
    }

    if ($fileName -like "hud_pill_dark*.png") {
        return @{
            template = "HUD_PILL"
            prompt = "HUD pill background plate for counters, dark chocolate/navy fill, low contrast gradient, thin outline, subtle bevel, soft inner shadow, soft highlight, reserve a clean flat center area for number/text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded numbers, embedded text, noisy texture, ornate decorations"
        }
    }

    if ($fileName -eq "hud_level_label_bg.png") {
        return @{
            template = "HUD_LABEL"
            prompt = "HUD level label background plate, creamy fill, subtle bevel, thin outline, soft inner shadow, reserve clean center area for level text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons, noisy texture"
        }
    }

    if ($fileName -like "tag_fast_*_bg.png") {
        $mood = if ($fileName -like "*danger*") { "danger (orange-red)" } else { "info (mint/blue)" }
        return @{
            template = "TAG_PILL"
            prompt = "pill tag background plate for HUD label, $mood mood, rounded capsule, thin but clear outline, subtle highlight from top-left, gentle inner shadow, reserve a clean flat center area for text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons, noisy texture, corner ornaments"
        }
    }

    if ($fileName -like "tag_small_*_bg.png") {
        return @{
            template = "TAG_PILL_SMALL"
            prompt = "small pill tag background plate for HUD label, info mood, rounded capsule, thin outline, subtle highlight from top-left, gentle inner shadow, reserve clean center area for text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons, noisy texture"
        }
    }

    if ($fileName -eq "badge_red_bg.png") {
        return @{
            template = "BADGE_BG"
            prompt = "small notification badge background plate, red candy plastic, round shape, thick outline, glossy highlight, soft inner shadow, reserve clean center area for number overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, numbers, icons"
        }
    }

    if ($fileName -like "pill_bg*.png") {
        $statePhrase = Get-StatePhrase $state
        return @{
            template = "PILL_BG"
            prompt = "pressable long pill background plate, warm beige cream candy plastic, thick outline, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, flat clean center area for overlay text/icon, $statePhrase, base-only (no embedded icon, no embedded text)"
            background = "transparent"
            layer = "base-only (icon/text is separate)"
            negative = "embedded text, embedded icon, corner ornaments, asymmetric decorations"
        }
    }

    if ($fileName -eq "pill_timer_beige.png") {
        return @{
            template = "PILL_TIMER"
            prompt = "timer pill background plate, warm beige cream candy plastic, rounded capsule, subtle bevel, thin outline, soft inner shadow, reserve clean center area for timer text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons, noisy texture"
        }
    }

    if ($fileName -eq "lock_chip_plate.png") {
        return @{
            template = "LOCK_CHIP_PLATE"
            prompt = "lock chip plate background (container for lock icon + color disc), rounded rectangle, creamy plastic, subtle bevel, medium outline, soft inner shadow, reserve clean center area for overlay icon, no embedded text"
            background = "transparent"
            layer = "panel/background"
            negative = "embedded text, ornate patterns"
        }
    }

    if ($fileName -eq "lock_node_base.png") {
        return @{
            template = "LOCK_NODE_BASE"
            prompt = "lock node base plate, rounded square with thick soft frame, creamy plastic, subtle bevel, inner shadow, flat center area for overlay content, no text"
            background = "transparent"
            layer = "panel/background"
            negative = "text, icons, ornate patterns"
        }
    }

    if ($fileName -eq "lock_node_label_bg.png") {
        return @{
            template = "LOCK_NODE_LABEL_BG"
            prompt = "lock node label background plate, small rounded pill, creamy plastic, subtle bevel, thin outline, soft inner shadow, reserve clean center area for label text overlay, no embedded text"
            background = "transparent"
            layer = "text background (no embedded text)"
            negative = "embedded text, icons"
        }
    }

    if ($fileName -eq "lock_node_lock.png") {
        return @{
            template = "LOCK_NODE_LOCK"
            prompt = "UI icon glyph of padlock, single bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean edges, no text, no background plate, centered (optical center)"
            background = "transparent"
            layer = "glyph-only (no background plate)"
            negative = "background plate, button base, letters, words"
        }
    }

    if ($fileName -like "digit_?.png") {
        $d = ($fileName -replace "^digit_", "") -replace "\.png$", ""
        return @{
            template = "DIGIT"
            prompt = "single digit glyph '$d', bold rounded toy font, consistent baseline and stroke thickness across 0-9, consistent left/right padding, warm white fill, thick dark brown outline, subtle inner shading, crisp edges, no background plate, centered"
            background = "transparent"
            layer = "digit-glyph-only"
            negative = "multiple digits, letters, words, punctuation, background plate"
        }
    }

    if ($fileName -like "icon_*.png") {
        $subject = Get-IconSubject $fileName
        return @{
            template = "ICON_GLYPH"
            prompt = "UI icon glyph of $subject, single symbol, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean edges, no text, no letters, no background plate, centered (optical center)"
            background = "transparent"
            layer = "glyph-only (no background plate)"
            negative = "button base, rounded square plate, circle badge, letters, words, watermark"
        }
    }

    if ($fileName -eq "heart_big.png") {
        return @{
            template = "ICON_GLYPH"
            prompt = "UI icon glyph of heart, single symbol, bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean edges, no text, no background plate, centered (optical center)"
            background = "transparent"
            layer = "glyph-only (no background plate)"
            negative = "button base, background plate, letters, words"
        }
    }

    if ($fileName -like "toggle_*.png") {
        $base = Strip-PngSuffix($fileName)
        if ($base -like "toggle_track_*") {
            $isOn = $base -like "*_on"
            $tone = if ($isOn) { "ON track (mint/green accent)" } else { "OFF track (desaturated/gray)" }
            return @{
                template = "TOGGLE_TRACK"
                prompt = "toggle track only (no knob), rounded capsule track with inner recess, $tone, thick outline, subtle highlight from top-left, soft inner shadow, no text"
                background = "transparent"
                layer = "toggle track only"
                negative = "knob, slider handle, checkbox, ON/OFF text, letters"
            }
        }
        if ($base -eq "toggle_knob") {
            return @{
                template = "TOGGLE_KNOB"
                prompt = "toggle knob only (no track), circular glossy knob, warm cream plastic, strong highlight from top-left, subtle ambient occlusion and tiny drop shadow, centered, no text"
                background = "transparent"
                layer = "toggle knob only"
                negative = "track, slider bar, ON/OFF text, checkbox"
            }
        }
        if ($base -like "toggle_full_*") {
            $isOn = $base -like "*_on"
            $tone = if ($isOn) { "ON state (mint/green accent), knob on the right" } else { "OFF state (desaturated/gray), knob on the left" }
            return @{
                template = "TOGGLE_FULL"
                prompt = "integrated toggle switch (track + knob in one sprite), rounded capsule track, circular knob, $tone, thick outline, subtle highlight from top-left, soft inner shadow, no ON/OFF text"
                background = "transparent"
                layer = "toggle (integrated)"
                negative = "checkbox, slider with ticks, labels, letters"
            }
        }
        return @{
            template = "TOGGLE"
            prompt = "toggle UI component part, creamy plastic toy style, rounded beveled edges, thick outline, soft inner shadow, no text"
            background = "transparent"
            layer = "toggle part"
            negative = "checkbox, slider with ticks, labels, letters"
        }
    }

    if ($fileName -eq "card_setting_row.png") {
        return @{
            template = "CARD_ROW"
            prompt = "settings row card background (list row container), creamy plastic, rounded rectangle, subtle bevel, soft inner shadow, thin outline, flat clean center area for row content, no text"
            background = "transparent"
            layer = "panel/row background"
            negative = "embedded text, embedded icons, ornate patterns, noisy texture"
        }
    }

    return @{
        template = "GENERIC"
        prompt = "UI element background, creamy plastic toy style, rounded corners, thick outline, soft inner shadow, no text"
        background = "transparent"
        layer = "unspecified"
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
    # Consolidated workflow: keep a single canonical prompt sheet.
    # If you want subset outputs, pass -OutFile explicitly.
    $OutFile = "Tools/UiRestyleV05/_prompt_sheet_all_v05.md"
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

    $info = Get-TemplateInfo $dirName $f.Name

    # Background intent is carried as a metadata field (see below); the positive prompt focuses on function/shape.
    $nineSliceHint = ""
    if (![string]::IsNullOrWhiteSpace($border)) {
        $nineSliceHint = ", 9-slice friendly: keep border uniform, no unique edge details, flat clean center area"
    }

    $positive = $info.prompt + $nineSliceHint + ", exact $($size.w)x$($size.h)px, " + $EXPORT_CORE + ", " + $STYLE_CORE

    $negative = $NEGATIVE_CORE
    if ($info.negative) {
        $negative = $negative + ", " + $info.negative
    }

    $null = $sb.AppendLine("## $dirName/$($f.Name) ($($size.w)x$($size.h))")
    $null = $sb.AppendLine("- template: $($info.template)")
    $null = $sb.AppendLine("- background: $($info.background)")
    if ($info.layer) {
        $null = $sb.AppendLine("- layer: $($info.layer)")
    }
    if ($f.Name -like "*_pressed.png") {
        $null = $sb.AppendLine("- state: Pressed")
    }
    elseif ($f.Name -like "*_disabled.png") {
        $null = $sb.AppendLine("- state: Disabled")
    }
    if (![string]::IsNullOrWhiteSpace($border)) {
        $null = $sb.AppendLine("- nine-slice: YES")
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
    $null = $sb.AppendLine($negative)
    $null = $sb.AppendLine("~~~")
    $null = $sb.AppendLine("")
}

Ensure-OutDir $OutFile
$sb.ToString() | Set-Content -Encoding UTF8 $OutFile
Write-Host "Wrote: $OutFile"
