using UnityEngine;

namespace LoopSorting.Motion
{
    /// <summary>
    /// 动效参数集中配置。建议做成 ScriptableObject，方便策划/TA 调参。
    /// </summary>
    [CreateAssetMenu(menuName = "LoopSorting/Motion/MotionConfig")]
    public class MotionConfig : ScriptableObject
    {
        [Header("Global Speed")]
        [Tooltip("传送带 1x 时每个 tick 的表现时长（秒）。逻辑仍按 tick 推进。")]
        public float conveyorTickDuration = 0.25f;

        [Tooltip("5x 模式速度倍数（满槽、道具前置）。")]
        public float fastForwardMultiplier = 5f;

        [Header("Curves")]
        public AnimationCurve easeInOutCubic = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("用于轻微过冲的曲线（可手工调成 OutBack）。")]
        public AnimationCurve outBackLike = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Box Ship")]
        public float shipPerBlockDuration = 0.12f;
        public float shipStagger = 0.05f;

        [Header("Block Enter Box")]
        public float enterBoxDuration = 0.26f;
        public float enterFailBounceDuration = 0.12f;

        [Header("Hidden Reveal")]
        public float hiddenRevealDuration = 0.22f;
        public float hiddenRevealStagger = 0.03f;

        [Header("Complete / Unlock")]
        public float completeSealDuration = 0.42f;
        public float unlockDuration = 0.45f;

        [Header("UI/Flow")]
        public float winTransitionDuration = 0.8f;
        public float loseTransitionDuration = 0.6f;
    }
}
