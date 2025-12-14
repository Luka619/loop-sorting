# Loop Sorting - BGM 设计文档（v1 原型）

> 说明：本 BGM 包为“可直接接入与验证”的原型资产（合成音色），用于你把 **BGM 系统、状态切换、ducking、音量标定** 先跑通。
> 后续若你要更高的商业质感，建议在不改事件/工程结构的前提下，替换为更高保真（录音/软音源/专业混音）的最终音乐。

---

## 1. 设计目标

1) **降低重复操作的疲劳感**：玩家在“点击箱体 → 出货 run → 传送带轮转 → 入箱判定”的循环中长时间游玩，需要稳定的背景音乐承托节奏。  
2) **不掩盖关键规则音效**：你的入箱条件、空箱延后、满槽 5x 等规则需要强可读性，BGM 必须让位给 SFX。  
3) **用音乐表达系统状态**：尤其是“满槽压力 / 5x 快进 / 解锁与完成 / 胜负结算”。

---

## 2. 风格方向（建议）

- **基调**：轻松、可循环、不抢戏（偏“休闲解谜”气质）。
- **音色**：柔和 Pad + 轻木琴/拨弦点缀 + 极轻打击；避免尖锐高频与硬瞬态（以免和你大量点击/咔哒类 SFX 打架）。
- **节奏**：稳态律动为主；压力阶段用“加层/开滤波”而不是硬切换 BPM。

---

## 3. 资产清单（本包提供）

### 3.1 Loops
- `bgm_menu_loop.wav`（38.4s, 16 bars, 100 BPM）
- `bgm_gameplay_base_loop.wav`（57.6s, 24 bars, 100 BPM）
- `bgm_gameplay_pressure_loop.wav`（57.6s, 24 bars, 100 BPM，含额外紧张层）

### 3.2 Stems（用于 Vertical Layering）
- Gameplay：`gameplay_pad / gameplay_arp / gameplay_perc / gameplay_pressure`
- Menu：`menu_pad / menu_arp / menu_perc`

### 3.3 Stingers（音乐提示）
- `stinger_full_warning`：检测到满槽瞬间
- `stinger_speedup / stinger_speeddown`：进入/退出 5x
- `stinger_unlock`：锁箱解锁
- `stinger_box_complete`：箱体完成
- `stinger_win / stinger_lose`：胜负结算
- `stinger_booster_activate`：道具使用（输入暂停提示）

---

## 4. 推荐的“自适应 BGM 结构”

你有两种实现方式（二选一即可）：

### 方案 A：两条 Loop Crossfade（最简单）
- 常态：播放 `bgm_gameplay_base_loop`
- 压力：Crossfade 到 `bgm_gameplay_pressure_loop`

**Crossfade 建议**：200–400ms；保持节拍对齐（都为 100 BPM 且同长度）。

### 方案 B：Vertical Layering（更高级，推荐）
始终播放同一个“对齐的 stem 集合”，根据状态开关 layer：
- 常态：Pad + Arp + Perc
- 压力：再叠加 Pressure Layer

优点：切换更自然、更像“系统随局势变化”。

---

## 5. 状态映射与触发建议（对齐你的玩法逻辑）

> 你可以把下面这些作为一个 `BgmStateMachine` 来做。

### 5.1 Level 开始
- 进入关卡：启动 `bgm_gameplay_base`（或 stems 常态组合）
- 若关卡有锁箱：可以在首次解锁前对 BGM 做轻微低通/音量略低，解锁时打开（可选）

### 5.2 满槽压力与 5x（核心）
当关卡存在容量限制（beltCapacity > 0）时，建议计算：
- `fillRatio = blocksOnBelt / beltCapacity`

阈值建议（可从 Metadata 里取默认）：
- `fillRatio >= 0.75` → 进入压力态（叠加 pressure layer / crossfade）
- `fillRatio <= 0.60` → 退出压力态

事件触发：
- **检测到满槽**：播放 `stinger_full_warning`，并强制进入压力态
- **进入 5x**：播放 `stinger_speedup`，保持压力态
- **退出 5x**：播放 `stinger_speeddown`，根据 fillRatio 决定是否回到常态

### 5.3 解锁与完成（里程碑）
- 锁箱解锁：播放 `stinger_unlock`
- 某箱体完成：播放 `stinger_box_complete`

### 5.4 道具（Boosters）
- 道具使用时会暂停输入：播放 `stinger_booster_activate`
- 若道具触发“先跑一圈 5x”：复用 speedup/speeddown 事件（与你的满槽逻辑一致）

### 5.5 胜负结算与关卡流
- 胜利：停止/淡出 BGM（300–800ms），播放 `stinger_win`
- 失败：快速停止/淡出 BGM（100–300ms），播放 `stinger_lose`

---

## 6. 混音与工程建议（务必做，直接决定品质）

1) **BGM 永远让位给 SFX**：建议 BGM 总线相对 SFX 低 10–14 dB。  
2) **高通（HPF）**：BGM 建议在 80–120Hz 做高通，避免低频占位导致其它声效“听不见”。  
3) **Sidechain Ducking**：当播放关键 stinger 或高优先级 SFX（完成/解锁/胜负）时，让 BGM duck 6–10 dB，持续 300–800ms。  
4) **移动端优先**：避免把主要能量放在 60Hz 以下（小音箱会“听起来没声音”）。

---

## 7. QA 清单（接入后必测）

- [ ] Loop 是否无缝（无 click、无节拍漂移）
- [ ] 压力切换是否自然（crossfade 或 layer 开关）
- [ ] 5x speedup/speeddown 是否每次都触发正确
- [ ] 勝負结算时 BGM 是否正确淡出，且不遮挡 stinger
- [ ] 音量设置：BGM/SFX 独立滑杆、静音开关
- [ ] App 进入后台/来电话时是否暂停并正确恢复

---

### 附：本包文件清单与参数
见 `Docs/BGM_Manifest.csv` 与 `Docs/BGM_Metadata.json`。
