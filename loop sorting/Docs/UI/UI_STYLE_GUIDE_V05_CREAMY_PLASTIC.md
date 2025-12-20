# Loop Sorting UI 风格规范（v0.5 / Creamy Plastic）

> 总索引：`../README.md`

> 适用范围：优先覆盖 **玩法界面 + HUD**，并作为后续所有弹窗/商店/结算的统一风格基准。  
> 风格参考：你提供的 Shuffle / Setting 弹窗截图（奶油底 + 橙色标题条 + 3D 塑料糖果质感）。

---

## 1. 风格目标（Pillars）
1) **玩具级“立体可按”质感**：所有可点击元素必须“像实体按钮”，有明确的高光、厚度与落影。  
2) **温暖奶油底**：整体以奶油/米色作为主背景氛围，彩色按钮像“贴上去的糖果”。  
3) **一致的光照语言**：同一方向的主光/阴影；禁止各资产光照方向不一致导致“拼贴感”。  
4) **可规模化生产**：所有资源可按模板批量生成（Normal/Pressed/Disabled 三态成组；9-slice 可复用）。

---

## 2. 全局光照与材质语言（必须统一）
### 2.1 光照方向
- **主光（Key Light）**：左上 → 右下
- **投影（Drop Shadow）**：右下偏移（更靠下，不要左右偏得太多）
- **内阴影（Inner Shadow / AO）**：四周轻微，底边略重

### 2.2 材质关键词（Prompt 里反复强调）
- **soft 3D plastic / candy plastic**
- **rounded beveled edges**
- **subtle specular highlight**（高光柔，不要镜面反光）
- **soft inner shadow / ambient occlusion**
- **fine grain**（2–4% 颗粒，避免过于“平”）

### 2.3 禁止项（避免跑偏）
- 禁止写实照片/真实材质纹理（木头/金属拉丝/皮革）
- 禁止强透视与倾斜角（所有 UI 必须近似正交）
- 禁止复杂背景与多余装饰（必须可 9-slice，必须可组合）

---

## 3. 颜色系统（Design Tokens）
> 颜色只定义“角色”，具体数值允许在生成时微调，但必须在同一套 token 范围内。

### 3.1 中性色（奶油底）
- `Cream.0`（高光白）：`#FFF7E7`
- `Cream.1`（主奶油）：`#F7EBCB`
- `Cream.2`（米色阴影）：`#E3CFA0`
- `Ink.Brown`（深棕文字/描边）：`#6A3D17`
- `Ink.Navy`（深蓝描边备选）：`#0B1730`

### 3.2 功能色（与 v0.4 语义对齐）
- `Mint`（通用/主操作）：基色建议 `#48D7B4`，阴影 `#138C75`，高光 `#B6FFE9`
- `Purple`（Shuffle）：基色建议 `#8E7BFF`，阴影 `#4B2BC7`，高光 `#E7E1FF`
- `Orange`（强 CTA/标题条）：基色建议 `#F3A12B`，阴影 `#C06600`，高光 `#FFE1A3`
- `Green`（价格/奖励）：基色建议 `#7ED348`，阴影 `#3D8E00`，高光 `#D7FFB2`
- `Red`（危险/关闭/重试）：基色建议 `#FF5A3A`，阴影 `#B51E0C`，高光 `#FFD0C6`

### 3.3 渐变（推荐固定方向）
- 垂直渐变：上亮下暗（更“立体”）
- 高光带：左上角一条柔光弧（不要硬边）
- 描边：外描边更深，内描边偏亮（形成“塑料边缘”）

---

## 4. 形状语言（Shape Tokens）
### 4.1 圆角
- **大圆角**是本风格核心：所有面板/按钮都应明显圆润
- 建议按资源尺寸比例控制：短边的 `18%~26%` 作为视觉圆角（不是硬性像素）

