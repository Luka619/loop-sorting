# Box Completed FX Redesign (V05) — “3D-ish / Juicy”

> 总索引：`../../README.md`

## 1) 目标（Goals）
- **更像 3D 的完成反馈**：看起来像“有厚度的盖子/玻璃盖”压在箱子上，而不是一张 2D 贴片。
- **不拉伸变形**：在不同尺寸/比例（1:1、横向长条、纵向长条）的 box 上都保持圆角与边缘不被拉扯。
- **匹配 box 大小**：覆盖范围精确贴合 box（或轻微外扩），不会漂浮/过大。
- **半透明**：完成后有“玻璃/亚克力”质感遮罩，能透出箱内元素，但有明显完成状态差异。
- **性能可控**：同屏多个完成箱子也不爆 drawcall/材质实例，尽量复用材质/贴图。

## 2) 现状（Current Implementation）
完成效果目前在 `Assets/Scripts/BoxView.cs` 内实现：
- 构建：`EnsureCompletedOverlayBuilt()`（`Assets/Scripts/BoxView.cs:1597`）
- 更新尺寸/颜色：`UpdateCompletedOverlayVisuals()`（`Assets/Scripts/BoxView.cs:1657`）
- 动画/粒子：`PlayCompletedFx()`（`Assets/Scripts/BoxView.cs:1867`）

当前层级大致是：
- `FrameGlow`（发光框）
- `Glass`（玻璃遮罩）
- `Badge`（对勾）
- Confetti 粒子

问题根因（你看到的“拉伸严重/像没 9-slice”）通常来自两点：
1) **贴图本身不适合 9-slice**：高光/阴影跨越边界、边缘纹理在中心区域出现，导致拉伸后视觉崩。
2) **实现方式不等价于 Unity 原生 9-slice**：目前是“按比例切 UV 的九宫格 mesh”，但“角/边在世界尺寸上会跟着 box 线性变大”，在大尺寸 box 上仍会显得被拉伸。

## 3) 设计方向（Visual Direction）
核心把“完成态”做成一个 **3D-ish 盖板套件**，由多个层叠出厚度与材质：

### Layer A：Cap（厚边框 / 立体外壳）
- 视觉：厚实圆角塑料/金属边框，有上边高光、下边阴影，像一个“框架”盖在箱体上。
- 要求：**强 9-slice 友好**（角=完整圆角+厚度；边=可沿长度拉伸的均匀纹理；中心=基本平坦或轻微渐变）。
- 颜色：可用 box 的主题色做轻微 tint（更像“完成”）或保持统一金色系。

### Layer B：Glass（半透明玻璃/亚克力盖）
- 视觉：偏奶油/玻璃的柔和透光层，带一点内阴影/边缘折射感。
- 透明度：建议 `alpha 0.18 ~ 0.28`（按关卡背景/箱体对比调）。
- 可选：叠加一层轻微噪点/细纹，提升“真实材质”。

### Layer C：Shadow / AO（贴合 box 的投影/环境遮蔽）
- 视觉：盖板对箱体的压暗（尤其边缘），让它“压在上面”。
- 实现：单独一层软阴影贴图（Multiply/Alpha darken）或用一张 AO 贴图叠在 Glass 下方。

### Layer D：Badge（立体徽章）
- 视觉：对勾徽章做成“厚 coin / badge”，带阴影与高光，出现时有轻微旋转+弹性。
- 位置：右上角偏内（避免贴边），随 box 大小自适应。

### Layer E：Light Sweep（高光扫过，可选）
- 视觉：一条斜向高光扫过玻璃/外壳，强化 3D 质感。
- 实现：独立 additive 材质的一张“条纹高光”贴图，UV/位移动画。

### Layer F：Particles（Confetti + Sparkles）
- 保留现有 confetti（已有 `vfx_confetti_*`），可新增少量 sparkles（短寿命、少数量）。

## 4) 动画节奏（Timeline）
建议“更像 3D”的关键不是更复杂，而是**有重量**：

1) **Cap+Glass 落下**（0.00s → 0.18s）
   - Scale：`0.92 → 1.02 → 1.00`（EaseOutBack）
   - Alpha：`0 → 1`
   - 轻微下压（可选）：让 box 自身 y 方向 squash 1–2 帧（更“压住”）。

2) **Badge 弹出/盖章**（0.08s → 0.26s）
   - Scale：`0 → 1.12 → 1.00`
   - Rotation：`-10° → 0°`（或小幅随机）
   - 同步“咚”的音效/轻微震动（如果有）。

