# Functional Split TODO (v0.5)

目标：按“功能结构契约”拆分 UI 资源，把“资源生成”和“代码/Prefab 改动”解耦；先把资源拆好并列出需要改的功能点，你再逐项改实现。

默认规则（除非你特别说明某资源不用拆）：
- 可交互组件（button/toggle/tab/slider）默认拆成：`base`（可 9-slice）+ `icon_glyph`（可选）+ `text`（TMP/Text）+ `decor`（shadow/outline/highlight，按需）。
- 列表/卡片/面板默认拆成：`panel_base`（9-slice）+ `panel_decor`（边缘/高光/阴影，按需）+ 内容层（icon/text 分离）。
- 透明 PNG：画布外必须 100% 透明；不要把 vignette/雾化背景/大面积渐变“画进透明图里”（那会影响叠加与裁切判断）。
- 关于尺寸：**生成图不需要“真实尺寸”对齐**，对齐/裁切会截断透明资产；只要留足 padding，保证完整轮廓和阴影都在画布内即可。

---

## 1) Toggle（设置页）

### 现状
- 当前实现使用整张状态图：`Assets/Resources/setting_page_assets/toggle_on*.png / toggle_off*.png`
- 代码入口：`Assets/Scripts/GameRuntimeController.cs:1246` `ApplySettingsToggleSprites(...)`
- Manifest：`Assets/Resources/setting_page_assets/assets_manifest.json`（实例尺寸约 221x132）

### 目标资源（拆分）
默认拆分为：
- Track：`UI_Sprites/toggle_track_on.png`、`UI_Sprites/toggle_track_off.png`
- Knob：`UI_Sprites/toggle_knob.png`
-（可选）Shadow：`UI_Sprites/toggle_shadow.png`
-（可选）Outline：`UI_Sprites/toggle_outline.png`

运行时组合顺序建议：`shadow (optional)` → `track` → `knob` → `outline (optional)`

### 需要改的功能点（你改代码/Prefab）
- Prefab 结构：把“一个 Image”改为“多层 Image”
  - Track Image（底层）
  - Knob Image（上层，可移动）
  -（可选）Shadow/Outline Image
- Toggle 状态逻辑：
  - `isOn` 切换时：替换 track sprite（on/off），并把 knob 移到左/右端
  - `pressed` 反馈：建议用 `Button` 的 `ColorTint`/缩放，而不是再做一套 pressed 贴图（除非你明确需要 pressed 贴图）
- 资源加载来源（二选一）：
  - 方案 A（推荐）：使用 `LoopSortingUIKit` 的 sprite key
    - 现有 key 已在 `Assets/Resources/LoopSortingUIKitConfig.json`：`ui.toggle.track_on`、`ui.toggle.track_off`、`ui.toggle.knob`
  - 方案 B：继续走 `setting_page_assets/assets_manifest.json`
    - 需要扩展 manifest：为每个 toggle 实例提供 `track_sprite` / `knob_sprite`（以及 knob 左右偏移）
    - 并更新 `TryLoadSettingsPageSprite(...)` 的调用路径

---

## 2) Button（通用：底图 + 图标/文字）

### 目标资源拆分规则（默认）
- `button_base_*`：按钮底图（normal/pressed/disabled）
- `icon_*`：按钮图标（与底图分离）
- 文案：统一走 TMP/Text（不画进按钮底图）

### 需要改的功能点（你改代码/Prefab）
- Prefab：按钮从“单 Image”改为（Base Image + Icon Image + Text）
- 状态：pressed/disabled 只影响 base（以及必要时 icon tint），避免把文字/图标烘焙进底图导致不可复用

### 按现有 Screen 的拆分清单（来自 `_ui_screen_usage_report.md`）
- Screen_MainMenu
  - `orange_long_{normal,pressed,disabled}.png` → `button_base_long_orange_{normal,pressed,disabled}.png`（文字 PLAY/LEVEL 用 TMP）
  - `mint_square_{normal,pressed,disabled}.png` → `button_base_square_mint_{normal,pressed,disabled}.png`（图标单独 `icon_*`）
- Screen_GameplayHUD
  - `mint_square_{normal,pressed,disabled}.png` → `button_base_square_mint_{normal,pressed,disabled}.png`（Booster：FILL/SHUFFLE 等）
  - `purple_square_{normal,pressed,disabled}.png` → `button_base_square_purple_{normal,pressed,disabled}.png`（速度按钮 1x/2x/… 用 TMP）
