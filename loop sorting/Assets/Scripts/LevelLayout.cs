using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Level layout data used by both runtime and the editor.
    /// </summary>
    [CreateAssetMenu(menuName = "LoopSorting/Level Layout", fileName = "LevelLayout")]
    public class LevelLayout : ScriptableObject
    {
        [Tooltip("Max blocks allowed on conveyor (0 = unlimited).")]
        public int beltCapacity = 0;
        [Tooltip("Desired spacing between belt slots (units). 0 = use controller default.")]
        public float beltSlotSpacing = 0.6f;
        [Tooltip("Smooth corners for conveyor path in preview/runtime.")]
        public bool smoothCorners = true;
        [Tooltip("Corner smoothing tension (0=straight, 1=very round).")]
        [Range(0f, 1f)] public float cornerSmoothTension = 0.5f;
        [Tooltip("Subdivisions per segment when smoothing (>1 enables rounding).")]
        [Range(2, 24)] public int cornerSubdivisions = 10;
        public List<ConveyorPath> conveyors = new List<ConveyorPath>();
        public List<BoxSpec> boxes = new List<BoxSpec>();
    }
}
