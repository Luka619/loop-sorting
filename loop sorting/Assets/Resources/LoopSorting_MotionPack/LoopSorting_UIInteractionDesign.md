# Loop Sorting UI 交互设计稿（落地版 v1.0）

> 目标：把“已实现的界面”按统一的信息架构与交互规则串起来；同时给开发/美术/动效一个可执行的组件清单与位置规范，避免后续再“边做边猜”导致体验散乱。

---

## 0. 范围与引用

### 0.1 本稿覆盖的界面（当前项目已存在/已生成）
- 主界面：`MainMenuCanvas`（标题、关卡 pill、PLAY、Settings）
- 游戏 HUD：`HUDCanvas/HUDRoot`（Shop、Free Slots、Level、Speed、Settings、Coins、Lives、Boosters、FAST Tag）
- 弹窗：`SettingsPanel`、`ShopPanel`、`ResultPanel`
- 规则提示（轻提示）：由玩法反馈触发（例如 EmptyDeferred、Reject 等），目前主要在世界中与 HUD 层实现
- 锁提示（锁芯片）：`HUDRoot/LockChipLayer` 跟随锁箱位置

### 0.2 位置与资源来源
- UI 布局参考分辨率：`1080x1920`，CanvasScaler=`ScaleWithScreenSize`，`matchWidthOrHeight=0.5`
- HUD 模块矩形来自：`Assets/Resources/LoopSortingUIKitConfig.json`（layout.modules）
- UI 组件与九宫格规范参考：`Assets/Resources/loop_sorting_ui_components_v04_1_meta_pack/Docs/UI_GUIDE.md`
- 动效语法/节奏参考：`Assets/Resources/LoopSorting_MotionPack/LoopSorting_MotionDesign.md`

---

## 1. 信息架构（玩家心智模型）

### 1.1 三层信息
1) **主目标**：把传送带上的积木导入正确的箱子，最终所有非空箱“满且同色”，且传送带清空。  
2) **当前状态**：剩余空位（Free Slots）、当前关卡（LEVEL）、当前速度（x 倍速/FAST x5）、资源（Coins/Lives）、可用道具（Fill/Shuffle）。  
3) **立即可操作**：点击箱子“出货 run”、点击道具、切换倍速、打开设置、打开商店、结果弹窗的 Next/Retry。

### 1.2 交互优先级（从高到低）
1) **系统弹窗**（Result / Shop / Settings）——必须阻断底层输入  
2) **道具执行态**（Booster 执行、5x 归一化）——锁定一切输入（避免状态竞争）  
3) **出货态**（某箱 busy 正在出货 run）——允许继续观察，但限制“对其他箱子的点击”（避免多箱同时出货）  
4) **常规态**——所有 HUD & 箱体可交互  

---

## 2. Screen Flow（把已有关联成完整路径）

### 2.1 启动 → 主界面
`App Launch` → `MainMenu`
- 玩家可：`PLAY` 开始、右上 `Settings` 打开设置

### 2.2 主界面 → 游戏内
`PLAY` → `GameplayHUD + World`
- HUD 可：Shop / Coins+ / Lives+ / Speed / Settings / Boosters  
- 世界可：点击箱子出货 run（有 Busy/Locked/不匹配等反馈）

### 2.3 游戏内 → 弹窗（覆盖层）
- `Settings` → SettingsPanel（关闭后回到游戏内）
- `Shop / Coins+ / Lives+` → ShopPanel（Coins tab 或 Lives tab）
- `胜利/失败` → ResultPanel（Next/Retry/Close）

### 2.4 结果弹窗的返回（当前实现）
- Win：Primary=`NEXT`（若有 flow 则进入下一关；否则重开当前关），Secondary=`RETRY`（重开当前关）
- Lose：Primary=`RETRY`，Secondary=`CLOSE`（返回主界面）

---

## 3. 全局布局规范（可直接给程序落地）

