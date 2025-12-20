using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private void EnsureSettingsUI()
        {
            if (_uiCanvas == null) return;
            if (_settingsPanel != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            if (TryInstantiateUiPrefab(SettingsPanelPrefabResourcePath, out SettingsPanelPrefabRefs prefab))
            {
                prefab.AutoAssign();

                _settingsPanel = prefab.gameObject;
                _settingsMusicToggleButton = prefab.musicToggleButton;
                _settingsMusicToggleImage = prefab.musicToggleImage;
                _settingsSfxToggleButton = prefab.sfxToggleButton;
                _settingsSfxToggleImage = prefab.sfxToggleImage;
                _settingsVibrationToggleButton = prefab.vibrationToggleButton;
                _settingsVibrationToggleImage = prefab.vibrationToggleImage;
                _settingsCloseButton = prefab.closeButton;
                _settingsCloseImage = prefab.closeImage;
                _settingsRetryButton = prefab.retryButton;
                _settingsRetryImage = prefab.retryImage;

                if (_settingsCloseButton != null)
                {
                    _settingsCloseButton.onClick.AddListener(() => SettingsUi.Toggle(false));
                }
                if (_settingsRetryButton != null)
                {
                    _settingsRetryButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.LevelRetry);
                        SettingsUi.HideImmediate();
                        RestartCurrent();
                    });
                }
                if (_settingsMusicToggleButton != null)
                {
                    _settingsMusicToggleButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        musicEnabled = !musicEnabled;
                        EnsureMusic();
                        SettingsUi.RefreshToggles();
                        RequestSave(SaveDelayStrongSeconds);
                    });
                }
                if (_settingsSfxToggleButton != null)
                {
                    _settingsSfxToggleButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        soundEnabled = !soundEnabled;
                        EnsureSfx();
                        SettingsUi.RefreshToggles();
                        RequestSave(SaveDelayStrongSeconds);
                    });
                }
                if (_settingsVibrationToggleButton != null)
                {
                    _settingsVibrationToggleButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        vibrationEnabled = !vibrationEnabled;
                        if (vibrationEnabled) TryVibrate();
                        SettingsUi.RefreshToggles();
                        RequestSave(SaveDelayStrongSeconds);
                    });
                }

                ApplyButtonPressScale(_settingsMusicToggleButton, pressedScale: 0.96f);
                ApplyButtonPressScale(_settingsSfxToggleButton, pressedScale: 0.96f);
                ApplyButtonPressScale(_settingsVibrationToggleButton, pressedScale: 0.96f);

                RebindSettingsPanelPrefabSprites(prefab, hasKit);
                SettingsUi.RefreshToggles();

                _settingsPopupRect = prefab.popupRect;
                _settingsTitleRect = FindRectTransformByName(_settingsPanel != null ? _settingsPanel.transform : null, "Title");
                _settingsCloseRect = _settingsCloseButton != null ? _settingsCloseButton.GetComponent<RectTransform>() : null;
                _settingsRetryRect = _settingsRetryButton != null ? _settingsRetryButton.GetComponent<RectTransform>() : null;
                _settingsMusicRowRect = _settingsMusicToggleButton != null ? _settingsMusicToggleButton.transform.parent as RectTransform : null;
                _settingsSfxRowRect = _settingsSfxToggleButton != null ? _settingsSfxToggleButton.transform.parent as RectTransform : null;
                _settingsVibrationRowRect = _settingsVibrationToggleButton != null ? _settingsVibrationToggleButton.transform.parent as RectTransform : null;
                _settingsBasePoseCaptured = false;
                SettingsUi.CaptureBasePose();

                _settingsPanel.SetActive(false);
                return;
            }

            _settingsPanel = new GameObject("SettingsPanel");
            _settingsPanel.transform.SetParent(_uiCanvas.transform, false);
            _settingsMusicToggleImage = null;
            _settingsMusicToggleButton = null;
            _settingsSfxToggleImage = null;
            _settingsSfxToggleButton = null;
            _settingsVibrationToggleImage = null;
            _settingsVibrationToggleButton = null;
            _settingsCloseButton = null;
            _settingsCloseImage = null;
            _settingsRetryButton = null;
            _settingsRetryImage = null;

            var dim = _settingsPanel.AddComponent<Image>();
            dim.raycastTarget = true;
            // Use a solid full-screen dim (no sprite) to avoid accidental gradients/alpha artifacts from themed overlay sprites.
            dim.sprite = null;
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            var overlayRect = _settingsPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var popupGO = new GameObject("Popup");
            popupGO.transform.SetParent(_settingsPanel.transform, false);
            var popupRect = popupGO.AddComponent<RectTransform>();
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = ModalPopupAnchoredPos;

            float popupWidth = ModalPopupSize.x;
            float popupHeight = ModalPopupSize.y;
            popupRect.sizeDelta = new Vector2(popupWidth, popupHeight);

            var bgImg = popupGO.AddComponent<Image>();
            bgImg.raycastTarget = false;
            if (hasKit)
            {
                bgImg.preserveAspect = false;
                bgImg.color = Color.white;

                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                ApplySplitBackground(
                    baseImage: bgImg,
                    parent: popupGO.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/panel_modal_base_9slice.png",
                    decorPath: null,
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(1f, 1f, 1f, 0.92f));

                var layoutParent = TryCreatePaddingTrimmedLayoutRoot(
                    parent: popupGO.transform,
                    panelRect: popupRect,
                    sprite: bgImg.sprite,
                    desiredVisibleSizeUnits: new Vector2(popupWidth, popupHeight),
                    centerStretchFraction: 1f / 3f);

                var titleGO = new GameObject("Title");
                titleGO.transform.SetParent(layoutParent, false);
                var title = titleGO.AddComponent<TextMeshProUGUI>();
                title.raycastTarget = false;
                title.text = "SETTINGS";
                title.alignment = TextAlignmentOptions.Center;
                title.fontSize = 68;
                title.color = new Color(0.24f, 0.14f, 0.08f, 0.96f);
                title.outlineWidth = 0.18f;
                title.outlineColor = new Color(1f, 1f, 1f, 0.6f);
                var titleRect = titleGO.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -130f);
                titleRect.sizeDelta = new Vector2(760f, 110f);

                var closeBtn = CreateIconButton(
                    parent: layoutParent,
                    name: "CloseButton",
                    anchor: new Vector2(1f, 1f),
                    anchoredPos: ModalCloseInset,
                    size: new Vector2(128f, 128f),
                    normal: "ui.button.close_red.normal",
                    pressed: "ui.button.close_red.pressed",
                    disabled: "ui.button.close_red.disabled",
                    icon: "ui.icon.close");
                var closeRect = closeBtn.GetComponent<RectTransform>();
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = ModalCloseInset;
                _settingsCloseButton = closeBtn;
                _settingsCloseImage = closeBtn.GetComponent<Image>();
                _settingsCloseButton.onClick.AddListener(() => SettingsUi.Toggle(false));

                void CreateToggleRow(string label, float topY, out Button button, out Image image)
                {
                    var rowGO = new GameObject(label + "Row");
                    rowGO.transform.SetParent(layoutParent, false);
                    var rowRect = rowGO.AddComponent<RectTransform>();
                    rowRect.anchorMin = new Vector2(0.5f, 1f);
                    rowRect.anchorMax = new Vector2(0.5f, 1f);
                    rowRect.pivot = new Vector2(0.5f, 1f);
                    rowRect.anchoredPosition = new Vector2(0f, topY);
                    rowRect.sizeDelta = new Vector2(760f, 140f);

                    var textGO = new GameObject("Label");
                    textGO.transform.SetParent(rowGO.transform, false);
                    var tmp = textGO.AddComponent<TextMeshProUGUI>();
                    tmp.raycastTarget = false;
                    tmp.text = label;
                    tmp.alignment = TextAlignmentOptions.Left;
                    tmp.fontSize = 52;
                    tmp.color = new Color(0.24f, 0.14f, 0.08f, 0.96f);
                    var tRect = textGO.GetComponent<RectTransform>();
                    tRect.anchorMin = new Vector2(0f, 0.5f);
                    tRect.anchorMax = new Vector2(0f, 0.5f);
                    tRect.pivot = new Vector2(0f, 0.5f);
                    tRect.anchoredPosition = new Vector2(30f, 0f);
                    tRect.sizeDelta = new Vector2(420f, 90f);

                    var toggleGO = new GameObject("Toggle");
                    toggleGO.transform.SetParent(rowGO.transform, false);
                    var toggleRect = toggleGO.AddComponent<RectTransform>();
                    toggleRect.anchorMin = new Vector2(1f, 0.5f);
                    toggleRect.anchorMax = new Vector2(1f, 0.5f);
                    toggleRect.pivot = new Vector2(1f, 0.5f);
                    toggleRect.anchoredPosition = new Vector2(-50f, 0f);
                    toggleRect.sizeDelta = new Vector2(240f, 100f);

                    image = toggleGO.AddComponent<Image>();
                    image.raycastTarget = true;
                    image.color = Color.white;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = true;

                    button = toggleGO.AddComponent<Button>();
                    button.targetGraphic = image;
                    button.transition = Selectable.Transition.ColorTint;
                    ApplyButtonPressScale(button, pressedScale: 0.96f);
                }

                CreateToggleRow("MUSIC", topY: -320f, out _settingsMusicToggleButton, out _settingsMusicToggleImage);
                CreateToggleRow("SFX", topY: -490f, out _settingsSfxToggleButton, out _settingsSfxToggleImage);
                CreateToggleRow("VIBRATION", topY: -660f, out _settingsVibrationToggleButton, out _settingsVibrationToggleImage);

                _settingsMusicToggleButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    musicEnabled = !musicEnabled;
                    EnsureMusic();
                    SettingsUi.RefreshToggles();
                    RequestSave(SaveDelayStrongSeconds);
                });
                _settingsSfxToggleButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    soundEnabled = !soundEnabled;
                    EnsureSfx();
                    SettingsUi.RefreshToggles();
                    RequestSave(SaveDelayStrongSeconds);
                });
                _settingsVibrationToggleButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    vibrationEnabled = !vibrationEnabled;
                    if (vibrationEnabled) TryVibrate();
                    SettingsUi.RefreshToggles();
                    RequestSave(SaveDelayStrongSeconds);
                });

                TMP_Text retryLabelText;
                _settingsRetryButton = CreateLongButton(
                    parent: layoutParent,
                    name: "RetryButton",
                    anchor: new Vector2(0.5f, 0f),
                    size: new Vector2(760f, 180f),
                    normal: "ui.button.orange_long.normal",
                    pressed: "ui.button.orange_long.pressed",
                    disabled: "ui.button.orange_long.disabled",
                    label: "RETRY",
                    out retryLabelText,
                    reserveIconSpace: false);
                _settingsRetryImage = _settingsRetryButton != null ? _settingsRetryButton.GetComponent<Image>() : null;
                if (_settingsRetryButton != null)
                {
                    var rr = _settingsRetryButton.GetComponent<RectTransform>();
                    rr.pivot = new Vector2(0.5f, 0f);
                    rr.anchoredPosition = new Vector2(0f, 140f);
                    _settingsRetryButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.LevelRetry);
                        SettingsUi.HideImmediate();
                        RestartCurrent();
                    });
                }

                SettingsUi.RefreshToggles();

                _settingsPopupRect = popupRect;
                _settingsTitleRect = titleRect;
                _settingsCloseRect = closeRect;
                _settingsRetryRect = _settingsRetryButton != null ? _settingsRetryButton.GetComponent<RectTransform>() : null;
                _settingsMusicRowRect = _settingsMusicToggleButton != null ? _settingsMusicToggleButton.transform.parent as RectTransform : null;
                _settingsSfxRowRect = _settingsSfxToggleButton != null ? _settingsSfxToggleButton.transform.parent as RectTransform : null;
                _settingsVibrationRowRect = _settingsVibrationToggleButton != null ? _settingsVibrationToggleButton.transform.parent as RectTransform : null;
                _settingsBasePoseCaptured = false;
                SettingsUi.CaptureBasePose();

                _settingsPanel.SetActive(false);
                return;
            }

            Sprite settingsSprite = Resources.Load<Sprite>("setting_page");
            if (settingsSprite == null)
            {
                var settingsTex = Resources.Load<Texture2D>("setting_page");
                if (settingsTex != null)
                {
                    settingsSprite = Sprite.Create(
                        settingsTex,
                        new Rect(0, 0, settingsTex.width, settingsTex.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    settingsSprite.name = "setting_page";
                }
            }

            if (settingsSprite != null && settingsSprite.rect.width > 0.01f)
            {
                float aspect = settingsSprite.rect.height / settingsSprite.rect.width;
                popupHeight = popupWidth * aspect;
                popupRect.sizeDelta = new Vector2(popupWidth, popupHeight);
            }

            bgImg.sprite = settingsSprite;
            bgImg.color = Color.white;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = true;

            static bool TryExtractInt(string json, string pattern, out int value)
            {
                value = 0;
                var m = System.Text.RegularExpressions.Regex.Match(json, pattern, System.Text.RegularExpressions.RegexOptions.Singleline);
                if (!m.Success || m.Groups.Count < 2) return false;
                return int.TryParse(m.Groups[1].Value, out value);
            }

            static bool TryExtractRect(string json, string key, out RectInt rect)
            {
                rect = new RectInt();
                var m = System.Text.RegularExpressions.Regex.Match(
                    json,
                    $"\"{System.Text.RegularExpressions.Regex.Escape(key)}\"\\s*:\\s*\\{{[^}}]*?\"x\"\\s*:\\s*(\\d+)\\s*,[^}}]*?\"y\"\\s*:\\s*(\\d+)\\s*,[^}}]*?\"w\"\\s*:\\s*(\\d+)\\s*,[^}}]*?\"h\"\\s*:\\s*(\\d+)",
                    System.Text.RegularExpressions.RegexOptions.Singleline);

                if (!m.Success || m.Groups.Count < 5) return false;
                if (!int.TryParse(m.Groups[1].Value, out int x)) return false;
                if (!int.TryParse(m.Groups[2].Value, out int y)) return false;
                if (!int.TryParse(m.Groups[3].Value, out int w)) return false;
                if (!int.TryParse(m.Groups[4].Value, out int h)) return false;
                rect = new RectInt(x, y, w, h);
                return true;
            }

            int sourceW = 902;
            int sourceH = 1233;
            RectInt rectClose = new RectInt(761, 2, 143, 144);
            RectInt rectRetry = new RectInt(119, 929, 632, 244);
            RectInt rectMusic = new RectInt(572, 384, 221, 132);
            RectInt rectSfx = new RectInt(572, 562, 221, 132);
            RectInt rectVibration = new RectInt(572, 750, 221, 131);

            var manifest = Resources.Load<TextAsset>("setting_page_assets/assets_manifest");
            if (manifest != null && !string.IsNullOrWhiteSpace(manifest.text))
            {
                TryExtractInt(manifest.text, "\"source_size\"\\s*:\\s*\\[\\s*(\\d+)\\s*,", out sourceW);
                TryExtractInt(manifest.text, "\"source_size\"\\s*:\\s*\\[\\s*\\d+\\s*,\\s*(\\d+)\\s*\\]", out sourceH);

                TryExtractRect(manifest.text, "btn_close", out rectClose);
                TryExtractRect(manifest.text, "btn_retry", out rectRetry);
                TryExtractRect(manifest.text, "toggle_music", out rectMusic);
                TryExtractRect(manifest.text, "toggle_sound_effects", out rectSfx);
                TryExtractRect(manifest.text, "toggle_vibration", out rectVibration);
            }

            void PlaceRect(RectTransform rect, RectInt srcRect)
            {
                float cx = srcRect.x + srcRect.width * 0.5f;
                float cy = srcRect.y + srcRect.height * 0.5f;
                float nx = sourceW > 0 ? (cx / sourceW) : 0.5f;
                float ny = sourceH > 0 ? (1f - (cy / sourceH)) : 0.5f;

                rect.anchoredPosition = new Vector2((nx - 0.5f) * popupWidth, (ny - 0.5f) * popupHeight);
                rect.sizeDelta = new Vector2(
                    (sourceW > 0 ? (srcRect.width / (float)sourceW) : 0.2f) * popupWidth,
                    (sourceH > 0 ? (srcRect.height / (float)sourceH) : 0.1f) * popupHeight);
            }

            void CreateOverlayButton(string name, RectInt srcRect, out Button button, out Image image)
            {
                var go = new GameObject(name);
                go.transform.SetParent(popupGO.transform, false);
                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                PlaceRect(rect, srcRect);

                image = go.AddComponent<Image>();
                image.raycastTarget = true;
                image.color = Color.white;
                image.preserveAspect = true;

                button = go.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.SpriteSwap;
                ApplyButtonPressScale(button, pressedScale: 0.96f);
            }

            CreateOverlayButton("CloseButton", rectClose, out _settingsCloseButton, out _settingsCloseImage);
            bool usingCloseBase = false;
            var closeBase = TryLoadSettingsPageSprite("btn_close_base");
            if (closeBase != null) usingCloseBase = true;
            closeBase = closeBase ?? TryLoadSettingsPageSprite("btn_close");
            var closePressed = TryLoadSettingsPageSprite("btn_close_base_pressed") ?? TryLoadSettingsPageSprite("btn_close_pressed");
            if (_settingsCloseImage != null)
            {
                if (closeBase != null)
                {
                    _settingsCloseImage.sprite = closeBase;
                    _settingsCloseImage.color = Color.white;
                    _settingsCloseImage.preserveAspect = true;
                }
                else
                {
                    _settingsCloseImage.sprite = null;
                    _settingsCloseImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
            if (_settingsCloseButton != null)
            {
                if (closeBase != null && closePressed != null)
                {
                    _settingsCloseButton.transition = Selectable.Transition.SpriteSwap;
                    _settingsCloseButton.spriteState = new SpriteState { pressedSprite = closePressed };
                }
                else
                {
                    _settingsCloseButton.transition = Selectable.Transition.ColorTint;
                }
            }
            if (usingCloseBase && hasKit && _settingsCloseImage != null)
            {
                var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                if (iconSprite != null)
                {
                    var iconImg = EnsureOverlayImage(_settingsCloseImage.transform, "Icon", iconSprite);
                    if (iconImg != null)
                    {
                        iconImg.preserveAspect = true;
                        var r = iconImg.rectTransform;
                        float side = Mathf.Min(_settingsCloseImage.rectTransform.rect.width, _settingsCloseImage.rectTransform.rect.height) * 0.62f;
                        r.anchorMin = new Vector2(0.5f, 0.5f);
                        r.anchorMax = new Vector2(0.5f, 0.5f);
                        r.pivot = new Vector2(0.5f, 0.5f);
                        r.anchoredPosition = Vector2.zero;
                        r.sizeDelta = new Vector2(side, side);
                    }
                }
            }
            _settingsCloseButton.onClick.AddListener(() => SettingsUi.Toggle(false));

            CreateOverlayButton("RetryButton", rectRetry, out _settingsRetryButton, out _settingsRetryImage);
            bool usingRetryBase = false;
            var retryBase = TryLoadSettingsPageSprite("btn_retry_base_normal");
            if (retryBase != null) usingRetryBase = true;
            retryBase = retryBase ?? TryLoadSettingsPageSprite("btn_retry") ?? TryLoadSettingsPageSprite("btn_retry_base");
            var retryPressed = TryLoadSettingsPageSprite("btn_retry_base_pressed") ?? TryLoadSettingsPageSprite("btn_retry_pressed");
            if (_settingsRetryImage != null)
            {
                if (retryBase != null)
                {
                    _settingsRetryImage.sprite = retryBase;
                    _settingsRetryImage.color = Color.white;
                    _settingsRetryImage.preserveAspect = true;
                }
                else
                {
                    _settingsRetryImage.sprite = null;
                    _settingsRetryImage.color = new Color(1f, 1f, 1f, 0f);
                }
            }
            if (_settingsRetryButton != null)
            {
                if (retryBase != null && retryPressed != null)
                {
                    _settingsRetryButton.transition = Selectable.Transition.SpriteSwap;
                    _settingsRetryButton.spriteState = new SpriteState { pressedSprite = retryPressed };
                }
                else
                {
                    _settingsRetryButton.transition = Selectable.Transition.ColorTint;
                }
            }
            if (usingRetryBase && _settingsRetryImage != null)
            {
                var textGO = new GameObject("Text");
                textGO.transform.SetParent(_settingsRetryImage.transform, false);
                var tmp = textGO.AddComponent<TextMeshProUGUI>();
                tmp.raycastTarget = false;
                tmp.text = "RETRY";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 64;
                tmp.color = new Color(0.35f, 0.22f, 0.12f, 1f);
                var tr = tmp.GetComponent<RectTransform>();
                tr.anchorMin = Vector2.zero;
                tr.anchorMax = Vector2.one;
                tr.offsetMin = new Vector2(20f, 10f);
                tr.offsetMax = new Vector2(-20f, -10f);
            }
            _settingsRetryButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.LevelRetry);
                SettingsUi.HideImmediate();
                RestartCurrent();
            });

            CreateOverlayButton("ToggleMusic", rectMusic, out _settingsMusicToggleButton, out _settingsMusicToggleImage);
            _settingsMusicToggleButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiClick);
                musicEnabled = !musicEnabled;
                EnsureMusic();
                SettingsUi.RefreshToggles();
                RequestSave(SaveDelayStrongSeconds);
            });

            CreateOverlayButton("ToggleSfx", rectSfx, out _settingsSfxToggleButton, out _settingsSfxToggleImage);
            _settingsSfxToggleButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiClick);
                soundEnabled = !soundEnabled;
                EnsureSfx();
                SettingsUi.RefreshToggles();
                RequestSave(SaveDelayStrongSeconds);
            });

            CreateOverlayButton("ToggleVibration", rectVibration, out _settingsVibrationToggleButton, out _settingsVibrationToggleImage);
            _settingsVibrationToggleButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiClick);
                vibrationEnabled = !vibrationEnabled;
                if (vibrationEnabled)
                {
                    TryVibrate();
                }
                SettingsUi.RefreshToggles();
                RequestSave(SaveDelayStrongSeconds);
            });

            SettingsUi.RefreshToggles();

            _settingsPopupRect = popupRect;
            _settingsTitleRect = null;
            _settingsCloseRect = _settingsCloseButton != null ? _settingsCloseButton.GetComponent<RectTransform>() : null;
            _settingsRetryRect = _settingsRetryButton != null ? _settingsRetryButton.GetComponent<RectTransform>() : null;
            _settingsMusicRowRect = _settingsMusicToggleButton != null ? _settingsMusicToggleButton.GetComponent<RectTransform>() : null;
            _settingsSfxRowRect = _settingsSfxToggleButton != null ? _settingsSfxToggleButton.GetComponent<RectTransform>() : null;
            _settingsVibrationRowRect = _settingsVibrationToggleButton != null ? _settingsVibrationToggleButton.GetComponent<RectTransform>() : null;
            _settingsBasePoseCaptured = false;
            SettingsUi.CaptureBasePose();

            _settingsPanel.SetActive(false);
        }

    }
}





