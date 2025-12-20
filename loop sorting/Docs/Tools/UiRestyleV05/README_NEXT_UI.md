# 下一阶段：全游戏 UI 重做清单（v0.5 / Creamy Plastic）

> 总索引：`../../README.md`

目标：在已完成 HUD 的基础上，继续把 **设置 / 商店 / 购买弹窗 / 结算 / 关卡** 等 UI 全部替换到同一风格，并保持“同名覆盖 `.png`、不动 `.meta`”的闭环流程。

**重要（避免图片被截断）**：透明背景资源不要做任何“对齐尺寸/抠图/裁切/贴边”的后处理；本工具链也不会做这些处理。请从源头出图时留足 padding，保证阴影/外轮廓完整在画布内。

## 1) 资源范围（当前项目）
- UIKit 包内（可直接替换）
  - `Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/UI_Sprites/*.png`
  - `Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/World_Sprites/*.png`（可选）
- UIKit 包外（也属于 UI，需要一起重做）
  - `Assets/Resources/setting_page.png`（设置页整张底图）
  - `Assets/Resources/setting_page_assets/*.png`（设置页按钮/Toggle 叠加图）
  - `Assets/Resources/BoosterPurchase/*.png`（Boosters 购买弹窗整套图）

## 2) Prompt（推荐：Prompt DB）
- 主存储：`Tools/UiRestyleV05/_prompt_db_all_v05.json`

## 3) 推荐重做顺序
1) 面板与按钮体系：`panel_*`, `btn_small_*`, `btn_price_green_*`, `btn_close_red_*`, `*_long_*`, `pill_*`
2) 商店：`shop_*`（卡片/行/分组/滚动 fade/装饰 tile）
3) 锁节点：`lock_*`
4) 设置页：`setting_page.png` + `setting_page_assets/*.png`
5) BoosterPurchase：`Assets/Resources/BoosterPurchase/*.png`
6) （可选）World_Sprites：完成锁等世界 UI 视觉统一

## 4) 直接用 API 易批量出图到 `_openai_output`
```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py `
  --api-base https://api.apiyi.com/v1 `
  --model gpt-image-1.5 `
  --quality low `
  --gen-size auto `
  --background transparent `
  --parallel 5 `
  --prompt-sheet Tools/UiRestyleV05/_prompt_db_all_v05.json `
  --out-dir Tools/UiRestyleV05/_openai_output `
  --overwrite
```

`UI_Sprites/bg_main.png` 建议单独用 `--background opaque` 重出，避免关卡背景缺失：
```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py `
  --api-base https://api.apiyi.com/v1 `
  --model gpt-image-1.5 `
  --quality low `
  --gen-size auto `
  --background opaque `
  --only UI_Sprites/bg_main.png `
  --prompt-sheet Tools/UiRestyleV05/_prompt_db_all_v05.json `
  --out-dir Tools/UiRestyleV05/_openai_output `
  --overwrite
```

## 5) 覆盖到 Unity 工程（保留 `.meta`）
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -Backup -AllowPartial
```
