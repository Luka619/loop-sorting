# Quick Start

1. 导入 `SFX/` 下所有 WAV。
2. 读取 `Docs/SFX_Manifest.csv` 建立事件映射（或直接按文件名硬编码）。
3. 把 `conveyor_loop.wav` 作为持续 Loop；其余按事件播放。
4. 按 Manifest 的 `priority/polyphony/cooldown_ms` 做并发管理与抢占策略。

建议：先把“入箱成功/失败、完成/解锁、满槽警告/失败”接入，立刻提升可读性。
