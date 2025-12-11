using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using LoopSorting;

public class LevelEditorWindow : EditorWindow
{
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
    private bool _snapToGrid = false;
    private bool _showSlotMarkers = true;
    private bool _onlyShowSelectedBox = false;
    private bool _onlyShowSelectedConveyor = false;
    private float _gridSize = 0.5f;
    private float _previewSize = 520f;
    private int _selectedBox = -1;
    private int _selectedConveyor = -1;
    private int _draggingConveyor = -1;
    private int _draggingPoint = -1;
    private int _selectedPoint = -1;
    private int _tabIndex = 0;
    private readonly string[] _tabs = new[] { "Levels", "Flow" };
    private LevelFlow _flowAsset;
    private SerializedObject _flowSO;
    private ReorderableList _flowList;
    private LevelLayout _flowAddCandidate;
    private Vector2 _flowScroll;

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
    }

    private void OnGUI()
    {
        DrawHeader();

        EditorGUILayout.BeginHorizontal();
        DrawLevelSidebar();

        _tabIndex = GUILayout.Toolbar(_tabIndex, _tabs);

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
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Loop Sorting Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("选择已有关卡 (Assets/Levels)", EditorStyles.miniBoldLabel);
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

        if (GUILayout.Button("刷新列表", GUILayout.Width(80)))
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

        if (_level != null && GUILayout.Button("保存", GUILayout.Width(60)))
        {
            SaveCurrentLevel();
        }

        if (_level != null && GUILayout.Button("设为运行关卡", GUILayout.Width(100)))
        {
            SetActiveRuntimeLevel(_level);
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

        EditorGUILayout.LabelField("Drag Options", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        _snapToGrid = EditorGUILayout.ToggleLeft("Snap to Grid", _snapToGrid, GUILayout.Width(120));
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
                        drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Boxes (???????size ????capacity ???????)"),
                        drawElementCallback = DrawBoxElement,
                        elementHeightCallback = GetBoxHeight
                    };
                }
                else
                {
                    EditorGUILayout.HelpBox("Missing property: boxes", MessageType.Error);
                }
            }
            _boxesList?.DoLayoutList();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
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
        EditorGUILayout.LabelField("关卡列表", EditorStyles.boldLabel);
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
        EditorGUILayout.LabelField("关卡预览", EditorStyles.boldLabel);
        var rect = GUILayoutUtility.GetRect(_previewSize, _previewSize, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 0.4f));
        }

        if (_level == null)
        {
            EditorGUI.LabelField(rect, "选择或创建一个 LevelLayout", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        var bounds = ComputeBounds(_level);
        var localRect = new Rect(0, 0, rect.width, rect.height);
        GUI.BeginClip(rect);
        Handles.BeginGUI();
        var slotPositions = BuildSlotPositionsForPreview(_level);
        DrawPreviewConveyors(localRect, bounds, slotPositions);
        DrawPreviewBoxes(localRect, bounds, slotPositions);
        HandlePreviewClick(localRect, bounds, slotPositions);
        HandlePreviewDrag(localRect, bounds);
        Handles.EndGUI();
        GUI.EndClip();
        EditorGUILayout.EndVertical();
    }

    private void DrawCreateButtons()
    {
        EditorGUILayout.HelpBox("选择已有的 LevelLayout 资源，或者新建一个。", MessageType.Info);
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

    private void DrawPreviewConveyors(Rect rect, Rect bounds, List<Vector2> slotPositions)
    {
        Handles.color = new Color(0.12f, 0.56f, 0.91f, 0.9f);
        for (int ci = 0; ci < _level.conveyors.Count; ci++)
        {
        if (_onlyShowSelectedConveyor && _selectedConveyor != ci) continue;

        var conveyor = _level.conveyors[ci];
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

    private void DrawPreviewBoxes(Rect rect, Rect bounds, List<Vector2> slotPositions)
    {
        if (_level.boxes == null) return;
        foreach (var box in _level.boxes)
        {
            if (_onlyShowSelectedBox && (_selectedBox < 0 || _level.boxes[_selectedBox] != box))
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
            for (int i = 0; i < 4; i++)
            {
                poly[i] = ToScreen(rect, bounds, rectWorld[i]);
            }

            var face = new Color(box.color.r, box.color.g, box.color.b, 0.25f);
            var outline = (_selectedBox >= 0 && _level.boxes[_selectedBox] == box) ? Color.green : Color.white;
            Handles.DrawSolidRectangleWithOutline(poly, face, outline);
            Handles.Label(ToScreen(rect, bounds, box.position), $"{box.name} ({box.opening})");

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

    private void HandlePreviewClick(Rect rect, Rect bounds, List<Vector2> slotPositions)
    {
        var e = Event.current;
        if (e.type != EventType.MouseDown || e.button != 0) return;
        if (!rect.Contains(e.mousePosition)) return;

        var local = e.mousePosition; // already in local rect because of BeginClip
        var world = ToWorld(rect, bounds, local);

        // point hit first (screen-based threshold)
        var pointHit = FindConveyorPointAt(local, rect, bounds, out int convIndex, out int pointIndex);
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

        int hit = FindBoxAt(world);
        if (hit != -1)
        {
            _selectedBox = hit;
            _selectedConveyor = -1;
            Repaint();
            e.Use();
            return;
        }

        int conv = FindConveyorAt(rect, bounds, local);
        if (conv != -1)
        {
            _selectedConveyor = conv;
            _selectedPoint = -1;
            _selectedBox = -1;
            Repaint();
            e.Use();
            return;
        }
    }

    private void HandlePreviewDrag(Rect rect, Rect bounds)
    {
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
    }

    private int FindBoxAt(Vector2 world)
    {
        if (_level == null || _level.boxes == null) return -1;
        for (int i = 0; i < _level.boxes.Count; i++)
        {
            var b = _level.boxes[i];
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

    private int FindConveyorAt(Rect rect, Rect bounds, Vector2 localMouse)
    {
        if (_level == null || _level.conveyors == null) return -1;
        int best = -1;
        float bestDist = float.MaxValue;
        for (int ci = 0; ci < _level.conveyors.Count; ci++)
        {
            var c = _level.conveyors[ci];
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
        var center = bounds.center;
        var x = viewRect.width * 0.5f + (world.x - center.x) * scale;
        var y = viewRect.height * 0.5f - (world.y - center.y) * scale;
        return new Vector3(x, y, 0f);
    }

    private static Vector2 ToWorld(Rect viewRect, Rect bounds, Vector2 screen)
    {
        var scale = GetScale(viewRect, bounds);
        var center = bounds.center;
        float x = (screen.x - viewRect.width * 0.5f) / scale + center.x;
        float y = (viewRect.height * 0.5f - screen.y) / scale + center.y;
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
        return Mathf.Min(viewRect.width / size.x, viewRect.height / size.y) * 0.9f;
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
        var b = LayoutUtils.ComputeLayoutBounds(level);
        return new Rect(
            new Vector2(b.center.x, b.center.y) - new Vector2(b.size.x, b.size.y) * 0.5f,
            new Vector2(b.size.x, b.size.y)
        );
    }

    private List<Vector2> BuildSlotPositionsForPreview(LevelLayout level)
    {
        var list = new List<Vector2>();
        if (level.conveyors == null || level.conveyors.Count == 0) return list;
        float used;
        float spacing = level.beltSlotSpacing > 0 ? level.beltSlotSpacing : 0.6f;
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
        return box.position + normal * Mathf.Max(half.x, half.y);
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

    private bool FindConveyorPointAt(Vector2 localMouse, Rect rect, Rect bounds, out int conveyorIndex, out int pointIndex)
    {
        conveyorIndex = -1;
        pointIndex = -1;
        if (_level == null || _level.conveyors == null) return false;
        const float screenThreshold = 10f;
        for (int ci = 0; ci < _level.conveyors.Count; ci++)
        {
            var c = _level.conveyors[ci];
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
        Debug.Log($"已保存关卡: {_level.name}");
    }

    private void SetLevel(LevelLayout level)
    {
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
        Repaint();
        RefreshLevelList();
        _selectedIndex = IndexOfLevel(level, _levelOptions);
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
        var guids = AssetDatabase.FindAssets("t:LevelLayout", new[] { "Assets/Levels" });
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
            Debug.Log("LevelEditorWindow: 在 Assets/Levels 下没有找到 LevelLayout 资源。");
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
        if (_flowAsset != null && GUILayout.Button("设为运行Flow", GUILayout.Width(110)))
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
        const string levelsFolder = "Assets/Levels";
        if (!Directory.Exists(levelsFolder))
        {
            Directory.CreateDirectory(levelsFolder);
            AssetDatabase.Refresh();
        }

        var path = EditorUtility.SaveFilePanelInProject("Create Level Layout", "LevelLayout", "asset", "保存 LevelLayout 资源", levelsFolder);
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
        const string levelsFolder = "Assets/Levels";
        if (!Directory.Exists(levelsFolder))
        {
            Directory.CreateDirectory(levelsFolder);
            AssetDatabase.Refresh();
        }

        var path = EditorUtility.SaveFilePanelInProject("Create Level Flow", "LevelFlow", "asset", "保存 LevelFlow 资源", levelsFolder);
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
        var resourcePath = "Assets/Levels/Resources/Levels";
        if (!Directory.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }

        var assetPath = $"{resourcePath}/LevelRuntimeConfig.asset";
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
        Debug.Log($"已将关卡设置为运行关卡: {layout.name}");
    }

    private static void SetActiveRuntimeFlow(LevelFlow flow)
    {
        var resourcePath = "Assets/Levels/Resources/Levels";
        if (!Directory.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }

        var assetPath = $"{resourcePath}/LevelRuntimeConfig.asset";
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
        Debug.Log($"已将 Flow 设置为运行: {flow?.name}");
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

    private void OnSceneGUI(SceneView view)
    {
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
}
