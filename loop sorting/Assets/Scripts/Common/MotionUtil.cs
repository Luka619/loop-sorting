using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    public static class MotionUtil
    {
        public static float EaseOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return 1f - u * u * u;
        }

        public static float EaseInOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float EaseOutBack(float t, float overshoot = 1.70158f)
        {
            t = Mathf.Clamp01(t);
            float u = t - 1f;
            return 1f + (u * u * ((overshoot + 1f) * u + overshoot));
        }

        public static IEnumerator ScalePop(Transform target, Vector3 from, Vector3 to, float seconds, bool easeOutBack = true)
        {
            if (target == null) yield break;
            seconds = Mathf.Max(0.0001f, seconds);

            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = easeOutBack ? EaseOutBack(u) : EaseOutCubic(u);
                target.localScale = Vector3.LerpUnclamped(from, to, e);
                yield return null;
            }
            target.localScale = to;
        }

        public static IEnumerator ScalePunch(Transform target, Vector3 baseScale, float punchScale, float seconds)
        {
            if (target == null) yield break;
            seconds = Mathf.Max(0.0001f, seconds);

            float t = 0f;
            while (t < seconds)
            {
                if (target == null) yield break;
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float a = u < 0.5f ? EaseOutCubic(u / 0.5f) : 1f - EaseInOutCubic((u - 0.5f) / 0.5f);
                float s = 1f + punchScale * a;
                target.localScale = baseScale * s;
                yield return null;
            }
            if (target != null)
            {
                target.localScale = baseScale;
            }
        }

        public static IEnumerator ShakeLocalPosition(Transform target, Vector3 baseLocalPos, float amplitude, float seconds, int shakes = 10)
        {
            if (target == null) yield break;
            seconds = Mathf.Max(0.0001f, seconds);
            shakes = Mathf.Max(2, shakes);

            float t = 0f;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float fade = 1f - EaseOutCubic(u);
                float s = Mathf.Sin(u * Mathf.PI * shakes);
                target.localPosition = baseLocalPos + new Vector3(s * amplitude * fade, 0f, 0f);
                yield return null;
            }
            target.localPosition = baseLocalPos;
        }

        public static CanvasGroup EnsureCanvasGroup(GameObject go)
        {
            if (go == null) return null;
            var cg = go.GetComponent<CanvasGroup>();
            if (cg != null) return cg;
            return go.AddComponent<CanvasGroup>();
        }

        public static IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float seconds, bool unscaled = true)
        {
            if (cg == null) yield break;
            seconds = Mathf.Max(0.0001f, seconds);

            cg.alpha = from;
            float t = 0f;
            while (t < seconds)
            {
                t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                cg.alpha = Mathf.Lerp(from, to, EaseOutCubic(u));
                yield return null;
            }
            cg.alpha = to;
        }
    }
}
