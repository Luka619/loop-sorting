using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    /// <summary>
    /// Opening side for a container mouth.
    /// </summary>
    public enum OpeningSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    /// <summary>
    /// Conveyor path described as a set of 2D points in order.
    /// Direction flows from element 0 to the end; loop makes it wrap.
    /// </summary>
    [System.Serializable]
    public class ConveyorPath
    {
        public string name = "Conveyor";
        public List<Vector2> points = new List<Vector2>();
        public bool loop;
        public float width = 1f;
    }

    /// <summary>
    /// Container configuration (position is center in world units).
    /// Size is width/height in world units.
    /// Capacity is how many blocks fit; use size.x * size.y as a guide.
    /// </summary>
    [System.Serializable]
    public class ColorCount
    {
        public BlockColor color = BlockColor.Red;
        public int count = 1;
        public bool hidden = false;
    }

    /// <summary>
    /// Container configuration (position is center in world units).
    /// Size is width/height in world units.
    /// Capacity = columns * rows.
    /// </summary>
    [System.Serializable]
    public class BoxSpec
    {
        public string name = "Box";
        public Vector2 position;
        [HideInInspector] public Vector2 size = Vector2.one;
        public Color color = Color.white;
        [Tooltip("Grid width (a) for visual layout and capacity calculation a*b.")]
        public int columns = 1;
        [Tooltip("Grid height (b) for visual layout and capacity calculation a*b.")]
        public int rows = 1;
        public OpeningSide opening = OpeningSide.Top;
        [Tooltip("自动对齐到开口方向最近的传送带槽位。关闭后使用 beltSlotIndex。")]
        public bool autoAlignSlot = true;
        [Tooltip("颜色+数量。按列表顺序依次填充，索引0为最外层，直到容量(columns*rows)填满。")]
        public List<ColorCount> colorCounts = new List<ColorCount>();

        // Deprecated: use colorCounts. Kept hidden for backward compatibility.
        [HideInInspector] public List<BlockColor> initialBlocks = new List<BlockColor>();

        [Tooltip("Belt slot index where this box connects (0-based along conveyor slots).")]
        public int beltSlotIndex = 0;

        [Tooltip("Whether this box is locked and hidden until unlockColor box is completed.")]
        public bool locked = false;
        [Tooltip("The color that must be completed in another box to unlock this box.")]
        public BlockColor unlockColor = BlockColor.Red;
    }

}
