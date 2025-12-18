param(
  [Alias("ApiBase")]
  [string]$BaseUrl = $env:APIYI_BASE_URL,
  [string]$ApiKey = $env:APIYI_API_KEY,
  [string]$ApiKeyFile = "",
  [string]$Model = "gpt-image-1.5",
  [string]$Size = "1536x1024",
  [string]$Quality = "low",
  [string]$Background = "transparent",
  [string]$Out = "Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/title_fangkuai_zhuan_bu_ting.png",
  [string]$MetaTemplate = "Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/btn_small_blue_normal.png.meta"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\\..")
$pyScript = Join-Path $repoRoot "Tools/ImageGen/apiyi_image_gen.py"
$outPath = if ([System.IO.Path]::IsPathRooted($Out)) { $Out } else { Join-Path $repoRoot $Out }
$metaTemplatePath = if ([System.IO.Path]::IsPathRooted($MetaTemplate)) { $MetaTemplate } else { Join-Path $repoRoot $MetaTemplate }
$apiKeyFilePath = if (-not $ApiKeyFile) { "" } elseif ([System.IO.Path]::IsPathRooted($ApiKeyFile)) { $ApiKeyFile } else { Join-Path $repoRoot $ApiKeyFile }

$baseUrlEffective = $BaseUrl
if (-not $baseUrlEffective) { $baseUrlEffective = $env:OPENAI_BASE_URL }
if (-not $baseUrlEffective) { throw "Missing API base URL. Set APIYI_BASE_URL/OPENAI_BASE_URL or pass -BaseUrl/-ApiBase." }

$apiKeyEffective = $ApiKey
if (-not $apiKeyEffective -and $apiKeyFilePath) { $apiKeyEffective = [System.IO.File]::ReadAllText($apiKeyFilePath).Trim() }
if (-not $apiKeyEffective) { $apiKeyEffective = $env:OPENAI_API_KEY }
if (-not $apiKeyEffective -and -not $apiKeyFilePath) { throw "Missing API key. Set APIYI_API_KEY/OPENAI_API_KEY or pass -ApiKey/-ApiKeyFile." }

$titleText = [string]::Concat(
  [char]0x65B9,
  [char]0x5757,
  [char]0x8F6C,
  [char]0x4E0D,
  [char]0x505C
)

$prompt = @"
Game title logo text in Chinese: $titleText
Only the text. No other words. No watermark.
Cute blocky arcade style that matches a creamy-plastic mobile puzzle UI:
soft 3D plastic, rounded beveled edges, subtle specular highlight, soft inner shadow/ambient occlusion,
thick dark outline for readability, gentle vertical gradient fill, small confetti/square accents suggesting rotation/motion.
Centered, generous padding, transparent background.
"@

$keyArgs = if ($apiKeyFilePath) { @("--api-key-file", $apiKeyFilePath) } else { @("--api-key", $apiKeyEffective) }

python $pyScript `
  --api-base $baseUrlEffective `
  $keyArgs `
  --model $Model `
  --size $Size `
  --quality $Quality `
  --background $Background `
  --out $outPath `
  --meta-template $metaTemplatePath `
  --prompt $prompt
