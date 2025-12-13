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
