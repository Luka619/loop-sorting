# Loop Sorting UI Kit（v0.3 Bright + Candy）

你选择了“1+2”：
1. **背景更亮**（更轻快、降低蓝色压迫感：加入薄荷/粉/橙的柔光 bokeh）
2. **按钮更糖果感**（更亮的高光、更软的阴影、颗粒+闪点的材质细节）

本包继续保持“组件化可复用 + Blueprint + 文档驱动”，便于你让程序 AI 直接落地 Unity UI。

---

## 配色与分工
- **Mint（薄荷绿）**：通用/主操作（HUD、Fill、Settings、结果按钮）
- **Purple（紫）**：Shuffle
- **Orange（橙）**：主界面 PLAY、需要强调的 CTA
- **Pink（粉）**：预留扩展色（可用于新道具/商店等）

---

## Locked Box 视觉（锁 + unlockColor）
你提出“锁元素 + 颜色本身”来表达锁箱解锁颜色，本包提供：
- World：`World_Sprites/lock_overlay.png` + `lock_marker_*` 三件套（plate/color/lock）
- UI：`UI_Sprites/lock_chip_plate.png` + `icon_lock.png` + colorDisc（tint）

Blueprint 里已定义：
- `LockedBoxVisual`（世界 SpriteRenderer 组合）
- `LockedBoxChip_UI`（UI 组合，适合教程/提示/关卡信息）

用法要点：
- `MarkerColor` / `ColorDisc` 的 tint 由运行时 `unlockColor` 决定。

---

## 目录结构
- `UI_Sprites/`
- `World_Sprites/`
- `Blocks/`
- `Layout/ui_blueprint.json`
- `Preview/`

---

## 你可以直接复制给程序 AI 的实现指令

> 读取 `Layout/ui_blueprint.json`，在 Unity 中按 prefab 定义创建 UI 组件（Image/Button/TMP_Text）。  
> 对 Button 绑定 SpriteState（normal/pressed/disabled）。  
> 实现 UIStateController：在 BoxShipping / FastForward / Booster / Modal 时锁定 HUD（Button.interactable=false 并切 disabled sprite）；显示 FAST tag（info/danger）。  
> 锁箱：当 BoxSpec.locked=true 时启用 `LockedBoxVisual`，并将其 `MarkerColor` tint 设置为 unlockColor；解锁后禁用。  
> 教程/提示：如需显示“锁箱需要什么颜色”，实例化 `LockedBoxChip_UI`，同样 tint 颜色圆片为 unlockColor。

---

## 九宫格建议（9-slice）
- `*_square_*`：170
- `*_long_*`：140,140,90,90
- `pill_bg*`：90,90,60,60
- `panel_*`：120
- `tag_fast_*`：60,60,40,40
- `lock_chip_plate.png`：60,60,40,40

---

# Meta / HUD / Shop / Settings 扩展（v0.4）

本章节对应你补充的“完整度”需求：顶栏 HUD、Settings 弹窗、Shop 页面、More Lives 弹窗、底部关卡锁节点。

> 说明：所有资源均为 **可复用组件化** 设计，推荐用 `Layout/ui_blueprint.json` 让程序 AI 自动搭建层级。

---

## 顶部 HUD（HUD_TopBar）
### 资源
- `icon_pause.png`：暂停（||）
- `icon_shop.png`：商店（小店铺）
- `hud_level_label_bg.png`：Level 文本底板（深色 pill，9-slice）
- `hud_pill_dark.png / hud_pill_dark_small.png`：货币/生命数值底板（9-slice）
- `icon_plus.png`：“+”符号
- `icon_coin.png / icon_coin_128.png`：金币
- `icon_heart.png / icon_heart_128.png`：爱心

### 文字描边方案（推荐 TMP）
- Outline：深色（接近 `#0B1730`），Width 0.18~0.25
- Underlay（阴影）：Offset (2,-3)，Softness 0.3，Dilate 0.05
- 统一用白字，保证在深色 pill 上清晰

---

## Settings 弹窗（Modal_Settings_Full）
### 面板与关闭按钮
- `panel_thick_gold_blue.png`：厚黄框 + 内阴影蓝面板（9-slice）
- `btn_close_red_normal.png / btn_close_red_pressed.png`：右上角红色关闭按钮

### 设置行（Audio / Vibration）
- `card_setting_row.png`：浅色卡片底（9-slice）
- `icon_music.png`：音符
- `icon_vibrate.png`：震动
- `toggle_full_on.png / toggle_full_off.png`：ON/OFF 两态 toggle 底图

### 底部功能按钮
- `btn_small_blue_*`：Restore Purchases
- `btn_small_green_*`：Support
- `btn_small_red_*`：Retry Level

---

