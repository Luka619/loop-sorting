# UI 资源提示词结构规划（v0.5 / Creamy Plastic）

> 总索引：`../../README.md`

目标：把“看起来好看”的出图，约束成“在 Unity 工程里真实可用”的 UI PNG 资源。  
适用范围：`Tools/UiRestyleV05/_prompt_sheet_*.md` 这类 Prompt Sheet 的生成、人工微调与批量出图。

> 你遇到的“功能不匹配”，本质通常不是“风格不对”，而是**资源契约（Asset Contract）缺失**：  
> 同名只是基础；真正可用还需要：层级职责、可点击语义、9-slice 可伸缩、文字/图标安全区、三态一致性等。（尺寸不要求“对齐裁切”）

---

## 1. 什么叫“真实可用”（验收标准）

把每一张 PNG 当成一个“组件零件”，它必须满足以下约束（缺一就会在项目里翻车）：

1) **硬约束（脚本可验证）**
- 文件名与路径：必须与目标工程一致（覆盖同名 `.png`，不动 `.meta`）
- 像素尺寸：**不强制“对齐/裁切”去匹配所谓真实尺寸**（透明资源的阴影/外轮廓会被截断）；允许尺寸不一致先验收视觉效果，必要时再对少数资源单独重出更合适的尺寸
- 透明/不透明：除 `bg_main.png`、`overlay_dim.png` 等明确要求外，其余必须透明背景

2) **软约束（功能可用性）**
- **职责明确**：按钮“底座”不烤字、不烤图标；图标是独立 `icon_*`；数字是独立 `digit_*`
- **三态一致性**：Normal/Pressed/Disabled 只做“受控变化”（亮度/阴影/饱和），不能换造型
- **9-slice 友好**：边框/圆角/内阴影能被拉伸规则容纳，中心区域足够“平”
- **安全区**：阴影不出框；文字/图标区域不被装饰干扰；边缘不贴图（避免裁切感）
- **可读性**：HUD 数字/图标在深色/浅色底上都清晰；描边厚度统一

---

## 2. 核心方法：为每个资源写“资源契约（Asset Contract）”

Prompt 的结构不是“形容词堆叠”，而是把 **UI 功能 → 图形约束 → 导出约束** 串起来。

每个资源至少回答这些问题：

- **它是什么（WHAT）**：按钮底座 / 面板背景 / 图标字形 / 数字字形 / Toggle 轨道 / Toggle 旋钮……
- **它为谁服务（WHERE）**：HUD / Shop / Settings / Result / BoosterPurchase……
- **它如何被复用（HOW）**：是否 9-slice、是否 tileable、是否有三态、是否需要文字安全区……
- **它不应该包含什么（NOT）**：禁止文字、禁止 logo、水印、禁止场景、禁止透视……

> 结论：Prompt Sheet 里除了 `Positive/Negative prompt`，应额外显式写出“契约字段”，即使脚本暂不解析，也能显著降低出图走偏概率。

---

## 3. 推荐的 Prompt Sheet 条目结构（兼容现有脚本）

当前 `Tools/UiRestyleV05/GenerateOpenAiImages.py` 只解析：
- `## <dir>/<file>`
- `**Positive prompt** ~~~ ... ~~~`
- `**Negative prompt** ~~~ ... ~~~`

因此我们把“契约字段”写在标题下方的 `- key: value` 行里（不会影响解析），结构如下：

```md
## UI_Sprites/icon_retry.png (128x128)
- template: ICON_GLYPH
- usage: HUD / Retry / Button icon
- layer: glyph-only (no background plate)
- background: transparent
- content-safe: keep silhouette inside, no cropped tips
- style-tokens: Ink.Brown outline + Warm white fill

**Positive prompt**
~~~
<PROMPT_ASSEMBLED>
~~~

**Negative prompt**
~~~
<NEGATIVE_CORE + item-specific negatives>
~~~
```

### 3.1 Prompt 组装规范（Positive）

把 Positive prompt 固定拆成 5 段（顺序固定，减少随机漂移）：

1) **ROLE（职责）**  
一句话定义它是什么、用于哪里、属于哪一层（base/glyph/text-bg）。

2) **GEOMETRY（几何契约）**  
形状（圆角矩形/胶囊/圆形）、边框厚度、圆角半径风格、中心是否需要“平”、是否可 9-slice。

3) **STATE（状态，若有）**  
Normal/Pressed/Disabled 的差异只能在此段出现：亮度、阴影长度、饱和度、对比度。

