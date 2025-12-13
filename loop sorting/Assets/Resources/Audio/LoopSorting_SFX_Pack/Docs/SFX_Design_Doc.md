# Loop Sorting 音效设计文档（SFX Design Doc）

生成日期（UTC）：2025-12-13

本包为 **Loop Sorting** 的“玩法反馈型音效（SFX）”提供一套可直接接入引擎的基础资产与实现建议。音色定位为：**轻工业 / 玩具机械 / 低侵入的解谜质感**，核心目标是让玩家在“点击出货—传送带轮转—口对口入箱—解锁/完成—胜负”全链路上获得清晰、即时、层次分明的反馈。

---

## 1. 玩法音频目标

### 1.1 清晰（Readability）
- 让玩家仅凭声音也能区分：**出货开始、单个积木出货、成功入箱、被规则阻止/失败入箱、隐藏色揭示、箱体完成、锁箱解锁、满槽风险**。
- 对“高频事件”（传送带 tick、连续出货）必须做 **限声、随机与混合策略**，避免听觉疲劳与机枪效应。

### 1.2 满足（Satisfaction）
- 重要里程碑（箱体完成、解锁、胜利）使用更“音乐化”的上行结构，让成就感明显高于常规交互。
- 常规交互（点击、出货、入箱）更“机械”，强调触感与物理性。

### 1.3 压力（Tension）
- “满槽”是核心失败压力源之一：建议在检测到满槽与触发 **5x 快进一圈** 的阶段加入警告与速度过渡，形成节奏突变与紧迫感。

---

## 2. 总体混音与系统设计

### 2.1 Mixer 分组（建议）
- **UI**：按钮、弹窗、拒绝
- **SFX**：交互、箱体、积木、道具、状态提示
- **Ambience**：传送带底噪 loop（可选）
- （可选）**Music**：BGM（本包不提供）

### 2.2 典型处理（建议）
- **随机化**（对高频事件）：音量 ±1 dB、Pitch ±2%（或半音 0.1~0.2），随机挑选变体。
- **限声（Voice Limit）**：
  - conveyor.tick：建议 1~2（或直接使用 conveyor.loop，tick 仅用于强调关键帧）
  - block.eject：建议 6~10（由 run 长度与出货速度决定）
  - block.insert / reject：建议 3~6
- **优先级**：
  - 胜利/失败 > 解锁 > 箱体完成 > 满槽警告 > 道具结果 > 入箱/出货 > UI > 传送带底噪
- **Ducking**：
  - 在 box.complete / box.unlock / win / lose 期间，对 conveyor.loop 做 -6~-10 dB 的短暂 duck（300~800ms）。

### 2.3 5x 快进的音频策略（关键）
当规则进入“满槽快进”或“道具前置跑圈”的 5x 阶段时：
1. 播放 **conveyor.speedup** 作为过渡（一次）。
2. conveyor.loop 可在 200ms 内做 **Pitch 上扬**（例如 +3~+5 半音）并略增音量（+1~+2 dB），制造速度感。
3. 同时使用 **belt_full_warning**（一次或循环间隔播放）提示风险，但不要过于刺耳，以免疲劳。

---

## 3. 事件—音效映射（Manifest）

> 文件结构：`SFX/<Folder>/<file>.wav`  
> 变体：带 `_01/_02/_03` 后缀（若 variations>1）

