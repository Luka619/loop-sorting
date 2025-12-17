#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting.Editor
{
    public static class LoopSortingUIPrefabExporter
    {
        private const string OutputFolder = "Assets/Resources/UI";

        [MenuItem("LoopSorting/UI/Generate Panel Prefabs (if missing)")]
        public static void GenerateAllIfMissing()
        {
            GenerateAllInternal(overwriteExisting: false);
        }

        [MenuItem("LoopSorting/UI/Regenerate Panel Prefabs (overwrite)")]
        public static void RegenerateAllOverwrite()
        {
            if (!EditorUtility.DisplayDialog(
                    "Regenerate UI Prefabs",
                    "This will overwrite existing prefabs under Assets/Resources/UI and may discard your manual layout tweaks.\n\nContinue?",
                    "Overwrite",
                    "Cancel"))
            {
                return;
            }

            GenerateAllInternal(overwriteExisting: true);
        }

        private static void GenerateAllInternal(bool overwriteExisting)
        {
            EnsureFolder(OutputFolder);

            var root = new GameObject("__LoopSortingUIPrefabExportRoot");
            root.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                var canvasGO = new GameObject("__TempUICanvas");
                canvasGO.hideFlags = HideFlags.HideAndDontSave;
                canvasGO.transform.SetParent(root.transform, false);

                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080f, 1920f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasGO.AddComponent<GraphicRaycaster>();

                var controllerGO = new GameObject("__TempController");
                controllerGO.hideFlags = HideFlags.HideAndDontSave;
                controllerGO.transform.SetParent(root.transform, false);

                var controller = controllerGO.AddComponent<GameRuntimeController>();
                SetPrivateField(controller, "_uiCanvas", canvas);

                InvokePrivate(controller, "EnsureSettingsUI");
                InvokePrivate(controller, "EnsureShopUI");
                InvokePrivate(controller, "EnsureResultPanel");
                InvokePrivate(controller, "EnsureBoosterPurchaseUI");

                SavePanelPrefab<SettingsPanelPrefabRefs>(canvasGO.transform, "SettingsPanel", "SettingsPanel", overwriteExisting);
                SavePanelPrefab<ShopPanelPrefabRefs>(canvasGO.transform, "ShopPanel", "ShopPanel", overwriteExisting);
                SavePanelPrefab<ResultPanelPrefabRefs>(canvasGO.transform, "ResultPanel", "ResultPanel", overwriteExisting);
                SavePanelPrefab<BoosterPurchasePanelPrefabRefs>(canvasGO.transform, "BoosterPurchasePanel", "BoosterPurchasePanel", overwriteExisting);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void SavePanelPrefab<TRefs>(Transform canvasRoot, string panelName, string prefabFileName, bool overwriteExisting) where TRefs : Component
        {
            var panel = FindChildByName(canvasRoot, panelName);
            if (panel == null)
            {
                Debug.LogWarning($"[LoopSortingUIPrefabExporter] Panel '{panelName}' not found under canvas.");
                return;
            }

            var refs = panel.GetComponent<TRefs>();
            if (refs == null) refs = panel.gameObject.AddComponent<TRefs>();

            if (refs is SettingsPanelPrefabRefs settings) settings.AutoAssign();
            if (refs is ShopPanelPrefabRefs shop) shop.AutoAssign();
            if (refs is ResultPanelPrefabRefs result) result.AutoAssign();
            if (refs is BoosterPurchasePanelPrefabRefs booster) booster.AutoAssign();

            // Ensure there is a CanvasGroup so runtime `AnimateUiPanel()` works with prefabs too.
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;

            string assetPath = $"{OutputFolder}/{prefabFileName}.prefab";
            if (!overwriteExisting && File.Exists(assetPath))
            {
                Debug.Log($"[LoopSortingUIPrefabExporter] Skip (already exists): {assetPath}");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(panel.gameObject, assetPath);
            Debug.Log($"[LoopSortingUIPrefabExporter] Saved: {assetPath}");
        }

        private static void EnsureFolder(string folderPath)
        {
            folderPath = folderPath.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
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

        private static Transform FindChildByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name)) return null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name) return t;
            }
            return null;
        }

        private static void SetPrivateField(object instance, string fieldName, object value)
        {
            if (instance == null) return;
            var f = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (f == null)
            {
                Debug.LogWarning($"[LoopSortingUIPrefabExporter] Field not found: {instance.GetType().Name}.{fieldName}");
                return;
            }
            f.SetValue(instance, value);
        }

        private static void InvokePrivate(object instance, string methodName)
        {
            if (instance == null) return;
            var m = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (m == null)
            {
                Debug.LogWarning($"[LoopSortingUIPrefabExporter] Method not found: {instance.GetType().Name}.{methodName}()");
                return;
            }
            try
            {
                m.Invoke(instance, null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif
