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
