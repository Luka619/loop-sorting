using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private sealed class SettingsUiController
        {
            private readonly GameRuntimeController _host;
            private bool _activatedCanvasForSettings;
            private bool _hidHudRoot;
            private bool _hudRootWasActive;
            private int _uiCanvasPrevSortingOrder;

            public SettingsUiController(GameRuntimeController host)
            {
                _host = host;
            }

            public void EnsureBuilt()
            {
                _host.EnsureSettingsUI();
            }

            public void Toggle(bool show)
            {
                EnsureBuilt();
                if (_host._settingsPanel == null) return;

                if (show)
                {
                    PrepareCanvasForSettings();
                    _host.HideUiPanelImmediate(_host._shopPanel);
                    _host.HideUiPanelImmediate(_host._boosterPurchasePanel);
                    _host.HideUiPanelImmediate(_host._resultPanel);

                    RefreshToggles();
                    _host.AnimateUiPanel(_host._settingsPanel, true, seconds: 0.20f);
                    StartEffects();
                    _host.PlaySfx(SfxId.UiPopupOpen);
                    return;
                }

                StopEffects();
                _host.AnimateUiPanel(_host._settingsPanel, false, seconds: 0.18f);
                _host.PlaySfx(SfxId.UiPopupClose);
            }

            public void HideImmediate()
            {
                if (_host._settingsPanel == null) return;
                StopEffects();
                _host.HideUiPanelImmediate(_host._settingsPanel);
            }

            public void RefreshToggles()
            {
                ApplyToggleVisual(_host._settingsMusicToggleButton, _host._settingsMusicToggleImage, _host.musicEnabled);
                ApplyToggleVisual(_host._settingsSfxToggleButton, _host._settingsSfxToggleImage, _host.soundEnabled);
                ApplyToggleVisual(_host._settingsVibrationToggleButton, _host._settingsVibrationToggleImage, _host.vibrationEnabled);
            }

            public void CaptureBasePose()
            {
                if (_host._settingsPanel == null)
                {
                    _host._settingsBasePoseCaptured = false;
                    return;
                }

                _host._settingsBasePoseCaptured = true;

                if (_host._settingsTitleRect != null)
                {
                    _host._settingsTitleBasePos = _host._settingsTitleRect.anchoredPosition;
                    _host._settingsTitleBaseScale = _host._settingsTitleRect.localScale;
                }
                if (_host._settingsCloseRect != null)
                {
                    _host._settingsCloseBasePos = _host._settingsCloseRect.anchoredPosition;
                    _host._settingsCloseBaseScale = _host._settingsCloseRect.localScale;
                }
                if (_host._settingsMusicRowRect != null)
                {
                    _host._settingsMusicRowBasePos = _host._settingsMusicRowRect.anchoredPosition;
                    _host._settingsMusicRowBaseScale = _host._settingsMusicRowRect.localScale;
                }
                if (_host._settingsSfxRowRect != null)
                {
                    _host._settingsSfxRowBasePos = _host._settingsSfxRowRect.anchoredPosition;
                    _host._settingsSfxRowBaseScale = _host._settingsSfxRowRect.localScale;
                }
                if (_host._settingsVibrationRowRect != null)
                {
                    _host._settingsVibrationRowBasePos = _host._settingsVibrationRowRect.anchoredPosition;
                    _host._settingsVibrationRowBaseScale = _host._settingsVibrationRowRect.localScale;
                }
                if (_host._settingsRetryRect != null)
                {
                    _host._settingsRetryBasePos = _host._settingsRetryRect.anchoredPosition;
                    _host._settingsRetryBaseScale = _host._settingsRetryRect.localScale;
                }
            }

            public void StartEffects()
            {
                StopEffects();
                if (_host._settingsPanel == null || !_host._settingsPanel.activeInHierarchy) return;
                ResetPose();
                _host._settingsIntroRoutine = _host.StartCoroutine(AnimateIntro());
            }

            public void StopEffects()
            {
                if (_host._settingsIntroRoutine != null)
                {
                    _host.StopCoroutine(_host._settingsIntroRoutine);
                    _host._settingsIntroRoutine = null;
                }
            }

            public void OnHidden()
            {
                StopEffects();

                if (!_activatedCanvasForSettings) return;
                if (_host._hudRootRect != null && _hidHudRoot)
                {
                    _host._hudRootRect.gameObject.SetActive(_hudRootWasActive);
                }
                if (_host._uiCanvas != null)
                {
                    _host._uiCanvas.sortingOrder = _uiCanvasPrevSortingOrder;
                    _host._uiCanvas.gameObject.SetActive(false);
                }

                _activatedCanvasForSettings = false;
                _hidHudRoot = false;
                _hudRootWasActive = false;
            }

            private void PrepareCanvasForSettings()
            {
                if (_host._uiCanvas == null) return;
                if (_host._uiCanvas.gameObject.activeSelf) return;

                _activatedCanvasForSettings = true;
                _uiCanvasPrevSortingOrder = _host._uiCanvas.sortingOrder;

                int targetOrder = _uiCanvasPrevSortingOrder;
                if (_host._mainMenuCanvas != null)
                {
                    targetOrder = Mathf.Max(targetOrder, _host._mainMenuCanvas.sortingOrder + 1);
                }
                else
                {
                    targetOrder = Mathf.Max(targetOrder, 20);
                }

                _host._uiCanvas.overrideSorting = true;
                _host._uiCanvas.sortingOrder = targetOrder;
                _host._uiCanvas.gameObject.SetActive(true);

                if (_host._hudRootRect != null)
                {
                    _hudRootWasActive = _host._hudRootRect.gameObject.activeSelf;
                    _host._hudRootRect.gameObject.SetActive(false);
                    _hidHudRoot = true;
                }
            }

            private void ResetPose()
            {
                if (!_host._settingsBasePoseCaptured)
                {
                    CaptureBasePose();
                }

                ResetRect(_host._settingsTitleRect, _host._settingsTitleBasePos, _host._settingsTitleBaseScale);
                ResetRect(_host._settingsCloseRect, _host._settingsCloseBasePos, _host._settingsCloseBaseScale);
                ResetRect(_host._settingsMusicRowRect, _host._settingsMusicRowBasePos, _host._settingsMusicRowBaseScale);
                ResetRect(_host._settingsSfxRowRect, _host._settingsSfxRowBasePos, _host._settingsSfxRowBaseScale);
                ResetRect(_host._settingsVibrationRowRect, _host._settingsVibrationRowBasePos, _host._settingsVibrationRowBaseScale);
                ResetRect(_host._settingsRetryRect, _host._settingsRetryBasePos, _host._settingsRetryBaseScale);
            }

            private static void ResetRect(RectTransform rect, Vector2 basePos, Vector3 baseScale)
            {
                if (rect == null) return;
                rect.anchoredPosition = basePos;
                rect.localScale = baseScale;
                var cg = MotionUtil.EnsureCanvasGroup(rect.gameObject);
                if (cg != null) cg.alpha = 1f;
            }

            private IEnumerator AnimateIntro()
            {
                if (_host._settingsPanel == null || _host._settingsPopupRect == null) yield break;
                yield return null;
                if (_host._settingsPanel == null || !_host._settingsPanel.activeInHierarchy) yield break;

                var title = _host._settingsTitleRect;
                var close = _host._settingsCloseRect;
                var music = _host._settingsMusicRowRect;
                var sfx = _host._settingsSfxRowRect;
                var vibration = _host._settingsVibrationRowRect;
                var retry = _host._settingsRetryRect;

                var titleCg = EnsureCg(title);
                var closeCg = EnsureCg(close);
                var musicCg = EnsureCg(music);
                var sfxCg = EnsureCg(sfx);
                var vibrationCg = EnsureCg(vibration);
                var retryCg = EnsureCg(retry);

                var titlePos0 = _host._settingsTitleBasePos;
                var closePos0 = _host._settingsCloseBasePos;
                var musicPos0 = _host._settingsMusicRowBasePos;
                var sfxPos0 = _host._settingsSfxRowBasePos;
                var vibrationPos0 = _host._settingsVibrationRowBasePos;
                var retryPos0 = _host._settingsRetryBasePos;

                var titleScale0 = _host._settingsTitleBaseScale;
                var closeScale0 = _host._settingsCloseBaseScale;
                var musicScale0 = _host._settingsMusicRowBaseScale;
                var sfxScale0 = _host._settingsSfxRowBaseScale;
                var vibrationScale0 = _host._settingsVibrationRowBaseScale;
                var retryScale0 = _host._settingsRetryBaseScale;

                Prep(title, titleCg, titlePos0 + new Vector2(0f, 24f), titleScale0 * 0.96f);
                Prep(close, closeCg, closePos0 + new Vector2(0f, 18f), closeScale0 * 0.92f);
                Prep(music, musicCg, musicPos0 + new Vector2(0f, -16f), musicScale0 * 0.98f);
                Prep(sfx, sfxCg, sfxPos0 + new Vector2(0f, -16f), sfxScale0 * 0.98f);
                Prep(vibration, vibrationCg, vibrationPos0 + new Vector2(0f, -16f), vibrationScale0 * 0.98f);
                Prep(retry, retryCg, retryPos0 + new Vector2(0f, -22f), retryScale0 * 0.98f);

                float seconds = 0.32f;
                float t = 0f;
                while (t < seconds)
                {
                    if (_host._settingsPanel == null || !_host._settingsPanel.activeInHierarchy) yield break;
                    t += Time.unscaledDeltaTime;
                    float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, seconds));

                    Apply(title, titleCg, titlePos0, titleScale0, new Vector2(0f, 24f), 0.96f, u, 0f);
                    Apply(close, closeCg, closePos0, closeScale0, new Vector2(0f, 18f), 0.92f, u, 0.05f);
                    Apply(music, musicCg, musicPos0, musicScale0, new Vector2(0f, -16f), 0.98f, u, 0.12f);
                    Apply(sfx, sfxCg, sfxPos0, sfxScale0, new Vector2(0f, -16f), 0.98f, u, 0.18f);
                    Apply(vibration, vibrationCg, vibrationPos0, vibrationScale0, new Vector2(0f, -16f), 0.98f, u, 0.24f);
                    Apply(retry, retryCg, retryPos0, retryScale0, new Vector2(0f, -22f), 0.98f, u, 0.30f);

                    yield return null;
                }

                Finalize(title, titleCg, titlePos0, titleScale0);
                Finalize(close, closeCg, closePos0, closeScale0);
                Finalize(music, musicCg, musicPos0, musicScale0);
                Finalize(sfx, sfxCg, sfxPos0, sfxScale0);
                Finalize(vibration, vibrationCg, vibrationPos0, vibrationScale0);
                Finalize(retry, retryCg, retryPos0, retryScale0);

                _host._settingsIntroRoutine = null;
            }

            private static CanvasGroup EnsureCg(RectTransform rect)
            {
                if (rect == null) return null;
                return MotionUtil.EnsureCanvasGroup(rect.gameObject);
            }

            private static void Prep(RectTransform rect, CanvasGroup cg, Vector2 startPos, Vector3 startScale)
            {
                if (rect == null || cg == null) return;
                rect.anchoredPosition = startPos;
                rect.localScale = startScale;
                cg.alpha = 0f;
            }

            private static void Apply(
                RectTransform rect,
                CanvasGroup cg,
                Vector2 basePos,
                Vector3 baseScale,
                Vector2 offset,
                float startScale,
                float u,
                float delay)
            {
                if (rect == null || cg == null) return;
                float localU = Mathf.Clamp01((u - delay) / Mathf.Max(0.0001f, 1f - delay));
                float e = MotionUtil.EaseOutBack(localU);
                float ea = MotionUtil.EaseOutCubic(localU);
                rect.anchoredPosition = Vector2.LerpUnclamped(basePos + offset, basePos, e);
                rect.localScale = Vector3.LerpUnclamped(baseScale * startScale, baseScale, e);
                cg.alpha = Mathf.Lerp(0f, 1f, ea);
            }

            private static void Finalize(RectTransform rect, CanvasGroup cg, Vector2 basePos, Vector3 baseScale)
            {
                if (rect == null || cg == null) return;
                rect.anchoredPosition = basePos;
                rect.localScale = baseScale;
                cg.alpha = 1f;
            }

            private void ApplyToggleVisual(Button button, Image image, bool isOn)
            {
                if (image == null) return;

                var knobTransform = image.transform.Find("Knob");
                void HideKnob()
                {
                    if (knobTransform != null) knobTransform.gameObject.SetActive(false);
                }

                if (LoopSortingUIKit.IsAvailable())
                {
                    var track = LoopSortingUIKit.LoadSpriteByKey(isOn ? "ui.toggle.track_on" : "ui.toggle.track_off");
                    var knobSprite = LoopSortingUIKit.LoadSpriteByKey("ui.toggle.knob");
                    if (track != null && knobSprite != null)
                    {
                        image.sprite = track;
                        image.type = Image.Type.Simple;
                        image.preserveAspect = true;
                        image.color = Color.white;

                        var knob = EnsureToggleKnobImage(image, knobSprite);
                        if (knob != null)
                        {
                            knob.gameObject.SetActive(true);
                            LayoutSplitToggle(image.rectTransform, knob.rectTransform, isOn);
                        }

                        if (button != null)
                        {
                            button.targetGraphic = image;
                            button.transition = Selectable.Transition.ColorTint;
                        }
                        return;
                    }

                    var full = LoopSortingUIKit.LoadSpriteByKey(isOn ? "ui.toggle.full_on" : "ui.toggle.full_off");
                    if (full != null)
                    {
                        image.sprite = full;
                        image.type = Image.Type.Simple;
                        image.preserveAspect = true;
                        image.color = Color.white;
                        HideKnob();

                        if (button != null)
                        {
                            button.targetGraphic = image;
                            button.transition = Selectable.Transition.ColorTint;
                        }
                        return;
                    }
                }

                var onSprite = TryLoadSettingsPageSprite("toggle_on");
                var offSprite = TryLoadSettingsPageSprite("toggle_off");
                var pressedOn = TryLoadSettingsPageSprite("toggle_on_pressed");
                var pressedOff = TryLoadSettingsPageSprite("toggle_off_pressed");
                var sprite = isOn ? onSprite : offSprite;
                var pressed = isOn ? pressedOn : pressedOff;

                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;
                    image.color = Color.white;
                    HideKnob();

                    if (button != null)
                    {
                        button.targetGraphic = image;
                        if (pressed != null)
                        {
                            button.transition = Selectable.Transition.SpriteSwap;
                            var state = button.spriteState;
                            state.pressedSprite = pressed;
                            state.highlightedSprite = sprite;
                            state.disabledSprite = sprite;
                            button.spriteState = state;
                        }
                        else
                        {
                            button.transition = Selectable.Transition.ColorTint;
                        }
                    }
                    return;
                }

                image.sprite = null;
                image.color = isOn ? new Color(0.2f, 0.75f, 0.2f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
                HideKnob();

                if (button != null)
                {
                    button.targetGraphic = image;
                    button.transition = Selectable.Transition.ColorTint;
                }
            }
        }
    }
}