- Screen_Shop
  - `btn_price_green_{normal,pressed,disabled}.png` → `button_base_price_green_{normal,pressed,disabled}.png`（价格文本用 TMP）
  - `btn_close_red_{normal,pressed}.png` → `button_base_square_red_{normal,pressed}.png` + `icon_close_x.png`
- Modal_MoreLives
  - `btn_small_{orange,green}_{normal,pressed,disabled}.png` → `button_base_small_{orange,green}_{normal,pressed,disabled}.png`（Get +1 / Refill 等用 TMP；video/coin 图标独立）
  - `btn_close_red_{normal,pressed}.png` 同上
- Modal_Settings_Full
  - `btn_small_{orange,green,blue,red}_{normal,pressed,disabled}.png` → `button_base_small_{color}_{normal,pressed,disabled}.png`（Restore Purchases/Retry/Support 用 TMP；必要图标独立）
  - `btn_close_red_{normal,pressed}.png` 同上
- Modal_Result
  - `mint_long_{normal,pressed,disabled}.png` → `button_base_long_mint_{normal,pressed,disabled}.png`（NEXT 用 TMP）

---

## 3) Panel / Modal（9-slice 友好）

### 目标资源拆分规则（默认）
- `panel_*_base_9slice.png`：9-slice 面板底（保持可拉伸）
- `panel_*_decor.png`：装饰边/高光/阴影（独立层）

### 需要改的功能点（你改代码/Prefab）
- Prefab：9-slice 只挂在 base 上；decor 用独立 Image（不参与拉伸，或按需拉伸）

### 按现有 Screen 的拆分清单
- Modal_Settings
  - `panel_modal.png`（916x794）→
    - `panel_modal_base_9slice.png`（916x794）
    - `panel_modal_decor.png`（916x794）
- Modal_MoreLives / Modal_Settings_Full
  - `panel_thick_gold_blue.png`（960x1140）→
    - `panel_gold_blue_base_9slice.png`（960x1140）
    - `panel_gold_blue_decor.png`（960x1140）
- Modal_Result
  - `panel_result.png`（956x794）→
    - `panel_result_base_9slice.png`（956x794）
    - `panel_result_decor.png`（956x794）
- Screen_Shop
  - `shop_card_beige.png` / `shop_row_yellow.png` 建议按“base 9-slice + decor”拆（否则行高变化/适配时容易拉伸变形）

---

## 4) Shop 列表组件（行/卡片/分组）

### 目标资源拆分规则（默认）
- Row/Card：`*_base_9slice` + `*_decor` + 内容（icon/text 分离）
- Section bar：`shop_group_bar_base` + `shop_group_bar_decor`
- Scroll fade：保持单图（`shop_scroll_fade_top/bottom.png`）即可
- Topbar scallop：保持 tile 单图（`shop_topbar_scallop_tile_512x128.png`）即可

### 拆分资源清单（按现有文件名落地）
- Row
  - `shop_row_yellow.png`（1044x258）→
    - `shop_row_yellow_base_9slice.png`（1044x258）
    - `shop_row_yellow_decor.png`（1044x258）
- Card
  - `shop_card_beige.png`（1048x324）→
    - `shop_card_beige_base_9slice.png`（1048x324）
    - `shop_card_beige_decor.png`（1048x324）
- Group bar
  - `shop_group_bar.png`（752x138）→
    - `shop_group_bar_base.png`（752x138）
    - `shop_group_bar_decor.png`（752x138）

### 需要改的功能点（你改代码/Prefab）
- Prefab：ShopRow/ShopCard 从“单张背景”改为（Base 9-slice + Decor + 内容层）
- 布局：价格/数量/标题等文本必须独立可布局（不要画进背景）

---

## 5) 设置页（ResourcesRoot/setting_page_assets）

### 现状
- 当前使用整张页底：`Assets/Resources/setting_page.png`
- 叠加资源：`Assets/Resources/setting_page_assets/*.png`（toggle / close / retry 等）

### 拆分资源（默认）
- Toggle：按第 1 节（track/knob/outline/shadow），不再使用 `setting_page_assets/toggle_*.png`
- Close：
  - `setting_page_assets/btn_close_base.png`
  - `setting_page_assets/btn_close_base_pressed.png`
  - 图标层复用：`UI_Sprites/icon_close.png`（运行时缩放到按钮内）