4) **EXPORT（导出契约）**  
必须包含：`exact WxHpx`、`centered`、`no cropping`、`safe padding`、`sRGB`、`PNG`、`straight alpha`。背景透明/不透明建议用条目元数据标注：`- background: transparent|opaque`（避免把背景描述塞进 Positive prompt）。

5) **STYLE（风格核心）**  
统一追加 `STYLE_CORE`（来自 Prompt Sheet / Style Guide），避免每条写变体。

> 建议：把 `EXPORT_CORE` 扩展为“项目级导出合同”，并在每条 Positive 中都出现。

### 3.2 Negative 的规划方式

Negative 分两层：
- **NEGATIVE_CORE（全局）**：写死的禁用项（写实材质、透视、场景、裁切、文字、水印等）
- **NEGATIVE_ITEM（条目）**：只针对该资源的禁用项（例如 icon 禁止背景底座；panel 禁止浮雕花纹）

这样你只需要在条目里补充少量“针对性排除”，而不是每次重写一大串。

---

## 4. 按资源类型定义“模板契约”（让功能更匹配）

下面是“功能到图形”的模板化约束。你现在的模板（`BTN_SQUARE/BTN_LONG/ICON_GLYPH/DIGIT/...`）已经覆盖了一部分；这里补全“功能字段”和“可用性字段”，用于提升命中率。

### 4.1 Button Base（按钮底座：`btn_*`、`*_square_*`、`*_long_*`）

**职责**
- 只画“可按压的底座”，不画文字，不画图标（除非该资源就是“烤字按钮”且工程如此设计）

**几何契约**
- 中心区域需足够“平”（给文字/图标叠加）
- 外轮廓清晰：厚描边 + 内阴影 + 高光方向统一
- 9-slice：边框、圆角、内阴影必须在边缘带内完成；不要让装饰跨越边缘带

**三态契约（必须受控）**
- Normal：高光最强、阴影最长、对比最高
- Pressed：整体暗 6%~10%，阴影缩短 40%~60%，高光变弱（形状不变）
- Disabled：去饱和 25%~45%，对比降低，阴影更淡（形状不变）

**Prompt 关键词建议（放在 ROLE/GEOMETRY/STATE 段）**
- “pressable toy button base / thick outline / soft bevel / inner shadow / top-left highlight / bottom-right drop shadow”
- “flat center area for overlay text/icon”
- “9-slice friendly border; uniform edge shading; no unique details on edges”

### 4.2 Icon Glyph（图标字形：`icon_*`）

**职责**
- 只画“符号本身”，不画底座、不画圆角方块、不画背景光斑（除非资源名明确是 framed/no-frame）

**几何契约**
- 清晰轮廓：单主体、封闭轮廓、可一眼识别
- 统一描边与填充：`warm white fill + thick dark brown outline`
- 光照极轻：只允许微弱高光/微阴影，避免图标变成“立体物件”

**语义约束（防止功能不匹配）**
- 每个 subject 必须写明确：`retry arrow / pause || / shop storefront / gear / loop / lock / shuffle / sort / coin stack / heart`
- 禁止引入字母：例如 `retry` 不要出现 “R”，`shop` 不要出现文字

**对齐约束（减少 HUD 抖动感）**
- 视觉重心居中（optical center）
- 不要让尖角贴边；留出足够 padding（尤其是箭头尖端）

### 4.3 Digit Glyph（数字字形：`digit_0..9`）

**职责**
- 只画单个数字，不画底板、不画阴影底座

**一致性契约（解决“数字跳动/不齐”）**
- 同一字体家族、同一描边厚度、同一高光方向
- **统一 baseline**：数字底部基线一致；顶部高度一致
- **统一左右 padding**：避免 1 太瘦导致视觉偏移

**Prompt 建议**
- “single digit glyph 'X', bold rounded toy font, consistent baseline, consistent stroke thickness”

### 4.4 Panel / Card（面板/卡片：`panel_*`、`shop_card_*`、`card_setting_row.png`）

**职责**
- 为内容承载服务：中心区域必须干净、低纹理；边框提供风格

**9-slice 契约**
- 边框厚度均匀；四角圆角一致
- 内阴影不要出现“角落独特纹理”
- 渐变要非常柔和且可重复拉伸（避免拉伸后出现明显条纹）

**可读性契约**
- 中心对比低但不“脏”
- 不能有高频噪点、花纹、字符（会与文字冲突）

### 4.5 Tag / Pill（标签/信息底：`tag_*`、`hud_pill_*`）

**职责**
- 背后要叠文字（例如 `FAST xN`），因此需要明确的**文字安全区**

