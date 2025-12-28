using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using LoopSorting;

public class LevelEditorWindow : EditorWindow
{
    private const string LevelsFolder = "Assets/Levels";
    private const string FlowsFolder = "Assets/Flows";
    private const string RuntimeConfigResourcesFolder = "Assets/Levels/Resources/Levels";

    private LevelLayout _level;
    private SerializedObject _serializedLevel;
    private ReorderableList _conveyorsList;
    private ReorderableList _boxesList;
    private LevelLayout[] _levelOptions = new LevelLayout[0];
    private string[] _levelOptionNames = new string[0];
    private int _selectedIndex = -1;

    // UI state
    private Vector2 _paramScroll;
    private Vector2 _levelListScroll;
    private bool _showConveyorSettings = true;
    private bool _showBoxSettings = true;
    private bool _snapToGrid = true;
    private bool _showSlotMarkers = true;
    private bool _onlyShowSelectedBox = false;
    private bool _onlyShowSelectedConveyor = false;
    private float _gridSize = 0.5f;
    private float _previewSize = 520f;
    private bool _previewPortrait916 = true;
    private bool _showRuntimeUIOverlay = true;
    private int _selectedBox = -1;
    private int _selectedConveyor = -1;
    private int _draggingConveyor = -1;
    private int _draggingPoint = -1;
    private int _draggingBox = -1;
    private Vector2 _boxDragOffset = Vector2.zero;
    private int _selectedPoint = -1;
    private int _tabIndex = 0;
    private int _lastTabIndex = 0;
    private readonly string[] _tabs = new[] { "Levels", "Flow" };
    private LevelFlow _flowAsset;
    private SerializedObject _flowSO;
    private ReorderableList _flowList;
    private LevelLayout _flowAddCandidate;
    private Vector2 _flowScroll;
    private int _lastPaletteColor = 0;
    private ReorderableList _colorCountsList;
    private int _colorListBoxIndex = -1;
    private int _selectedColorIndex = -1;
    private int _lastSelBox = -2;
    private int _lastSelConv = -2;
    private int _lastSelPoint = -2;
    private bool _showLayoutAutoFix = true;
    private bool _showCameraClamp = true;
    private LevelLayout _previewLayoutInstance;
    private LoopSorting.Editor.LevelDifficultyMetrics.FailureRateSimulation _dsrSim;
    private int _dsrSimMaxBoxes;
    private readonly Dictionary<int, float> _dsrLastRByLayoutId = new Dictionary<int, float>();
    private int _dsrSimTicksPerUpdate = 1000;
    private float _dsrSimBudgetMs = 10f;
    private bool _dsrSimRandomizeSeed = true;
    private int _dsrSimSeed;
    private bool _previewSimActive;
    private LoopSorting.Editor.LevelDifficultyMetrics.SimulationDebugSnapshot _previewSimSnapshot;
    private int _previewSimSnapshotHash;
    private bool _previewSimHoldLastSnapshot;
    private bool _showPressureGraph = true;
    private int _pressureGraphMinCap = 0;
    private int _pressureGraphMaxCap = 0;
    private int _pressureGraphStep = 1;
    private int _pressureGraphTargetCap = -1;
    private int _pressureGraphLastBeltLen = -1;
    private readonly Dictionary<int, PressureSnapshot> _pressureByLayoutId = new Dictionary<int, PressureSnapshot>();
    private bool _showStrategyPressureGraph = true;
    private StrategyCompareBatch _strategyCompareBatch;
    private readonly Dictionary<int, StrategyPressureSnapshot> _strategyPressureByLayoutId = new Dictionary<int, StrategyPressureSnapshot>();
    private static readonly LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy[] StrategyCompareOrder =
    {
        LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy.Balanced,
        LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy.Aggressive,
        LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy.Cautious
    };

    private struct PressureSnapshot
    {
        public int BeltLength;
        public int[] Peaks;
    }

    private struct StrategyPressureSnapshot
    {
        public int BeltLength;
        public Dictionary<LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy, int[]> PeaksByStrategy;
    }

    private sealed class StrategyCompareBatch
    {
        public int LayoutId;
        public int LayoutHash;
        public int SeedSalt;
        public int ActiveIndex;
        public LoopSorting.Editor.LevelDifficultyMetrics.FailureRateSimulation ActiveSim;
        public LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy[] Strategies;
        public Dictionary<LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy, int[]> PeaksByStrategy =
            new Dictionary<LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy, int[]>();
        public int BeltLength;
    }

    // Preview-only camera framing (to match GameRuntimeController.FitCameraToLevel).
    private static bool _previewCameraActive;
    private static float _previewScaleMultiplier = 1f;
    private static Vector2 _previewCameraCenterWorld;

    [MenuItem("Tools/Loop Sorting/Level Editor")]
    public static void Open()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnEnable()
    {
        RefreshLevelList();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        CancelDsrSimulation();
        CancelStrategyCompareSimulation();
        if (_previewLayoutInstance != null)
        {
            DestroyImmediate(_previewLayoutInstance);
            _previewLayoutInstance = null;
        }
    }

    private void OnGUI()
    {
        DrawHeader();

        HandleGlobalHotkeys();

        EditorGUILayout.BeginHorizontal();
        DrawLevelSidebar();

        _tabIndex = GUILayout.Toolbar(_tabIndex, _tabs);
        if (_tabIndex != _lastTabIndex)
        {
            CancelDsrSimulation();
            CancelStrategyCompareSimulation();
            _lastTabIndex = _tabIndex;
        }

        if (_tabIndex == 0)
        {
            if (_level == null)
            {
                DrawCreateButtons();
            }
            else
            {
                UpdateBoxSizesFromBlockSize();
                if (_serializedLevel == null)
                {
                    BindSerializedObject();
                }

                _serializedLevel.Update();
                DrawPreviewPanel();
                DrawParameterPanel();
                _serializedLevel.ApplyModifiedProperties();
            }
        }
        else
        {
            DrawFlowPanel();
        }
        EditorGUILayout.EndHorizontal();

        // clear keyboard focus if selection changed (avoid stale input on new target)
        if (_selectedBox != _lastSelBox || _selectedConveyor != _lastSelConv || _selectedPoint != _lastSelPoint)
        {
            ClearEditorInputFocus();
            _lastSelBox = _selectedBox;
            _lastSelConv = _selectedConveyor;
            _lastSelPoint = _selectedPoint;
        }
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Loop Sorting Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Select Level (Assets/Levels)", EditorStyles.miniBoldLabel);
        var nextIndex = EditorGUILayout.Popup("Level Layout", _selectedIndex, _levelOptionNames);
        if (nextIndex != _selectedIndex && nextIndex >= 0 && nextIndex < _levelOptions.Length)
        {
            SetLevel(_levelOptions[nextIndex]);
        }

        EditorGUILayout.BeginHorizontal();
        var nextLevel = (LevelLayout)EditorGUILayout.ObjectField("Level Asset", _level, typeof(LevelLayout), false);
        if (nextLevel != _level)
        {
            SetLevel(nextLevel);
        }

        if (GUILayout.Button("Refresh List", GUILayout.Width(80)))
        {
            RefreshLevelList();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        if (GUILayout.Button("New", GUILayout.Width(60)))
        {
            CreateNewLevelAsset();
        }

        if (_level != null && GUILayout.Button("Ping", GUILayout.Width(60)))
        {
            EditorGUIUtility.PingObject(_level);
        }

        if (_level != null && GUILayout.Button("Save", GUILayout.Width(60)))
        {
            SaveCurrentLevel();
        }

        if (_level != null && GUILayout.Button("Set Runtime Level", GUILayout.Width(100)))
        {
            SetActiveRuntimeLevel(_level);
        }
        using (new EditorGUI.DisabledScope(_level == null))
        {
            if (GUILayout.Button("Jump To This Level (Play)", GUILayout.Width(140)))
            {
                JumpToCurrentLevelAtRuntime();
            }
        }
        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("Trigger Lose (Play)", GUILayout.Width(140)))
            {
                TriggerRuntimeLose();
            }
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
    }

