# Loop Sorting UI 适配与组件规范（路线 2 / Prefab + LayoutGroup）

> 版本：v0.1（起草稿）  
> 目标：形成一套“可复用、可扩展、可测试”的 UI 规范；后续按关卡/页面逐步迁移并迭代本规范。

本规范面向两类平台同时成立：
- **Unity 移动端（iOS/Android）**
- **微信小游戏（WebGL）**：存在右上角胶囊区域、safeArea 坐标/像素体系差异等特殊点。

---

## 0.5 风格基准（v0.5 / Creamy Plastic）
- **参考**：你提供的 Shuffle/Setting 弹窗截图（奶油底 + 橙色标题条 + 3D 塑料糖果质感）
- **主规范**：`UI_STYLE_GUIDE_V05_CREAMY_PLASTIC.md`
- **资源出图提示词**：`UI_ASSET_PROMPT_PACK_V05_CREAMY_PLASTIC.md`
- **重制执行计划**：`UI_RESTYLE_V05_CREAMY_PLASTIC.md`
- **资源替换脚本**：`Tools/UiRestyleV05/README.md`
- **旧资源包说明（v0.4 组件/命名/9-slice 仍可复用）**：`Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/Docs/UI_GUIDE.md`
- **优先落地范围**：玩法界面 + HUD（TopBar/货币条/BoosterButtons/FAST tag），再扩展到弹窗与商店

## 0. 原则（必须遵守）
1. **可预测**：同一 UI 在不同机型只允许“留白变化/边缘避让变化”，不允许“整体挤压导致布局语义改变”（例如按钮挤在一起、标题与按钮重叠）。
2. **分层适配**：缩放（Scale）、安全区（SafeArea）、布局（Layout）必须拆开处理，避免“一个脚本改全局导致副作用”。
3. **约束优先**：新 UI 一律用 **Prefab + 锚点 + LayoutGroup/Constraints**，尽量不写死 `anchoredPosition/sizeDelta`。
4. **边缘避让优先**：safeArea 用于“边缘避让”，不要把“整棵 UI”缩进 safeArea 导致中间内容被压缩。
5. **组件复用优先**：按钮/弹窗/货币条等必须组件化；任何页面出现第二次就应抽成 Prefab。

---

## 1. 术语与坐标体系（统一口径）
- **Reference Resolution（参考分辨率）**：`1080×1920`（与 `LoopSortingUIKitConfig.json` 一致）。
- **UI Units（UI 单位）**：CanvasScaler 缩放后的 UI 坐标单位（`RectTransform.anchoredPosition` 所在单位）。
- **SafeArea Insets（安全区边距）**：`left/right/top/bottom`，同时维护两份：
  - `Px`：屏幕像素（`Screen.width/height` 体系）。
  - `Units`：按 CanvasScaler 换算后的 UI 单位（用于布局偏移/ padding）。
- **Capsule（微信胶囊）**：微信右上角“···/○”区域；它是“功能区避让”，不应把全局 safeArea 宽度挤窄。

---

## 2. UI 架构规范（Canvas 与层级）
### 2.1 Canvas 分层（推荐）
建议统一一个 UI Root Prefab（后续迁移目标）：
- `UICanvasRoot`（sortingOrder: 0）：HUD/主页面 UI
- `UIOverlayCanvas`（sortingOrder: 10）：弹窗/遮罩/引导/Toast（可选单独 Canvas，避免频繁重建主 Canvas）
- `UIDebugCanvas`（sortingOrder: 100）：调试覆盖（safeArea 可视化、设备信息）

约束：
- 一个场景只允许 **一个 EventSystem**。
- Overlay 层使用 `CanvasGroup` 控制显隐与交互（`alpha/interactable/blocksRaycasts`），避免频繁 `SetActive` 造成 Layout 重算抖动。

### 2.2 Root 层级（每个页面/屏幕必须一致）
每个 Screen Prefab 建议固定结构（命名统一，便于脚本查找与团队协作）：
- `BG`：背景（全屏铺满，不吃 safeArea）
- `SafeRoot`：承载 safe-area padding 的容器（只负责“边缘避让”，不负责缩放）
  - `TopBar`：顶部 HUD（吃 top inset；右侧按钮额外避让胶囊）
  - `Content`：中间内容区（默认不吃 safeArea；必要时仅吃 top/bottom）
  - `BottomBar`：底部操作区（吃 bottom inset）
- `OverlayRoot`：弹窗层容器（一般不吃 safeArea；仅 close/按钮避让）

---

## 3. 缩放规范（CanvasScaler / 分辨率适配）
### 3.1 参考配置（必须）
- `CanvasScaler.uiScaleMode = ScaleWithScreenSize`
- `referenceResolution = 1080×1920`
- `matchWidthOrHeight` **必须是“稳定策略”**，不允许每帧 lerp 造成布局漂移感。

