# Loop Sorting UI Kit（Bright Variant）v0.2

本包是 **组件化 UI 资源**（非抠图），整体风格对齐 UI Review v0.2 的“圆角高光 + 软阴影 + 轻量描边”体系，同时将配色调整为 **更明亮、轻快**（减少蓝色占比，增加薄荷绿/紫/橙的色彩分工）。

并且补齐 **Locked Box 视觉组件**：用“锁元素 + unlockColor 颜色圆片”表达锁箱被什么颜色解锁。

---

## 为什么要补 Locked Box 视觉组件？
设计文档要求锁箱在解锁前不可见内容、不可操作，并且需要“灰色遮罩覆盖箱体 + 中间标记解锁所需颜色”。fileciteturn3file1L6-L10  
同时视觉层级要求：锁箱遮罩高于积木，颜色标记高于遮罩。fileciteturn3file0L6-L14  

本包用更清晰的“锁图标 + 颜色圆片”实现该标记。

---

## 目录结构
- `UI_Sprites/`：Canvas UI PNG（按钮、面板、标签、badge、数字、icon、toggle、背景）
- `World_Sprites/`：世界空间 PNG（传送带槽位、箱体虚线、锁遮罩、锁标记、完成覆盖、run 描边）
- `Blocks/`：积木（多色 + hidden）
- `Layout/ui_blueprint.json`：可复用 UI 组件层级（给程序 AI / 自动脚本生成 Unity UI）
- `Docs/UI_GUIDE.md`：本说明（给程序 AI 直接读）

---

## 配色策略（更轻快）
- **主按钮 / 通用按钮：Mint（薄荷绿）**：`mint_*`
- **Shuffle：Purple（紫）**：`purple_*`
- **主界面 Play：Orange（橙）**：`orange_*`
- FAST 标签：info 用薄荷绿系，danger 用红系（保持警示一致性）

这样能把 UI 的信息层级“颜色化”，减少全蓝导致的疲劳感。

---

## UI_Sprites：资源与用法

### 1) Button Variants（都提供 normal/pressed/disabled）
- `mint_square_*`：通用 HUD / Fill / Settings
- `purple_square_*`：Shuffle
- `orange_long_*`：主菜单 PLAY
- `mint_long_*`：结果弹窗 NEXT/RETRY（也可按需换成 orange）

> Disabled 视觉内置“变暗 + 底部遮罩”，程序只需切换 sprite + 关闭 interactable。

### 2) FreeSlots Counter
- `pill_bg.png` + `icon_loop.png` + TMP_Text 数字（推荐 TMP；如要 sprite 数字可用 digit_0~9）

### 3) Badge（按你的要求拆分）
- `badge_red_bg.png`：红圆底（独立）
- `digit_0~9.png`：数字（独立）

### 4) Tags（背景与文字分离）
- `tag_fast_info_bg.png`：FAST x5（info）
- `tag_fast_danger_bg.png`：FULL • FAST x5（danger）
- `tag_small_info_bg.png`：APPLYING 等小标签

标签文字请用 TMP_Text（可本地化、清晰）。

### 5) Modals
- `overlay_dim.png`：dim 拦截层
- `panel_modal.png`：Settings panel
- `panel_result.png`：Win/Lose panel
- Toggle：`toggle_track_on/off.png` + `toggle_knob.png`

---

## World_Sprites：锁箱（Locked Box）与其它世界表现

### Locked Box（新增）
本包提供一个可复用的锁箱标记组件，推荐按层叠放置：

- 遮罩：`lock_overlay.png`（覆盖箱体）
- 中央标记（组合成一个 marker）：
  - `lock_marker_plate.png`（底板）
  - `lock_marker_color_disc.png`（颜色圆片；运行时 tint 为 unlockColor）
  - `lock_marker_lock_icon.png`（锁图标）

这符合“遮罩覆盖 + 中央标记解锁所需颜色”的要求。fileciteturn3file1L6-L10  

### 其它世界贴图
- `conveyor_slot.png`
- `box_outline_dashed_open_top/right.png`
- `completed_overlay.png`
- `run_outline_9slice.png`（可操作 run 黑描边贴图方案）

---

## Layout/ui_blueprint.json（给程序 AI 的“可执行 UI 结构”）

Blueprint 提供：
- `IconButton`
- `BoosterButton_Fill`（mint）
- `BoosterButton_Shuffle`（purple）
- `FreeSlotsCounter`
- `Screen_MainMenu`（PLAY=orange）
- `Screen_GameplayHUD`
- `Modal_Settings`
- `Modal_Result`
- `LockedBoxVisual`（世界空间 SpriteRenderer 组合，支持 unlockColor tint）

> 程序 AI 只需读取 json，按 rectTransform 参数创建层级，并绑定 spriteStates。

---

## 程序实现提示（给程序 AI 的最短指令）
1) 导入所有 PNG 为 Sprite（2D and UI）。
2) 按 `Layout/ui_blueprint.json` 创建 Prefab 与 Screen。
3) 用 UIState 控制交互锁：在 BoxShipping / FastForward / Booster / Modal 时，Booster & Settings `interactable=false`，并切换到 disabled sprite。
4) LockedBox：当 `BoxSpec.locked=true` 时启用 `LockedBoxVisual`，并把 `MarkerColor` 的 tint 设置为 `unlockColor`；解锁后禁用该对象。