### 4.2 描边与厚度（关键）
- “厚边框 + 内阴影 + 外投影”三件套必须同时存在
- 描边颜色优先用 `Ink.Brown` 或更深的同色系阴影色

### 4.3 阴影（统一参数区间）
> 不追求精确数值一致，但视觉语言必须一致。
- Drop Shadow：偏移 `(+0~+6, -6~-14)`，模糊 `10~24`，透明度 `0.18~0.30`
- Inner Shadow：偏移 `(+0~+2, -2~-6)`，模糊 `8~16`，透明度 `0.12~0.22`

---

## 5. 字体与文字表现（HUD 必须可读）
> 参考图是“泡泡粗圆字体 + 棕色阴影”。项目当前 TMP 字体是 `LiberationSans`，可用材质效果逼近风格。

### 5.1 HUD 数字（Coins/Lives/Speed/FAST）
- 文字颜色：白（`#FFFFFF`）
- Outline：深色描边（推荐 `Ink.Navy`），宽度 **0.18~0.25**
- Underlay：偏移 `(2,-3)`，Softness `0.28~0.38`，Dilate `0.02~0.06`（与 `../../Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/Docs/UI_GUIDE.md` 建议一致）
- 数字格式：窄空间采用 `K/M/B`（10k 起压缩），避免挤压导致跳字

### 5.2 大标题（Modal 标题）
- 建议优先使用 **图片标题条**（`orange_long_*` 或 `pill_bg*`）+ TMP 文字叠加
- 标题必须全大写（更像参考图）

---

## 6. 组件规范（对齐现有资源命名）
### 6.1 顶栏 HUD（TopBar 单行）
- 左侧：Shop（mint_square）/ Counter（hud_pill_dark_small）/ Level（hud_level_label_bg）
- 右侧：CurrencyBar（hud_pill_dark）+ Speed（mint_square）+ Settings（mint_square）
- **胶囊避让**：只作用在 TopRight Cluster（CurrencyBar/Speed/Settings），不缩整个 HUD

### 6.2 Square Button（`*_square_*`）
- 视觉：圆角方块 + 厚边框 + 高光 + 内阴影 + 外投影
- Normal：高光更明显，投影更“立”
- Pressed：整体略暗、投影更短（像被按下去）
- Disabled：去饱和 + 降对比 + 投影更弱

### 6.3 Long Button（`*_long_*` / CTA）
- 视觉：大胶囊条（可做标题条/主按钮）
- 文案叠加：白字 + 深色描边 + 轻 Underlay

### 6.4 价格按钮（`btn_price_green_*`）
- 视觉：绿色糖果胶囊，左侧金币图标 + 数字
- Pressed：投影变短 + 高光减弱
- Disabled：灰化且仍保留“厚度”

### 6.5 Toggle（`toggle_full_on/off`）
- 轨道：绿色 ON / 灰色 OFF（都要有内阴影）
- 旋钮：更亮一档，带高光和轻阴影

### 6.6 Tag（`tag_fast_*` / `tag_small_*`）
- 视觉：小胶囊标签，信息态偏蓝/青，危险态偏红/橙
- 文案：全大写，白字描边

### 6.7 Icon（`icon_*.png`）
- 同一套线条语言：厚轮廓 + 内高光 + 轻阴影
- 禁止细线/极简扁平风（会破坏参考图“玩具感”）

---

## 7. 组件“层级配方”（用于保证风格化一致）
> 你可以把这一节当成“生成时必须包含的视觉层”。如果某个资产缺层，整体质感会立刻掉档。

### 7.1 Button Square / Long（通用配方）
从下到上（建议顺序）：
1) **Base Fill**：垂直渐变（上亮下暗），对比不要过强（避免塑料变“金属”）
2) **Outer Stroke**：厚描边（同色系更深一档或 `Ink.Brown`），视觉上像“塑料外壳”
3) **Inner Rim**：一圈细亮边（提升“厚度/倒角”）
4) **Specular Highlight**：左上角柔高光弧（覆盖面积约 18%~28%，透明度 10%~20%）
5) **Inner Shadow (AO)**：四周轻微内阴影，底边略重（强化立体）
6) **Drop Shadow**：右下方向（偏下），必须包含在 PNG 透明边距内（不许被裁切）
7) **Grain**：2%~4% 细颗粒（统一材质细节）