**契约**
- 中心“文字区”尽量平、对比稳定
- 边缘装饰不能侵入文字区
- 9-slice：优先保证左右延展后仍然自然

**Prompt 建议**
- “pill tag background, reserve clean center area for text, low texture, no embedded text”

### 4.6 Toggle（开关：`toggle_*` / `setting_page_assets/toggle_*`）

Toggle 经常“功能不匹配”的原因是：模型把 Track/Knob 合并，或者把 ON/OFF 语义画反。

**拆分契约（推荐）**
- Track（轨道）与 Knob（旋钮）分资源：`toggle_track_*`、`toggle_knob.png`
- 如果工程只有整合图（如 `toggle_full_on/off`），也要在 prompt 里明确“track+knob 一体”

**语义约束**
- OFF：灰/冷淡、低饱和；ON：mint/绿色系、亮
- Knob 永远更亮、更有高光，并有微小投影（体现浮起）
- 不要出现文字 “ON/OFF”

---

## 5. 9-slice 友好出图的“可操作规则”

Prompt 里要让模型理解“这是要被拉伸的 UI 皮肤”，否则经常画出不可拉伸的细节。

建议在 GEOMETRY 段加这些硬句式（按需挑选）：

- “9-slice friendly: uniform border, no unique details on edges”
- “flat center area, minimal texture”
- “corner radius and border thickness consistent on all corners”
- “inner shadow and highlight are uniform along edges”

并在 Negative 增加：
- “ornate frame details, corner ornaments, asymmetric decorations, noisy texture”

> 注意：Prompt Sheet 里的 `nine-slice border: L,T,R,B` 数值是给人看的；模型不会真的“遵守像素边界”，但你可以用语言把“边缘带要均匀、可拉伸”说清楚。

---

## 6. 透明、裁切与阴影：避免“看起来对但用起来错”

常见翻车点与对应约束：

1) **裁切/贴边**  
在 EXPORT 段必须出现：
- “no cropping”
- “leave generous padding”
- “no element touches image border”
- “all shadows fully inside the frame”

2) **透明背景不干净**
- Prompt Sheet：用 `- background: transparent` 标注即可；如果你的出图流程本身保证透明（如 API 参数 `background=transparent`），Positive 里不需要再写 `transparent background`
- Negative：必须包含 `checkerboard background, alpha grid, transparency grid`

3) **阴影导致 9-slice 拉伸怪**
- 让阴影“沿边均匀”，避免只在某个角特别厚
- Pressed 只缩短阴影，不改变形状外轮廓

---

## 7. 出图流程里的“结构化校验”建议（把问题尽量前置）

### 7.1 出图前：Prompt 自检（10 秒）

对每条检查：
- 是否写清 **职责（base / glyph / text-bg）**
- 是否标注 `- background: transparent|opaque`
- 是否写清 **exact WxH**
- 是否写清 **是否叠文字/图标 → 文字安全区**
- 是否有三态 → 三态差异是否“受控”
- 是否需要 9-slice → 是否写“9-slice friendly”约束

### 7.2 出图后：工程校验（5 分钟）

建议按批次验收：
1) `ReplacePngs.ps1 -DryRun`（确认映射与缺失）
2) `ReplacePngs.ps1 -Backup`（替换）
3) Unity 里只看 3 类场景：
- 9-slice 拉伸（panel、button、pill）
- 文本叠加可读性（tag、hud_pill、price）
- 三态一致性（normal/pressed/disabled）

---

## 8. 建议的“Prompt 片段库”（可直接复制）

把以下片段当作“积木”，按 3.1 的 5 段结构组装。

### 8.1 ROLE 片段
- Button base：`pressable button base for mobile UI, no text, no icon`
- Icon glyph：`UI icon glyph of <subject>, single symbol, no background plate`
- Text bg：`UI label background, reserve clean center area for text`

### 8.2 GEOMETRY 片段
- `rounded rectangle, thick outline, soft bevel, flat center area`
- `pill capsule, uniform border, symmetric shading, 9-slice friendly`

### 8.3 STATE 片段
- Normal：`Normal state: brightest highlight, longest shadow`
- Pressed：`Pressed state: slightly darker (6-10%), shorter shadow (40-60%), weaker highlight`
- Disabled：`Disabled state: desaturated (25-45%), reduced contrast, lighter shadow`

### 8.4 EXPORT 片段（建议固定）
- `exact <W>x<H>px, centered, no cropping, leave generous padding, all shadows inside frame, crisp edges, sRGB, PNG, straight alpha`

---

## 9. 落地建议（最小改动版本）

你不需要立刻改脚本，也能用这份规划提升可用性：

