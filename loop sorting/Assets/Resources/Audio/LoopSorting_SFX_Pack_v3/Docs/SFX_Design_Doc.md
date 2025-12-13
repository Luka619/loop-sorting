# Loop Sorting 音效设计文档 v3

版本日期：2025-12-13

本文档用于把《Loop Sorting 设计文档（系统总览）》中的关键规则、关键反馈点，转换成可落地的音频事件（Events）、资源组织（Assets）、以及混音/工程参数（Priority / Polyphony / Cooldown / Ducking）建议。

---

## 1. 设计目标

1. **可读性**：用声音把“规则结果”说清楚，尤其是入箱失败原因（锁箱/Busy/满箱/颜色不符/空箱延后）。
2. **手感**：点击箱体、出货 run、积木落槽、入箱成功，这些高频反馈必须“干净、短促、有弹性”。
3. **节奏与压力**：传送带 tick 与满槽 5x 快进是你游戏的节奏骨架；需要从“正常—加速—失败”形成可听化曲线。
4. **不疲劳**：高频事件（eject/tick/insert）必须做变体、限声、冷却，避免堆叠失真与听觉疲劳。

---

## 2. 声音语言（风格基调）

- **UI**：短、亮、干净（<200ms）。
- **积木交互**：偏“木质/塑料积木”的 clack/thunk，中频清晰。
- **规则阻止**：闷、下行、短促（让玩家直觉“被挡住”）。
- **里程碑（完成/解锁/胜负）**：更音乐化、上行结构，并对环境声做短暂 ducking。

---

## 3. 事件总表（见 Manifest）

事件与文件的完整映射见：
- `Docs/SFX_Manifest.csv`
- `Docs/SFX_Manifest.json`

---

## 4. 关键事件的触发时机建议

### 4.1 玩家输入与 UI

- `ui.click`：任意按钮点击
- `ui.hover`：悬停/焦点变化（可选）
- `ui.confirm / ui.cancel`：确认/取消
- `ui.popup.open / ui.popup.close`：弹窗打开/关闭（胜负弹窗、道具面板等）
- `ui.denied`：不可操作（点击锁箱/完成箱、无效按钮等）

### 4.2 箱体与 run（出货）

- `box.select`：玩家点到有效箱体（准备出货）
- `run.ship.start`：run 开始逐个出货时触发（进入“自动连续出货”状态）
- `block.eject`：每一个积木从箱体弹出并进入传送带（高频，务必限声+变体）
- `block.land`：积木落到传送带槽位（建议在表现层“落槽”关键帧触发）
- `run.ship.end`：run 结束（可选，但能明显提升“过程闭环感”）
- `box.busy.denied`：出货期间 Busy 状态产生的拒绝反馈（可用于入箱失败原因或玩家误触）

### 4.3 传送带（Tick / Loop / 速度变化）

- `conveyor.loop`：持续底噪（建议只允许 1 实例）
- `conveyor.tick`：离散 tick 的轻提示（强烈建议做 **冷却** 或按比例播放，否则会刷屏）
- `conveyor.speedup / conveyor.speeddown`：进入/退出 5x（满槽快进、道具跑圈）
- `conveyor.full.warning`：检测到满槽时（进入压力态）
- `conveyor.full.fail`：快进跑一圈仍满时（在 `level.lose` 前作为“原因提示”）

### 4.4 入箱判定（可读性核心）

- `block.insert`：成功入箱（建议比 eject 更“确定、更结实”）
- `block.reject.*`：失败入箱的原因分型（品质提升非常明显）
  - `block.reject.locked`
  - `block.reject.busy`
  - `block.reject.full`
  - `block.reject.mismatch`
- `block.skip_empty_box`：空箱延后规则触发时的轻提示（可作为可选开关）

### 4.5 规则里程碑

- `hidden.reveal`：隐藏色 run 暴露时（信息揭示）
- `box.complete`：箱体完成
- `box.unlock`：锁箱解锁
- `level.win / level.lose`：胜负结算
- `level.retry / level.next`：重试/下一关按钮

---

## 5. 混音与工程参数（强建议）

### 5.1 Bus 分组

- **UI Bus**
- **Gameplay Bus**（box/run/block）
- **Conveyor Bus**
- **Stinger Bus**（complete/unlock/win/lose）

### 5.2 Ducking（压制背景，突出关键反馈）

当以下事件触发时，对 **Conveyor Bus** 做短暂 duck（-6 ~ -10 dB，300~800ms）：
- `box.complete`
- `box.unlock`
- `level.win`
- `level.lose`

### 5.3 高频事件的限声与冷却

- `conveyor.loop`：polyphony=1
- `conveyor.tick`：建议 cooldown ≥ 80ms，或按 2~3 tick 播一次
- `block.eject / block.insert`：polyphony 允许略高，但要避免同一帧堆叠

---

## 6. “播放但听不到”的排查清单

1. **路径/事件名是否一致**（事件触发了，但指向的文件不存在）
2. **并发上限抢占**（tick/eject 抢占了 insert/reject）
3. **Mixer 总线被静音或 Ducking 过度**
4. **3D 音频距离衰减**：UI 声音强烈建议用 2D
5. **设备低频缺失**：本包已避免“只有超低频”的设计，但你仍可能在引擎侧加了低通/滤波导致听感变弱

---

## 7. 版本说明（v3 相对 v2）

- 新增：`conveyor.speeddown`、`belt_full_fail`
- 新增：`block.land`（落槽反馈）
- 新增：`run.ship.end`
- 新增：细分 reject（locked/busy/full/mismatch）
- 新增：`ui.hover`、`box_select_02`、`level_retry`、`level_next`
- 更新：整体频段更偏中频可闻，适配手机/小音箱

