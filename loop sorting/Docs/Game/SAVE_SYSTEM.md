# Loop Sorting 存档规范（v0.1）

> 总索引：`../README.md`

本文件定义“存什么 / 什么时候存 / 怎么存”，用于后续逐步扩展（账号、云存档、更多货币与系统）。

## 1. 目标
- 任何设备/平台（含微信 WebGL）都能稳定保存：设置、货币、道具数量、关卡进度。
- 写入频率可控：关键事件立即保存；连续变化做防抖合并；应用切后台兜底 flush。
- 可升级：通过 `saveVersion` 做向后兼容与字段扩展。

## 2. 当前实现落地（代码）
- 存档服务：`Assets/Scripts/LoopSortingSaveService.cs`
- 持久化介质：`PlayerPrefs` 存一段 JSON（key：`LoopSorting.SaveV1`）
  - 微信小游戏侧通常会由 SDK 覆盖/适配 `PlayerPrefs` 到其本地存储体系。

## 3. 存档内容（v1）
- 进度
  - `flowIndex`：当前关卡索引（基于 `LevelFlow` 的 index）
  - `highestUnlockedFlowIndex`：已解锁的最高关卡索引（用于未来关卡选择/解锁逻辑）
- 经济
  - `coins`
  - `lives`
- 道具
  - `boosterSortCount`
  - `boosterShuffleCount`
- 设置
  - `soundEnabled`
  - `musicEnabled`
  - `vibrationEnabled`
- 诊断
  - `lastSaveUnixSeconds`

## 4. 触发时机（事件驱动 + 防抖合并）
### 4.1 强触发（建议 0.2s 防抖合并）
以下事件必须触发保存（允许同帧合并）：
- 过关进入下一关（更新 `flowIndex`、解锁进度）
- 购买/消耗类：扣/加 `coins`、加 `lives`、道具数量变化
- 设置变更：音效/音乐/震动开关变化

### 4.2 生命周期兜底（立即 flush）
以下时机如果存在脏数据则必须立即写入：
- `OnApplicationPause(true)`
- `OnApplicationFocus(false)`
- `OnApplicationQuit()`

## 5. 写入策略（实现原则）
- 所有数据变更只标记脏（`RequestSave`），不直接频繁写盘。
- `FlushSave` 写入全量 JSON（避免多 key 漏写）。
- 写入失败不清脏，等待下次生命周期兜底或再次触发。

## 6. 扩展约束（后续迭代规则）
- 新增字段：只能追加，不破坏旧字段；读取时缺字段用默认值。
- 删除字段：必须保留迁移兼容至少一个版本周期。
- 如果引入账号/云存档：必须保留本地缓存作为离线兜底，并给出冲突合并规则。
