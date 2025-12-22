using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LoopSorting.Editor
{
    public static class Brick3DGenerator
    {
        private const int DefaultColumns = 2;
        private const int DefaultRows = 2;
        private const float DefaultCellSize = 1f;
        private const float DefaultBaseHeight = 0.8f;
        private const float DefaultBaseRoundness = 0.15f;
        private const int DefaultBaseSegments = 12;
        private const int DefaultHeightSegments = 4;
        private const float DefaultStudRadius = 0.3f;
        private const float DefaultStudHeight = 0.28f;
        private const float DefaultStudEdgeRadius = 0.08f;
        private const int DefaultStudSegments = 24;
        private const int DefaultStudEdgeSegments = 4;

        private const string RootFolder = "Assets/Art3D";
        private const string MeshFolder = "Assets/Art3D/Meshes";
        private const string MaterialFolder = "Assets/Art3D/Materials";
        private const string PrefabFolder = "Assets/Art3D/Bricks";
        private const string ResourcePrefabFolder = "Assets/Resources/Art3D";

        [MenuItem("LoopSorting/Art/Generate 3D Brick 2x2 Rounded")]
        private static void GenerateRoundedBrick()
        {
            GenerateBrickPrefab(
                "Brick2x2Rounded",
                DefaultColumns,
                DefaultRows,
                DefaultCellSize,
                DefaultBaseHeight,
                DefaultBaseRoundness,
                DefaultBaseSegments,
                DefaultHeightSegments,
                DefaultStudRadius,
                DefaultStudHeight,
                DefaultStudEdgeRadius,
                DefaultStudSegments,
                DefaultStudEdgeSegments);
        }

        private static void GenerateBrickPrefab(
            string name,
            int columns,
            int rows,
            float cellSize,
            float baseHeight,
            float baseRoundness,
            int baseSegments,
            int heightSegments,
            float studRadius,
            float studHeight,
            float studEdgeRadius,
            int studSegments,
            int studEdgeSegments)
        {
            EnsureFolder("Assets", "Art3D");
            EnsureFolder(RootFolder, "Meshes");
            EnsureFolder(RootFolder, "Materials");
            EnsureFolder(RootFolder, "Bricks");
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Art3D");

            string combinedMeshPath = $"{MeshFolder}/{name}.asset";
            string baseMeshPath = $"{MeshFolder}/{name}_Base.asset";
            string studsMeshPath = $"{MeshFolder}/{name}_Studs.asset";
            string matPath = $"{MaterialFolder}/{name}.mat";
            string prefabPath = $"{PrefabFolder}/{name}.prefab";
            string resourcePrefabPath = $"{ResourcePrefabFolder}/{name}.prefab";

            BuildBrickMeshes(
                columns,
                rows,
                cellSize,
                baseHeight,
                baseRoundness,
                baseSegments,
                heightSegments,
                studRadius,
                studHeight,
                studEdgeRadius,
                studSegments,
                studEdgeSegments,
                out var baseMesh,
                out var studsMesh);
            baseMesh.name = $"{name}_Base";
            studsMesh.name = $"{name}_Studs";
            var combinedMesh = CombineMeshes(name, baseMesh, studsMesh);
            SaveOrUpdateMesh(combinedMesh, combinedMeshPath);
            var baseMeshAsset = SaveOrUpdateMesh(baseMesh, baseMeshPath);
            var studsMeshAsset = SaveOrUpdateMesh(studsMesh, studsMeshPath);
            var mat = LoadOrCreateMaterial(matPath, Color.white);

            var root = new GameObject(name);
            var basePart = new GameObject("Base");
            basePart.transform.SetParent(root.transform, false);
            var baseFilter = basePart.AddComponent<MeshFilter>();
            baseFilter.sharedMesh = baseMeshAsset;
            var baseRenderer = basePart.AddComponent<MeshRenderer>();
            baseRenderer.sharedMaterial = mat;

            var studsPart = new GameObject("Studs");
            studsPart.transform.SetParent(root.transform, false);
            var studsFilter = studsPart.AddComponent<MeshFilter>();
            studsFilter.sharedMesh = studsMeshAsset;
            var studsRenderer = studsPart.AddComponent<MeshRenderer>();
            studsRenderer.sharedMaterial = mat;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            var resourcePrefab = PrefabUtility.SaveAsPrefabAsset(root, resourcePrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (resourcePrefab != null)
            {
                Selection.activeObject = resourcePrefab;
                EditorGUIUtility.PingObject(resourcePrefab);
            }
            else if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
        }

        private static void BuildBrickMeshes(
            int columns,
            int rows,
            float cellSize,
            float baseHeight,
            float baseRoundness,
            int baseSegments,
            int heightSegments,
            float studRadius,
            float studHeight,
            float studEdgeRadius,
            int studSegments,
            int studEdgeSegments,
            out Mesh baseMesh,
            out Mesh studsMesh)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.05f, cellSize);
            baseHeight = Mathf.Max(0.05f, baseHeight);
            baseSegments = Mathf.Max(2, baseSegments);
            heightSegments = Mathf.Max(1, heightSegments);

            float width = columns * cellSize;
            float height = rows * cellSize;

            float maxRound = Mathf.Min(width * 0.5f, height * 0.5f, baseHeight * 0.5f);
            baseRoundness = Mathf.Clamp(baseRoundness, 0.001f, maxRound);

            baseMesh = BuildRoundedBoxMesh(width, height, baseHeight, baseRoundness, baseSegments, baseSegments, heightSegments);
            studsMesh = BuildStudsMesh(
                columns,
                rows,
                cellSize,
                baseHeight,
                studRadius,
                studHeight,
                studEdgeRadius,
                studSegments,
                studEdgeSegments);
        }

        private static Mesh BuildRoundedBoxMesh(
            float width,
            float height,
            float depth,
            float roundness,
            int xSegments,
            int ySegments,
            int zSegments)
        {
            width = Mathf.Max(0.05f, width);
            height = Mathf.Max(0.05f, height);
            depth = Mathf.Max(0.05f, depth);
            xSegments = Mathf.Max(1, xSegments);
            ySegments = Mathf.Max(1, ySegments);
            zSegments = Mathf.Max(1, zSegments);

            float halfX = width * 0.5f;
            float halfY = height * 0.5f;
            float halfZ = depth * 0.5f;
            float maxRound = Mathf.Min(halfX, halfY, halfZ);
            roundness = Mathf.Clamp(roundness, 0.001f, maxRound);

            int xCount = xSegments + 1;
            int yCount = ySegments + 1;
            int zCount = zSegments + 1;
            int vertexCount = xCount * yCount * zCount;

            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];

            float xStep = width / xSegments;
            float yStep = height / ySegments;
            float zStep = depth / zSegments;

            int index = 0;
            for (int z = 0; z < zCount; z++)
            {
                float zPos = -halfZ + z * zStep;
                for (int y = 0; y < yCount; y++)
                {
                    float yPos = -halfY + y * yStep;
                    for (int x = 0; x < xCount; x++)
                    {
                        float xPos = -halfX + x * xStep;
                        var p = new Vector3(xPos, yPos, zPos);
                        var inner = new Vector3(
                            Mathf.Clamp(p.x, -halfX + roundness, halfX - roundness),
                            Mathf.Clamp(p.y, -halfY + roundness, halfY - roundness),
                            Mathf.Clamp(p.z, -halfZ + roundness, halfZ - roundness));

                        var n = p - inner;
                        if (n.sqrMagnitude <= 0.000001f)
                        {
                            n = GuessNormal(p, halfX, halfY, halfZ);
                        }
                        n.Normalize();

                        vertices[index] = inner + n * roundness;
                        normals[index] = n;
                        uvs[index] = new Vector2(x / (float)xSegments, z / (float)zSegments);
                        index++;
                    }
                }
            }

            var triangles = new List<int>((xSegments * ySegments + xSegments * zSegments + ySegments * zSegments) * 12);
            int Index(int x, int y, int z) => x + xCount * (y + yCount * z);

            for (int y = 0; y < ySegments; y++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    AddQuadWithNormal(triangles, vertices,
                        Index(x, y, zSegments),
                        Index(x + 1, y, zSegments),
                        Index(x + 1, y + 1, zSegments),
                        Index(x, y + 1, zSegments),
                        Vector3.forward);

                    AddQuadWithNormal(triangles, vertices,
                        Index(x, y, 0),
                        Index(x + 1, y, 0),
                        Index(x + 1, y + 1, 0),
                        Index(x, y + 1, 0),
                        Vector3.back);
                }
            }

            for (int y = 0; y < ySegments; y++)
            {
                for (int z = 0; z < zSegments; z++)
                {
                    AddQuadWithNormal(triangles, vertices,
                        Index(xSegments, y, z),
                        Index(xSegments, y, z + 1),
                        Index(xSegments, y + 1, z + 1),
                        Index(xSegments, y + 1, z),
                        Vector3.right);

                    AddQuadWithNormal(triangles, vertices,
                        Index(0, y, z),
                        Index(0, y, z + 1),
                        Index(0, y + 1, z + 1),
                        Index(0, y + 1, z),
                        Vector3.left);
                }
            }

            for (int z = 0; z < zSegments; z++)
            {
                for (int x = 0; x < xSegments; x++)
                {
                    AddQuadWithNormal(triangles, vertices,
                        Index(x, ySegments, z),
                        Index(x + 1, ySegments, z),
                        Index(x + 1, ySegments, z + 1),
                        Index(x, ySegments, z + 1),
                        Vector3.up);

                    AddQuadWithNormal(triangles, vertices,
                        Index(x, 0, z),
                        Index(x + 1, 0, z),
                        Index(x + 1, 0, z + 1),
                        Index(x, 0, z + 1),
                        Vector3.down);
                }
            }

            var mesh = new Mesh { name = "BrickBase" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildStudsMesh(
            int columns,
            int rows,
            float cellSize,
            float baseHeight,
            float studRadius,
            float studHeight,
            float studEdgeRadius,
            int studSegments,
            int studEdgeSegments)
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.05f, cellSize);
            studRadius = Mathf.Clamp(studRadius, 0.02f, cellSize * 0.49f);
            studHeight = Mathf.Max(0.02f, studHeight);
            studEdgeRadius = Mathf.Clamp(studEdgeRadius, 0f, Mathf.Min(studRadius, studHeight * 0.5f));

            var studMesh = BuildRoundedCylinderMesh(
                studRadius,
                studHeight,
                studEdgeRadius,
                studSegments,
                studEdgeSegments);

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            float width = columns * cellSize;
            float height = rows * cellSize;
            float startX = -width * 0.5f + cellSize * 0.5f;
            float startY = -height * 0.5f + cellSize * 0.5f;
            float zOffset = baseHeight * 0.5f + studHeight * 0.5f;
            var studRotation = Quaternion.Euler(90f, 0f, 0f);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    var offset = new Vector3(startX + c * cellSize, startY + r * cellSize, zOffset);
                    AppendMeshTransformed(studMesh, offset, studRotation, verts, normals, uvs, tris);
                }
            }

            var mesh = new Mesh { name = "BrickStuds" };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildRoundedCylinderMesh(
            float radius,
            float height,
            float edgeRadius,
            int radialSegments,
            int edgeSegments)
        {
            radius = Mathf.Max(0.01f, radius);
            height = Mathf.Max(0.01f, height);
            radialSegments = Mathf.Max(8, radialSegments);
            edgeSegments = Mathf.Max(1, edgeSegments);
            edgeRadius = Mathf.Clamp(edgeRadius, 0f, Mathf.Min(radius, height * 0.5f));

            float halfH = height * 0.5f;
            var ys = new List<float>();
            if (edgeRadius <= 0.0001f)
            {
                ys.Add(-halfH);
                ys.Add(halfH);
            }
            else
            {
                for (int i = 0; i <= edgeSegments; i++)
                {
                    float t = i / (float)edgeSegments;
                    AddUniqueY(ys, -halfH + edgeRadius * t);
                }
                if (height - 2f * edgeRadius > 0.0001f)
                {
                    AddUniqueY(ys, 0f);
                }
                for (int i = edgeSegments; i >= 0; i--)
                {
                    float t = i / (float)edgeSegments;
                    AddUniqueY(ys, halfH - edgeRadius * t);
                }
            }

            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            var rings = new List<List<int>>();

            float angleStep = Mathf.PI * 2f / radialSegments;
            float bottomCap = -halfH + edgeRadius;
            float topCap = halfH - edgeRadius;

            for (int y = 0; y < ys.Count; y++)
            {
                float yPos = ys[y];
                float r = radius;
                if (edgeRadius > 0.0001f)
                {
                    if (yPos < bottomCap)
                    {
                        float dy = yPos - bottomCap;
                        float term = Mathf.Max(0f, edgeRadius * edgeRadius - dy * dy);
                        r = radius - edgeRadius + Mathf.Sqrt(term);
                    }
                    else if (yPos > topCap)
                    {
                        float dy = yPos - topCap;
                        float term = Mathf.Max(0f, edgeRadius * edgeRadius - dy * dy);
                        r = radius - edgeRadius + Mathf.Sqrt(term);
                    }
                }

                var ring = new List<int>(radialSegments);
                for (int i = 0; i < radialSegments; i++)
                {
                    float a = angleStep * i;
                    float x = Mathf.Cos(a) * r;
                    float z = Mathf.Sin(a) * r;
                    var pos = new Vector3(x, yPos, z);
                    var radial = new Vector3(x, 0f, z);
                    Vector3 n;
                    if (radial.sqrMagnitude <= 0.000001f)
                    {
                        n = yPos >= 0f ? Vector3.up : Vector3.down;
                    }
                    else if (edgeRadius > 0.0001f && (yPos < bottomCap || yPos > topCap))
                    {
                        radial.Normalize();
                        float centerY = yPos < bottomCap ? bottomCap : topCap;
                        float dy = yPos - centerY;
                        float radialComponent = r - (radius - edgeRadius);
                        n = (radial * radialComponent + Vector3.up * dy).normalized;
                    }
                    else
                    {
                        n = radial.normalized;
                    }
                    verts.Add(pos);
                    normals.Add(n);
                    uvs.Add(new Vector2(i / (float)radialSegments, (yPos + halfH) / height));
                    ring.Add(verts.Count - 1);
                }
                rings.Add(ring);
            }

            for (int r = 0; r < rings.Count - 1; r++)
            {
                var ringA = rings[r];
                var ringB = rings[r + 1];
                for (int i = 0; i < radialSegments; i++)
                {
                    int j = (i + 1) % radialSegments;
                    AddQuadWithNormal(tris, verts,
                        ringA[i],
                        ringA[j],
                        ringB[j],
                        ringB[i],
                        new Vector3(verts[ringA[i]].x, 0f, verts[ringA[i]].z));
                }
            }

            if (rings.Count > 0)
            {
                AddCap(tris, verts, normals, uvs, rings[0], Vector3.down);
                AddCap(tris, verts, normals, uvs, rings[rings.Count - 1], Vector3.up);
            }

            var mesh = new Mesh { name = "BrickStud" };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddUniqueY(List<float> ys, float y)
        {
            const float eps = 0.0001f;
            if (ys.Count > 0 && Mathf.Abs(ys[ys.Count - 1] - y) <= eps)
            {
                return;
            }
            ys.Add(y);
        }

        private static Mesh CombineMeshes(string name, Mesh a, Mesh b)
        {
            var verts = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();

            if (a != null) AppendMesh(a, Vector3.zero, verts, normals, uvs, tris);
            if (b != null) AppendMesh(b, Vector3.zero, verts, normals, uvs, tris);

            var mesh = new Mesh { name = name };
            mesh.SetVertices(verts);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            return mesh;
        }

        private static void AppendMesh(
            Mesh mesh,
            Vector3 offset,
            List<Vector3> verts,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> tris)
        {
            AppendMeshTransformed(mesh, offset, Quaternion.identity, verts, normals, uvs, tris);
        }

        private static void AppendMeshTransformed(
            Mesh mesh,
            Vector3 offset,
            Quaternion rotation,
            List<Vector3> verts,
            List<Vector3> normals,
            List<Vector2> uvs,
            List<int> tris)
        {
            if (mesh == null) return;
            int start = verts.Count;
            var mVerts = mesh.vertices;
            var mNormals = mesh.normals;
            var mUvs = mesh.uv;
            var mTris = mesh.triangles;

            for (int i = 0; i < mVerts.Length; i++)
            {
                verts.Add(rotation * mVerts[i] + offset);
            }

            if (mNormals != null && mNormals.Length == mVerts.Length)
            {
                for (int i = 0; i < mNormals.Length; i++)
                {
                    normals.Add((rotation * mNormals[i]).normalized);
                }
            }
            else
            {
                for (int i = 0; i < mVerts.Length; i++) normals.Add(Vector3.up);
            }

            if (mUvs != null && mUvs.Length == mVerts.Length)
            {
                uvs.AddRange(mUvs);
            }
            else
            {
                for (int i = 0; i < mVerts.Length; i++) uvs.Add(Vector2.zero);
            }

            for (int i = 0; i < mTris.Length; i++)
            {
                tris.Add(start + mTris[i]);
            }
        }

        private static Vector3 GuessNormal(Vector3 p, float halfX, float halfY, float halfZ)
        {
            float dx = Mathf.Abs(Mathf.Abs(p.x) - halfX);
            float dy = Mathf.Abs(Mathf.Abs(p.y) - halfY);
            float dz = Mathf.Abs(Mathf.Abs(p.z) - halfZ);

            if (dx <= dy && dx <= dz) return new Vector3(Mathf.Sign(p.x), 0f, 0f);
            if (dy <= dz) return new Vector3(0f, Mathf.Sign(p.y), 0f);
            return new Vector3(0f, 0f, Mathf.Sign(p.z));
        }

        private static void AddQuadWithNormal(List<int> tris, Vector3[] verts, int a, int b, int c, int d, Vector3 normal)
        {
            Vector3 n = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
            if (Vector3.Dot(n, normal) < 0f)
            {
                int tmp = b; b = d; d = tmp;
            }
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
            tris.Add(a);
            tris.Add(c);
            tris.Add(d);
        }

        private static void AddQuadWithNormal(List<int> tris, List<Vector3> verts, int a, int b, int c, int d, Vector3 normal)
        {
            Vector3 n = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
            if (Vector3.Dot(n, normal) < 0f)
            {
                int tmp = b; b = d; d = tmp;
            }
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
            tris.Add(a);
            tris.Add(c);
            tris.Add(d);
        }

        private static List<float> BuildZPositions(float depth, float edgeRadius, int segments)
        {
            var zs = new List<float>();
            float halfZ = depth * 0.5f;
            if (edgeRadius <= 0.0001f)
            {
                zs.Add(-halfZ);
                zs.Add(halfZ);
                return zs;
            }

            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float z = -halfZ + edgeRadius * t;
                AddUniqueZ(zs, z);
            }

            float flatDepth = depth - 2f * edgeRadius;
            if (flatDepth > 0.0001f)
            {
                AddUniqueZ(zs, 0f);
            }

            for (int i = segments; i >= 0; i--)
            {
                float t = i / (float)segments;
                float z = halfZ - edgeRadius * t;
                AddUniqueZ(zs, z);
            }

            return zs;
        }

        private static void AddUniqueZ(List<float> zs, float z)
        {
            const float eps = 0.0001f;
            if (zs.Count > 0 && Mathf.Abs(zs[zs.Count - 1] - z) <= eps)
            {
                return;
            }
            zs.Add(z);
        }

        private static float ComputeEdgeInset(float z, float halfZ, float edgeRadius)
        {
            if (edgeRadius <= 0.0001f) return 0f;
            float capStart = -halfZ + edgeRadius;
            float capEnd = halfZ - edgeRadius;
            if (z < capStart)
            {
                float dz = z - (-halfZ);
                float r2 = edgeRadius * edgeRadius;
                float term = Mathf.Max(0f, r2 - dz * dz);
                return edgeRadius - Mathf.Sqrt(term);
            }
            if (z > capEnd)
            {
                float dz = halfZ - z;
                float r2 = edgeRadius * edgeRadius;
                float term = Mathf.Max(0f, r2 - dz * dz);
                return edgeRadius - Mathf.Sqrt(term);
            }
            return 0f;
        }

        private static void AddQuad(List<int> tris, List<Vector3> verts, int a, int b, int c, int d)
        {
            Vector3 v0 = verts[a];
            Vector3 v1 = verts[b];
            Vector3 v2 = verts[c];
            Vector3 v3 = verts[d];
            Vector3 n = Vector3.Cross(v1 - v0, v2 - v0);
            Vector3 mid = (v0 + v1 + v2 + v3) * 0.25f;
            Vector3 outward = new Vector3(mid.x, mid.y, 0f);
            if (Vector3.Dot(n, outward) < 0f)
            {
                int tmp = b; b = d; d = tmp;
            }
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
            tris.Add(a);
            tris.Add(c);
            tris.Add(d);
        }

        private static void AddCap(List<int> tris, List<Vector3> verts, List<Vector3> normals, List<Vector2> uvs, List<int> ring, Vector3 normal)
        {
            if (ring == null || ring.Count < 3) return;
            Vector3 center = Vector3.zero;
            for (int i = 0; i < ring.Count; i++)
            {
                center += verts[ring[i]];
            }
            center /= ring.Count;
            int centerIndex = verts.Count;
            verts.Add(center);
            normals.Add(normal.normalized);
            uvs.Add(new Vector2(0.5f, 0.5f));

            for (int i = 0; i < ring.Count; i++)
            {
                int j = (i + 1) % ring.Count;
                AddTriangleCorrected(tris, verts, centerIndex, ring[i], ring[j], normal);
            }
        }

        private static void AddTriangleCorrected(List<int> tris, List<Vector3> verts, int a, int b, int c, Vector3 normal)
        {
            Vector3 n = Vector3.Cross(verts[b] - verts[a], verts[c] - verts[a]);
            if (Vector3.Dot(n, normal) < 0f)
            {
                int tmp = b; b = c; c = tmp;
            }
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
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
            AddCorner(pts, new Vector2(halfX - radius, halfY - radius), radius, 90f, 0f, segmentsPerCorner, true);
            AddCorner(pts, new Vector2(halfX - radius, -halfY + radius), radius, 0f, -90f, segmentsPerCorner, false);
            AddCorner(pts, new Vector2(-halfX + radius, -halfY + radius), radius, -90f, -180f, segmentsPerCorner, false);
            AddCorner(pts, new Vector2(-halfX + radius, halfY - radius), radius, -180f, -270f, segmentsPerCorner, false);
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

            OverwriteMeshData(existing, mesh);
            existing.name = mesh.name;
            EditorUtility.SetDirty(existing);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ForceReserializeAssets(new[] { path }, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            return existing;
        }

        private static void OverwriteMeshData(Mesh target, Mesh source)
        {
            if (target == null || source == null) return;
            target.Clear();
            target.indexFormat = source.indexFormat;
            target.subMeshCount = Mathf.Max(1, source.subMeshCount);
            target.vertices = source.vertices;

            var normals = source.normals;
            if (normals != null && normals.Length == source.vertexCount) target.normals = normals;
            var tangents = source.tangents;
            if (tangents != null && tangents.Length == source.vertexCount) target.tangents = tangents;
            var uv = source.uv;
            if (uv != null && uv.Length == source.vertexCount) target.uv = uv;
            var uv2 = source.uv2;
            if (uv2 != null && uv2.Length == source.vertexCount) target.uv2 = uv2;
            var colors = source.colors;
            if (colors != null && colors.Length == source.vertexCount) target.colors = colors;
            var colors32 = source.colors32;
            if (colors32 != null && colors32.Length == source.vertexCount) target.colors32 = colors32;

            if (source.subMeshCount > 0)
            {
                for (int i = 0; i < source.subMeshCount; i++)
                {
                    target.SetTriangles(source.GetTriangles(i), i);
                }
            }
            else
            {
                target.triangles = source.triangles;
            }

            target.RecalculateBounds();
            if (target.normals == null || target.normals.Length != target.vertexCount)
            {
                target.RecalculateNormals();
            }
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
            EditorUtility.SetDirty(mat);
            return mat;
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
    }
}
