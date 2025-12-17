#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LoopSorting.Editor
{
    public static class LoopSortingHudRuntimeSaver
    {
        private const string PrefabAssetPath = "Assets/Resources/UI/GameplayHUD.prefab";

        [MenuItem("LoopSorting/UI/Save Runtime Gameplay HUD To Prefab (Play Mode)")]
        public static void SaveRuntimeHudToPrefab()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Save Runtime HUD", "Enter Play Mode first, then run this command to save the current HUD to prefab.", "OK");
                return;
            }

            var controller = Object.FindObjectOfType<GameRuntimeController>();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Save Runtime HUD", "GameRuntimeController not found.", "OK");
                return;
            }

            var hud = Object.FindObjectOfType<GameplayHudPrefabRefs>();
            if (hud == null)
            {
                EditorUtility.DisplayDialog("Save Runtime HUD", "GameplayHudPrefabRefs not found in the current scene. Is the HUD instantiated from the GameplayHUD prefab?", "OK");
                return;
            }

            hud.AutoAssign();

            // Record the insets that were applied while this HUD was running, so next runs apply only the delta.
            hud.authoredTopInsetUnits = controller.HudTopInsetUnits;
            hud.authoredRightInsetUnits = controller.HudRightInsetUnits;
            hud.authoredBottomInsetUnits = controller.HudBottomInsetUnits;

            EnsureFolderForAsset(PrefabAssetPath);

            PrefabUtility.SaveAsPrefabAsset(hud.gameObject, PrefabAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LoopSortingHudRuntimeSaver] Saved runtime HUD to: {PrefabAssetPath}");
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
    }
}
#endif
