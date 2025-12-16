# UI 概念稿 → 组件资源（v0.5 / Creamy Plastic）工作流

你的目标是做“多套 UI 风格横向对比”，所以推荐把流程拆成两阶段：
1) **概念稿（Screen Concept）**：先出整屏 UI（像 Figma 截图那种），快速判断风格方向、信息层级、对比度、整体气质。
2) **组件资源（Component Assets）**：再把概念稿里用到的真实组件逐一落到可替换的 PNG（同名覆盖、三态一致、9-slice 友好等；不做“对齐裁切”）。

这份文档给你三样东西：
- 现有游戏 UI 应该分成哪些 Screen/Modal
- 每个 Screen/Modal 在“概念稿 Prompt”里必须出现哪些真实组件
- 从概念稿回落到“组件 Prompt Sheet”的做法（对应本项目已有脚本）

---

## 0) 先把现有功能分成界面（Screen/Modal）
当前 UIKit blueprint 中已明确的界面节点：
- `Screen_MainMenu`：主界面（PLAY / LEVEL / Settings）
- `Screen_GameplayHUD`：关卡 HUD（顶部信息/加号/Boosters）
- `Screen_Shop`：商店页面（顶部 scallop / Title / Close / CurrencyRow / ScrollList）
- `Modal_Settings`：设置弹窗（简版）
- `Modal_Settings_Full`：设置弹窗（完整版）
- `Modal_Result`：结算弹窗（Result）
- `Modal_MoreLives`：体力不足弹窗（More Lives）

如果后续新增功能，建议继续按这个粒度：**一个“能单独打开/关闭”的 UI 视图就是一个 Screen/Modal**。

---

## 1) 自动提取“每个界面用到哪些组件”（推荐）
不靠手写清单，直接从 `ui_blueprint.json` 抽取每个界面引用的 sprite 文件名：

```powershell
python Tools/UiRestyleV05/ReportUiScreensFromBlueprint.py --out Tools/UiRestyleV05/_ui_screen_usage_report.md
```

输出文件：`Tools/UiRestyleV05/_ui_screen_usage_report.md`
- 里面按 `Screen_* / Modal_*` 列出用到的 `UI_Sprites/*.png`
- 同时会附带（如果能查到）尺寸信息（来自 `Tools/UiRestyleV05/_sizes_ui_sprites.json`）

这份报告就是你写“概念稿 Prompt”的组件库（不要让模型自由发挥 invent 新组件）。

---

## 2) 概念稿 Prompt 怎么写（整屏）
概念稿的目的不是“直接拿来进 Unity”，而是：
- 快速对比视觉方向（材质、配色、对比度、圆角、阴影强度、信息层级）
- 让你确认：这套风格能否覆盖你游戏的真实组件体系（按钮、面板、货币条、商店行、结算等）

### 2.1 概念稿的约束（强烈建议）
在概念稿 Prompt 里强制写清：
- 画布：竖屏比例即可（分辨率不强制；不要为了“对齐尺寸”而裁切/贴边）
- **必须使用的组件清单**：来自 `_ui_screen_usage_report.md` 对应 Screen/Modal 的 sprite 文件名
- **不允许新增组件**：不允许 invent 新按钮形状/新卡片框/新 icon 语言
- **文字只作为 TMP**：不要把文字“烤”到按钮底图上（比如 PLAY 字样不能作为按钮贴图的一部分）
- 输出：`PNG`，概念稿建议用 `background=opaque`（整屏不是透明资产）

### 2.2 概念稿 Prompt 模板（可复制）
把 `{SCREEN_NAME}` 和 `{COMPONENT_LIST}` 换成你的目标界面即可：