## Shop 页面（Screen_Shop）
### 顶部页签/标题条
- `shop_topbar_scallop_tile_512x128.png`：波浪/扇贝边缘（建议 Image=Tiled）
- 关闭按钮：复用 `btn_close_red_*`

### 商品卡片模板
- `shop_card_purple.png`：紫色大条（去广告/礼包）
- `shop_card_beige.png`：米色大条（通用）
- `shop_card_yellow.png`：金币包黄条（金币商品）

### 分组标题条
- `shop_group_bar.png`：深色圆角条（9-slice），用于 “NO ADS / BUNDLES / COINS” 等

### 价格按钮皮肤
- `btn_price_green_*`：绿色价格按钮（normal/pressed/disabled）

### 商品图标（可选占位）
- `icon_no_ads_tv.png`：去广告电视
- `icon_coin_stack.png`：金币堆
- `icon_coin_bag.png`：金币袋
- `icon_coin_chest.png`：金币箱
- `icon_coin_safe.png`：金币保险箱

---

## More Lives 弹窗（Modal_MoreLives）
- 复用面板：`panel_thick_gold_blue.png`
- `heart_big.png`：大心心图
- `pill_timer_beige.png`：倒计时/FULL 条（9-slice）
- CTA：
  - 看广告：`btn_small_orange_*` + `icon_video.png`
  - 花金币：`btn_small_green_*` + `icon_coin_128.png`

---

## 关卡锁定节点（LockNode）
- `lock_node_base.png`：灰色圆形立体底座
- `lock_node_lock.png`：锁图标
- `lock_node_label_bg.png`：Lvl x 文本底板（9-slice）
- 文本：建议 TMP 描边同 HUD 方案

---

## 9-slice 推荐
- `panel_thick_gold_blue.png`：border 140
- `card_setting_row.png`：border 70
- `hud_pill_dark*.png`：border 40
- `shop_card_*.png`：border 90
- `shop_group_bar.png`：border 48
- `btn_small_*`：border 80
- `btn_price_green_*`：border 60

---

# Shop 滚动列表（ScrollRect）实现规范（v0.4.2）

你已确认 Shop 需要滚动列表。本包提供可直接复用的组件化层级：

## 组件
- `ShopScrollList`：ScrollRect（Vertical）
  - `Viewport`：RectMask2D（裁切）
  - `Content`：VerticalLayoutGroup + ContentSizeFitter（PreferredSize）
  - 可选 `FadeTop/FadeBottom`：用 `shop_scroll_fade_top/bottom.png` 做滚动提示（可删）

- `ShopSectionHeader`：分组标题条（NO ADS / BUNDLES / COINS）
- `ShopItemCard`：通用商品卡（紫/米色大条）
- `ShopCoinPackRow`：金币包行（黄色短条）

## Unity 关键参数建议（供程序实现）
### ScrollRect
- Vertical = true，Horizontal = false
- MovementType = Elastic
- Inertia = true，DecelerationRate = 0.135
- ScrollSensitivity = 25
- 不使用 Scrollbar（更符合超休闲）

### Content（VerticalLayoutGroup）
- Spacing = 28
- Padding：Top 24 / Bottom 60
- Child Alignment：UpperCenter
- Child Control Width = true
- Child Force Expand Width = true
- Child Control Height = false（高度由 LayoutElement 控制）

### Item 高度（LayoutElement）
- `ShopSectionHeader`：PreferredHeight = 96
- `ShopItemCard`：PreferredHeight = 260
- `ShopCoinPackRow`：PreferredHeight = 200

## 资源
- 行模板：
  - `shop_row_yellow.png`（金币包）
  - `shop_row_beige.png` / `shop_row_purple.png`（备用）
- 滚动提示：
  - `shop_scroll_fade_top.png`
  - `shop_scroll_fade_bottom.png`

## 使用方式
- 直接使用 prefab：`Screen_Shop`（已内置 CurrencyRow + ScrollList）
- 或者只取 `ShopScrollList` 并按你的商品数据动态生成 Content 子项：
  1) 插入 `ShopSectionHeader`（设置 Text）
  2) 插入 `ShopItemCard` 或 `ShopCoinPackRow`（设置 BG / Icon / Title / Desc / Price）

---

# 完成箱体（满且同色）视觉标志 + 动效特效

设计文档要求：**完成箱体** 在逻辑上“变为不可操作，并有完成覆盖效果”。（见 `DESIGN.md`）  
因此这里提供 **可复用、可 tint** 的“完成覆盖”资源与一套轻量动效贴图。

## 新增资源（World_Sprites/）
### 完成标志（静态）
- `box_completed_badge_check_256.png` / `box_completed_badge_check_512.png`  
  用途：完成后在箱体角落贴一个“√”徽章（建议右上角）。

