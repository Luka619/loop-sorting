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
    public partial class GameRuntimeController : MonoBehaviour, IGameStateHost
    {
        [Header("Config")]
        public float conveyorTickSeconds = 0.35f;
        public float beltSlotSpacing = 0.6f;
        [Tooltip("Camera padding around level bounds. Negative -> use preview-style auto padding.")]
        public float cameraPadding = -1f;
        public float cameraZOffset = -10f;
        [Tooltip("Camera tilt (degrees) around X. Small tilt reveals block sides for a more 3D look.")]
        public float cameraTiltX = -30f;
        [Tooltip("Camera yaw (degrees) around Y. Keep 0 for symmetrical layout.")]
        public float cameraYawY = 0f;
        [Tooltip("Reserve a fraction of vertical viewport for top UI when framing the level (0..0.45).")]
        public float cameraReservedTop = 0.08f;
        [Tooltip("Reserve a fraction of vertical viewport for bottom UI when framing the level (0..0.55).")]
        public float cameraReservedBottom = 0.12f;
        [Header("Layout Auto Fix")]
        [Tooltip("Auto push boxes away from the belt when they overlap or get too close.")]
        public bool autoResolveLayoutOverlap = true;
        [Tooltip("Minimum gap between box bounds and the belt ribbon (world units).")]
        public float minBoxToBeltGap = 0.08f;
        [Tooltip("Preferred gap between box bounds and the belt ribbon (world units). 0 = disabled.")]
        public float preferredBoxToBeltGap = 0.18f;
        [Tooltip("Minimum gap between box bounds (world units).")]
        public float minBoxToBoxGap = 0.05f;
        [Range(1, 8)]
        public int overlapResolveIterations = 3;
        [Header("Camera Clamp")]
        [Tooltip("Clamp the max orthographic size so oversized layouts don't look too small (0 = disabled).")]
        public float cameraMaxOrthoSize = 0f;
        [Tooltip("Minimum on-screen pixel size for a single block (0 = disabled). May crop very large layouts.")]
        public float minBlockPixelSize = 0f;
        [Tooltip("Visual size of each block in box grid.")]
        public Vector2 blockVisualSize = new Vector2(0.45f, 0.45f);
        [Tooltip("Max blocks / slots on the conveyor (default 50). If layout sets beltCapacity > 0, it overrides this.")]
        public int beltBlockLimit = 50;
        [Tooltip("Scale factor for belt block size relative to slot spacing.")]
        public float beltBlockSizeFactor = 0.65f;
        [Tooltip("Depth scale for belt blocks (Z), relative to belt block size.")]
        public float beltBlockDepthFactor = 1f;
        [Header("Animation")]
        [Tooltip("Seconds between consecutive block releases from the same container.")]
        public float releaseInterval = 0.12f;
        [Tooltip("Seconds to wait before retrying if belt slot is blocked.")]
        public float releaseBlockedRetry = 0.1f;
        [Tooltip("Z offset for belt blocks so they render above markers (negative brings closer to camera).")]
        public float beltBlockZOffset = -0.05f;

        private const float SaveDelayStrongSeconds = 0.20f;
        private const float SaveDelayWeakSeconds = 2.00f;
        private const string RuntimeUnlitTextureMaterialResourcePath = "LoopSortingUnlitTexture";
        private const float BeltSpawnPunchScale = 0.12f;
        private const float BeltSpawnPunchSeconds = 0.12f;
        private static readonly Color BeltRailColor = Color.white;
        private const float BeltRailCornerRadiusRatio = 0.12f;
        private const int BeltRailCornerSegments = 2;
        private static readonly Color BeltEndcapColor = new Color(0.93f, 0.58f, 0.34f, 1f);
        private const float BeltEndcapLengthRatio = 0.75f;
        private const float BeltEndcapHeightRatio = 1.2f;
        private const float BeltEndcapCornerRadiusRatio = 0.45f;
        private const int BeltEndcapCornerSegments = 4;
        private const float BeltEndcapRingLengthRatio = 0.26f;
        private const float BeltEndcapRingWidthRatio = 1.08f;
        private const float BeltEndcapRingHeightRatio = 1.05f;

        private bool _hasLoadedSave;
        private bool _saveDirty;
        private float _saveDueUnscaledTime = -1f;
        [Header("Debug/Visuals")]
        public bool showSlotGizmos = true;
        [Tooltip("Log each box's resolved belt port mapping (slot index + world position). Useful when blocks don't enter the expected box.")]
        public bool debugLogBoxPorts = false;
        public Color slotColor = new Color(1f, 0.8f, 0.2f, 0.9f);
        public float slotGizmoRadius = 0.1f;
        [Tooltip("Slot markers visible in-game (visual only).")]
        public bool showSlotMarkersRuntime = true;
        public float slotMarkerScale = 0.15f;
        public Color slotMarkerColor = new Color(0.8f, 0.8f, 0.8f, 0.25f);
        [Tooltip("Update slot marker visuals every N frames (1 = every frame).")]
        [Min(1)] public int slotMarkerVisualUpdateInterval = 1;
        [Tooltip("Override slot marker visual update interval on mobile/web platforms.")]
        [Min(1)] public int slotMarkerVisualUpdateIntervalMobile = 2;
        [Header("Speed")]
        public float[] speedSteps = new float[] { 1f, 1.5f, 2f };

        private LoopSortingGame _game;
        private List<Transform> _beltSlots = new List<Transform>();
        private Dictionary<int, GameObject> _beltBlockVisuals = new Dictionary<int, GameObject>();
        private List<BoxView> _boxViews = new List<BoxView>();
        private Dictionary<int, int> _containerToBelt = new Dictionary<int, int>();
        private readonly Dictionary<Container, int> _containerIndexByRef = new Dictionary<Container, int>();
        private readonly List<ConveyorPortEvent> _portEvents = new List<ConveyorPortEvent>(32);
        private readonly Dictionary<int, Vector3> _beltBlockOffsets = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Coroutine> _beltBlockOffsetCoroutines = new Dictionary<int, Coroutine>();
        private Coroutine _emptyDeferredHintRoutine;
        private LineRenderer _emptyDeferredLine;
        private RejectFeedbackGate _rejectGate;
        private List<BoxSpec> _boxSpecs = new List<BoxSpec>();
        private List<bool> _boxLocked = new List<bool>();
        private List<bool> _boxCompleted = new List<bool>();
        private List<GameObject> _slotMarkers = new List<GameObject>();
        private List<Vector3> _slotBasePositions = new List<Vector3>();
        private List<Vector3> _slotCurrentPositions = new List<Vector3>();
        private Material _slotMarkerMaterial;
        private bool _beltLoop;
        private float _tickTimer;
        private Bounds _levelBounds;
        private int _beltCapacity;
        private float _beltSpacingUsed;
        private bool _isReleasing;
        private int? _activeReleasePort;
        private float _speedMultiplier = 2.5f;
        private int _speedIndex = 0;
        private bool _didLogOrangeLongNineSlice;
        private readonly GameProgressState _progress = new GameProgressState
        {
            Coins = 810,
            Lives = 5,
            BoosterSortCount = InitialBoosterCount,
            BoosterShuffleCount = InitialBoosterCount,
        };
        private GameStateMachine _stateMachine;
        private AudioService _audio;
        private GameObject _fastTag;
	        private System.Random _rng = new System.Random();
	        private bool _inputLocked = false;
        private Camera _cachedMainCamera;
        private int _slotMarkerVisualFrameCounter;
        private GameObject _backgroundQuad;
        private bool _backgroundDebugLogged;
        private GameObject _conveyorBelt;
        private static bool _beltMaterialDebugLogged;
        private static bool _beltMaterialFailedLogged;
        private static bool _backgroundMaterialFailedLogged;
        private float _beltWidthUsed;
        private LayoutUtils.BeltPathCache _beltPathCache;
        private LevelFlow _pendingFlow;
        private int _pendingFlowIndex;
        private LevelLayout _pendingLevel;
        private LevelFlow _flow;
        private int _flowIndex;
        private LevelLayout _currentLayout;
        private LevelLayout _currentLayoutSource;
        private LevelLayout _runtimeLayoutInstance;
        private bool _gameOver;
        private Coroutine _endSequenceRoutine;
        private const float WinEndSequenceDelaySeconds = 0.75f;
        private const float LoseEndSequenceDelaySeconds = 0.60f;
        private bool _fullBeltFastForward;
        private int _fullBeltStepsRemaining;
        private const int WinCoinsReward = 40;
        private const int WinAdRewardMultiplier = 5;
        private const int LoseReviveCoinsCost = 900;
        private const int InitialBoosterCount = 0;
        private const int SortPurchaseCoinsPrice = 300;
        private const int ShufflePurchaseCoinsPrice = 400;
        private const int InitialCoins = 810;
        private const int InitialLives = 5;
        private static Material _runtimeUnlitTextureMaterialTemplate;
        private static bool _runtimeUnlitTextureMaterialTemplateLoggedMissing;

        private static Material GetRuntimeUnlitTextureMaterialTemplate()
        {
            if (_runtimeUnlitTextureMaterialTemplate != null) return _runtimeUnlitTextureMaterialTemplate;
            _runtimeUnlitTextureMaterialTemplate = Resources.Load<Material>(RuntimeUnlitTextureMaterialResourcePath);
            if (_runtimeUnlitTextureMaterialTemplate == null)
            {
                // Fallback: create a runtime template from the shader if the material asset isn't packed for some reason.
                var shader = Shader.Find("LoopSorting/UnlitTexture");
                if (shader != null && shader.isSupported)
                {
                    _runtimeUnlitTextureMaterialTemplate = new Material(shader)
                    {
                        name = "LoopSortingUnlitTexture_RuntimeTemplate",
                    };
                    _runtimeUnlitTextureMaterialTemplate.hideFlags = HideFlags.HideAndDontSave;
                }
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            if (_runtimeUnlitTextureMaterialTemplate == null && !_runtimeUnlitTextureMaterialTemplateLoggedMissing)
            {
                _runtimeUnlitTextureMaterialTemplateLoggedMissing = true;
                Debug.LogError(
                    $"[WorldMaterial] Missing '{RuntimeUnlitTextureMaterialResourcePath}.mat' in Resources and can't find shader 'LoopSorting/UnlitTexture'.");
            }
#endif
            return _runtimeUnlitTextureMaterialTemplate;
        }

        private enum BoosterType
        {
            Sort,
            Shuffle
        }

        private static int GetBoosterCoinPrice(BoosterType type)
        {
            return type == BoosterType.Shuffle ? ShufflePurchaseCoinsPrice : SortPurchaseCoinsPrice;
        }

        private bool _bgmPressure;
        private bool _sfxHasSnapshot;
        private bool _sfxPrevFastForward;
        private bool _sfxSuppressSpeeddownOnce;
        private readonly List<int> _sfxPrevContainerCounts = new List<int>();
        private readonly List<bool> _sfxPrevLockedStates = new List<bool>();
        private readonly List<bool> _sfxPrevCompletedStates = new List<bool>();
        private int _conveyorTickSfxCountdown;
        private int _sfxInsertEventsThisTick;

        public float EffectiveSpeedMultiplier => _fullBeltFastForward ? 5f : _speedMultiplier;

        private readonly HashSet<int> _beltSpawnAnimating = new HashSet<int>();
        private readonly Dictionary<int, Coroutine> _beltSpawnCoroutines = new Dictionary<int, Coroutine>();
        private readonly Dictionary<int, Vector3> _beltFrozenPositions = new Dictionary<int, Vector3>();
        private readonly HashSet<int> _beltWaitingIndices = new HashSet<int>();
        private readonly List<int> _beltFrozenRemove = new List<int>();

        private void EnsureStateMachine()
        {
            if (_stateMachine == null)
            {
                _stateMachine = new GameStateMachine(this);
            }
        }

        private void EnsureAudioService()
        {
            if (_audio == null)
            {
                _audio = new AudioService(this);
            }
        }

        private void LoadSaveIfNeeded()
        {
            if (_hasLoadedSave) return;
            _hasLoadedSave = true;

            if (!LoopSortingSaveService.TryLoad(out var save)) return;

            soundEnabled = save.soundEnabled;
            musicEnabled = save.musicEnabled;
            vibrationEnabled = save.vibrationEnabled;

            _progress.Coins = Mathf.Max(0, save.coins);
            _progress.Lives = Mathf.Max(0, save.lives);
            _progress.BoosterSortCount = save.boosterSortCount;
            _progress.BoosterShuffleCount = save.boosterShuffleCount;

            _progress.SavedFlowIndex = save.flowIndex;
            _progress.SavedHighestUnlockedFlowIndex = save.highestUnlockedFlowIndex;
        }

        private LoopSortingSaveService.SaveData BuildSaveData()
        {
            int flowIndex = _flow != null ? Mathf.Max(0, _flowIndex) : Mathf.Max(0, _progress.SavedFlowIndex);
            int highestUnlocked = Mathf.Max(_progress.SavedHighestUnlockedFlowIndex, flowIndex);

            return new LoopSortingSaveService.SaveData
            {
                flowIndex = flowIndex,
                highestUnlockedFlowIndex = highestUnlocked,
                coins = Mathf.Max(0, _progress.Coins),
                lives = Mathf.Max(0, _progress.Lives),
                boosterSortCount = Mathf.Clamp(_progress.BoosterSortCount, 0, 99),
                boosterShuffleCount = Mathf.Clamp(_progress.BoosterShuffleCount, 0, 99),
                soundEnabled = soundEnabled,
                musicEnabled = musicEnabled,
                vibrationEnabled = vibrationEnabled,
            };
        }

        private void RequestSave(float delaySeconds, bool coalesce = true)
        {
            _saveDirty = true;
            delaySeconds = Mathf.Max(0f, delaySeconds);
            float due = Time.unscaledTime + delaySeconds;
            if (!coalesce || _saveDueUnscaledTime < 0f)
            {
                _saveDueUnscaledTime = due;
                return;
            }

            _saveDueUnscaledTime = Mathf.Min(_saveDueUnscaledTime, due);
        }

        private void FlushSave()
        {
            if (!_saveDirty) return;
            _saveDirty = false;
            _saveDueUnscaledTime = -1f;

            LoopSortingSaveService.Save(BuildSaveData());
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

        public void Build(LevelFlow flow, int startIndex = 0)
        {
            _flow = flow;
            _flowIndex = Mathf.Clamp(startIndex, 0, flow != null ? Mathf.Max(0, flow.levels.Count - 1) : 0);
            var layout = flow != null && flow.levels.Count > 0 ? flow.levels[_flowIndex] : null;
            BuildInternal(layout, clearFlow: false);
        }

        private void EnsureSfx()
        {
            EnsureAudioService();
            _audio.EnsureSfx(soundEnabled);
        }

        private void EnsureMusic()
        {
            EnsureBgm();
        }

        private void EnsureBgm()
        {
            EnsureAudioService();
            _audio.EnsureBgm(musicEnabled);

            if (!musicEnabled)
            {
                _bgmPressure = false;
                return;
            }

            if (_audio.Bgm == null)
            {
                _bgmPressure = false;
                return;
            }

            // Ensure a base loop is active (Menu when not in gameplay; GameplayBase otherwise).
            if (_game == null || _gameOver)
            {
                _bgmPressure = false;
                _audio.PlayBgmLoop(BgmLoopId.Menu, fadeSeconds: 0f);
                return;
            }

            UpdateBgmPressureAfterTick(force: true);
        }

        private void UpdateBgmPressureAfterTick(bool force = false)
        {
            if (!musicEnabled || _audio == null || _audio.Bgm == null || _game == null || _gameOver)
            {
                return;
            }

            bool wantPressure = _fullBeltFastForward || _speedMultiplier >= 4.99f;
            if (!force && wantPressure == _bgmPressure)
            {
                return;
            }

            _bgmPressure = wantPressure;
            _audio.PlayBgmLoop(wantPressure ? BgmLoopId.GameplayPressure : BgmLoopId.GameplayBase, fadeSeconds: force ? 0f : 0.4f);
        }

        private void UpdateConveyorLoopSfx()
        {
            if (_audio == null || _audio.Sfx == null) return;
            if (!conveyorAmbienceEnabled) { _audio.StopSfxLoop(); return; }
            if (!soundEnabled) { _audio.StopSfxLoop(); return; }

            // Only run loop SFX during active gameplay.
            if (_game == null || _gameOver)
            {
                _audio.StopSfxLoop();
                return;
            }

            float pitch = _fullBeltFastForward ? 1.15f : 1f;
            if (!_fullBeltFastForward && _speedMultiplier >= 4.99f) pitch = 1.12f;

            _audio.StartSfxLoop(SfxId.ConveyorLoop, volumeMultiplier: 1f, pitch: pitch);
        }

        private void PlaySfx(SfxId id, float volumeMultiplier = 1f)
        {
            TryVibrateForSfx(id);
            if (!soundEnabled)
            {
                return;
            }

            EnsureSfx();
            _audio.PlaySfx(id, volumeMultiplier);
        }

        private void TryVibrateForSfx(SfxId id)
        {
            if (!vibrationEnabled) return;

            switch (id)
            {
                case SfxId.UiDenied:
                case SfxId.BlockInsert:
                case SfxId.BlockReject:
                case SfxId.BlockRejectLocked:
                case SfxId.BlockRejectBusy:
                case SfxId.BlockRejectFull:
                case SfxId.BlockRejectMismatch:
                case SfxId.BoxComplete:
                case SfxId.BoxUnlock:
                case SfxId.BoosterActivate:
                case SfxId.BoosterFail:
                case SfxId.ConveyorFullFail:
                case SfxId.LevelWin:
                case SfxId.LevelLose:
                    TryVibrate();
                    break;
            }
        }

        private static void TryVibrate()
        {
#if (UNITY_WEBGL || WEIXINMINIGAME) && !UNITY_EDITOR
            try
            {
                WeChatWASM.WX.VibrateShort(new WeChatWASM.VibrateShortOption { type = "light" });
            }
            catch
            {
                // Best-effort only.
            }
#else
            try
            {
                Handheld.Vibrate();
            }
            catch
            {
                // Best-effort only.
            }
#endif
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

            if (musicEnabled)
            {
                EnsureBgm();
            }

            int inserts = 0;
            int insertEvents = _sfxInsertEventsThisTick;
            _sfxInsertEventsThisTick = 0;
            int completions = 0;
            int unlocks = 0;

            int n = _game.Containers.Count;
            for (int i = 0; i < n; i++)
            {
                int countNow = _game.Containers[i].Count;
                int countPrev = i < _sfxPrevContainerCounts.Count ? _sfxPrevContainerCounts[i] : 0;
                if (insertEvents <= 0 && countNow > countPrev)
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

            if (insertEvents > 0)
            {
                inserts = insertEvents;
            }
            if (inserts > 0)
            {
                float vol = 1f + Mathf.Min(0.6f, inserts * 0.08f);
                PlaySfx(SfxId.BlockInsert, vol);
            }
            if (completions > 0)
            {
                PlaySfx(SfxId.BoxComplete);
                if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.BoxComplete);
            }
            if (unlocks > 0)
            {
                PlaySfx(SfxId.BoxUnlock);
                if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.Unlock);
            }
            if (!_sfxPrevFastForward && _fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                PlaySfx(SfxId.ConveyorFullWarning);
                if (musicEnabled && _audio != null && _audio.Bgm != null)
                {
                    _bgmPressure = true;
                    _audio.PlayBgmLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.6f);
                    _audio.PlayBgmStinger(BgmStingerId.FullWarning);
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
                    if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.Speeddown);
                }
            }

            CaptureSfxSnapshot();
        }

        private static int FindFirstIncludedLayer(int cullingMask)
        {
            for (int i = 0; i < 32; i++)
            {
                if ((cullingMask & (1 << i)) != 0) return i;
            }
            return 0;
        }

        private int? TryGetBlockedPort()
        {
            if (!_isReleasing || _game == null || !_activeReleasePort.HasValue)
            {
                return null;
            }

            int portIdx = _activeReleasePort.Value;
            if (portIdx < 0 || portIdx >= _game.Conveyor.Length)
            {
                return null;
            }

            // Reserve the port for release so the queued blocks behind it wait.
            return portIdx;
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

        private int GetBoosterCount(BoosterType type)
        {
            return type == BoosterType.Sort ? _progress.BoosterSortCount : _progress.BoosterShuffleCount;
        }

        private void AddBooster(BoosterType type, int delta)
        {
            if (delta == 0) return;

            if (type == BoosterType.Sort)
            {
                _progress.BoosterSortCount = Mathf.Clamp(_progress.BoosterSortCount + delta, 0, 99);
            }
            else
            {
                _progress.BoosterShuffleCount = Mathf.Clamp(_progress.BoosterShuffleCount + delta, 0, 99);
            }

            RefreshBoosterBadges();
            RequestSave(SaveDelayStrongSeconds);
        }

        private void ConsumeBooster(BoosterType type, int amount)
        {
            amount = Mathf.Max(0, amount);
            if (amount == 0) return;
            AddBooster(type, -amount);
        }

	        private IEnumerator BoosterSortSequence(bool consumeBooster = true)
	        {
	            if (_game == null || IsGameplayInputLocked) yield break;
	            _inputLocked = true;
            SetInteractableForBooster(false);
            PlaySfx(SfxId.BoosterActivate);
            EnsureBgm();
            if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.BoosterActivate);

            float prevSpeed = _speedMultiplier;
            if (!_fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                if (musicEnabled && _audio != null && _audio.Bgm != null)
                {
                    _bgmPressure = true;
                    _audio.PlayBgmLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.4f);
                    _audio.PlayBgmStinger(BgmStingerId.Speedup);
                }
            }
            _speedMultiplier = 5f;
            RefreshFastTag();

            if (_game.Conveyor.BlockCount > 0)
            {
                yield return StartCoroutine(NormalizeBeltStateAnimated());
            }

            var before = CaptureContainerStates();
            bool ok = ApplyBoosterSortColor();
            var after = CaptureContainerStates();
            if (ok) StartCoroutine(PlayBoosterSortFx(before, after));
            PlaySfx(ok ? SfxId.BoosterFillSort : SfxId.BoosterFail);
            EmitSfxFromStateChanges();
            if (ok && consumeBooster) ConsumeBooster(BoosterType.Sort, 1);

            _speedMultiplier = prevSpeed;
                if (!_fullBeltFastForward && prevSpeed < 4.99f)
                {
                    PlaySfx(SfxId.ConveyorSpeeddown);
                    if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.Speeddown);
                }
            UpdateBgmPressureAfterTick();
            RefreshFastTag();
            SetInteractableForBooster(true);
            if (!_gameOver) _inputLocked = false;
        }

	        private IEnumerator BoosterShuffleSequence()
	        {
	            if (_game == null || IsGameplayInputLocked) yield break;
	            _inputLocked = true;
            SetInteractableForBooster(false);
            PlaySfx(SfxId.BoosterActivate);
            EnsureBgm();
            if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.BoosterActivate);

            float prevSpeed = _speedMultiplier;
            if (!_fullBeltFastForward)
            {
                PlaySfx(SfxId.ConveyorSpeedup);
                if (musicEnabled && _audio != null && _audio.Bgm != null)
                {
                    _bgmPressure = true;
                    _audio.PlayBgmLoop(BgmLoopId.GameplayPressure, fadeSeconds: 0.4f);
                    _audio.PlayBgmStinger(BgmStingerId.Speedup);
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
            if (ok) ConsumeBooster(BoosterType.Shuffle, 1);

            _speedMultiplier = prevSpeed;
                if (!_fullBeltFastForward && prevSpeed < 4.99f)
                {
                    PlaySfx(SfxId.ConveyorSpeeddown);
                    if (musicEnabled && _audio != null && _audio.Bgm != null) _audio.PlayBgmStinger(BgmStingerId.Speeddown);
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

        private IEnumerator PlayBoosterSortFx(ContainerStateSnapshot before, ContainerStateSnapshot after)
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
            go.transform.localScale = new Vector3(s0, s0, s0 * beltBlockDepthFactor);

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
                go.transform.localScale = new Vector3(s0, s0, s0 * beltBlockDepthFactor) * shrink;
                yield return null;
            }

            Destroy(go);
        }

        private bool ApplyBoosterSortColor()
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

            // Filter: only colors that can fully Sort at least one unlocked container.
            var candidates = new List<BlockColor>();
            foreach (var kv in colorCounts)
            {
                if (completedColors.Contains(kv.Key)) continue;
                // check if any eligible container can be fully Sorted by available blocks
                bool canSort = false;
                for (int i = 0; i < _game.Containers.Count; i++)
                {
                    if (i < _boxLocked.Count && _boxLocked[i]) continue;
                    if (i < _boxCompleted.Count && _boxCompleted[i]) continue;
                    var cont = _game.Containers[i];
                    if (cont.IsUniformAndFull()) continue;
                    if (kv.Value >= cont.Capacity)
                    {
                        canSort = true;
                        break;
                    }
                }
                if (canSort)
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
                // only consider containers we can actually Sort
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

            // must have enough to Sort target container; otherwise abort without changes
            int required = _game.Containers[targetIdx].Capacity;
            if (sourceBlocks.Count < required)
            {
                Build(_currentLayoutSource != null ? _currentLayoutSource : _currentLayout);
                return false;
            }

            // collect displaced non-target from target container so they won't disappear
            var displaced = _game.Containers[targetIdx].RemoveBlocksWhere(b => b.Color != targetColor);

            // Sort target container with targetColor up to capacity
            int cap = _game.Containers[targetIdx].Capacity;
            var SortList = new List<Block>();
            for (int i = 0; i < cap; i++)
            {
                SortList.Add(sourceBlocks[0]);
                sourceBlocks.RemoveAt(0);
            }
            _game.Containers[targetIdx].ClearAndAdd(SortList);

            // put displaced + leftover target blocks back into containers (Sort other unfinished containers)
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
            if (_saveDirty && _saveDueUnscaledTime >= 0f && Time.unscaledTime >= _saveDueUnscaledTime)
            {
                FlushSave();
            }

            if (_game == null)
            {
                return;
            }

            UpdateTutorial();
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
                    if (conveyorAmbienceEnabled)
                    {
                        PlaySfx(SfxId.ConveyorTick);
                    }
                    _conveyorTickSfxCountdown = 6 + _rng.Next(2); // 6~7 ticks
                }
                SyncBeltVisuals();
                ResetSlotPositionsToBase();
                UpdateBeltBlockVisuals(0f);
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

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushSave();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                FlushSave();
            }
        }

        private void OnApplicationQuit()
        {
            FlushSave();
        }

        private void ProcessConveyorPortEvents(List<ConveyorPortEvent> events)
        {
            if (events == null || events.Count == 0) return;
            _rejectGate ??= new RejectFeedbackGate();
            _sfxInsertEventsThisTick = 0;

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
                    _sfxInsertEventsThisTick++;
                    StartBeltBlockEnterBoxAnimation(e.BeltIndex, containerIndex, e.Block);
                    if (containerIndex >= 0 && containerIndex < _boxViews.Count)
                    {
                        _boxViews[containerIndex].PlayMouthSquash(Color.white, seconds: 0.12f);
                        _boxViews[containerIndex].PlayBoxBounce();
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

            // Detach from belt tracking so the slot can be reused immediately.
            StopBeltSpawnAnimation(beltIndex);
            StopBeltBlockOffsetAnimation(beltIndex);
            _beltBlockVisuals.Remove(beltIndex);
            StartCoroutine(AnimateBeltEnterBox(go, containerIndex));
        }

        private IEnumerator AnimateBeltEnterBox(GameObject go, int containerIndex)
        {
            if (go == null) yield break;
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
            if (go != null)
            {
                Destroy(go);
            }
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

        private void StopBeltBlockOffsetAnimation(int beltIndex)
        {
            if (_beltBlockOffsetCoroutines.TryGetValue(beltIndex, out var existing) && existing != null)
            {
                StopCoroutine(existing);
            }
            _beltBlockOffsetCoroutines.Remove(beltIndex);
            _beltBlockOffsets.Remove(beltIndex);
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

	        public void HandleContainerClick(int containerIndex)
	        {
	            if (_game == null || _isReleasing || IsGameplayInputLocked)
	            {
	                return;
	            }

            if (containerIndex < 0 || containerIndex >= _game.Containers.Count)
            {
                return;
            }

            if (!IsTutorialClickAllowed(containerIndex))
            {
                PlaySfx(SfxId.UiDenied);
                if (containerIndex < _boxViews.Count) _boxViews[containerIndex].PlayDeniedFeedback();
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

            NotifyTutorialContainerClicked(containerIndex);
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
                smoothSubdivisions: layout.cornerSubdivisions,
                out _beltPathCache);

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
                _beltPathCache = null;
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
            // Keep belt width visually reasonable relative to slot spacing (prevents "screen-Sorting" ribbon).
            float beltWidth = Mathf.Clamp(requestedWidth, spacing * 0.8f, spacing * 1.6f);
            _beltWidthUsed = beltWidth;

            bool loop = path != null && path.loop;
            float beltSurfaceZ = 0.2f;
            var pts = BuildBeltPolylinePoints(path, spacing, smoothCorners, smoothTension, smoothSubdivisions, loop, z: beltSurfaceZ);
            if (pts == null || pts.Count < 2) return;

            var go = new GameObject("ConveyorBelt");
            go.transform.SetParent(transform, false);
            _conveyorBelt = go;

            float railHeight = Mathf.Clamp(spacing * 0.18f, 0.05f, 0.12f);
            float railWidth = Mathf.Clamp(spacing * 0.22f, beltWidth * 0.12f, beltWidth * 0.28f);
            float surfaceWidth = beltWidth;
            float railCornerMax = Mathf.Min(railWidth, railHeight) * 0.5f;
            float railCornerRadius = Mathf.Clamp(Mathf.Min(railWidth, railHeight) * BeltRailCornerRadiusRatio, 0.001f, railCornerMax);

            float railOffset = beltWidth * 0.5f - railWidth * 0.5f;
            var leftPts = OffsetPolyline(pts, railOffset);
            var rightPts = OffsetPolyline(pts, -railOffset);

            var surfaceGO = new GameObject("BeltSurface");
            surfaceGO.transform.SetParent(go.transform, false);
            var surfaceMf = surfaceGO.AddComponent<MeshFilter>();
            var surfaceMr = surfaceGO.AddComponent<MeshRenderer>();
            surfaceMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            surfaceMr.receiveShadows = false;
            surfaceMr.allowOcclusionWhenDynamic = false;
            surfaceMf.sharedMesh = BuildRibbonMesh(pts, surfaceWidth, out float totalLen);
            surfaceMr.sharedMaterial = CreateConveyorBeltMaterial(totalLen, spacing, loop);

            var railMat = CreateConveyorRailMaterial(BeltRailColor);
            var leftGO = new GameObject("BeltRailLeft");
            leftGO.transform.SetParent(go.transform, false);
            var leftMf = leftGO.AddComponent<MeshFilter>();
            var leftMr = leftGO.AddComponent<MeshRenderer>();
            leftMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            leftMr.receiveShadows = false;
            leftMr.allowOcclusionWhenDynamic = false;
            leftMf.sharedMesh = BuildRoundedRibbonMesh(leftPts, railWidth, railHeight, railCornerRadius, BeltRailCornerSegments, loop);
            leftMr.sharedMaterial = railMat;

            var rightGO = new GameObject("BeltRailRight");
            rightGO.transform.SetParent(go.transform, false);
            var rightMf = rightGO.AddComponent<MeshFilter>();
            var rightMr = rightGO.AddComponent<MeshRenderer>();
            rightMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rightMr.receiveShadows = false;
            rightMr.allowOcclusionWhenDynamic = false;
            rightMf.sharedMesh = BuildRoundedRibbonMesh(rightPts, railWidth, railHeight, railCornerRadius, BeltRailCornerSegments, loop);
            rightMr.sharedMaterial = railMat;

            if (!loop && pts.Count >= 2)
            {
                float capLength = Mathf.Clamp(spacing * BeltEndcapLengthRatio, beltWidth * 0.5f, beltWidth * 1.25f);
                float capHeight = Mathf.Clamp(railHeight * BeltEndcapHeightRatio, railHeight * 0.8f, railHeight * 1.6f);
                float capWidth = beltWidth;
                float capRadius = Mathf.Clamp(Mathf.Min(capWidth, capHeight) * BeltEndcapCornerRadiusRatio, 0.001f, Mathf.Min(capWidth, capHeight) * 0.5f);

                float ringLength = Mathf.Clamp(capLength * BeltEndcapRingLengthRatio, capLength * 0.12f, capLength * 0.6f);
                float ringWidth = capWidth * BeltEndcapRingWidthRatio;
                float ringHeight = capHeight * BeltEndcapRingHeightRatio;
                float ringRadius = Mathf.Clamp(Mathf.Min(ringWidth, ringHeight) * BeltEndcapCornerRadiusRatio, 0.001f, Mathf.Min(ringWidth, ringHeight) * 0.5f);

                var capMat = CreateConveyorEndcapMaterial(BeltEndcapColor);
                AddConveyorEndcap(go.transform, pts[0], pts[1], capLength, capWidth, capHeight, capRadius, BeltEndcapCornerSegments,
                    ringLength, ringWidth, ringHeight, ringRadius, BeltEndcapCornerSegments, capMat, railMat, "BeltEndcapStart");
                AddConveyorEndcap(go.transform, pts[pts.Count - 1], pts[pts.Count - 2], capLength, capWidth, capHeight, capRadius, BeltEndcapCornerSegments,
                    ringLength, ringWidth, ringHeight, ringRadius, BeltEndcapCornerSegments, capMat, railMat, "BeltEndcapEnd");
            }
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

        private static List<Vector3> OffsetPolyline(List<Vector3> points, float offset)
        {
            if (points == null || points.Count < 2) return points;
            if (Mathf.Abs(offset) < 0.0001f) return new List<Vector3>(points);

            int n = points.Count;
            var result = new List<Vector3>(n);
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
                result.Add(new Vector3(p.x + perp.x * offset, p.y + perp.y * offset, p.z));
            }

            return result;
        }

        private static Mesh BuildBoxedRibbonMesh(List<Vector3> points, float width, float height, bool loop)
        {
            var mesh = new Mesh();
            mesh.name = "ConveyorBeltRail";
            if (points == null || points.Count < 2)
            {
                return mesh;
            }

            int n = points.Count;
            var vertices = new Vector3[n * 4];
            var uvs = new Vector2[n * 4];

            float half = width * 0.5f;
            var cumulative = new float[n];
            cumulative[0] = 0f;
            for (int i = 1; i < n; i++)
            {
                cumulative[i] = cumulative[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
            float totalLen = Mathf.Max(0.0001f, cumulative[n - 1]);
            float invTotal = 1f / totalLen;

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
                var off = new Vector3(perp.x * half, perp.y * half, 0f);

                int v = i * 4;
                vertices[v + 0] = new Vector3(p.x + off.x, p.y + off.y, p.z);
                vertices[v + 1] = new Vector3(p.x - off.x, p.y - off.y, p.z);
                vertices[v + 2] = vertices[v + 0] + new Vector3(0f, 0f, height);
                vertices[v + 3] = vertices[v + 1] + new Vector3(0f, 0f, height);

                float u01 = cumulative[i] * invTotal;
                uvs[v + 0] = new Vector2(u01, 1f);
                uvs[v + 1] = new Vector2(u01, 0f);
                uvs[v + 2] = new Vector2(u01, 1f);
                uvs[v + 3] = new Vector2(u01, 0f);
            }

            int segmentCount = n - 1;
            int capCount = loop ? 0 : 2;
            int quadCount = segmentCount * 4 + capCount;
            var tris = new int[quadCount * 6];
            int ti = 0;

            for (int i = 0; i < segmentCount; i++)
            {
                int a = i * 4;
                int b = (i + 1) * 4;

                // Top
                tris[ti++] = a + 0;
                tris[ti++] = b + 0;
                tris[ti++] = a + 1;
                tris[ti++] = a + 1;
                tris[ti++] = b + 0;
                tris[ti++] = b + 1;

                // Bottom
                tris[ti++] = a + 2;
                tris[ti++] = a + 3;
                tris[ti++] = b + 2;
                tris[ti++] = a + 3;
                tris[ti++] = b + 3;
                tris[ti++] = b + 2;

                // Left side
                tris[ti++] = a + 0;
                tris[ti++] = b + 0;
                tris[ti++] = a + 2;
                tris[ti++] = a + 2;
                tris[ti++] = b + 0;
                tris[ti++] = b + 2;

                // Right side
                tris[ti++] = a + 1;
                tris[ti++] = a + 3;
                tris[ti++] = b + 1;
                tris[ti++] = a + 3;
                tris[ti++] = b + 3;
                tris[ti++] = b + 1;
            }

            if (!loop)
            {
                // Start cap
                tris[ti++] = 0;
                tris[ti++] = 2;
                tris[ti++] = 1;
                tris[ti++] = 1;
                tris[ti++] = 2;
                tris[ti++] = 3;

                // End cap
                int last = (n - 1) * 4;
                tris[ti++] = last + 0;
                tris[ti++] = last + 1;
                tris[ti++] = last + 2;
                tris[ti++] = last + 1;
                tris[ti++] = last + 3;
                tris[ti++] = last + 2;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static Mesh BuildRoundedRibbonMesh(List<Vector3> points, float width, float height, float cornerRadius, int cornerSegments, bool loop)
        {
            var mesh = new Mesh();
            mesh.name = "ConveyorBeltRailRounded";
            if (points == null || points.Count < 2)
            {
                return mesh;
            }

            var profile = BuildRoundedRectProfile(width, height, cornerRadius, cornerSegments);
            int ring = profile.Count;
            int n = points.Count;
            if (ring < 3 || n < 2)
            {
                return mesh;
            }

            var vertices = new List<Vector3>(n * ring + (loop ? 0 : 2));
            var uvs = new List<Vector2>(vertices.Capacity);
            var cumulative = new float[n];
            cumulative[0] = 0f;
            for (int i = 1; i < n; i++)
            {
                cumulative[i] = cumulative[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
            float totalLen = Mathf.Max(0.0001f, cumulative[n - 1]);
            float invTotal = 1f / totalLen;

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
                float u01 = cumulative[i] * invTotal;
                for (int j = 0; j < ring; j++)
                {
                    var c = profile[j];
                    var pos = new Vector3(p.x + perp.x * c.x, p.y + perp.y * c.x, p.z + c.y);
                    vertices.Add(pos);
                    uvs.Add(new Vector2(u01, (float)j / ring));
                }
            }

            int segments = loop ? n : (n - 1);
            int quadCount = segments * ring;
            int capTriCount = loop ? 0 : ring * 2;
            int triCount = quadCount * 2 + capTriCount;
            var tris = new int[triCount * 3];
            int ti = 0;

            for (int i = 0; i < segments; i++)
            {
                int iNext = (i + 1) % n;
                int baseA = i * ring;
                int baseB = iNext * ring;
                for (int j = 0; j < ring; j++)
                {
                    int jNext = (j + 1) % ring;
                    int a = baseA + j;
                    int b = baseB + j;
                    int aNext = baseA + jNext;
                    int bNext = baseB + jNext;

                    tris[ti++] = a;
                    tris[ti++] = b;
                    tris[ti++] = aNext;
                    tris[ti++] = aNext;
                    tris[ti++] = b;
                    tris[ti++] = bNext;
                }
            }

            if (!loop)
            {
                int startCenter = vertices.Count;
                vertices.Add(points[0]);
                uvs.Add(new Vector2(0f, 0.5f));

                int endCenter = vertices.Count;
                vertices.Add(points[n - 1]);
                uvs.Add(new Vector2(1f, 0.5f));

                for (int j = 0; j < ring; j++)
                {
                    int jNext = (j + 1) % ring;
                    // Start cap (normal faces -dir due to clockwise ring order).
                    tris[ti++] = startCenter;
                    tris[ti++] = jNext;
                    tris[ti++] = j;

                    // End cap (reverse winding to face +dir).
                    int baseEnd = (n - 1) * ring;
                    tris[ti++] = endCenter;
                    tris[ti++] = baseEnd + j;
                    tris[ti++] = baseEnd + jNext;
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        private static void AddConveyorEndcap(
            Transform parent,
            Vector3 anchor,
            Vector3 neighbor,
            float capLength,
            float capWidth,
            float capHeight,
            float capRadius,
            int capSegments,
            float ringLength,
            float ringWidth,
            float ringHeight,
            float ringRadius,
            int ringSegments,
            Material capMat,
            Material ringMat,
            string name)
        {
            if (parent == null) return;
            if (capMat == null && ringMat == null) return;

            Vector2 dir2 = new Vector2(anchor.x - neighbor.x, anchor.y - neighbor.y);
            if (dir2.sqrMagnitude < 0.0001f)
            {
                dir2 = Vector2.up;
            }
            dir2.Normalize();
            var dir3 = new Vector3(dir2.x, dir2.y, 0f);

            float safeCapLength = Mathf.Max(0.01f, capLength);
            float safeRingLength = Mathf.Max(0.01f, ringLength);

            var capCenter = anchor + dir3 * (safeCapLength * 0.5f);
            var ringCenter = anchor + dir3 * (safeRingLength * 0.5f);
            ringCenter.z += 0.002f;

            if (capMat != null)
            {
                var capGO = new GameObject(name);
                capGO.transform.SetParent(parent, false);
                var capMf = capGO.AddComponent<MeshFilter>();
                var capMr = capGO.AddComponent<MeshRenderer>();
                capMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                capMr.receiveShadows = false;
                capMr.allowOcclusionWhenDynamic = false;
                capMf.sharedMesh = BuildRoundedRibbonMesh(BuildStraightPoints(capCenter, dir3, safeCapLength), capWidth, capHeight, capRadius, capSegments, false);
                capMr.sharedMaterial = capMat;
            }

            if (ringMat != null)
            {
                var ringGO = new GameObject(name + "_Ring");
                ringGO.transform.SetParent(parent, false);
                var ringMf = ringGO.AddComponent<MeshFilter>();
                var ringMr = ringGO.AddComponent<MeshRenderer>();
                ringMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ringMr.receiveShadows = false;
                ringMr.allowOcclusionWhenDynamic = false;
                ringMf.sharedMesh = BuildRoundedRibbonMesh(BuildStraightPoints(ringCenter, dir3, safeRingLength), ringWidth, ringHeight, ringRadius, ringSegments, false);
                ringMr.sharedMaterial = ringMat;
            }
        }

        private static List<Vector3> BuildStraightPoints(Vector3 center, Vector3 dir, float length)
        {
            float half = length * 0.5f;
            return new List<Vector3>
            {
                center - dir * half,
                center + dir * half
            };
        }

        private static List<Vector2> BuildRoundedRectProfile(float width, float height, float radius, int cornerSegments)
        {
            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float maxRadius = Mathf.Min(halfW, halfH);
            float r = Mathf.Clamp(radius, 0f, maxRadius);
            int seg = Mathf.Max(1, cornerSegments);

            if (r <= 0.0001f)
            {
                return new List<Vector2>
                {
                    new Vector2(-halfW, halfH),
                    new Vector2(halfW, halfH),
                    new Vector2(halfW, -halfH),
                    new Vector2(-halfW, -halfH)
                };
            }

            var pts = new List<Vector2>((seg + 1) * 4);
            var tl = new Vector2(-halfW + r, halfH - r);
            var tr = new Vector2(halfW - r, halfH - r);
            var br = new Vector2(halfW - r, -halfH + r);
            var bl = new Vector2(-halfW + r, -halfH + r);

            AddArcPoints(pts, tl, 180f, 90f, r, seg, true);
            AddArcPoints(pts, tr, 90f, 0f, r, seg, false);
            AddArcPoints(pts, br, 0f, -90f, r, seg, false);
            AddArcPoints(pts, bl, -90f, -180f, r, seg, false);

            return pts;
        }

        private static void AddArcPoints(List<Vector2> pts, Vector2 center, float startDeg, float endDeg, float radius, int segments, bool includeStart)
        {
            float startRad = startDeg * Mathf.Deg2Rad;
            float endRad = endDeg * Mathf.Deg2Rad;
            for (int i = 0; i <= segments; i++)
            {
                if (!includeStart && i == 0) continue;
                float t = segments <= 0 ? 0f : (float)i / segments;
                float ang = Mathf.Lerp(startRad, endRad, t);
                pts.Add(center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius);
            }
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
                // Keep showing blocks under the lock overlay (frosted glass effect).
                _boxViews[i].SyncBlocks(_game.Containers[i].Blocks);
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

        private void RestartCurrent()
        {
            _gameOver = false;
            if (_flow != null && _flow.levels.Count > 0)
            {
                Build(_flow, _flowIndex);
            }
            else
            {
                Build(_currentLayoutSource != null ? _currentLayoutSource : _currentLayout);
            }
        }

        private static IEnumerable<Block> BuildBlocksForSpec(BoxSpec spec, int capacity)
        {
            if (spec.colorCounts == null) yield break;

            var list = new List<Block>(capacity);
            int Sorted = 0;
            // colorCounts are authored outer->inner (index 0 is the outermost / mouth-facing layer).
            // Keep the same order at runtime so the editor preview and gameplay match.
            for (int idx = 0; idx < spec.colorCounts.Count && Sorted < capacity; idx++)
            {
                var cc = spec.colorCounts[idx];
                int cnt = Mathf.Max(0, cc.count);
                for (int i = 0; i < cnt && Sorted < capacity; i++)
                {
                    list.Add(new Block(cc.color, cc.hidden));
                    Sorted++;
                }
            }

            foreach (var b in list)
            {
                yield return b;
            }
        }

        private void EnsureEconomyDefaults()
        {
            _progress.Coins = Mathf.Max(0, _progress.Coins);
            _progress.Lives = Mathf.Max(0, _progress.Lives);
        }

        private static string FormatCompact(int value, int unit, string suffix, int decimals)
        {
            float scaled = value / (float)unit;
            decimals = Mathf.Clamp(decimals, 0, 2);
            string fmt = decimals == 0 ? "0" : (decimals == 1 ? "0.#" : "0.##");
            return scaled.ToString(fmt, System.Globalization.CultureInfo.InvariantCulture) + suffix;
        }

	        private static Quaternion ComputeSlotMarkerRotation(
	            int index,
	            IReadOnlyList<Vector3> slotPositions,
	            bool loop,
	            Quaternion faceRotation)
	        {
	            int count = slotPositions != null ? slotPositions.Count : 0;
	            if (count < 2 || index < 0 || index >= count)
	            {
	                return faceRotation;
	            }

	            Vector3 dir;
	            if (index < count - 1)
	            {
	                dir = slotPositions[index + 1] - slotPositions[index];
	            }
	            else
	            {
	                dir = loop
	                    ? (slotPositions[0] - slotPositions[index])
	                    : (slotPositions[index] - slotPositions[index - 1]);
	            }

	            if (dir.sqrMagnitude < 0.000001f)
	            {
	                if (index > 0)
	                {
	                    dir = slotPositions[index] - slotPositions[index - 1];
	                }
	                else if (count > 1)
	                {
	                    dir = slotPositions[1] - slotPositions[0];
	                }
	            }

	            var normal = faceRotation * Vector3.forward;
	            var inPlane = Vector3.ProjectOnPlane(dir, normal);
	            if (inPlane.sqrMagnitude < 0.000001f)
	            {
	                return faceRotation;
	            }

	            inPlane.Normalize();
	            var right = faceRotation * Vector3.right;
	            var up = faceRotation * Vector3.up;
	            float angle = Mathf.Atan2(Vector3.Dot(inPlane, up), Vector3.Dot(inPlane, right)) * Mathf.Rad2Deg;
	            return Quaternion.AngleAxis(angle, normal) * faceRotation;
	        }

        private void ResetSlotPositionsToBase()
        {
            if (_slotBasePositions.Count == 0)
            {
                return;
            }

            int count = _slotBasePositions.Count;
            for (int i = 0; i < count; i++)
            {
                SetSlotCurrent(i, _slotBasePositions[i]);
            }
        }

        private void UpdateSlotMarkersVisuals(float progress)
        {
            if (_slotBasePositions.Count == 0)
            {
                return;
            }

            int count = _slotBasePositions.Count;
            bool updateVisuals = ShouldUpdateSlotMarkerVisuals() && showSlotMarkersRuntime && _slotMarkers.Count > 0;
            Quaternion markerRotation = Quaternion.identity;
            if (updateVisuals)
            {
                if (_cachedMainCamera == null || !_cachedMainCamera.isActiveAndEnabled)
                {
                    _cachedMainCamera = Camera.main;
                }
                markerRotation = _cachedMainCamera != null ? _cachedMainCamera.transform.rotation : Quaternion.identity;
            }

            if (_beltLoop && _beltPathCache != null && _beltPathCache.TotalLength > 0f &&
                _beltPathCache.EvalPoints != null && _beltPathCache.Cumulative != null)
            {
                float step = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
                float total = _beltPathCache.TotalLength;
                float offset = _beltPathCache.Offset;

                for (int i = 0; i < count; i++)
                {
                    float dist = (i * step) + offset + (progress * step);
                    dist = dist % total;
                    var p = LayoutUtils.PointAtDistance(_beltPathCache.EvalPoints, _beltPathCache.Cumulative, dist);
                    var pos = new Vector3(p.x, p.y, 0f);
                    SetSlotCurrent(i, pos);

                    if (!updateVisuals) continue;
                    if (i >= _slotMarkers.Count) continue;
                    var marker = _slotMarkers[i];
                    if (marker == null) continue;

                    marker.transform.rotation = ComputeSlotMarkerRotation(i, _slotBasePositions, _beltLoop, markerRotation);
                    marker.SetActive(true);
                    marker.transform.position = pos;
                }

                return;
            }

            for (int i = 0; i < count; i++)
            {
                var from = _slotBasePositions[i];
                bool isLast = i == count - 1;
                Vector3 pos;

                // Do not interpolate the last one to avoid a visual jump between exit and entry.
                if (isLast)
                {
                    if (!_beltLoop)
                    {
                        pos = from;
                        SetSlotCurrent(i, from);
                    }
                    else
                    {
                        var to = _slotBasePositions[0];
                        pos = Vector3.Lerp(from, to, progress);
                        SetSlotCurrent(i, pos);
                    }
                }
                else
                {
                    var to = _slotBasePositions[i + 1];
                    pos = Vector3.Lerp(from, to, progress);
                    SetSlotCurrent(i, pos);
                }

                if (!updateVisuals) continue;
                if (i >= _slotMarkers.Count) continue;
                var marker = _slotMarkers[i];
                if (marker == null) continue;

                marker.transform.rotation = ComputeSlotMarkerRotation(i, _slotBasePositions, _beltLoop, markerRotation);

                if (isLast && !_beltLoop)
                {
                    marker.SetActive(false);
                }
                else
                {
                    marker.SetActive(true);
                    marker.transform.position = pos;
                }
            }
            // last slot current position fallback
            if (count > 0 && _slotCurrentPositions.Count >= count && !_beltLoop)
            {
                _slotCurrentPositions[count - 1] = _slotBasePositions[count - 1];
            }
        }

        private bool ShouldUpdateSlotMarkerVisuals()
        {
            int interval = slotMarkerVisualUpdateInterval;
            if (Application.isMobilePlatform || Application.platform == RuntimePlatform.WebGLPlayer)
            {
                interval = slotMarkerVisualUpdateIntervalMobile;
            }
            interval = Mathf.Max(1, interval);
            if (interval <= 1) return true;

            _slotMarkerVisualFrameCounter = (_slotMarkerVisualFrameCounter + 1) % interval;
            return _slotMarkerVisualFrameCounter == 0;
        }

        private void UpdateBeltBlockVisuals(float progress)
        {
            if (_beltBlockVisuals.Count == 0 || _slotCurrentPositions.Count == 0)
            {
                ClearBeltWaitingState();
                return;
            }

            RefreshBeltWaitingState();

            foreach (var kv in _beltBlockVisuals)
            {
                int idx = kv.Key;
                var go = kv.Value;
                if (go == null) continue;
                if (idx < 0 || idx >= _slotCurrentPositions.Count) continue;
                if (_beltSpawnAnimating.Contains(idx)) continue;

                if (_beltFrozenPositions.TryGetValue(idx, out var frozen))
                {
                    go.transform.position = frozen;
                    continue;
                }

                var pos = _slotCurrentPositions[idx] + GetBeltBlockOffset(idx) + new Vector3(0f, 0f, beltBlockZOffset);
                go.transform.position = pos;
            }
        }

        private void RefreshBeltWaitingState()
        {
            if (!_isReleasing || !_activeReleasePort.HasValue || _game == null)
            {
                ClearBeltWaitingState();
                return;
            }

            int portIdx = _activeReleasePort.Value;
            if (portIdx < 0 || portIdx >= _game.Conveyor.Length)
            {
                ClearBeltWaitingState();
                return;
            }

            bool hasEmpty = false;
            for (int i = 0; i < _game.Conveyor.Length; i++)
            {
                if (!_game.Conveyor.GetSlot(i).HasValue)
                {
                    hasEmpty = true;
                    break;
                }
            }
            if (!hasEmpty)
            {
                // Full belt keeps moving; don't freeze visuals.
                ClearBeltWaitingState();
                return;
            }

            _beltWaitingIndices.Clear();
            int count = _game.Conveyor.Length;
            int idx = (portIdx - 1 + count) % count;
            while (idx != portIdx)
            {
                if (!_game.Conveyor.GetSlot(idx).HasValue)
                {
                    break;
                }

                if (!_beltSpawnAnimating.Contains(idx))
                {
                    _beltWaitingIndices.Add(idx);
                }

                idx = (idx - 1 + count) % count;
            }

            _beltFrozenRemove.Clear();
            foreach (var kv in _beltFrozenPositions)
            {
                if (!_beltWaitingIndices.Contains(kv.Key))
                {
                    _beltFrozenRemove.Add(kv.Key);
                }
            }
            for (int i = 0; i < _beltFrozenRemove.Count; i++)
            {
                _beltFrozenPositions.Remove(_beltFrozenRemove[i]);
            }

            foreach (var waitIdx in _beltWaitingIndices)
            {
                if (_beltFrozenPositions.ContainsKey(waitIdx)) continue;
                if (_beltBlockVisuals.TryGetValue(waitIdx, out var go) && go != null)
                {
                    _beltFrozenPositions[waitIdx] = go.transform.position;
                }
            }
        }

        private void ClearBeltWaitingState()
        {
            if (_beltFrozenPositions.Count > 0) _beltFrozenPositions.Clear();
            if (_beltWaitingIndices.Count > 0) _beltWaitingIndices.Clear();
            if (_beltFrozenRemove.Count > 0) _beltFrozenRemove.Clear();
        }

        private void EnsureBlockVisual(int index, Block block)
        {
            bool created = false;
            if (!_beltBlockVisuals.TryGetValue(index, out var go) || go == null)
            {
                go = BlockVisual.CreateBlock(block.Color);
                _beltBlockVisuals[index] = go;
                go.transform.SetParent(transform, true);
                created = true;
            }

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float baseSize = Mathf.Max(0.05f, spacing * beltBlockSizeFactor);
            go.transform.localScale = new Vector3(baseSize, baseSize, baseSize * beltBlockDepthFactor);

            BlockVisual.ApplyColor(go, BlockVisual.ToUnityColor(block.Color));

            // Avoid one-frame "flash" at world origin when a block visual is created before the belt positions update.
            if (created)
            {
                go.transform.position = GetBeltBlockWorldPosition(index);
            }
        }

        private Vector3 GetBeltBlockWorldPosition(int index)
        {
            Vector3 basePos;
            if (index >= 0 && index < _slotCurrentPositions.Count)
            {
                basePos = _slotCurrentPositions[index];
            }
            else if (index >= 0 && index < _beltSlots.Count && _beltSlots[index] != null)
            {
                basePos = _beltSlots[index].position;
            }
            else
            {
                // Fallback: move out of view rather than (0,0,0) which can be visible in gameplay.
                basePos = transform.position + new Vector3(0f, -9999f, 0f);
            }

            return basePos + GetBeltBlockOffset(index) + new Vector3(0f, 0f, beltBlockZOffset);
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

            var end = GetBeltBlockWorldPosition(beltIndex);
            var startOffset = start - end;

            StopBeltSpawnAnimation(beltIndex);
            _beltSpawnAnimating.Add(beltIndex);
            _beltSpawnCoroutines[beltIndex] = StartCoroutine(AnimateBeltSpawn(beltIndex, startOffset));
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

        private IEnumerator AnimateBeltSpawn(int beltIndex, Vector3 startOffset)
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
                float e = MotionUtil.EaseOutCubic(u);
                var end = GetBeltBlockWorldPosition(beltIndex);
                go.transform.position = end + Vector3.LerpUnclamped(startOffset, Vector3.zero, e);
                yield return null;
            }

            if (_beltBlockVisuals.TryGetValue(beltIndex, out var finalGo) && finalGo != null && beltIndex >= 0 && beltIndex < _slotCurrentPositions.Count)
            {
                finalGo.transform.position = GetBeltBlockWorldPosition(beltIndex);
                StartCoroutine(MotionUtil.ScalePunch(finalGo.transform, finalGo.transform.localScale, BeltSpawnPunchScale, BeltSpawnPunchSeconds));
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