```
You are designing a mobile game UI screen mockup (full-screen concept), NOT individual assets.

CANVAS: portrait aspect ratio, PNG, opaque background (no strict resolution requirement).
STYLE: creamy plastic, candy warm, soft 3D UI, top-left highlight, gentle inner shadow, clean silhouette.

STRICT COMPONENT LIBRARY (must use ONLY these components, no new UI shapes):
{COMPONENT_LIST}

LAYOUT GOAL:
- Compose a complete {SCREEN_NAME} screen using the components above.
- Use consistent spacing, safe margins, and clear hierarchy.
- Text should be separate UI text (TMP-like), not baked into button background textures.
- Keep icons separate (icon_*), digits separate (digit_*), buttons are backgrounds only (btn_* / *_long_*).

DO NOT:
- invent new buttons/cards/panels
- add logos/watermarks
- add scene backgrounds inside panels
- perspective / isometric / skewed UI
```

### 2.3 每个界面“概念稿里必须出现什么”
你可以先按下面的最低要求做第一版概念稿（先求覆盖功能，再求细节）：

- `Screen_MainMenu`
  - 背景：`bg_main.png`
  - 主 CTA：`orange_long_normal.png`（PLAY）
  - Settings：`mint_square_normal.png` + `icon_gear.png`
  - 标题：`TMP_Text`（LOOP SORTING）
- `Screen_GameplayHUD`
  - 顶部 HUD pill：`hud_pill_dark_small.png`（coins/lives/speed 等）
  - `icon_coin.png` / `icon_heart.png` / `icon_speed.png`
  - `btn_small_*`（加号/小按钮）
  - Booster：`icon_sort_noframe.png` / `icon_shuffle_noframe.png`
- `Screen_Shop`
  - 面板：`panel_thick_gold_blue.png`（或项目映射的 `ui.panel_shop`）
  - 顶部 scallop：`shop_topbar_scallop_tile_512x128.png`（tiled）
  - 关闭按钮：`btn_close_red_*`
  - 货币条：`hud_pill_dark_small.png` + icon + value
  - 列表：`shop_group_bar.png` + 多行 `shop_row_yellow.png` 或 `shop_card_beige.png`
- `Modal_Settings(_Full)`
  - Dim：`overlay_dim.png`（或纯色 dim）
  - 面板：`panel_thick_gold_blue.png` / `panel_modal.png`
  - Toggle：`toggle_track_*` + `toggle_knob.png`
  - close：`btn_close_red_*`
- `Modal_Result`
  - 面板：`panel_result.png`
  - 标题/数值：TMP + digits
  - 主要按钮：`*_long_*` / `btn_price_green_*`
- `Modal_MoreLives`
  - 面板：`panel_thick_gold_blue.png`
  - 主要按钮：`*_long_*`
  - 货币条：`hud_pill_dark_small.png` + icon/value

---

## 3) 从概念稿落到“组件资源 Prompt Sheet”
概念稿确定之后，做组件资源时要遵守本项目的 Asset Contract（同名覆盖、三态一致、9-slice 友好、透明背景等；不做对齐裁切）。

推荐步骤：
1) 打开某个 Screen 概念稿，把“出现的组件”按文件名列出来（尽量只用项目已有的命名体系）
2) 对照 `Tools/UiRestyleV05/_prompt_db_all_v05.json`（或导出的 `Tools/UiRestyleV05/_prompt_sheet_all_v05.md`）：
   - 已存在且满意：跳过
   - 已存在但需要更贴合概念稿：修改该条目的 prompt（保持文件名不变）
   - 缺失：补一条 prompt（文件名必须与工程一致）
3) 用 API易 批量生成组件 PNG（透明背景，且不做后处理）：
```powershell
python Tools/UiRestyleV05/GenerateOpenAiImages.py --api-base https://api.apiyi.com/v1 --model gpt-image-1-mini --quality low --gen-size auto --background transparent --parallel 5 --prompt-sheet Tools/UiRestyleV05/_prompt_db_all_v05.json --out-dir Tools/UiRestyleV05/_openai_output
```
4) 把输出覆盖到你要对比的 PackRoot（不影响其它风格包）：
```powershell
powershell -ExecutionPolicy Bypass -File Tools/UiRestyleV05/ReplacePngs.ps1 -SourceDir Tools/UiRestyleV05/_openai_output -KitRoot Assets/Resources/<PackRoot> -Backup -AllowPartial
```
5) 在游戏里切换包（只改一个值）：
- `PlayerPrefs` key：`LoopSortingUIKit.ResourcesRoot`
- value：`<PackRoot>`（例如 `loop_sorting_ui_components_v05_pack_b`）

