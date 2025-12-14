using UnityEngine;

namespace LoopSorting
{
    public static class BlockVisual
    {
        private const string LegoModelResourcePathV4 =
            "LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter_v4";
        private const string LegoModelResourcePathV3 =
            "LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v3/LegoLikeBrick_2x2_Detailed_BevelAO_PivotCenter";
        private const string LegoModelResourcePathV2 =
            "LegoLikeBrick_2x2_Detailed_UnityPack/LegoLikeBrick_2x2_Detailed_PivotCenter";
        private const string LegoShaderV3BuiltIn = "Custom/BrickUnlit_AO_Curv_VertexColor_BuiltIn";
        private const string LegoShaderV4ResourcePath =
            "LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v4/BrickUnlit_AO_Curv_VertexColor_BuiltIn";
        private const string LegoShaderV3ResourcePath =
            "LegoLikeBrick_2x2_Detailed_BevelAO_UnityPack_v3/BrickUnlit_AO_Curv_VertexColor_BuiltIn";
        private const string LegoShaderFallback = "LoopSorting/UnlitRim";
        private static GameObject _legoModelPrefab;
        private static Material _legoBaseMaterial;
        private static string _legoShaderKey;
        private static bool _warnedMissingLegoShader;
        private static int _legoVariant; // 4,3,2
        private static readonly MaterialPropertyBlock SharedPropertyBlock = new MaterialPropertyBlock();
        private static readonly Quaternion LegoFacingRotation = Quaternion.Euler(-90f, 0f, 0f);

        public static GameObject CreateBlock(BlockColor color)
        {
            var root = new GameObject($"Block_{color}");

            if (TryEnsureLegoModel())
            {
                var model = Object.Instantiate(_legoModelPrefab, root.transform);
                model.name = "Model";
                model.transform.localPosition = Vector3.zero;
                // This pack's "up" is +Y (studs face +Y). Our game camera looks along +Z,
                // so rotate so the studs face the camera (towards -Z).
                model.transform.localRotation = LegoFacingRotation;
                model.transform.localScale = Vector3.one;

                StripColliders(root);
                NormalizeChildToUnit(model, root);
                EnsureLegoMaterials(model);
                ApplyColor(root, ToUnityColor(color));
                return root;
            }

            // Fallback: colored cube.
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Model";
            go.transform.SetParent(root.transform, false);
            go.transform.localScale = new Vector3(1f, 1f, 0.6f);
            StripColliders(root);
            ApplyColor(root, ToUnityColor(color));
            return root;
        }

        public static void ApplyColor(GameObject root, Color color)
        {
            if (root == null) return;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            // Use per-renderer property blocks so colors don't affect each other.
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;

                r.GetPropertyBlock(SharedPropertyBlock);
                SharedPropertyBlock.SetColor("_Color", color);
                SharedPropertyBlock.SetColor("_BaseColor", color);
                r.SetPropertyBlock(SharedPropertyBlock);
            }
        }

        public static Color ToUnityColor(BlockColor color)
        {
            switch (color)
            {
                case BlockColor.Red: return new Color(0.78f, 0.16f, 0.16f);
                case BlockColor.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case BlockColor.Yellow: return new Color(0.98f, 0.8f, 0.15f);
                case BlockColor.Green: return new Color(0.25f, 0.8f, 0.35f);
                case BlockColor.Purple: return new Color(0.6f, 0.35f, 0.9f);
                case BlockColor.Orange: return new Color(1.0f, 0.6f, 0.2f);
                default: return Color.white;
            }
        }

        private static bool TryEnsureLegoModel()
        {
            if (_legoModelPrefab != null) return true;

            // Prefer the newer pack if present.
            _legoModelPrefab = Resources.Load<GameObject>(LegoModelResourcePathV4);
            if (_legoModelPrefab != null)
            {
                _legoVariant = 4;
                return true;
            }

            _legoModelPrefab = Resources.Load<GameObject>(LegoModelResourcePathV3);
            if (_legoModelPrefab != null)
            {
                _legoVariant = 3;
                return true;
            }

            _legoModelPrefab = Resources.Load<GameObject>(LegoModelResourcePathV2);
            _legoVariant = 2;
            return _legoModelPrefab != null;
        }