### 3.1 参考坐标系
- 所有 “位置/尺寸”以 `1080x1920` 为基准
- Top-Left 像素矩形来自 `LoopSortingUIKitConfig.json`，由代码换算为 anchor/anchoredPosition

### 3.2 HUD 模块矩形（Top-Left origin, px）
来自：`Assets/Resources/LoopSortingUIKitConfig.json`

| 模块 | x | y | w | h | 用途 |
|---|---:|---:|---:|---:|---|
| shop | 24 | 20 | 84 | 84 | 商店入口 |
| counter | 120 | 20 | 220 | 84 | Free Slots |
| level | 380 | 20 | 320 | 84 | LEVEL 文本 |
| speed | 876 | 20 | 84 | 84 | 倍速 |
| settings | 972 | 20 | 84 | 84 | 设置 |
| lives | 600 | 120 | 220 | 78 | Lives pill + “+” |
| coins | 836 | 120 | 220 | 78 | Coins pill + “+” |

### 3.3 Booster 区域（底部）
来自：`LoopSortingUIKitConfig.json` boosters
- anchor：`(0.5, 0.07)`
- offset：`x=120`（左右分布），`y=0`
- size：`180x180`

### 3.4 主界面（MainMenu）
实现与 blueprint 对齐（anchor + size）：
- Title：anchor `(0.5,0.8)`，`anchoredPosition=(0,-80)`，size `700x260`
- LevelPill：anchor `(0.5,0.55)`，size `380x90`
- PlayButton：anchor `(0.5,0.34)`，size `900x260`
- SettingsButton：anchor `(1,1)`，`anchoredPosition=(-80,-80)`，size `180x180`

---

## 4. 组件库（统一交互语义，减少“乱”）

### 4.1 Button（按钮）
统一行为：
- 正常态：`SpriteSwap`（normal/pressed/disabled）
- 点击音效：`UiClick`（轻点击）/ `UiConfirm`（关键确认）/ `UiCancel`（取消）
- 命中区域：最小 `84x84`
- 禁用态：必须可见（disabled sprite），且不播放点击音

已存在类型（key → sprite）：
- CTA（橙）：`ui.button.orange_long.*`（主界面 PLAY、强调 CTA）
- 主操作（薄荷）：`ui.button.mint_long.*`（结果弹窗 NEXT）
- 图标方形：`ui.button.mint_square.*`、`ui.button.purple_square.*`（HUD/道具）
- 关闭按钮：`ui.button.close_red.*`
- 小按钮：`ui.button.small_blue/green/red.*`（Settings 底部 action）

### 4.2 Pill（信息条/资源条）
统一行为：
- 展示信息，不直接改变状态（资源条的“+”按钮才是入口）
- 数字变化：轻微 scale-in（XS 0.06~0.10s）+ 不吵的提示音（可选）
- 数字过长：优先完整显示（自动缩小字号），仍放不下时用紧凑格式（例如 `12.3M`）保证不溢出框外

已存在：
- Free Slots：`ui.counter.bg` + `ui.counter.icon`
- Level：`ui.hud.level_bg` + `LEVEL n`
- Coins/Lives：`ui.counter.bg`（复用）+ icon + value + plus button

### 4.3 Tag（状态标签）
- FAST tag：只在两类状态出现：  
  1) 满带 panic：danger 样式；  
  2) Booster 执行：info 样式（与 panic 区分）  
- Tag 文本统一：`FAST x5`

已存在：
- `ui.tag_fast.info`、`ui.tag_fast.danger`

### 4.4 Modal（弹窗）
统一行为：
- 背景 dim：必须 `raycastTarget=true`，阻断底层输入
- 弹出动效：`alpha 0→1` + `scale 0.92→1`（0.18~0.22s）
- 关闭动效：`alpha 1→0` + `scale 1→0.96`（0.18s）
- 关闭方式：右上角 Close（当前实现）；**是否允许点 dim 关闭**作为需求明确（默认：不允许，避免误触）

