using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Auto-spawn a simple runtime visualizer/loader so play mode uses the selected level without manual scene setup.
    /// </summary>
    public static class LevelRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoad()
        {
            var config = Resources.Load<LevelRuntimeConfig>("Levels/LevelRuntimeConfig");
            if (config == null || config.activeLevel == null)
            {
                Debug.LogWarning("LevelRuntimeBootstrap: No Levels/LevelRuntimeConfig (in Resources) or activeLevel set.");
                return;
            }

            var go = new GameObject("LevelRuntimeController");
            Object.DontDestroyOnLoad(go);
            var controller = go.AddComponent<GameRuntimeController>();
            controller.Build(config.activeLevel);
        }
    }
}
