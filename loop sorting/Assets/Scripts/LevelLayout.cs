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
        [Tooltip("Global block edge length (units). Used to derive box size = columns * blockSize by rows * blockSize.")]
        public float blockSize = 0.6f;
        [Header("Layout Auto Fix")]
        [Tooltip("Override the runtime defaults for auto layout fixes in this level.")]
        public bool overrideLayoutAutoSettings = false;
        [Tooltip("Auto push boxes away from the belt when they overlap or get too close.")]
        public bool autoResolveLayoutOverlap = true;
        [Tooltip("Minimum gap between box bounds and the belt ribbon (world units).")]
        public float minBoxToBeltGap = 0.08f;
        [Tooltip("Preferred gap between box bounds and the belt ribbon (world units). 0 = disabled.")]
        public float preferredBoxToBeltGap = 0.18f;
        [Range(1, 8)]
        public int overlapResolveIterations = 3;
        [Header("Camera Clamp")]
        [Tooltip("Clamp the max orthographic size so oversized layouts don't look too small (0 = disabled).")]
        public float cameraMaxOrthoSize = 0f;
        [Tooltip("Minimum on-screen pixel size for a single block (0 = disabled). May crop very large layouts.")]
        public float minBlockPixelSize = 0f;
        public List<ConveyorPath> conveyors = new List<ConveyorPath>();
        public List<BoxSpec> boxes = new List<BoxSpec>();
    }
}