三态规则：
- **Normal**：Drop Shadow 最清晰；Highlight 最明显
- **Pressed**：整体暗 6%~10%；Drop Shadow 缩短 40%~60%；Highlight 变弱
- **Disabled**：去饱和（饱和度约 25%~45%）；对比降低；Drop Shadow 透明度减半

### 7.2 Panel（Modal/Result/Thick）
1) **Base Fill**：奶油/蓝色大面，渐变非常柔（避免喧宾夺主）
2) **Frame**：厚边框（白/金），带轻高光与内阴影
3) **Inner Bevel**：内侧一圈细暗边（制造“嵌入”感）
4) **Corner Softness**：角部更软，避免“硬切圆角”的廉价感
5) **Subtle Noise**：极轻颗粒（1%~2%）

### 7.3 Tag / HUD Pill
1) Base Fill：同 Button，但对比更低（HUD 不抢戏）
2) Stroke：更薄一档（避免像按钮）
3) Inner Shadow：更弱（信息底板感）

### 7.4 Toggle
**Track**
1) Base Fill：ON 绿 / OFF 灰（上亮下暗）
2) Inner Shadow：必须明显（参考图的“凹槽感”）
3) Stroke：厚描边（更玩具）

**Knob**
1) Base Fill：更亮一档
2) Highlight：强一点（旋钮是“抛光塑料”）
3) Tiny Shadow：让旋钮“浮起来”

### 7.5 Icons / Digits
1) **Silhouette**：粗轮廓、圆润边
2) **Outline**：深色外描边（保证在奶油底/彩色底都清晰）
3) **Inner Shading**：极轻（让它不像纯扁平矢量）
4) **Micro Shadow**：非常轻，避免“贴图浮在空中”

---

## 8. 9-slice 与可缩放规则（必须）
> 资源生成时必须预留“边框不被拉伸”的安全区；保持与现有 `LoopSortingUIKitConfig.json` 一致，避免返工。
- `*_square_*`：border `170,170,170,170`
- `*_long_*`：border `140,140,90,90`
- `hud_pill_dark*.png`：border `40,40,30,30`
- `panel_modal.png / panel_result.png`：border `120,120,120,120`
- `panel_thick_gold_blue.png`：border `140,140,140,140`
- `btn_small_*`：border `80,80,55,55`
- `btn_price_green_*`：border `60,60,40,40`
- `card_setting_row.png`：border `70,70,50,50`
- `tag_fast_*`：border `60,60,40,40`
- `tag_small_*`：border `50,50,30,30`

---

## 9. 出图与导出规范（生产必看）
1) **严格像素尺寸**：必须与目标文件一致（见 Prompt Pack 里的尺寸表）
2) **透明背景**：除 `bg_main.png / overlay_dim.png` 外，其他均为透明 PNG
3) **阴影不出框**：投影必须留在 PNG 透明边距内，避免被裁切
4) **边缘无白边**：避免透明边缘被预乘导致白边；导出用 straight alpha
5) **三态成组**：Normal/Pressed/Disabled 必须同风格、同光照、同圆角体系

---

## 10. QA 验收清单（风格一致性）
- 同类按钮（mint/purple/orange）高光方向一致、厚度一致
- 所有可点元素“像被抬起”，Pressed 明显“被按下”
- 描边厚度在同类资产里保持一致（不忽粗忽细）
- HUD 数字在窄空间不挤压、不跳动（K/M/B 生效）
- 9-slice 缩放后边框不变形、圆角不被拉长
