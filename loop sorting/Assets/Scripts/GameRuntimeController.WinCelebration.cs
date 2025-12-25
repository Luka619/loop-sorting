using System.Collections;
using UnityEngine;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private const float WinToastSeconds = 1.0f;
        private const int WinBounceCount = 3;
        private const float WinBounceInterval = 0.12f;
        private const float WinBounceScale = 0.06f;
        private const float WinBounceSeconds = 0.12f;

        private IEnumerator PlayWinCelebration()
        {
            yield return StartCoroutine(PlayWinBoxBounce());
            bool showToast = IsTutorialLevel(_currentLayoutSource ?? _currentLayout);
            if (showToast)
            {
                ShowTutorialMessage(LocalizedText.TutorialWinToast);
                yield return new WaitForSeconds(WinToastSeconds);
            }
            SetTutorialVisible(false);
        }

        private IEnumerator PlayWinBoxBounce()
        {
            if (_game == null || _boxViews == null || _boxViews.Count == 0)
            {
                yield break;
            }

            var wait = new WaitForSeconds(WinBounceInterval);
            for (int pulse = 0; pulse < WinBounceCount; pulse++)
            {
                for (int i = 0; i < _boxViews.Count; i++)
                {
                    if (!IsBoxFilled(i)) continue;
                    var view = _boxViews[i];
                    if (view == null) continue;
                    view.PlayBoxBounce(punchScale: WinBounceScale, seconds: WinBounceSeconds);
                }
                yield return wait;
            }
        }

        private bool IsBoxFilled(int index)
        {
            if (_game == null || _game.Containers == null) return false;
            if (index < 0 || index >= _game.Containers.Count) return false;
            var container = _game.Containers[index];
            if (container == null || container.Count == 0) return false;
            return container.IsUniformAndFull();
        }
    }
}