---

## 3.1) 功能优先（Function-first）：先定“结构/状态/约束”，再写 Prompt
你说的 toggle 就是典型例子：它不是“随便一个好看的开关图”，而是一个**可交互组件**，必须满足运行时结构与状态逻辑，否则看起来再“好看”也会不对。

### A) 先写清楚功能结构（要生成的到底是什么）
每个资源在写 prompt 前先回答 4 个问题（写在条目旁边/Prompt DB 的 tags 或你自己备注都行）：
1) **组件类型**：`panel(9-slice?) / button_base / icon_glyph / digit / toggle / bg(opaque)`？
2) **由哪些部件组成**：例如 toggle = `track(槽) + knob(圆帽) + outline + shadow`（不是“一个圆角长条”）。
   - 例（Toggle 拆分清单，默认做法）：
     - `UI_Sprites/toggle_track_on.png`：轨道/凹槽（含外框 outline + 轨道自身阴影）
     - `UI_Sprites/toggle_track_off.png`：轨道/凹槽（OFF 配色；也可后续用运行时对 `track_on` 做去饱和）
     - `UI_Sprites/toggle_knob.png`：圆帽/滑块（凸起；含自身 outline + 自身阴影）
     - （可选）`UI_Sprites/toggle_shadow.png`：整体投影（只做“最外层软阴影”，方便统一调强弱）
     - （可选）`UI_Sprites/toggle_outline.png`：独立描边层（如果你想在不同主题下复用同一条描边）
  - 运行时组合顺序建议：`shadow (optional)` → `track` → `knob` → `outline (optional)`

补充（实现状态）：
- 当前运行时代码已支持以下“拆分文件名”组合（存在则启用；不存在会回退到旧单图）：
  - `UI_Sprites/panel_modal_base_9slice.png` + `UI_Sprites/panel_modal_decor.png`
  - `UI_Sprites/panel_result_base_9slice.png` + `UI_Sprites/panel_result_decor.png`
  - `UI_Sprites/hud_pill_dark_small_base_9slice.png` + `UI_Sprites/hud_pill_dark_small_decor.png`
  - `UI_Sprites/hud_pill_dark_base_9slice.png` + `UI_Sprites/hud_pill_dark_decor.png`
- Booster badge 数字使用 TMP（无需 digit 贴图；旧 digit 方式仍兼容）
3) **有哪些状态**：`normal / pressed / disabled / on / off`，并明确“状态差异来自哪里”（颜色、阴影、凹凸、位置）。
4) **不可变约束**（必须遵守的运行时约束）：
   - 透明资产：画面外必须是**完全透明**（不要 vignette、不要雾、不要背景渐变、不要光晕）。
   - 不能裁切阴影/外轮廓（保留安全 padding），但也不能留超大空白导致 UI 里显示变小。
   - 对齐/位置：例如 toggle 的 knob 必须严格在左/右端，不能漂移。

### B) 把“功能结构”写进 prompt（让模型没法胡来）
以 toggle 为例：
- 必须明确：`track 是凹槽，knob 是凸起圆帽，knob 在左/右`。
- 必须明确：`外部全透明、无背景、无光晕、无木纹皮革材质`。
- 必须明确 pressed 的差异：`更暗、阴影更短、凹槽更深、knob 更贴近槽`。

### C) 何时拆分成多个资源
默认**一律按层拆成多个资源分别生成**（按功能结构来拆），不要把文字/图标画进底图里。
只有在你后续**特殊说明某个资源不用拆**时，才允许做“合成后的单图资源”。

## 4) 两条关键建议（避免返工）
1) 概念稿用 `opaque`，组件资产用 `transparent`：两者目的不同，不要混用。
2) 不要在概念稿里“画出按钮文字/图标作为贴图的一部分”：概念稿可以显示文字，但组件资产必须分离（按钮底、icon、digit、TMP）。
