using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LoopSorting.Editor
{
    public static class Box3DGenerator
    {
        private const int DefaultColumns = 3;
        private const int DefaultRows = 8;
        private const float DefaultCellSize = 1f;
        private const float DefaultWallThickness = 1.0f;
        private const float DefaultWallDepth = 1f;
        private const float DefaultFloorThickness = 0.18f;
        private const float DefaultGrooveDepth = 0.4f;
        private const float DefaultGridThickness = 0.04f;
        // Match Brick3DGenerator base roundness after normalization: 0.40 / 2 = 0.20 (per cell size 1).
        private const float BrickCornerRadiusRatio = 0.2f;
        // Make the box inner corners a bit squarer than the brick to avoid corner clashes.
        private const float BoxCornerRadiusScale = 0.85f;
        private const float BoxInnerCornerRadiusRatio = BrickCornerRadiusRatio * BoxCornerRadiusScale;
        private const float DefaultCornerRadius = DefaultWallThickness + (BoxInnerCornerRadiusRatio * DefaultCellSize);
        private const int DefaultCornerSegments = 16;
        private const float DefaultEdgeRadius = BoxInnerCornerRadiusRatio;
        private const int DefaultEdgeSegments = 6;
        private const float DefaultCellCornerRadius = BoxInnerCornerRadiusRatio;
        private const OpeningSide DefaultOpeningSide = OpeningSide.Top;
        private const float DefaultOpeningWidthCells = 2.8f;
        private const float DefaultLidThickness = 0.16f;
        private const float DefaultLidFrameWidth = 0.18f;
        private const float DefaultLidFrontOffset = -0.02f;
        private const float DefaultGlassInset = 0.1f;
        private const float DefaultGlassAlpha = 0.35f;
        private const float DefaultLidGlassWidthRatio = 0.5f;
        private const float DefaultLidGlassHeightRatio = 0.75f;
        private const float DefaultMouthInsetFrac = 0.18f;
        private const float DefaultMouthBandExtraFrac = 0.35f;
        private const bool DebugDisableCavityMouthFade = false;
        private const bool DebugDisableCavityEdgeClip = false;
        private const bool DebugForceFlatCavity = false;
        private const bool DebugLogCavityGen = false;

        private const string RootFolder = "Assets/Art3D";
        private const string MeshFolder = "Assets/Art3D/Meshes";
        private const string MaterialFolder = "Assets/Art3D/Materials";
        private const string ResourcePrefabFolder = "Assets/Resources/Art3D";

        private enum OpeningSide
        {
            Top,
            Right,
            Bottom,
            Left
        }

        [MenuItem("LoopSorting/Art/Generate 3D Box 3x8")]
        private static void Generate3x8()
        {
            GenerateBoxPrefab(
                DefaultColumns,
                DefaultRows,
                DefaultCellSize,
                DefaultWallThickness,
                DefaultWallDepth,
                DefaultFloorThickness,
                DefaultGrooveDepth,
                DefaultGridThickness,
                DefaultCornerRadius,
                DefaultCornerSegments,
                DefaultEdgeRadius,
                DefaultEdgeSegments,
                DefaultOpeningSide,
                DefaultOpeningWidthCells,
                DefaultLidThickness,
                DefaultLidFrameWidth,
                DefaultLidFrontOffset,
                DefaultGlassInset,
                DefaultGlassAlpha);
        }

        private static void GenerateBoxPrefab(
            int columns,
            int rows,
            float cellSize,
            float wallThickness,
            float wallDepth,
            float floorThickness,
            float grooveDepth,
            float gridThickness,
            float cornerRadius,
            int cornerSegments,
            float edgeRadius,
            int edgeSegments,
            OpeningSide openingSide,
            float openingWidthCells,
            float lidThickness,
            float lidFrameWidth,
            float lidFrontOffset,
            float glassInset,
            float glassAlpha)
        {
            EnsureFolder("Assets", "Art3D");
            EnsureFolder(RootFolder, "Meshes");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Art3D");

            string bodyMeshPath = $"{MeshFolder}/Box3x8_Body.asset";
            string cavityMeshPath = $"{MeshFolder}/Box3x8_Cavity.asset";
            string lidFrameMeshPath = $"{MeshFolder}/Box3x8_LidFrame.asset";
            string lidGlassMeshPath = $"{MeshFolder}/Box3x8_LidGlass.asset";
            string bodyMatPath = $"{MaterialFolder}/Box3D_Body.mat";
            string cavityMatPath = $"{MaterialFolder}/Box3D_Cavity.mat";
            string lidMatPath = $"{MaterialFolder}/Box3D_Lid.mat";
            string glassMatPath = $"{MaterialFolder}/Box3D_Glass.mat";
            string resourcePrefabPath = $"{ResourcePrefabFolder}/Box3x8.prefab";

            float openingWidth = Mathf.Clamp(openingWidthCells * cellSize, 0f, columns * cellSize);
            var bodyMesh = BuildTrayMesh(
                columns,
                rows,
                cellSize,
                wallThickness,
                wallDepth,
                floorThickness,
                cornerRadius,
                cornerSegments,
                edgeRadius,
                edgeSegments,
                openingSide,
                openingWidth);
            var cavityMesh = BuildCavityGridMesh(
                columns,
                rows,
                cellSize,
                wallThickness,
                wallDepth,
                floorThickness,
                grooveDepth,
                gridThickness,
                cornerRadius,
                cornerSegments,
                edgeRadius,
                DefaultCellCornerRadius,
                openingSide,
                openingWidth);
            var lidFrameMesh = BuildLidFrameMesh(
                columns,
                rows,
                cellSize,
                wallThickness,
                lidThickness,
                lidFrameWidth,
                cornerRadius,
                cornerSegments);
            var lidGlassMesh = BuildLidGlassMesh(
                columns,
                rows,
                cellSize,
                wallThickness,
                lidThickness,
                lidFrameWidth,
                glassInset,
                cornerRadius,
                cornerSegments);

            var bodyMeshAsset = SaveOrUpdateMesh(bodyMesh, bodyMeshPath);
            var cavityMeshAsset = SaveOrUpdateMesh(cavityMesh, cavityMeshPath);
            var lidFrameMeshAsset = SaveOrUpdateMesh(lidFrameMesh, lidFrameMeshPath);
            var lidGlassMeshAsset = SaveOrUpdateMesh(lidGlassMesh, lidGlassMeshPath);
            if (DebugLogCavityGen && cavityMeshAsset != null)
            {
                Debug.Log($"Box3DGenerator: Cavity verts={cavityMeshAsset.vertexCount} bounds={cavityMeshAsset.bounds}");
            }

            var bodyColor = new Color(0.87f, 0.55f, 0.35f, 1f);
            var cavityColor = new Color(bodyColor.r * 0.9f, bodyColor.g * 0.9f, bodyColor.b * 0.9f, 1f);
            var bodyMat = LoadOrCreateMaterial(bodyMatPath, bodyColor);
            var cavityMat = LoadOrCreateMaterial(cavityMatPath, cavityColor);
            var lidMat = LoadOrCreateMaterial(lidMatPath, new Color(0.9f, 0.58f, 0.38f, 1f));
            var glassMat = LoadOrCreateGlassMaterial(glassMatPath, new Color(0.9f, 0.95f, 0.98f, 1f), glassAlpha);
            if (bodyMat != null && bodyMat.HasProperty("_Cull"))
            {
                bodyMat.SetFloat("_Cull", 0f);
                EditorUtility.SetDirty(bodyMat);
            }
            if (cavityMat != null && cavityMat.HasProperty("_Cull"))
            {
                cavityMat.SetFloat("_Cull", 0f);
                EditorUtility.SetDirty(cavityMat);
            }
            if (lidMat != null && lidMat.HasProperty("_Cull"))
            {
                lidMat.SetFloat("_Cull", 0f);
                EditorUtility.SetDirty(lidMat);
            }

            var root = new GameObject("Box3D_3x8");
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            var bodyFilter = body.AddComponent<MeshFilter>();
            bodyFilter.sharedMesh = bodyMeshAsset;
            var bodyRenderer = body.AddComponent<MeshRenderer>();
            bodyRenderer.sharedMaterial = bodyMat;

            var cavity = new GameObject("CavityGrid");
            cavity.transform.SetParent(root.transform, false);
            var cavityFilter = cavity.AddComponent<MeshFilter>();
            cavityFilter.sharedMesh = cavityMeshAsset;
            var cavityRenderer = cavity.AddComponent<MeshRenderer>();
            cavityRenderer.sharedMaterial = cavityMat;

            var lid = new GameObject("Lid");
            lid.transform.SetParent(root.transform, false);
            lid.transform.localPosition = new Vector3(0f, 0f, lidFrontOffset);

            var lidFrame = new GameObject("Frame");
            lidFrame.transform.SetParent(lid.transform, false);
            var lidFrameFilter = lidFrame.AddComponent<MeshFilter>();
            lidFrameFilter.sharedMesh = lidFrameMeshAsset;
            var lidFrameRenderer = lidFrame.AddComponent<MeshRenderer>();
            lidFrameRenderer.sharedMaterial = lidMat;

            var lidGlass = new GameObject("Glass");
            lidGlass.transform.SetParent(lid.transform, false);
            var lidGlassFilter = lidGlass.AddComponent<MeshFilter>();
            lidGlassFilter.sharedMesh = lidGlassMeshAsset;
            var lidGlassRenderer = lidGlass.AddComponent<MeshRenderer>();
            lidGlassRenderer.sharedMaterial = glassMat;

            var resourcePrefab = PrefabUtility.SaveAsPrefabAsset(root, resourcePrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (resourcePrefab != null)
            {
                Selection.activeObject = resourcePrefab;
                EditorGUIUtility.PingObject(resourcePrefab);
            }
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }

        private static Mesh SaveOrUpdateMesh(Mesh mesh, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
                return mesh;
            }

            existing.Clear();
            existing.indexFormat = mesh.indexFormat;
            existing.vertices = mesh.vertices;
            existing.normals = mesh.normals;
            existing.uv = mesh.uv;
            if (mesh.colors != null && mesh.colors.Length == mesh.vertexCount)
            {
                existing.colors = mesh.colors;
            }
            if (mesh.colors32 != null && mesh.colors32.Length == mesh.vertexCount)
            {
                existing.colors32 = mesh.colors32;
            }
            if (mesh.tangents != null && mesh.tangents.Length == mesh.vertexCount)
            {
                existing.tangents = mesh.tangents;
            }

            existing.subMeshCount = mesh.subMeshCount;
            for (int i = 0; i < mesh.subMeshCount; i++)
            {
                existing.SetTriangles(mesh.GetTriangles(i), i);
            }

            existing.RecalculateBounds();
            if (existing.normals == null || existing.normals.Length != existing.vertexCount)
            {
                existing.RecalculateNormals();
            }

            existing.name = mesh.name;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        private static Material LoadOrCreateMaterial(string path, Color color)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("LoopSorting/UnlitRim") ?? Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader)
                {
                    color = color,
                    name = System.IO.Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Ambient")) mat.SetFloat("_Ambient", 1.0f);
            if (mat.HasProperty("_RimStrength")) mat.SetFloat("_RimStrength", 0.2f);
            if (mat.HasProperty("_RimPower")) mat.SetFloat("_RimPower", 2.5f);
            if (mat.HasProperty("_FakeLightDir")) mat.SetVector("_FakeLightDir", new Vector4(0f, 0f, 1f, 0f));
            if (mat.HasProperty("_FakeLightStrength")) mat.SetFloat("_FakeLightStrength", 0.25f);
            if (mat.HasProperty("_TopLightDir")) mat.SetVector("_TopLightDir", new Vector4(0f, 0f, -1f, 0f));
            if (mat.HasProperty("_ViewLightStrength")) mat.SetFloat("_ViewLightStrength", 0.95f);
            if (mat.HasProperty("_ViewPower")) mat.SetFloat("_ViewPower", 1.6f);
            if (mat.HasProperty("_ViewSideMin")) mat.SetFloat("_ViewSideMin", 0.62f);
            if (mat.HasProperty("_Curv")) mat.SetFloat("_Curv", 0.12f);
            if (mat.HasProperty("_EdgeDarken")) mat.SetFloat("_EdgeDarken", 0.1f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material LoadOrCreateGlassMaterial(string path, Color tint, float alpha)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("LoopSorting/Box3DGlass") ??
                         Shader.Find("Unlit/Transparent") ??
                         Shader.Find("Legacy Shaders/Transparent/Diffuse") ??
                         Shader.Find("Universal Render Pipeline/Unlit") ??
                         Shader.Find("Standard");
            if (mat == null)
            {
                mat = new Material(shader)
                {
                    name = System.IO.Path.GetFileNameWithoutExtension(path)
                };
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (shader != null && mat.shader != shader)
            {
                mat.shader = shader;
            }

            ConfigureTransparentMaterial(mat, tint, alpha);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void ConfigureTransparentMaterial(Material mat, Color tint, float alpha)
        {
            if (mat == null) return;
            var color = new Color(tint.r, tint.g, tint.b, Mathf.Clamp01(alpha));

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.color = color;
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
            if (mat.HasProperty("_AlphaClip")) mat.SetFloat("_AlphaClip", 0f);
            if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 3f);
            if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
            if (mat.HasProperty("_Cull")) mat.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off);

            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        }

        private static Mesh BuildTrayMesh(
            int columns,
            int rows,
            float cellSize,
            float wallThickness,
            float wallDepth,
            float floorThickness,
            float cornerRadius,
            int cornerSegments,
            float edgeRadius,
            int edgeSegments,
            OpeningSide openingSide,
            float openingWidth)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.1f, cellSize);
            wallThickness = Mathf.Clamp(wallThickness, 0.02f, cellSize * 0.5f);
            wallDepth = Mathf.Max(0.1f, wallDepth);
            floorThickness = Mathf.Clamp(floorThickness, 0.02f, wallDepth * 0.9f);
            edgeSegments = Mathf.Max(1, edgeSegments);

            float innerWidth = columns * cellSize;
            float innerHeight = rows * cellSize;
            float outerWidth = innerWidth + wallThickness * 2f;
            float outerHeight = innerHeight + wallThickness * 2f;
            float outerHalfX = outerWidth * 0.5f;
            float outerHalfY = outerHeight * 0.5f;
            float innerHalfX = innerWidth * 0.5f;
            float innerHalfY = innerHeight * 0.5f;
            float depth = wallDepth;
            float cavityDepth = Mathf.Clamp(depth - floorThickness, depth * 0.35f, depth);
            float outerRadius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(outerHalfX, outerHalfY));
            float innerRadius = Mathf.Clamp(outerRadius - wallThickness, 0f, Mathf.Min(innerHalfX, innerHalfY));
            float maxEdgeRadius = Mathf.Max(0f, Mathf.Min(outerHalfX, outerHalfY, innerHalfX, innerHalfY) - 0.02f);
            float maxEdgeDepth = Mathf.Max(0f, Mathf.Min(depth, cavityDepth) * 0.45f);
            edgeRadius = Mathf.Clamp(edgeRadius, 0f, Mathf.Min(maxEdgeRadius, maxEdgeDepth));
            float openingInset = Mathf.Clamp(wallThickness * 0.5f, 0.02f, Mathf.Min(innerHalfX, innerHalfY) * 0.25f);
            float openingAxisLimit = (openingSide == OpeningSide.Top || openingSide == OpeningSide.Bottom)
                ? Mathf.Max(0f, innerHalfX - openingInset)
                : Mathf.Max(0f, innerHalfY - openingInset);
            float maxOpening = openingAxisLimit * 2f;
            openingWidth = Mathf.Clamp(openingWidth, 0f, maxOpening);
            float targetInnerRadius = Mathf.Clamp(cellSize * BoxInnerCornerRadiusRatio, 0f, Mathf.Min(innerHalfX, innerHalfY));
            innerRadius = Mathf.Min(innerRadius, targetInnerRadius);

            var builder = new MeshBuilder();
            var outerPts = BuildRoundedRectPoints(outerWidth, outerHeight, outerRadius, cornerSegments);
            var innerPts = BuildRoundedRectPoints(innerWidth, innerHeight, innerRadius, cornerSegments);

            // Front rim (rounded ring, z=0, facing camera).
            AddRingWithOpening(
                builder,
                outerPts,
                innerPts,
                0f,
                Vector3.back,
                openingSide,
                openingWidth,
                openingAxisLimit);

            // Keep walls planar; rely on smoothed normals for soft edge appearance.
            AddExtrudedWallWithOpening(builder, outerPts, 0f, depth, outward: true, openingSide, openingWidth, openingAxisLimit);

            // Back face.
            AddPolygonFan(builder, outerPts, depth, Vector3.forward);

            AddExtrudedWallWithOpening(builder, innerPts, 0f, cavityDepth, outward: false, openingSide, openingWidth, openingAxisLimit);

            AddOpeningEdgeCaps(
                builder,
                openingSide,
                openingWidth,
                openingAxisLimit,
                0f,
                depth,
                outerHalfX,
                outerHalfY,
                outerRadius,
                innerHalfX,
                innerHalfY,
                innerRadius);

            var mesh = builder.Build("Box3x8_Body");
            WeldAndSmoothMesh(mesh, 0.0001f);
            return mesh;
        }

        private static Mesh BuildCavityGridMesh(
            int columns,
            int rows,
            float cellSize,
            float wallThickness,
            float wallDepth,
            float floorThickness,
            float grooveDepth,
            float gridThickness,
            float cornerRadius,
            int cornerSegments,
            float edgeRadius,
            float cellCornerRadius,
            OpeningSide openingSide,
            float openingWidth)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.1f, cellSize);
            wallDepth = Mathf.Max(0.1f, wallDepth);
            floorThickness = Mathf.Clamp(floorThickness, 0.02f, wallDepth * 0.9f);
            grooveDepth = Mathf.Max(0.01f, grooveDepth);
            gridThickness = Mathf.Clamp(gridThickness, 0.01f, cellSize * 0.4f);

            float innerWidth = columns * cellSize;
            float innerHeight = rows * cellSize;
            float innerHalfX = innerWidth * 0.5f;
            float innerHalfY = innerHeight * 0.5f;
            float cavityDepth = Mathf.Clamp(wallDepth - floorThickness, wallDepth * 0.35f, wallDepth);
            float grooveBottom = Mathf.Clamp(cavityDepth + grooveDepth, cavityDepth + 0.005f, wallDepth - 0.01f);
            float innerRadius = Mathf.Clamp(cornerRadius - wallThickness, 0f, Mathf.Min(innerHalfX, innerHalfY));
            float targetInnerRadius = Mathf.Clamp(cellSize * BoxInnerCornerRadiusRatio, 0f, Mathf.Min(innerHalfX, innerHalfY));
            innerRadius = Mathf.Min(innerRadius, targetInnerRadius);
            cellCornerRadius = targetInnerRadius;
            float mouthInset = Mathf.Clamp(cellSize * DefaultMouthInsetFrac, 0.05f, cellSize * 0.45f);
            float openingHalf = openingWidth * 0.5f;
            float bandExtra = cellSize * DefaultMouthBandExtraFrac;

            int subDiv = Mathf.Clamp(Mathf.RoundToInt(cellSize * 6f), 5, 10);
            int xCount = columns * subDiv + 1;
            int yCount = rows * subDiv + 1;

            var builder = new MeshBuilder();
            int[,] indices = new int[xCount, yCount];

            float invX = 1f / (xCount - 1);
            float invY = 1f / (yCount - 1);
            for (int y = 0; y < yCount; y++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    float u = x * invX;
                    float v = y * invY;
                    float pxRaw = Mathf.Lerp(-innerHalfX, innerHalfX, u);
                    float pyRaw = Mathf.Lerp(-innerHalfY, innerHalfY, v);

                    float px = pxRaw;
                    float py = pyRaw;

                    float dimple = ComputeCellDimple(pxRaw, pyRaw, innerHalfX, innerHalfY, cellSize, columns, rows, cellCornerRadius);
                    if (!DebugDisableCavityMouthFade && openingWidth > 0.0001f && mouthInset > 0.0001f)
                    {
                        bool inBand = true;
                        switch (openingSide)
                        {
                            case OpeningSide.Top:
                            case OpeningSide.Bottom:
                                float bandX = Mathf.Clamp(openingHalf + bandExtra, 0f, innerHalfX);
                                inBand = Mathf.Abs(px) <= bandX;
                                break;
                            case OpeningSide.Left:
                            case OpeningSide.Right:
                                float bandY = Mathf.Clamp(openingHalf + bandExtra, 0f, innerHalfY);
                                inBand = Mathf.Abs(py) <= bandY;
                                break;
                        }

                        if (inBand)
                        {
                            float mouthDist = 0f;
                            switch (openingSide)
                            {
                                case OpeningSide.Top: mouthDist = innerHalfY - py; break;
                                case OpeningSide.Bottom: mouthDist = innerHalfY + py; break;
                                case OpeningSide.Left: mouthDist = innerHalfX + px; break;
                                case OpeningSide.Right: mouthDist = innerHalfX - px; break;
                            }
                            mouthDist = Mathf.Max(0f, mouthDist);
                            float mouthFade = SmoothStep01(Mathf.Clamp01(mouthDist / mouthInset));
                            dimple *= mouthFade;
                        }
                    }
                    if (!DebugDisableCavityEdgeClip)
                    {
                        float edgeDist = -SignedDistanceRoundedRect(new Vector2(pxRaw, pyRaw), innerHalfX, innerHalfY, innerRadius);
                        if (edgeDist <= 0f)
                        {
                            dimple = 0f;
                        }
                    }

                    if (DebugForceFlatCavity)
                    {
                        dimple = 0f;
                    }
                    float z = Mathf.Lerp(cavityDepth, grooveBottom, dimple);

                    indices[x, y] = builder.AddVertex(new Vector3(px, py, z), Vector3.back, new Vector2(u, v));
                }
            }

            for (int y = 0; y < yCount - 1; y++)
            {
                for (int x = 0; x < xCount - 1; x++)
                {
                    int a = indices[x, y];
                    int b = indices[x + 1, y];
                    int c = indices[x + 1, y + 1];
                    int d = indices[x, y + 1];
                    builder.AddTriangle(a, c, b);
                    builder.AddTriangle(a, d, c);
                }
            }

            var mesh = builder.Build("Box3x8_Cavity");
            mesh.RecalculateNormals();
            return mesh;
        }

        private struct WallRing
        {
            public List<Vector2> Points;
            public float Z;
            public float HalfX;
            public float HalfY;
            public float Radius;
        }

        private struct RingSlice
        {
            public float Offset;
            public float Z;
        }

        private static void AddRoundedWallWithOpening(
            MeshBuilder builder,
            float width,
            float height,
            float radius,
            float zStart,
            float zEnd,
            float edgeRadius,
            int edgeSegments,
            bool outward,
            OpeningSide openingSide,
            float openingWidth,
            int cornerSegments)
        {
            float depth = zEnd - zStart;
            if (depth <= 0.0001f) return;

            edgeSegments = Mathf.Max(1, edgeSegments);
            float maxOffset = Mathf.Max(0f, Mathf.Min(width, height) * 0.5f - 0.02f);
            float maxEdge = Mathf.Min(maxOffset, depth * 0.49f);
            edgeRadius = maxEdge <= 0f ? 0f : Mathf.Clamp(edgeRadius, 0f, maxEdge);

            var slices = new List<RingSlice>();
            void AddSlice(float offset, float z)
            {
                if (slices.Count == 0 || Mathf.Abs(slices[slices.Count - 1].Z - z) > 0.0001f)
                {
                    slices.Add(new RingSlice { Offset = offset, Z = z });
                }
            }

            if (edgeRadius <= 0.0001f)
            {
                AddSlice(0f, zStart);
                AddSlice(0f, zEnd);
            }
            else
            {
                for (int i = 0; i <= edgeSegments; i++)
                {
                    float t = i / (float)edgeSegments;
                    float theta = t * Mathf.PI * 0.5f;
                    float offset = edgeRadius * (1f - Mathf.Cos(theta));
                    float z = zStart + edgeRadius * Mathf.Sin(theta);
                    AddSlice(offset, z);
                }

                float midStart = zStart + edgeRadius;
                float midEnd = zEnd - edgeRadius;
                if (midEnd > midStart + 0.0001f)
                {
                    AddSlice(edgeRadius, midEnd);
                }

                for (int i = edgeSegments - 1; i >= 0; i--)
                {
                    float t = i / (float)edgeSegments;
                    float theta = t * Mathf.PI * 0.5f;
                    float offset = edgeRadius * (1f - Mathf.Cos(theta));
                    float z = zEnd - edgeRadius * Mathf.Sin(theta);
                    AddSlice(offset, z);
                }
            }

            var rings = new List<WallRing>();
            foreach (var slice in slices)
            {
                float widthAt = width - slice.Offset * 2f;
                float heightAt = height - slice.Offset * 2f;
                if (widthAt <= 0.02f || heightAt <= 0.02f) break;
                float radiusAt = Mathf.Clamp(radius - slice.Offset, 0f, Mathf.Min(widthAt, heightAt) * 0.5f);
                rings.Add(new WallRing
                {
                    Points = BuildRoundedRectPoints(widthAt, heightAt, radiusAt, cornerSegments),
                    Z = slice.Z,
                    HalfX = widthAt * 0.5f,
                    HalfY = heightAt * 0.5f,
                    Radius = radiusAt
                });
            }

            for (int i = 0; i < rings.Count - 1; i++)
            {
                AddLoftedWallWithOpening(builder, rings[i], rings[i + 1], outward, openingSide, openingWidth);
            }
        }

        private static void AddLoftedWallWithOpening(
            MeshBuilder builder,
            WallRing ringA,
            WallRing ringB,
            bool outward,
            OpeningSide openingSide,
            float openingWidth)
        {
            if (openingWidth <= 0f)
            {
                AddLoftedWall(builder, ringA, ringB, outward);
                return;
            }

            float axisLimit = (openingSide == OpeningSide.Top || openingSide == OpeningSide.Bottom)
                ? Mathf.Max(0f, ringA.HalfX - ringA.Radius)
                : Mathf.Max(0f, ringA.HalfY - ringA.Radius);
            if (axisLimit <= 0.0001f)
            {
                AddLoftedWall(builder, ringA, ringB, outward);
                return;
            }

            float halfOpen = openingWidth * 0.5f;
            float openMin = Mathf.Clamp(-halfOpen, -axisLimit, axisLimit);
            float openMax = Mathf.Clamp(halfOpen, -axisLimit, axisLimit);
            float eps = 0.0001f;

            int count = Mathf.Min(ringA.Points.Count, ringB.Points.Count);
            if (count < 2) return;

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var p0 = ringA.Points[i];
                var p1 = ringA.Points[j];
                var q0 = ringB.Points[i];
                var q1 = ringB.Points[j];

                if (!IsSegmentOnOpeningSide(p0, p1, openingSide, eps))
                {
                    AddLoftedWallSegment(builder, p0, p1, q0, q1, ringA.Z, ringB.Z, outward);
                    continue;
                }

                float p0Axis = GetOpeningAxis(p0, openingSide);
                float p1Axis = GetOpeningAxis(p1, openingSide);
                float segMin = Mathf.Min(p0Axis, p1Axis);
                float segMax = Mathf.Max(p0Axis, p1Axis);

                if (openMax <= segMin + eps || openMin >= segMax - eps)
                {
                    AddLoftedWallSegment(builder, p0, p1, q0, q1, ringA.Z, ringB.Z, outward);
                    continue;
                }

                if (openMin <= segMin + eps && openMax >= segMax - eps)
                {
                    continue;
                }

                float denom = p1Axis - p0Axis;
                if (Mathf.Abs(denom) < eps)
                {
                    AddLoftedWallSegment(builder, p0, p1, q0, q1, ringA.Z, ringB.Z, outward);
                    continue;
                }

                if (openMin > segMin + eps)
                {
                    float t = (openMin - p0Axis) / denom;
                    var pMid = Vector2.Lerp(p0, p1, t);
                    var qMid = Vector2.Lerp(q0, q1, t);
                    AddLoftedWallSegment(builder, p0, pMid, q0, qMid, ringA.Z, ringB.Z, outward);
                }

                if (openMax < segMax - eps)
                {
                    float t = (openMax - p0Axis) / denom;
                    var pMid = Vector2.Lerp(p0, p1, t);
                    var qMid = Vector2.Lerp(q0, q1, t);
                    AddLoftedWallSegment(builder, pMid, p1, qMid, q1, ringA.Z, ringB.Z, outward);
                }
            }
        }

        private static void AddLoftedWall(MeshBuilder builder, WallRing ringA, WallRing ringB, bool outward)
        {
            int count = Mathf.Min(ringA.Points.Count, ringB.Points.Count);
            if (count < 2) return;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                AddLoftedWallSegment(
                    builder,
                    ringA.Points[i],
                    ringA.Points[j],
                    ringB.Points[i],
                    ringB.Points[j],
                    ringA.Z,
                    ringB.Z,
                    outward);
            }
        }

        private static void AddLoftedWallSegment(
            MeshBuilder builder,
            Vector2 p0,
            Vector2 p1,
            Vector2 q0,
            Vector2 q1,
            float z0,
            float z1,
            bool outward)
        {
            var v0 = new Vector3(p0.x, p0.y, z0);
            var v1 = new Vector3(p1.x, p1.y, z0);
            var v2 = new Vector3(q1.x, q1.y, z1);
            var v3 = new Vector3(q0.x, q0.y, z1);
            var n = Vector3.Cross(v1 - v0, v2 - v0);
            if (n.sqrMagnitude <= 0.0000001f)
            {
                n = Vector3.forward;
            }
            if (outward) n = -n;
            builder.AddQuad(v0, v1, v2, v3, n.normalized);
        }

        private static void AddLoftedRing(
            MeshBuilder builder,
            List<Vector2> ringA,
            float zA,
            List<Vector2> ringB,
            float zB,
            bool flipNormal)
        {
            int count = Mathf.Min(ringA.Count, ringB.Count);
            if (count < 2) return;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var v0 = new Vector3(ringA[i].x, ringA[i].y, zA);
                var v1 = new Vector3(ringA[j].x, ringA[j].y, zA);
                var v2 = new Vector3(ringB[j].x, ringB[j].y, zB);
                var v3 = new Vector3(ringB[i].x, ringB[i].y, zB);
                var n = Vector3.Cross(v1 - v0, v2 - v0);
                if (n.sqrMagnitude <= 0.0000001f)
                {
                    n = Vector3.back;
                }
                if (flipNormal) n = -n;
                builder.AddQuad(v0, v1, v2, v3, n.normalized);
            }
        }

        private static Vector2 ClampToRoundedRect(Vector2 p, float halfX, float halfY, float radius)
        {
            if (radius <= 0f)
            {
                return new Vector2(Mathf.Clamp(p.x, -halfX, halfX), Mathf.Clamp(p.y, -halfY, halfY));
            }

            float innerX = Mathf.Max(0f, halfX - radius);
            float innerY = Mathf.Max(0f, halfY - radius);
            float cx = Mathf.Clamp(p.x, -innerX, innerX);
            float cy = Mathf.Clamp(p.y, -innerY, innerY);
            float dx = p.x - cx;
            float dy = p.y - cy;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            if (dist > radius && dist > 0.000001f)
            {
                float scale = radius / dist;
                return new Vector2(cx + dx * scale, cy + dy * scale);
            }

            return new Vector2(cx + dx, cy + dy);
        }

        private static float SignedDistanceRoundedRect(Vector2 p, float halfX, float halfY, float radius)
        {
            Vector2 b = new Vector2(halfX - radius, halfY - radius);
            Vector2 q = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - b;
            Vector2 maxQ = new Vector2(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f));
            float outside = maxQ.magnitude;
            float inside = Mathf.Min(Mathf.Max(q.x, q.y), 0f);
            return outside + inside - radius;
        }

        private static float ComputeCellDimple(float x, float y, float halfX, float halfY, float cellSize, int columns, int rows, float cellCornerRadius)
        {
            float halfCell = cellSize * 0.5f;
            float localX = Mathf.Repeat(x + halfX, cellSize) - halfCell;
            float localY = Mathf.Repeat(y + halfY, cellSize) - halfCell;
            float corner = Mathf.Clamp(cellCornerRadius, 0f, halfCell - 0.0001f);
            float maxDist = Mathf.Max(0.0001f, halfCell - corner);
            float dist = Mathf.Max(0f, -SignedDistanceRoundedRect(new Vector2(localX, localY), halfCell, halfCell, corner));
            return SmoothStep01(Mathf.Clamp01(dist / maxDist));
        }

        private static float SmoothStep01(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        private static void WeldAndSmoothMesh(Mesh mesh, float tolerance, float minNormalDot = 0f)
        {
            if (mesh == null) return;
            var verts = mesh.vertices;
            var uvs = mesh.uv;
            var normals = mesh.normals;
            var tris = mesh.triangles;
            if (verts == null || verts.Length == 0 || tris == null || tris.Length == 0) return;

            if (normals == null || normals.Length != verts.Length)
            {
                mesh.RecalculateNormals();
                normals = mesh.normals;
            }

            float invTol = tolerance <= 0f ? 100000f : 1f / tolerance;
            minNormalDot = Mathf.Clamp(minNormalDot, -1f, 1f);
            const float normalEps = 0.000001f;
            var map = new Dictionary<Vector3Int, List<int>>(verts.Length);
            var newVerts = new List<Vector3>(verts.Length);
            var newUvs = new List<Vector2>(verts.Length);
            var newNormals = new List<Vector3>(verts.Length);
            var remap = new int[verts.Length];

            for (int i = 0; i < verts.Length; i++)
            {
                var v = verts[i];
                var n = (i < normals.Length) ? normals[i] : Vector3.zero;
                if (n.sqrMagnitude > normalEps) n.Normalize();
                var key = new Vector3Int(
                    Mathf.RoundToInt(v.x * invTol),
                    Mathf.RoundToInt(v.y * invTol),
                    Mathf.RoundToInt(v.z * invTol));

                if (!map.TryGetValue(key, out var bucket))
                {
                    bucket = new List<int>(2);
                    map[key] = bucket;
                }

                int match = -1;
                for (int b = 0; b < bucket.Count; b++)
                {
                    int idx = bucket[b];
                    var bn = newNormals[idx];
                    if (bn.sqrMagnitude <= normalEps || n.sqrMagnitude <= normalEps || Vector3.Dot(bn, n) >= minNormalDot)
                    {
                        match = idx;
                        break;
                    }
                }

                if (match < 0)
                {
                    match = newVerts.Count;
                    newVerts.Add(v);
                    newUvs.Add(i < uvs.Length ? uvs[i] : Vector2.zero);
                    newNormals.Add(n);
                    bucket.Add(match);
                }

                remap[i] = match;
            }

            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = remap[tris[i]];
            }

            mesh.Clear();
            mesh.SetVertices(newVerts);
            mesh.SetUVs(0, newUvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private static Mesh BuildLidFrameMesh(
            int columns,
            int rows,
            float cellSize,
            float wallThickness,
            float lidThickness,
            float lidFrameWidth,
            float cornerRadius,
            int cornerSegments)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.1f, cellSize);
            wallThickness = Mathf.Clamp(wallThickness, 0.02f, cellSize * 0.5f);
            lidThickness = Mathf.Max(0.02f, lidThickness);
            cornerSegments = Mathf.Max(1, cornerSegments);

            float innerWidth = columns * cellSize;
            float innerHeight = rows * cellSize;
            float outerWidth = innerWidth + wallThickness * 2f;
            float outerHeight = innerHeight + wallThickness * 2f;
            float outerHalfX = outerWidth * 0.5f;
            float outerHalfY = outerHeight * 0.5f;
            float outerRadius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(outerHalfX, outerHalfY));

            float maxFrameWidth = Mathf.Max(0.02f, Mathf.Min(outerHalfX, outerHalfY) - 0.02f);
            lidFrameWidth = Mathf.Clamp(lidFrameWidth, 0.02f, maxFrameWidth);

            float targetInnerWidth = Mathf.Clamp(outerWidth * DefaultLidGlassWidthRatio, 0.02f, outerWidth - 0.02f);
            float targetInnerHeight = Mathf.Clamp(outerHeight * DefaultLidGlassHeightRatio, 0.02f, outerHeight - 0.02f);
            float minInnerWidth = Mathf.Max(outerWidth - lidFrameWidth * 2f, 0.02f);
            float minInnerHeight = Mathf.Max(outerHeight - lidFrameWidth * 2f, 0.02f);
            float frameInnerWidth = Mathf.Min(targetInnerWidth, minInnerWidth);
            float frameInnerHeight = Mathf.Min(targetInnerHeight, minInnerHeight);
            float frameInnerHalfX = frameInnerWidth * 0.5f;
            float frameInnerHalfY = frameInnerHeight * 0.5f;
            float frameRadiusScale = Mathf.Min(frameInnerWidth / outerWidth, frameInnerHeight / outerHeight);
            float frameInnerRadius = Mathf.Clamp(outerRadius * frameRadiusScale, 0f, Mathf.Min(frameInnerHalfX, frameInnerHalfY));

            var builder = new MeshBuilder();
            var outerPts = BuildRoundedRectPoints(outerWidth, outerHeight, outerRadius, cornerSegments);
            var innerPts = BuildRoundedRectPoints(frameInnerWidth, frameInnerHeight, frameInnerRadius, cornerSegments);

            float zFront = -lidThickness;
            float zBack = 0f;
            AddRing(builder, outerPts, innerPts, zBack, Vector3.forward);
            AddRing(builder, outerPts, innerPts, zFront, Vector3.back);
            AddExtrudedWall(builder, outerPts, zFront, zBack, outward: true);
            AddExtrudedWall(builder, innerPts, zFront, zBack, outward: false);

            var mesh = builder.Build("Box3x8_LidFrame");
            WeldAndSmoothMesh(mesh, 0.0001f);
            return mesh;
        }

        private static Mesh BuildLidGlassMesh(
            int columns,
            int rows,
            float cellSize,
            float wallThickness,
            float lidThickness,
            float lidFrameWidth,
            float glassInset,
            float cornerRadius,
            int cornerSegments)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.1f, cellSize);
            wallThickness = Mathf.Clamp(wallThickness, 0.02f, cellSize * 0.5f);
            lidThickness = Mathf.Max(0.02f, lidThickness);
            cornerSegments = Mathf.Max(1, cornerSegments);
            glassInset = Mathf.Max(0f, glassInset);

            float innerWidth = columns * cellSize;
            float innerHeight = rows * cellSize;
            float outerWidth = innerWidth + wallThickness * 2f;
            float outerHeight = innerHeight + wallThickness * 2f;
            float outerHalfX = outerWidth * 0.5f;
            float outerHalfY = outerHeight * 0.5f;
            float outerRadius = Mathf.Clamp(cornerRadius, 0f, Mathf.Min(outerHalfX, outerHalfY));

            float maxFrameWidth = Mathf.Max(0.02f, Mathf.Min(outerHalfX, outerHalfY) - 0.02f);
            lidFrameWidth = Mathf.Clamp(lidFrameWidth, 0.02f, maxFrameWidth);

            float targetInnerWidth = Mathf.Clamp(outerWidth * DefaultLidGlassWidthRatio, 0.02f, outerWidth - 0.02f);
            float targetInnerHeight = Mathf.Clamp(outerHeight * DefaultLidGlassHeightRatio, 0.02f, outerHeight - 0.02f);
            float minInnerWidth = Mathf.Max(outerWidth - lidFrameWidth * 2f, 0.02f);
            float minInnerHeight = Mathf.Max(outerHeight - lidFrameWidth * 2f, 0.02f);
            float frameInnerWidth = Mathf.Min(targetInnerWidth, minInnerWidth);
            float frameInnerHeight = Mathf.Min(targetInnerHeight, minInnerHeight);
            float frameInnerHalfX = frameInnerWidth * 0.5f;
            float frameInnerHalfY = frameInnerHeight * 0.5f;
            float maxInset = Mathf.Max(0f, Mathf.Min(frameInnerHalfX, frameInnerHalfY) - 0.01f);
            glassInset = Mathf.Clamp(glassInset, 0f, maxInset);

            float glassWidth = Mathf.Max(0.02f, frameInnerWidth - glassInset * 2f);
            float glassHeight = Mathf.Max(0.02f, frameInnerHeight - glassInset * 2f);
            float glassHalfX = glassWidth * 0.5f;
            float glassHalfY = glassHeight * 0.5f;
            float frameRadiusScale = Mathf.Min(frameInnerWidth / outerWidth, frameInnerHeight / outerHeight);
            float frameInnerRadius = Mathf.Clamp(outerRadius * frameRadiusScale, 0f, Mathf.Min(frameInnerHalfX, frameInnerHalfY));
            float glassRadius = Mathf.Clamp(frameInnerRadius - glassInset, 0f, Mathf.Min(glassHalfX, glassHalfY));

            float depthInset = Mathf.Clamp(lidThickness * 0.2f, 0.01f, lidThickness * 0.45f);
            float zFront = -lidThickness + depthInset;
            float zBack = -depthInset;
            if (zBack <= zFront + 0.005f)
            {
                zFront = -lidThickness;
                zBack = -lidThickness * 0.5f;
            }

            var builder = new MeshBuilder();
            var glassPts = BuildRoundedRectPoints(glassWidth, glassHeight, glassRadius, cornerSegments);
            AddPolygonFan(builder, glassPts, zFront, Vector3.back);
            AddPolygonFan(builder, glassPts, zBack, Vector3.forward);
            AddExtrudedWall(builder, glassPts, zFront, zBack, outward: true);

            var mesh = builder.Build("Box3x8_LidGlass");
            WeldAndSmoothMesh(mesh, 0.0001f);
            return mesh;
        }

        private static void AddRingWithOpening(
            MeshBuilder builder,
            List<Vector2> outer,
            List<Vector2> inner,
            float z,
            Vector3 normal,
            OpeningSide openingSide,
            float openingWidth,
            float openingAxisLimit)
        {
            if (openingWidth <= 0f)
            {
                AddRing(builder, outer, inner, z, normal);
                return;
            }

            if (openingAxisLimit <= 0.0001f)
            {
                AddRing(builder, outer, inner, z, normal);
                return;
            }

            float halfOpen = openingWidth * 0.5f;
            float openMin = Mathf.Clamp(-halfOpen, -openingAxisLimit, openingAxisLimit);
            float openMax = Mathf.Clamp(halfOpen, -openingAxisLimit, openingAxisLimit);
            float eps = 0.0001f;

            int count = Mathf.Min(outer.Count, inner.Count);
            if (count < 2) return;

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var o0 = outer[i];
                var o1 = outer[j];
                var i0 = inner[i];
                var i1 = inner[j];

                if (!IsSegmentOnOpeningSide(o0, o1, openingSide, eps))
                {
                    builder.AddQuad(
                        new Vector3(o0.x, o0.y, z),
                        new Vector3(o1.x, o1.y, z),
                        new Vector3(i1.x, i1.y, z),
                        new Vector3(i0.x, i0.y, z),
                        normal);
                    continue;
                }

                float o0Axis = GetOpeningAxis(o0, openingSide);
                float o1Axis = GetOpeningAxis(o1, openingSide);
                float segMin = Mathf.Min(o0Axis, o1Axis);
                float segMax = Mathf.Max(o0Axis, o1Axis);

                if (openMax <= segMin + eps || openMin >= segMax - eps)
                {
                    builder.AddQuad(
                        new Vector3(o0.x, o0.y, z),
                        new Vector3(o1.x, o1.y, z),
                        new Vector3(i1.x, i1.y, z),
                        new Vector3(i0.x, i0.y, z),
                        normal);
                    continue;
                }

                if (openMin <= segMin + eps && openMax >= segMax - eps)
                {
                    continue;
                }

                float i0Axis = GetOpeningAxis(i0, openingSide);
                float i1Axis = GetOpeningAxis(i1, openingSide);
                float denomOuter = o1Axis - o0Axis;
                float denomInner = i1Axis - i0Axis;
                if (Mathf.Abs(denomOuter) < eps || Mathf.Abs(denomInner) < eps)
                {
                    builder.AddQuad(
                        new Vector3(o0.x, o0.y, z),
                        new Vector3(o1.x, o1.y, z),
                        new Vector3(i1.x, i1.y, z),
                        new Vector3(i0.x, i0.y, z),
                        normal);
                    continue;
                }

                if (openMin > segMin + eps)
                {
                    float tOuter = (openMin - o0Axis) / denomOuter;
                    float tInner = (openMin - i0Axis) / denomInner;
                    var oMid = Vector3.Lerp(new Vector3(o0.x, o0.y, z), new Vector3(o1.x, o1.y, z), tOuter);
                    var iMid = Vector3.Lerp(new Vector3(i0.x, i0.y, z), new Vector3(i1.x, i1.y, z), tInner);
                    builder.AddQuad(
                        new Vector3(o0.x, o0.y, z),
                        oMid,
                        iMid,
                        new Vector3(i0.x, i0.y, z),
                        normal);
                }

                if (openMax < segMax - eps)
                {
                    float tOuter = (openMax - o0Axis) / denomOuter;
                    float tInner = (openMax - i0Axis) / denomInner;
                    var oMid = Vector3.Lerp(new Vector3(o0.x, o0.y, z), new Vector3(o1.x, o1.y, z), tOuter);
                    var iMid = Vector3.Lerp(new Vector3(i0.x, i0.y, z), new Vector3(i1.x, i1.y, z), tInner);
                    builder.AddQuad(
                        oMid,
                        new Vector3(o1.x, o1.y, z),
                        new Vector3(i1.x, i1.y, z),
                        iMid,
                        normal);
                }
            }
        }

        private static void AddRing(MeshBuilder builder, List<Vector2> outer, List<Vector2> inner, float z, Vector3 normal)
        {
            int count = Mathf.Min(outer.Count, inner.Count);
            if (count < 2) return;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var o0 = new Vector3(outer[i].x, outer[i].y, z);
                var o1 = new Vector3(outer[j].x, outer[j].y, z);
                var i1 = new Vector3(inner[j].x, inner[j].y, z);
                var i0 = new Vector3(inner[i].x, inner[i].y, z);
                builder.AddQuad(o0, o1, i1, i0, normal);
            }
        }

        private static void AddOpeningEdgeCaps(
            MeshBuilder builder,
            OpeningSide openingSide,
            float openingWidth,
            float openingAxisLimit,
            float z0,
            float z1,
            float outerHalfX,
            float outerHalfY,
            float outerRadius,
            float innerHalfX,
            float innerHalfY,
            float innerRadius)
        {
            if (openingWidth <= 0f || openingAxisLimit <= 0.0001f) return;

            float halfOpen = openingWidth * 0.5f;
            float openMin = Mathf.Clamp(-halfOpen, -openingAxisLimit, openingAxisLimit);
            float openMax = Mathf.Clamp(halfOpen, -openingAxisLimit, openingAxisLimit);
            if (openMax - openMin <= 0.0001f) return;

            switch (openingSide)
            {
                case OpeningSide.Top:
                {
                    AddOpeningCapAtX(builder, openMin, 1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.left);
                    AddOpeningCapAtX(builder, openMax, 1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.right);
                    break;
                }
                case OpeningSide.Bottom:
                {
                    AddOpeningCapAtX(builder, openMin, -1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.left);
                    AddOpeningCapAtX(builder, openMax, -1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.right);
                    break;
                }
                case OpeningSide.Right:
                {
                    AddOpeningCapAtY(builder, openMin, 1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.down);
                    AddOpeningCapAtY(builder, openMax, 1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.up);
                    break;
                }
                case OpeningSide.Left:
                {
                    AddOpeningCapAtY(builder, openMin, -1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.down);
                    AddOpeningCapAtY(builder, openMax, -1f, z0, z1, outerHalfX, outerHalfY, outerRadius, innerHalfX, innerHalfY, innerRadius, Vector3.up);
                    break;
                }
            }
        }

        private static void AddOpeningCapAtX(
            MeshBuilder builder,
            float x,
            float sideSign,
            float z0,
            float z1,
            float outerHalfX,
            float outerHalfY,
            float outerRadius,
            float innerHalfX,
            float innerHalfY,
            float innerRadius,
            Vector3 normal)
        {
            float outerY = sideSign * GetRoundedRectHalfExtentY(x, outerHalfX, outerHalfY, outerRadius);
            float innerY = sideSign * GetRoundedRectHalfExtentY(x, innerHalfX, innerHalfY, innerRadius);

            var v0 = new Vector3(x, outerY, z0);
            var v1 = new Vector3(x, innerY, z0);
            var v2 = new Vector3(x, innerY, z1);
            var v3 = new Vector3(x, outerY, z1);
            builder.AddQuad(v0, v1, v2, v3, normal);
            builder.AddQuad(v0, v3, v2, v1, -normal);
        }

        private static void AddOpeningCapAtY(
            MeshBuilder builder,
            float y,
            float sideSign,
            float z0,
            float z1,
            float outerHalfX,
            float outerHalfY,
            float outerRadius,
            float innerHalfX,
            float innerHalfY,
            float innerRadius,
            Vector3 normal)
        {
            float outerX = sideSign * GetRoundedRectHalfExtentX(y, outerHalfX, outerHalfY, outerRadius);
            float innerX = sideSign * GetRoundedRectHalfExtentX(y, innerHalfX, innerHalfY, innerRadius);

            var v0 = new Vector3(outerX, y, z0);
            var v1 = new Vector3(innerX, y, z0);
            var v2 = new Vector3(innerX, y, z1);
            var v3 = new Vector3(outerX, y, z1);
            builder.AddQuad(v0, v1, v2, v3, normal);
            builder.AddQuad(v0, v3, v2, v1, -normal);
        }

        private static void AddExtrudedWallWithOpening(
            MeshBuilder builder,
            List<Vector2> pts,
            float z0,
            float z1,
            bool outward,
            OpeningSide openingSide,
            float openingWidth,
            float openingAxisLimit)
        {
            if (openingWidth <= 0f)
            {
                AddExtrudedWall(builder, pts, z0, z1, outward);
                return;
            }

            if (openingAxisLimit <= 0.0001f)
            {
                AddExtrudedWall(builder, pts, z0, z1, outward);
                return;
            }

            float halfOpen = openingWidth * 0.5f;
            float openMin = Mathf.Clamp(-halfOpen, -openingAxisLimit, openingAxisLimit);
            float openMax = Mathf.Clamp(halfOpen, -openingAxisLimit, openingAxisLimit);
            float eps = 0.0001f;

            int count = pts.Count;
            if (count < 2) return;

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var p0 = pts[i];
                var p1 = pts[j];

                if (!IsSegmentOnOpeningSide(p0, p1, openingSide, eps))
                {
                    AddExtrudedWallSegment(builder, p0, p1, z0, z1, outward);
                    continue;
                }

                float p0Axis = GetOpeningAxis(p0, openingSide);
                float p1Axis = GetOpeningAxis(p1, openingSide);
                float segMin = Mathf.Min(p0Axis, p1Axis);
                float segMax = Mathf.Max(p0Axis, p1Axis);

                if (openMax <= segMin + eps || openMin >= segMax - eps)
                {
                    AddExtrudedWallSegment(builder, p0, p1, z0, z1, outward);
                    continue;
                }

                if (openMin <= segMin + eps && openMax >= segMax - eps)
                {
                    continue;
                }

                float denom = p1Axis - p0Axis;
                if (Mathf.Abs(denom) < eps)
                {
                    AddExtrudedWallSegment(builder, p0, p1, z0, z1, outward);
                    continue;
                }

                if (openMin > segMin + eps)
                {
                    float t = (openMin - p0Axis) / denom;
                    var pMid = Vector2.Lerp(p0, p1, t);
                    AddExtrudedWallSegment(builder, p0, pMid, z0, z1, outward);
                }

                if (openMax < segMax - eps)
                {
                    float t = (openMax - p0Axis) / denom;
                    var pMid = Vector2.Lerp(p0, p1, t);
                    AddExtrudedWallSegment(builder, pMid, p1, z0, z1, outward);
                }
            }
        }

        private static void AddExtrudedWallSegment(
            MeshBuilder builder,
            Vector2 p0,
            Vector2 p1,
            float z0,
            float z1,
            bool outward)
        {
            var edge = new Vector2(p1.x - p0.x, p1.y - p0.y);
            if (edge.sqrMagnitude <= 0.0000001f) return;
            // Points are clockwise; use right-hand normal for outward-facing walls.
            var normal2 = new Vector2(-edge.y, edge.x).normalized;
            if (!outward) normal2 = -normal2;
            var normal = new Vector3(normal2.x, normal2.y, 0f);

            var v0 = new Vector3(p0.x, p0.y, z0);
            var v1 = new Vector3(p1.x, p1.y, z0);
            var v2 = new Vector3(p1.x, p1.y, z1);
            var v3 = new Vector3(p0.x, p0.y, z1);
            builder.AddQuad(v0, v1, v2, v3, normal);
        }

        private static void AddExtrudedWall(MeshBuilder builder, List<Vector2> pts, float z0, float z1, bool outward)
        {
            int count = pts.Count;
            if (count < 2) return;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var p0 = pts[i];
                var p1 = pts[j];
                var edge = new Vector2(p1.x - p0.x, p1.y - p0.y);
                // Points are clockwise; use right-hand normal for outward-facing walls.
                var normal2 = new Vector2(-edge.y, edge.x).normalized;
                if (!outward) normal2 = -normal2;
                var normal = new Vector3(normal2.x, normal2.y, 0f);

                var v0 = new Vector3(p0.x, p0.y, z0);
                var v1 = new Vector3(p1.x, p1.y, z0);
                var v2 = new Vector3(p1.x, p1.y, z1);
                var v3 = new Vector3(p0.x, p0.y, z1);
                builder.AddQuad(v0, v1, v2, v3, normal);
            }
        }

        private static float GetOpeningAxis(Vector2 point, OpeningSide openingSide)
        {
            return (openingSide == OpeningSide.Top || openingSide == OpeningSide.Bottom) ? point.x : point.y;
        }

        private static bool IsSegmentOnOpeningSide(
            Vector2 p0,
            Vector2 p1,
            OpeningSide openingSide,
            float epsilon)
        {
            var edge = p1 - p0;
            if (edge.sqrMagnitude <= epsilon * epsilon) return false;
            var normal = new Vector2(-edge.y, edge.x).normalized;
            Vector2 target;
            switch (openingSide)
            {
                case OpeningSide.Top:
                    target = Vector2.up;
                    break;
                case OpeningSide.Bottom:
                    target = Vector2.down;
                    break;
                case OpeningSide.Right:
                    target = Vector2.right;
                    break;
                case OpeningSide.Left:
                    target = Vector2.left;
                    break;
                default:
                    target = Vector2.zero;
                    break;
            }

            return Vector2.Dot(normal, target) >= 0.3f;
        }

        private static void AddPolygonFan(MeshBuilder builder, List<Vector2> pts, float z, Vector3 normal)
        {
            int count = pts.Count;
            if (count < 3) return;

            float area = 0f;
            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                area += pts[i].x * pts[j].y - pts[j].x * pts[i].y;
            }
            bool clockwise = area < 0f;
            bool flip = (clockwise && normal.z > 0f) || (!clockwise && normal.z < 0f);

            var center = Vector2.zero;
            for (int i = 0; i < count; i++) center += pts[i];
            center /= count;

            int centerIndex = builder.VertexCount;
            builder.AddVertex(new Vector3(center.x, center.y, z), normal, new Vector2(0.5f, 0.5f));

            for (int i = 0; i < count; i++)
            {
                int j = (i + 1) % count;
                var v0 = new Vector3(pts[i].x, pts[i].y, z);
                var v1 = new Vector3(pts[j].x, pts[j].y, z);
                int idx0 = builder.AddVertex(v0, normal, new Vector2(0f, 0f));
                int idx1 = builder.AddVertex(v1, normal, new Vector2(1f, 0f));

                if (flip)
                {
                    builder.AddTriangle(centerIndex, idx1, idx0);
                }
                else
                {
                    builder.AddTriangle(centerIndex, idx0, idx1);
                }
            }
        }

        private static List<Vector2> BuildRoundedRectPoints(float width, float height, float radius, int segmentsPerCorner)
        {
            float halfX = width * 0.5f;
            float halfY = height * 0.5f;
            radius = Mathf.Clamp(radius, 0f, Mathf.Min(halfX, halfY));
            segmentsPerCorner = Mathf.Max(1, segmentsPerCorner);

            if (radius <= 0.0001f)
            {
                return new List<Vector2>
                {
                    new Vector2(halfX, halfY),
                    new Vector2(halfX, -halfY),
                    new Vector2(-halfX, -halfY),
                    new Vector2(-halfX, halfY)
                };
            }

            var pts = new List<Vector2>(segmentsPerCorner * 4);
            AddCorner(pts, new Vector2(halfX - radius, halfY - radius), radius, 90f, 0f, segmentsPerCorner, includeStart: true);
            AddCorner(pts, new Vector2(halfX - radius, -halfY + radius), radius, 0f, -90f, segmentsPerCorner, includeStart: false);
            AddCorner(pts, new Vector2(-halfX + radius, -halfY + radius), radius, -90f, -180f, segmentsPerCorner, includeStart: false);
            AddCorner(pts, new Vector2(-halfX + radius, halfY - radius), radius, -180f, -270f, segmentsPerCorner, includeStart: false);
            return pts;
        }

        private static void AddCorner(
            List<Vector2> pts,
            Vector2 center,
            float radius,
            float startAngle,
            float endAngle,
            int segments,
            bool includeStart)
        {
            float step = (endAngle - startAngle) / segments;
            for (int i = 0; i <= segments; i++)
            {
                if (i == 0 && !includeStart) continue;
                float a = (startAngle + step * i) * Mathf.Deg2Rad;
                pts.Add(center + new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
            }
        }

        private static float GetRoundedRectHalfExtentY(float x, float halfX, float halfY, float radius)
        {
            if (radius <= 0f) return halfY;
            float ax = Mathf.Abs(x);
            float cornerX = halfX - radius;
            float cornerY = halfY - radius;
            if (ax <= cornerX) return halfY;
            float dx = ax - cornerX;
            float dy = Mathf.Sqrt(Mathf.Max(0f, radius * radius - dx * dx));
            return cornerY + dy;
        }

        private static float GetRoundedRectHalfExtentX(float y, float halfX, float halfY, float radius)
        {
            if (radius <= 0f) return halfX;
            float ay = Mathf.Abs(y);
            float cornerY = halfY - radius;
            float cornerX = halfX - radius;
            if (ay <= cornerY) return halfX;
            float dy = ay - cornerY;
            float dx = Mathf.Sqrt(Mathf.Max(0f, radius * radius - dy * dy));
            return cornerX + dx;
        }

        private sealed class MeshBuilder
        {
            private readonly List<Vector3> _verts = new List<Vector3>();
            private readonly List<Vector3> _normals = new List<Vector3>();
            private readonly List<Vector2> _uvs = new List<Vector2>();
            private readonly List<int> _tris = new List<int>();

            public int VertexCount => _verts.Count;

            public void AddQuad(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 normal)
            {
                var n = Vector3.Cross(v1 - v0, v2 - v0);
                if (Vector3.Dot(n, normal) < 0f)
                {
                    var tmp = v1;
                    v1 = v3;
                    v3 = tmp;
                }

                int idx = _verts.Count;
                _verts.Add(v0);
                _verts.Add(v1);
                _verts.Add(v2);
                _verts.Add(v3);

                _normals.Add(normal);
                _normals.Add(normal);
                _normals.Add(normal);
                _normals.Add(normal);

                _uvs.Add(new Vector2(0f, 0f));
                _uvs.Add(new Vector2(1f, 0f));
                _uvs.Add(new Vector2(1f, 1f));
                _uvs.Add(new Vector2(0f, 1f));

                _tris.Add(idx);
                _tris.Add(idx + 1);
                _tris.Add(idx + 2);
                _tris.Add(idx);
                _tris.Add(idx + 2);
                _tris.Add(idx + 3);
            }

            public int AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
            {
                int idx = _verts.Count;
                _verts.Add(position);
                _normals.Add(normal);
                _uvs.Add(uv);
                return idx;
            }

            public void AddTriangle(int a, int b, int c)
            {
                _tris.Add(a);
                _tris.Add(b);
                _tris.Add(c);
            }

            public void AddBox(Vector3 min, Vector3 max)
            {
                AddQuad(
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(min.x, max.y, min.z),
                    Vector3.back);
                AddQuad(
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, max.y, max.z),
                    new Vector3(max.x, min.y, max.z),
                    Vector3.forward);
                AddQuad(
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(min.x, min.y, max.z),
                    Vector3.left);
                AddQuad(
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                    Vector3.right);
                AddQuad(
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(max.x, max.y, max.z),
                    new Vector3(min.x, max.y, max.z),
                    Vector3.up);
                AddQuad(
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, min.y, min.z),
                    Vector3.down);
            }

            public Mesh Build(string name)
            {
                var mesh = new Mesh { name = name };
                mesh.SetVertices(_verts);
                mesh.SetNormals(_normals);
                mesh.SetUVs(0, _uvs);
                mesh.SetTriangles(_tris, 0);
                mesh.RecalculateBounds();
                return mesh;
            }
        }
    }
}
