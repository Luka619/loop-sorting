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
        private void EnsureCounterUI()
        {
            if (_uiCanvas != null && beltCounterUI != null && _speedButton != null && _resultPanel != null && _settingsButton != null && _boosterPanel != null && _boosterSortButton != null && _boosterShuffleButton != null)
            {
                if (_currencyFlyFx == null) _currencyFlyFx = _uiCanvas.GetComponent<CurrencyFlyFx>();
                if (_currencyFlyFx == null) _currencyFlyFx = _uiCanvas.gameObject.AddComponent<CurrencyFlyFx>();
                if (_hudRootRect == null)
                {
                    var hudRoot = _uiCanvas.transform.Find("HUDRoot");
                    if (hudRoot != null) _hudRootRect = hudRoot.GetComponent<RectTransform>();
                }
                return;
            }

            // If something is partially built, rebuild cleanly.
            if (_uiCanvas != null)
            {
                Destroy(_uiCanvas.gameObject);
                _uiCanvas = null;
                _currencyFlyFx = null;
                _hudRootRect = null;
                _lockChipLayer = null;
                _lockChipByBox.Clear();
                _tutorialLayer = null;
                _tutorialBubble = null;
                _tutorialText = null;
                _tutorialBubbleBg = null;
            }

            beltCounterUI = null;
            _speedButton = null;
            _speedButtonLabel = null;
            _settingsButton = null;
            _shopButton = null;
            _levelHudText = null;
            _coinText = null;
            _lifeText = null;
            _coinPlusButton = null;
            _lifePlusButton = null;
            _shopPanel = null;
            _shopTitle = null;
            _shopCoinValue = null;
            _shopLifeValue = null;
            _shopScroll = null;
            _shopScrollFadeTop = null;
            _shopScrollFadeBottom = null;
            _shopContentRoot = null;
            _boosterPanel = null;
            _boosterSortButton = null;
            _boosterShuffleButton = null;
            _fastTag = null;
            _fastTagBg = null;
            _fastTagText = null;

            var canvasGO = new GameObject("HUDCanvas");
            _uiCanvas = canvasGO.AddComponent<Canvas>();
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _uiCanvas.overrideSorting = true;
            _uiCanvas.sortingOrder = 0;
            _currencyFlyFx = canvasGO.AddComponent<CurrencyFlyFx>();

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            var screenAdapter = canvasGO.AddComponent<LoopSortingScreenAdapter>();
            screenAdapter.canvasScaler = scaler;
            screenAdapter.referenceResolution = scaler.referenceResolution;
            // Route-2 safe-area strategy for HUD: do NOT shrink the whole UI into safeArea.
            // Instead, keep full-screen coordinates and apply safe insets as padding to edge UI elements.
            screenAdapter.safeAreaRect = null;
            screenAdapter.Refresh();

            float safeTopUnits = 0f;
            float safeBottomUnits = 0f;
            float safeRightUnits = 0f;
            float capsuleRightUnits = 0f;
            float capsuleBottomFromTopUnits = 0f;
            if (screenAdapter != null)
            {
                float sf = ComputeCanvasScaleFactor(scaler);
                if (sf > 0.0001f)
                {
                    safeTopUnits = screenAdapter.RawSafeAreaInsetsPx.z / sf;
                    safeBottomUnits = screenAdapter.RawSafeAreaInsetsPx.w / sf;
                    safeRightUnits = screenAdapter.RawSafeAreaInsetsPx.y / sf;
                    capsuleRightUnits = screenAdapter.MenuButtonRightInsetPx / sf;
                    safeTopUnits = Mathf.Max(safeTopUnits, screenAdapter.StatusBarHeightPx / sf);

                    var menuRect = screenAdapter.MenuButtonRectPx;
                    if (menuRect.width > 1f && menuRect.height > 1f)
                    {
                        float menuBottomFromTopPx = Screen.height - menuRect.yMin;
                        capsuleBottomFromTopUnits = menuBottomFromTopPx / sf;
                    }
                }
            }
            safeTopUnits = Mathf.Clamp(safeTopUnits, 0f, 260f);
            safeBottomUnits = Mathf.Clamp(safeBottomUnits, 0f, 260f);
            safeRightUnits = Mathf.Clamp(safeRightUnits, 0f, 420f);
            capsuleRightUnits = Mathf.Clamp(capsuleRightUnits, 0f, 420f);
            capsuleBottomFromTopUnits = Mathf.Clamp(capsuleBottomFromTopUnits, 0f, 420f);

            bool isPortrait = Screen.height >= Screen.width;

            // WeChat capsule handling: prefer pushing the TopBar down instead of shrinking it left.
            float topBarTopUnits = safeTopUnits;
            float topBarExtraRightUnits = safeRightUnits;

            if (isPortrait && screenAdapter != null && screenAdapter.ignoreHorizontalInsetsInPortrait)
            {
                // In portrait we intentionally ignore horizontal safe-area insets to keep the HUD centered.
                topBarExtraRightUnits = 0f;
            }

            // If the platform reports a large right inset (typical for WeChat capsule) in portrait, push the TopBar down.
            float capsuleLikelyUnits = Mathf.Max(capsuleRightUnits, safeRightUnits);
            if (isPortrait && capsuleLikelyUnits >= 80f)
            {
                const float capsuleGapUnits = 12f;
                if (capsuleBottomFromTopUnits > 1f)
                {
                    topBarTopUnits = Mathf.Max(topBarTopUnits, capsuleBottomFromTopUnits + capsuleGapUnits);
                }
                else
                {
                    // Fallback: if we can¡¯t locate the capsule rect, nudge down conservatively rather than shifting left.
                    topBarTopUnits = Mathf.Max(topBarTopUnits, safeTopUnits + 120f);
                }

                // Keep the TopBar on the right edge when it¡¯s below the capsule.
                topBarExtraRightUnits = 0f;
            }

            topBarTopUnits = Mathf.Clamp(topBarTopUnits, 0f, 420f);

            bool hasKit = LoopSortingUIKit.IsAvailable();
            EnsureEconomyDefaults();

            _hudTopInsetUnits = topBarTopUnits;
            _hudRightInsetUnits = topBarExtraRightUnits;
            _hudBottomInsetUnits = safeBottomUnits;

            // Prefer prefab-driven HUD so layout can be tweaked manually in the editor.
            if (TryInstantiateUiPrefab(GameplayHudPrefabResourcePath, out GameplayHudPrefabRefs hudPrefab))
            {
                hudPrefab.AutoAssign();
                RebindGameplayHudPrefabSprites(hudPrefab, hasKit);

                _hudRootRect = hudPrefab.rootRect != null ? hudPrefab.rootRect : hudPrefab.GetComponent<RectTransform>();
                if (_hudRootRect != null)
                {
                    _hudRootRect.anchorMin = Vector2.zero;
                    _hudRootRect.anchorMax = Vector2.one;
                    _hudRootRect.offsetMin = Vector2.zero;
                    _hudRootRect.offsetMax = Vector2.zero;
                }

                beltCounterUI = hudPrefab.beltCounterUI;
                _levelHudText = hudPrefab.levelText;
                _shopButton = hudPrefab.shopButton;
                _coinText = hudPrefab.coinText;
                _coinPlusButton = hudPrefab.coinPlusButton;
                _lifeText = hudPrefab.lifeText;
                _lifePlusButton = hudPrefab.lifePlusButton;
                _speedButton = hudPrefab.speedButton;
                _speedButtonLabel = hudPrefab.speedLabel;
                _settingsButton = hudPrefab.settingsButton;
                _boosterPanel = hudPrefab.boosterPanel;
                _boosterSortButton = hudPrefab.boosterSortButton;
                _boosterShuffleButton = hudPrefab.boosterShuffleButton;

                // Apply current feature toggles to prefab content.
                var shopBtnT = _hudRootRect != null ? _hudRootRect.Find("ShopButton") : null;
                if (shopBtnT != null) shopBtnT.gameObject.SetActive(shopEnabled);

                var livesPillT = _hudRootRect != null ? _hudRootRect.Find("LivesPill") : null;
                if (livesPillT != null) livesPillT.gameObject.SetActive(livesHudEnabled);

                var coinPlusT = _hudRootRect != null ? _hudRootRect.Find("CoinsPill/Plus") : null;
                if (coinPlusT != null) coinPlusT.gameObject.SetActive(shopEnabled);

                var lifePlusT = _hudRootRect != null ? _hudRootRect.Find("LivesPill/Plus") : null;
                if (lifePlusT != null) lifePlusT.gameObject.SetActive(shopEnabled && livesHudEnabled);

                // Safe-area adjustments: nudge top HUD down, and boosters up from bottom inset.
                float topDelta = topBarTopUnits - hudPrefab.authoredTopInsetUnits;
                float rightDelta = topBarExtraRightUnits - hudPrefab.authoredRightInsetUnits;
                float bottomDelta = safeBottomUnits - hudPrefab.authoredBottomInsetUnits;

                void NudgeTop(string childName, bool applyRightInset)
                {
                    if (_hudRootRect == null || string.IsNullOrEmpty(childName)) return;
                    var t = _hudRootRect.Find(childName);
                    if (t == null) return;
                    var rt = t.GetComponent<RectTransform>();
                    if (rt == null) return;
                    var p = rt.anchoredPosition;
                    p.y -= topDelta;
                    if (applyRightInset) p.x -= rightDelta;
                    rt.anchoredPosition = p;
                }

                NudgeTop("FreeSlotsCounter", applyRightInset: false);
                NudgeTop("LevelLabel", applyRightInset: false);
                NudgeTop("ShopButton", applyRightInset: false);
                NudgeTop("CoinsPill", applyRightInset: true);
                NudgeTop("LivesPill", applyRightInset: true);
                NudgeTop("SpeedButton", applyRightInset: true);
                NudgeTop("SettingsButton", applyRightInset: true);
                NudgeTop("FastTag", applyRightInset: false);

                void NudgeBooster(Button b)
                {
                    if (b == null) return;
                    var rt = b.GetComponent<RectTransform>();
                    if (rt == null) return;
                    var p = rt.anchoredPosition;
                    p.y += bottomDelta;
                    rt.anchoredPosition = p;
                }

                NudgeBooster(_boosterSortButton);
                NudgeBooster(_boosterShuffleButton);

                // Rebind button actions (prefabs are layout-only).
                if (_speedButton != null)
                {
                    _speedButton.onClick.RemoveAllListeners();
                    _speedButton.onClick.AddListener(CycleSpeed);
                    ApplyButtonPressScale(_speedButton, pressedScale: 0.96f);
                }
                if (_settingsButton != null)
                {
                    _settingsButton.onClick.RemoveAllListeners();
                    _settingsButton.onClick.AddListener(() => SettingsUi.Toggle(true));
                    ApplyButtonPressScale(_settingsButton, pressedScale: 0.96f);
                }
                if (_shopButton != null)
                {
                    _shopButton.onClick.RemoveAllListeners();
                    _shopButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        OpenShop(ShopTab.Coins);
                    });
                    ApplyButtonPressScale(_shopButton, pressedScale: 0.96f);
                }
                if (_coinPlusButton != null)
                {
                    _coinPlusButton.onClick.RemoveAllListeners();
                    _coinPlusButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        OpenShop(ShopTab.Coins);
                    });
                    ApplyButtonPressScale(_coinPlusButton, pressedScale: 0.96f);
                }
                if (_lifePlusButton != null)
                {
                    _lifePlusButton.onClick.RemoveAllListeners();
                    _lifePlusButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiClick);
                        OpenShop(ShopTab.Lives);
                    });
                    ApplyButtonPressScale(_lifePlusButton, pressedScale: 0.96f);
                }
                if (_boosterSortButton != null)
                {
                    _boosterSortButton.onClick.RemoveAllListeners();
                    _boosterSortButton.onClick.AddListener(() => HandleBoosterButtonClick(BoosterType.Sort));
                    ApplyButtonPressScale(_boosterSortButton, pressedScale: 0.96f);
                    if (hasKit) AttachBoosterBadge(_boosterSortButton.transform, _progress.BoosterSortCount);
                }
                if (_boosterShuffleButton != null)
                {
                    _boosterShuffleButton.onClick.RemoveAllListeners();
                    _boosterShuffleButton.onClick.AddListener(() => HandleBoosterButtonClick(BoosterType.Shuffle));
                    ApplyButtonPressScale(_boosterShuffleButton, pressedScale: 0.96f);
                    if (hasKit) AttachBoosterBadge(_boosterShuffleButton.transform, _progress.BoosterShuffleCount);
                }

                UpdateSpeedButtonLabel();
                RefreshEconomyHUD();

                EnsureResultPanel();
                if (shopEnabled) EnsureShopUI();
                EnsureBoosterPurchaseUI();
                return;
            }

            // Root helper
            var root = new GameObject("HUDRoot");
            root.transform.SetParent(canvasGO.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            _hudRootRect = rootRect;

            var uiLayout = LoopSortingUIKit.GetRuntimeLayout();

            void PlaceTopLeft(RectTransform r, Rect topLeft)
            {
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(topLeft.x, -(topLeft.y + topBarTopUnits));
                r.sizeDelta = new Vector2(topLeft.width, topLeft.height);
            }

            void PlaceTopRight(RectTransform r, Rect topLeft, float refW, float extraRight)
            {
                float right = refW - (topLeft.x + topLeft.width) + extraRight;
                r.anchorMin = new Vector2(1f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(1f, 1f);
                r.anchoredPosition = new Vector2(-right, -(topLeft.y + topBarTopUnits));
                r.sizeDelta = new Vector2(topLeft.width, topLeft.height);
            }

            void PlaceTopCenter(RectTransform r, Rect topLeft, float refW)
            {
                float centerX = (topLeft.x + topLeft.width * 0.5f) - refW * 0.5f;
                r.anchorMin = new Vector2(0.5f, 1f);
                r.anchorMax = new Vector2(0.5f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.anchoredPosition = new Vector2(centerX, -(topLeft.y + topBarTopUnits));
                r.sizeDelta = new Vector2(topLeft.width, topLeft.height);
            }

            // Free slots counter (top-left)
            var counterRoot = new GameObject("FreeSlotsCounter");
            counterRoot.transform.SetParent(root.transform, false);
            var counterRect = counterRoot.AddComponent<RectTransform>();
            counterRect.anchorMin = Vector2.zero;
            counterRect.anchorMax = Vector2.one;
            counterRect.offsetMin = Vector2.zero;
            counterRect.offsetMax = Vector2.zero;

            var counterBgGO = new GameObject("BG");
            counterBgGO.transform.SetParent(counterRoot.transform, false);
            var counterBg = counterBgGO.AddComponent<Image>();
            counterBg.raycastTarget = false;
            if (hasKit)
            {
                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                ApplySplitBackground(
                    baseImage: counterBg,
                    parent: counterBgGO.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                    decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(0.1f, 0.1f, 0.1f, 0.55f));
            }
            else
            {
                counterBg.color = new Color(0.1f, 0.1f, 0.1f, 0.55f);
            }
            var counterBgRect = counterBg.GetComponent<RectTransform>();
            PlaceTopLeft(counterBgRect, uiLayout.counter);
            float counterX = uiLayout.counter.x;
            float counterY = uiLayout.counter.y;

            var counterIconGO = new GameObject("Icon");
            counterIconGO.transform.SetParent(counterBgGO.transform, false);
            var counterIcon = counterIconGO.AddComponent<Image>();
            counterIcon.raycastTarget = false;
            if (hasKit)
            {
                counterIcon.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.counter.icon");
                counterIcon.color = Color.white;
            }
            var counterIconRect = counterIcon.GetComponent<RectTransform>();
            counterIconRect.anchorMin = new Vector2(0f, 0.5f);
            counterIconRect.anchorMax = new Vector2(0f, 0.5f);
            counterIconRect.pivot = new Vector2(0f, 0.5f);
            counterIconRect.anchoredPosition = new Vector2(18f, 0f);
            counterIconRect.sizeDelta = new Vector2(Mathf.Min(84f, uiLayout.counter.height * 0.78f), Mathf.Min(84f, uiLayout.counter.height * 0.78f));

            var counterValueGO = new GameObject("Value");
            counterValueGO.transform.SetParent(counterBgGO.transform, false);
            var counterValue = counterValueGO.AddComponent<TextMeshProUGUI>();
            counterValue.raycastTarget = false;
            counterValue.text = "-";
            counterValue.alignment = TextAlignmentOptions.MidlineLeft;
            counterValue.fontSize = 64;
            counterValue.enableWordWrapping = false;
            counterValue.color = Color.white;
            ApplyTmpOutlineUnderlay(
                counterValue,
                outlineWidth: 0.22f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.32f,
                underlayDilate: 0.05f);
            var counterValueRect = counterValue.GetComponent<RectTransform>();
            counterValueRect.anchorMin = new Vector2(0f, 0f);
            counterValueRect.anchorMax = new Vector2(1f, 1f);
            counterValueRect.offsetMin = new Vector2(110f, 0f);
            counterValueRect.offsetMax = new Vector2(-14f, 0f);

            beltCounterUI = counterValueGO.AddComponent<BeltCounterUI>();

            // Level label (top-center)
            var levelGO = new GameObject("LevelLabel");
            levelGO.transform.SetParent(root.transform, false);
            var levelRectRoot = levelGO.AddComponent<RectTransform>();
            PlaceTopCenter(levelRectRoot, uiLayout.level, uiLayout.referenceWidth);

            var levelBg = levelGO.AddComponent<Image>();
            levelBg.raycastTarget = false;
            if (hasKit)
            {
                levelBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.hud.level_bg");
                if (levelBg.sprite != null)
                {
                    levelBg.type = levelBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    levelBg.color = Color.white;
                }
                else
                {
                    levelBg.color = new Color(0f, 0f, 0f, 0f);
                }
            }
            else
            {
                levelBg.color = new Color(0f, 0f, 0f, 0.25f);
            }

            var levelTextGO = new GameObject("Text");
            levelTextGO.transform.SetParent(levelGO.transform, false);
            _levelHudText = levelTextGO.AddComponent<TextMeshProUGUI>();
            _levelHudText.raycastTarget = false;
            _levelHudText.text = $"LEVEL {(_flow != null ? (_flowIndex + 1) : 1)}";
            _levelHudText.alignment = TextAlignmentOptions.Center;
            _levelHudText.fontSize = 52;
            _levelHudText.enableWordWrapping = false;
            _levelHudText.color = Color.white;
            ApplyTmpOutlineUnderlay(
                _levelHudText,
                outlineWidth: 0.20f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.32f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.30f,
                underlayDilate: 0.04f);
            var levelTextRect = _levelHudText.GetComponent<RectTransform>();
            levelTextRect.anchorMin = Vector2.zero;
            levelTextRect.anchorMax = Vector2.one;
            levelTextRect.offsetMin = new Vector2(16f, 0f);
            levelTextRect.offsetMax = new Vector2(-16f, 0f);

            // Shop button (top-left under pause area, uses placeholder icon if missing)
            if (shopEnabled)
            {
                _shopButton = CreateIconButton(
                    parent: root.transform,
                    name: "ShopButton",
                    anchor: new Vector2(0f, 1f),
                    anchoredPos: new Vector2(uiLayout.shop.x + uiLayout.shop.width * 0.5f, -(uiLayout.shop.y + topBarTopUnits) - uiLayout.shop.height * 0.5f),
                    size: new Vector2(uiLayout.shop.width, uiLayout.shop.height),
                    normal: hasKit ? "ui.button.mint_square.normal" : null,
                    pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                    disabled: hasKit ? "ui.button.mint_square.disabled" : null,
                    icon: hasKit ? "ui.icon.shop" : null);
                _shopButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    OpenShop(ShopTab.Coins);
                });
                ApplyButtonPressScale(_shopButton, pressedScale: 0.96f);
            }

            // Currency pills (top-right): separate coins/lives, no shared background.
            Rect coinsRect = livesHudEnabled ? uiLayout.coins : uiLayout.lives;
            float coinsRight = uiLayout.referenceWidth - (coinsRect.x + coinsRect.width) + topBarExtraRightUnits;
            CreateCurrencyPill(
                parent: root.transform,
                name: "CoinsPill",
                anchor: new Vector2(1f, 1f),
                anchoredPos: new Vector2(-coinsRight, -(coinsRect.y + topBarTopUnits)),
                size: new Vector2(coinsRect.width, coinsRect.height),
                iconKey: "ui.icon.coin",
                showPlusButton: shopEnabled,
                out _coinText,
                out _coinPlusButton);

            if (livesHudEnabled)
            {
                float livesRight = uiLayout.referenceWidth - (uiLayout.lives.x + uiLayout.lives.width) + topBarExtraRightUnits;
                CreateCurrencyPill(
                    parent: root.transform,
                    name: "LivesPill",
                    anchor: new Vector2(1f, 1f),
                    anchoredPos: new Vector2(-livesRight, -(uiLayout.lives.y + topBarTopUnits)),
                    size: new Vector2(uiLayout.lives.width, uiLayout.lives.height),
                    iconKey: "ui.icon.heart",
                    showPlusButton: shopEnabled,
                    out _lifeText,
                    out _lifePlusButton);
            }

            if (_coinPlusButton != null)
            {
                _coinPlusButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    OpenShop(ShopTab.Coins);
                });
                ApplyButtonPressScale(_coinPlusButton, pressedScale: 0.96f);
            }
            if (_lifePlusButton != null)
            {
                _lifePlusButton.onClick.AddListener(() =>
                {
                    PlaySfx(SfxId.UiClick);
                    OpenShop(ShopTab.Lives);
                });
                ApplyButtonPressScale(_lifePlusButton, pressedScale: 0.96f);
            }

            RefreshEconomyHUD();

            // Speed button (top-right)
            var speedGO = new GameObject("SpeedButton");
            speedGO.transform.SetParent(root.transform, false);
            var speedImg = speedGO.AddComponent<Image>();
            _speedButton = speedGO.AddComponent<Button>();
            ApplyUIKitButtonSprites(_speedButton, speedImg,
                normal: hasKit ? "ui.button.mint_square.normal" : null,
                pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                disabled: hasKit ? "ui.button.mint_square.disabled" : null);

            var speedRect = speedGO.GetComponent<RectTransform>();
            PlaceTopRight(speedRect, uiLayout.speed, uiLayout.referenceWidth, topBarExtraRightUnits);

            var speedLabelGO = new GameObject("Label");
            speedLabelGO.transform.SetParent(speedGO.transform, false);
            _speedButtonLabel = speedLabelGO.AddComponent<TextMeshProUGUI>();
            _speedButtonLabel.raycastTarget = false;
            _speedButtonLabel.alignment = TextAlignmentOptions.Center;
            _speedButtonLabel.fontSize = 54;
            _speedButtonLabel.color = Color.white;
            ApplyTmpOutlineUnderlay(
                _speedButtonLabel,
                outlineWidth: 0.22f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.32f,
                underlayDilate: 0.05f);
            var speedLabelRect = _speedButtonLabel.GetComponent<RectTransform>();
            speedLabelRect.anchorMin = Vector2.zero;
            speedLabelRect.anchorMax = Vector2.one;
            speedLabelRect.offsetMin = Vector2.zero;
            speedLabelRect.offsetMax = Vector2.zero;

            _speedButton.onClick.AddListener(CycleSpeed);
            UpdateSpeedButtonLabel();
            ApplyButtonPressScale(_speedButton, pressedScale: 0.96f);

            // Settings button (top-right)
            var settingsGO = new GameObject("SettingsButton");
            settingsGO.transform.SetParent(root.transform, false);
            var settingsImg = settingsGO.AddComponent<Image>();
            _settingsButton = settingsGO.AddComponent<Button>();
            ApplyUIKitButtonSprites(_settingsButton, settingsImg,
                normal: hasKit ? "ui.button.mint_square.normal" : null,
                pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                disabled: hasKit ? "ui.button.mint_square.disabled" : null);

            var settingsRect = settingsGO.GetComponent<RectTransform>();
            PlaceTopRight(settingsRect, uiLayout.settings, uiLayout.referenceWidth, topBarExtraRightUnits);

            var gearGO = new GameObject("Icon");
            gearGO.transform.SetParent(settingsGO.transform, false);
            var gearImg = gearGO.AddComponent<Image>();
            gearImg.raycastTarget = false;
            if (hasKit)
            {
                gearImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.gear");
                gearImg.color = Color.white;
            }
            var gearRect = gearGO.GetComponent<RectTransform>();
            gearRect.anchorMin = new Vector2(0.5f, 0.5f);
            gearRect.anchorMax = new Vector2(0.5f, 0.5f);
            gearRect.pivot = new Vector2(0.5f, 0.5f);
            float settingsSide = Mathf.Max(1f, Mathf.Min(uiLayout.settings.width, uiLayout.settings.height));
            float gearSide = Mathf.Clamp(settingsSide * 0.68f, 24f, 9999f);
            gearRect.anchoredPosition = new Vector2(0f, gearSide * 0.05f);
            gearRect.sizeDelta = new Vector2(gearSide, gearSide);

            _settingsButton.onClick.AddListener(() => SettingsUi.Toggle(true));
            ApplyButtonPressScale(_settingsButton, pressedScale: 0.96f);

            // Fast tag (top-center), toggled by full-belt fast-forward / boosters.
            _fastTag = new GameObject("FastTag");
            _fastTag.transform.SetParent(root.transform, false);
            var fastRootRect = _fastTag.AddComponent<RectTransform>();
            fastRootRect.anchorMin = Vector2.zero;
            fastRootRect.anchorMax = Vector2.one;
            fastRootRect.offsetMin = Vector2.zero;
            fastRootRect.offsetMax = Vector2.zero;

            var fastBgGO = new GameObject("BG");
            fastBgGO.transform.SetParent(_fastTag.transform, false);
            _fastTagBg = fastBgGO.AddComponent<Image>();
            _fastTagBg.raycastTarget = false;
            if (hasKit)
            {
                _fastTagBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info");
                _fastTagBg.type = _fastTagBg.sprite != null && _fastTagBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                _fastTagBg.color = Color.white;
            }
            var fastBgRect = _fastTagBg.GetComponent<RectTransform>();
            fastBgRect.anchorMin = new Vector2(0.5f, 1f);
            fastBgRect.anchorMax = new Vector2(0.5f, 1f);
            fastBgRect.pivot = new Vector2(0.5f, 1f);
            fastBgRect.anchoredPosition = new Vector2(0f, -(160f + topBarTopUnits));
            fastBgRect.sizeDelta = new Vector2(330f, 78f);

            var fastTextGO = new GameObject("Text");
            fastTextGO.transform.SetParent(_fastTag.transform, false);
            _fastTagText = fastTextGO.AddComponent<TextMeshProUGUI>();
            _fastTagText.raycastTarget = false;
            _fastTagText.text = "FAST";
            _fastTagText.alignment = TextAlignmentOptions.Center;
            _fastTagText.fontSize = 44;
            _fastTagText.color = Color.white;
            ApplyTmpOutlineUnderlay(
                _fastTagText,
                outlineWidth: 0.20f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.32f,
                underlayDilate: 0.05f);
            var fastTextRect = _fastTagText.GetComponent<RectTransform>();
            fastTextRect.anchorMin = new Vector2(0.5f, 1f);
            fastTextRect.anchorMax = new Vector2(0.5f, 1f);
            fastTextRect.pivot = new Vector2(0.5f, 1f);
            fastTextRect.anchoredPosition = new Vector2(0f, -(160f + topBarTopUnits));
            fastTextRect.sizeDelta = new Vector2(320f, 70f);

            _fastTag.SetActive(false);

            // Booster buttons (bottom-center)
            _boosterPanel = new GameObject("BoosterPanel");
            _boosterPanel.transform.SetParent(root.transform, false);
            var boosterRootRect = _boosterPanel.AddComponent<RectTransform>();
            boosterRootRect.anchorMin = Vector2.zero;
            boosterRootRect.anchorMax = Vector2.one;
            boosterRootRect.offsetMin = Vector2.zero;
            boosterRootRect.offsetMax = Vector2.zero;

            _boosterSortButton = CreateBoosterButton(
                _boosterPanel.transform,
                name: "BoosterSort",
                anchor: uiLayout.boosterAnchor,
                anchoredPos: new Vector2(-uiLayout.boosterOffset.x, uiLayout.boosterOffset.y + safeBottomUnits),
                size: uiLayout.boosterSize,
                normal: hasKit ? "ui.button.mint_square.normal" : null,
                 pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                 disabled: hasKit ? "ui.button.mint_square.disabled" : null,
                 icon: hasKit ? "ui.icon.sort" : null);
            ApplyButtonPressScale(_boosterSortButton, pressedScale: 0.96f);
            _boosterSortButton.onClick.AddListener(() => HandleBoosterButtonClick(BoosterType.Sort));
            if (hasKit) AttachBoosterBadge(_boosterSortButton.transform, _progress.BoosterSortCount);

            _boosterShuffleButton = CreateBoosterButton(
                _boosterPanel.transform,
                name: "BoosterShuffle",
                anchor: uiLayout.boosterAnchor,
                anchoredPos: new Vector2(uiLayout.boosterOffset.x, uiLayout.boosterOffset.y + safeBottomUnits),
                size: uiLayout.boosterSize,
                normal: hasKit ? "ui.button.purple_square.normal" : null,
                 pressed: hasKit ? "ui.button.purple_square.pressed" : null,
                 disabled: hasKit ? "ui.button.purple_square.disabled" : null,
                 icon: hasKit ? "ui.icon.shuffle" : null);
            ApplyButtonPressScale(_boosterShuffleButton, pressedScale: 0.96f);
            _boosterShuffleButton.onClick.AddListener(() => HandleBoosterButtonClick(BoosterType.Shuffle));
            if (hasKit) AttachBoosterBadge(_boosterShuffleButton.transform, _progress.BoosterShuffleCount);

            EnsureResultPanel();
            if (shopEnabled) EnsureShopUI();
            EnsureBoosterPurchaseUI();
        }

    }
}