1) 在现有 `_prompt_sheet_*.md` 中，为“问题资产”补充 `- usage / - layer / - content-safe / - text-safe` 等契约字段  
2) 把 Positive prompt 按 5 段结构重排（ROLE→GEOMETRY→STATE→EXPORT→STYLE），减少随机跑偏  
3) 对 icon/digit/toggle 这类“功能敏感资产”，优先用更硬的语义词（glyph/silhouette/baseline）而不是风格形容词

> 后续可选：如果你希望脚本也理解这些字段，可以再扩展 `GeneratePromptSheet.ps1` 与 `GenerateOpenAiImages.py` 去解析 `- usage:` 等信息（例如自动决定 `background`、自动加“9-slice friendly”句式）。

---

## 10. 完整示例（可直接粘贴到 Prompt Sheet）

说明：
- 示例里把 Positive 按 **ROLE→GEOMETRY→STATE→EXPORT→STYLE** 写成一段（仍然是模型可读的一段英文）
- 你可以把这些示例替换到 `_prompt_sheet_*.md` 对应条目的 `**Positive prompt**` 里

### 10.1 `ICON_GLYPH` 示例：`UI_Sprites/icon_retry.png (192x192)`

```md
## UI_Sprites/icon_retry.png (192x192)
- template: ICON_GLYPH
- usage: HUD / Retry action icon
- layer: glyph-only (no background plate)
- background: transparent

**Positive prompt**
~~~
UI icon glyph of retry arrow, single bold rounded silhouette, warm white fill, thick dark brown outline, subtle highlight and micro-shadow, clean edges, no text, no background plate, centered; no cropping, leave generous padding, all shadows fully inside the frame, crisp edges, sRGB, PNG, straight alpha; soft 3D plastic UI, creamy warm candy style, thick outline, subtle specular highlight from top-left, soft ambient occlusion, clean silhouette, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, metal texture, fabric, isometric, 3D scene, perspective skew, background scene, button base, rounded square plate, circle badge, extra decorations, extra text, watermark, logo, checkerboard background, alpha grid, cropped, cut off, blurry, artifacts
~~~
```

### 10.2 `BTN_SQUARE` 示例：`UI_Sprites/mint_square_normal.png (552x566)`

```md
## UI_Sprites/mint_square_normal.png (552x566)
- template: BTN_SQUARE
- usage: HUD / Primary action button base (icon/text overlay)
- layer: base-only (icon/text is separate)
- background: transparent
- nine-slice: YES (keep border uniform, flat center)

**Positive prompt**
~~~
pressable square rounded button base for mobile UI, Mint candy plastic, thick outline using darker tone, soft bevel, top-left highlight, soft inner shadow, bottom-right drop shadow, flat clean center area for overlay icon/text, 9-slice friendly uniform border with no unique edge details, Normal state; centered, no cropping, leave generous padding, all shadows inside frame, crisp edges, sRGB, PNG, straight alpha; soft 3D plastic UI, creamy warm candy style, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, soft inner shadow, clean silhouette, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, wood texture, metal texture, fabric, noisy texture, corner ornaments, asymmetric decorations, embossed patterns, embedded text, embedded icon, watermark, logo, perspective skew, background scene, checkerboard background, alpha grid, tight framing, cropped, cut off, blurry, low-res
~~~
```

### 10.3 `TAG_PILL` 示例：`UI_Sprites/tag_fast_info_bg.png (362x120)`

```md
## UI_Sprites/tag_fast_info_bg.png (362x120)
- template: TAG_PILL
- usage: HUD / FAST label background (text overlay: "FAST xN")
- layer: text-bg (no embedded text)
- background: transparent
- nine-slice: YES (prefer horizontal stretch)
- text-safe: keep clean center zone

**Positive prompt**
~~~
pill tag background for HUD label, info mood (mint/blue), rounded capsule shape, thin but clear outline, subtle highlight from top-left, gentle inner shadow, reserve a clean flat center area for text overlay (no texture in the center), 9-slice friendly uniform border, no embedded text; centered, no cropping, leave generous padding, all shadows inside frame, crisp edges, sRGB, PNG, straight alpha; soft 3D plastic UI, creamy warm candy style, thick outline, subtle specular highlight from top-left, soft ambient occlusion, clean silhouette, orthographic front view
~~~

**Negative prompt**
~~~
photorealistic, realistic materials, noisy texture, busy patterns, gradients with banding, corner ornaments, embedded text, letters, numbers, watermark, logo, background scene, checkerboard background, alpha grid, cropped, clipped edges, blurry, artifacts
~~~
```
