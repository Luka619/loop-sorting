# UI 重制计划（v0.5 / Creamy Plastic）

目标：把现有 UI 资源替换为“奶油底 + 橙色标题条 + 3D 塑料糖果质感”的统一风格，并优先保证 **玩法界面 + HUD** 的高品质表现。

## 1) 你将使用的文档/工具
- 风格规范：`UI_STYLE_GUIDE_V05_CREAMY_PLASTIC.md`
- 资源出图 Prompt Pack：`UI_ASSET_PROMPT_PACK_V05_CREAMY_PLASTIC.md`
- HUD 工作流：`UI_WORKFLOW_GAMEPLAY_HUD.md`
- 资源替换脚本：`Tools/UiRestyleV05/README.md`
- Prompt Sheet 生成器：`Tools/UiRestyleV05/GeneratePromptSheet.ps1`

## 1.1) 开始重制（今天就能跑通的一套最小闭环）
1) 导出当前 UI_Sprites 的像素尺寸清单（给出图“定尺”）：
   - `powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReportPngSizes.ps1 -OutFile Tools/UiRestyleV05/_sizes_ui_sprites.json`
2) 生成 HUD 核心资源的逐文件 Prompt Sheet（直接复制出图）：
   - `powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/GeneratePromptSheet.ps1 -Scope Hud`
   - 输出文件：`Tools/UiRestyleV05/_prompt_sheet_hud_v05.md`
3) 用 Prompt Sheet 出图，保证：**同名 + 同尺寸 + PNG**（除 `bg_main.png / overlay_dim.png` 之外透明）
4) 用替换脚本覆盖到工程（校验尺寸、保留 `.meta`）：见 `Tools/UiRestyleV05/README.md`

## 2) 推荐的替换顺序（先可见、先高频）
1) **HUD 核心**：`*_square_*`、`hud_pill_dark*`、`hud_level_label_bg.png`、`tag_fast_*`、`badge_red_bg.png`、`digit_*.png`、`icon_*.png`
2) **玩法弹窗（最像参考图）**：`panel_modal.png`、`panel_thick_gold_blue.png`、`card_setting_row.png`、`toggle_*`、`btn_small_*`、`btn_close_red_*`
3) **商店与长尾**：`shop_*`、`lock_*`、`pill_*`、其它未覆盖 icon

## 3) 出图与导入约束（避免返工）
- 不改文件名、不改像素尺寸（否则会引发布局/9-slice/点击区问题）
- Normal/Pressed/Disabled 三态同一套光照与厚度
- 透明 PNG（除 `bg_main.png / overlay_dim.png`）

## 4) 替换步骤（建议按批次）
1) 用 `UI_ASSET_PROMPT_PACK_V05_CREAMY_PLASTIC.md` 生成一批 PNG（建议先 HUD 核心）
2) 放到输出目录（建议带 `UI_Sprites/` 子目录）
3) 运行替换脚本（会校验像素尺寸，保留 `.meta`）：见 `Tools/UiRestyleV05/README.md`
4) 打开 Unity / 运行游戏，按 `UI_STANDARDS.md` 的矩阵验收（safeArea/胶囊/最小点击区/文字可读性）

## 5) 验收要点（风格化）
- “像按钮”：所有可点元素必须有厚度（高光 + 内阴影 + 外投影）
- “统一光照”：高光方向一致；不要出现某些按钮高光在右上
- “奶油底”：面板/背景要更暖更软；彩色按钮像糖果贴片
- “HUD 可读”：数字不抖动、不挤压，FAST 文案随倍率变化
