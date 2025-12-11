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
            bool hasFlow = config != null && config.activeFlow != null && config.activeFlow.levels.Count > 0;
            bool hasLevel = config != null && config.activeLevel != null;
            if (!hasFlow && !hasLevel)
            {
                Debug.LogWarning("LevelRuntimeBootstrap: No Levels/LevelRuntimeConfig (in Resources) or activeLevel set.");
                return;
            }

            var go = new GameObject("LevelRuntimeController");
            Object.DontDestroyOnLoad(go);
            var controller = go.AddComponent<GameRuntimeController>();
            if (hasFlow)
            {
                int start = Mathf.Clamp(config.flowStartIndex, 0, config.activeFlow.levels.Count - 1);
                controller.Build(config.activeFlow, start);
            }
            else
            {
                controller.Build(config.activeLevel);
            }
        }
    }
}
