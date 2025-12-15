# Gameplay/HUD AI 产出工作流（Bright + Candy）

目标：在“全 AI 生成（交互稿/资源/代码）”前提下，把玩法界面与 HUD 做到**稳定适配 + 高质感 + 可迭代**。

## 0. 固定输入（每次都要喂给 AI）
- 规范：`UI_STANDARDS.md`
- 资源包说明（风格基准/组件清单/9-slice）：`Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/Docs/UI_GUIDE.md`
- 动效语言：`Assets/Resources/LoopSorting_MotionPack/LoopSorting_MotionDesign.md`
- 资源入口与布局：`Assets/Resources/LoopSortingUIKitConfig.json`（sprite keys / layout.modules / nineSliceRules）

## 1. HUD 先做什么（优先级）
1) **TopBar（含胶囊避让）**：Shop / Level / Speed / Settings / 货币条（Coins/Lives + “+”）
2) **Bottom Boosters**：Fill/Sort + Shuffle（含角标数量）
3) **FAST tag**：info/danger 两态（满槽快进/道具快进）

> 注意：SafeArea 只做“边缘避让”。玩法内容区不整体缩进 safeArea；胶囊仅影响 TopRight Cluster。

## 2. 一次迭代的标准产物
- **交互稿（文本即可）**：状态机（Normal / InputLocked / FastForward / ModalOpen）、每个按钮的可用条件、点击反馈、音效触发点
- **布局稿（结构树 + 约束说明）**：按 `BG / SafeRoot / TopBar / Content / BottomBar / OverlayRoot` 输出；TopBar 右侧必须单独处理胶囊 inset
- **资源清单（可执行）**：`spriteKey -> 文件名 -> 是否 9-slice -> 备注（Normal/Pressed/Disabled）`
- **工程变更**：只允许两类
  - 更新 `Assets/Resources/LoopSortingUIKitConfig.json`（新增 key/path/9-slice 规则）
  - 更新 UI 构建代码/Prefab（当前项目 HUD 在 `Assets/Scripts/GameRuntimeController.cs` 内生成）
- **QA 清单**：按测试矩阵出“问题列表 + 修复建议”，不写空话

## 3. AI 生成的提示词（直接复制用）

### 3.1 交互+布局规格（输出 Markdown）
你是移动端休闲解谜游戏 UI/UX 总监。严格遵守：
1) `UI_STANDARDS.md`
2) `Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/Docs/UI_GUIDE.md`
3) `Assets/Resources/LoopSorting_MotionPack/LoopSorting_MotionDesign.md`

为【玩法界面 + HUD】输出：
- 组件清单与信息优先级（TopBar / Currency / Boosters / FAST tag）
- 状态机（Normal/InputLocked/FastForward/ModalOpen），每个状态下哪些按钮可点
- 层级结构树（含 TopBar.Left/Center/RightCluster，RightCluster 需要 CapsuleInset）
- 布局策略（只允许 LayoutGroup/锚点/约束；禁止“整体缩进 safeArea”）
- 动效时序（按压反馈、HUD 入场、FAST tag 切换）
- 验收点（safeArea + 胶囊 + 最小点击区 + 多分辨率）

### 3.2 资源清单（输出 JSON diff 思路）
你是 UI 资产制作总监。基于 Bright + Candy 风格，输出：
- HUD 现有可复用 spriteKey 列表（优先复用）
- 缺失资源：给出 `key/path/applyNineSlice/pixelsPerUnit`（对齐 `Assets/Resources/LoopSortingUIKitConfig.json`）
- 9-slice 规则：给出 pattern + border（参考 `UI_GUIDE.md`）
- 文件命名必须可批量生成（同色系 normal/pressed/disabled 成组）

### 3.3 Unity 落地（输出代码变更点）
你是 Unity UGUI 工程师。目标是“玩法界面 + HUD”高品质呈现：
- UI 资源只允许通过 `LoopSortingUIKit.LoadSpriteByKey(key)` 取
- SafeArea：只对 TopBar/BottomBar 做 padding；胶囊只影响 TopRight Cluster
- 文本：TMP 必须有 Outline + Underlay（参数参考 `UI_GUIDE.md`）
- 按钮：必须有按压反馈（scale 0.95~0.97）+ pressed sprite；禁用时切 disabled sprite
- 输出：需要修改的函数/代码块清单（文件路径 + 关键方法名），避免大范围重写

### 3.4 QA 审查（输出问题列表）
你是 UI QA。按设备矩阵（iPhone 刘海/非刘海、Android 打孔、iPad、微信 WebGL）检查：
- 顶部元素是否被状态栏/胶囊遮挡
- 底部 Booster 是否被 Home Indicator 覆盖
- 任意机型是否发生“整体挤压导致布局语义改变”
- 点击区是否满足 ≥120×120（参考分辨率）
- 文本是否抖动（数字跳动/自动排版抖动）

## 4. 已确认约束（用于后续所有迭代）
1) 顶栏 **一排**（TopBar 单行布局）
2) Boosters **常显**（仅在必要的输入锁定时禁用交互，不隐藏）
3) FAST tag 文案 **随倍率变化**（例如 `FAST x2` / `FAST x5`）