### 3.2 推荐 match 策略（v0.1 建议，后续可调）
以竖屏为主：
- **手机竖屏**：`matchWidthOrHeight = 0`（match width，保证横向视觉一致，纵向用留白/背景延展）
- **平板/接近 4:3**：可用 `matchWidthOrHeight = 1` 或“分段策略”，避免 UI 过大

> 说明：旧实现里动态 lerp match，会导致“同一套绝对坐标在不同 aspect 下挤压/漂移”；路线 2 要把这种不稳定因素降到最低。

---

## 4. SafeArea 规范（物理安全区 + 微信胶囊）
### 4.1 SafeArea 只做“边缘避让”
禁止做法（会导致你现在看到的“中间挤在一起”）：
- 把整棵 UI（含 Content）缩进 safeArea。

推荐做法：
- `TopBar/BottomBar` 使用 safeInsets 做 `padding` 或 `offset`。
- `Content` 默认不吃左右 safeInsets；如遇极端设备，仅吃 `top/bottom` 或通过背景留白解决。

### 4.2 微信胶囊处理（必须单独对待）
规则：
- **不要**用“缩小 safeArea 宽度”来避让胶囊（会导致全局 UI 左移/变窄）。
- 胶囊只影响：`TopBar` 的右上角按钮组（设置/商店/货币加号等）。

实现建议（后续代码落地）：
- `SafeAreaService` 输出 `CapsuleAvoidRightUnits`（或 CapsuleRect），仅用于 TopRight Cluster 的额外右边距。

### 4.3 safeInsets 的输出形态（供所有 Prefab 复用）
统一一个服务（脚本名建议）：
- `LoopSortingUIScreenMetrics` / `SafeAreaService`
  - `SafeInsetsPx`、`SafeInsetsUnits`
  - `CapsuleInsetsUnits`（微信专用）
  - `AspectBucket`（16:9 / 19.5:9 / 4:3 等）
  - `OnMetricsChanged` 事件（旋转/分屏/状态栏变化）

---

## 5. 布局规范（Prefab + LayoutGroup）
### 5.1 锚点规则（必须）
- 顶部条：锚点 `Top Stretch`（x: 0..1, y: 1..1），高度用 `LayoutElement.preferredHeight` 或固定高度。
- 底部条：锚点 `Bottom Stretch`。
- 中间内容：锚点 `Stretch`，上下由 TopBar/BottomBar 挤压出空间（不要靠绝对 y 定位）。
- 角落按钮：锚点到对应角（TopRight/TopLeft），使用 `LayoutGroup` 或父容器 padding 控制边距。

### 5.2 LayoutGroup 使用规则（必须）
推荐组件组合：
- 横排：`HorizontalLayoutGroup + LayoutElement`
- 竖排：`VerticalLayoutGroup + LayoutElement`
- 网格：`GridLayoutGroup`（仅用于规则网格，避免做复杂自适应）

约束：
- `ContentSizeFitter` **慎用**：只允许在“文字长度不确定且必须包裹”的局部容器使用；禁止嵌套使用（容易引发布局抖动/性能问题）。
- `LayoutRebuilder.ForceRebuildLayoutImmediate` 只允许在“打开弹窗/切语言/动态换图标”这类低频场景调用。

---

## 6. UI 组件规范（什么组件用什么规范）
> 这一节是“以后照着做”的核心。组件必须做成 Prefab，并在页面内复用。

### 6.1 Button（按钮）
统一要求：
- 可点击区域最小：参考分辨率下 **≥ 120×120**（触控容错）；图标按钮同理。
- 必须有 3 态：Normal / Pressed / Disabled（可用换图或色调）。
- 必须有按压反馈：缩放 `0.95~0.97` + 80~120ms（不允许完全无反馈）。
- 使用 `LoopSortingUIKit` 的 sprite key；如 sprite 有九宫 border，必须 `Image.Type = Sliced`。

按钮类型与规范：
- `ButtonPrimary`：用于主操作（PLAY/确认/购买）
  - 布局：可伸缩，宽度跟随容器，文字居中
- `ButtonSecondary`：次要操作（取消/返回）
- `ButtonIconSquare`：顶部角落按钮（设置/商店）
  - 布局：放在 `TopBar.RightCluster`，受 `CapsuleInsetsUnits` 影响
- `ButtonClose`：弹窗关闭
  - 布局：放在 `OverlayRoot` 内的右上角，但只避让 `SafeInsets.top`（不挤压整个弹窗）
- `ButtonPrice`：货币购买（绿底+金额）
  - 布局：内部结构固定：Icon + Text；用 `HorizontalLayoutGroup`
- `ButtonAd`：看广告（橙底+图标+FREE）

