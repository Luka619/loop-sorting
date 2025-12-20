# Loop Sorting UI 资源出图 Prompt Pack（v0.5 / Creamy Plastic）

> 总索引：`../README.md`

> 目标：用“可复制的 Prompt 模板 + 文件映射表”，让你可以稳定批量生成整套 UI 资源，并 **直接替换** Unity 工程内的 PNG 文件。  
> 风格基准：`UI_STYLE_GUIDE_V05_CREAMY_PLASTIC.md`

---

## 0. 总规则（生成前必读）
1) **文件名必须完全一致**：生成的文件必须覆盖到同名 PNG（避免代码/Key 全部改一遍）。  
2) **像素尺寸必须完全一致**：不要改宽高；否则 9-slice、布局、点击区都会出问题。  
3) **透明背景**：除 `bg_main.png` 与 `overlay_dim.png` 外，其余全部输出透明 PNG。  
4) **三态成组**：带 `normal/pressed/disabled` 的资产必须一起生成，保证同光照同厚度。  
5) **光照统一**：永远是左上高光、右下投影；任何资产光照反了都算失败。

---

## 1. 通用 Prompt 片段（建议复制拼装）

### 1.1 `STYLE_CORE`（所有 UI 资产都带）
把它加在每个 prompt 的结尾：
> soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, gentle drop shadow inside the canvas padding, clean silhouette, fine grain, mobile game UI asset, orthographic front view

### 1.2 `NEGATIVE_CORE`（所有 UI 资产都带）
> photorealistic, realistic materials, wood texture, metal texture, fabric, excessive noise, harsh reflections, perspective skew, background scene, extra decorations, extra text, watermark, logo, blurry, low-res, artifacts

### 1.3 `EXPORT_RULES`（强约束）
> transparent background (unless specified), centered, no cropping, keep safe padding for shadow, crisp edges, sRGB, PNG

---

## 2. 模板（按资源类型拆）
> 使用方式：选一个模板 → 填入变量 → 拼上 `STYLE_CORE + NEGATIVE_CORE + EXPORT_RULES`。

### 2.1 背景（`bg_main.png`）
**模板：`BG_MAIN`**
- 变量：`MOOD`（轻快/柔和/清爽）、`ACCENTS`（薄荷/粉/橙 bokeh）
- Prompt：
  - warm creamy background gradient, subtle bokeh accents in mint and pink, soft vignette, clean and minimal, no text, no characters, `MOOD`
  - + `STYLE_CORE`
  - + `NEGATIVE_CORE`
  - + `EXPORT_RULES`（此项改为：non-transparent background）

### 2.2 全屏遮罩（`overlay_dim.png`）
**模板：`OVERLAY_DIM`**
- Prompt：
  - full-screen dim overlay for mobile UI, smooth dark gradient, subtle noise, no hard edges, no text
  - + `NEGATIVE_CORE`
  - + `EXPORT_RULES`（non-transparent background）

### 2.3 面板（`panel_modal.png / panel_result.png / panel_thick_gold_blue.png`）
**模板：`PANEL_BASE`**
- 变量：`FILL`（cream / blue）、`FRAME`（gold / white）、`THICKNESS`（thin/thick）
- Prompt：
  - UI panel background, rounded rectangle, `FILL` fill with soft inner gradient, `FRAME` thick frame, beveled edges, inner shadow, gentle highlight, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.4 方形按钮底（`*_square_*.png`）
**模板：`BTN_SQUARE`**
- 变量：`COLOR_ROLE`（Mint/Purple/Orange/Pink/Red）、`STATE`（Normal/Pressed/Disabled）
- Prompt：
  - square rounded button base, `COLOR_ROLE` candy plastic, thick outline using darker tone, top-left highlight, soft inner shadow, bottom shadow, `STATE` state styling (pressed = darker + shorter shadow, disabled = desaturated + reduced contrast), no icon, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.5 长条按钮底（`*_long_*.png`）
