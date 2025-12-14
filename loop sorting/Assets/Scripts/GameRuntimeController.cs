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
    public class GameRuntimeController : MonoBehaviour
    {
        [Header("Config")]
        public float conveyorTickSeconds = 0.35f;
        public float beltSlotSpacing = 0.6f;
        [Tooltip("Camera padding around level bounds. Negative -> use preview-style auto padding.")]
        public float cameraPadding = -1f;
        public float cameraZOffset = -10f;
        [Tooltip("Camera tilt (degrees) around X. Small tilt reveals block sides for a more 3D look.")]
        public float cameraTiltX = -25f;
        [Tooltip("Camera yaw (degrees) around Y. Keep 0 for symmetrical layout.")]
        public float cameraYawY = 0f;
        [Tooltip("Reserve a fraction of vertical viewport for top UI when framing the level (0..0.45).")]
        public float cameraReservedTop = 0.08f;
        [Tooltip("Reserve a fraction of vertical viewport for bottom UI when framing the level (0..0.55).")]
        public float cameraReservedBottom = 0.12f;
        [Tooltip("Visual size of each block in box grid.")]
        public Vector2 blockVisualSize = new Vector2(0.45f, 0.45f);
        [Tooltip("Max blocks / slots on the conveyor (default 50). If layout sets beltCapacity > 0, it overrides this.")]
        public int beltBlockLimit = 50;
        [Tooltip("Scale factor for belt block size relative to slot spacing.")]
        public float beltBlockSizeFactor = 0.65f;
        [Header("Animation")]
        [Tooltip("Seconds between consecutive block releases from the same container.")]
        public float releaseInterval = 0.12f;
        [Tooltip("Seconds to wait before retrying if belt slot is blocked.")]
        public float releaseBlockedRetry = 0.1f;
        [Tooltip("Z offset for belt blocks so they render above markers (negative brings closer to camera).")]
        public float beltBlockZOffset = -0.05f;
        [Header("Settings")]
        public bool vibrationEnabled = true;
        public bool soundEnabled = true;
        [Header("BGM")]
        public BgmPlayer.Implementation bgmImplementation = BgmPlayer.Implementation.VerticalLayering;
        [Header("UI")]
        public BeltCounterUI beltCounterUI;
        [Header("Debug/Visuals")]
        public bool showSlotGizmos = true;
        [Tooltip("Log each box's resolved belt port mapping (slot index + world position). Useful when blocks don't enter the expected box.")]
        public bool debugLogBoxPorts = false;
        public Color slotColor = new Color(1f, 0.8f, 0.2f, 0.9f);
        public float slotGizmoRadius = 0.1f;
        [Tooltip("Slot markers visible in-game (visual only).")]
        public bool showSlotMarkersRuntime = true;
        public float slotMarkerScale = 0.15f;
        public Color slotMarkerColor = new Color(0.6f, 0.6f, 0.6f, 0.3f);
        [Header("Speed")]
        public float[] speedSteps = new float[] { 1f, 1.5f, 2f };
        [Header("UI Theme")]
        public UITheme uiTheme;

        private LoopSortingGame _game;
        private List<Transform> _beltSlots = new List<Transform>();
        private Dictionary<int, GameObject> _beltBlockVisuals = new Dictionary<int, GameObject>();
        private List<BoxView> _boxViews = new List<BoxView>();
        private Dictionary<int, int> _containerToBelt = new Dictionary<int, int>();
        private readonly Dictionary<Container, int> _containerIndexByRef = new Dictionary<Container, int>();
        private readonly List<ConveyorPortEvent> _portEvents = new List<ConveyorPortEvent>(32);
        private readonly Dictionary<GameObject, Coroutine> _uiPanelRoutines = new Dictionary<GameObject, Coroutine>();
        private readonly Dictionary<int, Vector3> _beltBlockOffsets = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Coroutine> _beltBlockOffsetCoroutines = new Dictionary<int, Coroutine>();
        private readonly HashSet<int> _beltInsertAnimating = new HashSet<int>();
        private readonly Dictionary<int, Coroutine> _beltInsertCoroutines = new Dictionary<int, Coroutine>();
        private Coroutine _emptyDeferredHintRoutine;
        private LineRenderer _emptyDeferredLine;
        private RejectFeedbackGate _rejectGate;
        private List<BoxSpec> _boxSpecs = new List<BoxSpec>();
        private List<bool> _boxLocked = new List<bool>();
        private List<bool> _boxCompleted = new List<bool>();
        private List<GameObject> _slotMarkers = new List<GameObject>();
        private List<Vector3> _slotBasePositions = new List<Vector3>();
        private List<Vector3> _slotCurrentPositions = new List<Vector3>();
        private bool _beltLoop;
        private float _tickTimer;
        private Bounds _levelBounds;
        private int _beltCapacity;
        private float _beltSpacingUsed;
        private bool _isReleasing;
        private int? _activeReleasePort;
        private float _speedMultiplier = 1f;
        private int _speedIndex = 0;
        private Button _speedButton;
        private TMP_Text _speedButtonLabel;
        private Button _settingsButton;
        private GameObject _settingsPanel;
        private Toggle _vibrationToggle;
        private Toggle _soundToggle;
        private int _coins = 810;
        private int _lives = 5;
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
        private Button _boosterFillButton;
        private Button _boosterShuffleButton;
        private GameObject _fastTag;
        private Image _fastTagBg;
        private TMP_Text _fastTagText;
        private System.Random _rng = new System.Random();
        private bool _inputLocked = false;
        private GameObject _backgroundQuad;
        private GameObject _conveyorBelt;
        private GameObject _eventSystem;
        private Canvas _uiCanvas;
        private Canvas _mainMenuCanvas;
        private Button _mainMenuPlayButton;
        private RectTransform _hudRootRect;
        private RectTransform _lockChipLayer;
        private readonly Dictionary<int, RectTransform> _lockChipByBox = new Dictionary<int, RectTransform>();
        private LevelFlow _pendingFlow;
        private int _pendingFlowIndex;
        private LevelLayout _pendingLevel;
        private LevelFlow _flow;
        private int _flowIndex;
        private LevelLayout _currentLayout;
        private GameObject _resultPanel;
        private TMP_Text _resultText;
        private Button _primaryButton;
        private Button _secondaryButton;
        private TMP_Text _primaryLabel;
        private TMP_Text _secondaryLabel;
        private bool _gameOver;
        private Coroutine _endSequenceRoutine;
        private const float WinEndSequenceDelaySeconds = 0.75f;
        private const float LoseEndSequenceDelaySeconds = 0.60f;
        private bool _fullBeltFastForward;
        private int _fullBeltStepsRemaining;
        private Image _resultPrimaryIcon;
        private Image _resultSecondaryIcon;
        private const int InitialBoosterCount = 99;
        private const int InitialCoins = 810;
        private const int InitialLives = 5;

        private SfxPlayer _sfx;
        private HapticsPlayer _haptics;
        private BgmPlayer _bgm;
        private bool _bgmPressure;
        private bool _sfxHasSnapshot;
        private bool _sfxPrevFastForward;
        private bool _sfxSuppressSpeeddownOnce;
        private readonly List<int> _sfxPrevContainerCounts = new List<int>();
        private readonly List<bool> _sfxPrevLockedStates = new List<bool>();
        private readonly List<bool> _sfxPrevCompletedStates = new List<bool>();
        private int _conveyorTickSfxCountdown;

        public float EffectiveSpeedMultiplier => _fullBeltFastForward ? 5f : _speedMultiplier;

        private readonly HashSet<int> _beltSpawnAnimating = new HashSet<int>();
        private readonly Dictionary<int, Coroutine> _beltSpawnCoroutines = new Dictionary<int, Coroutine>();

        private void ClearRuntime()
        {
            // Stop coroutines
            StopAllCoroutines();

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
            _gameOver = false;
            _inputLocked = false;
            _endSequenceRoutine = null;
            _fullBeltFastForward = false;
            _fullBeltStepsRemaining = 0;
            _beltSpawnAnimating.Clear();
            _beltSpawnCoroutines.Clear();
            _conveyorBelt = null;
            _conveyorTickSfxCountdown = 0;

            _coins = InitialCoins;
            _lives = InitialLives;

            _lockChipLayer = null;
            _lockChipByBox.Clear();

            ResetSfxSnapshot();
        }

        private void ResetSfxSnapshot()
        {
            _sfxHasSnapshot = false;
            _sfxPrevFastForward = false;
            _sfxSuppressSpeeddownOnce = false;
            _sfxPrevContainerCounts.Clear();
            _sfxPrevLockedStates.Clear();
            _sfxPrevCompletedStates.Clear();
        }

        [ContextMenu("Debug/Log Box Ports Now")]
        private void DebugLogBoxPortsNow()
        {
            if (!debugLogBoxPorts)
            {
                Debug.Log("DebugLogBoxPortsNow: enable 'debugLogBoxPorts' first.");
                return;
            }

            if (_currentLayout == null)
            {
                Debug.Log("DebugLogBoxPortsNow: no current layout.");
                return;
            }

            if (_beltSlots == null || _beltSlots.Count == 0)
            {
                Debug.Log("DebugLogBoxPortsNow: belt slots not built yet.");
                return;
            }

            if (_boxSpecs == null || _boxSpecs.Count == 0)
            {
                Debug.Log("DebugLogBoxPortsNow: boxes not built yet.");
                return;
            }

            for (int i = 0; i < _boxSpecs.Count; i++)
            {
                var spec = _boxSpecs[i];
                if (!_containerToBelt.TryGetValue(i, out var slotIndex)) continue;
                if (slotIndex < 0 || slotIndex >= _beltSlots.Count) continue;
                var mouth = LayoutUtils.ComputeMouth(spec, spec.size);
                var slotPos = _beltSlots[slotIndex] != null ? _beltSlots[slotIndex].position : Vector3.zero;
                Debug.Log($"BoxPort[{i}] '{(string.IsNullOrEmpty(spec.name) ? $"Box_{i}" : spec.name)}' opening={spec.opening} mouth=({mouth.x:F2},{mouth.y:F2}) -> slot={slotIndex} pos=({slotPos.x:F2},{slotPos.y:F2})");
            }
        }

        public void Build(LevelLayout layout)
        {
            BuildInternal(layout, clearFlow: true);
        }

        private void BuildInternal(LevelLayout layout, bool clearFlow)
        {
            if (clearFlow)
            {
                _flow = null;
                _flowIndex = 0;
            }
            _currentLayout = layout;
            if (layout == null)
            {
                Debug.LogError("GameRuntimeController.Build: layout is null");
                return;
            }

            // Reset root transform to avoid inherited offsets/scale from scene.
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            // Full cleanup before building next level.
            ClearRuntime();

            _beltCapacity = layout.beltCapacity > 0 ? layout.beltCapacity : beltBlockLimit;
            EnsureEventSystem();
            EnsureSfx();
            EnsureBgm();
            if (soundEnabled && _bgm != null)
            {
                _bgmPressure = false;
                _bgm.PlayLoop(BgmLoopId.GameplayBase, fadeSeconds: 0.6f);
            }
            PlaySfx(SfxId.LevelStart);
            _conveyorTickSfxCountdown = 6 + _rng.Next(2); // 6~7 ticks

            BuildConveyor(layout);
            BuildContainers(layout);
            _currentLayout = layout;
            _levelBounds = LayoutUtils.ComputeLayoutBounds(layout);
            FitCameraToLevel(layout);
            EnsureBackground();
            EnsureCounterUI();
            RefreshLevelHudLabel();
            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(true);
            EnsureSettingsUI();
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

        public void Build(LevelFlow flow, int startIndex = 0)
        {
            _flow = flow;
            _flowIndex = Mathf.Clamp(startIndex, 0, flow != null ? Mathf.Max(0, flow.levels.Count - 1) : 0);
            var layout = flow != null && flow.levels.Count > 0 ? flow.levels[_flowIndex] : null;
            BuildInternal(layout, clearFlow: false);
        }

        public void Boot(LevelLayout layout)
        {
            _pendingLevel = layout;
            _pendingFlow = null;
            _pendingFlowIndex = 0;
            ShowMainMenu();
        }

        public void Boot(LevelFlow flow, int startIndex = 0)
        {
            _pendingFlow = flow;
            _pendingFlowIndex = Mathf.Clamp(startIndex, 0, flow != null ? Mathf.Max(0, flow.levels.Count - 1) : 0);
            _pendingLevel = null;
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            EnsureEventSystem();
            EnsureSfx();
            EnsureBgm();
            if (soundEnabled && _bgm != null)
            {
                _bgmPressure = false;
                _bgm.PlayLoop(BgmLoopId.Menu, fadeSeconds: 0.6f);
            }

            // Build shared HUD canvas (hidden) so the Settings modal can be opened from the main menu.
            EnsureCounterUI();
            EnsureSettingsUI();
            EnsureMainMenuUI();
            if (_mainMenuCanvas != null) _mainMenuCanvas.gameObject.SetActive(true);
            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
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
            ShowMainMenu();
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
            DontDestroyOnLoad(canvasGO);

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

            static void ApplyTmpOutlineUnderlay(
                TMP_Text tmp,
                float outlineWidth,
                Color outlineColor,
                Color underlayColor,
                Vector2 underlayOffset,
                float underlaySoftness,
                float underlayDilate)
            {
                if (tmp == null) return;
                if (tmp.fontMaterial == null) return;

                // Clone material so we don't mutate shared TMP materials globally.
                var mat = new Material(tmp.fontMaterial);
                tmp.fontMaterial = mat;

                if (mat.HasProperty(ShaderUtilities.ID_OutlineWidth))
                {
                    mat.EnableKeyword("OUTLINE_ON");
                    mat.SetFloat(ShaderUtilities.ID_OutlineWidth, Mathf.Clamp01(outlineWidth));
                }
                if (mat.HasProperty(ShaderUtilities.ID_OutlineColor))
                {
                    mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
                }

                if (mat.HasProperty(ShaderUtilities.ID_UnderlayColor))
                {
                    mat.EnableKeyword("UNDERLAY_ON");
                    mat.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
                }
                if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetX)) mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffset.x);
                if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetY)) mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffset.y);
                if (mat.HasProperty(ShaderUtilities.ID_UnderlaySoftness)) mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, Mathf.Clamp01(underlaySoftness));
                if (mat.HasProperty(ShaderUtilities.ID_UnderlayDilate)) mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, Mathf.Clamp(underlayDilate, -1f, 1f));

                tmp.UpdateMeshPadding();
            }

            // Settings (top-right) - matches UI kit blueprint.
            if (LoopSortingUIKit.IsAvailable())
            {
                var settingsBtn = CreateIconButton(
                    parent: canvasGO.transform,
                    name: "SettingsButton",
                    anchor: new Vector2(1f, 1f),
                    anchoredPos: new Vector2(-80f, -80f),
                    size: new Vector2(180f, 180f),
                    normal: "ui.button.mint_square.normal",
                    pressed: "ui.button.mint_square.pressed",
                    disabled: "ui.button.mint_square.disabled",
                    icon: "ui.icon.gear");
                settingsBtn.onClick.RemoveAllListeners();
                settingsBtn.onClick.AddListener(() =>
                {
                    EnsureSettingsUI();
                    ToggleSettingsPanel(true);
                });
            }

            var playGO = new GameObject("PlayButton");
            playGO.transform.SetParent(canvasGO.transform, false);
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
                StartPendingGame();
            });

            // Title
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(canvasGO.transform, false);
            var title = titleGO.AddComponent<TextMeshProUGUI>();
            title.raycastTarget = false;
            title.text = "LOOP\nSORTING";
            title.alignment = TextAlignmentOptions.Center;
            title.fontSize = 96;
            title.color = Color.white;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, -80f);
            titleRect.sizeDelta = new Vector2(700f, 260f);
            ApplyTmpOutlineUnderlay(
                title,
                outlineWidth: 0.18f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -4f),
                underlaySoftness: 0.38f,
                underlayDilate: 0.06f);

            // Level pill (optional but matches UI kit blueprint)
            if (LoopSortingUIKit.IsAvailable())
            {
                var levelPillGO = new GameObject("LevelPill");
                levelPillGO.transform.SetParent(canvasGO.transform, false);
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

        private void EnsureSfx()
        {
            if (_sfx == null)
            {
                _sfx = GetComponent<SfxPlayer>();
                if (_sfx == null)
                {
                    _sfx = gameObject.AddComponent<SfxPlayer>();
                }
            }

            _sfx.SetEnabled(soundEnabled);
        }

        private void EnsureBgm()
        {
            if (_bgm == null)
            {
                _bgm = GetComponent<BgmPlayer>();
                if (_bgm == null)
                {
                    _bgm = gameObject.AddComponent<BgmPlayer>();
                }
            }

            _bgm.implementation = bgmImplementation;
            _bgm.SetEnabled(soundEnabled);
        }

        private void UpdateBgmPressureAfterTick()
        {
            if (!soundEnabled) return;
            if (_bgm == null) return;
            if (_game == null) return;
            if (_gameOver) return;

            int total = Mathf.Max(1, _game.Conveyor.Length);
            int occupied = Mathf.Clamp(_game.Conveyor.BlockCount, 0, total);
            float fillRatio = (float)occupied / total;

            const float EnterThreshold = 0.75f;
            const float ExitThreshold = 0.60f;

            bool shouldPressure = _fullBeltFastForward || fillRatio >= EnterThreshold || (_bgmPressure && fillRatio > ExitThreshold);
            if (shouldPressure == _bgmPressure) return;

            _bgmPressure = shouldPressure;
            _bgm.PlayLoop(_bgmPressure ? BgmLoopId.GameplayPressure : BgmLoopId.GameplayBase, fadeSeconds: 0.9f);
        }

        private void EnsureHaptics()
        {
            if (_haptics == null)
            {
                _haptics = GetComponent<HapticsPlayer>();
                if (_haptics == null)
                {
                    _haptics = gameObject.AddComponent<HapticsPlayer>();
                }
            }

            _haptics.SetEnabled(vibrationEnabled);
        }

        private void UpdateConveyorLoopSfx()
        {
            if (_sfx == null) return;
            if (!soundEnabled) { _sfx.StopLoop(); return; }

            // Only run loop SFX during active gameplay.
            if (_game == null || _gameOver)
            {
                _sfx.StopLoop();
                return;
            }

            float pitch = _fullBeltFastForward ? 1.15f : 1f;
            if (!_fullBeltFastForward && _speedMultiplier >= 4.99f) pitch = 1.12f;

            _sfx.StartLoop(SfxId.ConveyorLoop, volumeMultiplier: 1f, pitch: pitch);
        }

        private void PlaySfx(SfxId id, float volumeMultiplier = 1f)
        {
            if (vibrationEnabled)
            {
                EnsureHaptics();
                _haptics.PlayFromSfx(id);
            }

            if (!soundEnabled)
            {
                return;
            }

            EnsureSfx();
            _sfx.Play(id, volumeMultiplier);
        }

        private void CaptureSfxSnapshot()
        {
            if (_game == null)
            {
                ResetSfxSnapshot();
                return;
            }

            _sfxPrevFastForward = _fullBeltFastForward;

            int n = _game.Containers.Count;
            while (_sfxPrevContainerCounts.Count < n) _sfxPrevContainerCounts.Add(0);
            while (_sfxPrevLockedStates.Count < n) _sfxPrevLockedStates.Add(false);
            while (_sfxPrevCompletedStates.Count < n) _sfxPrevCompletedStates.Add(false);

            for (int i = 0; i < n; i++)
            {
                _sfxPrevContainerCounts[i] = _game.Containers[i].Count;
                _sfxPrevLockedStates[i] = i < _boxLocked.Count && _boxLocked[i];
                _sfxPrevCompletedStates[i] = i < _boxCompleted.Count && _boxCompleted[i];
            }

            _sfxHasSnapshot = true;
        }

        private void EmitSfxFromStateChanges()
        {
            if (_game == null)
            {
                return;
            }

            if (!_sfxHasSnapshot)
            {
                CaptureSfxSnapshot();
                return;
            }

            if (soundEnabled)
            {
                EnsureBgm();
            }

            int inserts = 0;
            int completions = 0;
            int unlocks = 0;

            int n = _game.Containers.Count;
            for (int i = 0; i < n; i++)
            {
                int countNow = _game.Containers[i].Count;
                int countPrev = i < _sfxPrevContainerCounts.Count ? _sfxPrevContainerCounts[i] : 0;
                if (countNow > countPrev)
                {
                    inserts += (countNow - countPrev);
                }

                bool lockedNow = i < _boxLocked.Count && _boxLocked[i];
                bool lockedPrev = i < _sfxPrevLockedStates.Count && _sfxPrevLockedStates[i];
                if (lockedPrev && !lockedNow)
                {
                    unlocks++;
                }

                bool completedNow = i < _boxCompleted.Count && _boxCompleted[i];
                bool completedPrev = i < _sfxPrevCompletedStates.Count && _sfxPrevCompletedStates[i];
                if (!completedPrev && completedNow)
                {
                    completions++;
                }
            }

            if (inserts > 0)
            {
                float vol = 1f + Mathf.Min(0.6f, inserts * 0.08f);
                PlaySfx(SfxId.BlockInsert, vol);
            }
            if (completions > 0)
            {
                PlaySfx(SfxId.BoxComplete);
                if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.BoxComplete);
            }
            if (unlocks > 0)
            {
                PlaySfx(SfxId.BoxUnlock);
                if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.Unlock);
            }
            if (!_sfxPrevFastForward && _fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                PlaySfx(SfxId.ConveyorFullWarning);
                if (soundEnabled && _bgm != null)
                {
                    _bgmPressure = true;
                    _bgm.PlayLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.6f);
                    _bgm.PlayStinger(BgmStingerId.FullWarning);
                }
            }
            if (_sfxPrevFastForward && !_fullBeltFastForward)
            {
                if (_sfxSuppressSpeeddownOnce)
                {
                    _sfxSuppressSpeeddownOnce = false;
                }
                else
                {
                    PlaySfx(SfxId.ConveyorSpeeddown);
                    if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.Speeddown);
                }
            }

            CaptureSfxSnapshot();
        }

        private void EnsureBackground()
        {
            // Rebuild each time to match camera framing and avoid offsets.
            if (_backgroundQuad != null)
            {
                DestroyImmediate(_backgroundQuad);
                _backgroundQuad = null;
            }

            var cam = Camera.main;
            if (cam == null) return;

            _backgroundQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backgroundQuad.name = "BackgroundQuad";
            _backgroundQuad.layer = cam.gameObject.layer;
            _backgroundQuad.transform.SetParent(cam.transform, false);

            // Anchor to camera far side so it always sits behind gameplay.
            float dist = Mathf.Max(5f, cam.farClipPlane * 0.5f);
            _backgroundQuad.transform.localPosition = Vector3.forward * dist;
            _backgroundQuad.transform.localRotation = Quaternion.identity;

            // Match camera viewport size with padding.
            float viewHeight = cam.orthographic ? cam.orthographicSize * 2f : 30f;
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
                Color top = new Color(1f, 0.92f, 0.78f);
                Color bottom = new Color(1f, 0.87f, 0.65f);
                tex.SetPixels(new[] { bottom, top });
                tex.Apply();
            }

            var shader =
                Shader.Find("Unlit/Texture") ??
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Standard");

            if (shader != null)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Background;
                if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                var renderer = _backgroundQuad.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = mat;
            }

            // Disable collider
            var col = _backgroundQuad.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private int? TryGetBlockedPort()
        {
            if (!_isReleasing || _game == null || !_activeReleasePort.HasValue)
            {
                return null;
            }

            int portIdx = _activeReleasePort.Value;
            var slot = _game.Conveyor.GetSlot(portIdx);
            if (slot.HasValue)
            {
                return portIdx;
            }

            return null;
        }

        private void ToggleSettingsPanel(bool show)
        {
            if (_settingsPanel == null) return;
            PlaySfx(show ? SfxId.UiPopupOpen : SfxId.UiPopupClose);
            AnimateUiPanel(_settingsPanel, show);
            if (show)
            {
                if (_vibrationToggle != null) _vibrationToggle.isOn = vibrationEnabled;
                if (_soundToggle != null) _soundToggle.isOn = soundEnabled;
            }
        }

        public void OnHiddenReveal(int containerIndex, BlockColor revealColor)
        {
            if (_gameOver) return;
            PlaySfx(SfxId.HiddenReveal);
            if (containerIndex >= 0 && containerIndex < _boxViews.Count)
            {
                _boxViews[containerIndex].PlayTapFeedback();
            }
        }

        private void AnimateUiPanel(GameObject panel, bool show, float seconds = 0.18f)
        {
            if (panel == null) return;

            if (_uiPanelRoutines.TryGetValue(panel, out var existing) && existing != null)
            {
                StopCoroutine(existing);
            }
            _uiPanelRoutines.Remove(panel);

            var cg = MotionUtil.EnsureCanvasGroup(panel);
            if (cg == null)
            {
                panel.SetActive(show);
                return;
            }

            if (show)
            {
                panel.SetActive(true);
                panel.transform.localScale = Vector3.one * 0.92f;
                cg.alpha = 0f;
                cg.blocksRaycasts = true;
                cg.interactable = true;
                _uiPanelRoutines[panel] = StartCoroutine(AnimateUiPanelIn(panel, cg, seconds));
            }
            else
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
                _uiPanelRoutines[panel] = StartCoroutine(AnimateUiPanelOut(panel, cg, seconds));
            }
        }

        private IEnumerator AnimateUiPanelIn(GameObject panel, CanvasGroup cg, float seconds)
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
            _uiPanelRoutines.Remove(panel);
        }

        private IEnumerator AnimateUiPanelOut(GameObject panel, CanvasGroup cg, float seconds)
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
            panel.SetActive(false);
            _uiPanelRoutines.Remove(panel);
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

        private IEnumerator NormalizeBeltStateAnimated()
        {
            if (_game == null) yield break;
            if (_game.Conveyor.BlockCount == 0) yield break;
            for (int i = 0; i < _game.Conveyor.Length; i++)
            {
                _game.Conveyor.Advance(null);
                SyncBeltVisuals();
                SyncContainersVisuals();
                UpdateLocks();
                UpdateCompletionStates();
                UpdateBeltCounter();
                EmitSfxFromStateChanges();
                if (_game.Conveyor.BlockCount == 0) break;
                yield return new WaitForSeconds(conveyorTickSeconds / Mathf.Max(0.0001f, _speedMultiplier));
            }
        }

        private IEnumerator BoosterFillSequence()
        {
            if (_game == null || _inputLocked) yield break;
            _inputLocked = true;
            SetInteractableForBooster(false);
            PlaySfx(SfxId.BoosterActivate);
            EnsureBgm();
            if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.BoosterActivate);

            float prevSpeed = _speedMultiplier;
            if (!_fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                if (soundEnabled && _bgm != null)
                {
                    _bgmPressure = true;
                    _bgm.PlayLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.4f);
                    _bgm.PlayStinger(BgmStingerId.Speedup);
                }
            }
            _speedMultiplier = 5f;
            RefreshFastTag();

            if (_game.Conveyor.BlockCount > 0)
            {
                yield return StartCoroutine(NormalizeBeltStateAnimated());
            }

            var before = CaptureContainerStates();
            bool ok = ApplyBoosterFillColor();
            var after = CaptureContainerStates();
            if (ok) StartCoroutine(PlayBoosterFillFx(before, after));
            PlaySfx(ok ? SfxId.BoosterFillSort : SfxId.BoosterFail);
            EmitSfxFromStateChanges();

            _speedMultiplier = prevSpeed;
            if (!_fullBeltFastForward && prevSpeed < 4.99f)
            {
                PlaySfx(SfxId.ConveyorSpeeddown);
                if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.Speeddown);
            }
            UpdateBgmPressureAfterTick();
            RefreshFastTag();
            SetInteractableForBooster(true);
            if (!_gameOver) _inputLocked = false;
        }

        private IEnumerator BoosterShuffleSequence()
        {
            if (_game == null || _inputLocked) yield break;
            _inputLocked = true;
            SetInteractableForBooster(false);
            PlaySfx(SfxId.BoosterActivate);
            EnsureBgm();
            if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.BoosterActivate);

            float prevSpeed = _speedMultiplier;
            if (!_fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                if (soundEnabled && _bgm != null)
                {
                    _bgmPressure = true;
                    _bgm.PlayLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.4f);
                    _bgm.PlayStinger(BgmStingerId.Speedup);
                }
            }
            _speedMultiplier = 5f;
            RefreshFastTag();

            if (_game.Conveyor.BlockCount > 0)
            {
                yield return StartCoroutine(NormalizeBeltStateAnimated());
            }
            var before = CaptureContainerStates();
            PlayBoosterShufflePreFx();
            bool ok = ApplyBoosterShuffle();
            var after = CaptureContainerStates();
            if (ok) StartCoroutine(PlayBoosterShuffleFx(before, after));
            PlaySfx(ok ? SfxId.BoosterShuffle : SfxId.BoosterFail);
            EmitSfxFromStateChanges();

            _speedMultiplier = prevSpeed;
            if (!_fullBeltFastForward && prevSpeed < 4.99f)
            {
                PlaySfx(SfxId.ConveyorSpeeddown);
                if (soundEnabled && _bgm != null) _bgm.PlayStinger(BgmStingerId.Speeddown);
            }
            UpdateBgmPressureAfterTick();
            RefreshFastTag();
            SetInteractableForBooster(true);
            if (!_gameOver) _inputLocked = false;
        }

        private sealed class ContainerStateSnapshot
        {
            public readonly string[] signature;
            public readonly bool[] uniformFull;
            public readonly int[,] colorCounts; // [container, colorIndex]

            public ContainerStateSnapshot(string[] signature, bool[] uniformFull, int[,] colorCounts)
            {
                this.signature = signature;
                this.uniformFull = uniformFull;
                this.colorCounts = colorCounts;
            }
        }

        private ContainerStateSnapshot CaptureContainerStates()
        {
            if (_game == null)
            {
                return new ContainerStateSnapshot(Array.Empty<string>(), Array.Empty<bool>(), new int[0, 0]);
            }

            int n = _game.Containers.Count;
            var sig = new string[n];
            var full = new bool[n];
            int colors = Enum.GetValues(typeof(BlockColor)).Length;
            var counts = new int[n, colors];
            for (int i = 0; i < n; i++)
            {
                var c = _game.Containers[i];
                if (c == null)
                {
                    sig[i] = string.Empty;
                    full[i] = false;
                    continue;
                }

                full[i] = c.IsUniformAndFull();
                sig[i] = BuildContainerSignature(c);
                for (int j = 0; j < c.Blocks.Count; j++)
                {
                    int ci = (int)c.Blocks[j].Color;
                    if (ci >= 0 && ci < colors) counts[i, ci]++;
                }
            }

            return new ContainerStateSnapshot(sig, full, counts);
        }

        private static string BuildContainerSignature(Container c)
        {
            if (c == null || c.Count <= 0) return string.Empty;

            // Compact signature: colors in order + 'H' markers.
            var sb = new System.Text.StringBuilder(c.Count * 2);
            for (int i = 0; i < c.Count; i++)
            {
                var b = c.Blocks[i];
                sb.Append((char)('A' + (int)b.Color));
                if (b.Hidden) sb.Append('H');
            }
            return sb.ToString();
        }

        private List<int> ComputeChangedContainers(ContainerStateSnapshot before, ContainerStateSnapshot after)
        {
            var list = new List<int>();
            if (before == null || after == null) return list;

            int n = Mathf.Min(before.signature.Length, after.signature.Length);
            for (int i = 0; i < n; i++)
            {
                if (!string.Equals(before.signature[i], after.signature[i], StringComparison.Ordinal))
                {
                    list.Add(i);
                }
            }
            return list;
        }

        private IEnumerator PlayBoosterFillFx(ContainerStateSnapshot before, ContainerStateSnapshot after)
        {
            if (_game == null) yield break;

            var changed = ComputeChangedContainers(before, after);
            if (changed.Count == 0) yield break;

            // Prefer a newly-completed box as the "target" of the effect.
            int target = -1;
            for (int i = 0; i < after.uniformFull.Length; i++)
            {
                bool now = after.uniformFull[i];
                bool prev = i < before.uniformFull.Length && before.uniformFull[i];
                if (now && !prev)
                {
                    target = i;
                    break;
                }
            }
            if (target < 0) target = changed[0];

            // Pick highlight color from target's front block if possible.
            Color tint = Color.white;
            if (target >= 0 && target < _game.Containers.Count)
            {
                var c = _game.Containers[target];
                if (c != null && c.Count > 0)
                {
                    tint = BlockVisual.ToUnityColor(c.Blocks[0].Color);
                }
            }
            tint.a = 1f;

            yield return StartCoroutine(PlayTransferTokenFx(before, after, maxTokens: 26));
        }

        private void PlayBoosterShufflePreFx()
        {
            if (_game == null) return;
            for (int i = 0; i < _game.Containers.Count && i < _boxViews.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                if (_boxViews[i] == null) continue;
                _boxViews[i].PlayShuffleJiggle(0.85f);
            }
        }

        private IEnumerator PlayBoosterShuffleFx(ContainerStateSnapshot before, ContainerStateSnapshot after)
        {
            var changed = ComputeChangedContainers(before, after);
            if (changed.Count == 0) yield break;

            yield return StartCoroutine(PlayTransferTokenFx(before, after, maxTokens: 32));
        }

        private IEnumerator PlayTransferTokenFx(ContainerStateSnapshot before, ContainerStateSnapshot after, int maxTokens)
        {
            if (_game == null) yield break;
            if (before?.colorCounts == null || after?.colorCounts == null) yield break;
            if (_boxViews == null || _boxViews.Count == 0) yield break;

            int n = Mathf.Min(before.colorCounts.GetLength(0), after.colorCounts.GetLength(0));
            int colors = Mathf.Min(before.colorCounts.GetLength(1), after.colorCounts.GetLength(1));
            if (n <= 0 || colors <= 0) yield break;

            // Build per-color donor/receiver lists based on count deltas.
            var tokenSpecs = new List<(BlockColor color, int from, int to)>(64);

            for (int ci = 0; ci < colors; ci++)
            {
                var donors = new List<(int idx, int count)>();
                var receivers = new List<(int idx, int count)>();
                for (int i = 0; i < n; i++)
                {
                    int delta = after.colorCounts[i, ci] - before.colorCounts[i, ci];
                    if (delta > 0) receivers.Add((i, delta));
                    else if (delta < 0) donors.Add((i, -delta));
                }

                int di = 0, ri = 0;
                while (di < donors.Count && ri < receivers.Count)
                {
                    var d = donors[di];
                    var r = receivers[ri];
                    int take = Mathf.Min(d.count, r.count);
                    for (int k = 0; k < take; k++)
                    {
                        tokenSpecs.Add(((BlockColor)ci, d.idx, r.idx));
                    }
                    d.count -= take;
                    r.count -= take;
                    donors[di] = d;
                    receivers[ri] = r;
                    if (d.count <= 0) di++;
                    if (r.count <= 0) ri++;
                }
            }

            if (tokenSpecs.Count == 0) yield break;

            // Cap the number of tokens to avoid overwhelming visuals on large moves.
            if (maxTokens > 0 && tokenSpecs.Count > maxTokens)
            {
                // Keep a deterministic subset: take evenly spaced samples.
                var sampled = new List<(BlockColor color, int from, int to)>(maxTokens);
                for (int i = 0; i < maxTokens; i++)
                {
                    int idx = Mathf.RoundToInt((float)i / Mathf.Max(1, maxTokens - 1) * (tokenSpecs.Count - 1));
                    sampled.Add(tokenSpecs[idx]);
                }
                tokenSpecs = sampled;
            }

            float baseDelay = 0.012f;
            float dur = 0.36f;
            for (int i = 0; i < tokenSpecs.Count; i++)
            {
                var spec = tokenSpecs[i];
                if (spec.from < 0 || spec.from >= _boxViews.Count) continue;
                if (spec.to < 0 || spec.to >= _boxViews.Count) continue;
                if (_boxViews[spec.from] == null || _boxViews[spec.to] == null) continue;

                var start = _boxViews[spec.from].GetMouthWorldPosition();
                var end = _boxViews[spec.to].GetMouthWorldPosition();
                StartCoroutine(AnimateTransferToken(spec.color, start, end, dur, delay: baseDelay * i));
            }

            yield return new WaitForSeconds(dur + baseDelay * tokenSpecs.Count);
        }

        private IEnumerator AnimateTransferToken(BlockColor color, Vector3 start, Vector3 end, float seconds, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            var go = BlockVisual.CreateBlock(color);
            go.name = $"TransferToken_{color}";
            go.transform.SetParent(transform, true);
            go.transform.position = start + new Vector3(0f, 0f, beltBlockZOffset);

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float baseSize = Mathf.Max(0.05f, spacing * beltBlockSizeFactor);
            float s0 = baseSize * 0.32f;
            go.transform.localScale = new Vector3(s0, s0, s0 * 0.6f);

            Vector3 mid = (start + end) * 0.5f + Vector3.up * Mathf.Clamp(spacing * 0.65f, 0.25f, 0.75f);

            float t = 0f;
            seconds = Mathf.Clamp(seconds, 0.22f, 0.55f);
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float e = MotionUtil.EaseOutCubic(u);

                // Quadratic Bezier.
                Vector3 a = Vector3.LerpUnclamped(start, mid, e);
                Vector3 b = Vector3.LerpUnclamped(mid, end, e);
                Vector3 p = Vector3.LerpUnclamped(a, b, e);
                p.z = beltBlockZOffset;
                go.transform.position = p;

                // Slight scale down at the end so it feels like it merges.
                float shrink = u > 0.75f ? Mathf.Lerp(1f, 0.7f, (u - 0.75f) / 0.25f) : 1f;
                go.transform.localScale = new Vector3(s0, s0, s0 * 0.6f) * shrink;
                yield return null;
            }

            Destroy(go);
        }

        // (Removed) Booster beam line: keep only token transfers for booster readability.

        private void SetInteractableForBooster(bool val)
        {
            if (_boosterFillButton != null) _boosterFillButton.interactable = val;
            if (_boosterShuffleButton != null) _boosterShuffleButton.interactable = val;
            if (_settingsButton != null) _settingsButton.interactable = val;
            if (_speedButton != null) _speedButton.interactable = val;
        }

        private bool ApplyBoosterFillColor()
        {
            if (_game == null) return false;

            // find completed colors (only unlocked containers)
            var completedColors = new HashSet<BlockColor>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (i < _boxCompleted.Count && _boxCompleted[i]) { if (_game.Containers[i].Blocks.Count > 0) completedColors.Add(_game.Containers[i].Blocks[0].Color); continue; }
                var c = _game.Containers[i];
                if (c.IsUniformAndFull() && c.Blocks.Count > 0)
                {
                    completedColors.Add(c.Blocks[0].Color);
                }
            }

            // Count colors available from UNLOCKED containers only (boosters do not move locked boxes or belt blocks).
            var colorCounts = new Dictionary<BlockColor, int>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                var cont = _game.Containers[i];
                foreach (var b in cont.Blocks)
                {
                    if (!colorCounts.ContainsKey(b.Color)) colorCounts[b.Color] = 0;
                    colorCounts[b.Color]++;
                }
            }

            // Filter: only colors that can fully fill at least one unlocked container.
            var candidates = new List<BlockColor>();
            foreach (var kv in colorCounts)
            {
                if (completedColors.Contains(kv.Key)) continue;
                // check if any eligible container can be fully filled by available blocks
                bool canFill = false;
                for (int i = 0; i < _game.Containers.Count; i++)
                {
                    if (i < _boxLocked.Count && _boxLocked[i]) continue;
                    if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                    var cont = _game.Containers[i];
                    if (cont.IsUniformAndFull()) continue;
                    if (kv.Value >= cont.Capacity)
                    {
                        canFill = true;
                        break;
                    }
                }
                if (canFill)
                {
                    candidates.Add(kv.Key);
                }
            }
            if (candidates.Count == 0) return false;

            var targetColor = candidates[_rng.Next(candidates.Count)];

            // pick target container with most targetColor and not already full uniform of another color
            int targetIdx = -1;
            int bestCount = -1;
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                var c = _game.Containers[i];
                if (c.IsUniformAndFull()) continue;
                // only consider containers we can actually fill
                int available = colorCounts[targetColor];
                if (available < c.Capacity) continue;
                int count = 0;
                foreach (var b in c.Blocks) if (b.Color == targetColor) count++;
                if (count > bestCount)
                {
                    bestCount = count;
                    targetIdx = i;
                }
            }
            if (targetIdx < 0) return false;

            // collect target color blocks from all UNLOCKED containers (keep conveyor intact)
            var sourceBlocks = new List<Block>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                var rem = _game.Containers[i].RemoveBlocksWhere(b => b.Color == targetColor);
                sourceBlocks.AddRange(rem);
            }

            // must have enough to fill target container; otherwise abort without changes
            int required = _game.Containers[targetIdx].Capacity;
            if (sourceBlocks.Count < required)
            {
                Build(_currentLayout);
                return false;
            }

            // collect displaced non-target from target container so they won't disappear
            var displaced = _game.Containers[targetIdx].RemoveBlocksWhere(b => b.Color != targetColor);

            // fill target container with targetColor up to capacity
            int cap = _game.Containers[targetIdx].Capacity;
            var fillList = new List<Block>();
            for (int i = 0; i < cap; i++)
            {
                fillList.Add(sourceBlocks[0]);
                sourceBlocks.RemoveAt(0);
            }
            _game.Containers[targetIdx].ClearAndAdd(fillList);

            // put displaced + leftover target blocks back into containers (fill other unfinished containers)
            var leftovers = new List<Block>();
            leftovers.AddRange(displaced);
            leftovers.AddRange(sourceBlocks);
            if (leftovers.Count > 0)
            {
                for (int i = 0; i < _game.Containers.Count && leftovers.Count > 0; i++)
                {
                    if (i < _boxLocked.Count && _boxLocked[i]) continue;
                    if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                    if (i == targetIdx) continue;
                    var cont = _game.Containers[i];
                    int room = cont.Capacity - cont.Count;
                    int take = Math.Min(room, leftovers.Count);
                    if (take > 0)
                    {
                        var extra = leftovers.GetRange(0, take);
                        cont.AddBlocks(extra);
                        leftovers.RemoveRange(0, take);
                    }
                }
            }

            SyncContainersVisuals();
            SyncBeltVisuals();
            UpdateLocks();
            UpdateCompletionStates();
            CheckEndConditions();
            return true;
        }

        private List<List<Block>> BuildBlockRuns(IReadOnlyList<Block> blocks)
        {
            var runs = new List<List<Block>>();
            if (blocks == null || blocks.Count == 0) return runs;

            var current = new List<Block> { blocks[0] };
            for (int i = 1; i < blocks.Count; i++)
            {
                var b = blocks[i];
                if (b.Color == current[0].Color)
                {
                    current.Add(b);
                }
                else
                {
                    runs.Add(new List<Block>(current));
                    current.Clear();
                    current.Add(b);
                }
            }
            runs.Add(new List<Block>(current));
            return runs;
        }

        private bool ApplyBoosterShuffle()
        {
            if (_game == null) return false;

            var completedColors = new HashSet<BlockColor>();
            var completedContainers = new HashSet<int>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                var c = _game.Containers[i];
                if (c.IsUniformAndFull())
                {
                    completedContainers.Add(i);
                    if (c.Blocks.Count > 0) completedColors.Add(c.Blocks[0].Color);
                }
            }

            // gather chunks (consecutive runs) from unfinished containers
            var chunks = new List<List<Block>>();
            var targetContainers = new List<int>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (completedContainers.Contains(i)) continue;
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                targetContainers.Add(i);
                var runs = BuildBlockRuns(_game.Containers[i].Blocks);
                chunks.AddRange(runs);
                _game.Containers[i].ClearAndAdd(Array.Empty<Block>());
            }

            // conveyor: keep as-is (do not disturb existing belt blocks)

            if (chunks.Count == 0) return false;

            // shuffle chunks
            for (int i = chunks.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                (chunks[i], chunks[j]) = (chunks[j], chunks[i]);
            }

            var queue = new Queue<List<Block>>(chunks);
            // distribute chunks into containers, splitting if overflow (remaining re-enqueued)
            foreach (var idx in targetContainers)
            {
                var cont = _game.Containers[idx];
                int space = cont.Capacity;
                var newBlocks = new List<Block>();
                while (space > 0 && queue.Count > 0)
                {
                    var chunk = queue.Dequeue();
                    int take = Math.Min(space, chunk.Count);
                    newBlocks.AddRange(chunk.GetRange(0, take));
                    space -= take;
                    if (take < chunk.Count)
                    {
                        chunk.RemoveRange(0, take);
                        queue.Enqueue(chunk);
                    }
                }
                cont.ClearAndAdd(newBlocks);
            }

            // leftover chunks: try to fit into any remaining space in unfinished containers, otherwise ignore (belt stays unchanged)
            int guard = 0;
            while (queue.Count > 0 && guard < 1024)
            {
                guard++;
                var ch = queue.Dequeue();
                bool placed = false;
                for (int i = 0; i < targetContainers.Count && ch.Count > 0; i++)
                {
                    var cont = _game.Containers[targetContainers[i]];
                    int room = cont.Capacity - cont.Count;
                    int take = Math.Min(room, ch.Count);
                    if (take > 0)
                    {
                        var extra = ch.GetRange(0, take);
                        cont.AddBlocks(extra);
                        ch.RemoveRange(0, take);
                        placed = true;
                    }
                }
                if (ch.Count > 0 && placed)
                {
                    queue.Enqueue(ch);
                }
                if (!placed)
                {
                    break;
                }
            }
            SyncContainersVisuals();
            SyncBeltVisuals();
            UpdateLocks();
            UpdateCompletionStates();
            CheckEndConditions();
            return true;
        }

        private void Update()
        {
            if (_game == null)
            {
                return;
            }

            UpdateConveyorLoopSfx();
            if (_gameOver)
            {
                return;
            }

            float effectiveSpeed = _fullBeltFastForward ? 5f : _speedMultiplier;
            _tickTimer += Time.deltaTime * effectiveSpeed;
            float progress = Mathf.Clamp01(_tickTimer / Mathf.Max(0.0001f, conveyorTickSeconds));
            UpdateSlotMarkersVisuals(progress);
            UpdateBeltBlockVisuals(progress);

            if (_tickTimer >= conveyorTickSeconds)
            {
                _tickTimer = 0f;
                int? blocked = _isReleasing && TryGetBlockedPort() is int idx ? idx : (int?)null;
                _portEvents.Clear();
                _game.TickConveyor(blocked, _portEvents);
                ProcessConveyorPortEvents(_portEvents);
                _conveyorTickSfxCountdown--;
                if (_conveyorTickSfxCountdown <= 0)
                {
                    PlaySfx(SfxId.ConveyorTick);
                    _conveyorTickSfxCountdown = 6 + _rng.Next(2); // 6~7 ticks
                }
                SyncBeltVisuals();
                SyncContainersVisuals();
                UpdateLocks();
                UpdateCompletionStates();
                UpdateBeltCounter();
                HandleFullBeltFastForwardAfterTick();
                UpdateBgmPressureAfterTick();
                EmitSfxFromStateChanges();
                CheckEndConditions();
            }
        }

        private void ProcessConveyorPortEvents(List<ConveyorPortEvent> events)
        {
            if (events == null || events.Count == 0) return;
            _rejectGate ??= new RejectFeedbackGate();

            ConveyorPortOutcome? bestSfxOutcome = null;
            int bestSfxContainerIndex = -1;

            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                int containerIndex = -1;
                if (e.Container != null && !_containerIndexByRef.TryGetValue(e.Container, out containerIndex))
                {
                    containerIndex = -1;
                }

                // Visual feedback: bounce the belt block on hard rejects (no flashing box).
                if (e.Outcome == ConveyorPortOutcome.RejectedLocked ||
                    e.Outcome == ConveyorPortOutcome.RejectedBusy ||
                    e.Outcome == ConveyorPortOutcome.RejectedFull ||
                    e.Outcome == ConveyorPortOutcome.RejectedMismatch)
                {
                    if (containerIndex >= 0 && containerIndex < _boxViews.Count)
                    {
                        var c = BlockVisual.ToUnityColor(e.Block.Color);
                        c.a = 1f;
                        _boxViews[containerIndex].PlayMouthSquash(c, seconds: 0.14f);
                    }
                    ConsiderForBestSfx(e.Outcome, containerIndex, ref bestSfxOutcome, ref bestSfxContainerIndex);
                }
                else if (e.Outcome == ConveyorPortOutcome.Inserted)
                {
                    StartBeltBlockEnterBoxAnimation(e.BeltIndex, containerIndex, e.Block);
                    if (containerIndex >= 0 && containerIndex < _boxViews.Count)
                    {
                        _boxViews[containerIndex].PlayMouthSquash(Color.white, seconds: 0.12f);
                    }
                }
                else if (e.Outcome == ConveyorPortOutcome.SkippedEmptyBoxPreferredTarget)
                {
                    // Empty-deferred is not a failure: show an info-only routing hint (no SFX).
                    if (containerIndex >= 0)
                    {
                        ShowEmptyDeferredHint(containerIndex, e.Block);
                    }
                }
            }

            if (bestSfxOutcome.HasValue && _rejectGate.ShouldPlay(bestSfxOutcome.Value, bestSfxContainerIndex))
            {
                PlaySfx(MapRejectOutcomeToSfx(bestSfxOutcome.Value));
            }
        }

        private static void ConsiderForBestSfx(
            ConveyorPortOutcome outcome,
            int containerIndex,
            ref ConveyorPortOutcome? best,
            ref int bestContainerIndex)
        {
            int Priority(ConveyorPortOutcome o) => o switch
            {
                ConveyorPortOutcome.RejectedLocked => 4,
                ConveyorPortOutcome.RejectedBusy => 3,
                ConveyorPortOutcome.RejectedFull => 2,
                ConveyorPortOutcome.RejectedMismatch => 1,
                _ => 0,
            };

            if (!best.HasValue || Priority(outcome) > Priority(best.Value))
            {
                best = outcome;
                bestContainerIndex = containerIndex;
            }
        }

        private static SfxId MapRejectOutcomeToSfx(ConveyorPortOutcome outcome)
        {
            return outcome switch
            {
                ConveyorPortOutcome.RejectedLocked => SfxId.BlockRejectLocked,
                ConveyorPortOutcome.RejectedBusy => SfxId.BlockRejectBusy,
                ConveyorPortOutcome.RejectedFull => SfxId.BlockRejectFull,
                ConveyorPortOutcome.RejectedMismatch => SfxId.BlockRejectMismatch,
                _ => SfxId.BlockReject
            };
        }

        private void StartBeltBlockRejectBounce(int beltIndex, int containerIndex)
        {
            if (beltIndex < 0) return;
            if (!_beltBlockVisuals.TryGetValue(beltIndex, out var go) || go == null) return;

            Vector3 dir = Vector3.up;
            if (containerIndex >= 0 && containerIndex < _boxSpecs.Count && _boxSpecs[containerIndex] != null)
            {
                dir = OpeningToWorldNormal(_boxSpecs[containerIndex].opening);
            }

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float amp = Mathf.Clamp(spacing * 0.18f, 0.06f, 0.14f);

            StartBeltBlockOffsetAnimation(beltIndex, dir.normalized * amp, 0.14f);
        }

        private void StartBeltBlockEnterBoxAnimation(int beltIndex, int containerIndex, Block block)
        {
            if (beltIndex < 0) return;
            if (containerIndex < 0 || containerIndex >= _boxViews.Count) return;

            if (!_beltBlockVisuals.TryGetValue(beltIndex, out var go) || go == null)
            {
                // If visual hasn't been built yet (rare timing), create one so we can animate it.
                EnsureBlockVisual(beltIndex, block);
                _beltBlockVisuals.TryGetValue(beltIndex, out go);
            }
            if (go == null) return;

            if (_beltInsertCoroutines.TryGetValue(beltIndex, out var existing) && existing != null)
            {
                StopCoroutine(existing);
            }
            _beltInsertCoroutines.Remove(beltIndex);
            _beltInsertAnimating.Add(beltIndex);
            _beltInsertCoroutines[beltIndex] = StartCoroutine(AnimateBeltEnterBox(beltIndex, containerIndex));
        }

        private IEnumerator AnimateBeltEnterBox(int beltIndex, int containerIndex)
        {
            if (!_beltBlockVisuals.TryGetValue(beltIndex, out var go) || go == null) yield break;
            if (containerIndex < 0 || containerIndex >= _boxViews.Count) yield break;

            var view = _boxViews[containerIndex];
            if (view == null) yield break;

            Vector3 from = go.transform.position;
            Vector3 baseScale = go.transform.localScale;
            Vector3 mouth = view.GetMouthWorldPosition() + view.GetMouthWorldNormal() * 0.02f + new Vector3(0f, 0f, beltBlockZOffset);

            float dur = Mathf.Clamp(conveyorTickSeconds * 0.45f, 0.08f, 0.18f);
            float t = 0f;
            while (t < dur)
            {
                if (go == null) break;
                t += Time.deltaTime * Mathf.Max(0.0001f, EffectiveSpeedMultiplier);
                float u = Mathf.Clamp01(t / dur);
                float e = MotionUtil.EaseOutCubic(u);
                go.transform.position = Vector3.LerpUnclamped(from, mouth, e);
                // slight shrink to imply "getting swallowed"
                float s = Mathf.Lerp(1f, 0.6f, e);
                go.transform.localScale = baseScale * s;
                yield return null;
            }

            // Cleanup: remove this belt visual now that it entered the box.
            if (_beltBlockVisuals.TryGetValue(beltIndex, out var finalGo) && finalGo != null)
            {
                Destroy(finalGo);
                _beltBlockVisuals.Remove(beltIndex);
            }
            _beltInsertAnimating.Remove(beltIndex);
            _beltInsertCoroutines.Remove(beltIndex);
        }

        private static Vector3 OpeningToWorldNormal(OpeningSide opening)
        {
            return opening switch
            {
                OpeningSide.Top => Vector3.up,
                OpeningSide.Bottom => Vector3.down,
                OpeningSide.Left => Vector3.left,
                OpeningSide.Right => Vector3.right,
                _ => Vector3.up
            };
        }

        private void StartBeltBlockOffsetAnimation(int beltIndex, Vector3 peakOffset, float seconds)
        {
            if (_beltBlockOffsetCoroutines.TryGetValue(beltIndex, out var existing) && existing != null)
            {
                StopCoroutine(existing);
            }
            _beltBlockOffsetCoroutines.Remove(beltIndex);

            _beltBlockOffsets[beltIndex] = Vector3.zero;
            _beltBlockOffsetCoroutines[beltIndex] = StartCoroutine(AnimateBeltBlockOffset(beltIndex, peakOffset, seconds));
        }

        private IEnumerator AnimateBeltBlockOffset(int beltIndex, Vector3 peakOffset, float seconds)
        {
            seconds = Mathf.Clamp(seconds, 0.08f, 0.22f);
            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime * Mathf.Max(0.0001f, EffectiveSpeedMultiplier);
                float u = Mathf.Clamp01(t / seconds);
                // quick push then settle: sin curve (0..pi)
                float s = Mathf.Sin(u * Mathf.PI);
                _beltBlockOffsets[beltIndex] = peakOffset * s;
                yield return null;
            }
            _beltBlockOffsets[beltIndex] = Vector3.zero;
            _beltBlockOffsetCoroutines.Remove(beltIndex);
        }

        private Vector3 GetBeltBlockOffset(int beltIndex)
        {
            return _beltBlockOffsets.TryGetValue(beltIndex, out var o) ? o : Vector3.zero;
        }

        private void ShowEmptyDeferredHint(int emptyBoxIndex, Block block)
        {
            if (_game == null) return;
            if (emptyBoxIndex < 0 || emptyBoxIndex >= _game.Containers.Count) return;

            int target = FindPreferredNonEmptyTargetIndex(block, excludeIndex: emptyBoxIndex);
            if (target < 0) return;

            var color = BlockVisual.ToUnityColor(block.Color);
            color.a = 0.35f; // treat alpha as intensity for hint

            if (target >= 0 && target < _boxViews.Count)
            {
                _boxViews[target].PlayInfoHint(color, sizeFactor: 1.10f, seconds: 0.18f);
            }

            EnsureEmptyDeferredLine();
            if (_emptyDeferredLine == null) return;

            var from = _boxViews[emptyBoxIndex] != null ? _boxViews[emptyBoxIndex].transform.position : Vector3.zero;
            var to = _boxViews[target] != null ? _boxViews[target].transform.position : Vector3.zero;
            from.z = to.z = 0.02f; // slightly above gameplay plane

            if (_emptyDeferredHintRoutine != null) StopCoroutine(_emptyDeferredHintRoutine);
            _emptyDeferredHintRoutine = StartCoroutine(AnimateEmptyDeferredLine(from, to, color, 0.15f));
        }

        private int FindPreferredNonEmptyTargetIndex(Block block, int excludeIndex)
        {
            if (_game == null) return -1;

            int best = -1;
            int bestCount = -1;
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i == excludeIndex) continue;
                var c = _game.Containers[i];
                if (c == null) continue;
                if (c.Count <= 0) continue;
                if (!c.CanAccept(block)) continue;
                if (c.Count > bestCount)
                {
                    bestCount = c.Count;
                    best = i;
                }
            }
            return best;
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

        private IEnumerator AnimateEmptyDeferredLine(Vector3 from, Vector3 to, Color color, float seconds)
        {
            if (_emptyDeferredLine == null) yield break;
            seconds = Mathf.Clamp(seconds, 0.10f, 0.22f);

            _emptyDeferredLine.enabled = true;
            _emptyDeferredLine.SetPosition(0, from);
            _emptyDeferredLine.SetPosition(1, to);

            var mat = _emptyDeferredLine.sharedMaterial;
            if (mat != null)
            {
                var c = color;
                c.a = 0f;
                mat.color = c;
            }

            float t = 0f;
            while (t < seconds)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / seconds);
                float a = MotionUtil.EaseOutCubic(u);
                if (mat != null)
                {
                    var c = color;
                    c.a = Mathf.Clamp01(color.a) * (u < 0.25f ? (u / 0.25f) : (1f - a));
                    mat.color = c;
                }
                yield return null;
            }

            _emptyDeferredLine.enabled = false;
        }

        private sealed class RejectFeedbackGate
        {
            private float _lastPlayTime = -999f;
            private ConveyorPortOutcome? _lastOutcome;
            private int _sameOutcomeCount;
            private readonly HashSet<int> _playedLockedBoxOnce = new HashSet<int>();
            private readonly HashSet<int> _playedBusyBoxOnce = new HashSet<int>();

            public void ResetForNewLevel()
            {
                _lastPlayTime = -999f;
                _lastOutcome = null;
                _sameOutcomeCount = 0;
                _playedLockedBoxOnce.Clear();
                _playedBusyBoxOnce.Clear();
            }

            public bool ShouldPlay(ConveyorPortOutcome outcome, int containerIndex)
            {
                float now = Time.unscaledTime;

                const float minInterval = 0.30f;
                if (now - _lastPlayTime < minInterval)
                {
                    return false;
                }

                if (outcome == ConveyorPortOutcome.RejectedLocked)
                {
                    if (containerIndex >= 0 && _playedLockedBoxOnce.Contains(containerIndex)) return false;
                    if (containerIndex >= 0) _playedLockedBoxOnce.Add(containerIndex);
                    MarkPlayed(now, outcome);
                    return true;
                }

                if (outcome == ConveyorPortOutcome.RejectedBusy)
                {
                    if (containerIndex >= 0 && _playedBusyBoxOnce.Contains(containerIndex)) return false;
                    if (containerIndex >= 0) _playedBusyBoxOnce.Add(containerIndex);
                    MarkPlayed(now, outcome);
                    return true;
                }

                // Full / mismatch: play on reason change; otherwise play occasionally with a higher threshold.
                if (_lastOutcome.HasValue && _lastOutcome.Value == outcome)
                {
                    _sameOutcomeCount++;
                    const int streakThreshold = 6;
                    const float repeatInterval = 0.65f;
                    if (_sameOutcomeCount < streakThreshold) return false;
                    if (now - _lastPlayTime < repeatInterval) return false;
                    MarkPlayed(now, outcome);
                    _sameOutcomeCount = 0;
                    return true;
                }

                _sameOutcomeCount = 0;
                MarkPlayed(now, outcome);
                return true;
            }

            private void MarkPlayed(float now, ConveyorPortOutcome outcome)
            {
                _lastPlayTime = now;
                _lastOutcome = outcome;
            }
        }

        private void HandleFullBeltFastForwardAfterTick()
        {
            if (_gameOver || _game == null) return;

            int limit = _beltCapacity > 0 ? Mathf.Min(_beltCapacity, _game.Conveyor.Length) : _game.Conveyor.Length;
            bool beltFull = _game.Conveyor.BlockCount >= limit;

            if (_fullBeltFastForward)
            {
                if (!beltFull)
                {
                    StopFullBeltFastForward();
                    return;
                }

                _fullBeltStepsRemaining = Mathf.Max(0, _fullBeltStepsRemaining - 1);
                if (_fullBeltStepsRemaining <= 0)
                {
                    // Still full after one full loop: fail the level.
                    PlaySfx(SfxId.ConveyorFullFail);
                    _sfxSuppressSpeeddownOnce = true;
                    BeginEndSequence(win: false, delaySeconds: LoseEndSequenceDelaySeconds);
                    StopFullBeltFastForward();
                }
                return;
            }

            if (beltFull)
            {
                StartFullBeltFastForward();
            }
        }

        private void StartFullBeltFastForward()
        {
            if (_game == null) return;
            _fullBeltFastForward = true;
            _fullBeltStepsRemaining = Mathf.Max(1, _game.Conveyor.Length);
            RefreshFastTag();
        }

        private void StopFullBeltFastForward()
        {
            _fullBeltFastForward = false;
            _fullBeltStepsRemaining = 0;
            RefreshFastTag();
        }

        private void RefreshFastTag()
        {
            if (_fastTag == null) return;
            bool show = _fullBeltFastForward || _speedMultiplier >= 4.99f;
            bool danger = _fullBeltFastForward;
            _fastTag.SetActive(show);
            if (_fastTagText != null) _fastTagText.text = "FAST x5";
            if (_fastTagBg != null && LoopSortingUIKit.IsAvailable())
            {
                _fastTagBg.sprite = LoopSortingUIKit.LoadSpriteByKey(danger ? "ui.tag_fast.danger" : "ui.tag_fast.info");
                _fastTagBg.type = Image.Type.Sliced;
                _fastTagBg.color = Color.white;
            }
        }

        public void HandleContainerClick(int containerIndex)
        {
            if (_game == null || _isReleasing || _inputLocked)
            {
                return;
            }

            if (containerIndex < 0 || containerIndex >= _game.Containers.Count)
            {
                return;
            }

            var container = _game.Containers[containerIndex];
            if (containerIndex < _boxCompleted.Count && _boxCompleted[containerIndex])
            {
                PlaySfx(SfxId.UiDenied);
                if (containerIndex < _boxViews.Count) _boxViews[containerIndex].PlayDeniedFeedback();
                return;
            }
            if (containerIndex < _boxLocked.Count && _boxLocked[containerIndex])
            {
                PlaySfx(SfxId.BoxLockedThunk);
                if (containerIndex < _boxViews.Count) _boxViews[containerIndex].PlayDeniedFeedback();
                return;
            }
            if (!container.TryPeek(out var first))
            {
                PlaySfx(SfxId.UiDenied);
                if (containerIndex < _boxViews.Count) _boxViews[containerIndex].PlayDeniedFeedback();
                return;
            }

            PlaySfx(SfxId.BoxSelect);
            if (containerIndex < _boxViews.Count) _boxViews[containerIndex].PlayTapFeedback();

            // show outline for the pending run
            int pending = 0;
            for (int i = 0; i < container.Count; i++)
            {
                if (container.Blocks[i].Color == first.Color) pending++; else break;
            }
            if (containerIndex < _boxViews.Count)
            {
                _boxViews[containerIndex].ShowFrontOutline(pending, true);
            }

            StartCoroutine(ReleaseRoutine(containerIndex, first.Color));
        }

        private void BuildConveyor(LevelLayout layout)
        {
            if (layout.conveyors == null || layout.conveyors.Count == 0)
            {
                Debug.LogError("No conveyor path defined in layout.");
                return;
            }

            // Pick the first conveyor that has at least 2 points; ignore empty placeholders.
            ConveyorPath path = null;
            for (int i = 0; i < layout.conveyors.Count; i++)
            {
                var candidate = layout.conveyors[i];
                if (candidate != null && candidate.points != null && candidate.points.Count >= 2)
                {
                    path = candidate;
                    break;
                }
            }
            if (path == null)
            {
                Debug.LogError("No valid conveyor path (needs at least 2 points).");
                return;
            }
            _beltLoop = path.loop;

            float spacing = layout.beltSlotSpacing > 0f ? layout.beltSlotSpacing : beltSlotSpacing;
            _beltSlots = LayoutUtils.BuildSlotsFromPath(
                path,
                spacing,
                _beltCapacity,
                out _beltSpacingUsed,
                smoothCorners: layout.smoothCorners,
                smoothTension: layout.cornerSmoothTension,
                smoothSubdivisions: layout.cornerSubdivisions);

            // Safety: ensure we always have at least one slot to avoid zero-length conveyor crashes.
            if (_beltSlots == null || _beltSlots.Count == 0)
            {
                _beltSlots = new List<Transform>();
                var t = new GameObject("Slot_0").transform;
                t.position = path.points != null && path.points.Count > 0
                    ? new Vector3(path.points[0].x, path.points[0].y, 0f)
                    : Vector3.zero;
                _beltSlots.Add(t);
                _beltSpacingUsed = spacing;
                Debug.LogWarning("BuildConveyor: slot generation failed, created a single fallback slot to keep the game running.");
            }

            BuildConveyorBelt(
                path,
                spacing,
                smoothCorners: layout.smoothCorners,
                smoothTension: layout.cornerSmoothTension,
                smoothSubdivisions: layout.cornerSubdivisions);
            BuildSlotMarkers();
            EnsureBackground();

            var trackParent = new GameObject("ConveyorSlots");
            trackParent.transform.SetParent(transform, false);
            foreach (var t in _beltSlots)
            {
                t.SetParent(trackParent.transform, true);
            }
        }

        private void BuildConveyorBelt(
            ConveyorPath path,
            float slotSpacing,
            bool smoothCorners,
            float smoothTension,
            int smoothSubdivisions)
        {
            if (_conveyorBelt != null)
            {
                DestroyImmediate(_conveyorBelt);
                _conveyorBelt = null;
            }

            if (_beltSlots == null || _beltSlots.Count < 2) return;
            if (path == null || path.points == null || path.points.Count < 2) return;

            float spacing = slotSpacing > 0.0001f ? slotSpacing : (_beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing);
            float requestedWidth = path != null ? path.width : 1f;
            // Keep belt width visually reasonable relative to slot spacing (prevents "screen-filling" ribbon).
            float beltWidth = Mathf.Clamp(requestedWidth, spacing * 0.8f, spacing * 1.6f);

            bool loop = path != null && path.loop;
            var pts = BuildBeltPolylinePoints(path, spacing, smoothCorners, smoothTension, smoothSubdivisions, loop, z: 0.2f);
            if (pts == null || pts.Count < 2) return;

            var go = new GameObject("ConveyorBelt");
            go.transform.SetParent(transform, false);
            _conveyorBelt = go;

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = BuildRibbonMesh(pts, beltWidth, out float totalLen);

            var mat = CreateConveyorBeltMaterial(totalLen, spacing, loop);
            mr.sharedMaterial = mat;
        }

        private static Mesh BuildRibbonMesh(List<Vector3> points, float width, out float totalLen)
        {
            totalLen = 0f;
            var mesh = new Mesh();
            mesh.name = "ConveyorBeltRibbon";

            if (points == null || points.Count < 2)
            {
                return mesh;
            }

            int n = points.Count;
            var vertices = new Vector3[n * 2];
            var uvs = new Vector2[n * 2];
            var tris = new int[(n - 1) * 6];

            float half = width * 0.5f;
            var cumulative = new float[n];
            cumulative[0] = 0f;
            for (int i = 1; i < n; i++)
            {
                cumulative[i] = cumulative[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
            totalLen = cumulative[n - 1];
            float invTotal = 1f / Mathf.Max(0.0001f, totalLen);

            for (int i = 0; i < n; i++)
            {
                Vector2 dir;
                if (i == 0)
                {
                    dir = (points[1] - points[0]);
                }
                else if (i == n - 1)
                {
                    dir = (points[n - 1] - points[n - 2]);
                }
                else
                {
                    dir = (points[i + 1] - points[i - 1]);
                }

                float len = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y);
                if (len < 0.0001f) len = 1f;
                dir /= len;

                var perp = new Vector2(-dir.y, dir.x);
                var p = points[i];
                vertices[i * 2 + 0] = new Vector3(p.x + perp.x * half, p.y + perp.y * half, p.z);
                vertices[i * 2 + 1] = new Vector3(p.x - perp.x * half, p.y - perp.y * half, p.z);

                // Normalize U into 0..1; material tiling controls repetition count.
                float u01 = cumulative[i] * invTotal;
                uvs[i * 2 + 0] = new Vector2(u01, 1f);
                uvs[i * 2 + 1] = new Vector2(u01, 0f);
            }

            int ti = 0;
            for (int i = 0; i < n - 1; i++)
            {
                int a0 = i * 2;
                int a1 = i * 2 + 1;
                int b0 = (i + 1) * 2;
                int b1 = (i + 1) * 2 + 1;

                tris[ti++] = a0;
                tris[ti++] = b0;
                tris[ti++] = a1;

                tris[ti++] = a1;
                tris[ti++] = b0;
                tris[ti++] = b1;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Material CreateConveyorBeltMaterial(float totalLength, float tileWorld, bool loop)
        {
            Texture2D tex = null;
            if (LoopSortingUIKit.IsAvailable())
            {
                tex = LoopSortingUIKit.LoadTextureByKey("world.conveyor_belt");
            }

            // Render early and don't write depth so blocks/markers always appear on top.
            // Important: prefer a shader that supports alpha; otherwise a missing texture can turn into an opaque white band.
            var shader =
                Shader.Find("Unlit/Transparent") ??
                Shader.Find("Sprites/Default") ??
                Shader.Find("UI/Default") ??
                Shader.Find("Unlit/Texture");
            if (shader == null)
            {
                // Can't create a custom material; return null so callers can fall back gracefully.
                return null;
            }

            var mat = new Material(shader);
            mat.renderQueue = 1800;
            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);

            if (tex != null)
            {
                tex.wrapMode = TextureWrapMode.Repeat;
                mat.mainTexture = tex;

                // Tile U by world length (normalized UVs): 1 tile per belt slot spacing by default.
                float worldPerTile = Mathf.Max(0.1f, tileWorld);
                float rawTilingU = Mathf.Max(0.01f, totalLength / worldPerTile);
                // For loops, force an integer repeat count so the seam meets perfectly (u=0 and u=1 sample same texel).
                float tilingU = loop ? Mathf.Max(1f, Mathf.Round(rawTilingU)) : rawTilingU;
                mat.mainTextureScale = new Vector2(tilingU, 1f);
                mat.color = Color.white;
            }
            else
            {
                // Placeholder: subtle band until art is dropped in.
                mat.mainTexture = Texture2D.whiteTexture;
                mat.color = new Color(0.12f, 0.16f, 0.22f, 0.45f);
            }

            return mat;
        }

        private static List<Vector3> BuildBeltPolylinePoints(
            ConveyorPath path,
            float slotSpacing,
            bool smoothCorners,
            float smoothTension,
            int smoothSubdivisions,
            bool loop,
            float z)
        {
            if (path == null || path.points == null || path.points.Count < 2)
            {
                return null;
            }

            // Build a polyline (optionally with rounded corners) in 2D.
            var basePts = new List<Vector2>(path.points);
            List<Vector2> samplePts;
            if (smoothCorners)
            {
                samplePts = loop
                    ? BuildRoundedPathLoop(basePts, slotSpacing, smoothTension, smoothSubdivisions)
                    : BuildRoundedPathOpen(basePts, slotSpacing, smoothTension, smoothSubdivisions);
            }
            else
            {
                samplePts = basePts;
            }

            if (samplePts == null || samplePts.Count < 2) return null;

            // Ensure loop is closed by repeating the first point at the end.
            if (loop)
            {
                if (samplePts.Count < 3) return null;
                samplePts = new List<Vector2>(samplePts);
                samplePts.Add(samplePts[0]);
            }

            // Resample to a finer point spacing so corners look smooth regardless of slot count.
            float total = 0f;
            for (int i = 0; i < samplePts.Count - 1; i++)
            {
                total += Vector2.Distance(samplePts[i], samplePts[i + 1]);
            }
            if (total <= 0.0001f) return null;

            float step = Mathf.Max(0.08f, slotSpacing * 0.35f);
            int count = Mathf.Clamp(Mathf.CeilToInt(total / step) + 1, 8, 4096);

            var cumulative = new List<float>(samplePts.Count);
            float sum = 0f;
            cumulative.Add(0f);
            for (int i = 1; i < samplePts.Count; i++)
            {
                sum += Vector2.Distance(samplePts[i - 1], samplePts[i]);
                cumulative.Add(sum);
            }

            var pts3 = new List<Vector3>(count + (loop ? 1 : 0));
            for (int i = 0; i < count; i++)
            {
                float dist = (i / (float)(count - 1)) * total;
                var p = PointAtDistance(samplePts, cumulative, dist);
                pts3.Add(new Vector3(p.x, p.y, z));
            }

            return pts3;
        }

        private static Vector2 PointAtDistance(List<Vector2> pts, List<float> cumulative, float dist)
        {
            if (pts == null || pts.Count == 0 || cumulative == null || cumulative.Count != pts.Count)
            {
                return Vector2.zero;
            }

            float total = cumulative[cumulative.Count - 1];
            if (dist >= total)
            {
                return pts[pts.Count - 1];
            }

            for (int i = 0; i < cumulative.Count - 1; i++)
            {
                float start = cumulative[i];
                float end = cumulative[i + 1];
                if (dist <= end)
                {
                    float segLen = end - start;
                    float t = segLen <= 0.0001f ? 0f : (dist - start) / segLen;
                    return Vector2.Lerp(pts[i], pts[i + 1], t);
                }
            }

            return pts[pts.Count - 1];
        }

        private static List<Vector2> BuildRoundedPathOpen(IList<Vector2> pts, float desiredSpacing, float tension, int subdivisions)
        {
            var samples = new List<Vector2>();
            if (pts == null || pts.Count < 2) return samples;

            tension = Mathf.Clamp01(tension);
            subdivisions = Mathf.Max(2, subdivisions);

            samples.Add(pts[0]);
            for (int i = 1; i < pts.Count - 1; i++)
            {
                var prev = pts[i - 1];
                var curr = pts[i];
                var next = pts[i + 1];

                var dirIn = curr - prev;
                var dirOut = next - curr;
                float lenIn = dirIn.magnitude;
                float lenOut = dirOut.magnitude;
                if (lenIn < 0.0001f || lenOut < 0.0001f)
                {
                    samples.Add(curr);
                    continue;
                }

                dirIn /= lenIn;
                dirOut /= lenOut;

                float maxRadius = desiredSpacing > 0f ? desiredSpacing * 0.75f : Mathf.Min(lenIn, lenOut);
                float radius = Mathf.Min(lenIn, lenOut, maxRadius) * tension;

                var pIn = curr - dirIn * radius;
                var pOut = curr + dirOut * radius;

                samples.Add(pIn);
                for (int s = 1; s < subdivisions - 1; s++)
                {
                    float t = s / (float)(subdivisions - 1);
                    samples.Add(Vector2.Lerp(pIn, pOut, t));
                }
                samples.Add(pOut);
            }
            samples.Add(pts[pts.Count - 1]);
            return samples;
        }

        private static List<Vector2> BuildRoundedPathLoop(IList<Vector2> pts, float desiredSpacing, float tension, int subdivisions)
        {
            var samples = new List<Vector2>();
            if (pts == null || pts.Count < 3) return samples;

            tension = Mathf.Clamp01(tension);
            subdivisions = Mathf.Max(2, subdivisions);

            int n = pts.Count;
            for (int i = 0; i < n; i++)
            {
                var prev = pts[(i - 1 + n) % n];
                var curr = pts[i];
                var next = pts[(i + 1) % n];

                var dirIn = curr - prev;
                var dirOut = next - curr;
                float lenIn = dirIn.magnitude;
                float lenOut = dirOut.magnitude;
                if (lenIn < 0.0001f || lenOut < 0.0001f)
                {
                    samples.Add(curr);
                    continue;
                }

                dirIn /= lenIn;
                dirOut /= lenOut;

                float maxRadius = desiredSpacing > 0f ? desiredSpacing * 0.75f : Mathf.Min(lenIn, lenOut);
                float radius = Mathf.Min(lenIn, lenOut, maxRadius) * tension;

                var pIn = curr - dirIn * radius;
                var pOut = curr + dirOut * radius;

                samples.Add(pIn);
                for (int s = 1; s < subdivisions - 1; s++)
                {
                    float t = s / (float)(subdivisions - 1);
                    samples.Add(Vector2.Lerp(pIn, pOut, t));
                }
                samples.Add(pOut);
            }

            return samples;
        }

        private void BuildContainers(LevelLayout layout)
        {
            _boxSpecs.Clear();
            _boxLocked.Clear();
            _boxCompleted.Clear();
            var containers = new List<Container>();
            var containerToBelt = new Dictionary<int, int>();
            var reservedSlots = new HashSet<int>();
            var autoAvoidSlots = new HashSet<int>();
            if (_beltSlots != null && _beltSlots.Count > 1)
            {
                autoAvoidSlots.Add(0); // Avoid slot0 for auto-align (loop seam / edge-case prone).
            }

            var parent = new GameObject("Containers");
            parent.transform.SetParent(transform, false);

            float unit = layout.blockSize > 0 ? layout.blockSize : blockVisualSize.x;
            blockVisualSize = new Vector2(unit, unit);

            for (int i = 0; i < layout.boxes.Count; i++)
            {
                var spec = layout.boxes[i];

                // derive size from rows/cols and block size
                spec.size = LayoutUtils.ComputeBoxSize(spec, unit);

                var go = new GameObject(string.IsNullOrEmpty(spec.name) ? $"Box_{i}" : spec.name);
                go.transform.SetParent(parent.transform, false);
                go.transform.position = new Vector3(spec.position.x, spec.position.y, 0f);
                go.transform.localScale = Vector3.one;

                var boxView = go.AddComponent<BoxView>();
                int columns = Mathf.Max(1, spec.columns);
                int rows = Mathf.Max(1, spec.rows);
                boxView.Init(i, this, spec.size, columns, rows, blockVisualSize, spec.opening);
                boxView.SetLocked(spec.locked, spec.unlockColor);
                boxView.SetCompleted(false);
                _boxViews.Add(boxView);
                _boxSpecs.Add(spec);
                _boxLocked.Add(spec.locked);
                _boxCompleted.Add(false);

                int capacity = Mathf.Max(1, columns * rows);
                var boxBlocks = BuildBlocksForSpec(spec, capacity);
                var container = new Container(capacity, boxBlocks);
                container.SetLocked(spec.locked);
                containers.Add(container);

                int slotIndex;
                if (spec.autoAlignSlot)
                {
                    slotIndex = LayoutUtils.ResolveBeltSlotIndex(spec, _beltSlots, unit, reservedSlots, autoAvoidSlots);
                }
                else
                {
                    slotIndex = Mathf.Clamp(spec.beltSlotIndex, 0, Mathf.Max(0, _beltSlots.Count - 1));
                    if (reservedSlots.Contains(slotIndex) && _beltSlots.Count > 1)
                    {
                        // Keep authored index if possible, but avoid port collisions by finding the nearest free slot.
                        int bestAlt = slotIndex;
                        int maxScan = Mathf.Max(1, _beltSlots.Count - 1);
                        for (int delta = 1; delta <= maxScan; delta++)
                        {
                            int a = (slotIndex + delta) % _beltSlots.Count;
                            int b = (slotIndex - delta + _beltSlots.Count) % _beltSlots.Count;
                            if (!reservedSlots.Contains(a)) { bestAlt = a; break; }
                            if (!reservedSlots.Contains(b)) { bestAlt = b; break; }
                        }
                        slotIndex = bestAlt;
                    }
                }
                _containerToBelt[i] = slotIndex;
                containerToBelt[i] = slotIndex;
                reservedSlots.Add(slotIndex);

                if (debugLogBoxPorts && _beltSlots != null && slotIndex >= 0 && slotIndex < _beltSlots.Count)
                {
                    var mouth = LayoutUtils.ComputeMouth(spec, spec.size);
                    var slotPos = _beltSlots[slotIndex] != null ? _beltSlots[slotIndex].position : Vector3.zero;
                    Debug.Log($"BoxPort[{i}] '{go.name}' opening={spec.opening} mouth=({mouth.x:F2},{mouth.y:F2}) -> slot={slotIndex} pos=({slotPos.x:F2},{slotPos.y:F2})");
                }
            }

            _game = new LoopSortingGame(_beltSlots.Count, containers, containerToBelt, _beltCapacity);
            _containerIndexByRef.Clear();
            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] != null) _containerIndexByRef[containers[i]] = i;
            }
            _rejectGate ??= new RejectFeedbackGate();
            _rejectGate.ResetForNewLevel();
            _beltBlockOffsets.Clear();
            _beltBlockOffsetCoroutines.Clear();
            _beltInsertAnimating.Clear();
            _beltInsertCoroutines.Clear();
            UpdateBeltCounter();
            UpdateLocks();
            UpdateCompletionStates();
        }

        private void SyncContainersVisuals()
        {
            if (_game == null)
            {
                return;
            }

            for (int i = 0; i < _game.Containers.Count && i < _boxViews.Count; i++)
            {
                bool locked = i < _boxLocked.Count && _boxLocked[i];
                var unlockColor = i < _boxSpecs.Count && _boxSpecs[i] != null ? _boxSpecs[i].unlockColor : BlockColor.Red;
                _boxViews[i].SetLocked(locked, unlockColor);
                // Locked boxes must hide all contents until unlocked.
                _boxViews[i].SyncBlocks(locked ? null : _game.Containers[i].Blocks);
            }
        }

        private void SyncBeltVisuals()
        {
            if (_game == null)
            {
                return;
            }

            // Remove visuals no longer needed.
            var toRemove = new List<int>();
            foreach (var kv in _beltBlockVisuals)
            {
                int idx = kv.Key;
                var slot = _game.Conveyor.GetSlot(idx);
                if (!slot.HasValue)
                {
                    if (_beltInsertAnimating.Contains(idx))
                    {
                        continue;
                    }
                    StopBeltSpawnAnimation(idx);
                    Destroy(kv.Value);
                    toRemove.Add(idx);
                }
            }
            foreach (var idx in toRemove)
            {
                _beltBlockVisuals.Remove(idx);
            }

            // Add/update visuals.
            for (int i = 0; i < _beltSlots.Count; i++)
            {
                var slot = _game.Conveyor.GetSlot(i);
                if (!slot.HasValue)
                {
                    continue;
                }

                EnsureBlockVisual(i, slot.Value);
            }
        }

        private void OnDrawGizmos()
        {
            if (!showSlotGizmos || _beltSlots == null) return;
            Gizmos.color = slotColor;
            foreach (var t in _beltSlots)
            {
                if (t == null) continue;
                Gizmos.DrawSphere(t.position, slotGizmoRadius);
            }
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

        private void UpdateCompletionStates()
        {
            if (_game == null) return;
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                bool completed = _game.Containers[i].IsUniformAndFull();
                if (i >= _boxCompleted.Count) _boxCompleted.Add(completed);
                else _boxCompleted[i] = completed;

                if (i < _boxViews.Count)
                {
                    _boxViews[i].SetCompleted(completed);
                }
            }
            UpdateFrontOutlines();
        }

        private void UpdateFrontOutlines()
        {
            if (_game == null) return;
            for (int i = 0; i < _game.Containers.Count && i < _boxViews.Count; i++)
            {
                var cont = _game.Containers[i];
                bool locked = i < _boxLocked.Count && _boxLocked[i];
                bool completed = i < _boxCompleted.Count && _boxCompleted[i];
                if (locked || completed || cont.Count == 0)
                {
                    _boxViews[i].HideFrontOutline();
                    continue;
                }

                // compute run length of front-most color
                int run = 0;
                var first = cont.Blocks[0].Color;
                for (int j = 0; j < cont.Count; j++)
                {
                    if (cont.Blocks[j].Color == first) run++; else break;
                }

                _boxViews[i].ShowFrontOutline(run, true);
            }
        }

        private void UpdateLocks()
        {
            if (_game == null)
            {
                return;
            }

            // capture completed colors
            var completedColors = new HashSet<BlockColor>();
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                var c = _game.Containers[i];
                if (c.IsUniformAndFull() && c.Blocks.Count > 0)
                {
                    completedColors.Add(c.Blocks[0].Color);
                }
            }

            for (int i = 0; i < _game.Containers.Count; i++)
            {
                bool currentlyLocked = i < _boxLocked.Count && _boxLocked[i];
                bool shouldUnlock = false;
                if (currentlyLocked && i < _boxSpecs.Count && _boxSpecs[i] != null)
                {
                    shouldUnlock = completedColors.Contains(_boxSpecs[i].unlockColor);
                }

                bool finalLocked = currentlyLocked && !shouldUnlock;
                if (i >= _boxLocked.Count)
                {
                    _boxLocked.Add(finalLocked);
                }
                else
                {
                    _boxLocked[i] = finalLocked;
                }

                _game.Containers[i].SetLocked(finalLocked);
                if (i < _boxViews.Count)
                {
                    var unlockColor = i < _boxSpecs.Count && _boxSpecs[i] != null ? _boxSpecs[i].unlockColor : BlockColor.Red;
                    _boxViews[i].SetLocked(finalLocked, unlockColor);
                }
            }

        }

        private void CheckEndConditions()
        {
            if (_gameOver || _game == null) return;

            bool win = true;
            for (int i = 0; i < _game.Containers.Count; i++)
            {
                var c = _game.Containers[i];
                if (c.Count == 0)
                {
                    continue; // empty box allowed
                }

                if (!c.IsUniformAndFull())
                {
                    win = false;
                    break;
                }
            }
            if (win && _game.Conveyor.BlockCount > 0)
            {
                win = false;
            }
            if (win)
            {
                BeginEndSequence(win: true, delaySeconds: WinEndSequenceDelaySeconds);
                return;
            }

            // Full-belt failure is handled by the fast-forward loop logic in Update().
            // If belt becomes full outside of a conveyor tick, arm the fast-forward state here.
            int limit = _beltCapacity > 0 ? Mathf.Min(_beltCapacity, _game.Conveyor.Length) : _game.Conveyor.Length;
            bool beltFull = _game.Conveyor.BlockCount >= limit;
            if (beltFull && !_fullBeltFastForward)
            {
                StartFullBeltFastForward();
            }
            else if (!beltFull && _fullBeltFastForward)
            {
                StopFullBeltFastForward();
            }
        }

        private bool CanAnyContainerAcceptAnyBeltBlock()
        {
            for (int i = 0; i < _game.Conveyor.Length; i++)
            {
                var slot = _game.Conveyor.GetSlot(i);
                if (!slot.HasValue) continue;
                var block = slot.Value;
                for (int c = 0; c < _game.Containers.Count; c++)
                {
                    if (_game.Containers[c].CanAccept(block))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void BeginEndSequence(bool win, float delaySeconds)
        {
            if (_endSequenceRoutine != null) return;

            // Freeze gameplay immediately so the state doesn't keep changing while we play the final feedback.
            _gameOver = true;
            _inputLocked = true;
            _isReleasing = false;
            StopFullBeltFastForward();

            _endSequenceRoutine = StartCoroutine(PlayEndSequenceThenShowResult(win, delaySeconds));
        }

        private IEnumerator PlayEndSequenceThenShowResult(bool win, float delaySeconds)
        {
            delaySeconds = Mathf.Max(0f, delaySeconds);
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            ShowResult(win);
            _endSequenceRoutine = null;
        }

        private void ShowResult(bool win)
        {
            _gameOver = true;
            PlaySfx(SfxId.UiPopupOpen);
            PlaySfx(win ? SfxId.LevelWin : SfxId.LevelLose);
            EnsureBgm();
            if (soundEnabled && _bgm != null)
            {
                _bgm.PlayStinger(win ? BgmStingerId.Win : BgmStingerId.Lose);
                _bgm.FadeOutLoops(fadeSeconds: 0.9f);
            }
            EnsureResultPanel();
            AnimateUiPanel(_resultPanel, true, seconds: 0.22f);
            _resultText.text = win ? "VICTORY" : "FAILED";
            if (_primaryLabel != null) _primaryLabel.text = win ? "NEXT" : "RETRY";
            if (_secondaryLabel != null) _secondaryLabel.text = win ? "RETRY" : "CLOSE";

            if (LoopSortingUIKit.IsAvailable())
            {
                if (_resultPrimaryIcon != null)
                {
                    _resultPrimaryIcon.sprite = LoopSortingUIKit.LoadSpriteByKey(win ? "ui.icon.next" : "ui.icon.retry");
                    _resultPrimaryIcon.color = Color.white;
                    _resultPrimaryIcon.gameObject.SetActive(_resultPrimaryIcon.sprite != null);
                }
                if (_resultSecondaryIcon != null)
                {
                    _resultSecondaryIcon.sprite = LoopSortingUIKit.LoadSpriteByKey(win ? "ui.icon.retry" : "ui.icon.close");
                    _resultSecondaryIcon.color = Color.white;
                    _resultSecondaryIcon.gameObject.SetActive(_resultSecondaryIcon.sprite != null);
                }
            }
        }

        private void OnPrimaryClicked()
        {
            PlaySfx(SfxId.UiConfirm);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (_flow != null && _flow.levels.Count > 0 && _primaryLabel != null && _primaryLabel.text == "NEXT")
            {
                int next = _flowIndex + 1;
                if (next < _flow.levels.Count)
                {
                    PlaySfx(SfxId.LevelNext);
                    _flowIndex = next;
                    _gameOver = false;
                    Build(_flow, _flowIndex);
                    return;
                }
            }
            PlaySfx(SfxId.LevelRetry);
            RestartCurrent();
        }

        private void OnSecondaryClicked()
        {
            bool isClose = _secondaryLabel != null && _secondaryLabel.text == "CLOSE";
            PlaySfx(isClose ? SfxId.UiCancel : SfxId.UiClick);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            if (isClose)
            {
                // "CLOSE" on lose returns to main menu (keeping the current level as pending selection).
                if (_flow != null && _flow.levels != null && _flow.levels.Count > 0)
                {
                    _pendingFlow = _flow;
                    _pendingFlowIndex = Mathf.Clamp(_flowIndex, 0, Mathf.Max(0, _flow.levels.Count - 1));
                    _pendingLevel = null;
                }
                else
                {
                    _pendingLevel = _currentLayout;
                    _pendingFlow = null;
                    _pendingFlowIndex = 0;
                }

                ShowMainMenu();
                return;
            }

            PlaySfx(SfxId.LevelRetry);
            RestartCurrent();
        }

        private void RestartCurrent()
        {
            _gameOver = false;
            if (_flow != null && _flow.levels.Count > 0)
            {
                Build(_flow, _flowIndex);
            }
            else
            {
                Build(_currentLayout);
            }
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

            // With X tilt and a mostly-flat (Z≈0) level, the effective vertical span in camera space is scaled by cos(tiltX).
            float heightInCamera = height * tiltCos;
            float orthoSize = Mathf.Max(heightInCamera * 0.5f, width * 0.5f / Mathf.Max(0.0001f, cam.aspect));

            bool hasLayout = LoopSortingUIKit.TryGetRuntimeLayout(out var uiLayout);
            float top = Mathf.Clamp01(hasLayout ? uiLayout.reservedTop : cameraReservedTop);
            float bottom = Mathf.Clamp01(hasLayout ? uiLayout.reservedBottom : cameraReservedBottom);
            float available = Mathf.Clamp01(1f - top - bottom);
            if (available < 0.35f) available = 0.35f;

            // Expand ortho size so the level fits into the remaining viewport area.
            orthoSize = orthoSize / available;
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

        private static IEnumerable<Block> BuildBlocksForSpec(BoxSpec spec, int capacity)
        {
            if (spec.colorCounts == null) yield break;

            var list = new List<Block>(capacity);
            int filled = 0;
            // colorCounts are authored outer->inner (index 0 is the outermost / mouth-facing layer).
            // Keep the same order at runtime so the editor preview and gameplay match.
            for (int idx = 0; idx < spec.colorCounts.Count && filled < capacity; idx++)
            {
                var cc = spec.colorCounts[idx];
                int cnt = Mathf.Max(0, cc.count);
                for (int i = 0; i < cnt && filled < capacity; i++)
                {
                    list.Add(new Block(cc.color, cc.hidden));
                    filled++;
                }
            }

            foreach (var b in list)
            {
                yield return b;
            }
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
                StartBeltSpawnFromBox(containerIndex, peek);

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
            container.SetBusy(false);
            if (containerIndex < _boxViews.Count)
            {
                _boxViews[containerIndex].HideFrontOutline();
            }
            UpdateLocks();
            CheckEndConditions();
        }

        private void EnsureCounterUI()
        {
            if (_uiCanvas != null && beltCounterUI != null && _speedButton != null && _resultPanel != null && _settingsButton != null && _boosterPanel != null && _boosterFillButton != null && _boosterShuffleButton != null)
            {
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
                _hudRootRect = null;
                _lockChipLayer = null;
                _lockChipByBox.Clear();
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
            _boosterFillButton = null;
            _boosterShuffleButton = null;
            _fastTag = null;
            _fastTagBg = null;
            _fastTagText = null;

            var canvasGO = new GameObject("HUDCanvas");
            _uiCanvas = canvasGO.AddComponent<Canvas>();
            _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _uiCanvas.overrideSorting = true;
            _uiCanvas.sortingOrder = 0;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            // Root helper
            var root = new GameObject("HUDRoot");
            root.transform.SetParent(canvasGO.transform, false);
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            _hudRootRect = rootRect;

            bool hasKit = LoopSortingUIKit.IsAvailable();
            var uiLayout = LoopSortingUIKit.GetRuntimeLayout();

            EnsureEconomyDefaults();

            void PlaceTopLeft(RectTransform r, Rect topLeft)
            {
                r.anchorMin = new Vector2(0f, 1f);
                r.anchorMax = new Vector2(0f, 1f);
                r.pivot = new Vector2(0f, 1f);
                r.anchoredPosition = new Vector2(topLeft.x, -topLeft.y);
                r.sizeDelta = new Vector2(topLeft.width, topLeft.height);
            }

            void PlaceTopRight(RectTransform r, Rect topLeft, float refW)
            {
                float right = refW - (topLeft.x + topLeft.width);
                r.anchorMin = new Vector2(1f, 1f);
                r.anchorMax = new Vector2(1f, 1f);
                r.pivot = new Vector2(1f, 1f);
                r.anchoredPosition = new Vector2(-right, -topLeft.y);
                r.sizeDelta = new Vector2(topLeft.width, topLeft.height);
            }

            void PlaceTopCenter(RectTransform r, Rect topLeft, float refW)
            {
                float centerX = (topLeft.x + topLeft.width * 0.5f) - refW * 0.5f;
                r.anchorMin = new Vector2(0.5f, 1f);
                r.anchorMax = new Vector2(0.5f, 1f);
                r.pivot = new Vector2(0.5f, 1f);
                r.anchoredPosition = new Vector2(centerX, -topLeft.y);
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
                counterBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                counterBg.type = counterBg.sprite != null && counterBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                counterBg.color = Color.white;
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
            var levelTextRect = _levelHudText.GetComponent<RectTransform>();
            levelTextRect.anchorMin = Vector2.zero;
            levelTextRect.anchorMax = Vector2.one;
            levelTextRect.offsetMin = new Vector2(16f, 0f);
            levelTextRect.offsetMax = new Vector2(-16f, 0f);

            // Shop button (top-left under pause area, uses placeholder icon if missing)
            _shopButton = CreateIconButton(
                parent: root.transform,
                name: "ShopButton",
                anchor: new Vector2(0f, 1f),
                anchoredPos: new Vector2(uiLayout.shop.x + uiLayout.shop.width * 0.5f, -uiLayout.shop.y - uiLayout.shop.height * 0.5f),
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

            // Coins + lives (top-right): value pill + "+" that opens shop.
            CreateCurrencyPill(
                parent: root.transform,
                name: "CoinsPill",
                anchor: new Vector2(1f, 1f),
                anchoredPos: new Vector2(-(uiLayout.referenceWidth - (uiLayout.coins.x + uiLayout.coins.width)), -uiLayout.coins.y),
                size: new Vector2(uiLayout.coins.width, uiLayout.coins.height),
                iconKey: hasKit ? "ui.icon.coin" : null,
                out _coinText,
                out _coinPlusButton);
            _coinPlusButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiClick);
                OpenShop(ShopTab.Coins);
            });

            CreateCurrencyPill(
                parent: root.transform,
                name: "LivesPill",
                anchor: new Vector2(1f, 1f),
                anchoredPos: new Vector2(-(uiLayout.referenceWidth - (uiLayout.lives.x + uiLayout.lives.width)), -uiLayout.lives.y),
                size: new Vector2(uiLayout.lives.width, uiLayout.lives.height),
                iconKey: hasKit ? "ui.icon.heart" : null,
                out _lifeText,
                out _lifePlusButton);
            _lifePlusButton.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiClick);
                OpenShop(ShopTab.Lives);
            });

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
            PlaceTopRight(speedRect, uiLayout.speed, uiLayout.referenceWidth);

            var speedLabelGO = new GameObject("Label");
            speedLabelGO.transform.SetParent(speedGO.transform, false);
            _speedButtonLabel = speedLabelGO.AddComponent<TextMeshProUGUI>();
            _speedButtonLabel.raycastTarget = false;
            _speedButtonLabel.alignment = TextAlignmentOptions.Center;
            _speedButtonLabel.fontSize = 54;
            _speedButtonLabel.color = Color.white;
            var speedLabelRect = _speedButtonLabel.GetComponent<RectTransform>();
            speedLabelRect.anchorMin = Vector2.zero;
            speedLabelRect.anchorMax = Vector2.one;
            speedLabelRect.offsetMin = Vector2.zero;
            speedLabelRect.offsetMax = Vector2.zero;

            _speedButton.onClick.AddListener(CycleSpeed);
            UpdateSpeedButtonLabel();

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
            PlaceTopRight(settingsRect, uiLayout.settings, uiLayout.referenceWidth);

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

            _settingsButton.onClick.AddListener(() => ToggleSettingsPanel(true));

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
            fastBgRect.anchoredPosition = new Vector2(0f, -160f);
            fastBgRect.sizeDelta = new Vector2(330f, 78f);

            var fastTextGO = new GameObject("Text");
            fastTextGO.transform.SetParent(_fastTag.transform, false);
            _fastTagText = fastTextGO.AddComponent<TextMeshProUGUI>();
            _fastTagText.raycastTarget = false;
            _fastTagText.text = "FAST x5";
            _fastTagText.alignment = TextAlignmentOptions.Center;
            _fastTagText.fontSize = 44;
            _fastTagText.color = Color.white;
            var fastTextRect = _fastTagText.GetComponent<RectTransform>();
            fastTextRect.anchorMin = new Vector2(0.5f, 1f);
            fastTextRect.anchorMax = new Vector2(0.5f, 1f);
            fastTextRect.pivot = new Vector2(0.5f, 1f);
            fastTextRect.anchoredPosition = new Vector2(0f, -160f);
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

            _boosterFillButton = CreateBoosterButton(
                _boosterPanel.transform,
                name: "BoosterFill",
                anchor: uiLayout.boosterAnchor,
                anchoredPos: new Vector2(-uiLayout.boosterOffset.x, uiLayout.boosterOffset.y),
                size: uiLayout.boosterSize,
                normal: hasKit ? "ui.button.mint_square.normal" : null,
                pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                disabled: hasKit ? "ui.button.mint_square.disabled" : null,
                icon: hasKit ? "ui.icon.fill" : null,
                label: "FILL");
            _boosterFillButton.onClick.AddListener(() => StartCoroutine(BoosterFillSequence()));
            if (hasKit) AttachBoosterBadge(_boosterFillButton.transform, InitialBoosterCount);

            _boosterShuffleButton = CreateBoosterButton(
                _boosterPanel.transform,
                name: "BoosterShuffle",
                anchor: uiLayout.boosterAnchor,
                anchoredPos: new Vector2(uiLayout.boosterOffset.x, uiLayout.boosterOffset.y),
                size: uiLayout.boosterSize,
                normal: hasKit ? "ui.button.purple_square.normal" : null,
                pressed: hasKit ? "ui.button.purple_square.pressed" : null,
                disabled: hasKit ? "ui.button.purple_square.disabled" : null,
                icon: hasKit ? "ui.icon.shuffle" : null,
                label: "SHUFFLE");
            _boosterShuffleButton.onClick.AddListener(() => StartCoroutine(BoosterShuffleSequence()));
            if (hasKit) AttachBoosterBadge(_boosterShuffleButton.transform, InitialBoosterCount);

            EnsureResultPanel();
            EnsureShopUI();
        }

        private void EnsureResultPanel()
        {
            if (_uiCanvas == null) return;
            if (_resultPanel != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            var panelGO = new GameObject("ResultPanel");
            panelGO.transform.SetParent(_uiCanvas.transform, false);
            _resultPanel = panelGO;

            var dim = panelGO.AddComponent<Image>();
            dim.raycastTarget = true;
            if (hasKit)
            {
                dim.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.overlay_dim");
                dim.color = Color.white;
            }
            else
            {
                dim.color = new Color(0f, 0f, 0f, 0.6f);
            }
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var boxGO = new GameObject("Panel");
            boxGO.transform.SetParent(panelGO.transform, false);
            var boxRect = boxGO.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = new Vector2(0f, 120f);
            boxRect.sizeDelta = new Vector2(900f, 760f);

            var boxImg = boxGO.AddComponent<Image>();
            boxImg.raycastTarget = false;
            if (hasKit)
            {
                boxImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_result");
                boxImg.type = boxImg.sprite != null && boxImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                boxImg.color = Color.white;
            }
            else
            {
                boxImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            }

            var bannerGO = new GameObject("Banner");
            bannerGO.transform.SetParent(boxGO.transform, false);
            var bannerImg = bannerGO.AddComponent<Image>();
            bannerImg.raycastTarget = false;
            if (hasKit)
            {
                bannerImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info");
                bannerImg.type = bannerImg.sprite != null && bannerImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                bannerImg.color = Color.white;
            }
            var bannerRect = bannerGO.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 1f);
            bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0f, -80f);
            bannerRect.sizeDelta = new Vector2(620f, 96f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(boxGO.transform, false);
            _resultText = titleGO.AddComponent<TextMeshProUGUI>();
            _resultText.raycastTarget = false;
            _resultText.alignment = TextAlignmentOptions.Center;
            _resultText.fontSize = 62;
            _resultText.color = Color.white;
            var titleRect = _resultText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -84f);
            titleRect.sizeDelta = new Vector2(600f, 90f);

            _primaryButton = CreateLongButton(
                parent: boxGO.transform,
                name: "PrimaryButton",
                anchor: new Vector2(0.5f, 0.48f),
                size: new Vector2(820f, 200f),
                normal: hasKit ? "ui.button.mint_long.normal" : null,
                pressed: hasKit ? "ui.button.mint_long.pressed" : null,
                disabled: hasKit ? "ui.button.mint_long.disabled" : null,
                label: "NEXT",
                out _primaryLabel);
            _primaryButton.onClick.AddListener(OnPrimaryClicked);

            _resultPrimaryIcon = CreateButtonIcon(_primaryButton.transform);

            _secondaryButton = CreateLongButton(
                parent: boxGO.transform,
                name: "SecondaryButton",
                anchor: new Vector2(0.5f, 0.23f),
                size: new Vector2(820f, 200f),
                normal: hasKit ? "ui.button.orange_long.normal" : null,
                pressed: hasKit ? "ui.button.orange_long.pressed" : null,
                disabled: hasKit ? "ui.button.orange_long.disabled" : null,
                label: "RETRY",
                out _secondaryLabel);
            _secondaryButton.onClick.AddListener(OnSecondaryClicked);

            _resultSecondaryIcon = CreateButtonIcon(_secondaryButton.transform);

            _resultPanel.SetActive(false);
        }

        private void EnsureSettingsUI()
        {
            if (_uiCanvas == null) return;
            if (_settingsPanel != null && _vibrationToggle != null && _soundToggle != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            _settingsPanel = new GameObject("SettingsPanel");
            _settingsPanel.transform.SetParent(_uiCanvas.transform, false);

            var dim = _settingsPanel.AddComponent<Image>();
            dim.raycastTarget = true;
            if (hasKit)
            {
                dim.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.overlay_dim");
                dim.color = Color.white;
            }
            else
            {
                dim.color = new Color(0f, 0f, 0f, 0.4f);
            }
            var overlayRect = _settingsPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(_settingsPanel.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.52f);
            panelRect.anchorMax = new Vector2(0.5f, 0.52f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(960f, 820f);

            var panelImg = panelGO.AddComponent<Image>();
            panelImg.raycastTarget = false;
            if (hasKit)
            {
                panelImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_thick");
                if (panelImg.sprite == null) panelImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                panelImg.type = panelImg.sprite != null && panelImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                panelImg.color = Color.white;
            }
            else
            {
                panelImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            }

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.raycastTarget = false;
            titleText.text = "SETTINGS";
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontSize = 68;
            titleText.color = Color.white;
            var titleRect = titleText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -70f);
            titleRect.sizeDelta = new Vector2(700f, 120f);

            var closeBtn = CreateIconButton(
                parent: panelGO.transform,
                name: "CloseButton",
                anchor: new Vector2(1f, 1f),
                anchoredPos: new Vector2(-96f, -96f),
                size: new Vector2(160f, 160f),
                normal: hasKit ? "ui.button.close_red.normal" : null,
                pressed: hasKit ? "ui.button.close_red.pressed" : null,
                disabled: hasKit ? "ui.button.close_red.disabled" : null,
                icon: null);
            closeBtn.onClick.AddListener(() => ToggleSettingsPanel(false));

            _soundToggle = CreateToggleRow(
                parent: panelGO.transform,
                name: "RowSound",
                anchor: new Vector2(0.5f, 0.5f),
                anchoredPos: new Vector2(0f, 140f),
                label: "SOUND",
                initial: soundEnabled,
                out var soundTrack,
                out var soundKnob);
            _soundToggle.onValueChanged.AddListener(val =>
            {
                soundEnabled = val;
                EnsureSfx();
                EnsureBgm();
                if (val && _bgm != null)
                {
                    bool inMenu = _mainMenuCanvas != null && _mainMenuCanvas.gameObject.activeSelf;
                    _bgmPressure = false;
                    _bgm.PlayLoop(inMenu ? BgmLoopId.Menu : BgmLoopId.GameplayBase, fadeSeconds: 0.6f);
                }
                UpdateToggleVisual(soundTrack, soundKnob, val);
            });
            UpdateToggleVisual(soundTrack, soundKnob, soundEnabled);

            _vibrationToggle = CreateToggleRow(
                parent: panelGO.transform,
                name: "RowVibration",
                anchor: new Vector2(0.5f, 0.5f),
                anchoredPos: new Vector2(0f, 10f),
                label: "VIBRATION",
                initial: vibrationEnabled,
                out var vibTrack,
                out var vibKnob);
            _vibrationToggle.onValueChanged.AddListener(val =>
            {
                vibrationEnabled = val;
                if (_haptics != null || val)
                {
                    EnsureHaptics();
                    if (val)
                    {
                        _haptics.Play(HapticsId.UiConfirm);
                    }
                }
                UpdateToggleVisual(vibTrack, vibKnob, val);
            });
            UpdateToggleVisual(vibTrack, vibKnob, vibrationEnabled);

            Button CreateSettingsActionButton(string name, Vector2 anchoredPos, string normalKey, string pressedKey, string disabledKey, string label)
            {
                var go = new GameObject(name);
                go.transform.SetParent(panelGO.transform, false);
                var rect = go.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPos;
                rect.sizeDelta = new Vector2(380f, 180f);

                var img = go.AddComponent<Image>();
                var btn = go.AddComponent<Button>();
                ApplyUIKitButtonSprites(btn, img, hasKit ? normalKey : null, hasKit ? pressedKey : null, hasKit ? disabledKey : null);

                var txtGO = new GameObject("Label");
                txtGO.transform.SetParent(go.transform, false);
                var txt = txtGO.AddComponent<TextMeshProUGUI>();
                txt.raycastTarget = false;
                txt.text = label;
                txt.alignment = TextAlignmentOptions.Center;
                txt.fontSize = 48;
                txt.color = Color.white;
                var tRect = txt.GetComponent<RectTransform>();
                tRect.anchorMin = Vector2.zero;
                tRect.anchorMax = Vector2.one;
                tRect.offsetMin = Vector2.zero;
                tRect.offsetMax = Vector2.zero;

                return btn;
            }

            // Bottom actions (placeholders; wired to real behaviors where available).
            var restoreBtn = CreateSettingsActionButton(
                "RestorePurchases",
                anchoredPos: new Vector2(-210f, -210f),
                normalKey: "ui.button.small_blue.normal",
                pressedKey: "ui.button.small_blue.pressed",
                disabledKey: "ui.button.small_blue.disabled",
                label: "RESTORE");
            restoreBtn.onClick.AddListener(() => Debug.Log("Restore Purchases (placeholder)"));

            var supportBtn = CreateSettingsActionButton(
                "Support",
                anchoredPos: new Vector2(210f, -210f),
                normalKey: "ui.button.small_green.normal",
                pressedKey: "ui.button.small_green.pressed",
                disabledKey: "ui.button.small_green.disabled",
                label: "SUPPORT");
            supportBtn.onClick.AddListener(() => Debug.Log("Support (placeholder)"));

            var retryBtn = CreateSettingsActionButton(
                "RetryLevel",
                anchoredPos: new Vector2(0f, -390f),
                normalKey: "ui.button.small_red.normal",
                pressedKey: "ui.button.small_red.pressed",
                disabledKey: "ui.button.small_red.disabled",
                label: "RETRY");
            retryBtn.onClick.AddListener(() =>
            {
                ToggleSettingsPanel(false);
                RestartCurrent();
            });

            _settingsPanel.SetActive(false);
        }

        private enum ShopTab
        {
            Coins,
            Lives
        }

        private void EnsureEconomyDefaults()
        {
            if (_coins <= 0) _coins = InitialCoins;
            if (_lives <= 0) _lives = InitialLives;
        }

        private void RefreshEconomyHUD()
        {
            if (_coinText != null) _coinText.text = FormatCurrencyValue(_coins);
            if (_lifeText != null) _lifeText.text = _lives.ToString();
            if (_shopCoinValue != null) _shopCoinValue.text = _coins.ToString();
            if (_shopLifeValue != null) _shopLifeValue.text = _lives.ToString();
        }

        private static string FormatCurrencyValue(int value)
        {
            if (value < 0) return value.ToString();
            if (value <= 9_999_999) return value.ToString(); // up to 7 digits: keep full for readability

            // Compact notation for large values to keep text inside pills (K/M/B).
            if (value < 1_000_000_000)
            {
                return FormatCompact(value, 1_000_000, "M", decimals: value < 100_000_000 ? 1 : 0);
            }
            return FormatCompact(value, 1_000_000_000, "B", decimals: 1);
        }

        private static string FormatCompact(int value, int unit, string suffix, int decimals)
        {
            float scaled = value / (float)unit;
            decimals = Mathf.Clamp(decimals, 0, 2);
            string fmt = decimals == 0 ? "0" : (decimals == 1 ? "0.#" : "0.##");
            return scaled.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture) + suffix;
        }

        private void OpenShop(ShopTab tab)
        {
            EnsureShopUI();
            RefreshEconomyHUD();
            PopulateShop(tab);
            if (_shopPanel != null) AnimateUiPanel(_shopPanel, true, seconds: 0.20f);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            PlaySfx(SfxId.UiPopupOpen);
        }

        private void EnsureShopUI()
        {
            if (_uiCanvas == null) return;
            if (_shopPanel != null && _shopContentRoot != null && _shopScroll != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            _shopPanel = new GameObject("ShopPanel");
            _shopPanel.transform.SetParent(_uiCanvas.transform, false);

            var dim = _shopPanel.AddComponent<Image>();
            dim.raycastTarget = true;
            if (hasKit)
            {
                dim.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.overlay_dim");
                dim.color = Color.white;
            }
            else
            {
                dim.color = new Color(0f, 0f, 0f, 0.55f);
            }
            var overlayRect = _shopPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(_shopPanel.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = new Vector2(0f, 40f);
            panelRect.sizeDelta = new Vector2(980f, 1200f);

            var panelImg = panelGO.AddComponent<Image>();
            panelImg.raycastTarget = false;
            if (hasKit)
            {
                panelImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_shop");
                if (panelImg.sprite == null) panelImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                panelImg.type = panelImg.sprite != null && panelImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                panelImg.color = Color.white;
            }
            else
            {
                panelImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            }

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(panelGO.transform, false);
            _shopTitle = titleGO.AddComponent<TextMeshProUGUI>();
            _shopTitle.raycastTarget = false;
            _shopTitle.text = "SHOP";
            _shopTitle.alignment = TextAlignmentOptions.Center;
            _shopTitle.fontSize = 70;
            _shopTitle.color = Color.white;
            var titleRect = _shopTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -50f);
            titleRect.sizeDelta = new Vector2(700f, 100f);

            var closeBtn = CreateIconButton(
                parent: panelGO.transform,
                name: "CloseButton",
                anchor: new Vector2(1f, 1f),
                anchoredPos: new Vector2(-96f, -96f),
                size: new Vector2(160f, 160f),
                normal: hasKit ? "ui.button.close_red.normal" : null,
                pressed: hasKit ? "ui.button.close_red.pressed" : null,
                disabled: hasKit ? "ui.button.close_red.disabled" : null,
                icon: null);
            closeBtn.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiPopupClose);
                AnimateUiPanel(_shopPanel, false, seconds: 0.18f);
            });

            // Currency strip
            var coinsStrip = CreateCurrencyStrip(panelGO.transform, "CoinsStrip", new Vector2(-220f, -150f), hasKit ? "ui.icon.coin" : null, out _shopCoinValue);
            var livesStrip = CreateCurrencyStrip(panelGO.transform, "LivesStrip", new Vector2(220f, -150f), hasKit ? "ui.icon.heart" : null, out _shopLifeValue);

            // Scroll list (v04_3 spec): ScrollRect -> Viewport (RectMask2D) -> Content (VerticalLayoutGroup + ContentSizeFitter)
            var scrollGO = new GameObject("ShopScrollList");
            scrollGO.transform.SetParent(panelGO.transform, false);
            var scrollRect = scrollGO.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0.5f, 0f);
            scrollRect.anchorMax = new Vector2(0.5f, 0f);
            scrollRect.pivot = new Vector2(0.5f, 0f);
            scrollRect.anchoredPosition = new Vector2(0f, 80f);
            scrollRect.sizeDelta = new Vector2(920f, 880f);

            _shopScroll = scrollGO.AddComponent<ScrollRect>();
            _shopScroll.horizontal = false;
            _shopScroll.vertical = true;
            _shopScroll.movementType = ScrollRect.MovementType.Elastic;
            _shopScroll.inertia = true;
            _shopScroll.decelerationRate = 0.135f;
            _shopScroll.scrollSensitivity = 25f;

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImg = viewportGO.AddComponent<Image>();
            viewportImg.raycastTarget = true;
            viewportImg.color = new Color(1f, 1f, 1f, 0.001f);
            viewportGO.AddComponent<RectMask2D>();
            _shopScroll.viewport = viewportRect;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            _shopContentRoot = contentGO.AddComponent<RectTransform>();
            _shopContentRoot.anchorMin = new Vector2(0f, 1f);
            _shopContentRoot.anchorMax = new Vector2(1f, 1f);
            _shopContentRoot.pivot = new Vector2(0.5f, 1f);
            _shopContentRoot.anchoredPosition = Vector2.zero;
            _shopContentRoot.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 28f;
            layout.padding = new RectOffset(0, 0, 24, 60);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            var fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _shopScroll.content = _shopContentRoot;

            // Optional scroll fades (visual only).
            if (hasKit)
            {
                var fadeTopGO = new GameObject("FadeTop");
                fadeTopGO.transform.SetParent(scrollGO.transform, false);
                _shopScrollFadeTop = fadeTopGO.AddComponent<Image>();
                _shopScrollFadeTop.raycastTarget = false;
                _shopScrollFadeTop.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_top.png", pixelsPerUnit: 100f, applyNineSlice: false);
                _shopScrollFadeTop.color = Color.white;
                var fadeTopRect = fadeTopGO.GetComponent<RectTransform>();
                fadeTopRect.anchorMin = new Vector2(0f, 1f);
                fadeTopRect.anchorMax = new Vector2(1f, 1f);
                fadeTopRect.pivot = new Vector2(0.5f, 1f);
                fadeTopRect.anchoredPosition = Vector2.zero;
                fadeTopRect.sizeDelta = new Vector2(0f, 140f);

                var fadeBottomGO = new GameObject("FadeBottom");
                fadeBottomGO.transform.SetParent(scrollGO.transform, false);
                _shopScrollFadeBottom = fadeBottomGO.AddComponent<Image>();
                _shopScrollFadeBottom.raycastTarget = false;
                _shopScrollFadeBottom.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_bottom.png", pixelsPerUnit: 100f, applyNineSlice: false);
                _shopScrollFadeBottom.color = Color.white;
                var fadeBottomRect = fadeBottomGO.GetComponent<RectTransform>();
                fadeBottomRect.anchorMin = new Vector2(0f, 0f);
                fadeBottomRect.anchorMax = new Vector2(1f, 0f);
                fadeBottomRect.pivot = new Vector2(0.5f, 0f);
                fadeBottomRect.anchoredPosition = Vector2.zero;
                fadeBottomRect.sizeDelta = new Vector2(0f, 160f);
            }

            _shopPanel.SetActive(false);
        }

        private RectTransform CreateCurrencyStrip(Transform parent, string name, Vector2 anchoredPos, string iconKey, out TMP_Text valueText)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(420f, 120f);

            var bg = go.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                bg.type = bg.sprite != null && bg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.35f);
            }

            if (!string.IsNullOrEmpty(iconKey) && hasKit)
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(go.transform, false);
                var icon = iconGO.AddComponent<Image>();
                icon.raycastTarget = false;
                icon.sprite = LoopSortingUIKit.LoadSpriteByKey(iconKey);
                icon.color = Color.white;
                var iRect = iconGO.GetComponent<RectTransform>();
                iRect.anchorMin = new Vector2(0f, 0.5f);
                iRect.anchorMax = new Vector2(0f, 0.5f);
                iRect.pivot = new Vector2(0f, 0.5f);
                iRect.anchoredPosition = new Vector2(24f, 0f);
                iRect.sizeDelta = new Vector2(90f, 90f);
            }

            var txtGO = new GameObject("Value");
            txtGO.transform.SetParent(go.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "0";
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 56;
            tmp.color = Color.white;
            var tRect = txtGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 0.5f);
            tRect.anchorMax = new Vector2(0.5f, 0.5f);
            tRect.pivot = new Vector2(0.5f, 0.5f);
            tRect.anchoredPosition = new Vector2(40f, 0f);
            tRect.sizeDelta = new Vector2(260f, 90f);

            valueText = tmp;
            return rect;
        }

        private void PopulateShop(ShopTab tab)
        {
            if (_shopContentRoot == null) return;

            for (int i = _shopContentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_shopContentRoot.GetChild(i).gameObject);
            }

            if (_shopTitle != null) _shopTitle.text = tab == ShopTab.Coins ? "SHOP" : "MORE LIVES";

            if (tab == ShopTab.Coins)
            {
                AddShopSectionHeader(_shopContentRoot, "COINS");
                AddShopCoinPackRow(_shopContentRoot, "Coins_1000", "1000 COINS", "+1000", () => { _coins += 1000; RefreshEconomyHUD(); PlaySfx(SfxId.UiConfirm); });
                AddShopCoinPackRow(_shopContentRoot, "Coins_5000", "5000 COINS", "+5000", () => { _coins += 5000; RefreshEconomyHUD(); PlaySfx(SfxId.UiConfirm); });
                AddShopCoinPackRow(_shopContentRoot, "Coins_10000", "10000 COINS", "+10000", () => { _coins += 10000; RefreshEconomyHUD(); PlaySfx(SfxId.UiConfirm); });
            }
            else
            {
                AddShopSectionHeader(_shopContentRoot, "LIVES");
                AddShopItem(_shopContentRoot, "Lives_1", "GET +1 LIFE", "+1", () => { _lives += 1; RefreshEconomyHUD(); PlaySfx(SfxId.UiConfirm); });
                AddShopItem(_shopContentRoot, "Lives_5", "REFILL 5 LIVES", "+5", () => { _lives = Mathf.Max(_lives, 5); RefreshEconomyHUD(); PlaySfx(SfxId.UiConfirm); });
            }

            if (_shopScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                _shopScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void AddShopItem(RectTransform parent, string name, string title, string rightLabel, Action onClick)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var itemGO = new GameObject(name);
            itemGO.transform.SetParent(parent, false);
            var rect = itemGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 260f);

            var le = itemGO.AddComponent<LayoutElement>();
            le.preferredHeight = 260f;

            var img = itemGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (hasKit)
            {
                // Prefer card background for lives; coins are rendered as rows by AddShopCoinPackRow().
                img.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.shop.item_bg");
                if (img.sprite == null) img.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_card_beige.png");
                if (img.sprite == null) img.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }

            var btn = itemGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(itemGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.raycastTarget = false;
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0f, 0.5f);
            tRect.pivot = new Vector2(0f, 0.5f);
            tRect.anchoredPosition = new Vector2(70f, 0f);
            tRect.sizeDelta = new Vector2(560f, 120f);

            // Price button background (visual only; the whole card is clickable).
            if (hasKit)
            {
                var priceBgGO = new GameObject("PriceBG");
                priceBgGO.transform.SetParent(itemGO.transform, false);
                var priceImg = priceBgGO.AddComponent<Image>();
                priceImg.raycastTarget = false;
                priceImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal");
                priceImg.type = priceImg.sprite != null && priceImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                priceImg.color = Color.white;
                var pRect = priceBgGO.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(1f, 0.5f);
                pRect.anchorMax = new Vector2(1f, 0.5f);
                pRect.pivot = new Vector2(1f, 0.5f);
                pRect.anchoredPosition = new Vector2(-60f, 0f);
                pRect.sizeDelta = new Vector2(240f, 120f);
            }

            var rightGO = new GameObject("Right");
            rightGO.transform.SetParent(itemGO.transform, false);
            var rightText = rightGO.AddComponent<TextMeshProUGUI>();
            rightText.raycastTarget = false;
            rightText.text = rightLabel;
            rightText.alignment = TextAlignmentOptions.Center;
            rightText.fontSize = 52;
            rightText.color = Color.white;
            var rRect = rightGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1f, 0.5f);
            rRect.anchorMax = new Vector2(1f, 0.5f);
            rRect.pivot = new Vector2(1f, 0.5f);
            rRect.anchoredPosition = new Vector2(-70f, 0f);
            rRect.sizeDelta = new Vector2(220f, 120f);
        }

        private void AddShopSectionHeader(RectTransform parent, string title)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject($"Section_{title}");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 96f);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 96f;

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            if (hasKit)
            {
                img.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_group_bar.png");
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0f, 0f, 0f, 0.35f);
            }

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = title;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 48;
            tmp.color = Color.white;
            var tRect = tmp.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
        }

        private void AddShopCoinPackRow(RectTransform parent, string name, string title, string rightLabel, Action onClick)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var itemGO = new GameObject(name);
            itemGO.transform.SetParent(parent, false);
            var rect = itemGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 200f);

            var le = itemGO.AddComponent<LayoutElement>();
            le.preferredHeight = 200f;

            var img = itemGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (hasKit)
            {
                img.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_row_yellow.png");
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }

            var btn = itemGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(itemGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.raycastTarget = false;
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0f, 0.5f);
            tRect.pivot = new Vector2(0f, 0.5f);
            tRect.anchoredPosition = new Vector2(70f, 0f);
            tRect.sizeDelta = new Vector2(560f, 120f);

            // Price background (visual only; row click triggers action).
            if (hasKit)
            {
                var priceBgGO = new GameObject("PriceBG");
                priceBgGO.transform.SetParent(itemGO.transform, false);
                var priceImg = priceBgGO.AddComponent<Image>();
                priceImg.raycastTarget = false;
                priceImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal");
                priceImg.type = priceImg.sprite != null && priceImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                priceImg.color = Color.white;
                var pRect = priceBgGO.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(1f, 0.5f);
                pRect.anchorMax = new Vector2(1f, 0.5f);
                pRect.pivot = new Vector2(1f, 0.5f);
                pRect.anchoredPosition = new Vector2(-60f, 0f);
                pRect.sizeDelta = new Vector2(240f, 120f);
            }

            var rightGO = new GameObject("Right");
            rightGO.transform.SetParent(itemGO.transform, false);
            var rightText = rightGO.AddComponent<TextMeshProUGUI>();
            rightText.raycastTarget = false;
            rightText.text = rightLabel;
            rightText.alignment = TextAlignmentOptions.Center;
            rightText.fontSize = 52;
            rightText.color = Color.white;
            var rRect = rightGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1f, 0.5f);
            rRect.anchorMax = new Vector2(1f, 0.5f);
            rRect.pivot = new Vector2(1f, 0.5f);
            rRect.anchoredPosition = new Vector2(-70f, 0f);
            rRect.sizeDelta = new Vector2(220f, 120f);
        }

        private void CreateCurrencyPill(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            string iconKey,
            out TMP_Text valueText,
            out Button plusButton)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(Mathf.Clamp01(anchor.x), Mathf.Clamp01(anchor.y));
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            float height = Mathf.Max(1f, size.y);
            float padding = Mathf.Clamp(height * 0.12f, 8f, 18f);
            float iconSize = Mathf.Clamp(height - padding * 2f, 18f, height);
            float plusSize = Mathf.Clamp(height - padding * 2f, 18f, height);

            var bg = root.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                bg.type = bg.sprite != null && bg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.35f);
            }

            // Icon
            float leftInset = padding;
            if (!string.IsNullOrEmpty(iconKey) && hasKit)
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(root.transform, false);
                var icon = iconGO.AddComponent<Image>();
                icon.raycastTarget = false;
                icon.sprite = LoopSortingUIKit.LoadSpriteByKey(iconKey);
                icon.color = Color.white;
                var iRect = iconGO.GetComponent<RectTransform>();
                iRect.anchorMin = new Vector2(0f, 0.5f);
                iRect.anchorMax = new Vector2(0f, 0.5f);
                iRect.pivot = new Vector2(0f, 0.5f);
                iRect.anchoredPosition = new Vector2(padding, 0f);
                iRect.sizeDelta = new Vector2(iconSize, iconSize);
                leftInset = padding + iconSize + padding;
            }

            // Value
            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(root.transform, false);
            var tmp = valueGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "0";
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            float maxSize = Mathf.Clamp(height * 0.72f, 34f, 56f);
            tmp.fontSize = maxSize;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = maxSize;
            tmp.fontSizeMin = Mathf.Clamp(maxSize * 0.55f, 20f, maxSize);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.color = Color.white;
            var vRect = tmp.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0f, 0f);
            vRect.anchorMax = new Vector2(1f, 1f);
            float rightInset = padding + plusSize + padding;
            vRect.offsetMin = new Vector2(leftInset, 0f);
            vRect.offsetMax = new Vector2(-rightInset, 0f);

            // Plus button
            plusButton = CreateIconButton(
                parent: root.transform,
                name: "Plus",
                anchor: new Vector2(1f, 0.5f),
                anchoredPos: new Vector2(-padding - plusSize * 0.5f, 0f),
                size: new Vector2(plusSize, plusSize),
                normal: hasKit ? "ui.button.mint_square.normal" : null,
                pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                disabled: hasKit ? "ui.button.mint_square.disabled" : null,
                icon: hasKit ? "ui.icon.plus" : null);

            valueText = tmp;
        }

        private void ApplyUIKitButtonSprites(Button button, Image image, string normal, string pressed, string disabled)
        {
            if (button == null || image == null) return;

            // Fallback: keep previous theme behavior if UI kit isn't available.
            if (string.IsNullOrEmpty(normal))
            {
                image.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
                if (uiTheme != null && uiTheme.buttonSprite != null) image.sprite = uiTheme.buttonSprite;
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                return;
            }

            var normalSprite = LoopSortingUIKit.LoadSpriteByKey(normal);
            var pressedSprite = !string.IsNullOrEmpty(pressed) ? LoopSortingUIKit.LoadSpriteByKey(pressed) : null;
            var disabledSprite = !string.IsNullOrEmpty(disabled) ? LoopSortingUIKit.LoadSpriteByKey(disabled) : null;
            if (normalSprite == null)
            {
                image.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
                if (uiTheme != null && uiTheme.buttonSprite != null) image.sprite = uiTheme.buttonSprite;
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                return;
            }

            image.sprite = normalSprite;
            image.type = normalSprite != null && normalSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;

            var state = button.spriteState;
            state.highlightedSprite = normalSprite;
            state.pressedSprite = pressedSprite;
            state.disabledSprite = disabledSprite;
            button.spriteState = state;
        }

        private Button CreateIconButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            string normal,
            string pressed,
            string disabled,
            string icon)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);

            if (!string.IsNullOrEmpty(icon))
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(go.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.raycastTarget = false;
                iconImg.sprite = LoopSortingUIKit.LoadSpriteByKey(icon);
                iconImg.color = Color.white;
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                float iconSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.68f, 12f, 9999f);
                iconRect.anchoredPosition = new Vector2(0f, iconSize * 0.05f);
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            return btn;
        }

        private Button CreateLongButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 size,
            string normal,
            string pressed,
            string disabled,
            string label,
            out TMP_Text labelText)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 64;
            tmp.color = Color.white;
            var tRect = tmp.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            // Leave room for the left-side icon in result buttons.
            tRect.offsetMin = new Vector2(160f, 0f);
            tRect.offsetMax = new Vector2(-60f, 0f);
            labelText = tmp;

            return btn;
        }

        private Image CreateButtonIcon(Transform buttonTransform)
        {
            if (buttonTransform == null) return null;

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(buttonTransform, false);
            var icon = iconGO.AddComponent<Image>();
            icon.raycastTarget = false;

            var rect = iconGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.18f, 0.5f);
            rect.anchorMax = new Vector2(0.18f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 0f);

            float side = 150f;
            var btnRect = buttonTransform.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                side = Mathf.Clamp(btnRect.rect.height * 0.72f, 110f, 150f);
            }
            rect.sizeDelta = new Vector2(side, side);

            return icon;
        }

        private void AttachBoosterBadge(Transform buttonTransform, int count)
        {
            if (buttonTransform == null) return;
            var existing = buttonTransform.Find("Badge");
            if (existing != null) return;

            count = Mathf.Clamp(count, 0, 99);
            int tens = count / 10;
            int ones = count % 10;
            bool showTens = count >= 10;

            float buttonSize = 420f;
            var btnRect = buttonTransform.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                buttonSize = Mathf.Max(200f, Mathf.Min(btnRect.rect.width, btnRect.rect.height));
            }
            float badgeSize = Mathf.Clamp(buttonSize * 0.34f, 110f, 140f);

            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(buttonTransform, false);
            var badgeRect = badgeGO.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-badgeSize * 0.15f, badgeSize * 0.28f);
            badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);

            var bgGO = new GameObject("BadgeBG");
            bgGO.transform.SetParent(badgeGO.transform, false);
            var bg = bgGO.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.badge.bg");
            bg.color = Color.white;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(badgeSize * 0.86f, badgeSize * 0.86f);

            var tensGO = new GameObject("DigitTens");
            tensGO.transform.SetParent(badgeGO.transform, false);
            var tensImg = tensGO.AddComponent<Image>();
            tensImg.raycastTarget = false;
            tensImg.sprite = LoopSortingUIKit.LoadSpriteByKey($"ui.digit.{tens}");
            tensImg.color = Color.white;
            tensImg.gameObject.SetActive(showTens);
            var tensRect = tensGO.GetComponent<RectTransform>();
            tensRect.anchorMin = new Vector2(0.5f, 0.5f);
            tensRect.anchorMax = new Vector2(0.5f, 0.5f);
            tensRect.pivot = new Vector2(0.5f, 0.5f);
            float digitW = badgeSize * 0.46f;
            float digitH = digitW * 1.375f;
            tensRect.anchoredPosition = new Vector2(-digitW * 0.35f, 0f);
            tensRect.sizeDelta = new Vector2(digitW, digitH);

            var onesGO = new GameObject("DigitOnes");
            onesGO.transform.SetParent(badgeGO.transform, false);
            var onesImg = onesGO.AddComponent<Image>();
            onesImg.raycastTarget = false;
            onesImg.sprite = LoopSortingUIKit.LoadSpriteByKey($"ui.digit.{ones}");
            onesImg.color = Color.white;
            var onesRect = onesGO.GetComponent<RectTransform>();
            onesRect.anchorMin = new Vector2(0.5f, 0.5f);
            onesRect.anchorMax = new Vector2(0.5f, 0.5f);
            onesRect.pivot = new Vector2(0.5f, 0.5f);
            onesRect.anchoredPosition = showTens ? new Vector2(digitW * 0.35f, 0f) : Vector2.zero;
            onesRect.sizeDelta = new Vector2(digitW, digitH);
        }

        private Button CreateBoosterButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            string normal,
            string pressed,
            string disabled,
            string icon,
            string label)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);

            if (!string.IsNullOrEmpty(icon))
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(go.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.raycastTarget = false;
                iconImg.sprite = LoopSortingUIKit.LoadSpriteByKey(icon);
                iconImg.color = Color.white;
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                float iconSide = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.42f, 120f, 190f);
                float iconY = Mathf.Clamp(size.y * 0.10f, 26f, 40f);
                iconRect.anchoredPosition = new Vector2(0f, iconY);
                iconRect.sizeDelta = new Vector2(iconSide, iconSide);
            }

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = Mathf.RoundToInt(Mathf.Clamp(size.y * 0.14f, 40f, 52f));
            tmp.color = Color.white;
            var labelRect = tmp.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0f);
            labelRect.anchorMax = new Vector2(0.5f, 0f);
            labelRect.pivot = new Vector2(0.5f, 0f);
            float labelY = Mathf.Clamp(size.y * 0.10f, 24f, 40f);
            labelRect.anchoredPosition = new Vector2(0f, labelY);
            labelRect.sizeDelta = new Vector2(Mathf.Clamp(size.x * 0.55f, 200f, 260f), Mathf.Clamp(size.y * 0.18f, 60f, 90f));

            return btn;
        }

        private Toggle CreateToggleRow(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            string label,
            bool initial,
            out Image trackImage,
            out RectTransform knobRect)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var rowGO = new GameObject(name);
            rowGO.transform.SetParent(parent, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = anchor;
            rowRect.anchorMax = anchor;
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = anchoredPos;
            rowRect.sizeDelta = new Vector2(760f, 120f);

            var toggle = rowGO.AddComponent<Toggle>();
            toggle.isOn = initial;

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.raycastTarget = false;
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.fontSize = 48;
            labelText.color = Color.white;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(40f, 0f);
            labelRect.sizeDelta = new Vector2(300f, 80f);

            var trackGO = new GameObject("ToggleTrack");
            trackGO.transform.SetParent(rowGO.transform, false);
            trackImage = trackGO.AddComponent<Image>();
            trackImage.raycastTarget = true;
            if (hasKit)
            {
                trackImage.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.toggle.track_on");
                trackImage.color = Color.white;
            }
            else
            {
                trackImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            }
            var trackRect = trackGO.GetComponent<RectTransform>();
            trackRect.anchorMin = new Vector2(1f, 0.5f);
            trackRect.anchorMax = new Vector2(1f, 0.5f);
            trackRect.pivot = new Vector2(1f, 0.5f);
            trackRect.anchoredPosition = new Vector2(-40f, 0f);
            trackRect.sizeDelta = new Vector2(220f, 60f);

            var knobGO = new GameObject("ToggleKnob");
            knobGO.transform.SetParent(rowGO.transform, false);
            var knobImg = knobGO.AddComponent<Image>();
            knobImg.raycastTarget = false;
            if (hasKit)
            {
                knobImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.toggle.knob");
                knobImg.color = Color.white;
            }
            knobRect = knobGO.GetComponent<RectTransform>();
            knobRect.anchorMin = new Vector2(1f, 0.5f);
            knobRect.anchorMax = new Vector2(1f, 0.5f);
            knobRect.pivot = new Vector2(1f, 0.5f);
            knobRect.sizeDelta = new Vector2(64f, 64f);
            knobRect.anchoredPosition = new Vector2(-130f, 0f);

            toggle.targetGraphic = trackImage;
            toggle.graphic = knobImg;

            return toggle;
        }

        private void UpdateToggleVisual(Image track, RectTransform knob, bool isOn)
        {
            if (track != null && LoopSortingUIKit.IsAvailable())
            {
                track.sprite = LoopSortingUIKit.LoadSpriteByKey(isOn ? "ui.toggle.track_on" : "ui.toggle.track_off");
                track.color = Color.white;
            }

            if (knob != null)
            {
                knob.anchoredPosition = new Vector2(isOn ? -130f : -190f, 0f);
            }
        }

        private void BuildSlotMarkers()
        {
            if (!showSlotMarkersRuntime)
            {
                return;
            }

            // Clean previous markers
            foreach (var m in _slotMarkers)
            {
                if (m != null) Destroy(m);
            }
            _slotMarkers.Clear();
            _slotBasePositions.Clear();

            var parent = new GameObject("SlotMarkers");
            parent.transform.SetParent(transform, false);

            var cam = Camera.main;
            var markerRotation = cam != null ? cam.transform.rotation : Quaternion.identity;

            bool hasKit = LoopSortingUIKit.IsAvailable();
            var slotTex = hasKit ? LoopSortingUIKit.LoadTextureByKey("world.conveyor_slot") : null;
            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;

            // Match belt block footprint: belt blocks are scaled by spacing * beltBlockSizeFactor in X/Y.
            float side = Mathf.Max(0.05f, spacing * beltBlockSizeFactor);

            foreach (var t in _beltSlots)
            {
                _slotBasePositions.Add(t.position);
                _slotCurrentPositions.Add(t.position);
                var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                marker.name = "SlotMarker";
                marker.transform.SetParent(parent.transform, false);
                marker.transform.position = t.position;
                marker.transform.rotation = markerRotation;
                marker.transform.localScale = new Vector3(side, side, 1f);
                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    if (slotTex != null)
                    {
                        var c = new Color(1f, 1f, 1f, Mathf.Clamp01(slotMarkerColor.a));
                        var mat = LoopSortingUIKit.CreateUnlitTextureMaterial(slotTex, c, 2900);
                        if (mat != null) renderer.sharedMaterial = mat;
                    }
                    else
                    {
                        var shader =
                            Shader.Find("Unlit/Color") ??
                            Shader.Find("Sprites/Default") ??
                            Shader.Find("UI/Default") ??
                            Shader.Find("Standard");
                        if (shader != null)
                        {
                            var mat = new Material(shader);
                            mat.color = slotMarkerColor;
                            renderer.sharedMaterial = mat;
                        }
                    }
                }
                var col = marker.GetComponent<Collider>();
                if (col != null) Destroy(col);
                _slotMarkers.Add(marker);
            }
        }

        private void UpdateSlotMarkersVisuals(float progress)
        {
            if (!showSlotMarkersRuntime || _slotMarkers.Count == 0 || _slotBasePositions.Count == 0)
            {
                return;
            }

            var cam = Camera.main;
            var markerRotation = cam != null ? cam.transform.rotation : Quaternion.identity;
            int count = _slotBasePositions.Count;

            for (int i = 0; i < _slotMarkers.Count && i < count; i++)
            {
                var marker = _slotMarkers[i];
                if (marker == null) continue;
                marker.transform.rotation = markerRotation;

                var from = _slotBasePositions[i];
                // Do not interpolate the last one to avoid a visual jump between exit and entry.
                if (i == count - 1)
                {
                    if (!_beltLoop)
                    {
                        marker.SetActive(false);
                        SetSlotCurrent(i, from);
                    }
                    else
                    {
                        marker.SetActive(true);
                        var to = _slotBasePositions[0];
                        var pos = Vector3.Lerp(from, to, progress);
                        marker.transform.position = pos;
                        SetSlotCurrent(i, pos);
                    }
                }
                else
                {
                    marker.SetActive(true);
                    var to = _slotBasePositions[i + 1];
                    var pos = Vector3.Lerp(from, to, progress);
                    marker.transform.position = pos;
                    SetSlotCurrent(i, pos);
                }
            }
            // last slot current position fallback
            if (count > 0 && _slotCurrentPositions.Count >= count && !_beltLoop)
            {
                _slotCurrentPositions[count - 1] = _slotBasePositions[count - 1];
            }
        }

        private void UpdateBeltBlockVisuals(float progress)
        {
            if (_beltBlockVisuals.Count == 0 || _slotCurrentPositions.Count == 0)
            {
                return;
            }

            foreach (var kv in _beltBlockVisuals)
            {
                int idx = kv.Key;
                var go = kv.Value;
                if (go == null) continue;
                if (idx < 0 || idx >= _slotCurrentPositions.Count) continue;
                if (_beltSpawnAnimating.Contains(idx)) continue;
                if (_beltInsertAnimating.Contains(idx)) continue;

                var pos = _slotCurrentPositions[idx] + GetBeltBlockOffset(idx) + new Vector3(0f, 0f, beltBlockZOffset);
                go.transform.position = pos;
            }
        }

        private void EnsureBlockVisual(int index, Block block)
        {
            if (!_beltBlockVisuals.TryGetValue(index, out var go) || go == null)
            {
                go = BlockVisual.CreateBlock(block.Color);
                _beltBlockVisuals[index] = go;
                go.transform.SetParent(transform, true);
            }

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float baseSize = Mathf.Max(0.05f, spacing * beltBlockSizeFactor);
            go.transform.localScale = new Vector3(baseSize, baseSize, baseSize * 0.6f);

            BlockVisual.ApplyColor(go, BlockVisual.ToUnityColor(block.Color));
        }

        private void StartBeltSpawnFromBox(int containerIndex, Block released)
        {
            if (_currentLayout == null || _beltSlots == null || _beltSlots.Count == 0) return;
            if (containerIndex < 0 || containerIndex >= _boxSpecs.Count) return;
            if (!_containerToBelt.TryGetValue(containerIndex, out var beltIndex)) return;
            if (beltIndex < 0 || beltIndex >= _beltSlots.Count) return;

            if (!_beltBlockVisuals.TryGetValue(beltIndex, out var go) || go == null)
            {
                EnsureBlockVisual(beltIndex, released);
                _beltBlockVisuals.TryGetValue(beltIndex, out go);
            }
            if (go == null) return;

            var spec = _boxSpecs[containerIndex];
            var size = LayoutUtils.ComputeBoxSize(spec, _currentLayout.blockSize);
            var mouth2 = LayoutUtils.ComputeMouth(spec, size);
            var mouth = new Vector3(mouth2.x, mouth2.y, 0f);

            Vector2 n2 = Vector2.down;
            switch (spec.opening)
            {
                case OpeningSide.Top: n2 = Vector2.up; break;
                case OpeningSide.Bottom: n2 = Vector2.down; break;
                case OpeningSide.Left: n2 = Vector2.left; break;
                case OpeningSide.Right: n2 = Vector2.right; break;
            }
            var normal = new Vector3(n2.x, n2.y, 0f);

            float unit = _currentLayout.blockSize > 0 ? _currentLayout.blockSize : 0.6f;
            float pad = Mathf.Max(0.05f, unit * 0.6f);

            var start = mouth - normal * pad + new Vector3(0f, 0f, beltBlockZOffset);
            go.transform.position = start;

            StopBeltSpawnAnimation(beltIndex);
            _beltSpawnAnimating.Add(beltIndex);
            _beltSpawnCoroutines[beltIndex] = StartCoroutine(AnimateBeltSpawn(beltIndex, start));
        }

        private void StopBeltSpawnAnimation(int beltIndex)
        {
            if (_beltSpawnCoroutines.TryGetValue(beltIndex, out var co) && co != null)
            {
                StopCoroutine(co);
            }
            _beltSpawnCoroutines.Remove(beltIndex);
            _beltSpawnAnimating.Remove(beltIndex);
        }

        private IEnumerator AnimateBeltSpawn(int beltIndex, Vector3 start)
        {
            float duration = Mathf.Clamp(conveyorTickSeconds * 0.55f, 0.06f, 0.22f);
            duration = Mathf.Max(0.0001f, duration);

            float t = 0f;
            while (t < duration)
            {
                if (!_beltBlockVisuals.TryGetValue(beltIndex, out var go) || go == null) break;
                if (beltIndex < 0 || beltIndex >= _slotCurrentPositions.Count) break;

                t += Time.deltaTime * Mathf.Max(0.0001f, EffectiveSpeedMultiplier);
                float u = Mathf.Clamp01(t / duration);
                var end = _slotCurrentPositions[beltIndex] + new Vector3(0f, 0f, beltBlockZOffset);
                end += GetBeltBlockOffset(beltIndex);
                go.transform.position = Vector3.Lerp(start, end, u);
                yield return null;
            }

            if (_beltBlockVisuals.TryGetValue(beltIndex, out var finalGo) && finalGo != null && beltIndex >= 0 && beltIndex < _slotCurrentPositions.Count)
            {
                finalGo.transform.position = _slotCurrentPositions[beltIndex] + GetBeltBlockOffset(beltIndex) + new Vector3(0f, 0f, beltBlockZOffset);
            }

            PlaySfx(SfxId.BlockLand);
            _beltSpawnAnimating.Remove(beltIndex);
            _beltSpawnCoroutines.Remove(beltIndex);
        }

        private void SetSlotCurrent(int index, Vector3 pos)
        {
            while (_slotCurrentPositions.Count <= index)
            {
                _slotCurrentPositions.Add(Vector3.zero);
            }
            _slotCurrentPositions[index] = pos;
        }
    }
}
