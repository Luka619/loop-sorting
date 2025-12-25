using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private enum TutorialPhase
        {
            None,
            ClickBox,
            Done
        }

        private TutorialPhase _tutorialPhase = TutorialPhase.None;
        private int _tutorialTargetBoxIndex = -1;
        private Coroutine _tutorialPulseRoutine;

        private RectTransform _tutorialLayer;
        private RectTransform _tutorialBubble;
        private TMP_Text _tutorialText;
        private Image _tutorialBubbleBg;

        private const float TutorialBubbleYOffset = 240f;
        private const float TutorialBubbleWidth = 860f;
        private const float TutorialBubbleHeight = 160f;

        private bool IsTutorialActive => _tutorialPhase != TutorialPhase.None && _tutorialPhase != TutorialPhase.Done;

        private void SetupTutorial(LevelLayout layout)
        {
            if (!IsTutorialLevel(layout))
            {
                ResetTutorial();
                return;
            }

            EnsureTutorialUI();
            _tutorialTargetBoxIndex = PickTutorialTargetBox();
            if (_tutorialTargetBoxIndex < 0)
            {
                ResetTutorial();
                return;
            }

            _tutorialPhase = TutorialPhase.ClickBox;
            ShowTutorialMessage(LocalizedText.TutorialClickBox);
            StartTutorialPulse();
        }

        private void ResetTutorial()
        {
            _tutorialPhase = TutorialPhase.None;
            _tutorialTargetBoxIndex = -1;
            StopTutorialPulse();
            SetTutorialVisible(false);
        }

        private bool IsTutorialLevel(LevelLayout layout)
        {
            if (layout == null) return false;
            if (_flow != null && _flowIndex == 0) return true;
            return layout.name == "1";
        }

        private int PickTutorialTargetBox()
        {
            if (_boxViews == null || _boxViews.Count == 0) return -1;
            int best = -1;
            float bestY = float.NegativeInfinity;

            for (int i = 0; i < _boxViews.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                if (_game == null || _game.Containers == null || i >= _game.Containers.Count) continue;
                var container = _game.Containers[i];
                if (container == null || container.Count == 0) continue;

                float y = _boxViews[i] != null ? _boxViews[i].transform.position.y : 0f;
                if (best < 0 || y > bestY)
                {
                    best = i;
                    bestY = y;
                }
            }

            return best;
        }

        private bool IsTutorialClickAllowed(int containerIndex)
        {
            if (_tutorialPhase != TutorialPhase.ClickBox) return true;
            return containerIndex == _tutorialTargetBoxIndex;
        }

        private void NotifyTutorialContainerClicked(int containerIndex)
        {
            if (_tutorialPhase != TutorialPhase.ClickBox) return;
            if (containerIndex != _tutorialTargetBoxIndex) return;
            AdvanceTutorialPhase(TutorialPhase.Done);
        }

        private void AdvanceTutorialPhase(TutorialPhase phase)
        {
            StopTutorialPulse();
            _tutorialPhase = phase;

            if (phase == TutorialPhase.Done || phase == TutorialPhase.None)
            {
                SetTutorialVisible(false);
            }
        }

        private void UpdateTutorial()
        {
            if (!IsTutorialActive) return;
        }

        private void EnsureTutorialUI()
        {
            if (_tutorialLayer != null || _hudRootRect == null) return;

            var layerGO = new GameObject("TutorialLayer");
            layerGO.transform.SetParent(_hudRootRect, false);
            _tutorialLayer = layerGO.AddComponent<RectTransform>();
            _tutorialLayer.anchorMin = Vector2.zero;
            _tutorialLayer.anchorMax = Vector2.one;
            _tutorialLayer.offsetMin = Vector2.zero;
            _tutorialLayer.offsetMax = Vector2.zero;

            var bubbleGO = new GameObject("TutorialBubble");
            bubbleGO.transform.SetParent(_tutorialLayer, false);
            _tutorialBubble = bubbleGO.AddComponent<RectTransform>();
            _tutorialBubble.anchorMin = new Vector2(0.5f, 1f);
            _tutorialBubble.anchorMax = new Vector2(0.5f, 1f);
            _tutorialBubble.pivot = new Vector2(0.5f, 1f);
            _tutorialBubble.sizeDelta = new Vector2(TutorialBubbleWidth, TutorialBubbleHeight);
            UpdateTutorialBubblePosition();

            _tutorialBubbleBg = bubbleGO.AddComponent<Image>();
            _tutorialBubbleBg.raycastTarget = false;
            if (LoopSortingUIKit.IsAvailable())
            {
                _tutorialBubbleBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info");
                if (_tutorialBubbleBg.sprite != null)
                {
                    _tutorialBubbleBg.type = _tutorialBubbleBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    _tutorialBubbleBg.color = Color.white;
                }
                else
                {
                    _tutorialBubbleBg.color = new Color(0f, 0f, 0f, 0.55f);
                }
            }
            else
            {
                _tutorialBubbleBg.color = new Color(0f, 0f, 0f, 0.55f);
            }

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(bubbleGO.transform, false);
            _tutorialText = textGO.AddComponent<TextMeshProUGUI>();
            _tutorialText.raycastTarget = false;
            _tutorialText.text = string.Empty;
            _tutorialText.alignment = TextAlignmentOptions.Center;
            _tutorialText.fontSize = 44;
            _tutorialText.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            _tutorialText.enableWordWrapping = true;
            _tutorialText.overflowMode = TextOverflowModes.Ellipsis;
            ApplyTmpOutlineUnderlay(
                _tutorialText,
                outlineWidth: 0.12f,
                outlineColor: new Color(1f, 1f, 1f, 0.5f),
                underlayColor: new Color(0f, 0f, 0f, 0.12f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.32f,
                underlayDilate: 0.05f);

            var textRect = _tutorialText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(24f, 18f);
            textRect.offsetMax = new Vector2(-24f, -18f);

            bubbleGO.SetActive(false);
        }

        private void UpdateTutorialBubblePosition()
        {
            if (_tutorialBubble == null) return;
            float y = -(TutorialBubbleYOffset + _hudTopInsetUnits);
            _tutorialBubble.anchoredPosition = new Vector2(0f, y);
        }

        private void ShowTutorialMessage(string message)
        {
            EnsureTutorialUI();
            UpdateTutorialBubblePosition();
            if (_tutorialText != null) _tutorialText.text = message ?? string.Empty;
            SetTutorialVisible(true);
        }

        private void SetTutorialVisible(bool visible)
        {
            if (_tutorialBubble != null)
            {
                _tutorialBubble.gameObject.SetActive(visible);
            }
        }

        private void StartTutorialPulse()
        {
            StopTutorialPulse();
            if (_tutorialPhase != TutorialPhase.ClickBox) return;
            _tutorialPulseRoutine = StartCoroutine(TutorialPulseRoutine());
        }

        private void StopTutorialPulse()
        {
            if (_tutorialPulseRoutine == null) return;
            StopCoroutine(_tutorialPulseRoutine);
            _tutorialPulseRoutine = null;
        }

        private IEnumerator TutorialPulseRoutine()
        {
            var wait = new WaitForSeconds(0.8f);
            while (_tutorialPhase == TutorialPhase.ClickBox)
            {
                var view = GetTutorialTargetBoxView();
                if (view != null)
                {
                    var c = new Color(1f, 1f, 1f, 0.9f);
                    view.PlayInfoHint(c, sizeFactor: 1.1f, seconds: 0.18f);
                    view.PlayBoxBounce();
                }
                yield return wait;
            }
            _tutorialPulseRoutine = null;
        }

        private BoxView GetTutorialTargetBoxView()
        {
            if (_tutorialTargetBoxIndex < 0 || _tutorialTargetBoxIndex >= _boxViews.Count) return null;
            return _boxViews[_tutorialTargetBoxIndex];
        }
    }
}
