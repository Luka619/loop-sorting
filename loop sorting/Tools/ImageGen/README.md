# Image Generation (API易 / OpenAI-compatible)

This folder contains a small script to generate PNGs using an OpenAI-compatible Images API (e.g. API易), with model `gpt-image-1.5`.

## Setup

- Set env vars (PowerShell):
  - `APIYI_BASE_URL` (example: `https://<your-host>/v1` or `https://<your-host>`)
  - `APIYI_API_KEY` (your key)

If your gateway is OpenAI-compatible, the script will call `POST {baseUrl}/images/generations` (or `{baseUrl}/v1/images/generations` if `baseUrl` doesn't end with `/v1`).

## Generate a grey conveyor slot

```powershell
python Tools/ImageGen/apiyi_image_gen.py `
  --model gpt-image-1.5 `
  --size 512x512 `
  --out Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot_apiyi_gray.png `
  --prompt "Top-down conveyor belt slot marker icon, square 512x512 PNG, transparent background, neutral gray palette, subtle bevel, soft inner shadow, minimal stylized game UI, centered, clean edges, no text"
```

Optional (copy Unity import settings + update mapping):

```powershell
python Tools/ImageGen/apiyi_image_gen.py `
  --model gpt-image-1.5 `
  --size 512x512 `
  --out Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot_apiyi_gray.png `
  --meta-template Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/conveyor_slot.png.meta `
  --apply-mapping
```