3) **Light Sweep**（0.12s → 0.60s，可选）
   - 斜高光从左上扫到右下，透明度低（0.15~0.25）。

4) **Confetti**（0.00s → 1.65s）
   - 保持目前 burst 的节奏即可。

## 5) 技术实现建议（Implementation Plan）
### 5.1 从“自定义 9-slice mesh”迁移到“SpriteRenderer Sliced”
参考 lock overlay 的实现（`Assets/Scripts/BoxView.cs:746`）：
- 使用 `SpriteRenderer.drawMode = Sliced` + `sr.size = boxSize`，Unity 会保证角不被拉伸。
- 用更高的 `pixelsPerUnit`（比如 500）让边框厚度在世界单位里稳定（小 box 也不糊）。

建议完成态也改成 SpriteRenderer（Cap/Glass/Shadow）三层：
- `CompletedCapSR`：Sliced
- `CompletedGlassSR`：Sliced
- `CompletedShadowSR`：Sliced（或 Simple 也可）
- `CompletedBadge`：可继续用 Quad + Unlit，或 SpriteRenderer Simple

### 5.2 资源加载/9-slice 规则
`LoopSortingUIKit.LoadSprite(..., applyNineSlice: true)` 会使用配置里的 `nineSliceRules` 来填 border（`Assets/Scripts/LoopSortingUIKit.cs:316`）。
因此需要：
- 给新资源命名成可匹配规则（建议 `*_9slice.png`），并在 `LoopSortingUIKitConfig.json` 的 `nineSliceRules` 里新增对应 pattern。

### 5.3 材质/渲染排序
沿用当前渲染层级（`Assets/Scripts/BoxView.cs:69`）：
- Completed：queue 3000
- Lock：queue 3100
- Badge：queue 3200

Cap/Glass/Shadow 可分别用：
- Cap：3000
- Glass：3001
- Shadow：2999（在 Glass 下方，但仍在 block 上方）
- Badge：3002

> 注意：透明物体排序容易抖动，推荐用 **renderQueue + sortingOrder** 双保险。

### 5.4 参数建议（可调范围）
- Cap alpha：0.9~1.0（不透明）
- Glass alpha：0.18~0.28
- Shadow alpha：0.10~0.22
- 外扩：Cap `1.02~1.06`，Glass `1.00~1.02`（先贴合再微调）

## 6) 资源清单（Assets）
建议新增一套更 3D-ish 且 9-slice 友好的资源（512/1024 两档）：
- `World_Sprites/box_completed_cap_9slice_512.png`
- `World_Sprites/box_completed_cap_9slice_1024.png`
- `World_Sprites/box_completed_glass_9slice_512.png`
- `World_Sprites/box_completed_glass_9slice_1024.png`
- `World_Sprites/box_completed_shadow_9slice_512.png`（可选）
- `World_Sprites/box_completed_badge_coin_256.png`（或 512）
- `World_Sprites/box_completed_sweep_256.png`（可选）

**9-slice 友好约束（非常重要）**
- 角部圆角与厚度必须完全落在 border 区域内（不要把关键高光放到中心区）。
- 边缘纹理必须沿长度方向可拉伸、沿厚度方向保持稳定。
- 中心区域尽量平滑/低频，避免明显纹理被拉伸。

## 7) 生成提示词（API易 / gpt-image-1.5）
用于生成 9-slice 贴图时，提示词要“约束构图”，否则很容易产生不可切片的高光。

Cap（9-slice）正向要点：
- “orthographic front view / no perspective”
- “rounded square frame with thick bevel / consistent corner radius”
- “edges uniform, center flat”
- “highlights confined to corner+edge border area”
- “transparent background, no vignette”

负向要点：
- “no global gradient across whole image”
- “no background, no glow bleeding to image edge, no cropping”

## 8) 验收标准（Acceptance）
- 任意 box 比例（1:1、2:1、1:2）下，圆角不会被拉扯成椭圆/厚度不均。
- Glass 遮罩可见但不遮挡玩法信息（alpha 合理）。
- Badge 有厚度感（阴影/高光明显），动效有重量不轻飘。
- 同屏 10+ 个完成箱子仍稳定（材质/贴图复用，避免每个实例 new Material）。

## 9) 下一步（Recommended Next Steps）
1) 先产出 **Cap+Glass** 两张 9-slice 友好贴图（512 先跑通），在 3 种 box 比例上测试。
2) 再补 Shadow/AO（如果“压住”的感觉还不够）。
3) 最后做 sweep（可选）与 badge 的 3D 化（coin badge）。