| Event ID | 分类 | 文件（示例） | 变体数 | Loop | Bus | Priority | VoiceLimit | 说明 |
|---|---|---|---:|:---:|---|---:|---:|---|
| `ui.click` | UI | `SFX/UI/ui_click_01.wav` | 3 | 否 | UI | 40 | 6 | 通用按钮点击，支持轻微pitch随机。 |
| `ui.confirm` | UI | `SFX/UI/ui_confirm.wav` | 1 | 否 | UI | 45 | 4 | 确认/继续/下一关。 |
| `ui.cancel` | UI | `SFX/UI/ui_cancel.wav` | 1 | 否 | UI | 45 | 4 | 取消/返回。 |
| `ui.popup_open` | UI | `SFX/UI/ui_popup_open.wav` | 1 | 否 | UI | 45 | 2 | 弹窗打开。 |
| `ui.popup_close` | UI | `SFX/UI/ui_popup_close.wav` | 1 | 否 | UI | 45 | 2 | 弹窗关闭。 |
| `ui.denied` | UI | `SFX/UI/ui_denied.wav` | 1 | 否 | UI | 55 | 2 | 不可操作/条件不满足时。 |
| `box.select` | Gameplay | `SFX/Gameplay/box_select.wav` | 1 | 否 | SFX | 55 | 3 | 玩家点击箱体（有效选择）。 |
| `box.locked_thunk` | Gameplay | `SFX/Gameplay/box_locked_thunk.wav` | 1 | 否 | SFX | 60 | 2 | 点击锁箱或尝试对锁箱进/出货时。 |
| `run.ship_start` | Gameplay | `SFX/Gameplay/run_ship_start.wav` | 1 | 否 | SFX | 60 | 1 | 箱体开始出货 run（出货启动）。 |
| `block.eject` | Gameplay | `SFX/Gameplay/block_eject_01.wav` | 3 | 否 | SFX | 50 | 8 | run中每个积木出货到传送带。 |
| `block.insert` | Gameplay | `SFX/Gameplay/block_insert_01.wav` | 2 | 否 | SFX | 55 | 6 | 积木成功入箱。 |
| `block.reject` | Gameplay | `SFX/Gameplay/block_reject_01.wav` | 2 | 否 | SFX | 55 | 4 | 积木尝试入箱但失败（busy/锁/满/颜色不符/规则阻止）。 |
| `block.skip_empty_box` | Gameplay | `SFX/Gameplay/block_skip_empty_box.wav` | 1 | 否 | SFX | 45 | 6 | 空箱延后规则触发：积木不进入空箱而继续前进（可选提示）。 |
| `hidden.reveal` | Gameplay | `SFX/Gameplay/hidden_reveal.wav` | 1 | 否 | SFX | 65 | 2 | 隐藏色run暴露到最外层并解除隐藏。 |
| `box.complete` | Gameplay | `SFX/Gameplay/box_complete.wav` | 1 | 否 | SFX | 80 | 1 | 箱体达成“完成箱体（满且同色）”。 |
| `box.unlock` | Gameplay | `SFX/Gameplay/box_unlock.wav` | 1 | 否 | SFX | 85 | 1 | 锁箱解锁（满足unlockColor）。 |
| `conveyor.tick` | Conveyor | `SFX/Conveyor/conveyor_tick_01.wav` | 2 | 否 | SFX | 35 | 2 | 离散tick可用；若tick过密建议用loop替代/混合。 |
| `conveyor.loop` | Conveyor | `SFX/Conveyor/conveyor_loop.wav` | 1 | 是 | Ambience | 20 | 1 | 传送带运转底噪loop（2s无缝）。 |
| `conveyor.speedup` | Conveyor | `SFX/Conveyor/conveyor_speedup.wav` | 1 | 否 | SFX | 60 | 1 | 进入5x快进（满槽或道具前置跑圈）时的过渡。 |
| `conveyor.full_warning` | Conveyor | `SFX/Conveyor/belt_full_warning.wav` | 1 | 否 | SFX | 75 | 1 | 检测到满槽并触发快进一圈前/期间的警告提示。 |
| `booster.activate` | Booster | `SFX/Boosters/booster_activate.wav` | 1 | 否 | SFX | 70 | 1 | 道具点击后触发（输入暂停）。 |
| `booster.fill_sort` | Booster | `SFX/Boosters/booster_fill.wav` | 1 | 否 | SFX | 75 | 1 | 完成颜色（Fill/Sort）执行成功。 |
| `booster.shuffle` | Booster | `SFX/Boosters/booster_shuffle.wav` | 1 | 否 | SFX | 75 | 1 | 打乱顺序（Shuffle）执行。 |
| `booster.fail` | Booster | `SFX/Boosters/booster_fail.wav` | 1 | 否 | SFX | 65 | 1 | 道具无法执行/无有效目标时（可选）。 |
| `level.start` | State | `SFX/States/level_start.wav` | 1 | 否 | SFX | 60 | 1 | 关卡开始/进入关卡。 |
| `level.win` | State | `SFX/States/win_jingle.wav` | 1 | 否 | SFX | 95 | 1 | 胜利：所有箱体满足条件且传送带为空。 |
| `level.lose` | State | `SFX/States/lose_jingle.wav` | 1 | 否 | SFX | 95 | 1 | 失败：满槽快进一圈后仍满。 |


---

## 4. 关键交互的触发时机建议

### 4.1 玩家点击箱体出货（run）
- **box.select**：玩家点击且该箱体可出货时立刻播放。
- **run.ship_start**：箱体进入 Busy=true 并开始逐个出货时播放（一次）。
- **block.eject(_xx)**：run 中每个积木从箱体弹出到传送带时播放（高频，务必变体+限声）。

### 4.2 传送带 tick 与口对口入箱
- **conveyor.loop**：传送带开始轮转时淡入；停止时淡出（Loop 资产为 2s 无缝）。
- **conveyor.tick(_xx)**：如果你想强调“离散 tick”逻辑（而不是连续运动），可在每个 tick 播放；若 tick 频率很高，建议降低播放频率（比如每 2~3 tick 播一次）或改用 loop。
- **block.insert(_xx)**：积木满足入箱条件、并实际落位（填充最内侧空位）时播放。
- **block.reject(_xx)**：积木到达口对口检测点，但由于锁箱/Busy/满/颜色不符/优先级规则而不入箱时播放（避免玩家误以为“没检测到”）。
- **block.skip_empty_box**：仅当“空箱延后”规则触发且你希望玩家获得轻提示时使用；若你更偏向“无声规则”，可不接入该事件。

### 4.3 隐藏色揭示
- **hidden.reveal**：当隐藏 run 暴露为最外层并解除隐藏时播放（一次即可）。

### 4.4 锁箱与解锁
- **box.locked_thunk**：玩家对锁箱进行任何交互尝试时播放（反馈“不可用”但不要太挫败）。
- **box.unlock**：满足 unlockColor 的完成触发解锁时播放（应明显且令人愉悦）。

### 4.5 关卡胜负
- **level.win**：满足胜利条件并进入下一关前播放（可与 UI 过场叠加）。
- **level.lose**：判定失败、弹出“重试”弹窗时播放，同时触发 UI.popup_open。

### 4.6 道具（Boosters）
- **booster.activate**：点击道具立即播放（输入暂停的提示）。
- （若传送带非空）进入 5x 跑圈阶段时按 2.3 方案处理。
- **booster.fill_sort / booster.shuffle**：道具逻辑完成、结果落地时播放。
- **booster.fail**：无可作用对象或被规则限制时播放（可选）。

---

## 5. 交付资产清单

- 采样率：44.1kHz
- 格式：16-bit PCM Mono WAV
- 风格：轻机械 / 电子点按 / 低侵入
- 备注：所有音效均为程序合成，可自由修改与替换。

