# Loop Sorting 振动（Haptics）设计稿 v1.0

> 目标：用“克制但清晰”的触觉反馈，让玩家在不看细节/不开声音时，也能更快理解**点击是否生效**、**操作是否被拒绝**、**关键状态变化**（完成/解锁/胜负）。

---

## 1. 总原则（Pillars）

1) **语义统一**：同类事件在任何界面都保持同一种“手感含义”。  
2) **不刷屏**：振动必须有冷却与节制，避免密集触发造成疲劳/烦躁。  
3) **轻>重**：默认只用 Light/Medium，Heavy/Long 只用于明确的“拒绝/失败/关键节点”。  
4) **与音效解耦**：SOUND 关闭时仍可振动；VIBRATION 关闭时任何情况下不振动。  
5) **平台一致、能力差异可接受**：iOS 退化为系统默认振动，依旧保持节奏（pattern）不滥用。

---

## 2. 开关与触发源

- 开关入口：`SettingsPanel` → `VIBRATION` Toggle（对应 `GameRuntimeController.vibrationEnabled`）。  
- 行为：
  - 打开：立即给一次“确认”触感（Confirm）作为反馈。
  - 关闭：立刻停止当前 pattern（后续不再触发）。
- 触发源（统一入口）：所有 `GameRuntimeController.PlaySfx(SfxId ...)` 都会尝试映射并播放对应振动（即使 SOUND 关闭也会触发振动）。

---

## 3. 脉冲库（Pulse Library）

说明：设计层只关心 **Light / Medium / Heavy / Long** 四种语义；不同平台用各自实现去近似。

| Pulse | 语义 | Android（默认实现） | WeChat 小游戏（WebGL） | iOS |
|---|---|---|---|---|
| Light | 轻触/可操作 | 18ms / amp≈80 | `VibrateShort(type="light")` | `Handheld.Vibrate()`（同一档退化） |
| Medium | 确认/执行 | 28ms / amp≈140 | `VibrateShort(type="medium")` | 同上 |
| Heavy | 拒绝/碰撞 | 45ms / amp≈220 | `VibrateShort(type="heavy")` | 同上 |
| Long | 失败/结束态 | 120ms / amp≈200 | `VibrateLong()` | 同上 |

- Android 强度：`amplitude = baseAmplitude * intensity`（0~1）再 clamp 到 1~255。  
- Pattern：由多个脉冲组成，按 step 的 `DelaySeconds` 依次播放（使用 realtime，不受 TimeScale 影响）。

---

## 4. 事件映射（Game-wide Mapping）

记法：`L/M/H/Long` 分别表示 Light/Medium/Heavy/Long；`M→(0.06s)→L` 表示两段脉冲与间隔。

### 4.1 UI

| 场景 | SfxId | HapticsId | Pattern | 冷却 |
|---|---|---|---|---|
| 按钮点击/弹窗开关 | `UiClick/UiHover/UiPopupOpen/UiPopupClose` | `UiTap` | `L` | 0.04s |
| 确认/购买/下一关/重试 | `UiConfirm/LevelNext/LevelRetry` | `UiConfirm` | `M` | 0.06s |
| 取消/返回 | `UiCancel` | `UiCancel` | `L→(0.05s)→L` | 0.10s |
| 禁止/不允许 | `UiDenied/BoxBusyDenied` | `UiDenied` | `M→(0.06s)→L` | 0.14s |

### 4.2 玩法（Box/Block/Hidden）

| 场景 | SfxId | HapticsId | Pattern | 冷却 | 说明 |
|---|---|---|---|---|---|
| 选择箱子出货 | `BoxSelect` | `GameplaySelect` | `L` | 0.06s | 轻确认：已选中 |
| 成功进箱/落位 | `BlockInsert/BlockLand` | `GameplayInsert` | `L` | 0.10s | 轻反馈：落位成功 |
| 被拒绝（各种原因） | `BlockReject*` | `GameplayReject` | `H` | 0.18s | 强反馈：失败/碰撞 |
| 锁箱撞击/不可用 | `BoxLockedThunk` | `GameplayLocked` | `H→(0.07s)→L` | 0.22s | “硬+回弹” |
| 箱子完成 | `BoxComplete` | `BoxComplete` | `M→(0.06s)→L` | 0.35s | 先确认再收尾 |
| 解锁 | `BoxUnlock` | `BoxUnlock` | `L→(0.06s)→M` | 0.35s | 先提示再确认 |
| 隐藏揭示 | `HiddenReveal` | `HiddenReveal` | `L` | 0.12s | 轻提示即可 |

### 4.3 传送带（Conveyor）

| 场景 | SfxId | HapticsId | Pattern | 冷却 | 说明 |
|---|---|---|---|---|---|
| 满带预警 | `ConveyorFullWarning` | `ConveyorFullWarning` | `M→(0.08s)→M` | 0.70s | 频率低，提醒但不焦躁 |
| 满带失败 | `ConveyorFullFail` | `ConveyorFullFail` | `M→(0.10s)→H` | 1.20s | 失败但保持克制（不做连续长震） |

> 刻意不做：`ConveyorTick/ConveyorLoop/Speedup/Speeddown` 等高频/状态音效，避免“走路震动”。

### 4.4 道具（Boosters）

| 场景 | SfxId | HapticsId | Pattern | 冷却 |
|---|---|---|---|---|
| 激活道具 | `BoosterActivate` | `BoosterActivate` | `M` | 0.20s |
| 道具成功 | `BoosterFillSort/BoosterShuffle` | `BoosterSuccess` | `M→(0.07s)→M` | 0.40s |
| 道具失败 | `BoosterFail` | `BoosterFail` | `H` | 0.50s |

### 4.5 关卡结束（Win/Lose）

| 场景 | SfxId | HapticsId | Pattern | 冷却 |
|---|---|---|---|---|
| 胜利 | `LevelWin` | `LevelWin` | `L→(0.06s)→L→(0.08s)→M` | 2.0s |
| 失败 | `LevelLose` | `LevelLose` | `H→(0.10s)→Long` | 2.0s |

---

## 5. 实装说明（代码位置）

- 统一触发入口：`Assets/Scripts/GameRuntimeController.cs`（`PlaySfx` 内先振动、后音效）。  
- 设计与映射：
  - `Assets/Scripts/Haptics/HapticsId.cs`：事件 ID。
  - `Assets/Scripts/Haptics/HapticsCatalog.cs`：HapticsProfile（pattern+冷却）与 `SfxId -> HapticsId` 映射。
  - `Assets/Scripts/Haptics/HapticsPlayer.cs`：跨平台播放（WeChat/Android/iOS）。

---

## 6. 调参指南（不改玩法也能提质）

1) **想更克制**：提高 `HapticsCatalog` 里对应 `HapticsId` 的 `CooldownSeconds`。  
2) **想更清晰**：把某些关键事件的 pulse 从 `L` 提到 `M`（慎用 `H/Long`）。  
3) **安卓强度**：调整 `HapticsPlayer.TryAndroidPulse` 的 `durationMs/amplitude` 基准值。  
4) **想支持强 iOS 触觉**：需要上 native haptics 插件（可选后续，不在本次范围）。

