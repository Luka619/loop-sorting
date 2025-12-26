using System.Collections;
using System.Collections.Generic;
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
            ClickBoxFirst,
            WaitSecondBoxComplete,
            ClickBoxAgain,
            Done
        }

        private TutorialPhase _tutorialPhase = TutorialPhase.None;
        private int _tutorialTargetBoxIndex = -1;
        private int _tutorialSecondaryBoxIndex = -1;
        private Coroutine _tutorialPulseRoutine;

        private RectTransform _tutorialLayer;
        private RectTransform _tutorialBubble;
        private TMP_Text _tutorialText;
        private Image _tutorialBubbleBg;
        private RectTransform _tutorialHand;
        private Image _tutorialHandImage;
        private Vector3 _tutorialHandBaseScale = Vector3.one;

        private const float TutorialBubbleYOffset = 240f;
        private const float TutorialBubbleWidth = 860f;
        private const float TutorialBubbleHeight = 190f;
        private const string TutorialHandSpritePath = "Tutorial/tutorial_hand";
        private const float TutorialHandSize = 150f;
        private const float TutorialHandTapScale = -0.1f;
        private const float TutorialHandTapSeconds = 0.22f;
        private const float TutorialHandPixelsPerUnit = 100f;
        private static readonly Vector2 TutorialHandOffset = new Vector2(-40f, 30f);

        private bool IsTutorialActive => _tutorialPhase != TutorialPhase.None && _tutorialPhase != TutorialPhase.Done;

        private void SetupTutorial(LevelLayout layout)
        {
            if (!IsTutorialLevel(layout))
            {
                ResetTutorial();
                return;
            }

            EnsureTutorialUI();
            if (IsTutorialLevel2(layout))
            {
                if (!TryPickTutorialTopBoxes(out _tutorialTargetBoxIndex, out _tutorialSecondaryBoxIndex))
                {
                    ResetTutorial();
                    return;
                }

                _tutorialPhase = TutorialPhase.ClickBoxFirst;
                ShowTutorialMessage(LocalizedText.TutorialClickTopBox);
                StartTutorialPulse();
                UpdateTutorialHandVisibility();
                return;
            }

            _tutorialSecondaryBoxIndex = -1;
            _tutorialTargetBoxIndex = PickTutorialTargetBox();
            if (_tutorialTargetBoxIndex < 0)
            {
                ResetTutorial();
                return;
            }

            _tutorialPhase = TutorialPhase.ClickBoxFirst;
            ShowTutorialMessage(LocalizedText.TutorialClickBox);
            StartTutorialPulse();
            UpdateTutorialHandVisibility();
        }

        private void ResetTutorial()
        {
            _tutorialPhase = TutorialPhase.None;
            _tutorialTargetBoxIndex = -1;
            _tutorialSecondaryBoxIndex = -1;
            StopTutorialPulse();
            SetTutorialVisible(false);
            SetTutorialHandVisible(false);
        }

        private bool IsTutorialLevel(LevelLayout layout)
        {
            if (layout == null) return false;
            if (_flow != null) return _flowIndex == 0 || _flowIndex == 1;
            return layout.name == "1" || layout.name == "2";
        }

        private bool IsTutorialLevel2(LevelLayout layout)
        {
            if (layout == null) return false;
            if (_flow != null) return _flowIndex == 1;
            return layout.name == "2";
        }

        private bool IsTutorialWinToastLevel(LevelLayout layout)
        {
            if (layout == null) return false;
            if (_flow != null) return _flowIndex == 0;
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

        private bool TryPickTutorialTopBoxes(out int primary, out int secondary)
        {
            primary = -1;
            secondary = -1;
            if (_boxViews == null || _boxViews.Count == 0) return false;

            var candidates = new List<(int index, float y)>(_boxViews.Count);
            for (int i = 0; i < _boxViews.Count; i++)
            {
                if (i < _boxLocked.Count && _boxLocked[i]) continue;
                var view = _boxViews[i];
                if (view == null) continue;
                float y = view.transform.position.y;
                candidates.Add((i, y));
            }

            if (candidates.Count < 2) return false;
            candidates.Sort((a, b) => b.y.CompareTo(a.y));

            for (int i = 0; i < candidates.Count; i++)
            {
                int idx = candidates[i].index;
                if (HasContainerBlocks(idx))
                {
                    primary = idx;
                    break;
                }
            }

            if (primary < 0) return false;

            for (int i = 0; i < candidates.Count; i++)
            {
                int idx = candidates[i].index;
                if (idx == primary) continue;
                secondary = idx;
                break;
            }

            return secondary >= 0;
        }

        private bool IsTutorialClickAllowed(int containerIndex)
        {
            switch (_tutorialPhase)
            {
                case TutorialPhase.ClickBoxFirst:
                case TutorialPhase.ClickBoxAgain:
                    return containerIndex == _tutorialTargetBoxIndex;
                case TutorialPhase.WaitSecondBoxComplete:
                    return false;
                default:
                    return true;
            }
        }

        private void NotifyTutorialContainerClicked(int containerIndex)
        {
            if (containerIndex != _tutorialTargetBoxIndex) return;

            if (_tutorialPhase == TutorialPhase.ClickBoxFirst)
            {
                if (_tutorialSecondaryBoxIndex >= 0)
                {
                    AdvanceTutorialPhase(TutorialPhase.WaitSecondBoxComplete);
                    ShowTutorialMessage(LocalizedText.TutorialWaitSecondBox);
                }
                else
                {
                    AdvanceTutorialPhase(TutorialPhase.Done);
                }
            }
            else if (_tutorialPhase == TutorialPhase.ClickBoxAgain)
            {
                AdvanceTutorialPhase(TutorialPhase.Done);
            }
        }

        private void AdvanceTutorialPhase(TutorialPhase phase)
        {
            StopTutorialPulse();
            _tutorialPhase = phase;

            if (phase == TutorialPhase.ClickBoxFirst || phase == TutorialPhase.ClickBoxAgain)
            {
                StartTutorialPulse();
            }
            else if (phase == TutorialPhase.Done || phase == TutorialPhase.None)
            {
                SetTutorialVisible(false);
            }

            UpdateTutorialHandVisibility();
        }

        private void UpdateTutorial()
        {
            if (!IsTutorialActive) return;
            if (_tutorialPhase == TutorialPhase.WaitSecondBoxComplete)
            {
                if (_tutorialSecondaryBoxIndex < 0) return;
                if (!_isReleasing && IsBoxCompleted(_tutorialSecondaryBoxIndex))
                {
                    AdvanceTutorialPhase(TutorialPhase.ClickBoxAgain);
                    ShowTutorialMessage(LocalizedText.TutorialClickTopBoxAgain);
                }
            }

            UpdateTutorialHandPosition();
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
            _tutorialText.enableAutoSizing = true;
            _tutorialText.fontSizeMax = 44;
            _tutorialText.fontSizeMin = 30;
            _tutorialText.color = new Color(0.12f, 0.12f, 0.12f, 1f);
            _tutorialText.enableWordWrapping = true;
            _tutorialText.overflowMode = TextOverflowModes.Overflow;
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

            var handGO = new GameObject("TutorialHand");
            handGO.transform.SetParent(_tutorialLayer, false);
            _tutorialHand = handGO.AddComponent<RectTransform>();
            _tutorialHand.anchorMin = new Vector2(0.5f, 0.5f);
            _tutorialHand.anchorMax = new Vector2(0.5f, 0.5f);
            _tutorialHand.pivot = new Vector2(0.5f, 1f);
            _tutorialHand.sizeDelta = new Vector2(TutorialHandSize, TutorialHandSize);

            _tutorialHandImage = handGO.AddComponent<Image>();
            _tutorialHandImage.raycastTarget = false;
            var handSprite = Resources.Load<Sprite>(TutorialHandSpritePath);
            if (handSprite == null)
            {
                var handTex = Resources.Load<Texture2D>(TutorialHandSpritePath);
                if (handTex != null)
                {
                    handSprite = Sprite.Create(
                        handTex,
                        new Rect(0f, 0f, handTex.width, handTex.height),
                        new Vector2(0.5f, 1f),
                        TutorialHandPixelsPerUnit);
                }
            }
            if (handSprite == null)
            {
                Debug.LogWarning($"Tutorial hand sprite missing at Resources/{TutorialHandSpritePath}.png");
            }
            _tutorialHandImage.sprite = handSprite;
            _tutorialHandImage.color = Color.white;
            _tutorialHandImage.preserveAspect = true;
            _tutorialHandBaseScale = _tutorialHand.localScale;
            handGO.SetActive(false);

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

        private void SetTutorialHandVisible(bool visible)
        {
            if (_tutorialHand != null)
            {
                if (visible)
                {
                    _tutorialHand.localScale = _tutorialHandBaseScale;
                    _tutorialHand.gameObject.SetActive(true);
                    return;
                }
                _tutorialHand.gameObject.SetActive(false);
            }
        }

        private void UpdateTutorialHandVisibility()
        {
            bool visible = _tutorialPhase == TutorialPhase.ClickBoxFirst || _tutorialPhase == TutorialPhase.ClickBoxAgain;
            SetTutorialHandVisible(visible);
            if (visible) UpdateTutorialHandPosition();
        }

        private void StartTutorialPulse()
        {
            StopTutorialPulse();
            if (_tutorialPhase != TutorialPhase.ClickBoxFirst && _tutorialPhase != TutorialPhase.ClickBoxAgain) return;
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
            while (_tutorialPhase == TutorialPhase.ClickBoxFirst || _tutorialPhase == TutorialPhase.ClickBoxAgain)
            {
                var view = GetTutorialTargetBoxView();
                if (view != null)
                {
                    var c = new Color(1f, 1f, 1f, 0.9f);
                    view.PlayInfoHint(c, sizeFactor: 1.1f, seconds: 0.18f);
                    view.PlayBoxBounce();
                }
                if (_tutorialHand != null && _tutorialHand.gameObject.activeInHierarchy)
                {
                    StartCoroutine(MotionUtil.ScalePunch(_tutorialHand, _tutorialHandBaseScale, TutorialHandTapScale, TutorialHandTapSeconds));
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

        private void UpdateTutorialHandPosition()
        {
            if (_tutorialHand == null || !_tutorialHand.gameObject.activeInHierarchy) return;
            if (_hudRootRect == null) return;

            var view = GetTutorialTargetBoxView();
            if (view == null)
            {
                return;
            }

            var cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Vector3 world = view.transform.position;
            var boxCollider = view.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                var bounds = boxCollider.bounds;
                world = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);
            }
            else
            {
                var renderer = view.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    var bounds = renderer.bounds;
                    world = new Vector3(bounds.max.x, bounds.min.y, bounds.center.z);
                }
            }

            var screen = cam.WorldToScreenPoint(world);
            if (screen.z < 0.01f)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_hudRootRect, screen, null, out var local))
            {
                _tutorialHand.anchoredPosition = local + TutorialHandOffset;
            }
        }

        private bool HasContainerBlocks(int index)
        {
            if (_game == null || _game.Containers == null) return false;
            if (index < 0 || index >= _game.Containers.Count) return false;
            var container = _game.Containers[index];
            return container != null && container.Count > 0;
        }

        private bool IsBoxCompleted(int index)
        {
            if (index < 0 || index >= _boxCompleted.Count) return false;
            return _boxCompleted[index];
        }
    }
}