### 6.2 HUD Currency Pill（货币条）
规范：
- 必须放在 `TopBar.RightCluster` 内，采用 `HorizontalLayoutGroup`。
- `+` 按钮与数值同组，不允许分散定位。
- 数值文本使用 TMP，开启等宽数字（或自定义字体特性）避免跳动。

### 6.3 Modal Panel（弹窗）
规范：
- `OverlayDim` 全屏遮罩：全屏，不吃 safeArea。
- `Panel` 本体：居中（或按设计偏移），不吃左右 safeInsets（避免变窄）；只对 close 按钮做 safeTop 处理。
- 打开动画：`fade in dim + panel scale 0.96->1 + y 轻微上移`（150~220ms）
- 关闭动画：反向（120~180ms）

### 6.4 Booster Purchase（购买弹窗）
规范（路线 2 目标）：
- 弹窗本体做成 Prefab：Header / Icon / Subtitle / Actions
- Action 区固定用 `HorizontalLayoutGroup`，两个按钮等宽
- Icon 支持轻微 idle 动效（bob/tilt），但必须可关闭（低端机/性能模式）

### 6.5 List / Scroll（列表）
规范：
- 滚动容器：`ScrollRect + Mask + Content(VerticalLayoutGroup)`
- 列表项 Prefab 统一高度与内边距
- 滚动渐隐（上下 Fade）可作为可选装饰，但不能影响点击区域

---

## 7. 脚本与绑定规范（路线 2 必须）
### 7.1 Prefab View 脚本（必须）
每个可复用 Prefab 必须有一个 `xxxView` MonoBehaviour：
- 所有子节点引用用 `[SerializeField]` 显式绑定（禁止运行时 `Find()` 作为主路径）
- 公开方法以“语义”命名：`SetCoins(int)`、`SetEnabled(bool)`、`PlayOpen()`、`Bind(Action onClick)` 等

### 7.2 UI 驱动方式（必须）
- 业务逻辑与 UI 解耦：Controller 只负责“状态 -> View”，不直接操作子节点层级结构。
- 弹窗采用 `Show/Hide`（CanvasGroup）而不是频繁 Instantiate/Destroy（除非活动页等一次性内容）。
- 任何会影响持久化状态的 UI 行为（设置开关/购买/道具增减/过关）必须走统一存档触发（见 `SAVE_SYSTEM.md`），禁止在页面里零散写 `PlayerPrefs`。

---

## 8. 资源规范（LoopSortingUIKit 与新增资源）
### 8.1 资源入口（必须）
- UI 图片尽量通过 `LoopSortingUIKit.LoadSpriteByKey(key)` 取（统一九宫、统一缓存）。
- 新增资源必须更新 `Assets/Resources/LoopSortingUIKitConfig.json`（新增 key/path/九宫规则）。

### 8.2 命名建议
- 组件 Prefab：`UI_<Category>_<Name>.prefab`（例如 `UI_Button_Price.prefab`）
- 页面 Prefab：`Screen_<Name>.prefab`（例如 `Screen_MainMenu.prefab`）
- 弹窗 Prefab：`Modal_<Name>.prefab`（例如 `Modal_BoosterPurchase.prefab`）

---

## 9. 适配测试矩阵（上线前必须全过）
设备/比例（至少）：
- iPhone 刘海机（19.5:9）+ 微信小游戏
- iPhone 非刘海（16:9 或接近）+ 微信小游戏
- Android 打孔屏（20:9）+ 原生/微信
- iPad（4:3）+ 原生

场景（至少）：
- 主菜单、HUD、设置、商店、胜负结算、BoosterPurchase

验收标准：
- 顶部所有可点元素不被刘海/状态栏/胶囊遮挡
- 底部按钮不贴边/不被 Home Indicator 覆盖
- 中间内容不因 safeArea 产生“整体挤压变形”
- 所有按钮可点区域足够（误触率低）

---

## 10. 迁移策略（一步步落地）
建议顺序（由易到难）：
1. `Screen_MainMenu` Prefab 化（TopBar + Content + BottomBar）
2. `HUD` Prefab 化（TopBar/BottomBar 的 safe padding）
3. Modal 系统抽象（Settings/Shop/Result/BoosterPurchase 统一 Modal 框架）
4. 组件库完善（按钮/货币条/标签/弹窗头部等）

每迁移一个页面，都要回到本规范补充/修正：新增的“组件类型/例外情况/适配规则”。

---

## 11. 规范迭代方式（我们“逐步调整规范”的流程）
- 本文件每次迭代用 `v0.x` 标注，并在本文件顶部维护简短变更记录。
- 任何“新增例外规则”必须写清楚：适用范围 + 原因 + 替代方案。
- 不允许“只在某个页面写死偏移”而不回写规范；否则后续页面必然复刻同类问题。
