using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private void BeginEndSequence(bool win, float delaySeconds)
        {
            if (_endSequenceRoutine != null) return;

            // Freeze gameplay immediately so the state doesn't keep changing while we play the final feedback.
            _gameOver = true;
            _inputLocked = true;
            _isReleasing = false;
            StopFullBeltFastForward();

            _endSequenceRoutine = StartCoroutine(PlayEndSequenceThenShowResult(win, delaySeconds));
        }

        private IEnumerator PlayEndSequenceThenShowResult(bool win, float delaySeconds)
        {
            delaySeconds = Mathf.Max(0f, delaySeconds);
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            ShowResult(win);
            _endSequenceRoutine = null;
        }

        private void ShowResult(bool win)
        {
            _gameOver = true;
            PlaySfx(SfxId.UiPopupOpen);
            PlaySfx(win ? SfxId.LevelWin : SfxId.LevelLose);
            EnsureBgm();
            if (musicEnabled && _audio != null && _audio.Bgm != null)
            {
                _audio.PlayBgmStinger(win ? BgmStingerId.Win : BgmStingerId.Lose);
                _audio.Bgm.FadeOutLoops(fadeSeconds: 0.9f);
            }
            EnsureResultPanel();
            EnsureResultCloseButton();
            CaptureResultButtonsBaseLayoutIfNeeded();

            AnimateUiPanel(_resultPanel, true, seconds: 0.22f);
            _resultText.text = win ? "VICTORY" : "FAILED";
            _resultPanelMode = win ? ResultPanelMode.Win : ResultPanelMode.Lose;

            if (win)
            {
                ConfigureResultWinRewardLayout();
            }
            else
            {
                ConfigureResultLoseReviveLayout();
            }
        }

        private void OnPrimaryClicked()
        {
            if (_resultPanelMode == ResultPanelMode.Win)
            {
                PlaySfx(SfxId.UiConfirm);
                PlayCoinFlyToHud(_resultWinRewardCoinPrimary, WinCoinsReward);
                GrantCoins(WinCoinsReward);
                HideUiPanelImmediate(_resultPanel);
                _resultPanelMode = ResultPanelMode.None;
                AdvanceAfterWinResult();
                return;
            }

            if (_resultPanelMode == ResultPanelMode.Lose)
            {
                if (_progress.Coins < LoseReviveCoinsCost)
                {
                    PlaySfx(SfxId.UiDenied);
                    return;
                }

                PlaySfx(SfxId.UiConfirm);
                SpendCoins(LoseReviveCoinsCost);
                BeginReviveFromResultPanel(useAd: false);
                return;
            }

            // Legacy fallback.
            PlaySfx(SfxId.UiConfirm);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            PlaySfx(SfxId.LevelRetry);
            RestartCurrent();
        }

        private void OnSecondaryClicked()
        {
            if (_resultPanelMode == ResultPanelMode.Win)
            {
                // Placeholder: grant immediately. Hook your ad SDK here.
                PlaySfx(SfxId.UiConfirm);
                PlayCoinFlyToHud(_resultWinRewardCoinSecondary, WinCoinsReward * WinAdRewardMultiplier);
                GrantCoins(WinCoinsReward * WinAdRewardMultiplier);
                HideUiPanelImmediate(_resultPanel);
                _resultPanelMode = ResultPanelMode.None;
                AdvanceAfterWinResult();
                return;
            }

            if (_resultPanelMode == ResultPanelMode.Lose)
            {
                // Placeholder: revive immediately. Hook your ad SDK here.
                PlaySfx(SfxId.UiConfirm);
                BeginReviveFromResultPanel(useAd: true);
                return;
            }

            // Legacy fallback.
            PlaySfx(SfxId.UiClick);
            if (_resultPanel != null) _resultPanel.SetActive(false);
            PlaySfx(SfxId.LevelRetry);
            RestartCurrent();
        }

        private void CaptureResultButtonsBaseLayoutIfNeeded()
        {
            if (_resultButtonsBaseLayoutCaptured) return;
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            _resultPrimaryBaseAnchorMin = primaryRect.anchorMin;
            _resultPrimaryBaseAnchorMax = primaryRect.anchorMax;
            _resultPrimaryBaseAnchoredPosition = primaryRect.anchoredPosition;
            _resultPrimaryBaseSizeDelta = primaryRect.sizeDelta;
            _resultSecondaryBaseAnchorMin = secondaryRect.anchorMin;
            _resultSecondaryBaseAnchorMax = secondaryRect.anchorMax;
            _resultSecondaryBaseAnchoredPosition = secondaryRect.anchoredPosition;
            _resultSecondaryBaseSizeDelta = secondaryRect.sizeDelta;
            _resultButtonsBaseLayoutCaptured = true;
        }

        private void ApplyResultButtonsLayoutForWinRewards()
        {
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            // If the prefab was already authored/saved in win order (secondary above primary), keep it.
            if (secondaryRect.localPosition.y > primaryRect.localPosition.y + 0.01f)
            {
                return;
            }

            if (!_resultButtonsBaseLayoutCaptured) CaptureResultButtonsBaseLayoutIfNeeded();
            if (!_resultButtonsBaseLayoutCaptured) return;

            // Swap Y layout: ad button (secondary) goes above, normal reward (primary) goes below.
            primaryRect.anchorMin = _resultSecondaryBaseAnchorMin;
            primaryRect.anchorMax = _resultSecondaryBaseAnchorMax;
            primaryRect.anchoredPosition = _resultSecondaryBaseAnchoredPosition;

            secondaryRect.anchorMin = _resultPrimaryBaseAnchorMin;
            secondaryRect.anchorMax = _resultPrimaryBaseAnchorMax;
            secondaryRect.anchoredPosition = _resultPrimaryBaseAnchoredPosition;
        }

        private void ApplyResultButtonsSizeForWinRewards()
        {
            if (!_resultButtonsBaseLayoutCaptured) CaptureResultButtonsBaseLayoutIfNeeded();
            if (!_resultButtonsBaseLayoutCaptured) return;
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            float primaryW = Mathf.Abs(_resultPrimaryBaseSizeDelta.x);
            float secondaryW = Mathf.Abs(_resultSecondaryBaseSizeDelta.x);
            float primaryH = Mathf.Abs(_resultPrimaryBaseSizeDelta.y);
            float secondaryH = Mathf.Abs(_resultSecondaryBaseSizeDelta.y);

            // Use a consistent long-button size for the win rewards, like the reference.
            float targetW = (primaryW > 1f && secondaryW > 1f) ? Mathf.Min(primaryW, secondaryW) : 760f;
            float targetH = (primaryH > 1f && secondaryH > 1f) ? Mathf.Min(primaryH, secondaryH) : 180f;

            var targetSize = new Vector2(targetW, targetH);
            primaryRect.sizeDelta = targetSize;
            secondaryRect.sizeDelta = targetSize;

            var pPos = primaryRect.anchoredPosition;
            pPos.x = 0f;
            primaryRect.anchoredPosition = pPos;

            var sPos = secondaryRect.anchoredPosition;
            sPos.x = 0f;
            secondaryRect.anchoredPosition = sPos;
        }

        private void RestoreResultButtonsLayoutBase()
        {
            if (!_resultButtonsBaseLayoutCaptured) CaptureResultButtonsBaseLayoutIfNeeded();
            if (!_resultButtonsBaseLayoutCaptured) return;
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            primaryRect.anchorMin = _resultPrimaryBaseAnchorMin;
            primaryRect.anchorMax = _resultPrimaryBaseAnchorMax;
            primaryRect.anchoredPosition = _resultPrimaryBaseAnchoredPosition;
            primaryRect.sizeDelta = _resultPrimaryBaseSizeDelta;

            secondaryRect.anchorMin = _resultSecondaryBaseAnchorMin;
            secondaryRect.anchorMax = _resultSecondaryBaseAnchorMax;
            secondaryRect.anchoredPosition = _resultSecondaryBaseAnchoredPosition;
            secondaryRect.sizeDelta = _resultSecondaryBaseSizeDelta;
        }

        private void ConfigureResultWinRewardLayout()
        {
            ApplyResultButtonsLayoutForWinRewards();

            // Don't override prefab-authored button layout. This lets "Apply Runtime Layout To Prefabs" persist
            // manual tweaks for the win result screen (positions/sizes of Primary/Secondary buttons).
            bool hasAuthoredWinLayout =
                (_primaryButton != null && _primaryButton.transform.Find("WinRewardLayout") != null) ||
                (_secondaryButton != null && _secondaryButton.transform.Find("WinRewardLayout") != null);
            if (!hasAuthoredWinLayout)
            {
                ApplyResultButtonsSizeForWinRewards();
            }

            if (_resultCloseButton != null) _resultCloseButton.gameObject.SetActive(false);

            // Hide the default label + left icon layout, and use a centered reward row like the reference image.
            if (_primaryLabel != null) _primaryLabel.gameObject.SetActive(false);
            if (_secondaryLabel != null) _secondaryLabel.gameObject.SetActive(false);
            if (_resultPrimaryIcon != null) _resultPrimaryIcon.gameObject.SetActive(false);
            if (_resultSecondaryIcon != null) _resultSecondaryIcon.gameObject.SetActive(false);

            EnsureWinRewardLayoutPrimary();
            EnsureWinRewardLayoutSecondary();

            if (_resultWinRewardRootPrimary != null) _resultWinRewardRootPrimary.gameObject.SetActive(true);
            if (_resultWinRewardRootSecondary != null) _resultWinRewardRootSecondary.gameObject.SetActive(true);

            if (_resultWinRewardAmountPrimary != null) _resultWinRewardAmountPrimary.text = WinCoinsReward.ToString();
            if (_resultWinRewardAmountSecondary != null) _resultWinRewardAmountSecondary.text = (WinCoinsReward * WinAdRewardMultiplier).ToString();
        }

        private void EnsureWinRewardLayoutPrimary()
        {
            EnsureWinRewardLayout(
                button: _primaryButton,
                ref _resultWinRewardRootPrimary,
                ref _resultWinRewardAdPrimary,
                ref _resultWinRewardAmountPrimary,
                ref _resultWinRewardCoinPrimary,
                includeAdIcon: false);
        }

        private void EnsureWinRewardLayoutSecondary()
        {
            EnsureWinRewardLayout(
                button: _secondaryButton,
                ref _resultWinRewardRootSecondary,
                ref _resultWinRewardAdSecondary,
                ref _resultWinRewardAmountSecondary,
                ref _resultWinRewardCoinSecondary,
                includeAdIcon: true);
        }

        private void EnsureWinRewardLayout(
            Button button,
            ref RectTransform root,
            ref Image adIcon,
            ref TMP_Text amountText,
            ref Image coinIcon,
            bool includeAdIcon)
        {
            if (button == null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            var btnRect = button.GetComponent<RectTransform>();
            float btnH = 180f;
            if (btnRect != null && btnRect.rect.height > 1f) btnH = btnRect.rect.height;
            float iconSize = Mathf.Clamp(btnH * 0.56f, 64f, 132f);

            bool createdRoot = false;

            if (root == null)
            {
                var existing = button.transform.Find("WinRewardLayout") as RectTransform;
                if (existing != null) root = existing;
            }

            if (root != null)
            {
                if (adIcon == null)
                {
                    var t = root.Find("AdIcon");
                    if (t != null) adIcon = t.GetComponent<Image>();
                }
                if (amountText == null)
                {
                    var t = root.Find("Amount");
                    if (t != null) amountText = t.GetComponent<TMP_Text>();
                }
                if (coinIcon == null)
                {
                    var t = root.Find("CoinIcon");
                    if (t != null) coinIcon = t.GetComponent<Image>();
                }
            }

            if (root == null)
            {
                var rootGO = new GameObject("WinRewardLayout");
                rootGO.transform.SetParent(button.transform, false);
                root = rootGO.AddComponent<RectTransform>();
                root.anchorMin = new Vector2(0.5f, 0.5f);
                root.anchorMax = new Vector2(0.5f, 0.5f);
                root.pivot = new Vector2(0.5f, 0.5f);
                root.anchoredPosition = Vector2.zero;
                root.sizeDelta = Vector2.zero;
                rootGO.SetActive(false);
                createdRoot = true;

                var layout = rootGO.AddComponent<HorizontalLayoutGroup>();
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.spacing = 18f;
                layout.childControlWidth = false;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;

                var fitter = rootGO.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Ad icon (optional)
                var adGO = new GameObject("AdIcon");
                adGO.transform.SetParent(rootGO.transform, false);
                adIcon = adGO.AddComponent<Image>();
                adIcon.raycastTarget = false;
                adIcon.preserveAspect = true;
                var adRect = adGO.GetComponent<RectTransform>();
                adRect.sizeDelta = new Vector2(iconSize, iconSize);

                // Amount
                var amountGO = new GameObject("Amount");
                amountGO.transform.SetParent(rootGO.transform, false);
                var tmp = amountGO.AddComponent<TextMeshProUGUI>();
                tmp.raycastTarget = false;
                var fallbackFont =
                    (_primaryLabel != null && _primaryLabel.font != null) ? _primaryLabel.font :
                    (_resultText != null && _resultText.font != null) ? _resultText.font :
                    TMP_Settings.defaultFontAsset;
                if (fallbackFont != null) tmp.font = fallbackFont;
                if (tmp.fontSharedMaterial == null && fallbackFont != null && fallbackFont.material != null)
                {
                    tmp.fontSharedMaterial = fallbackFont.material;
                }
                tmp.text = "0";
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TextOverflowModes.Overflow;
                tmp.color = Color.white;
                tmp.enableAutoSizing = true;
                float fontMax = Mathf.Clamp(btnH * 0.50f, 54f, 88f);
                tmp.fontSizeMax = fontMax;
                tmp.fontSizeMin = Mathf.Clamp(fontMax * 0.62f, 36f, fontMax);
                tmp.fontSize = fontMax;
                ApplyTmpOutlineUnderlay(
                    tmp,
                    outlineWidth: 0.22f,
                    outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                    underlayColor: new Color(0f, 0f, 0f, 0.35f),
                    underlayOffset: new Vector2(2f, -2f),
                    underlaySoftness: 0.30f,
                    underlayDilate: 0.03f);

                var amountRect = tmp.GetComponent<RectTransform>();
                amountRect.sizeDelta = new Vector2(0f, iconSize);
                amountRect.pivot = new Vector2(0.5f, 0.5f);
                amountRect.anchorMin = new Vector2(0.5f, 0.5f);
                amountRect.anchorMax = new Vector2(0.5f, 0.5f);
                amountRect.anchoredPosition = Vector2.zero;
                amountText = tmp;

                var amountFitter = amountGO.AddComponent<ContentSizeFitter>();
                amountFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                amountFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Coin icon
                var coinGO = new GameObject("CoinIcon");
                coinGO.transform.SetParent(rootGO.transform, false);
                coinIcon = coinGO.AddComponent<Image>();
                coinIcon.raycastTarget = false;
                coinIcon.preserveAspect = true;
                var coinRect = coinGO.GetComponent<RectTransform>();
                coinRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            if (adIcon != null)
            {
                adIcon.gameObject.SetActive(includeAdIcon);
                var s =
                    (hasKit ? LoopSortingUIKit.LoadSprite("UI_Sprites/icon_video.png", 100f, applyNineSlice: false) : null) ??
                    TryLoadBoosterPurchaseSprite("icon_video");
                adIcon.sprite = s;
                adIcon.color = s != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                adIcon.raycastTarget = false;
                adIcon.preserveAspect = true;
                if (createdRoot)
                {
                    var r = adIcon.rectTransform;
                    r.sizeDelta = new Vector2(iconSize, iconSize);
                }
            }

            if (coinIcon != null)
            {
                var s = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin") : null;
                coinIcon.sprite = s;
                coinIcon.color = s != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                coinIcon.raycastTarget = false;
                coinIcon.preserveAspect = true;
                if (createdRoot)
                {
                    var r = coinIcon.rectTransform;
                    r.sizeDelta = new Vector2(iconSize, iconSize);
                }
            }

            if (amountText != null)
            {
                if (amountText.font == null)
                {
                    var fallbackFont =
                        (_primaryLabel != null && _primaryLabel.font != null) ? _primaryLabel.font :
                        (_resultText != null && _resultText.font != null) ? _resultText.font :
                        TMP_Settings.defaultFontAsset;
                    if (fallbackFont != null) amountText.font = fallbackFont;
                }
                if (amountText.fontSharedMaterial == null)
                {
                    var font = amountText.font != null ? amountText.font : TMP_Settings.defaultFontAsset;
                    if (font != null && font.material != null)
                    {
                        amountText.fontSharedMaterial = font.material;
                    }
                }

                // Keep prefab-authored typography when the layout exists in the prefab.
                if (createdRoot)
                {
                    float fontMax = Mathf.Clamp(btnH * 0.50f, 54f, 88f);
                    amountText.enableAutoSizing = true;
                    amountText.fontSizeMax = fontMax;
                    amountText.fontSizeMin = Mathf.Clamp(fontMax * 0.62f, 36f, fontMax);
                }
            }
        }

        private void ConfigureResultLoseReviveLayout()
        {
            RestoreResultButtonsLayoutBase();

            if (_resultCloseButton != null) _resultCloseButton.gameObject.SetActive(true);

            if (_resultWinRewardRootPrimary != null) _resultWinRewardRootPrimary.gameObject.SetActive(false);
            if (_resultWinRewardRootSecondary != null) _resultWinRewardRootSecondary.gameObject.SetActive(false);

            if (_primaryLabel != null)
            {
                _primaryLabel.text = $"REVIVE {LoseReviveCoinsCost}";
                _primaryLabel.gameObject.SetActive(true);
            }
            if (_secondaryLabel != null)
            {
                _secondaryLabel.text = "REVIVE";
                _secondaryLabel.gameObject.SetActive(true);
            }

            bool hasKit = LoopSortingUIKit.IsAvailable();
            if (hasKit)
            {
                if (_resultPrimaryIcon != null)
                {
                    _resultPrimaryIcon.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin");
                    _resultPrimaryIcon.color = Color.white;
                    _resultPrimaryIcon.preserveAspect = true;
                    _resultPrimaryIcon.gameObject.SetActive(_resultPrimaryIcon.sprite != null);
                }
                if (_resultSecondaryIcon != null)
                {
                    var video =
                        LoopSortingUIKit.LoadSprite("UI_Sprites/icon_video.png", 100f, applyNineSlice: false) ??
                        TryLoadBoosterPurchaseSprite("icon_video");
                    _resultSecondaryIcon.sprite = video != null ? video : LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin");
                    _resultSecondaryIcon.color = Color.white;
                    _resultSecondaryIcon.preserveAspect = true;
                    _resultSecondaryIcon.gameObject.SetActive(_resultSecondaryIcon.sprite != null);
                }
            }
            else
            {
                if (_resultPrimaryIcon != null) _resultPrimaryIcon.gameObject.SetActive(false);
                if (_resultSecondaryIcon != null) _resultSecondaryIcon.gameObject.SetActive(false);
            }

            if (_primaryButton != null)
            {
                _primaryButton.interactable = _progress.Coins >= LoseReviveCoinsCost;
            }
        }

        private void EnsureResultCloseButton()
        {
            if (_resultPanel == null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            if (_resultCloseButton == null)
            {
                foreach (var b in _resultPanel.GetComponentsInChildren<Button>(true))
                {
                    if (b != null && b.name == "CloseButton")
                    {
                        _resultCloseButton = b;
                        break;
                    }
                }
            }

            if (_resultCloseButton == null)
            {
                var parent = _resultPanel.transform.Find("Panel") ?? _resultPanel.transform;
                _resultCloseButton = CreateIconButton(
                    parent: parent,
                    name: "CloseButton",
                    anchor: new Vector2(1f, 1f),
                    anchoredPos: ModalCloseInset,
                    size: new Vector2(128f, 128f),
                    normal: hasKit ? "ui.button.close_red.normal" : null,
                    pressed: hasKit ? "ui.button.close_red.pressed" : null,
                    disabled: hasKit ? "ui.button.close_red.disabled" : null,
                    icon: hasKit ? "ui.icon.close" : null);
                var rect = _resultCloseButton.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.pivot = new Vector2(1f, 1f);
                    rect.anchoredPosition = ModalCloseInset;
                }
            }

            if (_resultCloseButton != null)
            {
                _resultCloseImage = _resultCloseButton.GetComponent<Image>();
                if (_resultCloseImage != null)
                {
                    ApplyUIKitButtonSprites(
                        _resultCloseButton,
                        _resultCloseImage,
                        normal: hasKit ? "ui.button.close_red.normal" : null,
                        pressed: hasKit ? "ui.button.close_red.pressed" : null,
                        disabled: hasKit ? "ui.button.close_red.disabled" : null);

                    if (hasKit)
                    {
                        var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                        if (iconSprite != null)
                        {
                            var iconImg = EnsureOverlayImage(_resultCloseImage.transform, "Icon", iconSprite);
                            if (iconImg != null)
                            {
                                iconImg.raycastTarget = false;
                                iconImg.preserveAspect = true;
                                var r = iconImg.rectTransform;
                                float side = Mathf.Min(_resultCloseImage.rectTransform.rect.width, _resultCloseImage.rectTransform.rect.height) * 0.62f;
                                if (side <= 1f) side = 80f;
                                r.anchorMin = new Vector2(0.5f, 0.5f);
                                r.anchorMax = new Vector2(0.5f, 0.5f);
                                r.pivot = new Vector2(0.5f, 0.5f);
                                r.anchoredPosition = Vector2.zero;
                                r.sizeDelta = new Vector2(side, side);
                            }
                        }
                    }
                }
                _resultCloseButton.onClick.RemoveAllListeners();
                _resultCloseButton.onClick.AddListener(OnResultCloseClicked);
                _resultCloseButton.gameObject.SetActive(false);
            }
        }

        private void OnResultCloseClicked()
        {
            PlaySfx(SfxId.UiCancel);
            HideUiPanelImmediate(_resultPanel);
            _resultPanelMode = ResultPanelMode.None;
            ReturnToMainMenuFromResultPanel();
        }

        private void ReturnToMainMenuFromResultPanel()
        {
            // Keep the current level as pending selection so "Play" resumes where the player left off.
            if (_flow != null && _flow.levels != null && _flow.levels.Count > 0)
            {
                _pendingFlow = _flow;
                _pendingFlowIndex = Mathf.Clamp(_flowIndex, 0, Mathf.Max(0, _flow.levels.Count - 1));
                _pendingLevel = null;
            }
            else
            {
                _pendingLevel = _currentLayout;
                _pendingFlow = null;
                _pendingFlowIndex = 0;
            }

            EnsureStateMachine();
            _stateMachine.EnterMenu();
            RequestSave(SaveDelayStrongSeconds);
        }

        private void AdvanceAfterWinResult()
        {
            if (_flow != null && _flow.levels != null && _flow.levels.Count > 0)
            {
                int next = _flowIndex + 1;
                if (next < _flow.levels.Count)
                {
                    PlaySfx(SfxId.LevelNext);
                    _flowIndex = next;
                    _progress.SavedFlowIndex = _flowIndex;
                    _progress.SavedHighestUnlockedFlowIndex = Mathf.Max(_progress.SavedHighestUnlockedFlowIndex, _flowIndex);
                    RequestSave(SaveDelayStrongSeconds);
                    _gameOver = false;
                    Build(_flow, _flowIndex);
                    return;
                }
            }

            PlaySfx(SfxId.LevelRetry);
            RestartCurrent();
        }

        private void BeginReviveFromResultPanel(bool useAd)
        {
            if (_game == null) return;

            _resultPanelMode = ResultPanelMode.None;
            HideUiPanelImmediate(_resultPanel);
            if (_resultCloseButton != null) _resultCloseButton.gameObject.SetActive(false);

            _gameOver = false;
            _inputLocked = false;
            StopFullBeltFastForward();

            EnsureBgm();

            // Apply one Sort (Fill) booster use as the revive benefit (no inventory consumption).
            StartCoroutine(BoosterSortSequence(consumeBooster: false));
        }

        private void GrantCoins(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (amount == 0) return;
            _progress.Coins = Mathf.Max(0, _progress.Coins + amount);
            RefreshEconomyHUD();
            RequestSave(SaveDelayStrongSeconds);
        }

        private void PlayCoinFlyToHud(Image sourceCoinIcon, int amount)
        {
            if (amount <= 0) return;
            if (_currencyFlyFx == null) return;
            if (_coinText == null) return;

            RectTransform from = sourceCoinIcon != null ? sourceCoinIcon.rectTransform : null;
            RectTransform to = null;
            Sprite sprite = null;

            var pillRoot = _coinText.transform != null ? _coinText.transform.parent as RectTransform : null;
            var iconRt = pillRoot != null ? pillRoot.Find("Icon") as RectTransform : null;
            if (iconRt != null)
            {
                to = iconRt;
                var iconImg = iconRt.GetComponent<Image>();
                if (iconImg != null && iconImg.sprite != null) sprite = iconImg.sprite;
            }
            else
            {
                to = pillRoot != null ? pillRoot : _coinText.rectTransform;
            }

            if (to == null) return;

            if (sprite == null && sourceCoinIcon != null) sprite = sourceCoinIcon.sprite;
            _currencyFlyFx.PlayCoins(from, to, sprite, amount);
        }

        private bool SpendCoins(int amount)
        {
            amount = Mathf.Max(0, amount);
            if (amount == 0) return true;
            if (_progress.Coins < amount) return false;
            _progress.Coins -= amount;
            RefreshEconomyHUD();
            RequestSave(SaveDelayStrongSeconds);
            return true;
        }


        private void EnsureResultPanel()
        {
            if (_uiCanvas == null) return;
            if (_resultPanel != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            if (TryInstantiateUiPrefab(ResultPanelPrefabResourcePath, out ResultPanelPrefabRefs prefab))
            {
                prefab.AutoAssign();

                _resultPanel = prefab.gameObject;
                _resultText = prefab.resultText;
                _primaryButton = prefab.primaryButton;
                _primaryLabel = prefab.primaryLabel;
                _resultPrimaryIcon = prefab.primaryIcon;
                _secondaryButton = prefab.secondaryButton;
                _secondaryLabel = prefab.secondaryLabel;
                _resultSecondaryIcon = prefab.secondaryIcon;

                if (_primaryButton != null)
                {
                    _primaryButton.onClick.RemoveAllListeners();
                    _primaryButton.onClick.AddListener(OnPrimaryClicked);
                }
                if (_secondaryButton != null)
                {
                    _secondaryButton.onClick.RemoveAllListeners();
                    _secondaryButton.onClick.AddListener(OnSecondaryClicked);
                }

                RebindResultPanelPrefabSprites(hasKit);
                _resultPanel.SetActive(false);
                return;
            }

            var panelGO = new GameObject("ResultPanel");
            panelGO.transform.SetParent(_uiCanvas.transform, false);
            _resultPanel = panelGO;

            var dim = panelGO.AddComponent<Image>();
            dim.raycastTarget = true;
            // Use a solid full-screen dim (no sprite) for consistent readability across themes.
            dim.sprite = null;
            dim.color = new Color(0f, 0f, 0f, 0.55f);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var boxGO = new GameObject("Panel");
            boxGO.transform.SetParent(panelGO.transform, false);
            var boxRect = boxGO.AddComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = new Vector2(0f, 120f);
            boxRect.sizeDelta = new Vector2(900f, 760f);

            var boxImg = boxGO.AddComponent<Image>();
            boxImg.raycastTarget = false;
            Transform contentParent = boxGO.transform;
            if (hasKit)
            {
                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_result");
                ApplySplitBackground(
                    baseImage: boxImg,
                    parent: boxGO.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/panel_result_base_9slice.png",
                    decorPath: "UI_Sprites/panel_result_decor.png",
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(0.12f, 0.12f, 0.12f, 0.95f));

                contentParent = TryCreatePaddingTrimmedLayoutRoot(
                    parent: boxGO.transform,
                    panelRect: boxRect,
                    sprite: boxImg.sprite,
                    desiredVisibleSizeUnits: new Vector2(900f, 760f),
                    centerStretchFraction: 1f / 3f);
            }
            else
            {
                boxImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            }

            var bannerGO = new GameObject("Banner");
            bannerGO.transform.SetParent(contentParent, false);
            var bannerImg = bannerGO.AddComponent<Image>();
            bannerImg.raycastTarget = false;
            if (hasKit)
            {
                bannerImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info");
                bannerImg.type = bannerImg.sprite != null && bannerImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                bannerImg.color = Color.white;
            }
            var bannerRect = bannerGO.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0.5f, 1f);
            bannerRect.anchorMax = new Vector2(0.5f, 1f);
            bannerRect.pivot = new Vector2(0.5f, 1f);
            bannerRect.anchoredPosition = new Vector2(0f, -80f);
            bannerRect.sizeDelta = new Vector2(620f, 96f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(contentParent, false);
            _resultText = titleGO.AddComponent<TextMeshProUGUI>();
            _resultText.raycastTarget = false;
            _resultText.alignment = TextAlignmentOptions.Center;
            _resultText.fontSize = 62;
            _resultText.color = Color.white;
            var titleRect = _resultText.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -84f);
            titleRect.sizeDelta = new Vector2(600f, 90f);

            _primaryButton = CreateLongButton(
                parent: contentParent,
                name: "PrimaryButton",
                anchor: new Vector2(0.5f, 0.54f),
                size: new Vector2(760f, 180f),
                normal: hasKit ? "ui.button.mint_long.normal" : null,
                pressed: hasKit ? "ui.button.mint_long.pressed" : null,
                disabled: hasKit ? "ui.button.mint_long.disabled" : null,
                label: "NEXT",
                out _primaryLabel,
                reserveIconSpace: true);
            _primaryButton.onClick.AddListener(OnPrimaryClicked);

            _resultPrimaryIcon = CreateButtonIcon(_primaryButton.transform);

            _secondaryButton = CreateLongButton(
                parent: contentParent,
                name: "SecondaryButton",
                anchor: new Vector2(0.5f, 0.30f),
                size: new Vector2(760f, 180f),
                normal: hasKit ? "ui.button.orange_long.normal" : null,
                pressed: hasKit ? "ui.button.orange_long.pressed" : null,
                disabled: hasKit ? "ui.button.orange_long.disabled" : null,
                label: "RETRY",
                out _secondaryLabel,
                reserveIconSpace: true);
            _secondaryButton.onClick.AddListener(OnSecondaryClicked);

            _resultSecondaryIcon = CreateButtonIcon(_secondaryButton.transform);

            _resultPanel.SetActive(false);
        }

    }
}

