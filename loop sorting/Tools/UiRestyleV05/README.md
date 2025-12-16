# UiRestyleV05（Creamy Plastic）资源替换工具

目的：把你用 AI 生成的 PNG **安全地** 覆盖到 Unity 工程中（只替换 `.png`，不动 `.meta`），并在替换前做 **像素尺寸校验**，避免 UI 被拉伸/9-slice 崩掉。

## 1) 你需要准备什么
- 生成后的文件放在一个目录里（推荐结构）：
  - `<YourOutput>/UI_Sprites/*.png`
  - （可选）`<YourOutput>/World_Sprites/*.png`
- 文件名必须与工程内一致（见 `UI_ASSET_PROMPT_PACK_V05_CREAMY_PLASTIC.md` 的映射表）
- 像素尺寸必须与工程内原文件一致

## 2) 先导出当前工程的尺寸清单（可选但推荐）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReportPngSizes.ps1 -OutFile Tools/UiRestyleV05/_sizes_ui_sprites.json
```

## 2.1) 生成 Prompt Sheet（可选但推荐）
用于把“逐文件可复制的 Prompt”输出成一个 Markdown，方便你按文件名批量出图。
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope Hud
```
- 输出：`Tools/UiRestyleV05/_prompt_sheet_hud_v05.md`
- 玩法外（商店/设置/结算等）：`powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope Meta`
  - 输出：`Tools/UiRestyleV05/_prompt_sheet_meta_v05.md`
- 全量：`powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope All`
  - 输出：`Tools/UiRestyleV05/_prompt_sheet_all_v05.md`

## 2.2) 直接在本地“出图”（占位版，可用于先跑通替换与预览）
如果你现在不方便用外部 AI 出图，我可以先用脚本在本机生成一套“风格一致的占位 PNG”（按钮底/图标/数字/HUD pill/FAST tag），用于你在 Unity 里快速预览布局与风格方向。
```powershell
python Tools/UiRestyleV05/GenerateProceduralHudV05.py --prompt-sheet Tools/UiRestyleV05/_prompt_sheet_hud_v05.md --out-dir Tools/UiRestyleV05/_generated_v05 --scale 1
```
- 输出目录：`Tools/UiRestyleV05/_generated_v05/UI_Sprites/`
- 覆盖到工程（建议先演练）：`powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_generated_v05 -DryRun -AllowPartial`

## 2.3) 用 API易 代理批量“真正出图”（推荐）
统一走 API 代理（API易），在 API 调用里设置 `background=transparent`，并使用 `--no-postprocess` 直接保存模型输出：不切图、不抠图。

准备：
- 推荐：把 API易 key 粘贴到 `Tools/UiRestyleV05/_secrets/openai_api_key.txt`（只一行，不要引号）
- 或者：只在本机环境变量里放 key（不要写进任何文件）：
```powershell
$env:OPENAI_API_KEY = "<your key>"
```

先 dry-run 看要生成哪些文件（质量最低，省钱优先）：
```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py --api-base https://api.apiyi.com/v1 --model gpt-image-1-mini --quality low --gen-size auto --background transparent --no-postprocess --parallel 5 --dry-run --limit 5
```

正式生成（质量最低，建议先小批量试跑）：
```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py --api-base https://api.apiyi.com/v1 --model gpt-image-1-mini --quality low --gen-size auto --background transparent --no-postprocess --parallel 5 --limit 10
```
（默认会跳过已存在的输出文件；需要重生成时加 `--overwrite`）

如需把输出严格对齐到工程内原图的像素尺寸/留白（会做对齐/缩放），去掉 `--no-postprocess`。

输出目录（默认）：
- `Tools/UiRestyleV05/_openai_output/UI_Sprites/*.png`

把生成结果覆盖进 Unity 工程（先演练再备份替换）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -DryRun -AllowPartial
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -Backup -AllowPartial
```

## 2.4) 用网页版 ChatGPT 出图（不走 API 计费）+ 本地归一化（不再推荐）
现在统一使用 2.3 的 API易 代理出图（`--background transparent`），通常不再需要走网页版 + 归一化；下面内容仅保留备忘。
网页版出图的“像素尺寸/透明背景/留白”不一定 100% 按要求输出，所以推荐流程是：
1) 网页版按 `Tools/UiRestyleV05/_prompt_sheet_hud_v05.md` 逐个出图
2) **下载后改成工程要求的文件名**，放到 `<WebOut>/UI_Sprites/`
3) 运行归一化脚本：自动做“透明背景（可选）+ 对齐 bbox + 输出为精确尺寸 PNG”
4) 再用 `ReplacePngs.ps1` 覆盖到 Unity 工程

### A) 网页版单张出图提示词模板（复制用）
把下面模板里 `FILE_NAME / SIZE / POSITIVE / NEGATIVE` 替换为 prompt sheet 对应内容：
```
你是移动端休闲解谜游戏的 UI 资产制作总监。
请为 Unity UGUI 生成一张 UI Sprite，要求：

- FILE_NAME: <mint_square_normal.png>
- OUTPUT: PNG
- BACKGROUND: transparent (除 bg_main/overlay_dim 外都要透明)
- SIZE: 以你能输出的最高分辨率生成也可以，但主体必须正交正面、居中、留出阴影安全边距，禁止裁切
- LIGHT: highlight top-left, shadow bottom-right
- NO: extra text, watermark, logo, perspective skew, background scene

POSITIVE PROMPT:
<粘贴 Tools/UiRestyleV05/_prompt_sheet_hud_v05.md 的 Positive prompt>

NEGATIVE PROMPT:
<粘贴 Tools/UiRestyleV05/_prompt_sheet_hud_v05.md 的 Negative prompt>
```

### B) 下载后归一化到工程要求尺寸
假设你的下载整理目录是 `D:\ui_web_out\UI_Sprites\*.png`（文件名已改成工程同名）：
```powershell
python Tools/UiRestyleV05/NormalizeWebImages.py --in-dir D:\ui_web_out --out-dir Tools/UiRestyleV05/_web_output --allow-partial
```
输出会在：`Tools/UiRestyleV05/_web_output/UI_Sprites/*.png`

### C) 覆盖到 Unity 工程（先演练再备份替换）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_web_output -DryRun -AllowPartial
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_web_output -Backup -AllowPartial
```

## 3) 执行替换（只替换 UI_Sprites）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir "<YourOutput>" -Backup
```

## 4) 也替换 World_Sprites（可选）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir "<YourOutput>" -IncludeWorldSprites -Backup
```

## 5) 只演练（不写入）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir "<YourOutput>" -DryRun
```

## 6) 常见问题
- **提示尺寸不一致**：说明你出图尺寸改了；必须按原文件宽高重出（不要靠 Unity 缩放补救）。
- **只想替换部分文件**：加 `-AllowPartial`，缺失文件会跳过但会提示。
