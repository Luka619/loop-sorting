#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoopSorting.Editor
{
    public static class LoopSortingUIPrefabRuntimeSaver
    {
        [MenuItem("LoopSorting/UI/Apply Runtime Layout To Prefabs (RectTransform only)")]
        public static void ApplyRuntimeLayoutToPrefabs()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Apply Runtime Layout", "Enter Play Mode, adjust the UI, then run this action.", "OK");
                return;
            }

            var candidates = new[]
            {
                "SettingsPanel",
                "ShopPanel",
                "ResultPanel",
                "BoosterPurchasePanel",
            };

            int applied = 0;
            foreach (var name in candidates)
            {
                var instanceRoot = FindByNameInScene(name);
                if (instanceRoot == null) continue;
                if (TryApplyRectTransformLayout(instanceRoot))
                {
                    applied++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Apply Runtime Layout", $"Applied layout for {applied} prefab(s).", "OK");
        }

        private static GameObject FindByNameInScene(string name)
        {
            // Unity 2021 LTS doesn't have Object.FindObjectsByType/FindObjectsInactive/FindObjectsSortMode.
            // Use Resources.FindObjectsOfTypeAll and filter to scene objects.
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go != null && go.name == name)
                {
                    if (!go.scene.IsValid()) continue;
                    if (EditorUtility.IsPersistent(go)) continue;
                    if ((go.hideFlags & HideFlags.HideInHierarchy) != 0) continue;

                    // Prefer top-level instance (under Canvas), not nested child with same name.
                    if (go.transform.parent == null || go.transform.parent.GetComponent<Canvas>() != null || go.transform.parent.name.Contains("Canvas"))
                    {
                        return go;
                    }
                }
            }
            return null;
        }

        private static bool TryApplyRectTransformLayout(GameObject instanceRoot)
        {
            if (instanceRoot == null) return false;

            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = GuessPrefabAssetPath(instanceRoot.name);
                if (string.IsNullOrEmpty(assetPath))
                {
                    Debug.LogWarning($"[LoopSortingUIPrefabRuntimeSaver] '{instanceRoot.name}' is not a prefab instance and no matching prefab was found under Assets/Resources/UI.");
                    return false;
                }
            }

            var prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
            if (prefabRoot == null)
            {
                Debug.LogWarning($"[LoopSortingUIPrefabRuntimeSaver] Failed to load prefab contents: {assetPath}");
                return false;
            }

            try
            {
                CopyRectTransforms(instanceRoot.transform, prefabRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, assetPath);
                Debug.Log($"[LoopSortingUIPrefabRuntimeSaver] Applied layout: {assetPath}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static string GuessPrefabAssetPath(string prefabName)
        {
            if (string.IsNullOrEmpty(prefabName)) return null;

            string direct = $"Assets/Resources/UI/{prefabName}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(direct) != null) return direct;

            // Fall back to searching within the expected folder.
            var guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { "Assets/Resources/UI" });
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(path)) continue;
                if (Path.GetFileNameWithoutExtension(path) == prefabName) return path;
            }
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return string.IsNullOrEmpty(path) ? null : path;
            }

            return null;
        }

        private static void CopyRectTransforms(Transform src, Transform dst)
        {
            if (src == null || dst == null) return;

            var srcRect = src as RectTransform;
            var dstRect = dst as RectTransform;
            if (srcRect != null && dstRect != null)
            {
                dstRect.anchorMin = srcRect.anchorMin;
                dstRect.anchorMax = srcRect.anchorMax;
                dstRect.pivot = srcRect.pivot;
                dstRect.anchoredPosition = srcRect.anchoredPosition;
                dstRect.sizeDelta = srcRect.sizeDelta;
                dstRect.offsetMin = srcRect.offsetMin;
                dstRect.offsetMax = srcRect.offsetMax;
                dstRect.localRotation = srcRect.localRotation;
                dstRect.localScale = srcRect.localScale;
            }
            else
            {
                dst.localPosition = src.localPosition;
                dst.localRotation = src.localRotation;
                dst.localScale = src.localScale;
            }

            var dstChildrenByName = new Dictionary<string, Transform>();
            for (int i = 0; i < dst.childCount; i++)
            {
                var c = dst.GetChild(i);
                if (c != null && !dstChildrenByName.ContainsKey(c.name))
                {
                    dstChildrenByName[c.name] = c;
                }
            }

            for (int i = 0; i < src.childCount; i++)
            {
                var sc = src.GetChild(i);
                if (sc == null) continue;
                if (!dstChildrenByName.TryGetValue(sc.name, out var dc) || dc == null) continue;
                CopyRectTransforms(sc, dc);
            }
        }
    }
}
#endif
