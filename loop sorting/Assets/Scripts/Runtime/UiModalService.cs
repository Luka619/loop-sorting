using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    /// <summary>
    /// Centralizes modal panel animation + input lock tracking for UI overlays.
    /// </summary>
    public sealed class UiModalService
    {
        private readonly Dictionary<GameObject, Coroutine> _panelRoutines = new Dictionary<GameObject, Coroutine>();
        private readonly HashSet<GameObject> _heldPanels = new HashSet<GameObject>();
        private int _lockDepth;

        public bool IsLocked => _lockDepth > 0;

        public void Reset()
        {
            _panelRoutines.Clear();
            _heldPanels.Clear();
            _lockDepth = 0;
        }

        public void HoldPanel(GameObject panel)
        {
            if (panel == null) return;
            if (_heldPanels.Add(panel))
            {
                _lockDepth++;
            }
        }

        public void ReleasePanel(GameObject panel)
        {
            if (panel == null) return;
            if (_heldPanels.Remove(panel))
            {
                _lockDepth = Mathf.Max(0, _lockDepth - 1);
            }
        }

        public void HideImmediate(MonoBehaviour host, GameObject panel, Action<GameObject> onHidden)
        {
            if (panel == null) return;

            if (_panelRoutines.TryGetValue(panel, out var routine) && routine != null && host != null)
            {
                host.StopCoroutine(routine);
            }
            _panelRoutines.Remove(panel);

            var cg = MotionUtil.EnsureCanvasGroup(panel);
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }

            panel.SetActive(false);
            onHidden?.Invoke(panel);
        }

        public void AnimatePanel(
            MonoBehaviour host,
            GameObject panel,
            bool show,
            float seconds,
            Action<GameObject> onShown,
            Action<GameObject> onHidden)
        {
            if (panel == null || host == null) return;

            if (_panelRoutines.TryGetValue(panel, out var existing) && existing != null)
            {
                host.StopCoroutine(existing);
            }
            _panelRoutines.Remove(panel);

            var cg = MotionUtil.EnsureCanvasGroup(panel);
            if (cg == null)
            {
                panel.SetActive(show);
                if (show) onShown?.Invoke(panel);
                else onHidden?.Invoke(panel);
                return;
            }

            if (show)
            {
                panel.SetActive(true);
                panel.transform.localScale = Vector3.one * 0.92f;
                cg.alpha = 0f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
                onShown?.Invoke(panel);
                _panelRoutines[panel] = host.StartCoroutine(AnimatePanelIn(panel, cg, seconds));
            }
            else
            {
                cg.interactable = false;
                // Keep blocking raycasts during fade-out to prevent click-through to gameplay/HUD.
                cg.blocksRaycasts = true;
                _panelRoutines[panel] = host.StartCoroutine(AnimatePanelOut(panel, cg, seconds, onHidden));
            }
        }

        private IEnumerator AnimatePanelIn(GameObject panel, CanvasGroup cg, float seconds)
        {
            if (panel == null || cg == null) yield break;
            float t = 0f;
            seconds = Mathf.Max(0.05f, seconds);
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                cg.alpha = Mathf.Lerp(0f, 1f, MotionUtil.EaseOutCubic(u));
                float s = Mathf.Lerp(0.92f, 1f, MotionUtil.EaseOutBack(u));
                panel.transform.localScale = Vector3.one * s;
                yield return null;
            }
            cg.alpha = 1f;
            panel.transform.localScale = Vector3.one;
            _panelRoutines.Remove(panel);
        }

        private IEnumerator AnimatePanelOut(GameObject panel, CanvasGroup cg, float seconds, Action<GameObject> onHidden)
        {
            if (panel == null || cg == null) yield break;
            float startAlpha = cg.alpha;
            float t = 0f;
            seconds = Mathf.Max(0.05f, seconds);
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / seconds);
                cg.alpha = Mathf.Lerp(startAlpha, 0f, MotionUtil.EaseOutCubic(u));
                float s = Mathf.Lerp(1f, 0.96f, MotionUtil.EaseOutCubic(u));
                panel.transform.localScale = Vector3.one * s;
                yield return null;
            }
            cg.alpha = 0f;
            panel.transform.localScale = Vector3.one;
            cg.blocksRaycasts = false;
            cg.interactable = false;
            panel.SetActive(false);
            onHidden?.Invoke(panel);
            _panelRoutines.Remove(panel);
        }
    }
}