已存在：
- SettingsPanel（SOUND/VIBRATION toggles + Restore/Support/Retry）
- ShopPanel（Coins/Lives tab 内容；当前用 OpenShop(tab) 切换入口）
- ResultPanel（Victory/Failed + 主/副按钮）

---

## 5. 各界面交互详规（逐屏可验收）

## 5.1 主界面 MainMenu

### 元素与状态
- Title：纯展示
- LevelPill：展示即将进入的关卡（flowIndex+1 或 1）
- PLAY（橙 CTA）：进入游戏
- Settings（右上）：打开 SettingsPanel

### 交互
- 点击 PLAY：`UiConfirm` → 进入 `Gameplay`
- 点击 Settings：打开 `SettingsPanel`（与 HUD 共用同一个 SettingsPanel）

### 动效
- Title、PlayButton 文本统一 TMP 描边/阴影（提高可读性）
- PLAY 按下：由 SpriteSwap + 可选轻微 scale（建议后续统一实现）

---

## 5.2 游戏 HUD（常规态）

### 5.2.1 顶部区（Top）
- Shop（左上）：打开 ShopPanel（Coins tab）
- Free Slots（左上）：只读
- Level（中上）：只读
- Speed（右上）：循环倍速（`speedSteps`）；显示如 `1x/2x/4x/5x`
- Settings（右上）：打开 SettingsPanel

### 5.2.2 资源区（Top second row）
- Coins pill：展示 coins；点击 `+` 打开 ShopPanel（Coins tab）
- Lives pill：展示 lives；点击 `+` 打开 ShopPanel（Lives tab）

### 5.2.3 底部道具区（Bottom）
两枚按钮（左右对称）：
- Fill（mint）：执行 BoosterFill（会强制 5x + 归一化，再执行）
- Shuffle（purple）：执行 BoosterShuffle（同上）

交互规则：
- Booster 执行期间：锁定 HUD（speed/settings/boosters 不可点），避免乱触
- Booster 失败：提示（音效 `BoosterFail` + 轻提示，建议后续补文案）

---

## 5.3 SettingsPanel（设置弹窗）

### 结构
- Title：`SETTINGS`
- SOUND toggle：开/关音效（实时生效）
- VIBRATION toggle：开/关震动（如无震动实现，则仅记录开关状态）
- Bottom actions（占位/部分已接）：
  - RESTORE（placeholder）
  - SUPPORT（placeholder）
  - RETRY（已接：重开当前关）

### 交互规则
- Close：`UiPopupClose`（关闭动效）  
- Retry：关闭弹窗 → 重开当前关  
- dim 背景：阻断底层输入（当前不点 dim 关闭；如要支持需明确）

---

## 5.4 ShopPanel（商店弹窗）

### 入口与 tab
当前通过入口区分：
- 从 Shop/Coins+ 打开：Coins tab（标题 `SHOP`）
- 从 Lives+ 打开：Lives tab（标题 `MORE LIVES`）

> 建议（可选，后续）：在 ShopPanel 内增加显式 tab 切换（Coins/Lives），减少“只能从入口切换”的学习成本。

### 内容交互
- Coins tab：三行 coin pack，点击整行购买（当前直接加币）
- Lives tab：两行 lives pack，点击整卡购买（当前直接加命/补满）

### 交互规则
- Close：`UiPopupClose`
- 点击商品：`UiConfirm`
- Scroll：Elastic + inertia（已实现）

---

## 5.5 ResultPanel（结算弹窗）

### 触发
- Victory：所有非空箱“满且同色” + 传送带清空
- Failed：满带 panic 跑完一圈仍满（或其他失败条件扩展）

### 文案与按钮语义（当前实现）
| 状态 | 标题 | Primary | Secondary |
|---|---|---|---|
| Win | VICTORY | NEXT | RETRY |
| Lose | FAILED | RETRY | CLOSE（回主界面） |