**模板：`BTN_LONG`**
- 变量：`COLOR_ROLE`、`STATE`
- Prompt：
  - long pill button base, `COLOR_ROLE` candy plastic, thick outline, top-left highlight, soft inner shadow, `STATE` state styling, no icon, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.6 小按钮（`btn_small_*.png`）
**模板：`BTN_SMALL`**
- 变量：`COLOR_ROLE`（Blue/Green/Orange/Red）、`STATE`
- Prompt：
  - small pill button base, `COLOR_ROLE` candy plastic, thick outline, clean highlight, `STATE` state styling, no text, no icon
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.7 价格按钮（`btn_price_green_*.png`）
**模板：`BTN_PRICE`**
- 变量：`STATE`
- Prompt：
  - price button base, green candy plastic pill, thick outline, subtle highlight, `STATE` state styling, no text, no icon
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.8 HUD 背板（`hud_pill_dark*.png / hud_level_label_bg.png`）
**模板：`HUD_PILL`**
- 变量：`FILL`（chocolate/dark navy/cream）、`SIZE_HINT`（small/tiny/regular）
- Prompt：
  - HUD pill background, rounded capsule, `FILL` plastic with subtle gradient, inner shadow, slight highlight, `SIZE_HINT`, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.9 标签（`tag_fast_* / tag_small_*`）
**模板：`TAG_PILL`**
- 变量：`MOOD`（info/danger/small）
- Prompt：
  - small tag pill background, `MOOD` color theme, thick outline, soft highlight, inner shadow, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.10 设置行卡片（`card_setting_row.png`）
**模板：`CARD_ROW`**
- Prompt：
  - settings row card background, rounded rectangle, creamy beige plastic, subtle inner shadow, soft highlight, thick soft edge, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.11 Toggle（`toggle_full_on/off.png / toggle_track_on/off.png / toggle_knob.png`）
**模板：`TOGGLE_TRACK`**
- 变量：`STATE`（on/off）
- Prompt：
  - toggle track, `STATE` state, candy plastic, inner shadow, rounded capsule, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

**模板：`TOGGLE_KNOB`**
- Prompt：
  - toggle knob, glossy plastic, rounded circle capsule, soft highlight, tiny shadow, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

**模板：`TOGGLE_FULL`**
- 变量：`STATE`（on/off）
- Prompt：
  - full toggle switch (track + knob), `STATE` state, green for on, gray for off, thick outline, inner shadow, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.12 图标（`icon_*.png`）
**模板：`ICON_GLYPH`**
- 变量：`SUBJECT`（gear/shop/coin/heart/music/video…）、`TINT`（white fill + brown outline）
- Prompt：
  - `SUBJECT` icon glyph, chunky rounded silhouette, `TINT`, thick outline, subtle inner shading, tiny shadow, centered, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

### 2.13 数字（`digit_0..9.png`）与角标（`badge_red_bg.png`）
**模板：`DIGIT`**
- 变量：`DIGIT_CHAR`（0-9）
- Prompt：
  - single digit `DIGIT_CHAR`, chunky rounded, white fill, dark outline, subtle inner shadow, centered, no extra
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

**模板：`BADGE_BG`**
- Prompt：
  - small circular badge background, red candy plastic, thick outline, bright highlight, no text
  - + `STYLE_CORE` + `NEGATIVE_CORE` + `EXPORT_RULES`

---

## 3. 文件映射表（UI_Sprites）
> 说明：下面每个文件都对应一个模板与变量。尺寸请以工程现有文件为准（不要改）。

### 3.1 背景 / 遮罩
- `bg_main.png` → `BG_MAIN`
- `overlay_dim.png` → `OVERLAY_DIM`

### 3.2 Panel
- `panel_modal.png` → `PANEL_BASE`（FILL=cream, FRAME=white, THICKNESS=thin）
- `panel_result.png` → `PANEL_BASE`（FILL=cream, FRAME=white, THICKNESS=thin）
- `panel_thick_gold_blue.png` → `PANEL_BASE`（FILL=blue or cream, FRAME=gold, THICKNESS=thick）

