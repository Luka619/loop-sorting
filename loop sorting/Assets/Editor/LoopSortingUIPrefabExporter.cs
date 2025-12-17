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
        private const string GameplayHudPrefabName = "GameplayHUD";

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

        [MenuItem("LoopSorting/UI/Generate Gameplay HUD Prefab (if missing)")]
        public static void GenerateGameplayHudIfMissing()
        {
            GenerateGameplayHudInternal(overwriteExisting: false);
        }

        [MenuItem("LoopSorting/UI/Regenerate Gameplay HUD Prefab (overwrite)")]
        public static void RegenerateGameplayHudOverwrite()
        {
            if (!EditorUtility.DisplayDialog(
                    "Regenerate Gameplay HUD Prefab",
                    $"This will overwrite {OutputFolder}/{GameplayHudPrefabName}.prefab and may discard your manual layout tweaks.\n\nContinue?",
                    "Overwrite",
                    "Cancel"))
            {
                return;
            }

            GenerateGameplayHudInternal(overwriteExisting: true);
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
                GenerateGameplayHudUnder(root.transform, overwriteExisting);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void GenerateGameplayHudInternal(bool overwriteExisting)
        {
            EnsureFolder(OutputFolder);

            var root = new GameObject("__LoopSortingGameplayHudPrefabExportRoot");
            root.hideFlags = HideFlags.HideAndDontSave;

            try
            {
                GenerateGameplayHudUnder(root.transform, overwriteExisting);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void GenerateGameplayHudUnder(Transform parent, bool overwriteExisting)
        {
            var controllerGO = new GameObject("__TempController_HUD");
            controllerGO.hideFlags = HideFlags.HideAndDontSave;
            controllerGO.transform.SetParent(parent, false);

            var controller = controllerGO.AddComponent<GameRuntimeController>();
            InvokeEnsureHud(controller);
            SaveGameplayHudPrefab(controller, overwriteExisting);

            var canvas = GetPrivateField<Canvas>(controller, "_uiCanvas");
            if (canvas != null)
            {
                UnityEngine.Object.DestroyImmediate(canvas.gameObject);
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

        private static void InvokeEnsureHud(GameRuntimeController controller)
        {
            if (controller == null) return;

            // Export a "full" HUD so designers can tweak everything in the prefab.
            controller.shopEnabled = true;
            controller.livesHudEnabled = true;

            InvokePrivate(controller, "EnsureCounterUI");
        }

        private static void SaveGameplayHudPrefab(GameRuntimeController controller, bool overwriteExisting)
        {
            if (controller == null) return;

            var canvas = GetPrivateField<Canvas>(controller, "_uiCanvas");
            if (canvas == null)
            {
                Debug.LogWarning("[LoopSortingUIPrefabExporter] HUD canvas not found after EnsureCounterUI().");
                return;
            }

            var hudRoot = FindChildByName(canvas.transform, "HUDRoot");
            if (hudRoot == null)
            {
                Debug.LogWarning("[LoopSortingUIPrefabExporter] HUDRoot not found under HUD canvas.");
                return;
            }

            var refs = hudRoot.GetComponent<GameplayHudPrefabRefs>();
            if (refs == null) refs = hudRoot.gameObject.AddComponent<GameplayHudPrefabRefs>();
            refs.AutoAssign();
            refs.authoredTopInsetUnits = controller.HudTopInsetUnits;
            refs.authoredRightInsetUnits = controller.HudRightInsetUnits;
            refs.authoredBottomInsetUnits = controller.HudBottomInsetUnits;

            string assetPath = $"{OutputFolder}/{GameplayHudPrefabName}.prefab";
            if (!overwriteExisting && File.Exists(assetPath))
            {
                Debug.Log($"[LoopSortingUIPrefabExporter] Skip (already exists): {assetPath}");
                return;
            }

            PrefabUtility.SaveAsPrefabAsset(hudRoot.gameObject, assetPath);
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

        private static T GetPrivateField<T>(object instance, string fieldName) where T : class
        {
            if (instance == null) return null;
            var f = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (f == null) return null;
            return f.GetValue(instance) as T;
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
