#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting.Editor
{
    public static class LoopSortingUIPrefabRuntimeSaver
    {
        private const string GeneratedSpritesAssetPath = "Assets/Resources/UI/LoopSortingRuntimeGeneratedSprites.asset";

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
                 "MainMenuCanvas",
                 "GameplayHUD",
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

        [MenuItem("LoopSorting/UI/Save Runtime UI To Prefabs (FULL overwrite)")]
        public static void SaveRuntimeUiToPrefabsFull()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Save Runtime UI", "Enter Play Mode, adjust the UI, then run this action.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Save Runtime UI",
                    "This will OVERWRITE prefab assets under Assets/Resources/UI using the current runtime instances (full object save, not just RectTransform).\n\nContinue?",
                    "Overwrite",
                     "Cancel"))
             {
                 return;
             }

            var generatedSprites = GetOrCreateGeneratedSpritesAsset();
            var generatedSpriteByKey = BuildGeneratedSpriteCache(generatedSprites);
 
             var candidates = new[]
             {
                 "MainMenuCanvas",
                 "GameplayHUD",
                 "SettingsPanel",
                 "ShopPanel",
                 "ResultPanel",
                 "BoosterPurchasePanel",
            };

             int saved = 0;
             foreach (var name in candidates)
             {
                 var instanceRoot = FindByNameInScene(name);
                 if (instanceRoot == null) continue;
                 if (TrySaveFullPrefab(instanceRoot, generatedSprites, generatedSpriteByKey))
                 {
                     saved++;
                 }
             }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Save Runtime UI", $"Saved {saved} prefab(s).", "OK");
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

        private static bool TrySaveFullPrefab(GameObject instanceRoot)
        {
            if (instanceRoot == null) return false;

            var generatedSprites = GetOrCreateGeneratedSpritesAsset();
            var generatedSpriteByKey = BuildGeneratedSpriteCache(generatedSprites);
            return TrySaveFullPrefab(instanceRoot, generatedSprites, generatedSpriteByKey);
        }

        private static bool TrySaveFullPrefab(
            GameObject instanceRoot,
            LoopSorting.LoopSortingRuntimeGeneratedSprites generatedSprites,
            Dictionary<string, Sprite> generatedSpriteByKey)
        {
            if (instanceRoot == null) return false;
 
            string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = $"Assets/Resources/UI/{instanceRoot.name}.prefab";
            }

            if (instanceRoot.name == "MainMenuCanvas")
            {
                var refs = instanceRoot.GetComponent<LoopSorting.MainMenuCanvasPrefabRefs>();
                if (refs == null) refs = instanceRoot.AddComponent<LoopSorting.MainMenuCanvasPrefabRefs>();
                refs.AutoAssign();
            }

            // FULL overwrite saves everything, including sprites. Many of our UI sprites are created at runtime
            // via Sprite.Create from Texture2D (since source PNGs are imported as Default textures).
            // Non-persistent sprites cannot be serialized into prefabs and end up as null/white after saving.
            PersistRuntimeSprites(instanceRoot, generatedSprites, generatedSpriteByKey);
 
            EnsureFolderForAsset(assetPath);
            PrefabUtility.SaveAsPrefabAsset(instanceRoot, assetPath);
            Debug.Log($"[LoopSortingUIPrefabRuntimeSaver] Saved full prefab: {assetPath}");
            return true;
        }

        private static LoopSorting.LoopSortingRuntimeGeneratedSprites GetOrCreateGeneratedSpritesAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<LoopSorting.LoopSortingRuntimeGeneratedSprites>(GeneratedSpritesAssetPath);
            if (asset != null) return asset;

            EnsureFolderForAsset(GeneratedSpritesAssetPath);
            asset = ScriptableObject.CreateInstance<LoopSorting.LoopSortingRuntimeGeneratedSprites>();
            AssetDatabase.CreateAsset(asset, GeneratedSpritesAssetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static Dictionary<string, Sprite> BuildGeneratedSpriteCache(LoopSorting.LoopSortingRuntimeGeneratedSprites asset)
        {
            var map = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            if (asset == null) return map;

            string assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath)) return map;

            var all = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (int i = 0; i < all.Length; i++)
            {
                var s = all[i] as Sprite;
                if (s == null) continue;
                var key = BuildSpriteKey(s);
                if (string.IsNullOrEmpty(key)) continue;
                if (!map.ContainsKey(key)) map[key] = s;
            }

            return map;
        }

        private static void PersistRuntimeSprites(
            GameObject instanceRoot,
            LoopSorting.LoopSortingRuntimeGeneratedSprites generatedSprites,
            Dictionary<string, Sprite> generatedSpriteByKey)
        {
            if (instanceRoot == null) return;
            if (generatedSprites == null) return;
            if (generatedSpriteByKey == null) return;

            foreach (var img in instanceRoot.GetComponentsInChildren<Image>(includeInactive: true))
            {
                if (img == null) continue;
                img.sprite = PersistSprite(img.sprite, generatedSprites, generatedSpriteByKey);
            }

            foreach (var sr in instanceRoot.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
            {
                if (sr == null) continue;
                sr.sprite = PersistSprite(sr.sprite, generatedSprites, generatedSpriteByKey);
            }

            foreach (var btn in instanceRoot.GetComponentsInChildren<Button>(includeInactive: true))
            {
                if (btn == null) continue;
                var state = btn.spriteState;
                state.highlightedSprite = PersistSprite(state.highlightedSprite, generatedSprites, generatedSpriteByKey);
                state.pressedSprite = PersistSprite(state.pressedSprite, generatedSprites, generatedSpriteByKey);
                state.selectedSprite = PersistSprite(state.selectedSprite, generatedSprites, generatedSpriteByKey);
                state.disabledSprite = PersistSprite(state.disabledSprite, generatedSprites, generatedSpriteByKey);
                btn.spriteState = state;
            }
        }

        private static Sprite PersistSprite(
            Sprite sprite,
            LoopSorting.LoopSortingRuntimeGeneratedSprites generatedSprites,
            Dictionary<string, Sprite> generatedSpriteByKey)
        {
            if (sprite == null) return null;
            if (EditorUtility.IsPersistent(sprite)) return sprite;
            if (generatedSprites == null || generatedSpriteByKey == null) return sprite;

            var tex = sprite.texture;
            if (tex == null || !EditorUtility.IsPersistent(tex)) return sprite;
            string texPath = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(texPath)) return sprite;

            var key = BuildSpriteKey(texPath, sprite);
            if (string.IsNullOrEmpty(key)) return sprite;

            if (generatedSpriteByKey.TryGetValue(key, out var existing) && existing != null)
            {
                return existing;
            }

            var rect = sprite.rect;
            var border = sprite.border;
            float ppu = Mathf.Max(1f, sprite.pixelsPerUnit);

            var pivotPx = sprite.pivot;
            var pivot = new Vector2(
                rect.width > 0.0001f ? (pivotPx.x / rect.width) : 0.5f,
                rect.height > 0.0001f ? (pivotPx.y / rect.height) : 0.5f);

            var created = Sprite.Create(tex, rect, pivot, ppu, 0, SpriteMeshType.FullRect, border);
            created.name = $"{Path.GetFileNameWithoutExtension(texPath)}_{Fnv1a32(key):x8}";
            AssetDatabase.AddObjectToAsset(created, generatedSprites);
            EditorUtility.SetDirty(generatedSprites);
            generatedSpriteByKey[key] = created;
            return created;
        }

        private static string BuildSpriteKey(Sprite sprite)
        {
            if (sprite == null) return null;
            var tex = sprite.texture;
            if (tex == null) return null;
            string texPath = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(texPath)) return null;
            return BuildSpriteKey(texPath, sprite);
        }

        private static int Q(float v)
        {
            return Mathf.RoundToInt(v * 1000f);
        }

        private static string BuildSpriteKey(string texturePath, Sprite sprite)
        {
            if (string.IsNullOrEmpty(texturePath) || sprite == null) return null;
            var r = sprite.rect;
            var p = sprite.pivot; // pixels
            var b = sprite.border;
            return $"{texturePath}|r:{Q(r.x)},{Q(r.y)},{Q(r.width)},{Q(r.height)}|p:{Q(p.x)},{Q(p.y)}|ppu:{Q(sprite.pixelsPerUnit)}|b:{Q(b.x)},{Q(b.y)},{Q(b.z)},{Q(b.w)}";
        }

        private static uint Fnv1a32(string s)
        {
            unchecked
            {
                const uint offset = 2166136261u;
                const uint prime = 16777619u;
                uint hash = offset;
                if (string.IsNullOrEmpty(s)) return hash;
                for (int i = 0; i < s.Length; i++)
                {
                    hash ^= s[i];
                    hash *= prime;
                }
                return hash;
            }
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

        private static void EnsureFolderForAsset(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            assetPath = assetPath.Replace('\\', '/');
            var folder = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder)) return;
            if (AssetDatabase.IsValidFolder(folder)) return;

            string[] parts = folder.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{cur}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(cur, parts[i]);
                }
                cur = next;
            }
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

            CopyLayoutComponents(src, dst);
            CopyTextComponents(src, dst);

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

        private static void CopyTextComponents(Transform src, Transform dst)
        {
            if (src == null || dst == null) return;

            var srcTmp = src.GetComponent<TMP_Text>();
            var dstTmp = dst.GetComponent<TMP_Text>();
            if (srcTmp != null && dstTmp != null)
            {
                dstTmp.enabled = srcTmp.enabled;
                dstTmp.text = srcTmp.text;
                dstTmp.font = srcTmp.font;
                dstTmp.fontSize = srcTmp.fontSize;
                dstTmp.enableAutoSizing = srcTmp.enableAutoSizing;
                dstTmp.fontSizeMin = srcTmp.fontSizeMin;
                dstTmp.fontSizeMax = srcTmp.fontSizeMax;
                dstTmp.color = srcTmp.color;
                dstTmp.alignment = srcTmp.alignment;
                dstTmp.enableWordWrapping = srcTmp.enableWordWrapping;
                dstTmp.overflowMode = srcTmp.overflowMode;
                dstTmp.fontStyle = srcTmp.fontStyle;
                dstTmp.characterSpacing = srcTmp.characterSpacing;
                dstTmp.lineSpacing = srcTmp.lineSpacing;
                dstTmp.paragraphSpacing = srcTmp.paragraphSpacing;
                dstTmp.raycastTarget = srcTmp.raycastTarget;
                dstTmp.margin = srcTmp.margin;

                // Avoid copying runtime-created material instances; only copy persistent material presets.
                var srcMat = srcTmp.fontSharedMaterial;
                if (srcMat != null && EditorUtility.IsPersistent(srcMat))
                {
                    dstTmp.fontSharedMaterial = srcMat;
                }
            }

            var srcText = src.GetComponent<Text>();
            var dstText = dst.GetComponent<Text>();
            if (srcText != null && dstText != null)
            {
                dstText.enabled = srcText.enabled;
                dstText.text = srcText.text;
                dstText.font = srcText.font;
                dstText.fontSize = srcText.fontSize;
                dstText.fontStyle = srcText.fontStyle;
                dstText.alignment = srcText.alignment;
                dstText.lineSpacing = srcText.lineSpacing;
                dstText.supportRichText = srcText.supportRichText;
                dstText.horizontalOverflow = srcText.horizontalOverflow;
                dstText.verticalOverflow = srcText.verticalOverflow;
                dstText.resizeTextForBestFit = srcText.resizeTextForBestFit;
                dstText.resizeTextMinSize = srcText.resizeTextMinSize;
                dstText.resizeTextMaxSize = srcText.resizeTextMaxSize;
                dstText.color = srcText.color;
                dstText.raycastTarget = srcText.raycastTarget;
            }
        }

        private static void CopyLayoutComponents(Transform src, Transform dst)
        {
            if (src == null || dst == null) return;

            void CopyHorVer(HorizontalOrVerticalLayoutGroup s, HorizontalOrVerticalLayoutGroup d)
            {
                if (s == null || d == null) return;
                d.enabled = s.enabled;
                d.padding = s.padding;
                d.childAlignment = s.childAlignment;
                d.spacing = s.spacing;
                d.childControlWidth = s.childControlWidth;
                d.childControlHeight = s.childControlHeight;
                d.childForceExpandWidth = s.childForceExpandWidth;
                d.childForceExpandHeight = s.childForceExpandHeight;
                d.childScaleWidth = s.childScaleWidth;
                d.childScaleHeight = s.childScaleHeight;
                d.reverseArrangement = s.reverseArrangement;
            }

            var srcH = src.GetComponent<HorizontalLayoutGroup>();
            var dstH = dst.GetComponent<HorizontalLayoutGroup>();
            if (srcH != null && dstH != null) CopyHorVer(srcH, dstH);

            var srcV = src.GetComponent<VerticalLayoutGroup>();
            var dstV = dst.GetComponent<VerticalLayoutGroup>();
            if (srcV != null && dstV != null) CopyHorVer(srcV, dstV);

            var srcG = src.GetComponent<GridLayoutGroup>();
            var dstG = dst.GetComponent<GridLayoutGroup>();
            if (srcG != null && dstG != null)
            {
                dstG.enabled = srcG.enabled;
                dstG.padding = srcG.padding;
                dstG.childAlignment = srcG.childAlignment;
                dstG.cellSize = srcG.cellSize;
                dstG.spacing = srcG.spacing;
                dstG.startCorner = srcG.startCorner;
                dstG.startAxis = srcG.startAxis;
                dstG.constraint = srcG.constraint;
                dstG.constraintCount = srcG.constraintCount;
            }

            var srcF = src.GetComponent<ContentSizeFitter>();
            var dstF = dst.GetComponent<ContentSizeFitter>();
            if (srcF != null && dstF != null)
            {
                dstF.enabled = srcF.enabled;
                dstF.horizontalFit = srcF.horizontalFit;
                dstF.verticalFit = srcF.verticalFit;
            }

            var srcE = src.GetComponent<LayoutElement>();
            var dstE = dst.GetComponent<LayoutElement>();
            if (srcE != null && dstE != null)
            {
                dstE.enabled = srcE.enabled;
                dstE.ignoreLayout = srcE.ignoreLayout;
                dstE.minWidth = srcE.minWidth;
                dstE.minHeight = srcE.minHeight;
                dstE.preferredWidth = srcE.preferredWidth;
                dstE.preferredHeight = srcE.preferredHeight;
                dstE.flexibleWidth = srcE.flexibleWidth;
                dstE.flexibleHeight = srcE.flexibleHeight;
                dstE.layoutPriority = srcE.layoutPriority;
            }
        }
    }
}
#endif