### 3.3 Buttons（Square）
- `mint_square_normal.png` → `BTN_SQUARE`（COLOR_ROLE=Mint, STATE=Normal）
- `mint_square_pressed.png` → `BTN_SQUARE`（COLOR_ROLE=Mint, STATE=Pressed）
- `mint_square_disabled.png` → `BTN_SQUARE`（COLOR_ROLE=Mint, STATE=Disabled）
- `purple_square_normal.png` → `BTN_SQUARE`（COLOR_ROLE=Purple, STATE=Normal）
- `purple_square_pressed.png` → `BTN_SQUARE`（COLOR_ROLE=Purple, STATE=Pressed）
- `purple_square_disabled.png` → `BTN_SQUARE`（COLOR_ROLE=Purple, STATE=Disabled）
- `orange_square_normal.png` → `BTN_SQUARE`（COLOR_ROLE=Orange, STATE=Normal）
- `orange_square_pressed.png` → `BTN_SQUARE`（COLOR_ROLE=Orange, STATE=Pressed）
- `orange_square_disabled.png` → `BTN_SQUARE`（COLOR_ROLE=Orange, STATE=Disabled）
- `pink_square_normal.png` → `BTN_SQUARE`（COLOR_ROLE=Pink, STATE=Normal）
- `pink_square_pressed.png` → `BTN_SQUARE`（COLOR_ROLE=Pink, STATE=Pressed）
- `pink_square_disabled.png` → `BTN_SQUARE`（COLOR_ROLE=Pink, STATE=Disabled）

### 3.4 Buttons（Long）
- `mint_long_normal.png` → `BTN_LONG`（COLOR_ROLE=Mint, STATE=Normal）
- `mint_long_pressed.png` → `BTN_LONG`（COLOR_ROLE=Mint, STATE=Pressed）
- `mint_long_disabled.png` → `BTN_LONG`（COLOR_ROLE=Mint, STATE=Disabled）
- `purple_long_normal.png` → `BTN_LONG`（COLOR_ROLE=Purple, STATE=Normal）
- `purple_long_pressed.png` → `BTN_LONG`（COLOR_ROLE=Purple, STATE=Pressed）
- `purple_long_disabled.png` → `BTN_LONG`（COLOR_ROLE=Purple, STATE=Disabled）
- `orange_long_normal.png` → `BTN_LONG`（COLOR_ROLE=Orange, STATE=Normal）
- `orange_long_pressed.png` → `BTN_LONG`（COLOR_ROLE=Orange, STATE=Pressed）
- `orange_long_disabled.png` → `BTN_LONG`（COLOR_ROLE=Orange, STATE=Disabled）
- `pink_long_normal.png` → `BTN_LONG`（COLOR_ROLE=Pink, STATE=Normal）
- `pink_long_pressed.png` → `BTN_LONG`（COLOR_ROLE=Pink, STATE=Pressed）
- `pink_long_disabled.png` → `BTN_LONG`（COLOR_ROLE=Pink, STATE=Disabled）

### 3.5 Buttons（Small / Price / Close）
- `btn_small_blue_normal.png` → `BTN_SMALL`（COLOR_ROLE=Blue, STATE=Normal）
- `btn_small_blue_pressed.png` → `BTN_SMALL`（COLOR_ROLE=Blue, STATE=Pressed）
- `btn_small_blue_disabled.png` → `BTN_SMALL`（COLOR_ROLE=Blue, STATE=Disabled）
- `btn_small_green_normal.png` → `BTN_SMALL`（COLOR_ROLE=Green, STATE=Normal）
- `btn_small_green_pressed.png` → `BTN_SMALL`（COLOR_ROLE=Green, STATE=Pressed）
- `btn_small_green_disabled.png` → `BTN_SMALL`（COLOR_ROLE=Green, STATE=Disabled）
- `btn_small_orange_normal.png` → `BTN_SMALL`（COLOR_ROLE=Orange, STATE=Normal）
- `btn_small_orange_pressed.png` → `BTN_SMALL`（COLOR_ROLE=Orange, STATE=Pressed）
- `btn_small_orange_disabled.png` → `BTN_SMALL`（COLOR_ROLE=Orange, STATE=Disabled）
- `btn_small_red_normal.png` → `BTN_SMALL`（COLOR_ROLE=Red, STATE=Normal）
- `btn_small_red_pressed.png` → `BTN_SMALL`（COLOR_ROLE=Red, STATE=Pressed）
- `btn_small_red_disabled.png` → `BTN_SMALL`（COLOR_ROLE=Red, STATE=Disabled）

