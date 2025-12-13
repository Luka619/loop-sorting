using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting.Motion
{
    /// <summary>
    /// 动效播放模板（无第三方依赖）。
    /// 你可以把这里的“Transform 参数”替换为你项目已有的 View 组件（BoxView/BlockView/ConveyorView）。
    /// </summary>
    public class MotionPlayer : MonoBehaviour
    {
        [SerializeField] private MotionConfig config;

        private float speedMultiplier = 1f;

        public void SetSpeedMultiplier(float multiplier)
        {
            speedMultiplier = Mathf.Max(0.01f, multiplier);
        }

        public float GetConveyorTickDuration() => config.conveyorTickDuration / speedMultiplier;

        #region Conveyor

        /// <summary>
        /// 槽位 tick 平滑位移：把 slot 从当前位置移动到 targetPos。
        /// 建议由逻辑层在 OnConveyorTickStart 时批量调用，保证对齐。
        /// </summary>
        public Coroutine PlayConveyorSlotMove(Transform slot, Vector3 targetPos)
        {
            float dur = GetConveyorTickDuration();
            return StartCoroutine(TweenUtil.TweenVec3(dur, slot.position, targetPos,
                p => slot.position = p,
                config.easeInOutCubic));
        }

        #endregion

        #region Box / Block

        /// <summary>
        /// 出货：把 run 的积木逐个从 boxMouth 弹到 beltSlot。
        /// runBlocks 的 Transform 一般是“最外层可见的那一层”。
        /// </summary>
        public Coroutine PlayBoxShipRun(Transform boxMouth, Transform beltSlot, IReadOnlyList<Transform> runBlocks)
        {
            return StartCoroutine(CoShip());

            IEnumerator CoShip()
            {
                for (int i = 0; i < runBlocks.Count; i++)
                {
                    Transform b = runBlocks[i];
                    Vector3 from = b.position;
                    Vector3 to = beltSlot.position;

                    // 轻微抬起 + 过冲（可按美术资源调整）
                    Vector3 mid = Vector3.Lerp(from, to, 0.5f) + Vector3.up * 0.15f;

                    float dur = config.shipPerBlockDuration / speedMultiplier;
                    float half = dur * 0.5f;

                    yield return TweenUtil.TweenVec3(half, from, mid, p => b.position = p, config.outBackLike);
                    yield return TweenUtil.TweenVec3(half, mid, to, p => b.position = p, config.easeInOutCubic);

                    // Stagger
                    float wait = config.shipStagger / speedMultiplier;
                    if (wait > 0f) yield return new WaitForSeconds(wait);
                }
            }
        }

        /// <summary>
        /// 入箱成功：沿 boxMouth 吸入到 targetInnerSlot。
        /// </summary>
        public Coroutine PlayBlockEnterBoxSuccess(Transform block, Transform boxMouth, Transform targetInnerSlot)
        {
            return StartCoroutine(Co());

            IEnumerator Co()
            {
                Vector3 from = block.position;
                Vector3 mouth = boxMouth.position;
                Vector3 to = targetInnerSlot.position;

                float dur = config.enterBoxDuration / speedMultiplier;
                float a = dur * 0.45f;
                float b = dur * 0.55f;

                yield return TweenUtil.TweenVec3(a, from, mouth, p => block.position = p, config.easeInOutCubic);
                yield return TweenUtil.TweenVec3(b, mouth, to, p => block.position = p, config.easeInOutCubic);

                // 末端轻压缩（若 block 有单独的视觉 transform，可改为 localScale）
                Vector3 s0 = block.localScale;
                Vector3 s1 = s0 * 0.92f;
                yield return TweenUtil.TweenVec3(0.06f, s0, s1, p => block.localScale = p, config.easeInOutCubic);
                yield return TweenUtil.TweenVec3(0.06f, s1, s0, p => block.localScale = p, config.easeInOutCubic);
            }
        }

        /// <summary>
        /// 入箱失败：轻碰撞回弹（原因 icon 建议由 UI 层额外处理）。
        /// </summary>
        public Coroutine PlayBlockEnterBoxFailBounce(Transform block, Vector3 awayDir)
        {
            return StartCoroutine(Co());

            IEnumerator Co()
            {
                Vector3 from = block.position;
                Vector3 to = from + awayDir.normalized * 0.12f;

                float dur = config.enterFailBounceDuration / speedMultiplier;
                float half = dur * 0.5f;

                yield return TweenUtil.TweenVec3(half, from, to, p => block.position = p, config.easeInOutCubic);
                yield return TweenUtil.TweenVec3(half, to, from, p => block.position = p, config.easeInOutCubic);
            }
        }

        /// <summary>
        /// Hidden 揭示：这里用缩放 + 透明度占位（真实实现建议用材质/遮罩/翻牌）。
        /// </summary>
        public Coroutine PlayHiddenReveal(SpriteRenderer sr, float targetAlpha = 1f)
        {
            return StartCoroutine(Co());

            IEnumerator Co()
            {
                // 从略暗到正常
                float fromA = sr.color.a;
                float dur = config.hiddenRevealDuration / speedMultiplier;

                yield return TweenUtil.TweenFloat(dur, fromA, targetAlpha, a =>
                {
                    var c = sr.color;
                    c.a = a;
                    sr.color = c;
                }, config.easeInOutCubic);
            }
        }

        #endregion
    }
}