- Retry：
  - `setting_page_assets/btn_retry_base_normal.png`
  - `setting_page_assets/btn_retry_base_pressed.png`
  - 文本 “Retry” 走 TMP；如确实需要图标，再单独 `UI_Sprites/icon_retry_arrow.png`

### 需要改的功能点（你改代码/Prefab）
- `Assets/Scripts/GameRuntimeController.cs`
  - `ApplySettingsToggleSprites(...)`：从“整张 sprite swap”改为“track/knob 组合 + knob 位置移动 + pressed 用 tint/scale”
  - Close/Retry：如果当前是单图按钮，改为 Base + Icon + Text 的组合（或至少 Base + TMP）
- `Assets/Resources/setting_page_assets/assets_manifest.json`
  - 若继续使用 manifest：需要新增拆分后的 sprite key 与实例布局信息
  - 或直接切到 `LoopSortingUIKit` 的 UI_Sprites key（推荐）

---

## 6) HUD 计数条 / 胶囊底（coin/heart/time）

### 目标资源拆分规则（默认）
- 胶囊底：`hud_pill_*_base_9slice.png` +（可选）`hud_pill_*_decor.png`
- 内容（图标/数字/加号）全部独立（TMP + Icon）

### 现有资源映射（来自 `_ui_screen_usage_report.md`）
- `hud_pill_dark_small.png`（352x126）→ `hud_pill_dark_small_base_9slice.png`（352x126）+（可选）`hud_pill_dark_small_decor.png`
- `pill_bg.png`（392x162）→ `hud_pill_light_base_9slice.png`（392x162）+（可选）`hud_pill_light_decor.png`
- `pill_timer_beige.png`（464x178）→ `hud_pill_timer_beige_base_9slice.png`（464x178）+（可选）`hud_pill_timer_beige_decor.png`

### 需要改的功能点（你改代码/Prefab）
- HUD/Timer 的数字一律用 TMP；不要把数字烘焙进背景

---

## 7) Tag / Badge（提示条、角标、数量徽章）

### 目标资源拆分规则（默认）
- 背景底：`tag_*_bg.png` / `badge_*_bg.png`（无文字）
- 文案/数字：TMP/Text（例如 FAST x5、LEVEL 2、角标数量等）
-（可选）图标：独立 `icon_*`

### 现有资源（来自 `_ui_screen_usage_report.md`）
- `tag_fast_info_bg.png`（362x120）→ 保持为纯背景（无文字）
- `tag_small_info_bg.png`（292x112）→ 保持为纯背景（无文字）
- `badge_red_bg.png`（152x162）→ 保持为纯背景（无数字）

### 需要改的功能点（你改代码/Prefab）
- Badge 的 “1/0/…” 一律用 TMP；Badge 背景只负责形状与质感

---

## 8) Icon（通用图标资产）

### 规则
- 图标必须“自解释”（看形状就能认出来），不要只写“这是关闭按钮”
- 单独透明 PNG（无底板），运行时由按钮/面板提供底图
- 默认只做一套（颜色用 tint 解决）；除非某图标必须保持材质色（例如金币）

### 现有清单（来自 `_ui_screen_usage_report.md`）
- Gameplay：`icon_fill.png`、`icon_loop.png`、`icon_shuffle.png`
- Shop：`icon_coin_128.png`、`icon_coin_stack.png`、`icon_heart_128.png`
- MoreLives：`icon_clock.png`、`icon_video.png`
- Settings_Full：`icon_music.png`、`icon_vibrate.png`
- 通用：`icon_close_x.png` / `icon_close.png`（用于关闭按钮；和底板分离）

---

## 9) Digit（数字贴图，仅在确实不用 TMP 时）

### 规则
- 优先用 TMP 字体；只有“必须用 sprite 数字”（例如特殊数字风格）才做 digit_0-9

### 现有线索（来自 `_ui_screen_usage_report.md`）
- `digit_3.png`（64x88）说明至少有一处在用 sprite digit

### 待补齐（如果继续走 sprite digit）
- `digit_{0..9}.png`（统一尺寸与 baseline）

---

## 10) 全屏类（背景/遮罩/叠层）

### 规则
- `overlay_dim.png` 这类“遮罩”允许是半透明黑（不是“全透明 PNG”那类资产），但不要做花哨纹理，避免压 UI 质感
- `bg_main.png` 属于独立大背景图，不拆分

### 现有清单（来自 `_ui_screen_usage_report.md`）
- `bg_main.png`（1080x1920）
- `overlay_dim.png`（1080x1920）
