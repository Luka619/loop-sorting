# Loop Sorting 设计文档（最新版）

## 核心玩法
- 多个箱体 + 循环单向传送带。传送带按固定节奏均匀前进（首尾相连）。
- 点击箱体：最外层连续同色的积木依次出货；出口槽占用则等待，有空位继续；同一时刻只允许一个箱体出货。
- 入货：带子经过箱口槽位时，若箱体为空或最外层颜色匹配且未超容量，积木进入箱体；进入时填充到“最内层”未占用的位置（列表尾部）。
- 目标：每个箱体内部颜色统一且填满判定为胜利；若传送带满且没有任何箱体可以接收带上的任意积木则失败。

## 数据结构（运行时）
- `BlockColor` / `Block`：颜色枚举与只读块。
- `Container`：容量，最外层在索引0；`TryPeek/TryPop` 取最外层；`TryPush` 追加到尾部（最内层）；`CanAccept` 判断颜色/容量。
- `Conveyor`：循环槽位数组，`Advance(blockedPort?)` 支持出口前半段阻塞等待出口腾空。
- `LoopSortingGame`：容器与传送带协调，`TryReleaseFromContainer` 放块到指定槽位。
- `GameRuntimeController`：场景驱动；生成槽位/箱体可视化；处理点击出货协程、槽位插值、计数器、胜败判定、关卡流。
- `LayoutUtils`：槽位生成（等弧长、可局部圆角）、边界计算、槽位对齐。

## 关卡数据
- `LevelLayout`：传送带路径点（首个 conveyor 使用）、beltCapacity、beltSlotSpacing、平滑开关与参数、blockSize（格子边长）、箱体列表。
- `BoxSpec`：位置、列×行容量、开口方向、autoAlignSlot（最近槽位或手动 beltSlotIndex）、colorCounts（颜色+数量，外层→内层）。
- `LevelFlow`：关卡序列与 startIndex，用于按顺序播放关卡。
- `LevelRuntimeConfig`：可配置 activeLevel 或 activeFlow+flowStartIndex。

## 编辑器
- `LevelEditorWindow`：
  - Tabs：Levels / Flow。
  - Levels：左侧预览（槽位、网格、开口连线、槽位编号），右侧参数；关卡列表按钮网格快速切换；保存时若 autoAlign 会写回 beltSlotIndex；可调 beltCapacity、beltSlotSpacing、平滑参数。
  - Flow：选择/新建 LevelFlow，ReorderableList 维护关卡序列与起始索引，可一键设为运行 Flow（写入 LevelRuntimeConfig）。
  - 场景视图可拖拽传送带点、箱体位置；可选网格吸附。

## 运行时可视化与 UI
- 槽位标记：灰色半透明球，随 tick 在相邻槽位间插值（末槽不回跳）；槽位分布与编辑器一致。
- 积木：位置每帧对齐当前槽位插值坐标（含 z 偏移），与槽位同步平滑移动。
- 箱体：网格静态；出货/进货只更新相应槽位，已有积木不移动。
- 背景：世界空间渐变 Quad，放在相机远处（Background renderQueue）。
- HUD：空余计数器、速度按钮（1x/1.5x/2x）；结果面板（胜利/失败，Next/Retry/Close）；可通过 `UITheme` 配置字体、按钮皮肤、颜色、渐变。
- EventSystem 自动创建；HUD 在 Overlay Canvas，不遮挡玩法。

## 进/出货规则细节
- 出货：点击箱体，计算最外层连续同色 pending，逐块尝试放到出口槽；若槽占用则等待 `releaseBlockedRetry`，成功后等待 `releaseInterval`；出货中其他箱体不出货。
- 传送带阻塞：当出口槽占用时，索引小于出口的块若前方空则前进，前方是出口则等待；其他索引保持常规前进。
- 入货：经过箱口槽位时颜色匹配/空则进；填充箱体内部最内层空位。

## 关卡流与胜败逻辑
- 胜利：所有容器颜色统一且填满，弹出结果面板；Flow 中有下一关则 Next 进入下一关，否则可 Retry。
- 失败：传送带满且没有任何容器可接收带上任意积木时失败，弹窗 Retry。
- Flow 启动顺序由 `LevelRuntimeConfig.activeFlow` + `flowStartIndex` 或单关卡 `activeLevel` 决定。

## 注意/可调项
- 槽位平滑、槽距、容量、blockSize、速度倍率、出货间隔/阻塞重试等参数可在关卡或控制器中配置。
- 若槽位分布异常，检查路径点间距、平滑参数、容量与槽距设置。
- UI 主题可通过 `UITheme` 统一替换字体/按钮/背景渐变。