- `btn_price_green_normal.png` → `BTN_PRICE`（STATE=Normal）
- `btn_price_green_pressed.png` → `BTN_PRICE`（STATE=Pressed）
- `btn_price_green_disabled.png` → `BTN_PRICE`（STATE=Disabled）

- `btn_close_red_normal.png` → `BTN_SQUARE`（COLOR_ROLE=Orange or Red, STATE=Normal；注意：内含“X”可做成单独 `ICON_GLYPH` 叠加，或直接在按钮里烘焙）
- `btn_close_red_pressed.png` → 同上（Pressed）

### 3.6 HUD / Pill / Tag
- `hud_pill_dark.png` → `HUD_PILL`（FILL=chocolate or dark navy, SIZE_HINT=regular）
- `hud_pill_dark_small.png` → `HUD_PILL`（FILL=chocolate or dark navy, SIZE_HINT=small）
- `hud_pill_dark_tiny.png` → `HUD_PILL`（FILL=chocolate or dark navy, SIZE_HINT=tiny）
- `hud_level_label_bg.png` → `HUD_PILL`（FILL=cream, SIZE_HINT=label）

- `tag_fast_info_bg.png` → `TAG_PILL`（MOOD=info）
- `tag_fast_danger_bg.png` → `TAG_PILL`（MOOD=danger）
- `tag_small_info_bg.png` → `TAG_PILL`（MOOD=small info）

- `pill_bg.png` / `pill_bg_pressed.png` / `pill_bg_disabled.png` → `BTN_LONG`（COLOR_ROLE=Cream, STATE=Normal/Pressed/Disabled）
- `pill_timer_beige.png` → `BTN_LONG`（COLOR_ROLE=Cream, STATE=Normal）

### 3.7 Card / Shop / LockNode
- `card_setting_row.png` → `CARD_ROW`
- `shop_card_beige.png` / `shop_card_purple.png` / `shop_card_yellow.png` → `PANEL_BASE`（FILL=cream/purple/yellow, FRAME=soft, THICKNESS=thin）
- `shop_row_beige.png` / `shop_row_purple.png` / `shop_row_yellow.png` → `PANEL_BASE`（同上，偏“条形”）
- `shop_group_bar.png` → `TAG_PILL`（MOOD=dark info）
- `shop_scroll_fade_top.png` / `shop_scroll_fade_bottom.png` → `OVERLAY_DIM`（但更浅、更短、方向渐变）
- `shop_topbar_scallop_tile_512x128.png` → scallop decorative tile（奶油/浅阴影，tileable）

- `lock_chip_plate.png` / `lock_node_base.png` / `lock_node_label_bg.png` → `PANEL_BASE`（FILL=cream/stone, FRAME=soft, THICKNESS=thin）
- `lock_node_lock.png` → `ICON_GLYPH`（SUBJECT=lock）

### 3.8 Icons（按同一模板批量出）
> 全部走 `ICON_GLYPH`，只换 SUBJECT：  
`icon_gear/icon_shop/icon_coin/icon_coin_128/icon_heart/icon_heart_128/icon_plus/icon_music/icon_vibrate/icon_pause/icon_video/icon_retry/icon_next/icon_lock/icon_loop/icon_close/icon_clock*` 等。

### 3.9 Digits / Badge
- `digit_0.png`..`digit_9.png` → `DIGIT`
- `badge_red_bg.png` → `BADGE_BG`

---

## 4. 资源替换（执行方式）
1) 把生成的 PNG 放到一个输出目录（建议结构与 Unity 一致：包含 `UI_Sprites/` 子目录）。
2) 用脚本校验尺寸并覆盖拷贝（保留 `.meta` 不动）：见 `../Tools/UiRestyleV05/README.md`
