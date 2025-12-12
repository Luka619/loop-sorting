# Loop Sorting 设计文档（最新）

## 核心玩法
- 多个容器（Box）+ 单向循环传送带。传送带按固定节奏均匀前进，首尾相连。
- 点击容器：最外层“连续同色”的积木批量出货；出口槽位被占用则等待；同一时刻只允许一个容器出货。未出货的积木保持原位。
- 入货：传送带经过容器出口时，如果容器为空或最外层颜色匹配且未超容量，则积木进入容器，填充到最内层未占用的槽位。隐藏积木的显隐：同色连续段共享隐藏状态，只要段内有积木暴露在最外层，该段全部显色。
- 目标：每个容器内部单一颜色且填满即胜利；若传送带满且没有任何容器可接受带上的任意积木则失败。

## 数据结构（运行时）
- `BlockColor` / `Block`：颜色枚举与只读块（含 Hidden）。
- `Container`：容量、最外层在索引 0；`TryPeek/TryPop` 取最外层；`TryPush` 追加到最内层；`CanAccept` 判断颜色/容量；支持锁定状态（锁定时不能出/入/移除/清空）。
- `Conveyor`：循环槽位数组，`Advance(blockedPort?)` 支持出口前半段阻塞等待出口腾空。
- `LoopSortingGame`：容器与传送带协调，`TryReleaseFromContainer` 把块放到指定槽位；`IsSolved(requireFull)` 判胜。
- `GameRuntimeController`：场景驱动；生成槽位/容器可视化；处理点击出货协程、槽位插值、计数器、胜负判定、关卡流、背景/UI/booster。
- `LayoutUtils`：槽位生成（等弧长、可局部圆角）、边界计算、槽位对齐。

## 关卡数据
- `LevelLayout`：传送带路径点（首个 conveyor 使用）、beltCapacity、beltSlotSpacing、平滑开关与参数、blockSize（格子边长）、boxes 列表。
- `BoxSpec`：位置、列×行容量、开口方向、autoAlignSlot（最近槽位或手动 beltSlotIndex）、colorCounts（外层→内层，含 hidden）、locked + unlockColor、auto计算 size。
- `LevelFlow`：关卡序列与 startIndex，用于按顺序播放。
- `LevelRuntimeConfig`：可配置 activeLevel 或 activeFlow+flowStartIndex。

## 编辑器（LevelEditorWindow）
- 固定布局：左侧常驻关卡列表，右侧参数区；可拖拽调整容器/传送带点位，网格吸附可选；槽位预览与运行时一致。
- Levels：列表快速切换关卡；保存时若 autoAlign 会写回 beltSlotIndex；可配置 beltCapacity、beltSlotSpacing、平滑参数、blockSize。
- Flow：维护 LevelFlow 序列（ReorderableList），选择起始关，设置为当前运行流（写入 LevelRuntimeConfig）。
- Boxes：颜色与数量统一在 colorCounts 中配置；支持隐藏标记（H，仅显示标注，不在编辑器隐藏）、锁定标记（L，参数区配置 unlockColor）；创建时自动命名“Box N”；可删除/拖拽位置，快捷键删选中。
- 传送带：点位可拖拽/数值编辑/增删；点击范围与可视宽度匹配；平滑可开关；槽位分布与运行时一致。

## 运行时可视化与 UI
- 槽位标记：灰色半透明球，随 tick 在相邻槽位间插值（末槽不回跳）；槽位分布与编辑器一致。
- 积木：位置每帧对齐当前槽位插值坐标（有 z 偏移），移动与槽位同步。
- 容器：静态网格；出货/进货只更新对应槽位，已有积木不移动；锁定容器全灰遮罩，中心彩色徽章显示解锁所需颜色（遮罩在积木前，徽章在遮罩前）。
- 背景：相机远端的渐变 Quad（Unlit，RenderQueue Background），随相机裁剪适配。
- HUD：空余计数器、速度按钮（1x/1.5x/2x）、设置面板（振动/声音/关闭）、结果面板（胜/败，Next/Retry/Close），booster 两个按钮（完成颜色/打乱顺序）。`UITheme` 可替换字体/按钮皮肤/颜色。

## 出货/入货规则细节
- 出货：点击容器，计算最外层连续同色数量 pending，逐块尝试放到出口槽；若槽占用则等待 `releaseBlockedRetry`，成功后等待 `releaseInterval`；出货中其他容器不出货。
- 传送带阻塞：当出口槽占用时，索引小于出口的块若前方为空且不是出口则前进，前方是出口则等待；其他索引保持常规前进。
- 入货：经过容器出口槽位时若颜色匹配/容器为空且未满则进入，填充容器内部最内层空位。
- 隐藏：同色连续段共享 Hidden，若段包含最外层则整段显色。

## 关卡流与胜败逻辑
- 胜利：所有容器颜色统一且填满，弹出结果面板；Flow 有下一关则 Next 进入，否则仅 Retry/Close。
- 失败：传送带满且没有任何容器可接收带上的任意积木时失败，弹窗 Retry。
- Flow 启动顺序：`LevelRuntimeConfig.activeFlow` + `flowStartIndex` 或单关 `activeLevel` 决定。

## Booster 规则
- 完成颜色（Fill）：忽略传送带，仅操作未锁定容器；只在有足够数量可以完全填满某个未锁定容器的颜色时可选；抽取目标色块填满一个目标容器，不生成新块；锁定容器及其积木不动；若数量不足则放弃并还原当前关。
- 打乱顺序（Shuffle）：仅打乱未完成且未锁定容器，按同色连续段为单位洗牌分配；锁定容器和传送带保持不变；保证段数递减不会拆到低于原有最小连续行数（已实现最小 1 行约束）。

## 注意/可调项
- 可调：槽位平滑、槽距、容量、blockSize、速度倍数、出货间隔/阻塞重试、背景/按钮皮肤等。
- 若槽位分布异常，检查路径点距离、平滑参数、容量与槽距设置。
- UI 主题可通过 `UITheme` 统一替换字体/按钮/背景渐变。
