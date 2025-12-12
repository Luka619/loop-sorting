using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    /// <summary>
    /// Simple UI counter to display remaining empty slots on the conveyor.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class BeltCounterUI : MonoBehaviour
    {
        private Text _text;

        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        public void SetValue(int empty, int total)
        {
            if (_text == null) return;
            _text.text = $"Empty Slots: {empty}/{total}";
        }
    }
}