### 完成覆盖（可 tint）
- `box_completed_frame_glow_512.png` / `box_completed_frame_glow_1024.png`  
  用途：完成后在箱体上叠一圈发光描边（建议按箱体颜色 tint）。
- `box_completed_glass_overlay_512.png` / `box_completed_glass_overlay_1024.png`  
  用途：完成后在箱体表面叠一层轻微玻璃感（建议低 alpha，或 tint 为浅色）。

### 完成动效（一次性播放）
- `vfx_complete_burst_sheet_8f_512x256.png`（8 帧，4x2，单帧 128x128）
- `vfx_complete_burst_sheet_8f_1024x512.png`（2x）
- `vfx_sparkle_star_128.png` / `vfx_sparkle_star_256.png`（粒子/闪光贴图）

## 推荐层级（World 侧，SpriteRenderer）
当 `BoxState` 变为 Completed 时：
- `BoxRoot`
  - `Blocks`（已有）
  - `RunOutline`（已有）
  - `BoxOutline`（已有：虚线轮廓）
  - `CompletedOverlay`
    - `FrameGlow`：Sprite = `box_completed_frame_glow_512`，颜色 tint = boxColor，Alpha 0.85
    - `Glass`：Sprite = `box_completed_glass_overlay_512`，颜色 tint = 白或 boxColor*0.35，Alpha 0.45
    - `Badge`：Sprite = `box_completed_badge_check_256`（位置：右上角偏外）
  - `CompletedFX`（一次性）
    - `BurstAnim`：用 `vfx_complete_burst_sheet_8f_*` 播放 8 帧（0.30~0.45s），最后销毁
    - `Sparkles`：ParticleSystem（Texture= `vfx_sparkle_star_*`，Lifetime 0.4~0.8s，StartSpeed 0.6~1.2）

## 动效建议（不依赖复杂系统，程序易实现）
- Badge：Scale 0 → 1.08 → 1.0（0.22s，OutBack）
- FrameGlow：Alpha 0 → 1（0.18s）然后保持；或轻微 pulse（sin 0.85~1.0）
- Burst：一次性播放后销毁
- 可选：若完成颜色触发锁箱解锁，可复用同一 burst 颜色（unlockColor tint），加强反馈。

---

# 礼花（Firework Confetti）完成特效（高质量）

当箱体进入 Completed（满且同色）状态时，你可以播放一个一次性的“礼花”特效来强化反馈。

## 新增资源（World_Sprites/）
### 礼花 Burst 动画（Sprite Sheet）
- `vfx_firework_confetti_burst_sheet_16f_1024x1024.png`  
  - 16 帧，4x4 网格，单帧 256x256
- `vfx_firework_confetti_burst_sheet_16f_2048x2048.png`（2x）  
  - 单帧 512x512（用于高分辨率或近景放大）

### Confetti 粒子贴图（可 tint，建议用随机颜色）
- `vfx_confetti_rect_128.png / _256.png`
- `vfx_confetti_tri_128.png / _256.png`
- `vfx_confetti_stream_128.png / _256.png`
- `vfx_confetti_star_128.png / _256.png`

## 推荐实现（Unity）
### A) Burst（SpriteRenderer + 帧动画 或 ParticleSystem 的 Texture Sheet Animation）
**方案 1：SpriteRenderer 帧动画（最简单）**
- 导入 Burst sheet：Sprite Mode = Multiple，Grid 4x4 slicing
- 播放 16 帧：总时长 0.35~0.55s
- Material：Default-Sprite 或 Additive（更“礼花”）
- 播放完销毁对象

**方案 2：ParticleSystem（Texture Sheet Animation）**
- Texture Sheet Animation：Tiles X=4, Y=4
- Frame over Time：0 → 1
- Cycles：1
- Start Size：根据箱体尺寸（建议 0.8~1.2）
- Start Lifetime：0.45s（或与你的帧时间匹配）

### B) Confetti（ParticleSystem）
- Emission：Burst 1 次（Count 18~36）
- Start Lifetime：0.8~1.4
- Start Speed：4.5~8.0
- Gravity Modifier：0.9~1.3（让碎片下落更真实）
- Start Size：0.10~0.22（按画面尺度调）
- Start Rotation：0~360，Rotation over Lifetime：随机（建议）
- Start Color：Random Between Two Colors（或用 Gradient/多色）
  - 颜色建议复用积木颜色（更统一）
- Texture：从 `vfx_confetti_*` 中随机选（可用多个 renderer/多个系统，或用 Sub-Emitters）

## 与完成覆盖的组合建议
- 先出现：FrameGlow Alpha 0→1（0.18s）
- 同时触发：Burst（0.45s）+ Confetti（1.2s 内落地消失）
- Badge：Scale 弹出（0.22s）

> 这些参数在超休闲节奏中反馈非常明确，且实现成本低。

