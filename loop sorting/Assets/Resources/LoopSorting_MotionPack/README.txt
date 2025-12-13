LoopSorting 动效包（模板）
========================

包含内容：
- LoopSorting_MotionDesign.md：动效设计文档（可直接给团队拆任务）
- MotionId.cs：动效事件枚举
- MotionConfig.cs：动效参数 ScriptableObject（建议落地）
- TweenUtil.cs：无第三方依赖 Tween 工具（原型/占位）
- MotionPlayer.cs：动效播放模板（Transform 级别示例）

使用方式（建议）：
1) 逻辑层在关键节点抛事件（OnConveyorTickStart / OnBoxShipRunStart ...）。
2) View 层订阅事件，调用 MotionPlayer 对对应对象播放动效。
3) 由 TA/策划在 MotionConfig 中调参，统一出一个“手感基准”。

注意：
- 该模板不依赖 DOTween，但生产项目可替换实现以获得更丰富的曲线与序列能力。
