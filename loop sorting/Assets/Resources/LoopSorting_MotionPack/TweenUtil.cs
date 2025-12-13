using System;
using System.Collections;
using UnityEngine;

namespace LoopSorting.Motion
{
    /// <summary>
    /// 无第三方依赖的轻量 Tween 工具：用于模板/原型。
    /// 生产项目可替换为 DOTween/LeanTween/自研曲线系统。
    /// </summary>
    public static class TweenUtil
    {
        public static IEnumerator Tween(
            float duration,
            Action<float> onUpdate,
            AnimationCurve curve = null,
            bool unscaledTime = false)
        {
            if (duration <= 0f)
            {
                onUpdate?.Invoke(1f);
                yield break;
            }

            float t = 0f;
            while (t < duration)
            {
                t += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / duration);
                if (curve != null) u = Mathf.Clamp01(curve.Evaluate(u));
                onUpdate?.Invoke(u);
                yield return null;
            }

            onUpdate?.Invoke(1f);
        }

        public static IEnumerator TweenVec3(
            float duration,
            Vector3 from,
            Vector3 to,
            Action<Vector3> onUpdate,
            AnimationCurve curve = null,
            bool unscaledTime = false)
        {
            return Tween(duration, u =>
            {
                onUpdate?.Invoke(Vector3.LerpUnclamped(from, to, u));
            }, curve, unscaledTime);
        }

        public static IEnumerator TweenFloat(
            float duration,
            float from,
            float to,
            Action<float> onUpdate,
            AnimationCurve curve = null,
            bool unscaledTime = false)
        {
            return Tween(duration, u =>
            {
                onUpdate?.Invoke(Mathf.LerpUnclamped(from, to, u));
            }, curve, unscaledTime);
        }
    }
}
