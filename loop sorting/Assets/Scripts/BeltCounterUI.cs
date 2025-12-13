using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LoopSorting
{
    /// <summary>
    /// Simple UI counter to display remaining empty slots on the conveyor.
    /// </summary>
    // Supports both `Text` and `TMP_Text` (UI kit uses TMP by default).
    public class BeltCounterUI : MonoBehaviour
    {
        private Text _text;
        private TMP_Text _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
            _text = GetComponent<Text>();
        }

        public void SetValue(int empty, int total)
        {
            string value = Mathf.Max(0, empty).ToString();
            if (_tmp != null) _tmp.text = value;
            if (_text != null) _text.text = value;
        }
    }
}
