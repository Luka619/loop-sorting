# Loop Sorting 设计文档（总览）

## 核心玩法
- 单向循环传送带，槽位匀速前进；首尾不重合则不勾选 loop，闭合后才开启 loop。
- 多个箱体：只能操作最外层连续同色的积木；出货口被占时等待；同一时刻仅一个箱体出货，进货不受限。
- 进货：经过开口且颜色匹配/为空/未满时进入，填充最内侧空位；隐藏色整段共用隐藏状态，最外层暴露则整段显现。
- 胜利：所有非空箱为单一颜色且已填满，传送带为空。失败：传送带满且无容纳空间。

## 主要系统
- 数据：`BlockColor`/`Block(hidden)`；`Container`（Peek/Pop/Push/CanAccept，锁定状态）；`Conveyor`（槽位数组、阻塞等待）；`LoopSortingGame`（出/入货、判胜）；`GameRuntimeController`（场景可视化、点击出货协程、槽位插值、计数器、胜负与 Flow）。
- 布局：`LevelLayout`（传送带点集、平滑参数、slot spacing、belt capacity、blockSize、boxes）；`BoxSpec`（columns×rows=容量，开口，autoAlign/slotIndex，colorCounts[含hidden]，locked/unlockColor）；`LevelFlow`（关卡序列+startIndex）；`LevelRuntimeConfig`（activeLevel 或 activeFlow+startIndex）。

## 视觉与层级
- 箱体：黑色可操作描边 > 锁/完成覆盖 > 白色虚线轮廓 > 背景；开口侧虚线留空，方向可辨。
- 传送带：槽位可见灰色半透明，平滑转角；积木位置插值与槽位同步。
- UI：背景 < 玩法 < 半透明遮罩 < 按钮/文字 < 顶层提示。HUD 含空余计数器、速度按钮、设置面板、结果面板、Booster 按钮。

## 道具
- 完成色（Fill）：仅作用未完成且可被填满的颜色；不生成新块；锁箱与完成箱不动。
- 乱序（Shuffle）：仅未完成且未锁箱；按连续色段洗牌，保持最小连续行约束；锁/完成箱及传送带在运行前模拟一圈后再处理。

## 关卡规划
- 具体 30 关节奏与第 3 关设计详见 `LEVELS.md`。
