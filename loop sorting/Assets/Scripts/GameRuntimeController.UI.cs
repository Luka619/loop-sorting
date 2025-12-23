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
    /// <summary>
    /// Drives gameplay using LevelLayout; keeps visuals aligned with editor preview.
    /// </summary>
    public partial class GameRuntimeController
    {
        [Header("Settings")]
        public bool vibrationEnabled = true;
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        [Tooltip("Enable conveyor ambience SFX (loop + intermittent ticks).")]
        public bool conveyorAmbienceEnabled = false;
        public bool useSavedProgress = true;

        private static readonly Vector2 ModalPopupSize = new Vector2(980f, 1260f);
        private static readonly Vector2 ModalPopupAnchoredPos = new Vector2(0f, 20f);
        private static readonly Vector2 ModalCloseInset = new Vector2(-36f, -36f);

        private const string SettingsPanelPrefabResourcePath = "UI/SettingsPanel";
        private const string ShopPanelPrefabResourcePath = "UI/ShopPanel";
        private const string ResultPanelPrefabResourcePath = "UI/ResultPanel";
        private const string BoosterPurchasePanelPrefabResourcePath = "UI/BoosterPurchasePanel";
        private const string GameplayHudPrefabResourcePath = "UI/GameplayHUD";
        private const string MainMenuCanvasPrefabResourcePath = "UI/MainMenuCanvas";
        [Header("UI")]
        public BeltCounterUI beltCounterUI;
        [Tooltip("Enable Shop entry points (Shop button and + buttons in currency pills).")]
        public bool shopEnabled = false;
        [Tooltip("Show stamina (lives) pill in the HUD.")]
        public bool livesHudEnabled = false;
        [Tooltip("Use separate state sprites for buttons (pressed/disabled via SpriteSwap). Disable if state sprites have mismatched size/border and cause visual jitter/misalignment.")]
        public bool usePressedButtonSprites = false;
        [Header("UI Theme")]
        public UITheme uiTheme;
        private SettingsUiController SettingsUi => _settingsUi ??= new SettingsUiController(this);
        private SettingsUiController _settingsUi;
        private readonly UiModalService _uiModalService = new UiModalService();

        private float _hudTopInsetUnits;
        private float _hudRightInsetUnits;
        private float _hudBottomInsetUnits;

        public float HudTopInsetUnits => _hudTopInsetUnits;
        public float HudRightInsetUnits => _hudRightInsetUnits;
        public float HudBottomInsetUnits => _hudBottomInsetUnits;
        private Button _speedButton;
        private TMP_Text _speedButtonLabel;
        private Button _settingsButton;
        private GameObject _settingsPanel;
        private Toggle _musicToggle;
        private Toggle _vibrationToggle;
        private Toggle _soundToggle;
        private Image _settingsMusicToggleImage;
        private Button _settingsMusicToggleButton;
        private Image _settingsSfxToggleImage;
        private Button _settingsSfxToggleButton;
        private Image _settingsVibrationToggleImage;
        private Button _settingsVibrationToggleButton;
        private Button _settingsCloseButton;
        private Image _settingsCloseImage;
        private Button _settingsRetryButton;
        private Image _settingsRetryImage;
        private RectTransform _settingsPopupRect;
        private RectTransform _settingsTitleRect;
        private RectTransform _settingsCloseRect;
        private RectTransform _settingsMusicRowRect;
        private RectTransform _settingsSfxRowRect;
        private RectTransform _settingsVibrationRowRect;
        private RectTransform _settingsRetryRect;
        private bool _settingsBasePoseCaptured;
        private Vector2 _settingsTitleBasePos;
        private Vector3 _settingsTitleBaseScale = Vector3.one;
        private Vector2 _settingsCloseBasePos;
        private Vector3 _settingsCloseBaseScale = Vector3.one;
        private Vector2 _settingsMusicRowBasePos;
        private Vector3 _settingsMusicRowBaseScale = Vector3.one;
        private Vector2 _settingsSfxRowBasePos;
        private Vector3 _settingsSfxRowBaseScale = Vector3.one;
        private Vector2 _settingsVibrationRowBasePos;
        private Vector3 _settingsVibrationRowBaseScale = Vector3.one;
        private Vector2 _settingsRetryBasePos;
        private Vector3 _settingsRetryBaseScale = Vector3.one;
        private Coroutine _settingsIntroRoutine;
        private Button _shopButton;
        private TMP_Text _levelHudText;
        private TMP_Text _coinText;
        private TMP_Text _lifeText;
        private Button _coinPlusButton;
        private Button _lifePlusButton;
        private GameObject _shopPanel;
        private TMP_Text _shopTitle;
        private TMP_Text _shopCoinValue;
        private TMP_Text _shopLifeValue;
        private ScrollRect _shopScroll;
        private Image _shopScrollFadeTop;
        private Image _shopScrollFadeBottom;
        private RectTransform _shopContentRoot;
        private GameObject _boosterPanel;
        private Button _boosterSortButton;
        private Button _boosterShuffleButton;
        private GameObject _boosterPurchasePanel;
        private Button _boosterPurchaseCloseButton;
        private Image _boosterPurchaseCloseImage;
        private Button _boosterPurchaseCoinsButton;
        private Image _boosterPurchaseCoinsImage;
        private TMP_Text _boosterPurchaseCoinsLabel;
        private Image _boosterPurchaseCoinsPriceCover;
        private Button _boosterPurchaseAdButton;
        private Image _boosterPurchaseAdImage;
        private TMP_Text _boosterPurchaseAdLabel;
        private TMP_Text _boosterPurchaseTitleText;
        private TMP_Text _boosterPurchaseSubtitleText;
        private Image _boosterPurchaseBackground;
        private Image _boosterPurchaseHeader;
        private Image _boosterPurchaseIcon;
        private BoosterType _boosterPurchaseType;
        private RectTransform _boosterPurchasePopupRect;
        private RectTransform _boosterPurchaseHeaderRect;
        private RectTransform _boosterPurchaseIconRect;
        private RectTransform _boosterPurchaseCloseRect;
        private RectTransform _boosterPurchaseSubtitleRect;
        private RectTransform _boosterPurchaseCoinsRect;
        private RectTransform _boosterPurchaseAdRect;
        private bool _boosterPurchaseBasePoseCaptured;
        private Vector2 _boosterPurchaseHeaderBasePos;
        private Vector2 _boosterPurchaseIconBasePos;
        private Vector2 _boosterPurchaseSubtitleBasePos;
        private Vector2 _boosterPurchaseCoinsBasePos;
        private Vector2 _boosterPurchaseAdBasePos;
        private Vector2 _boosterPurchaseCloseBasePos;
        private Image _boosterPurchaseSubtitleBg;
        private Coroutine _boosterPurchaseIntroRoutine;
        private Coroutine _boosterPurchaseIdleRoutine;
        private Image _fastTagBg;
	        private TMP_Text _fastTagText;
        private bool IsGameplayInputLocked => _inputLocked || _uiModalService.IsLocked;
        private GameObject _eventSystem;
        private Canvas _uiCanvas;
        private CurrencyFlyFx _currencyFlyFx;
        private Canvas _mainMenuCanvas;
        private Button _mainMenuPlayButton;
        private RectTransform _hudRootRect;
        private RectTransform _lockChipLayer;
        private readonly Dictionary<int, RectTransform> _lockChipByBox = new Dictionary<int, RectTransform>();
        private GameObject _resultPanel;
        private TMP_Text _resultText;
        private Button _primaryButton;
        private Button _secondaryButton;
        private TMP_Text _primaryLabel;
        private TMP_Text _secondaryLabel;
        private Button _resultCloseButton;
        private Image _resultCloseImage;
        private enum ResultPanelMode { None, Win, Lose }
        private ResultPanelMode _resultPanelMode = ResultPanelMode.None;

        private bool _resultButtonsBaseLayoutCaptured;
        private Vector2 _resultPrimaryBaseAnchorMin;
        private Vector2 _resultPrimaryBaseAnchorMax;
        private Vector2 _resultPrimaryBaseAnchoredPosition;
        private Vector2 _resultPrimaryBaseSizeDelta;
        private Vector2 _resultSecondaryBaseAnchorMin;
        private Vector2 _resultSecondaryBaseAnchorMax;
        private Vector2 _resultSecondaryBaseAnchoredPosition;
        private Vector2 _resultSecondaryBaseSizeDelta;

        private RectTransform _resultWinRewardRootPrimary;
        private Image _resultWinRewardAdPrimary;
        private TMP_Text _resultWinRewardAmountPrimary;
        private Image _resultWinRewardCoinPrimary;
        private RectTransform _resultWinRewardRootSecondary;
        private Image _resultWinRewardAdSecondary;
        private TMP_Text _resultWinRewardAmountSecondary;
        private Image _resultWinRewardCoinSecondary;
        private Image _resultPrimaryIcon;
        private Image _resultSecondaryIcon;
        private const int BoosterPurchaseGrantCount = 1;
        private static readonly Dictionary<string, Sprite> BoosterPurchaseSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Sprite> SettingsPageSpriteCache = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
        private static BoosterPurchaseManifest _boosterPurchaseManifestCache;

        [Serializable]
        private sealed class BoosterPurchaseManifest
        {
            public int[] source_size;
            public BoosterPurchaseManifestAssets assets;
        }

        [Serializable]
        private sealed class BoosterPurchaseManifestAssets
        {
            public BoosterPurchaseManifestAsset btn_close;
            public BoosterPurchaseManifestAsset header_title_shuffle;
            public BoosterPurchaseManifestAsset title_shuffle_text;
            public BoosterPurchaseManifestAsset icon_booster_shuffle;
            public BoosterPurchaseManifestAsset btn_buy_coins_80;
            public BoosterPurchaseManifestAsset btn_watch_ad_free;
            public BoosterPurchaseManifestAsset popup_shuffle_full;
        }

        [Serializable]
        private sealed class BoosterPurchaseManifestAsset
        {
            public string file;
            public int[] box; // [x1,y1,x2,y2] in source image pixels, origin at top-left
            public int[] size; // [w,h]
        }

        private void ClearRuntime()
        {
            // Stop coroutines
            StopAllCoroutines();

            if (_runtimeLayoutInstance != null)
            {
                DestroyImmediate(_runtimeLayoutInstance);
                _runtimeLayoutInstance = null;
            }

            // Clear HUD runtime overlays that are level-specific but live under a persistent HUD canvas.
            if (_uiCanvas != null)
            {
                var hudRoot = _uiCanvas.transform.Find("HUDRoot");
                if (hudRoot != null)
                {
                    var existingLayer = hudRoot.Find("LockChipLayer");
                    if (existingLayer != null) DestroyImmediate(existingLayer.gameObject);
                }
            }

            // Destroy child objects under controller (conveyors, containers, slot markers, background, UI spawned under this transform)
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

            // Clear collections/state
            _beltSlots.Clear();
            _slotMarkers.Clear();
            _slotBasePositions.Clear();
            _slotCurrentPositions.Clear();
            foreach (var kv in _beltBlockVisuals)
            {
                if (kv.Value != null) DestroyImmediate(kv.Value);
            }
            _beltBlockVisuals.Clear();
            _boxViews.Clear();
            _containerToBelt.Clear();
            _boxSpecs.Clear();
            _boxLocked.Clear();
            _boxCompleted.Clear();
            _beltLoop = false;
            _game = null;
            _isReleasing = false;
            _activeReleasePort = null;
            _tickTimer = 0f;
	            _beltSpacingUsed = 0f;
		            _beltWidthUsed = 0f;
            _gameOver = false;
	            _inputLocked = false;
            _uiModalService.Reset();
	            _endSequenceRoutine = null;
	            _fullBeltFastForward = false;
	            _fullBeltStepsRemaining = 0;
            _beltSpawnAnimating.Clear();
            _beltSpawnCoroutines.Clear();
            _beltFrozenPositions.Clear();
            _beltWaitingIndices.Clear();
            _beltFrozenRemove.Clear();
            _conveyorBelt = null;
            _conveyorTickSfxCountdown = 0;

            _lockChipLayer = null;
            _lockChipByBox.Clear();

            ResetSfxSnapshot();
        }

        private void Awake()
        {
            EnsureStateMachine();
            EnsureAudioService();
            // Load persistent settings / economy / progress before building any UI.
            LoadSaveIfNeeded();
            EnsureEconomyDefaults();
            // Optional: allow swapping the entire UIKit Resources pack by changing one string (PlayerPrefs).
            // Example: PlayerPrefs.SetString("LoopSortingUIKit.ResourcesRoot", "loop_sorting_ui_components_v05_pack_b");
            LoopSortingUIKit.ApplyResourcesRootFromPlayerPrefs();
        }

        private void BuildInternal(LevelLayout layout, bool clearFlow)
        {
            if (clearFlow)
            {
                _flow = null;
                _flowIndex = 0;
            }
            if (layout == null)
            {
                Debug.LogError("GameRuntimeController.Build: layout is null");
                return;
            }
            var runtimeLayout = layout;
            if (autoResolveLayoutOverlap && minBoxToBeltGap > 0f)
            {
                runtimeLayout = LayoutUtils.CloneLayout(layout);
                LayoutUtils.ResolveBoxBeltOverlap(
                    runtimeLayout,
                    minBoxToBeltGap,
                    beltSlotSpacing,
                    overlapResolveIterations);
            }
            _runtimeLayoutInstance = runtimeLayout != layout ? runtimeLayout : null;
            // Reset root transform to avoid inherited offsets/scale from scene.
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // Full cleanup before building next level.
            ClearRuntime();

            _beltCapacity = layout.beltCapacity > 0 ? layout.beltCapacity : beltBlockLimit;
            EnsureEventSystem();
            EnsureSfx();
            EnsureMusic();
            PlaySfx(SfxId.LevelStart);
            _conveyorTickSfxCountdown = 6 + _rng.Next(2); // 6~7 ticks

            BuildConveyor(runtimeLayout);
            BuildContainers(runtimeLayout);
            _currentLayout = runtimeLayout;
            _levelBounds = LayoutUtils.ComputeLayoutBounds(runtimeLayout);
            FitCameraToLevel(runtimeLayout);
            EnsureBackground();
            EnsureCounterUI();
            RefreshLevelHudLabel();
            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(true);
            SettingsUi.EnsureBuilt();
            SyncContainersVisuals();
            SyncBeltVisuals();
            CaptureSfxSnapshot();
        }

        private void RefreshLevelHudLabel()
        {
            if (_levelHudText == null) return;
            int levelNumber = _flow != null ? (_flowIndex + 1) : 1;
            _levelHudText.text = $"LEVEL {levelNumber}";
        }

        public void Boot(LevelLayout layout)
        {
            LoadSaveIfNeeded();
            _pendingLevel = layout;
            _pendingFlow = null;
            _pendingFlowIndex = 0;
            EnsureStateMachine();
            _stateMachine.EnterMenu();
        }

        public void Boot(LevelFlow flow, int startIndex = 0)
        {
            LoadSaveIfNeeded();
            _pendingFlow = flow;
            int max = flow != null ? Mathf.Max(0, flow.levels.Count - 1) : 0;
            int savedIndex = useSavedProgress ? _progress.SavedFlowIndex : startIndex;
            _pendingFlowIndex = Mathf.Clamp(savedIndex, 0, max);
            _pendingLevel = null;
            EnsureStateMachine();
            _stateMachine.EnterMenu();
        }

        void IGameStateHost.EnterMenuState()
        {
            ShowMainMenu();
        }

        void IGameStateHost.EnterPlayingState()
        {
            StartPendingGame();
        }

        void IGameStateHost.ExitState(GameStateMachine.State from)
        {
        }

        private void ShowMainMenu()
        {
            EnsureEventSystem();
            EnsureSfx();
            EnsureMusic();

            // Build shared HUD canvas (hidden) so the Settings modal can be opened from the main menu.
	            EnsureCounterUI();
	            SettingsUi.EnsureBuilt();
	            EnsureMainMenuUI();
	            if (_mainMenuCanvas != null) _mainMenuCanvas.gameObject.SetActive(true);
	            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(false);
	            SettingsUi.HideImmediate();
	            HideUiPanelImmediate(_shopPanel);
	            HideUiPanelImmediate(_boosterPurchasePanel);
	            if (_resultPanel != null) _resultPanel.SetActive(false);
	        }

        private void StartPendingGame()
        {
            if (_mainMenuCanvas != null) _mainMenuCanvas.gameObject.SetActive(false);

            if (_pendingFlow != null && _pendingFlow.levels != null && _pendingFlow.levels.Count > 0)
            {
                _flow = _pendingFlow;
                _flowIndex = Mathf.Clamp(_pendingFlowIndex, 0, Mathf.Max(0, _flow.levels.Count - 1));
                BuildInternal(_flow.levels[_flowIndex], clearFlow: false);
                return;
            }

            if (_pendingLevel != null)
            {
                _flow = null;
                _flowIndex = 0;
                BuildInternal(_pendingLevel, clearFlow: true);
                return;
            }

            // Nothing to play; keep menu visible.
            EnsureStateMachine();
            _stateMachine.EnterMenu();
        }

        private void EnsureMainMenuUI()
        {
            if (_mainMenuCanvas != null && _mainMenuPlayButton != null)
            {
                return;
            }

            if (_mainMenuCanvas != null)
            {
                Destroy(_mainMenuCanvas.gameObject);
                _mainMenuCanvas = null;
                _mainMenuPlayButton = null;
            }

            // Prefer prefab-driven main menu so layout can be tweaked manually.
            var menuPrefab = Resources.Load<GameObject>(MainMenuCanvasPrefabResourcePath);
            if (menuPrefab != null)
            {
                var instance = Instantiate(menuPrefab);
                instance.name = menuPrefab.name;
                if (Application.isPlaying) DontDestroyOnLoad(instance);

                _mainMenuCanvas = instance.GetComponent<Canvas>();
                if (_mainMenuCanvas != null)
                {
                    _mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    _mainMenuCanvas.overrideSorting = true;
                    _mainMenuCanvas.sortingOrder = 10; // above gameplay HUD

                    var refs = instance.GetComponent<MainMenuCanvasPrefabRefs>();
                    if (refs != null) refs.AutoAssign();

                    _mainMenuPlayButton = refs != null ? refs.playButton : null;
                    if (_mainMenuPlayButton == null)
                    {
                        var playT = instance.transform.Find("SafeArea/PlayButton") ?? instance.transform.Find("PlayButton");
                        if (playT != null) _mainMenuPlayButton = playT.GetComponent<Button>();
                    }

                    if (_mainMenuPlayButton != null)
                    {
                        _mainMenuPlayButton.onClick.RemoveAllListeners();
                        _mainMenuPlayButton.onClick.AddListener(() =>
                        {
                            PlaySfx(SfxId.UiConfirm);
                            EnsureStateMachine();
                            _stateMachine.EnterPlaying();
                        });
                    }

                    var settingsButton = refs != null ? refs.settingsButton : null;
                    if (settingsButton == null)
                    {
                        var settingsT = instance.transform.Find("SafeArea/SettingsButton") ?? instance.transform.Find("SettingsButton");
                        if (settingsT != null) settingsButton = settingsT.GetComponent<Button>();
                    }

                    if (settingsButton != null)
                    {
                        settingsButton.onClick.RemoveAllListeners();
                        settingsButton.onClick.AddListener(() =>
                        {
                            SettingsUi.EnsureBuilt();
                            SettingsUi.Toggle(true);
                        });
                    }

                    RebindMainMenuCanvasPrefabSprites(refs, hasKit: LoopSortingUIKit.IsAvailable());
                    return;
                }

                Destroy(instance);
            }

            var canvasGO = new GameObject("MainMenuCanvas");
            _mainMenuCanvas = canvasGO.AddComponent<Canvas>();
            _mainMenuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _mainMenuCanvas.overrideSorting = true;
            _mainMenuCanvas.sortingOrder = 10; // above gameplay HUD

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            if (Application.isPlaying) DontDestroyOnLoad(canvasGO);

            var safeAreaGO = new GameObject("SafeArea");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            var safeAreaRect = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;

            var screenAdapter = canvasGO.AddComponent<LoopSortingScreenAdapter>();
            screenAdapter.canvasScaler = scaler;
            screenAdapter.referenceResolution = scaler.referenceResolution;
            screenAdapter.safeAreaRect = safeAreaRect;
            screenAdapter.Refresh();

            float safeRightUnits = 0f;
            if (screenAdapter != null)
            {
                float sf = ComputeCanvasScaleFactor(scaler);
                if (sf > 0.0001f) safeRightUnits = screenAdapter.RawSafeAreaInsetsPx.y / sf;
            }

            var bgGO = new GameObject("BG");
            bgGO.transform.SetParent(canvasGO.transform, false);
            var bg = bgGO.AddComponent<Image>();
            bg.raycastTarget = false;
            if (LoopSortingUIKit.IsAvailable())
            {
                var bgSprite = LoopSortingUIKit.LoadSpriteByKey("ui.bg_main");
                bg.sprite = bgSprite;
                bg.color = Color.white;
                bg.type = Image.Type.Simple;
                bg.preserveAspect = false;
            }
            else
            {
                bg.color = new Color(0.06f, 0.06f, 0.08f, 1f);
            }
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgGO.transform.SetAsFirstSibling();

            // Settings (top-right) - matches UI kit blueprint.
            if (LoopSortingUIKit.IsAvailable())
            {
                var settingsBtn = CreateIconButton(
                    parent: safeAreaGO.transform,
                    name: "SettingsButton",
                    anchor: new Vector2(1f, 1f),
                    anchoredPos: new Vector2(-80f - safeRightUnits, -80f),
                    size: new Vector2(180f, 180f),
                    normal: "ui.button.mint_square.normal",
                    pressed: "ui.button.mint_square.pressed",
                    disabled: "ui.button.mint_square.disabled",
                    icon: "ui.icon.gear");
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() =>
                {
                    SettingsUi.EnsureBuilt();
                    SettingsUi.Toggle(true);
                });
            }

            var playGO = new GameObject("PlayButton");
            playGO.transform.SetParent(safeAreaGO.transform, false);
            var playRect = playGO.AddComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.34f);
            playRect.anchorMax = new Vector2(0.5f, 0.34f);
            playRect.pivot = new Vector2(0.5f, 0.5f);
            playRect.anchoredPosition = Vector2.zero;
            playRect.sizeDelta = new Vector2(900f, 260f);

            var playImg = playGO.AddComponent<Image>();
            _mainMenuPlayButton = playGO.AddComponent<Button>();
            ApplyUIKitButtonSprites(_mainMenuPlayButton, playImg,
                normal: LoopSortingUIKit.IsAvailable() ? "ui.button.orange_long.normal" : null,
                pressed: LoopSortingUIKit.IsAvailable() ? "ui.button.orange_long.pressed" : null,
                disabled: LoopSortingUIKit.IsAvailable() ? "ui.button.orange_long.disabled" : null);

            var playTextGO = new GameObject("Text");
            playTextGO.transform.SetParent(playGO.transform, false);
            var playText = playTextGO.AddComponent<TextMeshProUGUI>();
            playText.raycastTarget = false;
            playText.text = "PLAY";
            playText.alignment = TextAlignmentOptions.Center;
            playText.fontSize = 84;
            playText.color = Color.white;
            ApplyTmpOutlineUnderlay(
                playText,
                outlineWidth: 0.22f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.35f,
                underlayDilate: 0.05f);
            var playTextRect = playText.GetComponent<RectTransform>();
            playTextRect.anchorMin = Vector2.zero;
            playTextRect.anchorMax = Vector2.one;
            playTextRect.offsetMin = Vector2.zero;
            playTextRect.offsetMax = Vector2.zero;

            _mainMenuPlayButton.onClick.RemoveAllListeners();
            _mainMenuPlayButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiConfirm);
                EnsureStateMachine();
                _stateMachine.EnterPlaying();
            });

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(safeAreaGO.transform, false);
            var titleRect = titleGO.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, -80f);
            titleRect.sizeDelta = new Vector2(700f, 260f);

            Sprite titleSprite = null;
            if (LoopSortingUIKit.IsAvailable())
            {
                titleSprite =
                    LoopSortingUIKit.LoadSpriteByKey("ui.title.main") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/title_fangkuai_zhuan_bu_ting.png", pixelsPerUnit: 100f, applyNineSlice: false);
            }

            if (titleSprite != null)
            {
                var titleImg = titleGO.AddComponent<Image>();
                titleImg.raycastTarget = false;
                titleImg.sprite = titleSprite;
                titleImg.color = Color.white;
                titleImg.type = Image.Type.Simple;
                titleImg.preserveAspect = true;
            }
            else
            {
                var title = titleGO.AddComponent<TextMeshProUGUI>();
                title.raycastTarget = false;
                title.text = "LOOP\nSORTING";
                title.alignment = TextAlignmentOptions.Center;
                title.fontSize = 96;
                title.color = Color.white;
                ApplyTmpOutlineUnderlay(
                    title,
                    outlineWidth: 0.18f,
                    outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                    underlayColor: new Color(0f, 0f, 0f, 0.35f),
                    underlayOffset: new Vector2(2f, -4f),
                    underlaySoftness: 0.38f,
                    underlayDilate: 0.06f);
            }

            // Level pill (optional but matches UI kit blueprint)
            if (LoopSortingUIKit.IsAvailable())
            {
                var levelPillGO = new GameObject("LevelPill");
                levelPillGO.transform.SetParent(safeAreaGO.transform, false);
                var pillRect = levelPillGO.AddComponent<RectTransform>();
                pillRect.anchorMin = new Vector2(0.5f, 0.55f);
                pillRect.anchorMax = new Vector2(0.5f, 0.55f);
                pillRect.pivot = new Vector2(0.5f, 0.5f);
                pillRect.anchoredPosition = Vector2.zero;
                pillRect.sizeDelta = new Vector2(380f, 90f);

                var pillBg = levelPillGO.AddComponent<Image>();
                pillBg.raycastTarget = false;
                pillBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info");
                pillBg.color = Color.white;
                pillBg.type = pillBg.sprite != null && pillBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                
                var pillTextGO = new GameObject("Text");
                pillTextGO.transform.SetParent(levelPillGO.transform, false);
                var pillText = pillTextGO.AddComponent<TextMeshProUGUI>();
                pillText.raycastTarget = false;
                int levelNumber = _pendingFlow != null ? (_pendingFlowIndex + 1) : 1;
                pillText.text = $"LEVEL {levelNumber}";
                pillText.alignment = TextAlignmentOptions.Center;
                pillText.fontSize = 44;
                pillText.color = new Color(0.12f, 0.12f, 0.12f, 1f);
                ApplyTmpOutlineUnderlay(
                    pillText,
                    outlineWidth: 0.12f,
                    outlineColor: new Color(1f, 1f, 1f, 0.55f),
                    underlayColor: new Color(0f, 0f, 0f, 0.12f),
                    underlayOffset: new Vector2(1f, -2f),
                    underlaySoftness: 0.28f,
                    underlayDilate: 0.02f);
                var pillTextRect = pillTextGO.GetComponent<RectTransform>();
                pillTextRect.anchorMin = Vector2.zero;
                pillTextRect.anchorMax = Vector2.one;
                pillTextRect.offsetMin = Vector2.zero;
                pillTextRect.offsetMax = Vector2.zero;
            }
        }

        private void CycleSpeed()
        {
            PlaySfx(SfxId.UiClick);
            if (speedSteps == null || speedSteps.Length == 0)
            {
                _speedMultiplier = 1f;
                UpdateSpeedButtonLabel();
                RefreshFastTag();
                return;
            }
            _speedIndex = (_speedIndex + 1) % speedSteps.Length;
            _speedMultiplier = Mathf.Max(0.0001f, speedSteps[_speedIndex]);
            UpdateSpeedButtonLabel();
            RefreshFastTag();
        }

        private void UpdateSpeedButtonLabel()
        {
            if (_speedButtonLabel == null) return;
            float val = _speedMultiplier;
            _speedButtonLabel.text = FormatSpeedLabel(val);
        }

        private static string FormatSpeedLabel(float speed)
        {
            // Avoid long decimals in the tiny square button; show tiered labels:
            // 1x, 2x, 3x correspond to 1.0, 1.5, 2.0.
            if (Mathf.Abs(speed - 1f) < 0.01f) return "1x";
            if (Mathf.Abs(speed - 1.5f) < 0.01f) return "2x";
            if (Mathf.Abs(speed - 2f) < 0.01f) return "3x";

            // Fallback for other steps (e.g., 4x/5x).
            if (speed >= 0f && speed <= 99f)
            {
                float rounded = Mathf.Round(speed * 10f) / 10f;
                if (Mathf.Abs(rounded - Mathf.Round(rounded)) < 0.001f)
                {
                    return $"{Mathf.RoundToInt(rounded)}x";
                }
                return $"{rounded:0.#}x";
            }

            return $"{speed:0.#}x";
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            _eventSystem = new GameObject("EventSystem");
            _eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            _eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(_eventSystem);
        }

        private void EnsureBackground()
        {
            // Rebuild each time to match camera framing and avoid offsets.
            if (_backgroundQuad != null)
            {
                if (Application.isPlaying) Destroy(_backgroundQuad);
                else DestroyImmediate(_backgroundQuad);
                _backgroundQuad = null;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                // Fallback for scenes where the gameplay camera isn't tagged MainCamera.
                var cams = Camera.allCameras;
                for (int i = 0; i < cams.Length; i++)
                {
                    if (cams[i] != null && cams[i].enabled)
                    {
                        cam = cams[i];
                        break;
                    }
                }
            }
            if (cam == null) return;

            _backgroundQuad = RuntimePrimitives.CreateQuad("BackgroundQuad");
            int layer = cam.gameObject.layer;
            if ((cam.cullingMask & (1 << layer)) == 0)
            {
                layer = FindFirstIncludedLayer(cam.cullingMask);
            }
            _backgroundQuad.layer = layer;
            _backgroundQuad.transform.SetParent(cam.transform, false);

            // Place inside the camera frustum near the far plane (avoid clipping when farClipPlane is small).
            float near = Mathf.Max(0.01f, cam.nearClipPlane);
            float far = Mathf.Max(near + 0.02f, cam.farClipPlane);
            float dist = near + (far - near) * 0.95f;
            _backgroundQuad.transform.localPosition = Vector3.forward * dist;
            _backgroundQuad.transform.localRotation = Quaternion.identity;

            // Match camera viewport size with padding.
            float viewHeight = cam.orthographic
                ? cam.orthographicSize * 2f
                : (2f * Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * dist);
            float aspect = Mathf.Max(0.01f, cam.aspect);
            float padding = 1.2f;
            _backgroundQuad.transform.localScale = new Vector3(viewHeight * aspect * padding, viewHeight * padding, 1f);

            // UI kit background if available; otherwise a simple gradient.
            Texture2D tex = null;
            if (uiTheme != null && uiTheme.backgroundTexture != null)
            {
                tex = uiTheme.backgroundTexture;
            }
            else if (LoopSortingUIKit.IsAvailable())
            {
                tex = LoopSortingUIKit.LoadTextureByKey("ui.bg_main");
            }

            if (tex == null)
            {
                tex = new Texture2D(1, 2);
                tex.wrapMode = TextureWrapMode.Clamp;
                Color top = uiTheme != null ? uiTheme.gradientTop : new Color(1f, 0.92f, 0.78f);
                Color bottom = uiTheme != null ? uiTheme.gradientBottom : new Color(1f, 0.87f, 0.65f);
                tex.SetPixels(new[] { bottom, top });
                tex.Apply();
            }

            Material mat = null;
            Shader shader = null;

            // Prefer a shipped runtime material so WebGL/WX builds don't lose shaders to stripping.
            // In the Editor we keep using built-in shaders for easier iteration/debugging.
            Material runtimeMat = GetRuntimeUnlitTextureMaterialTemplate();
            if (runtimeMat != null)
            {
                if (runtimeMat.shader != null && runtimeMat.shader.isSupported)
                {
                    mat = new Material(runtimeMat);
                    shader = mat.shader;
                }
                else if (!_backgroundDebugLogged)
                {
                    _backgroundDebugLogged = true;
                    Debug.LogWarning(
                        $"[Background] Runtime material shader unsupported: '{runtimeMat.shader?.name ?? "null"}' (falling back to Shader.Find).");
                }
            }

            if (mat == null)
            {
                shader =
                    Shader.Find("LoopSorting/UnlitTexture") ??
                    Shader.Find("Unlit/Texture") ??
                    Shader.Find("Unlit/Transparent") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Standard");
                if (shader != null)
                {
                    mat = new Material(shader);
                }
            }

            if (mat != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                mat.color = Color.white;
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Background;
                // Write depth so later-drawn far geometry can't overwrite the background; use normal depth test to avoid accidental overlay.
                if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 1);
                if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                // WebGL/WeChat can be sensitive to backface culling on MeshRenderer backgrounds.
                if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0); // 0=Off
                if (mat.HasProperty("_CullMode")) mat.SetInt("_CullMode", 0);
                var renderer = _backgroundQuad.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = mat;
                renderer.sortingLayerID = 0;
                renderer.sortingOrder = int.MinValue;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.allowOcclusionWhenDynamic = false;

                if (Debug.isDebugBuild && !_backgroundDebugLogged)
                {
                    _backgroundDebugLogged = true;
                    Debug.Log(
                        $"[Background] cam='{cam.name}' layer={layer} cullingMask=0x{cam.cullingMask:X8} " +
                        $"near={cam.nearClipPlane:0.###} far={cam.farClipPlane:0.###} " +
                        $"shader='{shader.name}', tex='{(tex != null ? tex.name : "null")}' {tex?.width}x{tex?.height}");
                }
            }
            else
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (!_backgroundMaterialFailedLogged)
                {
                    _backgroundMaterialFailedLogged = true;
                    Debug.LogError("[Background] Failed to create a material (all Shader.Find fallbacks returned null).");
                }