### 交互规则
- 打开：`UiPopupOpen` + `LevelWin/LevelLose`
- Primary：
  - Win：若有 flow 且存在下一关 → Next；否则 Retry 当前
  - Lose：Retry 当前
- Secondary：
  - Win：Retry 当前
  - Lose：Close（当前仍 Retry 当前）

---

## 6. 与玩法/世界反馈的衔接（让“UI 使用”更清晰）

> 这些不是传统 HUD 组件，但对玩家理解规则至关重要，需要与 UI 交互统一语义。

### 6.1 箱体点击（出货 run）
- 可点时：箱体有可操作描边（呼吸/提示）
- Busy：箱体不可再次点；其他箱可见但建议不可点（避免多箱同时出货）
- 点击反馈：`BoxSelect` + 轻微压缩/回弹

### 6.2 门口反馈（能否进箱）
统一原则：**不能进，就不要闪白框、不要播多余音效**；只给“原因提示 + 门口形变”。
- 可进：吸入 + 落位（0.22~0.28s）
- 不可进（Locked/Busy/Full/Mismatch/EmptyDeferred）：门口轻微收缩回弹；音效按 gate 策略限频

### 6.3 锁芯片（LockChipLayer）
- 锁箱上方展示“解锁所需颜色”提示（颜色圆 + lock icon）
- 跟随箱体的 world→screen 投影，超出屏幕边缘时 clamp（已实现）
- 解锁时：建议加一次“破封/淡出”（后续可补）

---

## 7. 文案与音效（避免噪声）

### 7.1 文案清单（当前最小集）
- 主界面：`PLAY` / `LEVEL n`
- HUD：`LEVEL n`、`FAST x5`、倍速 `1x/2x/4x/5x`
- Settings：`SETTINGS`、`SOUND`、`VIBRATION`、`RESTORE`、`SUPPORT`、`RETRY`
- Shop：`SHOP`、`MORE LIVES`、`COINS`、`LIVES`、商品标题/右侧收益
- Result：`VICTORY/FAILED`、`NEXT/RETRY/CLOSE`

### 7.2 音效绑定建议（对齐现状）
- 轻点击：`UiClick`（Shop、+、Speed）
- 关键确认：`UiConfirm`（PLAY、购买、结果 Primary）
- 取消/关闭：`UiCancel` 或 `UiPopupClose`（Result Close、关闭弹窗）
- 弹窗打开：`UiPopupOpen`

---

## 8. 统一验收清单（用来保证“不乱”）

- [ ] 主界面、HUD、弹窗使用同一套 Button/Pill/Modal 语义（颜色=功能）
- [ ] 所有可点击元素有明确的 pressed/disabled 表现，且禁用不出声
- [ ] 任意时刻最多只存在一个“输入最高层”（Modal 或 Booster 执行态）
- [ ] Shop/Settings/Result 关闭后回到正确状态（HUD 可交互、世界可交互）
- [ ] FAST tag 只在“panic/booster”两种状态出现，并区分样式
- [ ] 锁箱必有“需要的颜色提示”（LockChip）且不遮挡主要按钮

---

## 9. 与代码的映射（方便程序对照）

主要生成与逻辑入口：
- 主界面：`Assets/Scripts/GameRuntimeController.cs` → `EnsureMainMenuUI()` / `ShowMainMenu()`
- HUD：`Assets/Scripts/GameRuntimeController.cs` → `EnsureCounterUI()`
- Settings：`Assets/Scripts/GameRuntimeController.cs` → `EnsureSettingsUI()` / `ToggleSettingsPanel()`
- Shop：`Assets/Scripts/GameRuntimeController.cs` → `EnsureShopUI()` / `OpenShop()`
- Result：`Assets/Scripts/GameRuntimeController.cs` → `EnsureResultPanel()` / `ShowResult()`
- 锁芯片：`Assets/Scripts/GameRuntimeController.cs` → `SyncLockChipsUI()`