    private void DrawLevelSidebar()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200f));
        DrawLevelListPanel();
        EditorGUILayout.EndVertical();
    }

    private void DrawParameterPanel()
    {
        if (_level == null) return;
        if (_serializedLevel == null)
        {
            BindSerializedObject();
            if (_serializedLevel == null) return;
        }

        EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.45f));
        _paramScroll = EditorGUILayout.BeginScrollView(_paramScroll);

        DrawDifficultySummary();
        bool simRunning = IsSimRunning();
        if (simRunning)
        {
            EditorGUILayout.HelpBox("Simulation running: editing is disabled.", MessageType.Info);
        }
        using (new EditorGUI.DisabledScope(simRunning))
        {

        EditorGUILayout.LabelField("Global", EditorStyles.boldLabel);
        var propBlockSize = _serializedLevel.FindProperty("blockSize");
        if (propBlockSize != null)
        {
            EditorGUILayout.PropertyField(propBlockSize, new GUIContent("Block Edge Size"));
        }
        else
        {
            EditorGUILayout.HelpBox("Missing property: blockSize", MessageType.Error);
        }
        if (GUILayout.Button("Add Conveyor", GUILayout.Width(120)))
        {
            AddDefaultConveyor();
        }
        EditorGUILayout.Space(4f);

        _showConveyorSettings = EditorGUILayout.Foldout(_showConveyorSettings, "Conveyor Settings", true);
        if (_showConveyorSettings)
        {
            EditorGUI.indentLevel++;
            var propCap = _serializedLevel.FindProperty("beltCapacity");
            var propSpacing = _serializedLevel.FindProperty("beltSlotSpacing");
            var propSmooth = _serializedLevel.FindProperty("smoothCorners");
            var propTension = _serializedLevel.FindProperty("cornerSmoothTension");
            var propSubdiv = _serializedLevel.FindProperty("cornerSubdivisions");

            if (propCap != null) EditorGUILayout.PropertyField(propCap, new GUIContent("Belt Capacity (0=default 50)"));
            if (propSpacing != null) EditorGUILayout.PropertyField(propSpacing, new GUIContent("Belt Slot Spacing (units, 0=default 0.6)"));
            if (propSmooth != null) EditorGUILayout.PropertyField(propSmooth, new GUIContent("Smooth Corners"));
            if (propSmooth != null && propSmooth.boolValue)
            {
                if (propTension != null) EditorGUILayout.PropertyField(propTension, new GUIContent("Corner Smooth Tension"));
                if (propSubdiv != null) EditorGUILayout.PropertyField(propSubdiv, new GUIContent("Corner Subdivisions"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4f);
        }

        _showLayoutAutoFix = EditorGUILayout.Foldout(_showLayoutAutoFix, "Layout Auto Fix", true);
        if (_showLayoutAutoFix)
        {
            EditorGUI.indentLevel++;
            var propOverride = _serializedLevel.FindProperty("overrideLayoutAutoSettings");
            var propAuto = _serializedLevel.FindProperty("autoResolveLayoutOverlap");
            var propMinGap = _serializedLevel.FindProperty("minBoxToBeltGap");
            var propPreferredGap = _serializedLevel.FindProperty("preferredBoxToBeltGap");
            var propMinBoxGap = _serializedLevel.FindProperty("minBoxToBoxGap");
            var propIterations = _serializedLevel.FindProperty("overlapResolveIterations");

            if (propOverride != null)
            {
                EditorGUILayout.PropertyField(propOverride, new GUIContent("Override Runtime Defaults"));
            }
            bool overrideLayout = propOverride != null && propOverride.boolValue;
            using (new EditorGUI.DisabledScope(!overrideLayout))
            {
                if (propAuto != null) EditorGUILayout.PropertyField(propAuto, new GUIContent("Auto Resolve Overlap"));
                if (propAuto != null && propAuto.boolValue)
                {
                    if (propMinGap != null) EditorGUILayout.PropertyField(propMinGap, new GUIContent("Min Gap"));
                    if (propPreferredGap != null) EditorGUILayout.PropertyField(propPreferredGap, new GUIContent("Preferred Gap"));
                    if (propMinBoxGap != null) EditorGUILayout.PropertyField(propMinBoxGap, new GUIContent("Min Box Gap"));
                    if (propIterations != null) EditorGUILayout.PropertyField(propIterations, new GUIContent("Resolve Iterations"));
                }
            }
            if (!overrideLayout)
            {
                EditorGUILayout.HelpBox("Using runtime defaults. Enable override to customize per-level and preview.", MessageType.Info);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4f);
        }

        _showCameraClamp = EditorGUILayout.Foldout(_showCameraClamp, "Camera Clamp", true);
        if (_showCameraClamp)
        {
            EditorGUI.indentLevel++;
            var propOverride = _serializedLevel.FindProperty("overrideLayoutAutoSettings");
            var propMaxOrtho = _serializedLevel.FindProperty("cameraMaxOrthoSize");
            var propMinBlockPx = _serializedLevel.FindProperty("minBlockPixelSize");
            bool overrideLayout = propOverride != null && propOverride.boolValue;
            using (new EditorGUI.DisabledScope(!overrideLayout))
            {
                if (propMaxOrtho != null) EditorGUILayout.PropertyField(propMaxOrtho, new GUIContent("Max Ortho Size"));
                if (propMinBlockPx != null) EditorGUILayout.PropertyField(propMinBlockPx, new GUIContent("Min Block Pixel Size"));
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4f);
        }

        EditorGUILayout.LabelField("Drag Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _snapToGrid = EditorGUILayout.ToggleLeft("Snap to Grid (0.5)", _snapToGrid, GUILayout.Width(150));
        _showSlotMarkers = EditorGUILayout.ToggleLeft("Show Slots", _showSlotMarkers, GUILayout.Width(120));
        _onlyShowSelectedBox = EditorGUILayout.ToggleLeft("Only Selected Box", _onlyShowSelectedBox, GUILayout.Width(150));
        _onlyShowSelectedConveyor = EditorGUILayout.ToggleLeft("Only Selected Conveyor", _onlyShowSelectedConveyor, GUILayout.Width(170));
        EditorGUILayout.EndHorizontal();
        _gridSize = EditorGUILayout.Slider("Grid Size", _gridSize, 0.1f, 2f);
        EditorGUILayout.Space();

        DrawSelectedItemPanel();

        _showBoxSettings = EditorGUILayout.Foldout(_showBoxSettings, "Boxes", true);
        if (_showBoxSettings)
        {
            if (_boxesList == null)
            {
                var property = _serializedLevel.FindProperty("boxes");
                if (property != null)
                {
                    _boxesList = new ReorderableList(_serializedLevel, property, true, true, true, true)
                    {
                        drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Boxes (size=cols*rows*blockSize, capacity=cols*rows)"),
                        drawElementCallback = DrawBoxElement,
                        elementHeightCallback = GetBoxHeight,
                        onAddCallback = list =>
                        {
                            if (_level == null)
                                return;
                            Undo.RecordObject(_level, "Add Box");
                            var newBox = new BoxSpec
                            {
                                name = $"Box {_level.boxes.Count + 1}",
                                position = Vector2.zero,
                                size = Vector2.one,
                                color = Color.white,
                                columns = 1,
                                rows = 1,
                                opening = OpeningSide.Top,
                                colorCounts = new List<ColorCount>()
                            };
                            _level.boxes.Add(newBox);
                            _selectedBox = _level.boxes.Count - 1;
                            _selectedConveyor = -1;
                            EditorUtility.SetDirty(_level);
                            _boxesList.index = _selectedBox;
                        }
                    };
                }
                else
                {
                    EditorGUILayout.HelpBox("Missing property: boxes", MessageType.Error);
                }
            }
            _boxesList?.DoLayoutList();
        }

        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void DrawDifficultySummary()
    {
        if (_level == null) return;
        int maxBoxes = GetMaxBoxCount();
        var metrics = LoopSorting.Editor.LevelDifficultyMetrics.ComputeStatic(_level, maxBoxes);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Difficulty (DSR)", EditorStyles.boldLabel);
        float displayR = metrics.R;
        int layoutId = _level.GetInstanceID();
        if (displayR < 0f && _dsrLastRByLayoutId.TryGetValue(layoutId, out var lastR))
        {
            displayR = lastR;
        }
        else if (metrics.R >= 0f)
        {
            _dsrLastRByLayoutId[layoutId] = metrics.R;
        }

        float displayD = displayR >= 0f ? Mathf.Clamp01(0.6f * metrics.S + 0.4f * displayR) : -1f;
        string dText = displayD < 0f ? "--" : displayD.ToString("0.00");
        string rText = displayR < 0f ? "--" : displayR.ToString("0.00");
        EditorGUILayout.LabelField($"D {dText}   S {metrics.S:0.00}   R {rText}");
        EditorGUILayout.LabelField($"Boxes {metrics.Boxes}  Colors {metrics.Colors}  Blocks {metrics.Blocks}  BeltCap {metrics.BeltCapacity}");
        EditorGUILayout.BeginHorizontal();
        _dsrSimTicksPerUpdate = Mathf.Clamp(EditorGUILayout.IntField("Sim Speed (ticks/update)", _dsrSimTicksPerUpdate), 1, 100000);
        _dsrSimBudgetMs = Mathf.Clamp(EditorGUILayout.FloatField("Budget (ms)", _dsrSimBudgetMs), 0.1f, 200f);
        if (_dsrSim == null || _dsrSim.IsDone)
        {
            if (GUILayout.Button("Recompute R", GUILayout.Width(120)))
            {
                StartDsrSimulation();
            }
        }
        else
        {
            if (GUILayout.Button("Cancel R Sim", GUILayout.Width(120)))
            {
                CancelDsrSimulation();
            }
            EditorGUILayout.LabelField($"Running {_dsrSim.RunsCompleted}/{_dsrSim.RunsTotal}", GUILayout.Width(160));
        }
        EditorGUILayout.EndHorizontal();
        _dsrSimRandomizeSeed = EditorGUILayout.ToggleLeft("Randomize Seed", _dsrSimRandomizeSeed);
        using (new EditorGUI.DisabledScope(_dsrSimRandomizeSeed))
        {
            _dsrSimSeed = EditorGUILayout.IntField("Seed", _dsrSimSeed);
        }

        DrawPressureGraphSection(layoutId);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(4f);
    }

    private void DrawPressureGraphSection(int layoutId)
    {
        _showPressureGraph = EditorGUILayout.Foldout(_showPressureGraph, "Pressure Graph", true);
        if (!_showPressureGraph)
        {
            return;
        }

        if (!_pressureByLayoutId.TryGetValue(layoutId, out var snapshot) ||
            snapshot.Peaks == null || snapshot.Peaks.Length == 0)
        {
            EditorGUILayout.HelpBox("Run the simulation to generate pressure data.", MessageType.Info);
            return;
        }

        if (_pressureGraphLastBeltLen != snapshot.BeltLength)
        {
            _pressureGraphMinCap = 0;
            _pressureGraphMaxCap = snapshot.BeltLength;
            _pressureGraphStep = 1;
            int defaultTarget = _level != null && _level.beltCapacity > 0 ? _level.beltCapacity : snapshot.BeltLength;
            _pressureGraphTargetCap = Mathf.Clamp(defaultTarget, 0, snapshot.BeltLength);
            _pressureGraphLastBeltLen = snapshot.BeltLength;
        }

        _pressureGraphMinCap = Mathf.Clamp(EditorGUILayout.IntField("Min Used", _pressureGraphMinCap), 0, snapshot.BeltLength);
        _pressureGraphMaxCap = Mathf.Clamp(EditorGUILayout.IntField("Max Used", _pressureGraphMaxCap), _pressureGraphMinCap + 1, snapshot.BeltLength);
        _pressureGraphStep = Mathf.Clamp(EditorGUILayout.IntField("Step", _pressureGraphStep), 1, Mathf.Max(1, _pressureGraphMaxCap - _pressureGraphMinCap));
        _pressureGraphTargetCap = Mathf.Clamp(EditorGUILayout.IntField("Target Used", _pressureGraphTargetCap), _pressureGraphMinCap, _pressureGraphMaxCap);

        int runs = snapshot.Peaks.Length;
        int maxPeak = 0;
        for (int i = 0; i < snapshot.Peaks.Length; i++)
        {
            if (snapshot.Peaks[i] > maxPeak) maxPeak = snapshot.Peaks[i];
        }
        EditorGUILayout.LabelField($"Runs {runs}  BeltSlots {snapshot.BeltLength}  PeakMax {maxPeak}");

        int sampleCount = ((_pressureGraphMaxCap - _pressureGraphMinCap) / _pressureGraphStep) + 1;
        var counts = new int[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            int cap = _pressureGraphMinCap + i * _pressureGraphStep;
            int over = 0;
            for (int r = 0; r < snapshot.Peaks.Length; r++)
            {
                if (snapshot.Peaks[r] > cap) over++;
            }
            counts[i] = over;
        }

        float graphHeight = 120f;
        var rect = GUILayoutUtility.GetRect(1f, graphHeight);
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 0.2f));

        Handles.BeginGUI();
        var lineColor = new Color(0.2f, 0.8f, 0.9f, 1f);
        Handles.color = lineColor;
        for (int i = 1; i < counts.Length; i++)
        {
            float x0 = rect.x + (i - 1) / (float)(counts.Length - 1) * rect.width;
            float x1 = rect.x + i / (float)(counts.Length - 1) * rect.width;
            float y0 = rect.yMax - (counts[i - 1] / (float)runs) * rect.height;
            float y1 = rect.yMax - (counts[i] / (float)runs) * rect.height;
            Handles.DrawLine(new Vector3(x0, y0), new Vector3(x1, y1));
        }

        float capRange = Mathf.Max(1f, _pressureGraphMaxCap - _pressureGraphMinCap);
        float targetNorm = (_pressureGraphTargetCap - _pressureGraphMinCap) / capRange;
        float targetX = rect.x + targetNorm * rect.width;
        Handles.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        Handles.DrawLine(new Vector3(targetX, rect.y), new Vector3(targetX, rect.yMax));
        Handles.EndGUI();

        var labelStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            normal = { textColor = new Color(0.95f, 0.95f, 0.95f, 0.9f) }
        };
        for (int i = 0; i < counts.Length; i++)
        {
            bool turning = i == 0 || i == counts.Length - 1 || counts[i] != counts[i - 1];
            if (!turning) continue;
            int cap = _pressureGraphMinCap + i * _pressureGraphStep;
            float pct = runs > 0 ? (counts[i] / (float)runs) * 100f : 0f;
            float x = rect.x + (i / (float)(counts.Length - 1)) * rect.width;
            float y = rect.yMax - (counts[i] / (float)runs) * rect.height;
            var dot = new Rect(x - 2f, y - 2f, 4f, 4f);
            EditorGUI.DrawRect(dot, new Color(0.95f, 0.95f, 0.95f, 0.9f));
            var label = $"x={cap} y={pct:0.#}%";
            var labelRect = new Rect(x + 6f, y - 10f, 120f, 16f);
            GUI.Label(labelRect, label, labelStyle);
        }

        int overTarget = 0;
        for (int r = 0; r < snapshot.Peaks.Length; r++)
        {
            if (snapshot.Peaks[r] > _pressureGraphTargetCap) overTarget++;
        }
        float overPct = runs > 0 ? (overTarget / (float)runs) * 100f : 0f;
        EditorGUILayout.LabelField("y = runs with peak used > x (percent), x = used slots");
        EditorGUILayout.LabelField($"Target { _pressureGraphTargetCap }: {overTarget}/{runs}  ({overPct:0.0}%)");

        DrawStrategyPressureSection(layoutId);
    }

    private void DrawStrategyPressureSection(int layoutId)
    {
        _showStrategyPressureGraph = EditorGUILayout.Foldout(_showStrategyPressureGraph, "Strategy Pressure", true);
        if (!_showStrategyPressureGraph)
        {
            return;
        }

        bool running = IsStrategyCompareRunning();
        EditorGUILayout.BeginHorizontal();
        if (!running)
        {
            if (GUILayout.Button("Run Strategy Compare", GUILayout.Width(160)))
            {
                StartStrategyCompareSimulation();
            }
        }
        else
        {
            if (GUILayout.Button("Cancel Strategy Compare", GUILayout.Width(160)))
            {
                CancelStrategyCompareSimulation();
            }
            if (_strategyCompareBatch != null && _strategyCompareBatch.Strategies != null &&
                _strategyCompareBatch.ActiveIndex >= 0 &&
                _strategyCompareBatch.ActiveIndex < _strategyCompareBatch.Strategies.Length)
            {
                var activeStrategy = _strategyCompareBatch.Strategies[_strategyCompareBatch.ActiveIndex];
                string name = LoopSorting.Editor.LevelDifficultyMetrics.GetStrategyName(activeStrategy);
                EditorGUILayout.LabelField($"Running {name} ({_strategyCompareBatch.ActiveIndex + 1}/{_strategyCompareBatch.Strategies.Length})");
            }
        }
        EditorGUILayout.EndHorizontal();

        if (!_strategyPressureByLayoutId.TryGetValue(layoutId, out var snapshot) ||
            snapshot.PeaksByStrategy == null || snapshot.PeaksByStrategy.Count == 0)
        {
            EditorGUILayout.HelpBox("Run the strategy comparison to see per-strategy belt usage.", MessageType.Info);
            return;
        }

        int beltLength = snapshot.BeltLength > 0 ? snapshot.BeltLength : _pressureGraphLastBeltLen;
        int minCap = Mathf.Clamp(_pressureGraphMinCap, 0, beltLength);
        int maxCap = Mathf.Clamp(_pressureGraphMaxCap, minCap + 1, beltLength);
        int step = Mathf.Clamp(_pressureGraphStep, 1, Mathf.Max(1, maxCap - minCap));
        int sampleCount = ((maxCap - minCap) / step) + 1;

        float graphHeight = 120f;
        var rect = GUILayoutUtility.GetRect(1f, graphHeight);
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 0.2f));

        Handles.BeginGUI();
        for (int s = 0; s < StrategyCompareOrder.Length; s++)
        {
            var strategy = StrategyCompareOrder[s];
            if (!snapshot.PeaksByStrategy.TryGetValue(strategy, out var peaks) || peaks == null || peaks.Length == 0)
            {
                continue;
            }

            var counts = new int[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                int cap = minCap + i * step;
                int over = 0;
                for (int r = 0; r < peaks.Length; r++)
                {
                    if (peaks[r] > cap) over++;
                }
                counts[i] = over;
            }

            Handles.color = GetStrategyColor(strategy);
            for (int i = 1; i < counts.Length; i++)
            {
                float x0 = rect.x + (i - 1) / (float)(counts.Length - 1) * rect.width;
                float x1 = rect.x + i / (float)(counts.Length - 1) * rect.width;
                float y0 = rect.yMax - (counts[i - 1] / (float)peaks.Length) * rect.height;
                float y1 = rect.yMax - (counts[i] / (float)peaks.Length) * rect.height;
                Handles.DrawLine(new Vector3(x0, y0), new Vector3(x1, y1));
            }
        }

        float capRange = Mathf.Max(1f, maxCap - minCap);
        float targetNorm = (_pressureGraphTargetCap - minCap) / capRange;
        float targetX = rect.x + targetNorm * rect.width;
        Handles.color = new Color(1f, 0.8f, 0.2f, 0.9f);
        Handles.DrawLine(new Vector3(targetX, rect.y), new Vector3(targetX, rect.yMax));
        Handles.EndGUI();

        for (int s = 0; s < StrategyCompareOrder.Length; s++)
        {
            var strategy = StrategyCompareOrder[s];
            if (!snapshot.PeaksByStrategy.TryGetValue(strategy, out var peaks) || peaks == null || peaks.Length == 0)
            {
                continue;
            }

            float avgPeak = ComputeAverage(peaks);
            int p90 = ComputePercentile(peaks, 0.9f);
            string name = LoopSorting.Editor.LevelDifficultyMetrics.GetStrategyName(strategy);
            EditorGUILayout.LabelField($"{name}: runs {peaks.Length}  avg {avgPeak:0.0}  p90 {p90}");
        }

        EditorGUILayout.LabelField("y = runs with peak used > x (percent), x = used slots");
    }

    private static Color GetStrategyColor(LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy strategy)
    {
        switch (strategy)
        {
            case LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy.Aggressive:
                return new Color(1f, 0.6f, 0.2f, 1f);
            case LoopSorting.Editor.LevelDifficultyMetrics.SimStrategy.Cautious:
                return new Color(0.3f, 0.85f, 0.35f, 1f);
            default:
                return new Color(0.2f, 0.8f, 0.9f, 1f);
        }
    }

    private static float ComputeAverage(int[] values)
    {
        if (values == null || values.Length == 0) return 0f;
        long sum = 0;
        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }
        return sum / (float)values.Length;
    }

    private static int ComputePercentile(int[] values, float percentile)
    {
        if (values == null || values.Length == 0) return 0;
        var copy = new int[values.Length];
        System.Array.Copy(values, copy, values.Length);
        System.Array.Sort(copy);
        float t = Mathf.Clamp01(percentile);
        int idx = Mathf.Clamp(Mathf.RoundToInt((copy.Length - 1) * t), 0, copy.Length - 1);
        return copy[idx];
    }

    private int GetMaxBoxCount()
    {
        int max = 0;
        if (_levelOptions != null)
        {
            foreach (var lvl in _levelOptions)
            {
                if (lvl == null || lvl.boxes == null) continue;
                if (lvl.boxes.Count > max) max = lvl.boxes.Count;
            }
        }
        return Mathf.Max(1, max);
    }

    private bool IsSimRunning()
    {
        return _dsrSim != null && !_dsrSim.IsDone;
    }

    private void DrawSelectedItemPanel()
    {
        if (_level == null || _level.boxes == null || _level.conveyors == null) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Selected", EditorStyles.boldLabel);
        if (_selectedBox >= 0 && _selectedBox < _level.boxes.Count)
        {
        var box = _level.boxes[_selectedBox];
        EditorGUI.BeginChangeCheck();
        box.name = EditorGUILayout.TextField("Name", box.name);
        box.position = EditorGUILayout.Vector2Field("Position", box.position);
        box.size = EditorGUILayout.Vector2Field("Size (w,h)", box.size);
        box.opening = (OpeningSide)EditorGUILayout.EnumPopup("Opening", box.opening);
        box.rows = Mathf.Max(1, EditorGUILayout.IntField("Rows (b)", box.rows));
        box.columns = Mathf.Max(1, EditorGUILayout.IntField("Columns (a)", box.columns));
        box.autoAlignSlot = EditorGUILayout.Toggle("Auto Align Slot", box.autoAlignSlot);
        box.beltSlotIndex = EditorGUILayout.IntField("Belt Slot Index", box.beltSlotIndex);
        box.locked = EditorGUILayout.Toggle("Locked", box.locked);
        if (box.locked)
        {
            box.unlockColor = (BlockColor)EditorGUILayout.EnumPopup("Unlock Color", box.unlockColor);
        }

        // Color counts quick edit
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Hotkeys: 1-9 set color, mouse wheel changes count, Alt+click eyedropper, Tab switches box. Drag to reorder (outer->inner).", MessageType.None);
        EnsureColorList(box);
        _colorCountsList?.DoLayoutList();
        EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Color", GUILayout.Width(90)))
            {
                box.colorCounts.Add(new ColorCount { color = (BlockColor)_lastPaletteColor, count = 1 });
                NormalizeColorCounts(box);
                EnsureColorList(box, force: true);
            }
            if (GUILayout.Button("Normalize", GUILayout.Width(90)))
            {
                NormalizeColorCounts(box);
                EnsureColorList(box, force: true);
            }
            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                box.colorCounts.Clear();
                EnsureColorList(box, force: true);
            }
        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(_level, "Edit Box");
                _level.boxes[_selectedBox] = box;
                EditorUtility.SetDirty(_level);
            }
        }
        else if (_selectedConveyor >= 0 && _selectedConveyor < _level.conveyors.Count)
        {
            var conv = _level.conveyors[_selectedConveyor];
            EditorGUI.BeginChangeCheck();
            conv.width = EditorGUILayout.FloatField("Width", conv.width);
            conv.loop = EditorGUILayout.Toggle("Loop", conv.loop);
            EditorGUILayout.LabelField("Points", conv.points != null ? conv.points.Count.ToString() : "0");
            if (conv.points != null && conv.points.Count > 0)
            {
                if (_selectedPoint < 0 || _selectedPoint >= conv.points.Count) _selectedPoint = 0;
                _selectedPoint = EditorGUILayout.IntSlider("Selected Point", _selectedPoint, 0, conv.points.Count - 1);
                var p = conv.points[_selectedPoint];
                p = EditorGUILayout.Vector2Field($"P{_selectedPoint}", p);
                if (GUILayout.Button("Add Point After Selected"))
                {
                    Undo.RecordObject(_level, "Add Conveyor Point");
                    var insertAt = _selectedPoint + 1;
                    Vector2 baseP = conv.points[_selectedPoint];
                    Vector2 next = conv.points[Mathf.Min(conv.points.Count - 1, _selectedPoint + 1)];
                    Vector2 newP = Vector2.Lerp(baseP, next, 0.5f);
                    conv.points.Insert(insertAt, newP);
                    _selectedPoint = insertAt;
                    EditorUtility.SetDirty(_level);
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add Point At End"))
                {
                    Undo.RecordObject(_level, "Add Conveyor Point");
                    var tail = conv.points[conv.points.Count - 1];
                    var before = conv.points[Mathf.Max(0, conv.points.Count - 2)];
                    var dir = (tail - before);
                    if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right * 0.5f;
                    var newP = tail + dir.normalized * 0.5f;
                    conv.points.Add(newP);
                    _selectedPoint = conv.points.Count - 1;
                    EditorUtility.SetDirty(_level);
                }
                GUI.enabled = conv.points.Count > 2;
                if (GUILayout.Button("Remove Selected"))
                {
                    Undo.RecordObject(_level, "Remove Conveyor Point");
                    conv.points.RemoveAt(_selectedPoint);
                    _selectedPoint = Mathf.Clamp(_selectedPoint, 0, conv.points.Count - 1);
                    EditorUtility.SetDirty(_level);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Edit Conveyor");
                    conv.points[_selectedPoint] = p;
                    _level.conveyors[_selectedConveyor] = conv;
                    EditorUtility.SetDirty(_level);
                }
                else
                {
                    EditorUtility.SetDirty(_level);
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_level, "Edit Conveyor");
                _level.conveyors[_selectedConveyor] = conv;
                EditorUtility.SetDirty(_level);
            }
        }
        else
        {
            EditorGUILayout.LabelField("Click a box or conveyor in preview to edit.");
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void DrawLevelListPanel()
    {
        if (_levelOptions == null || _levelOptions.Length == 0) return;
        EditorGUILayout.LabelField("Level List", EditorStyles.boldLabel);
        _levelListScroll = EditorGUILayout.BeginScrollView(_levelListScroll);
        int cols = 3;
        int idx = 0;
        while (idx < _levelOptions.Length)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < cols && idx < _levelOptions.Length; c++, idx++)
            {
                var lvl = _levelOptions[idx];
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = (lvl == _level) ? Color.green : Color.white;
                if (GUILayout.Button(lvl != null ? lvl.name : "NULL"))
                {
                    SetLevel(lvl);
                }
                GUI.backgroundColor = prev;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawPreviewPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(_previewSize));
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        DrawColorLegend();
        var rect = GUILayoutUtility.GetRect(_previewSize, _previewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        _previewPortrait916 = EditorGUILayout.ToggleLeft("9:16 Frame", _previewPortrait916);
        _showRuntimeUIOverlay = EditorGUILayout.ToggleLeft("Show Runtime UI Overlay", _showRuntimeUIOverlay);
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 0.4f));
        }

        if (_level == null)
        {
            EditorGUI.LabelField(rect, "Select or create a LevelLayout", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        if (_previewLayoutInstance != null)
        {
            DestroyImmediate(_previewLayoutInstance);
            _previewLayoutInstance = null;
        }

        GetEffectiveLayoutSettings(
            _level,
            out bool autoResolve,
            out float minGap,
            out float preferredGap,
            out float minBoxToBoxGap,
            out int iterations,
            out float cameraMaxOrtho,
            out float minBlockPixel,
            out float fallbackBeltSpacing);

        var previewLayout = _level;
        if (autoResolve && (minGap > 0f || preferredGap > 0f || minBoxToBoxGap > 0f))
        {
            previewLayout = LayoutUtils.CloneLayout(_level);
            int fixIterations = Mathf.Clamp(iterations, 1, 8);
            for (int i = 0; i < fixIterations; i++)
            {
                bool moved = false;
                if (minGap > 0f || preferredGap > 0f)
                {
                    moved |= LayoutUtils.ResolveBoxBeltOverlap(
                        previewLayout,
                        minGap,
                        preferredGap,
                        fallbackBeltSpacing,
                        1) > 0;
                }
                if (minBoxToBoxGap > 0f)
                {
                    moved |= LayoutUtils.ResolveBoxBoxOverlap(previewLayout, minBoxToBoxGap, 1) > 0;
                }
                if (!moved)
                {
                    break;
                }
            }
            _previewLayoutInstance = previewLayout;
        }

        var bounds = ComputeBounds(previewLayout);
        var localRect = new Rect(0, 0, rect.width, rect.height);
        var frameRect = _previewPortrait916 ? FitAspect(localRect, 9f / 16f) : localRect;

        // Match runtime camera framing (GameRuntimeController.FitCameraToLevel):
        // scale is multiplied by "available", and camera is shifted in world space so the level center
        // appears centered within the available region.
        var uiLayout = LoopSortingUIKit.GetRuntimeLayout();
        float top = Mathf.Clamp01(uiLayout.reservedTop);
        float bottom = Mathf.Clamp01(uiLayout.reservedBottom);
        float available = Mathf.Clamp01(1f - top - bottom);
        if (available < 0.35f) available = 0.35f;

        _previewCameraActive = true;
        _previewScaleMultiplier = available;
        _previewCameraCenterWorld = bounds.center;
        {
            // Compute ortho size exactly like runtime does (with auto padding already baked into bounds).
            float aspect = Mathf.Max(0.0001f, frameRect.width / Mathf.Max(0.0001f, frameRect.height));
            float w = Mathf.Max(0.0001f, bounds.width);
            float h = Mathf.Max(0.0001f, bounds.height);
            float baseOrtho = Mathf.Max(h * 0.5f, w * 0.5f / aspect);
            float orthoSize = baseOrtho / available;

            if (minBlockPixel > 0f)
            {
                float unit = previewLayout != null && previewLayout.blockSize > 0f ? previewLayout.blockSize : 0.6f;
                float maxOrtho = unit * frameRect.height / (2f * minBlockPixel);
                if (maxOrtho > 0.0001f)
                {
                    orthoSize = Mathf.Min(orthoSize, maxOrtho);
                }
            }
            if (cameraMaxOrtho > 0f)
            {
                orthoSize = Mathf.Min(orthoSize, cameraMaxOrtho);
            }

            float baseScale = Mathf.Min(frameRect.width / w, frameRect.height / h);
            float targetScale = frameRect.height / (2f * Mathf.Max(0.0001f, orthoSize));
            _previewScaleMultiplier = baseScale > 0.0001f ? (targetScale / baseScale) : 1f;

            float desiredCenterY01 = bottom + available * 0.5f;
            float delta01 = desiredCenterY01 - 0.5f;
            float worldOffsetY = delta01 * (2f * orthoSize);
            _previewCameraCenterWorld = bounds.center + new Vector2(0f, worldOffsetY);
        }

        GUI.BeginClip(rect);
        Handles.BeginGUI();
        var slotPositions = BuildSlotPositionsForPreview(previewLayout, fallbackBeltSpacing);
        var simSnap = default(LoopSorting.Editor.LevelDifficultyMetrics.SimulationDebugSnapshot);
        bool simRunning = IsSimRunning();
        bool hasNewSnap = false;
        if (simRunning && _dsrSim != null && _dsrSim.TryGetDebugSnapshot(out simSnap))
        {
            _previewSimSnapshot = simSnap;
            _previewSimSnapshotHash = LoopSorting.Editor.LevelDifficultyMetrics.GetLayoutHash(_level);
            hasNewSnap = true;
        }
        bool hasSnapshot = hasNewSnap || _previewSimSnapshot.BeltSlots != null;
        bool showSim = false;
        if (simRunning)
        {
            showSim = hasSnapshot;
        }
        else if (_previewSimHoldLastSnapshot && hasSnapshot && _previewSimSnapshotHash != 0)
        {
            showSim = _previewSimSnapshotHash == LoopSorting.Editor.LevelDifficultyMetrics.GetLayoutHash(_level);
        }
        _previewSimActive = showSim;

        // Frame background/border.
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(frameRect, new Color(0f, 0f, 0f, 0.18f));
            DrawRectOutline(frameRect, new Color(1f, 1f, 1f, 0.35f), 2f);
        }

        if (_showRuntimeUIOverlay)
        {
            DrawRuntimeUIOverlay(frameRect);
        }

        DrawPreviewConveyors(frameRect, bounds, slotPositions, previewLayout);
        if (showSim)
        {
            DrawPreviewSimBelt(frameRect, bounds, slotPositions, previewLayout, _previewSimSnapshot);
        }
        DrawPreviewBoxes(frameRect, bounds, slotPositions, previewLayout);
        if (showSim)
        {
            DrawPreviewSimOverlay(frameRect, _previewSimSnapshot);
        }
        HandlePreviewClick(frameRect, bounds, slotPositions, previewLayout);
        HandlePreviewDrag(frameRect, bounds);
        Handles.EndGUI();
        GUI.EndClip();

        _previewCameraActive = false;
        _previewScaleMultiplier = 1f;
        _previewCameraCenterWorld = Vector2.zero;
        EditorGUILayout.EndVertical();
    }

    private static Rect FitAspect(Rect outer, float aspectWOverH)
    {
        aspectWOverH = Mathf.Max(0.0001f, aspectWOverH);
        float targetW = outer.height * aspectWOverH;
        float targetH = outer.height;
        if (targetW > outer.width)
        {
            targetW = outer.width;
            targetH = targetW / aspectWOverH;
        }

        float x = outer.x + (outer.width - targetW) * 0.5f;
        float y = outer.y + (outer.height - targetH) * 0.5f;
        return new Rect(x, y, targetW, targetH);
    }

    private static void DrawRectOutline(Rect r, Color c, float thickness)
    {
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, thickness), c);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - thickness, r.width, thickness), c);
        EditorGUI.DrawRect(new Rect(r.x, r.y, thickness, r.height), c);
        EditorGUI.DrawRect(new Rect(r.xMax - thickness, r.y, thickness, r.height), c);
    }

    private void DrawRuntimeUIOverlay(Rect frame)
    {
        var uiLayout = LoopSortingUIKit.GetRuntimeLayout();
        float reservedTop = Mathf.Clamp01(uiLayout.reservedTop);
        float reservedBottom = Mathf.Clamp01(uiLayout.reservedBottom);

        float topH = frame.height * reservedTop;
        float bottomH = frame.height * reservedBottom;
        var topRect = new Rect(frame.x, frame.y, frame.width, topH);
        var bottomRect = new Rect(frame.x, frame.yMax - bottomH, frame.width, bottomH);

        EditorGUI.DrawRect(topRect, new Color(0f, 0f, 0f, 0.14f));
        EditorGUI.DrawRect(bottomRect, new Color(0f, 0f, 0f, 0.16f));
        DrawRectOutline(topRect, new Color(1f, 1f, 1f, 0.10f), 1f);
        DrawRectOutline(bottomRect, new Color(1f, 1f, 1f, 0.10f), 1f);

        // Draw major HUD module footprints in reference resolution, scaled into the frame.
        float refW = Mathf.Max(1f, uiLayout.referenceWidth);
        float refH = Mathf.Max(1f, uiLayout.referenceHeight);
        float sx = frame.width / refW;
        float sy = frame.height / refH;

        // Helper: create a rect in top-left pixel coords, then scale+offset into frame.
        Rect Map(Rect rTopLeft)
        {
            return new Rect(frame.x + rTopLeft.x * sx, frame.y + rTopLeft.y * sy, rTopLeft.width * sx, rTopLeft.height * sy);
        }

        var counterBg = Map(uiLayout.counter);
        var levelLabel = Map(uiLayout.level);
        var shopBtn = Map(uiLayout.shop);
        var coinsPill = Map(uiLayout.coins);
        var livesPill = Map(uiLayout.lives);
        var speedBtn = Map(uiLayout.speed);
        var settingsBtn = Map(uiLayout.settings);
        var boosterFill = Map(AnchorRect(
            refW, refH,
            uiLayout.boosterAnchor.x, uiLayout.boosterAnchor.y,
            0.5f, 0.5f,
            -uiLayout.boosterOffset.x, uiLayout.boosterOffset.y,
            uiLayout.boosterSize.x, uiLayout.boosterSize.y));
        var boosterShuffle = Map(AnchorRect(
            refW, refH,
            uiLayout.boosterAnchor.x, uiLayout.boosterAnchor.y,
            0.5f, 0.5f,
            uiLayout.boosterOffset.x, uiLayout.boosterOffset.y,
            uiLayout.boosterSize.x, uiLayout.boosterSize.y));

        DrawRectOutline(counterBg, new Color(0.4f, 0.9f, 0.7f, 0.35f), 2f);
        DrawRectOutline(levelLabel, new Color(1f, 1f, 1f, 0.20f), 2f);
        DrawRectOutline(shopBtn, new Color(0.9f, 0.8f, 0.2f, 0.25f), 2f);
        DrawRectOutline(coinsPill, new Color(1f, 0.85f, 0.2f, 0.22f), 2f);
        DrawRectOutline(livesPill, new Color(1f, 0.4f, 0.4f, 0.22f), 2f);
        DrawRectOutline(speedBtn, new Color(0.8f, 0.8f, 1f, 0.35f), 2f);
        DrawRectOutline(settingsBtn, new Color(0.8f, 0.8f, 1f, 0.35f), 2f);
        DrawRectOutline(boosterFill, new Color(0.4f, 0.9f, 0.7f, 0.25f), 2f);
        DrawRectOutline(boosterShuffle, new Color(0.7f, 0.5f, 1f, 0.25f), 2f);

        // Playable region outline.
        var playable = new Rect(frame.x, frame.y + topH, frame.width, frame.height - topH - bottomH);
        DrawRectOutline(playable, new Color(0.2f, 1f, 0.2f, 0.18f), 2f);
    }

    private void GetEffectiveLayoutSettings(
        LevelLayout layout,
        out bool autoResolve,
        out float minGap,
        out float preferredGap,
        out float minBoxToBoxGap,
        out int iterations,
        out float cameraMaxOrtho,
        out float minBlockPixel,
        out float fallbackBeltSpacing)
    {
        autoResolve = true;
        minGap = 0.08f;
        preferredGap = 0.18f;
        minBoxToBoxGap = 0.05f;
        iterations = 3;
        cameraMaxOrtho = 0f;
        minBlockPixel = 0f;
        fallbackBeltSpacing = 0.6f;

        if (layout == null)
        {
            return;
        }

        var runtime = Object.FindObjectOfType<GameRuntimeController>();
        if (runtime != null)
        {
            fallbackBeltSpacing = runtime.beltSlotSpacing;
        }

        if (layout.overrideLayoutAutoSettings || runtime == null)
        {
            autoResolve = layout.autoResolveLayoutOverlap;
            minGap = layout.minBoxToBeltGap;
            preferredGap = layout.preferredBoxToBeltGap;
            minBoxToBoxGap = layout.minBoxToBoxGap;
            iterations = layout.overlapResolveIterations;
            cameraMaxOrtho = layout.cameraMaxOrthoSize;
            minBlockPixel = layout.minBlockPixelSize;
        }
        else
        {
            autoResolve = runtime.autoResolveLayoutOverlap;
            minGap = runtime.minBoxToBeltGap;
            preferredGap = runtime.preferredBoxToBeltGap;
            minBoxToBoxGap = runtime.minBoxToBoxGap;
            iterations = runtime.overlapResolveIterations;
            cameraMaxOrtho = runtime.cameraMaxOrthoSize;
            minBlockPixel = runtime.minBlockPixelSize;
        }
    }

    private static Rect AnchorRect(float w, float h, float ax, float ay, float px, float py, float ox, float oy, float rw, float rh)
    {
        // Returns a rect in TOP-LEFT origin pixels (same space as the preview overlay).
        float pivotX = ax * w + ox;
        float pivotY = ay * h + oy; // bottom-left origin Y
        float blX = pivotX - px * rw;
        float blY = pivotY - py * rh;
        float topLeftY = h - (blY + rh);
        return new Rect(blX, topLeftY, rw, rh);
    }

    private void DrawCreateButtons()
    {
        EditorGUILayout.HelpBox("Select an existing LevelLayout asset, or create a new one.", MessageType.Info);
        if (GUILayout.Button("New Level Layout"))
        {
            CreateNewLevelAsset();
        }
    }

    private void DrawConveyorElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var property = _conveyorsList.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2f;
        EditorGUI.PropertyField(rect, property, GUIContent.none, true);
    }

    private float GetConveyorHeight(int index)
    {
        var property = _conveyorsList.serializedProperty.GetArrayElementAtIndex(index);
        return EditorGUI.GetPropertyHeight(property, true) + 6f;
    }

    private void DrawBoxElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        var property = _boxesList.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2f;
        EditorGUI.PropertyField(rect, property, GUIContent.none, true);
    }

    private float GetBoxHeight(int index)
    {
        var property = _boxesList.serializedProperty.GetArrayElementAtIndex(index);
        return EditorGUI.GetPropertyHeight(property, true) + 6f;
    }

    private void DrawPreviewConveyors(Rect rect, Rect bounds, List<Vector2> slotPositions, LevelLayout layout)
    {
        if (layout == null || layout.conveyors == null) return;
        Handles.color = new Color(0.12f, 0.56f, 0.91f, 0.9f);
        for (int ci = 0; ci < layout.conveyors.Count; ci++)
        {
            if (_onlyShowSelectedConveyor && _selectedConveyor != ci) continue;

            var conveyor = layout.conveyors[ci];
            if (conveyor.points == null || conveyor.points.Count < 2)
            {
                continue;
            }

        var line = ToScreen(rect, bounds, conveyor.points);
            // Hit band visualization (click range), matches conveyor width in world scaled to screen
            float scale = GetScale(rect, bounds);
            float hitWidth = Mathf.Clamp(conveyor.width * scale * 0.35f, 6f, 48f);
            Handles.color = new Color(0.9f, 0.8f, 0.2f, 0.25f);
            Handles.DrawAAPolyLine(hitWidth, line.ToArray());

            Handles.color = (_selectedConveyor == ci) ? Color.green : new Color(0.12f, 0.56f, 0.91f, 0.9f);
        Handles.DrawAAPolyLine(conveyor.width, line.ToArray());

        // Show points
        Handles.color = Color.cyan;
        for (int i = 0; i < conveyor.points.Count; i++)
        {
            var p = ToScreen(rect, bounds, conveyor.points[i]);
            Handles.DrawSolidDisc(p, Vector3.forward, 5f);
            string label = _selectedPoint == i && _selectedConveyor == ci ? $"P{i} *" : $"P{i}";
            Handles.Label(p + new Vector3(6f, -6f, 0f), label, EditorStyles.miniLabel);
        }

            if (_showSlotMarkers && (!_onlyShowSelectedConveyor || _selectedConveyor == ci))
            {
                Handles.color = new Color(0.6f, 0.6f, 0.6f, 0.55f);
                for (int i = 0; i < slotPositions.Count; i++)
                {
                    var sp = ToScreen(rect, bounds, slotPositions[i]);
                    Handles.DrawSolidDisc(sp, Vector3.forward, 4f);
                }
            }
        }
    }

    private void DrawPreviewBoxes(Rect rect, Rect bounds, List<Vector2> slotPositions, LevelLayout layout)
    {
        if (layout == null || layout.boxes == null) return;
        for (int i = 0; i < layout.boxes.Count; i++)
        {
            var box = layout.boxes[i];
            bool isLocked = box.locked;
            Handles.color = Color.white;
            if (_onlyShowSelectedBox && _selectedBox != i)
            {
                continue;
            }

            var half = box.size * 0.5f;
            var min = box.position - half;
            var max = box.position + half;
            var rectWorld = new[]
            {
                new Vector2(min.x, min.y),
                new Vector2(max.x, min.y),
                new Vector2(max.x, max.y),
                new Vector2(min.x, max.y)
            };

            var poly = new Vector3[4];
            for (int p = 0; p < 4; p++)
            {
                poly[p] = ToScreen(rect, bounds, rectWorld[p]);
            }

            var face = Color.clear; // remove base tint
            var outline = (_selectedBox == i) ? Color.green : Color.white;
            Handles.DrawSolidRectangleWithOutline(poly, face, outline);
            string label = $"{box.name} ({box.opening})";
            if (_previewSimActive && _previewSimSnapshot.Containers != null && i < _previewSimSnapshot.Containers.Length)
            {
                var simBox = _previewSimSnapshot.Containers[i];
                label = $"{box.name} ({box.opening}) {simBox.Count}/{simBox.Capacity}";
                if (simBox.Completed) label += " C";
                if (simBox.Locked) label += " L";
                if (simBox.Busy) label += " B";
            }
            Handles.Label(ToScreen(rect, bounds, box.position), label);

            if (isLocked)
            {
                var center = box.position;
                float pad = Mathf.Max(box.size.x, box.size.y) * 0.1f;
                var cmin = box.position - box.size * 0.5f - Vector2.one * pad;
                var cmax = box.position + box.size * 0.5f + Vector2.one * pad;
                var cres = new[]
                {
                    new Vector2(cmin.x, cmin.y),
                    new Vector2(cmax.x, cmin.y),
                    new Vector2(cmax.x, cmax.y),
                    new Vector2(cmin.x, cmax.y)
                };
                Vector3[] scr = ToScreen(rect, bounds, (IList<Vector2>)cres).ToArray();
                Color unlockCol = ToColor(box.unlockColor);
                Handles.DrawSolidRectangleWithOutline(scr, Color.clear, unlockCol);
                // draw a second inset outline to give thickness
                float inset = pad * 0.35f;
                var imin = cmin + Vector2.one * inset;
                var imax = cmax - Vector2.one * inset;
                var ires = new[]
                {
                    new Vector2(imin.x, imin.y),
                    new Vector2(imax.x, imin.y),
                    new Vector2(imax.x, imax.y),
                    new Vector2(imin.x, imax.y)
                };
                Vector3[] iscr = ToScreen(rect, bounds, (IList<Vector2>)ires).ToArray();
                Handles.DrawSolidRectangleWithOutline(iscr, Color.clear, unlockCol);
            }

            // Color fill overlay based on actual cell order/counts
            if (_previewSimActive && _previewSimSnapshot.Containers != null && i < _previewSimSnapshot.Containers.Length)
            {
                DrawSimulationCells(rect, bounds, box, _previewSimSnapshot.Containers[i], i);
            }
            else
            {
                DrawColorCells(rect, bounds, box, i);
            }

            // Grid overlay
            int cols = Mathf.Max(1, box.columns);
            int rows = Mathf.Max(1, box.rows);
            Handles.color = new Color(1f, 1f, 1f, 0.3f);
            for (int c = 1; c < cols; c++)
            {
                float x = min.x + box.size.x * (c / (float)cols);
                var a = ToScreen(rect, bounds, new Vector2(x, min.y));
                var b = ToScreen(rect, bounds, new Vector2(x, max.y));
                Handles.DrawLine(a, b);
            }
            for (int r = 1; r < rows; r++)
            {
                float y = min.y + box.size.y * (r / (float)rows);
                var a = ToScreen(rect, bounds, new Vector2(min.x, y));
                var b = ToScreen(rect, bounds, new Vector2(max.x, y));
                Handles.DrawLine(a, b);
            }

            var mouth = GetMouthPosition(box);
            var mouth2d = ToScreen(rect, bounds, mouth);
            Handles.color = Color.green;
            Handles.DrawSolidDisc(mouth2d, Vector3.forward, 3f);
            if (slotPositions.Count > 0)
            {
                int slotIdx = box.autoAlignSlot
                    ? FindNearestSlot(mouth, slotPositions)
                    : Mathf.Clamp(box.beltSlotIndex, 0, slotPositions.Count - 1);
                var slot2d = ToScreen(rect, bounds, slotPositions[slotIdx]);
                Handles.DrawLine(mouth2d, slot2d);
                Handles.Label(slot2d + new Vector3(6f, -6f, 0f), box.autoAlignSlot ? $"Auto Slot {slotIdx}" : $"Slot {slotIdx}", EditorStyles.miniBoldLabel);
            }
        }
    }

    private void HandlePreviewClick(Rect rect, Rect bounds, List<Vector2> slotPositions, LevelLayout layout)
    {
        if (IsSimRunning()) return;
        if (layout == null) return;
        var e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0) return;
        if (!rect.Contains(e.mousePosition)) return;

        var local = e.mousePosition; // already in local rect because of BeginClip
        var world = ToWorld(rect, bounds, local);

        // point hit first (screen-based threshold)
        var pointHit = FindConveyorPointAt(layout, local, rect, bounds, out int convIndex, out int pointIndex);
        if (pointHit)
        {
            _selectedConveyor = convIndex;
            _selectedBox = -1;
            _selectedPoint = pointIndex;
            _draggingConveyor = convIndex;
            _draggingPoint = pointIndex;
            Repaint();
            e.Use();
            return;
        }

        int hit = FindBoxAt(layout, world);
        if (hit != -1)
        {
            // Eyedropper: Alt+click copies colors from clicked box into current selection (if different),
            // otherwise just select.
            if (e.alt && _selectedBox >= 0 && _selectedBox < _level.boxes.Count && _selectedBox != hit)
            {
                var copy = _level.boxes[_selectedBox];
                CopyColorCounts(_level.boxes[hit], ref copy);
                _level.boxes[_selectedBox] = copy;
                EditorUtility.SetDirty(_level);
                EnsureColorList(_level.boxes[_selectedBox], force: true);
            }
            _selectedBox = hit;
            if (!e.alt)
            {
                // prepare for dragging
                _draggingBox = hit;
                _boxDragOffset = _level.boxes[hit].position - world;
            }
            _selectedConveyor = -1;
            if (_level.boxes[hit].colorCounts != null && _level.boxes[hit].colorCounts.Count > 0)
            {
                // Select color based on clicked cell
                var boxVisual = layout.boxes[hit];
                var boxData = _level.boxes[hit];
                var half = boxVisual.size * 0.5f;
                var min = boxVisual.position - half;
                var max = boxVisual.position + half;
                int cols = Mathf.Max(1, boxData.columns);
                int rows = Mathf.Max(1, boxData.rows);
                var order = BuildCellOrder(cols, rows, boxData.opening);
                int capacity = cols * rows;
                var colorIdx = new int[capacity];
                for (int i = 0; i < capacity; i++) colorIdx[i] = -1;
                int fill = 0;
                for (int ci = 0; ci < boxData.colorCounts.Count && fill < capacity; ci++)
                {
                    int cnt = Mathf.Max(0, boxData.colorCounts[ci].count);
                    for (int k = 0; k < cnt && fill < capacity; k++)
                    {
                        colorIdx[fill++] = ci;
                    }
                }
                int hitCell = -1;
                // compute cell indices
                float cellW = boxVisual.size.x / cols;
                float cellH = boxVisual.size.y / rows;
                for (int i = 0; i < capacity; i++)
                {
                    var cell = order[i];
                    float cxMin = min.x + cell.x * cellW;
                    float cxMax = cxMin + cellW;
                    // Match runtime BoxView: (0,0) is top-left in the box grid.
                    float cyMax = max.y - cell.y * cellH;
                    float cyMin = cyMax - cellH;
                    if (world.x >= cxMin && world.x <= cxMax && world.y >= cyMin && world.y <= cyMax)
                    {
                        hitCell = i;
                        break;
                    }
                }
                if (hitCell >= 0 && hitCell < capacity && colorIdx[hitCell] >= 0)
                {
                    _selectedColorIndex = colorIdx[hitCell];
                }
                else
                {
                    _selectedColorIndex = 0;
                }
                _lastPaletteColor = (int)boxData.colorCounts[Mathf.Clamp(_selectedColorIndex, 0, boxData.colorCounts.Count - 1)].color;
                EnsureColorList(_level.boxes[hit], force: true);
            }
            Repaint();
            e.Use();
            return;
        }

        int conv = FindConveyorAt(layout, rect, bounds, local);
        if (conv != -1)
        {
            _selectedConveyor = conv;
            _selectedPoint = -1;
            _selectedBox = -1;
            Repaint();
            e.Use();
            return;
        }

        // Click on empty space: clear selection
        _selectedBox = -1;
        _selectedConveyor = -1;
        _selectedPoint = -1;
        Repaint();
    }

    private void DrawColorCells(Rect rect, Rect bounds, BoxSpec box, int boxIndex)
    {
        if (box.colorCounts == null || box.colorCounts.Count == 0) return;
        int cols = Mathf.Max(1, box.columns);
        int rows = Mathf.Max(1, box.rows);
        int capacity = cols * rows;
        if (capacity <= 0) return;

        var order = BuildCellOrder(cols, rows, box.opening);
        var colorIdx = new int[capacity];
        for (int i = 0; i < capacity; i++) colorIdx[i] = -1;

        int fillIndex = 0;
        for (int ci = 0; ci < box.colorCounts.Count && fillIndex < capacity; ci++)
        {
            int count = Mathf.Max(0, box.colorCounts[ci].count);
            for (int k = 0; k < count && fillIndex < capacity; k++)
            {
                colorIdx[fillIndex++] = ci;
            }
        }

        var min = box.position - box.size * 0.5f;
        var max = box.position + box.size * 0.5f;
        var cellSize = new Vector2(box.size.x / cols, box.size.y / rows);
        for (int i = 0; i < capacity; i++)
        {
            int ci = colorIdx[i];
            if (ci < 0 || ci >= box.colorCounts.Count) continue;
            var cell = order[i];
            // Match runtime BoxView: (0,0) is top-left in the box grid.
            var cmin = new Vector2(min.x + cell.x * cellSize.x, max.y - (cell.y + 1) * cellSize.y);
            var cmax = new Vector2(cmin.x + cellSize.x, cmin.y + cellSize.y);
            var poly = new Vector3[4]
            {
                ToScreen(rect, bounds, new Vector2(cmin.x, cmin.y)),
                ToScreen(rect, bounds, new Vector2(cmax.x, cmin.y)),
                ToScreen(rect, bounds, new Vector2(cmax.x, cmax.y)),
                ToScreen(rect, bounds, new Vector2(cmin.x, cmax.y))
            };
            var cc = box.colorCounts[ci];
            var col = cc.hidden ? new Color(0.2f, 0.2f, 0.2f, 0.5f) : ToColor(cc.color);
            col.a = cc.hidden ? 0.5f : 0.7f;
            Handles.DrawSolidRectangleWithOutline(poly, col, Color.clear);

            // highlight selected color
            if (_selectedBox == boxIndex && _selectedColorIndex == ci)
            {
                Handles.color = Color.white;
                Handles.DrawAAPolyLine(3f, poly);
            }

            if (cc.hidden)
            {
                Handles.color = Color.white;
                Handles.Label((poly[0] + poly[2]) * 0.5f, "H", EditorStyles.miniBoldLabel);
            }
        }
    }

    private void HandlePreviewDrag(Rect rect, Rect bounds)
    {
        if (IsSimRunning()) return;
        var e = Event.current;
        if (_draggingConveyor >= 0 && _draggingPoint >= 0)
        {
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseMove) && e.button == 0)
            {
                var local = e.mousePosition;
                var world = ToWorld(rect, bounds, local);
                world = SnapIfNeeded(world);
                var conv = _level.conveyors[_draggingConveyor];
                if (conv.points != null && _draggingPoint < conv.points.Count)
                {
                    Undo.RecordObject(_level, "Drag Conveyor Point");
                    conv.points[_draggingPoint] = world;
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                _draggingConveyor = -1;
                _draggingPoint = -1;
            }
        }

        // Drag selected box
        if (_draggingBox >= 0 && _draggingBox < (_level?.boxes?.Count ?? 0))
        {
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseMove) && e.button == 0)
            {
                var local = e.mousePosition;
                var world = ToWorld(rect, bounds, local);
                Undo.RecordObject(_level, "Move Box");
                var box = _level.boxes[_draggingBox];
                // Snap the final position (absolute grid from origin), not the mouse delta.
                // This avoids "offset snapping" when the initial position isn't on-grid.
                box.position = SnapIfNeeded(world + _boxDragOffset);
                _level.boxes[_draggingBox] = box;
                EditorUtility.SetDirty(_level);
                Repaint();
                e.Use();
            }
            else if (e.type == EventType.MouseUp)
            {
                _draggingBox = -1;
            }
        }
    }

    private void HandleGlobalHotkeys()
    {
        if (IsSimRunning()) return;
        var e = Event.current;
        if (e == null) return;
        if (_level == null || _level.boxes == null || _level.boxes.Count == 0) return;

        // Tab / Shift+Tab: cycle boxes
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Tab)
        {
            int dir = e.shift ? -1 : 1;
            if (_selectedBox < 0) _selectedBox = 0;
            else _selectedBox = (_selectedBox + dir + _level.boxes.Count) % _level.boxes.Count;
            _selectedConveyor = -1;
            Repaint();
            e.Use();
            return;
        }

        if (_selectedBox < 0 || _selectedBox >= _level.boxes.Count) return;
        var box = _level.boxes[_selectedBox];
        int capacity = Mathf.Max(1, box.columns * box.rows);
        if (box.colorCounts == null) box.colorCounts = new List<ColorCount>();

        // Delete key: remove selected box
        if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace))
        {
            Undo.RecordObject(_level, "Delete Box");
            _level.boxes.RemoveAt(_selectedBox);
            _selectedBox = Mathf.Clamp(_selectedBox - 1, -1, _level.boxes.Count - 1);
            _colorCountsList = null;
            _colorListBoxIndex = -1;
            _selectedColorIndex = -1;
            EditorUtility.SetDirty(_level);
            Repaint();
            e.Use();
            return;
        }

        // Number keys 1-9: fill single color to full capacity
        if (e.type == EventType.KeyDown && e.keyCode >= KeyCode.Alpha1 && e.keyCode <= KeyCode.Alpha9)
        {
            int idx = (int)e.keyCode - (int)KeyCode.Alpha1;
            _lastPaletteColor = idx;
            // If a color segment is selected, change that segment; otherwise fill the whole box.
            if (_selectedColorIndex >= 0 && _selectedColorIndex < box.colorCounts.Count)
            {
                box.colorCounts[_selectedColorIndex].color = (BlockColor)idx;
            }
            else
            {
                box.colorCounts.Clear();
                box.colorCounts.Add(new ColorCount { color = (BlockColor)idx, count = capacity });
                _selectedColorIndex = 0;
            }
            _level.boxes[_selectedBox] = box;
            EditorUtility.SetDirty(_level);
            EnsureColorList(box, force: true);
            Repaint();
            e.Use();
            return;
        }

        // H: mark selected color as hidden (toggle)
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.H)
        {
            if (_selectedColorIndex >= 0 && _selectedColorIndex < box.colorCounts.Count)
            {
                var cc = box.colorCounts[_selectedColorIndex];
                cc.hidden = !cc.hidden;
                box.colorCounts[_selectedColorIndex] = cc;
                _level.boxes[_selectedBox] = box;
                EditorUtility.SetDirty(_level);
                EnsureColorList(box, force: true);
                Repaint();
            }
            e.Use();
            return;
        }

        // L: toggle lock box
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.L)
        {
            box.locked = !box.locked;
            _level.boxes[_selectedBox] = box;
            EditorUtility.SetDirty(_level);
            Repaint();
            e.Use();
            return;
        }

        // Scroll wheel: adjust selected color count quickly
        if (e.type == EventType.ScrollWheel && box.colorCounts.Count > 0)
        {
            int delta = e.delta.y > 0 ? -1 : 1;
            int idx = (_selectedColorIndex >= 0 && _selectedColorIndex < box.colorCounts.Count) ? _selectedColorIndex : 0;
            var cc = box.colorCounts[idx];
            cc.count = Mathf.Clamp(cc.count + delta, 0, capacity);
            box.colorCounts[idx] = cc;
            // Rebalance others: allow selected to reach full capacity, others scaled down if needed
            int remainingCap = Mathf.Max(0, capacity - cc.count);
            int otherTotal = 0;
            for (int i = 0; i < box.colorCounts.Count; i++)
            {
                if (i == idx) continue;
                otherTotal += Mathf.Max(0, box.colorCounts[i].count);
            }
            if (remainingCap == 0)
            {
                for (int i = 0; i < box.colorCounts.Count; i++)
                {
                    if (i == idx) continue;
                    box.colorCounts[i].count = 0;
                }
            }
            else if (otherTotal > remainingCap && otherTotal > 0)
            {
                float scale = remainingCap / (float)otherTotal;
                int acc = 0;
                for (int i = 0; i < box.colorCounts.Count; i++)
                {
                    if (i == idx) continue;
                    int v = Mathf.Max(0, Mathf.RoundToInt(box.colorCounts[i].count * scale));
                    box.colorCounts[i].count = v;
                    acc += v;
                }
                // adjust selected if rounding error made sum exceed capacity
                int sum = acc + cc.count;
                if (sum > capacity)
                {
                    cc.count = Mathf.Max(0, cc.count - (sum - capacity));
                    box.colorCounts[idx] = cc;
                }
            }
            _level.boxes[_selectedBox] = box;
            EditorUtility.SetDirty(_level);
            EnsureColorList(box, force: true);
            Repaint();
            e.Use();
            return;
        }
    }

    private int FindBoxAt(LevelLayout layout, Vector2 world)
    {
        if (layout == null || layout.boxes == null) return -1;
        for (int i = 0; i < layout.boxes.Count; i++)
        {
            var b = layout.boxes[i];
            var half = b.size * 0.5f;
            var min = b.position - half;
            var max = b.position + half;
            if (world.x >= min.x && world.x <= max.x && world.y >= min.y && world.y <= max.y)
            {
                return i;
            }
        }
        return -1;
    }

    private int FindConveyorAt(LevelLayout layout, Rect rect, Rect bounds, Vector2 localMouse)
    {
        if (layout == null || layout.conveyors == null) return -1;
        int best = -1;
        float bestDist = float.MaxValue;
        for (int ci = 0; ci < layout.conveyors.Count; ci++)
        {
            var c = layout.conveyors[ci];
            if (c.points == null || c.points.Count < 2) continue;
            float scale = GetScale(rect, bounds);
            float hitWidth = Mathf.Clamp(c.width * scale * 0.35f, 6f, 48f);
            float threshold = hitWidth * 0.5f;
            var screenPts = ToScreen(rect, bounds, c.points);
            for (int i = 0; i < screenPts.Count - 1; i++)
            {
                float d = DistancePointToSegment2D(localMouse, screenPts[i], screenPts[i + 1]);
                if (d < bestDist && d <= threshold)
                {
                    bestDist = d;
                    best = ci;
                }
            }
        }
        return best;
    }

    private static Vector3 ToScreen(Rect viewRect, Rect bounds, Vector2 world)
    {
        var scale = GetScale(viewRect, bounds);
        var center = _previewCameraActive ? _previewCameraCenterWorld : bounds.center;
        var x = viewRect.x + viewRect.width * 0.5f + (world.x - center.x) * scale;
        var y = viewRect.y + viewRect.height * 0.5f - (world.y - center.y) * scale;
        return new Vector3(x, y, 0f);
    }

    private static Vector2 ToWorld(Rect viewRect, Rect bounds, Vector2 screen)
    {
        var scale = GetScale(viewRect, bounds);
        var center = _previewCameraActive ? _previewCameraCenterWorld : bounds.center;
        float x = (screen.x - (viewRect.x + viewRect.width * 0.5f)) / scale + center.x;
        float y = ((viewRect.y + viewRect.height * 0.5f) - screen.y) / scale + center.y;
        return new Vector2(x, y);
    }

    private static List<Vector3> ToScreen(Rect viewRect, Rect bounds, IList<Vector2> worldPoints)
    {
        var list = new List<Vector3>(worldPoints.Count);
        for (int i = 0; i < worldPoints.Count; i++)
        {
            list.Add(ToScreen(viewRect, bounds, worldPoints[i]));
        }

        return list;
    }

    private static float GetScale(Rect viewRect, Rect bounds)
    {
        var size = bounds.size;
        size.x = Mathf.Max(size.x, 0.001f);
        size.y = Mathf.Max(size.y, 0.001f);
        float baseScale = Mathf.Min(viewRect.width / size.x, viewRect.height / size.y);
        return baseScale * (_previewCameraActive ? _previewScaleMultiplier : 1f);
    }

    private static int IndexOfLevel(LevelLayout layout, LevelLayout[] options)
    {
        if (layout == null || options == null) return -1;
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i] == layout) return i;
        }

        return -1;
    }

    private Rect ComputeBounds(LevelLayout level)
    {
        // Match runtime camera padding behavior (GameRuntimeController.cameraPadding default is negative -> auto).
        var b = LayoutUtils.ComputeLayoutBounds(level);
        if (b.size == Vector3.zero)
        {
            return new Rect(Vector2.zero, Vector2.one);
        }

        var size = b.size;
        float paddingToUse = Mathf.Max(size.x, size.y) * 0.08f + 0.35f;
        b.Expand(paddingToUse * 2f);

        return new Rect(
            new Vector2(b.center.x, b.center.y) - new Vector2(b.size.x, b.size.y) * 0.5f,
            new Vector2(b.size.x, b.size.y)
        );
    }

    private List<Vector2> BuildSlotPositionsForPreview(LevelLayout level, float fallbackSpacing)
    {
        var list = new List<Vector2>();
        if (level.conveyors == null || level.conveyors.Count == 0) return list;
        float used;
        float spacing = level.beltSlotSpacing > 0 ? level.beltSlotSpacing : fallbackSpacing;
        var slots = LayoutUtils.BuildSlotsFromPath(
            level.conveyors[0],
            spacing,
            level.beltCapacity,
            out used,
            smoothCorners: level.smoothCorners,
            smoothTension: level.cornerSmoothTension,
            smoothSubdivisions: level.cornerSubdivisions);
        foreach (var t in slots)
        {
            list.Add(new Vector2(t.position.x, t.position.y));
            Object.DestroyImmediate(t.gameObject);
        }
        return list;
    }

    private static Vector2 GetMouthPosition(BoxSpec box)
    {
        Vector2 normal = Vector2.down;
        switch (box.opening)
        {
            case OpeningSide.Top: normal = Vector2.up; break;
            case OpeningSide.Bottom: normal = Vector2.down; break;
            case OpeningSide.Left: normal = Vector2.left; break;
            case OpeningSide.Right: normal = Vector2.right; break;
        }
        var half = box.size * 0.5f;
        float dist = (normal == Vector2.left || normal == Vector2.right) ? half.x : half.y;
        return box.position + normal * dist;
    }

    private static int FindNearestSlot(Vector2 point, List<Vector2> slots)
    {
        int best = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < slots.Count; i++)
        {
            float d = (slots[i] - point).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }

    private static void NormalizeColorCounts(BoxSpec box)
    {
        if (box.colorCounts == null) box.colorCounts = new List<ColorCount>();
        int capacity = Mathf.Max(1, box.columns * box.rows);
        int total = 0;
        foreach (var cc in box.colorCounts) total += Mathf.Max(0, cc.count);
        if (total <= 0)
        {
            box.colorCounts.Clear();
            return;
        }
        float scale = capacity / (float)total;
        int acc = 0;
        for (int i = 0; i < box.colorCounts.Count; i++)
        {
            int val = Mathf.Max(0, Mathf.RoundToInt(box.colorCounts[i].count * scale));
            box.colorCounts[i].count = val;
            acc += val;
        }
        if (acc < capacity && box.colorCounts.Count > 0)
        {
            box.colorCounts[0].count += (capacity - acc);
        }
        if (acc > capacity && box.colorCounts.Count > 0)
        {
            box.colorCounts[0].count = Mathf.Max(0, box.colorCounts[0].count - (acc - capacity));
        }
    }

    private static void CopyColorCounts(BoxSpec from, ref BoxSpec to)
    {
        if (from.colorCounts == null) return;
        if (to.colorCounts == null) to.colorCounts = new List<ColorCount>();
        to.colorCounts.Clear();
        foreach (var cc in from.colorCounts)
        {
            to.colorCounts.Add(new ColorCount { color = cc.color, count = cc.count });
        }
        NormalizeColorCounts(to);
    }

    private static List<Vector2Int> BuildCellOrder(int cols, int rows, OpeningSide opening)
    {
        var order = new List<Vector2Int>(cols * rows);
        switch (opening)
        {
            case OpeningSide.Top:
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols; c++)
                        order.Add(new Vector2Int(c, r));
                break;
            case OpeningSide.Bottom:
                for (int r = rows - 1; r >= 0; r--)
                    for (int c = 0; c < cols; c++)
                        order.Add(new Vector2Int(c, r));
                break;
            case OpeningSide.Left:
                for (int c = 0; c < cols; c++)
                    for (int r = 0; r < rows; r++)
                        order.Add(new Vector2Int(c, r));
                break;
            case OpeningSide.Right:
                for (int c = cols - 1; c >= 0; c--)
                    for (int r = 0; r < rows; r++)
                        order.Add(new Vector2Int(c, r));
                break;
        }
        return order;
    }

    private void EnsureColorList(BoxSpec box, bool force = false)
    {
        if (_serializedLevel == null || _level == null || box == null) return;
        if (!force && _colorCountsList != null && _colorListBoxIndex == _selectedBox) return;

        var boxesProp = _serializedLevel.FindProperty("boxes");
        if (_selectedBox < 0 || _selectedBox >= boxesProp.arraySize) return;
        var colorsProp = boxesProp.GetArrayElementAtIndex(_selectedBox).FindPropertyRelative("colorCounts");

        _colorCountsList = new ReorderableList(_serializedLevel, colorsProp, true, true, true, true);
        _colorCountsList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "Colors & Counts (outer->inner, drag to reorder)");
        };
        _colorCountsList.drawElementCallback = (rect, index, active, focused) =>
        {
            rect.y += 2f;
            var element = colorsProp.GetArrayElementAtIndex(index);
            var colorProp = element.FindPropertyRelative("color");
            var countProp = element.FindPropertyRelative("count");
            var hiddenProp = element.FindPropertyRelative("hidden");
            var left = new Rect(rect.x, rect.y, 140f, EditorGUIUtility.singleLineHeight);
            var right = new Rect(rect.x + 150f, rect.y, rect.width - 250f, EditorGUIUtility.singleLineHeight);
            var hid = new Rect(rect.x + rect.width - 90f, rect.y, 50f, EditorGUIUtility.singleLineHeight);
            var del = new Rect(rect.xMax - 30f, rect.y, 30f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(left, colorProp, GUIContent.none);
            countProp.intValue = Mathf.Max(0, EditorGUI.IntField(right, countProp.intValue));
            hiddenProp.boolValue = EditorGUI.ToggleLeft(hid, "H", hiddenProp.boolValue);
            if (GUI.Button(del, "X"))
            {
                colorsProp.DeleteArrayElementAtIndex(index);
            }
        };
        _colorCountsList.onSelectCallback = list =>
        {
            _selectedColorIndex = list.index;
            var element = colorsProp.GetArrayElementAtIndex(_selectedBox >= 0 && _selectedBox < colorsProp.arraySize ? list.index : 0);
            var colorProp = element.FindPropertyRelative("color");
            _lastPaletteColor = colorProp != null ? colorProp.enumValueIndex : 0;
        };
        _colorCountsList.onAddCallback = list =>
        {
            colorsProp.arraySize++;
            var elem = colorsProp.GetArrayElementAtIndex(colorsProp.arraySize - 1);
            elem.FindPropertyRelative("color").enumValueIndex = _lastPaletteColor;
            elem.FindPropertyRelative("count").intValue = 1;
            _selectedColorIndex = colorsProp.arraySize - 1;
        };
        _colorCountsList.onReorderCallback = list =>
        {
            _selectedColorIndex = list.index;
        };
        _colorListBoxIndex = _selectedBox;
    }

    private void DrawColorLegend()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Palette:", GUILayout.Width(80));
        int colorCount = System.Enum.GetValues(typeof(BlockColor)).Length;
        int max = Mathf.Min(9, colorCount);
        for (int i = 0; i < max; i++)
        {
            var col = ToColor((BlockColor)i);
            var prev = GUI.color;
            GUI.color = col;
            if (GUILayout.Button($"{i + 1}", GUILayout.Width(28), GUILayout.Height(20)))
            {
                _lastPaletteColor = i;
                if (_selectedBox >= 0 && _selectedBox < (_level?.boxes?.Count ?? 0))
                {
                    _selectedColorIndex = 0;
                }
            }
            GUI.color = prev;
        }
        EditorGUILayout.EndHorizontal();
    }

    private Color ToColor(BlockColor c)
    {
        switch (c)
        {
            case BlockColor.Red: return new Color(0.9f, 0.2f, 0.2f);
            case BlockColor.Blue: return new Color(0.2f, 0.4f, 0.9f);
            case BlockColor.Yellow: return new Color(0.98f, 0.8f, 0.15f);
            case BlockColor.Green: return new Color(0.25f, 0.8f, 0.35f);
            case BlockColor.Purple: return new Color(0.6f, 0.35f, 0.9f);
            case BlockColor.Orange: return new Color(1.0f, 0.6f, 0.2f);
            default: return Color.white;
        }
    }

    private bool FindConveyorPointAt(LevelLayout layout, Vector2 localMouse, Rect rect, Rect bounds, out int conveyorIndex, out int pointIndex)
    {
        conveyorIndex = -1;
        pointIndex = -1;
        if (layout == null || layout.conveyors == null) return false;
        const float screenThreshold = 10f;
        for (int ci = 0; ci < layout.conveyors.Count; ci++)
        {
            var c = layout.conveyors[ci];
            if (c.points == null) continue;
            for (int pi = 0; pi < c.points.Count; pi++)
            {
                var sp = ToScreen(rect, bounds, c.points[pi]);
                if (Vector2.Distance(sp, localMouse) <= screenThreshold)
                {
                    conveyorIndex = ci;
                    pointIndex = pi;
                    return true;
                }
            }
        }
        return false;
    }

    private static float DistancePointToSegment2D(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        var proj = a + ab * t;
        return Vector2.Distance(p, proj);
    }

    private void SaveCurrentLevel()
    {
        if (_level == null)
        {
            return;
        }

        ApplyAutoAlignSlots(_level);
        EditorUtility.SetDirty(_level);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Saved level {_level.name}");
    }

    private void SetLevel(LevelLayout level)
    {
        CancelDsrSimulation();
        _level = level;
        if (_level != null)
        {
            // Defensive init in case older assets have null lists
            if (_level.conveyors == null) _level.conveyors = new List<ConveyorPath>();
            if (_level.boxes == null) _level.boxes = new List<BoxSpec>();
        }
        _serializedLevel = null;
        _conveyorsList = null;
        _boxesList = null;
        _selectedBox = -1;
        _selectedConveyor = -1;
        _draggingConveyor = -1;
        _draggingPoint = -1;
        _selectedPoint = -1;
        _colorCountsList = null;
        _colorListBoxIndex = -1;
        _selectedColorIndex = -1;
        Repaint();
        RefreshLevelList();
        _selectedIndex = IndexOfLevel(level, _levelOptions);
        if (_level != null && _tabIndex == 0)
        {
            StartDsrSimulation();
        }
    }

    private void DrawSimulationCells(
        Rect rect,
        Rect bounds,
        BoxSpec box,
        LoopSorting.Editor.LevelDifficultyMetrics.SimulationContainerSnapshot simBox,
        int boxIndex)
    {
        if (simBox.Blocks == null || simBox.Blocks.Length == 0) return;
        int cols = Mathf.Max(1, box.columns);
        int rows = Mathf.Max(1, box.rows);
        int capacity = cols * rows;
        if (capacity <= 0) return;

        var order = BuildCellOrder(cols, rows, box.opening);
        var min = box.position - box.size * 0.5f;
        var max = box.position + box.size * 0.5f;
        var cellSize = new Vector2(box.size.x / cols, box.size.y / rows);
        int count = Mathf.Min(simBox.Blocks.Length, capacity);
        for (int i = 0; i < count; i++)
        {
            if (!simBox.Blocks[i].HasValue) continue;
            var cell = order[i];
            var cmin = new Vector2(min.x + cell.x * cellSize.x, max.y - (cell.y + 1) * cellSize.y);
            var cmax = new Vector2(cmin.x + cellSize.x, cmin.y + cellSize.y);
            var poly = new Vector3[4]
            {
                ToScreen(rect, bounds, new Vector2(cmin.x, cmin.y)),
                ToScreen(rect, bounds, new Vector2(cmax.x, cmin.y)),
                ToScreen(rect, bounds, new Vector2(cmax.x, cmax.y)),
                ToScreen(rect, bounds, new Vector2(cmin.x, cmax.y))
            };

            var col = BlockVisual.ToUnityColor(simBox.Blocks[i].Value);
            col.a = 0.8f;
            Handles.DrawSolidRectangleWithOutline(poly, col, Color.clear);
        }
    }

    private void DrawPreviewSimBelt(
        Rect rect,
        Rect bounds,
        List<Vector2> slotPositions,
        LevelLayout layout,
        LoopSorting.Editor.LevelDifficultyMetrics.SimulationDebugSnapshot snap)
    {
        if (slotPositions == null || snap.BeltSlots == null) return;
        int count = Mathf.Min(slotPositions.Count, snap.BeltSlots.Length);
        if (count <= 0) return;

        float blockSize = layout != null && layout.blockSize > 0f ? layout.blockSize : 0.6f;
        float scale = GetScale(rect, bounds);
        float size = Mathf.Clamp(blockSize * 0.6f * scale, 4f, 18f);
        for (int i = 0; i < count; i++)
        {
            var slotColor = snap.BeltSlots[i];
            if (!slotColor.HasValue) continue;
            var pos = ToScreen(rect, bounds, slotPositions[i]);
            var r = new Rect(pos.x - size * 0.5f, pos.y - size * 0.5f, size, size);
            EditorGUI.DrawRect(r, BlockVisual.ToUnityColor(slotColor.Value));
        }
    }

    private void DrawPreviewSimOverlay(
        Rect frameRect,
        LoopSorting.Editor.LevelDifficultyMetrics.SimulationDebugSnapshot snap)
    {
        float pad = 6f;
        float lineH = 16f;
        float height = lineH * 3f + pad * 2f;
        float width = Mathf.Min(frameRect.width - pad * 2f, 360f);
        var panel = new Rect(frameRect.x + pad, frameRect.y + pad, width, height);
        EditorGUI.DrawRect(panel, new Color(0f, 0f, 0f, 0.45f));

        var line1 = $"Sim {snap.RunIndex + 1}/{(_dsrSim != null ? _dsrSim.RunsTotal : 0)}  Tick {snap.Tick}";
        var line2 = $"Belt {snap.BeltCount}/{snap.BeltLength}  NoInsert {snap.NoInsertWhileFull}";
        var line3 = snap.IsReleasing
            ? $"Release box {snap.ActiveReleaseIndex} {snap.ActiveReleaseColor} pending {snap.PendingRelease}"
            : "Release none";

        var style = EditorStyles.miniLabel;
        var r1 = new Rect(panel.x + pad, panel.y + pad, panel.width - pad * 2f, lineH);
        var r2 = new Rect(r1.x, r1.y + lineH, r1.width, lineH);
        var r3 = new Rect(r2.x, r2.y + lineH, r2.width, lineH);
        GUI.Label(r1, line1, style);
        GUI.Label(r2, line2, style);
        GUI.Label(r3, line3, style);
    }

    private void BindSerializedObject()
    {
        if (_level == null)
        {
            return;
        }

        _serializedLevel = new SerializedObject(_level);
    }

    private void RefreshLevelList()
    {
        var guids = AssetDatabase.FindAssets("t:LevelLayout", new[] { LevelsFolder });
        var list = new List<LevelLayout>();
        var names = new List<string>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<LevelLayout>(path);
            if (asset != null)
            {
                list.Add(asset);
                names.Add(asset.name);
            }
        }

        _levelOptions = list.ToArray();
        _levelOptionNames = names.ToArray();
        _selectedIndex = IndexOfLevel(_level, _levelOptions);

        if (_levelOptions.Length == 0)
        {
            Debug.Log($"LevelEditorWindow: No LevelLayout assets found under {LevelsFolder}.");
        }
    }

    private void DrawFlowPanel()
    {
        float oldLabel = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 90f;

        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.LabelField("Level Flow", EditorStyles.boldLabel);
        _flowScroll = EditorGUILayout.BeginScrollView(_flowScroll, GUILayout.Height(position.height - 120f));

        EditorGUILayout.BeginHorizontal();
        _flowAsset = (LevelFlow)EditorGUILayout.ObjectField("Flow Asset", _flowAsset, typeof(LevelFlow), false);
        if (GUILayout.Button("New Flow", GUILayout.Width(90)))
        {
            CreateNewFlowAsset();
        }
        if (_flowAsset != null && GUILayout.Button("Set Runtime Flow", GUILayout.Width(110)))
        {
            SetActiveRuntimeFlow(_flowAsset);
        }
        EditorGUILayout.EndHorizontal();

        if (_flowAsset != null)
        {
            if (_flowSO == null || _flowSO.targetObject != _flowAsset)
            {
                _flowSO = new SerializedObject(_flowAsset);
                var prop = _flowSO.FindProperty("levels");
                _flowList = new ReorderableList(_flowSO, prop, true, true, true, true)
                {
                    drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Level Sequence"),
                    drawElementCallback = (rect, index, active, focused) =>
                    {
                        var element = prop.GetArrayElementAtIndex(index);
                        rect.y += 2f;
                        EditorGUI.ObjectField(rect, element, GUIContent.none);
                    }
                };
            }

            _flowSO.Update();
            EditorGUILayout.PropertyField(_flowSO.FindProperty("startIndex"), new GUIContent("Start Index"));
            _flowList?.DoLayoutList();
        EditorGUILayout.BeginHorizontal();
            _flowAddCandidate = (LevelLayout)EditorGUILayout.ObjectField("Add Level", _flowAddCandidate, typeof(LevelLayout), false);
            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                if (_flowAddCandidate != null)
                {
                    var prop = _flowSO.FindProperty("levels");
                    int newIndex = prop.arraySize;
                    prop.InsertArrayElementAtIndex(newIndex);
                    prop.GetArrayElementAtIndex(newIndex).objectReferenceValue = _flowAddCandidate;
                    _flowAddCandidate = null;
                }
            }
            if (_level != null && GUILayout.Button("Add Current", GUILayout.Width(85)))
            {
                var prop = _flowSO.FindProperty("levels");
                int newIndex = prop.arraySize;
                prop.InsertArrayElementAtIndex(newIndex);
                prop.GetArrayElementAtIndex(newIndex).objectReferenceValue = _level;
            }
            EditorGUILayout.EndHorizontal();
            _flowSO.ApplyModifiedProperties();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUIUtility.labelWidth = oldLabel;
    }

    private void ApplyAutoAlignSlots(LevelLayout level)
    {
        if (level == null || level.conveyors == null || level.conveyors.Count == 0)
        {
            return;
        }

        // Pick the first conveyor that has at least 2 points
        ConveyorPath path = null;
        for (int i = 0; i < level.conveyors.Count; i++)
        {
            var c = level.conveyors[i];
            if (c != null && c.points != null && c.points.Count >= 2)
            {
                path = c;
                break;
            }
        }
        if (path == null)
        {
            return;
        }

        float used;
        float spacing = level.beltSlotSpacing > 0 ? level.beltSlotSpacing : 0.6f;
        var slots = LayoutUtils.BuildSlotsFromPath(
            path,
            spacing,
            level.beltCapacity,
            out used,
            smoothCorners: level.smoothCorners,
            smoothTension: level.cornerSmoothTension,
            smoothSubdivisions: level.cornerSubdivisions);
        var slotPos = new List<Vector2>(slots.Count);
        foreach (var t in slots)
        {
            slotPos.Add(new Vector2(t.position.x, t.position.y));
            Object.DestroyImmediate(t.gameObject);
        }

        for (int i = 0; i < level.boxes.Count; i++)
        {
            var box = level.boxes[i];
            if (!box.autoAlignSlot || slotPos.Count == 0)
            {
                continue;
            }

            int nearest = FindNearestSlot(GetMouthPosition(box), slotPos);
            box.beltSlotIndex = nearest;
        }
    }

    private void CreateNewLevelAsset()
    {
        if (!Directory.Exists(LevelsFolder))
        {
            Directory.CreateDirectory(LevelsFolder);
            AssetDatabase.Refresh();
        }

        var path = EditorUtility.SaveFilePanelInProject("Create Level Layout", "LevelLayout", "asset", "Save LevelLayout asset", LevelsFolder);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var asset = ScriptableObject.CreateInstance<LevelLayout>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SetLevel(asset);
        EditorGUIUtility.PingObject(asset);
    }

    private void CreateNewFlowAsset()
    {
        if (!Directory.Exists(FlowsFolder))
        {
            Directory.CreateDirectory(FlowsFolder);
            AssetDatabase.Refresh();
        }

        var path = EditorUtility.SaveFilePanelInProject("Create Level Flow", "LevelFlow", "asset", "Save LevelFlow asset", FlowsFolder);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        var asset = ScriptableObject.CreateInstance<LevelFlow>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _flowAsset = asset;
        _flowSO = new SerializedObject(asset);
        EditorGUIUtility.PingObject(asset);
    }

    private static void SetActiveRuntimeLevel(LevelLayout layout)
    {
        if (!Directory.Exists(RuntimeConfigResourcesFolder))
        {
            Directory.CreateDirectory(RuntimeConfigResourcesFolder);
        }

        var assetPath = $"{RuntimeConfigResourcesFolder}/LevelRuntimeConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<LevelRuntimeConfig>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<LevelRuntimeConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
        }

        config.activeLevel = layout;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Set runtime level: {layout.name}");
    }

    private static void SetActiveRuntimeFlow(LevelFlow flow)
    {
        if (!Directory.Exists(RuntimeConfigResourcesFolder))
        {
            Directory.CreateDirectory(RuntimeConfigResourcesFolder);
        }

        var assetPath = $"{RuntimeConfigResourcesFolder}/LevelRuntimeConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<LevelRuntimeConfig>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<LevelRuntimeConfig>();
            AssetDatabase.CreateAsset(config, assetPath);
        }

        config.activeFlow = flow;
        config.activeLevel = null;
        config.flowStartIndex = flow != null ? Mathf.Clamp(flow.startIndex, 0, Mathf.Max(0, flow.levels.Count - 1)) : 0;
        EditorUtility.SetDirty(config);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Set runtime flow: {flow?.name}");
    }

    private Vector2 SnapIfNeeded(Vector2 v)
    {
        if (!_snapToGrid || _gridSize <= 0.0001f) return v;
        float g = _gridSize;
        return new Vector2(Mathf.Round(v.x / g) * g, Mathf.Round(v.y / g) * g);
    }

    private void UpdateBoxSizesFromBlockSize()
    {
        if (_level == null) return;
        float unit = _level.blockSize > 0 ? _level.blockSize : 0.6f;
        foreach (var b in _level.boxes)
        {
            b.size = new Vector2(Mathf.Max(1, b.columns) * unit, Mathf.Max(1, b.rows) * unit);
        }
    }

    private void AddDefaultConveyor()
    {
        if (_level == null) return;
        Undo.RecordObject(_level, "Add Conveyor");
        var path = new ConveyorPath();
        path.points.Add(new Vector2(-2f, 0f));
        path.points.Add(new Vector2(2f, 0f));
        path.width = 0.3f;
        _level.conveyors.Add(path);
        EditorUtility.SetDirty(_level);
        RefreshLevelList();
        Repaint();
    }

    private void ClearEditorInputFocus()
    {
        GUI.FocusControl(null);
        GUIUtility.keyboardControl = 0;
        GUIUtility.hotControl = 0;
    }

    private void JumpToCurrentLevelAtRuntime()
    {
        if (_level == null) return;
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Runtime jump requires Play mode.");
            return;
        }

        var ctrl = Object.FindObjectOfType<GameRuntimeController>();
        if (ctrl == null)
        {
            Debug.LogWarning("GameRuntimeController not found. Make sure one exists in the scene.");
            return;
        }

        if (TryFindFlowForLevel(_level, out var flow, out var index))
        {
            ctrl.Build(flow, index);
            Debug.Log($"Jumped to level {_level.name} in flow {flow.name} (index {index}).");
            return;
        }

        ctrl.Build(_level);
        Debug.Log($"Jumped to current level at runtime: {_level.name} (no flow found).");
    }

    private void TriggerRuntimeLose()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Runtime trigger requires Play mode.");
            return;
        }

        var ctrl = Object.FindObjectOfType<GameRuntimeController>();
        if (ctrl == null)
        {
            Debug.LogWarning("GameRuntimeController not found. Make sure one exists in the scene.");
            return;
        }

        ctrl.DebugForceLose();
        Debug.Log("Triggered runtime lose.");
    }

    private bool TryFindFlowForLevel(LevelLayout level, out LevelFlow flow, out int index)
    {
        flow = null;
        index = -1;
        if (level == null) return false;

        if (_flowAsset != null && _flowAsset.levels != null)
        {
            index = _flowAsset.levels.IndexOf(level);
            if (index >= 0)
            {
                flow = _flowAsset;
                return true;
            }
        }

        var configPath = $"{RuntimeConfigResourcesFolder}/LevelRuntimeConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<LevelRuntimeConfig>(configPath);
        if (config != null && config.activeFlow != null && config.activeFlow.levels != null)
        {
            index = config.activeFlow.levels.IndexOf(level);
            if (index >= 0)
            {
                flow = config.activeFlow;
                return true;
            }
        }

        var guids = AssetDatabase.FindAssets("t:LevelFlow", new[] { FlowsFolder });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var candidate = AssetDatabase.LoadAssetAtPath<LevelFlow>(path);
            if (candidate == null || candidate.levels == null) continue;
            index = candidate.levels.IndexOf(level);
            if (index >= 0)
            {
                flow = candidate;
                return true;
            }
        }

        index = -1;
        return false;
    }

    private void OnSceneGUI(SceneView view)
    {
        if (IsSimRunning()) return;
        if (_level == null) return;

        // Drag conveyor points
        Handles.color = Color.cyan;
        if (_level.conveyors != null)
        {
            for (int ci = 0; ci < _level.conveyors.Count; ci++)
            {
                bool allowDrag = _selectedConveyor < 0 || _selectedConveyor == ci;
                if (_onlyShowSelectedConveyor && _selectedConveyor != ci) continue;
                if (!allowDrag) continue;

                var conv = _level.conveyors[ci];
                if (conv.points == null) continue;
                for (int i = 0; i < conv.points.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();
                    var wp = new Vector3(conv.points[i].x, conv.points[i].y, 0f);
                    float size = HandleUtility.GetHandleSize(wp) * 0.1f;
                    var newPos = Handles.FreeMoveHandle(wp, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_level, "Move Conveyor Point");
                        conv.points[i] = SnapIfNeeded(new Vector2(newPos.x, newPos.y));
                        EditorUtility.SetDirty(_level);
                        Repaint();
                    }
                }
            }
        }

        // Drag boxes
        Handles.color = Color.yellow;
        if (_level.boxes != null)
        {
            for (int i = 0; i < _level.boxes.Count; i++)
            {
                var box = _level.boxes[i];
                EditorGUI.BeginChangeCheck();
                var pos = Handles.PositionHandle(new Vector3(box.position.x, box.position.y, 0f), Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_level, "Move Box");
                    box.position = SnapIfNeeded(new Vector2(pos.x, pos.y));
                    EditorUtility.SetDirty(_level);
                    Repaint();
                }
            }
        }
    }

    private void StartDsrSimulation()
    {
        CancelDsrSimulation();
        if (_level == null) return;
        _previewSimHoldLastSnapshot = false;
        _previewSimSnapshotHash = 0;
        int layoutId = _level.GetInstanceID();
        _pressureByLayoutId.Remove(layoutId);
        _dsrLastRByLayoutId.Remove(layoutId);
        _dsrSimMaxBoxes = GetMaxBoxCount();
        int seedSalt = _dsrSimRandomizeSeed
            ? unchecked((int)(System.DateTime.UtcNow.Ticks ^ (System.DateTime.UtcNow.Ticks >> 32)))
            : _dsrSimSeed;
        _dsrSim = LoopSorting.Editor.LevelDifficultyMetrics.StartFailureRateSimulation(_level, seedSalt: seedSalt);
        if (_dsrSim == null || _dsrSim.IsDone)
        {
            _dsrSim = null;
            return;
        }
        EditorApplication.update += OnDsrSimulationUpdate;
        Repaint();
    }

    private void CancelDsrSimulation(bool keepSnapshot = false)
    {
        if (_dsrSim != null)
        {
            _dsrSim.Cancel();
            _dsrSim = null;
        }
        if (!keepSnapshot)
        {
            _previewSimHoldLastSnapshot = false;
            _previewSimSnapshotHash = 0;
        }
        EditorApplication.update -= OnDsrSimulationUpdate;
    }

    private void OnDsrSimulationUpdate()
    {
        if (_dsrSim == null)
        {
            EditorApplication.update -= OnDsrSimulationUpdate;
            return;
        }

        if (_level == null || LoopSorting.Editor.LevelDifficultyMetrics.GetLayoutHash(_level) != _dsrSim.LayoutHash)
        {
            CancelDsrSimulation();
            return;
        }

        int ticks = Mathf.Clamp(_dsrSimTicksPerUpdate, 1, 100000);
        double budget = Mathf.Clamp(_dsrSimBudgetMs, 0.1f, 200f) / 1000.0;
        bool done = _dsrSim.Step(ticks, budget);
        if (_dsrSim.TryGetDebugSnapshot(out var snap))
        {
            _previewSimSnapshot = snap;
            _previewSimSnapshotHash = _dsrSim.LayoutHash;
        }
        if (done)
        {
            if (!_dsrSim.Cancelled && _level != null)
            {
                var metrics = LoopSorting.Editor.LevelDifficultyMetrics.ComputeStatic(_level, _dsrSimMaxBoxes, _dsrSim.FailureRate);
                LoopSorting.Editor.LevelDifficultyMetrics.StoreCachedMetrics(_level, metrics);
                int layoutId = _level.GetInstanceID();
                _dsrLastRByLayoutId[layoutId] = _dsrSim.FailureRate;
                StorePressureSnapshot(layoutId, _dsrSim);
                _previewSimHoldLastSnapshot = false;
            }
            CancelDsrSimulation(keepSnapshot: _previewSimHoldLastSnapshot);
        }

        Repaint();
    }

    private bool IsStrategyCompareRunning()
    {
        return _strategyCompareBatch != null &&
               _strategyCompareBatch.ActiveSim != null &&
               !_strategyCompareBatch.ActiveSim.IsDone;
    }

    private void StartStrategyCompareSimulation()
    {
        CancelDsrSimulation();
        CancelStrategyCompareSimulation();
        if (_level == null) return;

        int layoutId = _level.GetInstanceID();
        _strategyPressureByLayoutId.Remove(layoutId);
        int seedSalt = _dsrSimRandomizeSeed
            ? unchecked((int)(System.DateTime.UtcNow.Ticks ^ (System.DateTime.UtcNow.Ticks >> 32)))
            : _dsrSimSeed;

        _strategyCompareBatch = new StrategyCompareBatch
        {
            LayoutId = layoutId,
            LayoutHash = LoopSorting.Editor.LevelDifficultyMetrics.GetLayoutHash(_level),
            SeedSalt = seedSalt,
            ActiveIndex = 0,
            Strategies = StrategyCompareOrder
        };

        if (_strategyCompareBatch.Strategies == null || _strategyCompareBatch.Strategies.Length == 0)
        {
            _strategyCompareBatch = null;
            return;
        }

        var firstStrategy = _strategyCompareBatch.Strategies[0];
        _strategyCompareBatch.ActiveSim = LoopSorting.Editor.LevelDifficultyMetrics.StartFailureRateSimulation(
            _level,
            seedSalt: seedSalt,
            strategy: firstStrategy);

        if (_strategyCompareBatch.ActiveSim == null)
        {
            _strategyCompareBatch = null;
            return;
        }

        EditorApplication.update += OnStrategyCompareUpdate;
        Repaint();
    }

    private void CancelStrategyCompareSimulation()
    {
        if (_strategyCompareBatch != null && _strategyCompareBatch.ActiveSim != null)
        {
            _strategyCompareBatch.ActiveSim.Cancel();
        }
        _strategyCompareBatch = null;
        EditorApplication.update -= OnStrategyCompareUpdate;
    }

    private void OnStrategyCompareUpdate()
    {
        if (_strategyCompareBatch == null || _strategyCompareBatch.ActiveSim == null)
        {
            EditorApplication.update -= OnStrategyCompareUpdate;
            return;
        }

        if (_level == null || LoopSorting.Editor.LevelDifficultyMetrics.GetLayoutHash(_level) != _strategyCompareBatch.LayoutHash)
        {
            CancelStrategyCompareSimulation();
            return;
        }

        int ticks = Mathf.Clamp(_dsrSimTicksPerUpdate, 1, 100000);
        double budget = Mathf.Clamp(_dsrSimBudgetMs, 0.1f, 200f) / 1000.0;
        bool done = _strategyCompareBatch.ActiveSim.Step(ticks, budget);
        if (done)
        {
            var sim = _strategyCompareBatch.ActiveSim;
            var peaks = sim.PeakCounts;
            if (peaks != null && peaks.Count > 0)
            {
                var peakCopy = new int[peaks.Count];
                for (int i = 0; i < peakCopy.Length; i++)
                {
                    peakCopy[i] = peaks[i];
                }
                _strategyCompareBatch.PeaksByStrategy[sim.Strategy] = peakCopy;
                if (_strategyCompareBatch.BeltLength <= 0)
                {
                    _strategyCompareBatch.BeltLength = sim.BeltLength;
                }
            }

            _strategyCompareBatch.ActiveIndex++;
            if (_strategyCompareBatch.ActiveIndex < _strategyCompareBatch.Strategies.Length)
            {
                var nextStrategy = _strategyCompareBatch.Strategies[_strategyCompareBatch.ActiveIndex];
                _strategyCompareBatch.ActiveSim = LoopSorting.Editor.LevelDifficultyMetrics.StartFailureRateSimulation(
                    _level,
                    seedSalt: _strategyCompareBatch.SeedSalt,
                    strategy: nextStrategy);
                if (_strategyCompareBatch.ActiveSim == null)
                {
                    FinishStrategyCompare();
                    return;
                }
            }
            else
            {
                FinishStrategyCompare();
                return;
            }
        }

        Repaint();
    }

    private void FinishStrategyCompare()
    {
        if (_strategyCompareBatch != null)
        {
            _strategyPressureByLayoutId[_strategyCompareBatch.LayoutId] = new StrategyPressureSnapshot
            {
                BeltLength = _strategyCompareBatch.BeltLength,
                PeaksByStrategy = _strategyCompareBatch.PeaksByStrategy
            };
        }
        CancelStrategyCompareSimulation();
        Repaint();
    }

    private void StorePressureSnapshot(int layoutId, LoopSorting.Editor.LevelDifficultyMetrics.FailureRateSimulation sim)
    {
        if (sim == null || sim.PeakCounts == null || sim.PeakCounts.Count == 0) return;
        var peaks = new int[sim.PeakCounts.Count];
        for (int i = 0; i < peaks.Length; i++)
        {
            peaks[i] = sim.PeakCounts[i];
        }
        _pressureByLayoutId[layoutId] = new PressureSnapshot
        {
            BeltLength = sim.BeltLength,
            Peaks = peaks
        };
    }

    private void DrawDsrSimPreview(LoopSorting.Editor.LevelDifficultyMetrics.SimulationDebugSnapshot snap)
    {
        EditorGUILayout.LabelField(
            $"Run {snap.RunIndex + 1}/{_dsrSim.RunsTotal}  Tick {snap.Tick}  Belt {snap.BeltCount}/{snap.BeltLength}  NoInsert {snap.NoInsertWhileFull}");

        if (snap.IsReleasing)
        {
            EditorGUILayout.LabelField($"Releasing box {snap.ActiveReleaseIndex} color {snap.ActiveReleaseColor} pending {snap.PendingRelease}");
        }
        else
        {
            EditorGUILayout.LabelField("Releasing none");
        }

        if (snap.BeltSlots != null && snap.BeltSlots.Length > 0)
        {
            const float cell = 10f;
            const float pad = 2f;
            float available = EditorGUIUtility.currentViewWidth - 60f;
            int perRow = Mathf.Clamp(Mathf.FloorToInt(available / (cell + pad)), 4, snap.BeltSlots.Length);
            int rows = Mathf.CeilToInt(snap.BeltSlots.Length / (float)perRow);
            float height = rows * (cell + pad) + pad;
            var rect = GUILayoutUtility.GetRect(1f, height);

            for (int i = 0; i < snap.BeltSlots.Length; i++)
            {
                int row = i / perRow;
                int col = i % perRow;
                float x = rect.x + pad + col * (cell + pad);
                float y = rect.y + pad + row * (cell + pad);
                var r = new Rect(x, y, cell, cell);
                var color = snap.BeltSlots[i].HasValue
                    ? BlockVisual.ToUnityColor(snap.BeltSlots[i].Value)
                    : new Color(0.2f, 0.2f, 0.2f, 0.2f);
                EditorGUI.DrawRect(r, color);
            }
        }

        if (snap.Containers != null && snap.Containers.Length > 0)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < snap.Containers.Length; i++)
            {
                var c = snap.Containers[i];
                char front = c.FrontColor.HasValue ? c.FrontColor.Value.ToString()[0] : '-';
                sb.Append($"{c.Index}:{c.Count}/{c.Capacity}{front}");
                if (c.Locked) sb.Append("L");
                if (c.Busy) sb.Append("B");
                if (c.Completed) sb.Append("C");
                if (i < snap.Containers.Length - 1) sb.Append("  ");
            }
            EditorGUILayout.LabelField(sb.ToString(), EditorStyles.wordWrappedMiniLabel);
        }
    }
}
