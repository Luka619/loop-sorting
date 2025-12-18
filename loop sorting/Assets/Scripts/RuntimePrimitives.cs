using UnityEngine;

namespace LoopSorting
{
    public static class RuntimePrimitives
    {
        private static Mesh _sharedQuadMesh;

        public static GameObject CreateQuad(string name = "Quad")
        {
            var go = new GameObject(string.IsNullOrEmpty(name) ? "Quad" : name);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = GetSharedQuadMesh();

            // Safer defaults for WebGL/WX and runtime-generated overlays/backgrounds.
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.allowOcclusionWhenDynamic = false;

            return go;
        }

        private static Mesh GetSharedQuadMesh()
        {
            if (_sharedQuadMesh != null) return _sharedQuadMesh;

            var mesh = new Mesh();
            mesh.name = "RuntimeQuad";

            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };

            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            // Ensure COLOR channel exists (some shaders/platforms treat missing vertex colors as 0).
            mesh.colors = new[]
            {
                Color.white,
                Color.white,
                Color.white,
                Color.white,
            };

            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            _sharedQuadMesh = mesh;
            return _sharedQuadMesh;
        }
    }
}
