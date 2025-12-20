using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class CurrencyFlyFx : MonoBehaviour
    {
        [Header("Timing")]
        [Min(0.05f)] public float tokenDurationSeconds = 0.65f;
        [Min(0f)] public float tokenStaggerSeconds = 0.04f;

        [Header("Motion")]
        [Min(0f)] public float spawnSpreadUnits = 34f;
        [Min(0f)] public float arcHeightMinUnits = 120f;
        [Min(0f)] public float arcHeightMaxUnits = 360f;
        [Range(0.05f, 1f)] public float endScale = 0.6f;
        [Range(0f, 0.5f)] public float fadeTailFraction = 0.18f;

        [Header("Impact")]
        [Range(0f, 0.4f)] public float targetPunchScale = 0.12f;
        [Min(0.05f)] public float targetPunchSeconds = 0.18f;

        private RectTransform _layer;

        private void Awake()
        {
            EnsureLayer();
        }

        public void PlayCoins(RectTransform from, RectTransform to, Sprite sprite, int amount)
        {
            if (to == null) return;
            amount = Mathf.Max(0, amount);
            if (amount == 0) return;
            if (sprite == null && LoopSortingUIKit.IsAvailable())
            {
                sprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin");
            }
            if (sprite == null) return;

            EnsureLayer();
            _layer.SetAsLastSibling();
            StartCoroutine(PlayCoinsRoutine(from, to, sprite, ComputeTokenCount(amount)));
        }

        private void EnsureLayer()
        {
            if (_layer != null) return;

            var go = new GameObject("CurrencyFlyFxLayer");
            go.transform.SetParent(transform, false);

            _layer = go.AddComponent<RectTransform>();
            _layer.anchorMin = Vector2.zero;
            _layer.anchorMax = Vector2.one;
            _layer.offsetMin = Vector2.zero;
            _layer.offsetMax = Vector2.zero;
            _layer.pivot = new Vector2(0.5f, 0.5f);

            var cg = go.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }

        private IEnumerator PlayCoinsRoutine(RectTransform from, RectTransform to, Sprite sprite, int tokenCount)
        {
            if (tokenCount <= 0) yield break;

            Vector2 start = ResolveLocalPoint(from, fallbackWorldPosition: to.position);
            Vector2 end = ResolveLocalPoint(to, fallbackWorldPosition: to.position);

            float size = ResolveTokenSize(from, to);
            float distance = Vector2.Distance(start, end);
            float arcHeight = Mathf.Clamp(distance * 0.25f, arcHeightMinUnits, arcHeightMaxUnits);

            float dur = Mathf.Max(0.05f, tokenDurationSeconds);
            float stagger = Mathf.Max(0f, tokenStaggerSeconds);

            float totalSeconds = dur + stagger * Mathf.Max(0, tokenCount - 1);
            for (int i = 0; i < tokenCount; i++)
            {
                float delay = stagger * i;
                StartCoroutine(FlyOneToken(sprite, size, start, end, arcHeight, dur, delay));
            }

            if (totalSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(totalSeconds);
            }

            if (targetPunchScale > 0.0001f && to != null)
            {
                Vector3 baseScale = to.localScale;
                StartCoroutine(MotionUtil.ScalePunch(to, baseScale, targetPunchScale, targetPunchSeconds));
            }
        }

        private IEnumerator FlyOneToken(
            Sprite sprite,
            float size,
            Vector2 start,
            Vector2 end,
            float arcHeight,
            float seconds,
            float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

            var go = new GameObject("CoinFxToken");
            go.transform.SetParent(_layer, false);

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.preserveAspect = true;
            img.sprite = sprite;
            img.color = Color.white;

            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(size, size);

            Vector2 jitter = Random.insideUnitCircle * Mathf.Max(0f, spawnSpreadUnits);
            Vector2 startJittered = start + jitter;
            rt.anchoredPosition = startJittered;
            rt.localScale = Vector3.one;

            Vector2 mid =
                (startJittered + end) * 0.5f +
                Vector2.up * arcHeight +
                new Vector2(Random.Range(-0.18f, 0.18f) * arcHeight, 0f);

            float tail = Mathf.Clamp01(fadeTailFraction);
            float t = 0f;
            seconds = Mathf.Max(0.05f, seconds);
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = MotionUtil.EaseOutCubic(u);

                Vector2 a = Vector2.LerpUnclamped(startJittered, mid, e);
                Vector2 b = Vector2.LerpUnclamped(mid, end, e);
                rt.anchoredPosition = Vector2.LerpUnclamped(a, b, e);

                float shrink = u > 0.75f ? Mathf.Lerp(1f, Mathf.Clamp(endScale, 0.05f, 1f), (u - 0.75f) / 0.25f) : 1f;
                rt.localScale = Vector3.one * shrink;

                if (tail > 0.0001f && u > 1f - tail)
                {
                    float k = Mathf.Clamp01((u - (1f - tail)) / tail);
                    cg.alpha = 1f - k;
                }

                yield return null;
            }

            Destroy(go);
        }

        private Vector2 ResolveLocalPoint(RectTransform rt, Vector3 fallbackWorldPosition)
        {
            if (_layer == null) return Vector2.zero;
            Vector3 wp = rt != null ? rt.position : fallbackWorldPosition;
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam: null, worldPoint: wp);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_layer, sp, cam: null, out var lp);
            return lp;
        }

        private static float ResolveTokenSize(RectTransform from, RectTransform to)
        {
            float s = 0f;
            if (from != null)
            {
                float w = Mathf.Abs(from.rect.width);
                float h = Mathf.Abs(from.rect.height);
                if (w > 1f && h > 1f) s = Mathf.Min(w, h);
            }
            if (s <= 1f && to != null)
            {
                float w = Mathf.Abs(to.rect.width);
                float h = Mathf.Abs(to.rect.height);
                if (w > 1f && h > 1f) s = Mathf.Min(w, h);
            }
            if (s <= 1f) s = 96f;
            return Mathf.Clamp(s, 48f, 160f);
        }

        private static int ComputeTokenCount(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (amount == 0) return 0;
            int c = Mathf.RoundToInt(Mathf.Sqrt(amount) * 1.05f);
            return Mathf.Clamp(c, 4, 12);
        }
    }
}
