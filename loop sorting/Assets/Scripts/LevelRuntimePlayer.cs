using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Minimal runtime generator that instantiates conveyors and boxes from a LevelLayout.
    /// Replace visuals/logic with your real game components as needed.
    /// </summary>
    public class LevelRuntimePlayer : MonoBehaviour
    {
        [Header("Visual Prefabs (optional)")]
        public GameObject boxPrefab;
        public GameObject conveyorSegmentPrefab;

        private const float DefaultDepth = 0f;

        public void Build(LevelLayout layout)
        {
            if (layout == null)
            {
                Debug.LogWarning("LevelRuntimePlayer.Build called with null layout.");
                return;
            }

            BuildConveyors(layout.conveyors);
            BuildBoxes(layout);
        }

        private void BuildConveyors(List<ConveyorPath> conveyors)
        {
            var parent = new GameObject("Conveyors");
            parent.transform.SetParent(transform, false);

            foreach (var conveyor in conveyors)
            {
                var go = new GameObject(string.IsNullOrEmpty(conveyor.name) ? "Conveyor" : conveyor.name);
                go.transform.SetParent(parent.transform, false);

                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = true;
                lr.positionCount = conveyor.points.Count;
                lr.startWidth = conveyor.width;
                lr.endWidth = conveyor.width;
                var shader =
                    Shader.Find("Sprites/Default") ??
                    Shader.Find("Unlit/Color") ??
                    Shader.Find("UI/Default") ??
                    Shader.Find("Standard");
                if (shader != null)
                {
                    lr.material = new Material(shader);
                }
                lr.startColor = lr.endColor = new Color(0.1f, 0.1f, 0.1f, 1f);

                for (int i = 0; i < conveyor.points.Count; i++)
                {
                    var p = conveyor.points[i];
                    lr.SetPosition(i, new Vector3(p.x, p.y, DefaultDepth));
                }

                if (conveyor.loop && conveyor.points.Count > 1)
                {
                    lr.loop = true;
                }
            }
        }

        private void BuildBoxes(LevelLayout layout)
        {
            var parent = new GameObject("Boxes");
            parent.transform.SetParent(transform, false);

            float unit = layout.blockSize > 0 ? layout.blockSize : 0.6f;

            foreach (var box in layout.boxes)
            {
                int columns = Mathf.Max(1, box.columns);
                int rows = Mathf.Max(1, box.rows);
                int capacity = columns * rows;
                box.size = LayoutUtils.ComputeBoxSize(box, unit);

                GameObject go;
                if (boxPrefab != null)
                {
                    go = Instantiate(boxPrefab, parent.transform);
                    go.name = box.name;
                    go.transform.position = new Vector3(box.position.x, box.position.y, DefaultDepth);
                    go.transform.localScale = new Vector3(box.size.x, box.size.y, 1f);
                }
                else
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = box.name;
                    go.transform.SetParent(parent.transform, false);
                    go.transform.position = new Vector3(box.position.x, box.position.y, DefaultDepth);
                    go.transform.localScale = new Vector3(box.size.x, box.size.y, 0.2f);
                    var renderer = go.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        var shader =
                            Shader.Find("Standard") ??
                            Shader.Find("Universal Render Pipeline/Lit") ??
                            Shader.Find("Unlit/Color") ??
                            Shader.Find("Sprites/Default");
                        if (shader != null)
                        {
                            renderer.sharedMaterial = new Material(shader)
                            {
                                color = box.color
                            };
                        }
                    }
                }

                // Simple label for debugging (child object to avoid MeshFilter conflicts).
                var label = new GameObject("Label");
                label.transform.SetParent(go.transform, false);
                label.transform.localPosition = Vector3.zero;
                var text = label.AddComponent<TextMesh>();
                text.text = $"{box.name}\nCap:{capacity}\n{columns}x{rows}\nOpen:{box.opening}";
                text.characterSize = 0.2f;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.color = Color.black;
                text.transform.localPosition = Vector3.zero;
            }
        }
    }
}
