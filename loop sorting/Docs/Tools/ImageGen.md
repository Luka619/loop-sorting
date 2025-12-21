# Image Generation (API易 / OpenAI-compatible)

> 总索引：`../README.md`

This folder contains a small script to generate PNGs using an OpenAI-compatible Images API (e.g. API易), with model `gpt-image-1.5`.

## Setup

- Set env vars (PowerShell):
  - `APIYI_BASE_URL` (proxy base URL, e.g. `https://api.apiyi.com/v1`; never use `https://api.openai.com/v1`)
  - `APIYI_API_KEY` (your key)

Or pass them per-command:
- `--api-base https://api.apiyi.com/v1` (proxy only; never use `https://api.openai.com/v1`)
- `--api-key-file <path>` (or `--api-key <key>`)

If your gateway is OpenAI-compatible, the script will call `POST {baseUrl}/images/generations` (or `{baseUrl}/v1/images/generations` if `baseUrl` doesn't end with `/v1`).

Optional args (supported by some gateways/models):
- `--quality low|medium|high`
- `--background transparent`

For the full UI batch workflow using API易, see `UiRestyleV05/UI_CONCEPT_TO_ASSETS_WORKFLOW_V05.md`.

## Generate a grey conveyor slot

```powershell
python Tools/ImageGen/apiyi_image_gen.py `
  --model gpt-image-1.5 `
  --size 512x512 `
  --quality low `
  --background transparent `
  --out Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot_apiyi_gray.png `
  --prompt "Top-down conveyor belt slot marker icon, square 512x512 PNG, transparent background, neutral gray palette, subtle bevel, soft inner shadow, minimal stylized game UI, centered, clean edges, no text"
```

Optional (copy Unity import settings + update mapping):

```powershell
python Tools/ImageGen/apiyi_image_gen.py `
  --model gpt-image-1.5 `
  --size 512x512 `
  --quality low `
  --background transparent `
  --out Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot_apiyi_gray.png `
  --meta-template Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot.png.meta `
  --apply-mapping
```

## Generate game title text ("方块转不停")

One-command wrapper:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/ImageGen/generate_title_fangkuai_zhuan_bu_ting.ps1
```

With explicit API parameters:

```powershell
powershell -ExecutionPolicy Bypass -File Tools/ImageGen/generate_title_fangkuai_zhuan_bu_ting.ps1 `
  -ApiBase https://api.apiyi.com/v1 `
  -ApiKeyFile C:\\path\\to\\api_key.txt
```

Or call directly:

```powershell
python Tools/ImageGen/apiyi_image_gen.py `
  --api-base https://api.apiyi.com/v1 `
  --api-key-file C:\\path\\to\\api_key.txt `
  --model gpt-image-1.5 `
  --size 1536x1024 `
  --quality low `
  --background transparent `
  --out Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/title_fangkuai_zhuan_bu_ting.png `
  --meta-template Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/btn_small_blue_normal.png.meta `
  --prompt "Game title logo text in Chinese: 方块转不停. Only the text, no other words, no watermark. Cute blocky arcade style, creamy 3D plastic, rounded beveled edges, thick outline, subtle shadow, centered, generous padding, transparent background."
```