#endif
            }

            // Disable collider
            var col = _backgroundQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private void SyncLockChipsUI()
        {
            if (_uiCanvas == null || _hudRootRect == null) return;
            if (_boxViews == null || _boxViews.Count == 0) return;
            if (_boxLocked == null || _boxLocked.Count == 0) return;

            if (_lockChipLayer == null)
            {
                var layerGO = new GameObject("LockChipLayer");
                layerGO.transform.SetParent(_hudRootRect, false);
                _lockChipLayer = layerGO.AddComponent<RectTransform>();
                _lockChipLayer.anchorMin = Vector2.zero;
                _lockChipLayer.anchorMax = Vector2.one;
                _lockChipLayer.offsetMin = Vector2.zero;
                _lockChipLayer.offsetMax = Vector2.zero;
            }

            var toRemove = new List<int>();
            foreach (var kv in _lockChipByBox)
            {
                int idx = kv.Key;
                bool locked = idx >= 0 && idx < _boxLocked.Count && _boxLocked[idx];
                if (!locked)
                {
                    if (kv.Value != null) Destroy(kv.Value.gameObject);
                    toRemove.Add(idx);
                }
            }
            for (int i = 0; i < toRemove.Count; i++) _lockChipByBox.Remove(toRemove[i]);

            for (int i = 0; i < _boxViews.Count && i < _boxLocked.Count; i++)
            {
                if (!_boxLocked[i]) continue;
                if (_lockChipByBox.ContainsKey(i)) continue;

                var unlockColor = i < _boxSpecs.Count && _boxSpecs[i] != null ? _boxSpecs[i].unlockColor : BlockColor.Red;
                _lockChipByBox[i] = CreateLockChip(_lockChipLayer, unlockColor);
            }

            UpdateLockChipPositions();
        }

        private RectTransform CreateLockChip(RectTransform parent, BlockColor unlockColor)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject("LockChip");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240f, 90f);

            var bg = go.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.lock.chip_plate");
                bg.type = bg.sprite != null && bg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.45f);
            }

            var discGO = new GameObject("ColorDisc");
            discGO.transform.SetParent(go.transform, false);
            var disc = discGO.AddComponent<Image>();
            disc.raycastTarget = false;
            if (hasKit)
            {
                disc.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.lock.color_disc");
                var c = BlockVisual.ToUnityColor(unlockColor);
                disc.color = new Color(c.r, c.g, c.b, 1f);
            }
            else
            {
                disc.color = BlockVisual.ToUnityColor(unlockColor);
            }
            var discRect = discGO.GetComponent<RectTransform>();
            discRect.anchorMin = new Vector2(0f, 0.5f);
            discRect.anchorMax = new Vector2(0f, 0.5f);
            discRect.pivot = new Vector2(0f, 0.5f);
            discRect.anchoredPosition = new Vector2(18f, 0f);
            discRect.sizeDelta = new Vector2(64f, 64f);

            var iconGO = new GameObject("LockIcon");
            iconGO.transform.SetParent(go.transform, false);
            var icon = iconGO.AddComponent<Image>();
            icon.raycastTarget = false;
            if (hasKit)
            {
                icon.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.lock");
                icon.color = Color.white;
            }
            var iconRect = iconGO.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(58f, 0f);
            iconRect.sizeDelta = new Vector2(56f, 56f);

            return rect;
        }

        private void UpdateLockChipPositions()
        {
            if (_hudRootRect == null || _lockChipByBox.Count == 0) return;
            var cam = Camera.main;
            if (cam == null) return;

            float pad = 20f;
            float halfW = _hudRootRect.rect.width * 0.5f;
            float halfH = _hudRootRect.rect.height * 0.5f;

            foreach (var kv in _lockChipByBox)
            {
                int boxIndex = kv.Key;
                var chip = kv.Value;
                if (chip == null) continue;
                if (boxIndex < 0 || boxIndex >= _boxViews.Count) continue;
                var box = _boxViews[boxIndex];
                if (box == null) continue;

                float lift = 0.55f;
                if (boxIndex < _boxSpecs.Count && _boxSpecs[boxIndex] != null)
                {
                    lift = Mathf.Max(lift, _boxSpecs[boxIndex].size.y * 0.65f + 0.25f);
                }

                var world = box.transform.position + new Vector3(0f, lift, 0f);
                var screen = cam.WorldToScreenPoint(world);
                if (screen.z < 0.01f)
                {
                    chip.gameObject.SetActive(false);
                    continue;
                }
                chip.gameObject.SetActive(true);

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_hudRootRect, screen, null, out var local))
                {
                    float maxX = halfW - pad - chip.sizeDelta.x * 0.5f;
                    float minX = -halfW + pad + chip.sizeDelta.x * 0.5f;
                    float maxY = halfH - pad - chip.sizeDelta.y * 0.5f;
                    float minY = -halfH + pad + chip.sizeDelta.y * 0.5f;
                    chip.anchoredPosition = new Vector2(Mathf.Clamp(local.x, minX, maxX), Mathf.Clamp(local.y, minY, maxY));
                }
            }
        }

	        private void HandleBoosterButtonClick(BoosterType type)
	        {
	            if (_game == null || _gameOver || IsGameplayInputLocked)
	            {
	                return;
	            }

            if (GetBoosterCount(type) <= 0)
            {
                OpenBoosterPurchase(type);
                return;
            }

            if (type == BoosterType.Sort)
            {
                StartCoroutine(BoosterSortSequence());
            }
            else
            {
                StartCoroutine(BoosterShuffleSequence());
            }
        }

        private void RefreshBoosterBadges()
        {
            if (!LoopSortingUIKit.IsAvailable()) return;
            if (_boosterSortButton != null) SetBoosterBadgeCount(_boosterSortButton.transform, _progress.BoosterSortCount);
            if (_boosterShuffleButton != null) SetBoosterBadgeCount(_boosterShuffleButton.transform, _progress.BoosterShuffleCount);
        }

        private void SetBoosterBadgeCount(Transform buttonTransform, int count)
        {
            if (buttonTransform == null) return;
            if (!LoopSortingUIKit.IsAvailable()) return;

            var badge = buttonTransform.Find("Badge");
            if (badge == null)
            {
                AttachBoosterBadge(buttonTransform, count);
                return;
            }

            var badgeBg = badge.Find("BadgeBG")?.GetComponent<Image>();
            if (badgeBg != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(badgeBg, LoopSortingUIKit.LoadSpriteByKey("ui.badge.bg"), preserveAspect: true);
                badgeBg.raycastTarget = false;
            }

            count = Mathf.Clamp(count, 0, 99);
            var tmp = badge.Find("Text")?.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = badge.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
            if (tmp != null)
            {
                tmp.text = count.ToString();
                return;
            }

            // Back-compat: older badges used sprite digits.
            int tens = count / 10;
            int ones = count % 10;
            bool showTens = count >= 10;

            var tensImg = badge.Find("DigitTens")?.GetComponent<Image>();
            var onesImg = badge.Find("DigitOnes")?.GetComponent<Image>();
            if (tensImg != null)
            {
                tensImg.sprite = LoopSortingUIKit.LoadSpriteByKey($"ui.digit.{tens}");
                tensImg.gameObject.SetActive(showTens);
            }
            if (onesImg != null)
            {
                onesImg.sprite = LoopSortingUIKit.LoadSpriteByKey($"ui.digit.{ones}");
                var onesRect = onesImg.GetComponent<RectTransform>();
                if (onesRect != null)
                {
                    float digitW = onesRect.sizeDelta.x;
                    onesRect.anchoredPosition = showTens ? new Vector2(digitW * 0.35f, 0f) : Vector2.zero;
                }
            }
        }

        // (Removed) Booster beam line: keep only token transfers for booster readability.

        private void SetInteractableForBooster(bool val)
        {
            if (_boosterSortButton != null) _boosterSortButton.interactable = val;
            if (_boosterShuffleButton != null) _boosterShuffleButton.interactable = val;
            if (_settingsButton != null) _settingsButton.interactable = val;
            if (_speedButton != null) _speedButton.interactable = val;
        }

        private void EnsureEmptyDeferredLine()
        {
            if (_emptyDeferredLine != null) return;

            var go = new GameObject("EmptyDeferredHintLine");
            go.transform.SetParent(transform, false);
            _emptyDeferredLine = go.AddComponent<LineRenderer>();
            _emptyDeferredLine.useWorldSpace = true;
            _emptyDeferredLine.positionCount = 2;
            _emptyDeferredLine.startWidth = 0.03f;
            _emptyDeferredLine.endWidth = 0.01f;
            _emptyDeferredLine.numCapVertices = 0;
            _emptyDeferredLine.numCornerVertices = 0;
            _emptyDeferredLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _emptyDeferredLine.receiveShadows = false;

            var shader =
                Shader.Find("Unlit/Color") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");
            if (shader != null)
            {
                var mat = new Material(shader);
                mat.renderQueue = 2920;
                _emptyDeferredLine.sharedMaterial = mat;
            }
            _emptyDeferredLine.enabled = false;
        }

        private void RefreshFastTag()
        {
            if (_fastTag == null) return;
            float mult = EffectiveSpeedMultiplier;
            bool show = mult > 1.01f;
            bool danger = _fullBeltFastForward;
            _fastTag.SetActive(show);
            if (_fastTagText != null)
            {
                string label = FormatSpeedLabel(mult); // "2x", "3x", "5x"...
                string valuePart = label.EndsWith("x") ? label.Substring(0, label.Length - 1) : label;
                _fastTagText.text = $"FAST x{valuePart}";
            }
            if (_fastTagBg != null && LoopSortingUIKit.IsAvailable())
            {
                _fastTagBg.sprite = LoopSortingUIKit.LoadSpriteByKey(danger ? "ui.tag_fast.danger" : "ui.tag_fast.info");
                _fastTagBg.type = Image.Type.Sliced;
                _fastTagBg.color = Color.white;
            }
        }

        private static Material CreateConveyorBeltMaterial(float totalLength, float tileWorld, bool loop)
        {
            Texture2D tex = null;
            if (LoopSortingUIKit.IsAvailable())
            {
                tex = LoopSortingUIKit.LoadTextureByKey("world.conveyor_belt");
            }

            Material mat = null;

            // Prefer a shipped runtime material so WebGL/WX builds don't lose shaders to stripping.
            // In the Editor we keep using built-in shaders for easier iteration/debugging.
            Material runtimeMat = GetRuntimeUnlitTextureMaterialTemplate();
            if (runtimeMat != null)
            {
                if (runtimeMat.shader != null && runtimeMat.shader.isSupported)
                {
                    mat = new Material(runtimeMat);
                }
                else if (!_beltMaterialDebugLogged)
                {
                    _beltMaterialDebugLogged = true;
                    Debug.LogWarning(
                        $"[ConveyorBelt] Runtime material shader unsupported: '{runtimeMat.shader?.name ?? "null"}' (falling back to Shader.Find).");
                }
            }

            if (mat == null)
            {
                // Render early and don't write depth so blocks/markers always appear on top.
                // Important: prefer a shader that supports alpha; otherwise a missing texture can turn into an opaque white band.
                var shader =
                    Shader.Find("LoopSorting/UnlitTexture") ??
                    Shader.Find("Unlit/Transparent") ??
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Unlit/Texture") ??
                    Shader.Find("Standard");
                if (shader != null)
                {
                    mat = new Material(shader);
                }
            }

            if (mat == null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                if (!_beltMaterialFailedLogged)
                {
                    _beltMaterialFailedLogged = true;
                    Debug.LogError("[ConveyorBelt] Failed to create a material (all Shader.Find fallbacks returned null).");
                }
#endif
                return null;
            }

            mat.renderQueue = 1800;
            // Write depth so the belt can't be overwritten by later-drawn far geometry; keep normal depth testing.
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 1);
            if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
            if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0);

            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);

                // Tile U by world length (normalized UVs): 1 tile per belt slot spacing by default.
                float worldPerTile = Mathf.Max(0.1f, tileWorld);
                float rawTilingU = Mathf.Max(0.01f, totalLength / worldPerTile);
                // For loops, force an integer repeat count so the seam meets perfectly (u=0 and u=1 sample same texel).
                float tilingU = loop ? Mathf.Max(1f, Mathf.Round(rawTilingU)) : rawTilingU;

                // WebGL can be strict about NPOT repeat; fall back to clamp to avoid an invisible belt.
                if (Mathf.IsPowerOfTwo(tex.width) && Mathf.IsPowerOfTwo(tex.height))
                {
                    tex.wrapMode = TextureWrapMode.Repeat;
                    mat.mainTextureScale = new Vector2(tilingU, 1f);
                }
                else
                {
                    tex.wrapMode = TextureWrapMode.Clamp;
                    mat.mainTextureScale = Vector2.one;
                }

                if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
                mat.color = Color.white;
            }
            else
            {
                // Placeholder: subtle band until art is dropped in.
                mat.mainTexture = Texture2D.whiteTexture;
                var c = new Color(0.12f, 0.16f, 0.22f, 0.45f);
                if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
                mat.color = c;
            }

            return mat;
        }

        private void UpdateBeltCounter()
        {
            if (beltCounterUI == null || _game == null)
            {
                return;
            }

            int total = _game.Conveyor.Length;
            int occupied = _game.Conveyor.BlockCount;
            int empty = Mathf.Max(0, total - occupied);
            beltCounterUI.SetValue(empty, total);
        }

        private void FitCameraToLevel(LevelLayout layout)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("GameRuntimeController: no main camera found to frame level.");
                return;
            }

            var bounds = LayoutUtils.ComputeLayoutBounds(layout);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            float paddingToUse = cameraPadding;
            if (paddingToUse < 0f)
            {
                var size = bounds.size;
                paddingToUse = Mathf.Max(size.x, size.y) * 0.08f + 0.35f;
            }
            bounds.Expand(paddingToUse * 2f);

            cam.orthographic = true;
            float width = bounds.size.x;
            float height = bounds.size.y;

            var rotation = Quaternion.Euler(cameraTiltX, cameraYawY, 0f);
            float tiltCos = Mathf.Cos(cameraTiltX * Mathf.Deg2Rad);
            if (tiltCos < 0.25f) tiltCos = 0.25f; // avoid extreme distortion / division by near-zero

            // With X tilt and a mostly-flat (Z¡Ö0) level, the effective vertical span in camera space is scaled by cos(tiltX).
            float heightInCamera = height * tiltCos;
            float orthoSize = Mathf.Max(heightInCamera * 0.5f, width * 0.5f / Mathf.Max(0.0001f, cam.aspect));

            bool hasLayout = LoopSortingUIKit.TryGetRuntimeLayout(out var uiLayout);
            float top = Mathf.Clamp01(hasLayout ? uiLayout.reservedTop : cameraReservedTop);
            float bottom = Mathf.Clamp01(hasLayout ? uiLayout.reservedBottom : cameraReservedBottom);
            float available = Mathf.Clamp01(1f - top - bottom);
            if (available < 0.35f) available = 0.35f;

            // Expand ortho size so the level fits into the remaining viewport area.
            orthoSize = orthoSize / available;
            if (minBlockPixelSize > 0f)
            {
                float unit = layout != null && layout.blockSize > 0f ? layout.blockSize : blockVisualSize.x;
                float maxOrtho = unit * Screen.height / (2f * minBlockPixelSize);
                if (maxOrtho > 0.0001f)
                {
                    orthoSize = Mathf.Min(orthoSize, maxOrtho);
                }
            }
            if (cameraMaxOrthoSize > 0f)
            {
                orthoSize = Mathf.Min(orthoSize, cameraMaxOrthoSize);
            }
            cam.orthographicSize = orthoSize;

            // Shift camera so bounds center sits in the middle of the available region.
            float desiredCenterY01 = bottom + available * 0.5f;
            float delta01 = desiredCenterY01 - 0.5f;
            float worldOffsetYCamera = delta01 * (2f * orthoSize);
            float worldOffsetY = worldOffsetYCamera / tiltCos;

            var target = new Vector3(bounds.center.x, bounds.center.y + worldOffsetY, 0f);
            float distance = Mathf.Max(0.01f, Mathf.Abs(cameraZOffset));
            cam.transform.rotation = rotation;
            cam.transform.position = target - (rotation * Vector3.forward) * distance;
        }

        private IEnumerator ReleaseRoutine(int containerIndex, BlockColor targetColor)
        {
            _isReleasing = true;
            _activeReleasePort = _containerToBelt.TryGetValue(containerIndex, out var portIdx) ? portIdx : (int?)null;

            var container = _game.Containers[containerIndex];
            // This container cannot accept incoming blocks while releasing.
            container.SetBusy(true);
            PlaySfx(SfxId.RunShipStart);
            // Determine how many consecutive blocks of the same color are at the front.
            int pending = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if (container.Blocks[i].Color == targetColor)
                {
                    pending++;
                }
                else
                {
                    break;
                }
            }

            int safety = 0;
            int maxOps = Mathf.Max(128, pending * 4); // generous safety to avoid early stop when belt temporarily blocked

            while (pending > 0 && safety < maxOps)
            {
                // Check front color still matches.
                if (!container.TryPeek(out var peek) || peek.Color != targetColor)
                {
                    break;
                }

                var result = _game.TryReleaseFromContainer(containerIndex);
                if (result == ReleaseResult.BeltBlocked)
                {
                    PlaySfx(SfxId.BlockReject);
                    // Slot is occupied, wait and retry next frame/interval. Belt moves independently.
                    yield return new WaitForSeconds(releaseBlockedRetry / Mathf.Max(0.0001f, _speedMultiplier));
                    safety++;
                    continue;
                }
                if (result != ReleaseResult.Success)
                {
                    break;
                }

                pending--;
                PlaySfx(SfxId.BlockEject);

                SyncBeltVisuals();
                SyncContainersVisuals();
                UpdateBeltCounter();
                StartBeltSpawnFromBox(containerIndex, peek);
                if (containerIndex < _boxViews.Count)
                {
                    _boxViews[containerIndex].PlayBoxBounce();
                }

                // Keep the operable outline in sync with the remaining run.
                if (containerIndex < _boxViews.Count)
                {
                    _boxViews[containerIndex].ShowFrontOutline(pending, pending > 0);
                }

                yield return new WaitForSeconds(releaseInterval / Mathf.Max(0.0001f, _speedMultiplier));
                safety++;
            }
            PlaySfx(SfxId.RunShipEnd);
            _isReleasing = false;
            _activeReleasePort = null;
            ClearBeltWaitingState();
            container.SetBusy(false);
            if (containerIndex < _boxViews.Count)
            {
                _boxViews[containerIndex].HideFrontOutline();
            }
            UpdateLocks();
            CheckEndConditions();
        }
    }
}















