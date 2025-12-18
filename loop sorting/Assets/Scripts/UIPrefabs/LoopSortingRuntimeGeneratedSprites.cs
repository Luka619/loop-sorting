using UnityEngine;

namespace LoopSorting
{
    // Editor-only helper container: used by LoopSortingUIPrefabRuntimeSaver to persist runtime-created sprites
    // (e.g. sprites created from Texture2D via Sprite.Create) so FULL prefab saves don't turn UI images white.
    public sealed class LoopSortingRuntimeGeneratedSprites : ScriptableObject
    {
    }
}

