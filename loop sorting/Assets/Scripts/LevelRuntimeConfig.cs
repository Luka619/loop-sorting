using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Holds the currently active level for runtime boot.
    /// Place this asset under Resources/LevelRuntimeConfig.asset.
    /// </summary>
    [CreateAssetMenu(menuName = "LoopSorting/Runtime Config", fileName = "LevelRuntimeConfig")]
    public class LevelRuntimeConfig : ScriptableObject
    {
        public LevelLayout activeLevel;
        [Tooltip("Optional flow to drive sequential levels. If set, overrides activeLevel.")]
        public LevelFlow activeFlow;
        public int flowStartIndex = 0;
        [Tooltip("0-based flow indices that unlock a new mechanic (used for win-screen progress).")]
        public List<int> resultNewMechanicLevelIndices = new List<int>();
        [Header("UI Layout")]
        [Tooltip("Allow runtime code to override prefab layout (positions/sizes/anchors).")]
        public bool useRuntimeUiLayoutOverrides = false;
        [Tooltip("Allow runtime to auto-create UI if prefabs or nodes are missing.")]
        public bool allowRuntimeUiAutoCreate = true;
    }
}
