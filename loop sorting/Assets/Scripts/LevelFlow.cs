using System.Collections.Generic;
using UnityEngine;

namespace LoopSorting
{
    [CreateAssetMenu(menuName = "LoopSorting/Level Flow", fileName = "LevelFlow")]
    public class LevelFlow : ScriptableObject
    {
        [Tooltip("Sequence of levels played in order.")]
        public List<LevelLayout> levels = new List<LevelLayout>();
        [Tooltip("Index of the first level to play.")]
        public int startIndex = 0;
    }
}
