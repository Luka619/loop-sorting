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
    }
}