        private static void EnsureLegoMaterials(GameObject modelRoot)
        {
            if (modelRoot == null) return;

            string desiredShader = ResolveDesiredLegoShader();
            if (_legoBaseMaterial == null || !string.Equals(_legoShaderKey, desiredShader))
            {
                Shader shader = null;
                if (_legoVariant >= 3 && string.Equals(desiredShader, LegoShaderV3BuiltIn))
                {
                    shader = Resources.Load<Shader>(_legoVariant == 4 ? LegoShaderV4ResourcePath : LegoShaderV3ResourcePath);
                    if (shader == null && _legoVariant == 4)
                    {
                        // Fallback: keep working even if only v3 shaders are present.
                        shader = Resources.Load<Shader>(LegoShaderV3ResourcePath);
                    }
                }
                if (shader == null)
                {
                    shader = Shader.Find(desiredShader);
                }
                if (shader == null)
                {
                    shader = Shader.Find(LegoShaderFallback);
                    desiredShader = LegoShaderFallback;
                    if (shader == null) shader = Shader.Find("Standard");
                    if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) shader = Shader.Find("Unlit/Color");
                }

                if (shader != null)
                {
                    _legoBaseMaterial = new Material(shader);
                    _legoShaderKey = desiredShader;
                    if (_legoBaseMaterial.HasProperty("_Metallic")) _legoBaseMaterial.SetFloat("_Metallic", 0.05f);
                    if (_legoBaseMaterial.HasProperty("_Glossiness")) _legoBaseMaterial.SetFloat("_Glossiness", 0.6f);
                    if (_legoBaseMaterial.HasProperty("_Smoothness")) _legoBaseMaterial.SetFloat("_Smoothness", 0.6f);

                    if (_legoBaseMaterial.HasProperty("_AO")) _legoBaseMaterial.SetFloat("_AO", 0.82f);
                    if (_legoBaseMaterial.HasProperty("_AOPower")) _legoBaseMaterial.SetFloat("_AOPower", 2.8f);
                    if (_legoBaseMaterial.HasProperty("_Curv")) _legoBaseMaterial.SetFloat("_Curv", 0.34f);

                    // View-facing fake light (N·V): towards camera brighter, sides darker.
                    if (_legoBaseMaterial.HasProperty("_ViewLightStrength")) _legoBaseMaterial.SetFloat("_ViewLightStrength", 0.95f);
                    if (_legoBaseMaterial.HasProperty("_ViewPower")) _legoBaseMaterial.SetFloat("_ViewPower", 1.6f);
                    if (_legoBaseMaterial.HasProperty("_ViewSideMin")) _legoBaseMaterial.SetFloat("_ViewSideMin", 0.62f);

                    if (_legoBaseMaterial.HasProperty("_RimColor")) _legoBaseMaterial.SetColor("_RimColor", new Color(1f, 1f, 1f, 1f));
                    if (_legoBaseMaterial.HasProperty("_RimPower")) _legoBaseMaterial.SetFloat("_RimPower", 2.6f);
                    if (_legoBaseMaterial.HasProperty("_RimStrength")) _legoBaseMaterial.SetFloat("_RimStrength", 0.35f);
                    if (_legoBaseMaterial.HasProperty("_EdgeDarken")) _legoBaseMaterial.SetFloat("_EdgeDarken", 0.12f);
                    if (_legoBaseMaterial.HasProperty("_Ambient")) _legoBaseMaterial.SetFloat("_Ambient", 1.0f);
                    _legoBaseMaterial.color = Color.white;
                }
                else if (_legoBaseMaterial == null && !_warnedMissingLegoShader)
                {
                    _warnedMissingLegoShader = true;
                    Debug.LogWarning("BlockVisual: No shader found for Lego materials; blocks will use prefab/default materials.");
                }
            }

            if (_legoBaseMaterial == null) return;

            var renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;
                r.sharedMaterial = _legoBaseMaterial;
            }
        }

        private static string ResolveDesiredLegoShader()
        {
            // When v3/v4 pack is present, prefer its baked AO/curvature unlit shader for clearer edges without lights.
            if (_legoVariant >= 3)
            {
                return LegoShaderV3BuiltIn;
            }

            return LegoShaderFallback;
        }

        private static void StripColliders(GameObject root)
        {
            if (root == null) return;
            var colliders = root.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    Object.Destroy(colliders[i]);
                }
            }
        }

        private static void NormalizeChildToUnit(GameObject model, GameObject root)
        {
            if (model == null || root == null) return;

            // Compute world-space bounds at identity; then scale child so max(X,Y) fits 1 unit.
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            }

            float maxXY = Mathf.Max(0.0001f, Mathf.Max(bounds.size.x, bounds.size.y));
            float scale = 1f / maxXY;
            model.transform.localScale = model.transform.localScale * scale;

            // Re-center after scaling.
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null) bounds.Encapsulate(renderers[i].bounds);
            }
            var offset = root.transform.InverseTransformPoint(bounds.center);
            model.transform.localPosition -= offset;
        }
    }
}
