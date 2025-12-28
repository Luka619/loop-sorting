using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TextCore;
using TMPro;

namespace LoopSorting
{
    public partial class GameRuntimeController
    {
        private static TMP_SpriteAsset _resultCoinSpriteAsset;
        private Image _resultLoseTitleImage;
        private static Texture2D _resultGlassOverlayNoiseTexture;
        private static Sprite _resultGlassOverlaySprite;
        private static bool _resultGlassOverlaySpriteTried;

        private static void EnsureTmpSpriteAssetVersion(TMP_SpriteAsset asset)
        {
            if (asset == null) return;
            var field = typeof(TMP_SpriteAsset).GetField("m_Version", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) return;
            var value = field.GetValue(asset) as string;
            if (string.IsNullOrEmpty(value))
            {
                field.SetValue(asset, "1.1.0");
            }
        }

        private static TMP_SpriteAsset GetResultCoinSpriteAsset(Sprite coinSprite)
        {
            if (coinSprite == null || coinSprite.texture == null) return null;
            if (_resultCoinSpriteAsset != null && _resultCoinSpriteAsset.spriteSheet == coinSprite.texture)
            {
                return _resultCoinSpriteAsset;
            }

            var asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.hideFlags = HideFlags.DontSave;
            asset.name = "ResultCoinSpriteAsset";
            asset.hashCode = TMP_TextUtilities.GetSimpleHashCode(asset.name);
            asset.spriteSheet = coinSprite.texture;
            asset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
            asset.spriteInfoList = new List<TMP_Sprite>();

            var shader = Shader.Find("TextMeshPro/Sprite") ?? Shader.Find("Sprites/Default");
            if (shader != null)
            {
                var material = new Material(shader);
                material.mainTexture = coinSprite.texture;
                material.hideFlags = HideFlags.DontSave;
                asset.material = material;
                asset.materialHashCode = TMP_TextUtilities.GetSimpleHashCode(material.name);
            }

            var rect = coinSprite.textureRect;
            var glyphRect = new GlyphRect(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                Mathf.RoundToInt(rect.width),
                Mathf.RoundToInt(rect.height));
            var metrics = new GlyphMetrics(rect.width, rect.height, 0f, rect.height * 0.9f, rect.width);
            var glyph = new TMP_SpriteGlyph(0, metrics, glyphRect, 1f, 0, coinSprite);
            var character = new TMP_SpriteCharacter(0xE000, asset, glyph);
            character.name = "coin";

            asset.spriteGlyphTable.Clear();
            asset.spriteGlyphTable.Add(glyph);
            asset.spriteCharacterTable.Clear();
            asset.spriteCharacterTable.Add(character);
            EnsureTmpSpriteAssetVersion(asset);
            asset.UpdateLookupTables();
            if (asset.spriteCharacterTable.Count == 0)
            {
                asset.spriteGlyphTable.Clear();
                asset.spriteCharacterTable.Clear();
                asset.spriteGlyphTable.Add(glyph);
                asset.spriteCharacterTable.Add(character);
                EnsureTmpSpriteAssetVersion(asset);
                asset.UpdateLookupTables();
            }
            MaterialReferenceManager.AddSpriteAsset(asset);

            _resultCoinSpriteAsset = asset;
            return asset;
        }

        private static Sprite LoadResultGlassOverlaySprite()
        {
            if (_resultGlassOverlaySpriteTried) return _resultGlassOverlaySprite;
            _resultGlassOverlaySpriteTried = true;

            var tex = Resources.Load<Texture2D>("ResultPanel/glass_overlay_full_placeholder");
            if (tex == null)
            {
                tex = BuildResultGlassOverlayNoiseTexture();
            }

            if (tex != null)
            {
                _resultGlassOverlaySprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }

            return _resultGlassOverlaySprite;
        }

        private static Texture2D BuildResultGlassOverlayNoiseTexture()
        {
            if (_resultGlassOverlayNoiseTexture != null) return _resultGlassOverlayNoiseTexture;

            const int size = 96;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.hideFlags = HideFlags.DontSave;

            var rng = new System.Random(12341);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                float v = 0.86f + (float)rng.NextDouble() * 0.14f;
                pixels[i] = new Color(v, v, v, 1f);
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _resultGlassOverlayNoiseTexture = tex;
            return tex;
        }

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

#if UNITY_EDITOR
        public void DebugForceLose()
        {
            if (_gameOver || _endSequenceRoutine != null) return;
            BeginEndSequence(win: false, delaySeconds: LoseEndSequenceDelaySeconds);
        }
#endif

        private IEnumerator PlayEndSequenceThenShowResult(bool win, float delaySeconds)
        {
            delaySeconds = Mathf.Max(0f, delaySeconds);
            if (delaySeconds > 0f)
            {
                yield return new WaitForSeconds(delaySeconds);
            }

            if (win)
            {
                yield return StartCoroutine(PlayWinCelebration());
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
            if (_resultText != null)
            {
                if (win)
                {
                    if (LoadResultWinTitleSprite() == null)
                    {
                        _resultText.text = LocalizedText.ResultVictory;
                    }
                }
                else
                {
                    if (LoadResultLoseTitleSprite() == null)
                    {
                        _resultText.text = LocalizedText.ResultFailed;
                    }
                }
            }
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

            EnsureResultPanelLayoutRefs();
            EnsureResultWinLayout();
            EnsureResultLoseLayout();
            ApplyResultPanelLayoutForWin();
            ApplyResultButtonsLayoutForWinOverlay();

            SetResultBannerVisible(false);
            if (_resultText != null) _resultText.gameObject.SetActive(false);
            if (_resultLoseTitleImage != null) _resultLoseTitleImage.gameObject.SetActive(false);
            SetResultWinLayoutActive(true);
            SetResultLoseLayoutActive(false);
            UpdateResultWinStats();
            UpdateResultWinFeatureProgress();
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
                if (!AllowRuntimeUiAutoCreate) return;
                var rootGO = new GameObject("WinRewardLayout");
                rootGO.transform.SetParent(button.transform, false);
                MarkRuntimeUi(rootGO);
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
            EnsureResultPanelLayoutRefs();
            EnsureResultWinLayout();
            EnsureResultLoseLayout();
            RestoreResultPanelLayoutBase();
            ApplyResultPanelLayoutForLoseOverlay();
            RestoreResultButtonsLayoutBase();
            ApplyResultButtonsLayoutForLoseOverlay();
            SetResultWinLayoutActive(false);
            SetResultLoseLayoutActive(true);
            SetResultBannerVisible(false);
            EnsureResultLoseTitleImage();
            if (_resultLoseTitleImage != null && _resultLoseTitleImage.sprite == null)
            {
                var loseTitleSprite = LoadResultLoseTitleSprite();
                if (loseTitleSprite != null)
                {
                    _resultLoseTitleImage.sprite = loseTitleSprite;
                    _resultLoseTitleImage.color = Color.white;
                }
            }
            bool showLoseTitleImage = _resultLoseTitleImage != null && _resultLoseTitleImage.sprite != null;
            if (_resultLoseTitleImage != null) _resultLoseTitleImage.gameObject.SetActive(showLoseTitleImage);
            if (_resultText != null)
            {
                if (showLoseTitleImage)
                {
                    _resultText.gameObject.SetActive(false);
                }
                else
                {
                    if (string.IsNullOrEmpty(_resultText.text) || _resultText.text == LocalizedText.ResultFailed)
                    {
                        _resultText.text = "\u53ef\u60dc\uff0c\u5c31\u5dee\u4e00\u70b9";
                    }
                    _resultText.enableAutoSizing = true;
                    _resultText.fontSizeMax = 62f;
                    _resultText.fontSizeMin = 36f;
                    _resultText.gameObject.SetActive(true);
                }
            }
            if (_resultLoseCardRoot != null)
            {
                if (ShouldApplyRuntimeLayout(_resultLoseCardRoot))
                {
                    _resultLoseCardRoot.anchoredPosition = new Vector2(0f, 120f);
                    _resultLoseCardRoot.sizeDelta = new Vector2(700f, 320f);
                }
            }
            if (_resultLoseCardDesc != null) _resultLoseCardDesc.text = "\u4f7f\u7528\u6392\u5e8f\u9053\u5177\u81ea\u52a8\u6574\u7406\u4e00\u4e2a\u79ef\u6728\u76d2\u5b50";
            if (_resultLoseCardIcon != null)
            {
                Sprite sortSprite = null;
                if (LoopSortingUIKit.IsAvailable())
                {
                    sortSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.sort");
                }
                if (sortSprite == null)
                {
                    sortSprite =
                        TryLoadBoosterPurchaseSprite("icon_booster_sort") ??
                        TryLoadBoosterPurchaseSprite("icon_booster_Sort");
                }
                _resultLoseCardIcon.sprite = sortSprite;
                _resultLoseCardIcon.color = sortSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                _resultLoseCardIcon.preserveAspect = true;
                _resultLoseCardIcon.gameObject.SetActive(sortSprite != null);
                var iconRect = _resultLoseCardIcon.rectTransform;
                if (iconRect != null)
                {
                    if (ShouldApplyRuntimeLayout(iconRect))
                    {
                        iconRect.sizeDelta = new Vector2(200f, 200f);
                    }
                }
            }
            if (_resultLoseCardBg != null)
            {
                _resultLoseCardBg.sprite = null;
                _resultLoseCardBg.enabled = false;
            }

            if (_resultCloseButton != null) _resultCloseButton.gameObject.SetActive(true);

            if (_resultWinRewardRootPrimary != null) _resultWinRewardRootPrimary.gameObject.SetActive(false);
            if (_resultWinRewardRootSecondary != null) _resultWinRewardRootSecondary.gameObject.SetActive(false);

            Sprite coinSprite = null;
            if (LoopSortingUIKit.IsAvailable())
            {
                coinSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin");
            }
            TMP_SpriteAsset coinSpriteAsset = GetResultCoinSpriteAsset(coinSprite);
            bool useInlineCoin = coinSpriteAsset != null;
            bool showPrimaryIcon = !useInlineCoin && coinSprite != null;

            if (_primaryLabel != null)
            {
                if (useInlineCoin)
                {
                    _primaryLabel.spriteAsset = coinSpriteAsset;
                    _primaryLabel.text = "\u7ee7\u7eed\u73a9 <sprite index=0>" + LoseReviveCoinsCost;
                }
                else
                {
                    _primaryLabel.spriteAsset = null;
                    _primaryLabel.text = "\u7ee7\u7eed\u73a9 \u91d1\u5e01" + LoseReviveCoinsCost;
                }
                _primaryLabel.gameObject.SetActive(true);
                _primaryLabel.alignment = TextAlignmentOptions.Center;
                _primaryLabel.enableWordWrapping = false;
                var labelRect = _primaryLabel.rectTransform;
                if (labelRect != null)
                {
                    if (ShouldApplyRuntimeLayout(labelRect))
                    {
                        if (useInlineCoin)
                        {
                            labelRect.offsetMin = Vector2.zero;
                            labelRect.offsetMax = Vector2.zero;
                        }
                        else if (showPrimaryIcon)
                        {
                            labelRect.offsetMin = new Vector2(160f, 0f);
                            labelRect.offsetMax = new Vector2(-60f, 0f);
                        }
                        else
                        {
                            labelRect.offsetMin = Vector2.zero;
                            labelRect.offsetMax = Vector2.zero;
                        }
                    }
                }
            }
            if (_secondaryLabel != null)
            {
                _secondaryLabel.text = "\u514d\u8d39";
                _secondaryLabel.gameObject.SetActive(true);
                _secondaryLabel.alignment = TextAlignmentOptions.Center;
                _secondaryLabel.enableWordWrapping = false;
            }

            if (_resultPrimaryIcon != null)
            {
                _resultPrimaryIcon.sprite = coinSprite;
                _resultPrimaryIcon.color = Color.white;
                _resultPrimaryIcon.preserveAspect = true;
                _resultPrimaryIcon.gameObject.SetActive(showPrimaryIcon);
            }

            Sprite videoSprite = null;
            if (LoopSortingUIKit.IsAvailable())
            {
                videoSprite = LoopSortingUIKit.LoadSprite("UI_Sprites/icon_video.png", 100f, applyNineSlice: false);
            }
            if (videoSprite == null)
            {
                videoSprite = TryLoadBoosterPurchaseSprite("icon_video");
            }
            if (videoSprite == null && LoopSortingUIKit.IsAvailable())
            {
                videoSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin");
            }
            if (_resultSecondaryIcon != null)
            {
                _resultSecondaryIcon.sprite = videoSprite;
                _resultSecondaryIcon.color = Color.white;
                _resultSecondaryIcon.preserveAspect = true;
                _resultSecondaryIcon.gameObject.SetActive(videoSprite != null);
            }

            if (_secondaryLabel != null)
            {
                var labelRect = _secondaryLabel.rectTransform;
                if (labelRect != null)
                {
                    if (ShouldApplyRuntimeLayout(labelRect))
                    {
                        if (_resultSecondaryIcon != null && _resultSecondaryIcon.gameObject.activeSelf)
                        {
                            labelRect.offsetMin = new Vector2(160f, 0f);
                            labelRect.offsetMax = new Vector2(-60f, 0f);
                        }
                        else
                        {
                            labelRect.offsetMin = Vector2.zero;
                            labelRect.offsetMax = Vector2.zero;
                        }
                    }
                }
            }

            if (_primaryButton != null)
            {
                _primaryButton.interactable = _progress.Coins >= LoseReviveCoinsCost;
            }
        }

        private void EnsureResultPanelLayoutRefs()
        {
            if (_resultPanel == null) return;
            if (_resultPanelBoxRect != null) return;

            _resultPanelBoxRect = _resultPanel.transform.Find("Panel") as RectTransform;
            if (_resultPanelBoxRect != null)
            {
                _resultPanelBoxImage = _resultPanelBoxRect.GetComponent<Image>();
                _resultPanelLayoutRoot = _resultPanelBoxRect.Find("LayoutRoot") as RectTransform;
                if (_resultPanelLayoutRoot == null) _resultPanelLayoutRoot = _resultPanelBoxRect;
            }
        }

        private void EnsureResultDimLayer()
        {
            if (_resultPanel == null) return;
            var dim = _resultPanel.GetComponent<Image>();
            if (dim == null) return;
            dim.sprite = null;
            dim.color = new Color(0f, 0f, 0f, 0.65f);
        }

        private void EnsureResultGlassOverlay()
        {
            if (_resultPanel == null) return;

            if (_resultGlassOverlayRect == null)
            {
                var existing = _resultPanel.transform.Find("GlassOverlay") as RectTransform;
                if (existing != null)
                {
                    _resultGlassOverlayRect = existing;
                    _resultGlassOverlayImage = existing.GetComponent<Image>();
                }
            }

            if (_resultGlassOverlayRect == null)
            {
                if (!AllowRuntimeUiAutoCreate) return;
                var go = new GameObject("GlassOverlay");
                go.transform.SetParent(_resultPanel.transform, false);
                MarkRuntimeUi(go);
                _resultGlassOverlayRect = go.AddComponent<RectTransform>();
                _resultGlassOverlayRect.anchorMin = Vector2.zero;
                _resultGlassOverlayRect.anchorMax = Vector2.one;
                _resultGlassOverlayRect.offsetMin = Vector2.zero;
                _resultGlassOverlayRect.offsetMax = Vector2.zero;

                _resultGlassOverlayImage = go.AddComponent<Image>();
                _resultGlassOverlayImage.raycastTarget = false;
                _resultGlassOverlayImage.preserveAspect = false;
            }

            if (_resultGlassOverlayImage == null) return;

            var sprite = LoadResultGlassOverlaySprite();
            _resultGlassOverlayImage.sprite = sprite;
            bool isNoise = _resultGlassOverlayNoiseTexture != null && sprite != null && sprite.texture == _resultGlassOverlayNoiseTexture;
            _resultGlassOverlayImage.type = isNoise ? Image.Type.Tiled : Image.Type.Simple;
            _resultGlassOverlayImage.color = new Color(0f, 0f, 0f, 0.30f);

            if (ShouldApplyRuntimeLayout(_resultGlassOverlayRect))
            {
                _resultGlassOverlayRect.anchorMin = Vector2.zero;
                _resultGlassOverlayRect.anchorMax = Vector2.one;
                _resultGlassOverlayRect.offsetMin = Vector2.zero;
                _resultGlassOverlayRect.offsetMax = Vector2.zero;
                var panel = _resultPanel.transform.Find("Panel");
                if (panel != null)
                {
                    _resultGlassOverlayRect.SetSiblingIndex(panel.GetSiblingIndex());
                }
                else
                {
                    _resultGlassOverlayRect.SetSiblingIndex(0);
                }
            }
        }

        private void CaptureResultPanelBaseLayoutIfNeeded()
        {
            if (_resultPanelBaseLayoutCaptured) return;
            EnsureResultPanelLayoutRefs();
            if (_resultPanelBoxRect == null) return;

            _resultPanelBaseAnchorMin = _resultPanelBoxRect.anchorMin;
            _resultPanelBaseAnchorMax = _resultPanelBoxRect.anchorMax;
            _resultPanelBaseAnchoredPosition = _resultPanelBoxRect.anchoredPosition;
            _resultPanelBaseSizeDelta = _resultPanelBoxRect.sizeDelta;
            _resultPanelBasePivot = _resultPanelBoxRect.pivot;
            _resultPanelBaseColor = _resultPanelBoxImage != null ? _resultPanelBoxImage.color : Color.white;

            var decor = _resultPanelBoxRect.Find("Decor");
            _resultPanelBaseDecorActive = decor != null && decor.gameObject.activeSelf;
            _resultPanelBaseLayoutCaptured = true;
        }

        private void ApplyResultPanelLayoutForWin()
        {
            EnsureResultPanelLayoutRefs();
            CaptureResultPanelBaseLayoutIfNeeded();
            if (_resultPanelBoxRect == null) return;
            if (!ShouldApplyRuntimeLayout(_resultPanelBoxRect)) return;

            _resultPanelBoxRect.anchorMin = Vector2.zero;
            _resultPanelBoxRect.anchorMax = Vector2.one;
            _resultPanelBoxRect.pivot = new Vector2(0.5f, 0.5f);
            _resultPanelBoxRect.anchoredPosition = Vector2.zero;
            _resultPanelBoxRect.sizeDelta = Vector2.zero;

            if (_resultPanelBoxImage != null)
            {
                var c = _resultPanelBoxImage.color;
                c.a = 0f;
                _resultPanelBoxImage.color = c;
            }

            var decor = _resultPanelBoxRect.Find("Decor");
            if (decor != null) decor.gameObject.SetActive(false);
        }

        private void ApplyResultPanelLayoutForLoseOverlay()
        {
            EnsureResultPanelLayoutRefs();
            if (!_resultPanelBaseLayoutCaptured) CaptureResultPanelBaseLayoutIfNeeded();
            if (!_resultPanelBaseLayoutCaptured || _resultPanelBoxRect == null) return;
            if (!ShouldApplyRuntimeLayout(_resultPanelBoxRect)) return;

            var size = _resultPanelBaseSizeDelta;
            size.y -= 180f;
            _resultPanelBoxRect.sizeDelta = size;
            _resultPanelBoxRect.anchoredPosition = _resultPanelBaseAnchoredPosition + new Vector2(0f, 12f);
        }

        private void RestoreResultPanelLayoutBase()
        {
            EnsureResultPanelLayoutRefs();
            if (!_resultPanelBaseLayoutCaptured) CaptureResultPanelBaseLayoutIfNeeded();
            if (!_resultPanelBaseLayoutCaptured || _resultPanelBoxRect == null) return;

            _resultPanelBoxRect.anchorMin = _resultPanelBaseAnchorMin;
            _resultPanelBoxRect.anchorMax = _resultPanelBaseAnchorMax;
            _resultPanelBoxRect.pivot = _resultPanelBasePivot;
            _resultPanelBoxRect.anchoredPosition = _resultPanelBaseAnchoredPosition;
            _resultPanelBoxRect.sizeDelta = _resultPanelBaseSizeDelta;

            if (_resultPanelBoxImage != null)
            {
                _resultPanelBoxImage.color = _resultPanelBaseColor;
            }

            var decor = _resultPanelBoxRect.Find("Decor");
            if (decor != null) decor.gameObject.SetActive(_resultPanelBaseDecorActive);
        }

        private void ApplyResultButtonsLayoutForWinOverlay()
        {
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            bool applyPrimary = ShouldApplyRuntimeLayout(primaryRect);
            bool applySecondary = ShouldApplyRuntimeLayout(secondaryRect);
            if (!applyPrimary && !applySecondary) return;

            if (!UseRuntimeUiLayoutOverrides)
            {
                if (!_resultButtonsBaseLayoutCaptured) CaptureResultButtonsBaseLayoutIfNeeded();
                if (_resultButtonsBaseLayoutCaptured)
                {
                    float yDelta = Mathf.Abs(_resultPrimaryBaseAnchoredPosition.y - _resultSecondaryBaseAnchoredPosition.y);
                    float xDelta = Mathf.Abs(_resultPrimaryBaseAnchoredPosition.x - _resultSecondaryBaseAnchoredPosition.x);
                    float primaryH = Mathf.Abs(_resultPrimaryBaseSizeDelta.y);
                    float secondaryH = Mathf.Abs(_resultSecondaryBaseSizeDelta.y);
                    float yTolerance = Mathf.Max(6f, Mathf.Min(primaryH, secondaryH) * 0.25f);
                    if (xDelta >= 60f && yDelta <= yTolerance)
                    {
                        return;
                    }
                }
            }

            var panelRect = _resultPanelBoxRect != null ? _resultPanelBoxRect : _resultPanel.GetComponent<RectTransform>();
            float panelHeight = panelRect != null && panelRect.rect.height > 1f ? panelRect.rect.height : 1920f;
            float baseY = Mathf.Clamp(panelHeight * 0.12f, 140f, 240f);
            float gap = Mathf.Clamp(panelHeight * 0.12f, 170f, 280f);

            if (applyPrimary)
            {
                primaryRect.anchorMin = new Vector2(0.5f, 0f);
                primaryRect.anchorMax = new Vector2(0.5f, 0f);
                primaryRect.pivot = new Vector2(0.5f, 0.5f);
            }

            if (applySecondary)
            {
                secondaryRect.anchorMin = new Vector2(0.5f, 0f);
                secondaryRect.anchorMax = new Vector2(0.5f, 0f);
                secondaryRect.pivot = new Vector2(0.5f, 0.5f);
            }

            if (applyPrimary) primaryRect.anchoredPosition = new Vector2(0f, baseY);
            if (applySecondary) secondaryRect.anchoredPosition = new Vector2(0f, baseY + gap);
        }

        private void ApplyResultButtonsLayoutForLoseOverlay()
        {
            if (_primaryButton == null || _secondaryButton == null) return;

            var primaryRect = _primaryButton.GetComponent<RectTransform>();
            var secondaryRect = _secondaryButton.GetComponent<RectTransform>();
            if (primaryRect == null || secondaryRect == null) return;

            bool applyPrimary = ShouldApplyRuntimeLayout(primaryRect);
            bool applySecondary = ShouldApplyRuntimeLayout(secondaryRect);
            if (!applyPrimary && !applySecondary) return;

            if (!UseRuntimeUiLayoutOverrides)
            {
                if (!_resultButtonsBaseLayoutCaptured) CaptureResultButtonsBaseLayoutIfNeeded();
                if (_resultButtonsBaseLayoutCaptured)
                {
                    float yDelta = Mathf.Abs(_resultPrimaryBaseAnchoredPosition.y - _resultSecondaryBaseAnchoredPosition.y);
                    float xDelta = Mathf.Abs(_resultPrimaryBaseAnchoredPosition.x - _resultSecondaryBaseAnchoredPosition.x);
                    float primaryH = Mathf.Abs(_resultPrimaryBaseSizeDelta.y);
                    float secondaryH = Mathf.Abs(_resultSecondaryBaseSizeDelta.y);
                    float yTolerance = Mathf.Max(6f, Mathf.Min(primaryH, secondaryH) * 0.25f);
                    if (xDelta >= 60f && yDelta <= yTolerance)
                    {
                        return;
                    }
                }
            }

            var panelRect = _resultPanelBoxRect != null ? _resultPanelBoxRect : _resultPanel.GetComponent<RectTransform>();
            float panelHeight = panelRect != null && panelRect.rect.height > 1f ? panelRect.rect.height : 760f;
            float baseY = Mathf.Clamp(panelHeight * 0.22f, 140f, 230f);
            float gap = Mathf.Clamp(panelHeight * 0.18f, 120f, 200f);

            if (applyPrimary)
            {
                primaryRect.anchorMin = new Vector2(0.5f, 0f);
                primaryRect.anchorMax = new Vector2(0.5f, 0f);
                primaryRect.pivot = new Vector2(0.5f, 0.5f);
            }

            if (applySecondary)
            {
                secondaryRect.anchorMin = new Vector2(0.5f, 0f);
                secondaryRect.anchorMax = new Vector2(0.5f, 0f);
                secondaryRect.pivot = new Vector2(0.5f, 0.5f);
            }

            if (applyPrimary) primaryRect.anchoredPosition = new Vector2(0f, baseY);
            if (applySecondary) secondaryRect.anchoredPosition = new Vector2(0f, baseY + gap);
        }

        private void SetResultBannerVisible(bool visible)
        {
            EnsureResultPanelLayoutRefs();
            var banner = _resultPanelLayoutRoot != null ? _resultPanelLayoutRoot.Find("Banner") : null;
            if (banner != null) banner.gameObject.SetActive(visible);
        }

        private void SetResultWinLayoutActive(bool active)
        {
            if (_resultWinRoot != null) _resultWinRoot.gameObject.SetActive(active);
        }

        private void SetResultLoseLayoutActive(bool active)
        {
            if (_resultLoseCardRoot != null) _resultLoseCardRoot.gameObject.SetActive(active);
        }

        private void EnsureResultWinLayout()
        {
            EnsureResultPanelLayoutRefs();
            if (_resultPanelLayoutRoot == null) return;

            if (_resultWinRoot == null)
            {
                _resultWinRoot = _resultPanelLayoutRoot.Find("WinLayout") as RectTransform;
                if (_resultWinRoot == null)
                {
                    if (!AllowRuntimeUiAutoCreate) return;
                    var rootGO = new GameObject("WinLayout");
                    rootGO.transform.SetParent(_resultPanelLayoutRoot, false);
                    MarkRuntimeUi(rootGO);
                    _resultWinRoot = rootGO.AddComponent<RectTransform>();
                    _resultWinRoot.anchorMin = Vector2.zero;
                    _resultWinRoot.anchorMax = Vector2.one;
                    _resultWinRoot.offsetMin = Vector2.zero;
                    _resultWinRoot.offsetMax = Vector2.zero;
                }
            }

            bool hasKit = LoopSortingUIKit.IsAvailable();
            var uiLayout = LoopSortingUIKit.GetRuntimeLayout();

            if (showWinCoinsPill)
            {
                if (_resultWinCoinsRoot == null || _resultWinCoinsText == null)
                {
                    _resultWinCoinsRoot = _resultWinRoot.Find("CoinsPill") as RectTransform;
                    if (_resultWinCoinsRoot != null && _resultWinCoinsText == null)
                    {
                        _resultWinCoinsText = _resultWinCoinsRoot.Find("Value")?.GetComponent<TMP_Text>();
                    }
                    if (_resultWinCoinsRoot == null)
                    {
                        if (AllowRuntimeUiAutoCreate)
                        {
                            CreateCurrencyPill(
                                parent: _resultWinRoot,
                                name: "CoinsPill",
                                anchor: new Vector2(1f, 1f),
                                anchoredPos: new Vector2(-24f, -(uiLayout.coins.y + _hudTopInsetUnits)),
                                size: new Vector2(uiLayout.coins.width, uiLayout.coins.height),
                                iconKey: "ui.icon.coin",
                                showPlusButton: false,
                                out _resultWinCoinsText,
                                out _);
                            _resultWinCoinsRoot = _resultWinCoinsText != null ? _resultWinCoinsText.transform.parent as RectTransform : null;
                            if (_resultWinCoinsRoot != null) MarkRuntimeUi(_resultWinCoinsRoot.gameObject);
                        }
                    }
                }
            }
            else if (_resultWinCoinsRoot != null)
            {
                _resultWinCoinsRoot.gameObject.SetActive(false);
            }

            if (_resultWinLivesRoot == null || _resultWinLivesText == null)
            {
                _resultWinLivesRoot = _resultWinRoot.Find("LivesPill") as RectTransform;
                if (_resultWinLivesRoot != null && _resultWinLivesText == null)
                {
                    _resultWinLivesText = _resultWinLivesRoot.Find("Value")?.GetComponent<TMP_Text>();
                }
                if (_resultWinLivesRoot == null)
                {
                    if (AllowRuntimeUiAutoCreate)
                    {
                        CreateCurrencyPill(
                            parent: _resultWinRoot,
                            name: "LivesPill",
                            anchor: new Vector2(0f, 1f),
                            anchoredPos: new Vector2(uiLayout.counter.x, -(uiLayout.counter.y + _hudTopInsetUnits)),
                            size: new Vector2(uiLayout.counter.width, uiLayout.counter.height),
                            iconKey: "ui.icon.heart",
                            showPlusButton: false,
                            out _resultWinLivesText,
                            out _);
                        _resultWinLivesRoot = _resultWinLivesText != null ? _resultWinLivesText.transform.parent as RectTransform : null;
                        if (_resultWinLivesRoot != null) MarkRuntimeUi(_resultWinLivesRoot.gameObject);
                    }
                }
            }

            if (_resultWinPercentText == null)
            {
                var t = _resultWinRoot.Find("PercentText");
                if (t != null) _resultWinPercentText = t.GetComponent<TMP_Text>();
                if (_resultWinPercentText == null && AllowRuntimeUiAutoCreate)
                {
                    var percentGO = new GameObject("PercentText");
                    percentGO.transform.SetParent(_resultWinRoot, false);
                    MarkRuntimeUi(percentGO);
                    var tmp = percentGO.AddComponent<TextMeshProUGUI>();
                    tmp.raycastTarget = false;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.enableWordWrapping = false;
                    tmp.color = Color.white;
                    tmp.fontSize = 46;
                    tmp.font = _resultText != null ? _resultText.font : TMP_Settings.defaultFontAsset;
                    ApplyTmpOutlineUnderlay(
                        tmp,
                        outlineWidth: 0.22f,
                        outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                        underlayColor: new Color(0f, 0f, 0f, 0.35f),
                        underlayOffset: new Vector2(2f, -2f),
                        underlaySoftness: 0.28f,
                        underlayDilate: 0.02f);
                    var rect = tmp.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -(180f + _hudTopInsetUnits));
                    rect.sizeDelta = new Vector2(900f, 90f);
                    _resultWinPercentText = tmp;
                }
            }

            if (_resultWinTitleImage == null)
            {
                var t = _resultWinRoot.Find("WinTitle");
                if (t != null) _resultWinTitleImage = t.GetComponent<Image>();
                if (_resultWinTitleImage == null && AllowRuntimeUiAutoCreate)
                {
                    var titleGO = new GameObject("WinTitle");
                    titleGO.transform.SetParent(_resultWinRoot, false);
                    MarkRuntimeUi(titleGO);
                    var img = titleGO.AddComponent<Image>();
                    img.raycastTarget = false;
                    img.preserveAspect = true;
                    var sprite = LoadResultWinTitleSprite();
                    img.sprite = sprite;
                    img.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                    var rect = img.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, 280f);
                    _resultWinTitleImage = img;
                }
            }
            if (_resultWinFeatureRoot == null)
            {
                _resultWinFeatureRoot = _resultWinRoot.Find("FeatureProgress") as RectTransform;
                if (_resultWinFeatureRoot != null)
                {
                    _resultWinFeatureLabel = _resultWinFeatureRoot.Find("Label")?.GetComponent<TMP_Text>();
                    var bar = _resultWinFeatureRoot.Find("Bar");
                    _resultWinFeatureFill = bar != null ? bar.Find("Fill")?.GetComponent<Image>() : null;
                    _resultWinFeatureProgress = bar != null ? bar.Find("ProgressText")?.GetComponent<TMP_Text>() : null;
                    _resultWinFeatureIcon = _resultWinFeatureRoot.Find("Icon")?.GetComponent<Image>();
                }
                if (_resultWinFeatureRoot == null && AllowRuntimeUiAutoCreate)
                {
                    var rootGO = new GameObject("FeatureProgress");
                    rootGO.transform.SetParent(_resultWinRoot, false);
                    MarkRuntimeUi(rootGO);
                    var rect = rootGO.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, 40f);
                    rect.sizeDelta = new Vector2(760f, 160f);
                    _resultWinFeatureRoot = rect;

                    var labelGO = new GameObject("Label");
                    labelGO.transform.SetParent(rootGO.transform, false);
                    var label = labelGO.AddComponent<TextMeshProUGUI>();
                    label.raycastTarget = false;
                    label.alignment = TextAlignmentOptions.Left;
                    label.enableWordWrapping = false;
                    label.color = Color.white;
                    label.fontSize = 42;
                    label.font = _resultText != null ? _resultText.font : TMP_Settings.defaultFontAsset;
                    ApplyTmpOutlineUnderlay(
                        label,
                        outlineWidth: 0.20f,
                        outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                        underlayColor: new Color(0f, 0f, 0f, 0.28f),
                        underlayOffset: new Vector2(2f, -2f),
                        underlaySoftness: 0.24f,
                        underlayDilate: 0.02f);
                    var labelRect = label.GetComponent<RectTransform>();
                    labelRect.anchorMin = new Vector2(0f, 1f);
                    labelRect.anchorMax = new Vector2(0f, 1f);
                    labelRect.pivot = new Vector2(0f, 1f);
                    labelRect.anchoredPosition = new Vector2(0f, 0f);
                    labelRect.sizeDelta = new Vector2(360f, 54f);
                    _resultWinFeatureLabel = label;

                    var barGO = new GameObject("Bar");
                    barGO.transform.SetParent(rootGO.transform, false);
                    var barImg = barGO.AddComponent<Image>();
                    barImg.raycastTarget = false;
                    barImg.color = new Color(0f, 0f, 0f, 0.45f);
                    var barRect = barGO.GetComponent<RectTransform>();
                    barRect.anchorMin = new Vector2(0f, 0.5f);
                    barRect.anchorMax = new Vector2(0f, 0.5f);
                    barRect.pivot = new Vector2(0f, 0.5f);
                    barRect.anchoredPosition = new Vector2(0f, -40f);
                    barRect.sizeDelta = new Vector2(520f, 46f);

                    var fillGO = new GameObject("Fill");
                    fillGO.transform.SetParent(barGO.transform, false);
                    var fillImg = fillGO.AddComponent<Image>();
                    fillImg.raycastTarget = false;
                    fillImg.color = new Color(0.32f, 0.85f, 0.35f, 1f);
                    fillImg.type = Image.Type.Filled;
                    fillImg.fillMethod = Image.FillMethod.Horizontal;
                    fillImg.fillOrigin = 0;
                    fillImg.fillAmount = 0f;
                    var fillRect = fillGO.GetComponent<RectTransform>();
                    fillRect.anchorMin = Vector2.zero;
                    fillRect.anchorMax = Vector2.one;
                    fillRect.offsetMin = Vector2.zero;
                    fillRect.offsetMax = Vector2.zero;
                    _resultWinFeatureFill = fillImg;

                    var progressGO = new GameObject("ProgressText");
                    progressGO.transform.SetParent(barGO.transform, false);
                    var progress = progressGO.AddComponent<TextMeshProUGUI>();
                    progress.raycastTarget = false;
                    progress.alignment = TextAlignmentOptions.Center;
                    progress.enableWordWrapping = false;
                    progress.color = Color.white;
                    progress.fontSize = 34;
                    progress.font = _resultText != null ? _resultText.font : TMP_Settings.defaultFontAsset;
                    ApplyTmpOutlineUnderlay(
                        progress,
                        outlineWidth: 0.20f,
                        outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                        underlayColor: new Color(0f, 0f, 0f, 0.30f),
                        underlayOffset: new Vector2(2f, -2f),
                        underlaySoftness: 0.24f,
                        underlayDilate: 0.02f);
                    var progressRect = progress.GetComponent<RectTransform>();
                    progressRect.anchorMin = Vector2.zero;
                    progressRect.anchorMax = Vector2.one;
                    progressRect.offsetMin = Vector2.zero;
                    progressRect.offsetMax = Vector2.zero;
                    _resultWinFeatureProgress = progress;

                    var iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(rootGO.transform, false);
                    var iconImg = iconGO.AddComponent<Image>();
                    iconImg.raycastTarget = false;
                    iconImg.preserveAspect = true;
                    iconImg.sprite = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.icon.lock") : null;
                    iconImg.color = iconImg.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                    var iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(1f, 0.5f);
                    iconRect.anchorMax = new Vector2(1f, 0.5f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.anchoredPosition = new Vector2(-24f, -40f);
                    iconRect.sizeDelta = new Vector2(120f, 120f);
                    _resultWinFeatureIcon = iconImg;
                }
            }

            ApplyResultWinPillLayout(uiLayout);
        }

        private void ApplyResultWinPillLayout(LoopSortingUIKit.RuntimeLayout layout)
        {
            if (showWinCoinsPill && _resultWinCoinsRoot != null && ShouldApplyRuntimeLayout(_resultWinCoinsRoot))
            {
                ApplyResultWinPillRect(
                    _resultWinCoinsRoot,
                    layout.coins,
                    layout.referenceWidth,
                    _hudTopInsetUnits,
                    _hudRightInsetUnits,
                    rightSide: true);
            }
            if (_resultWinLivesRoot != null && ShouldApplyRuntimeLayout(_resultWinLivesRoot))
            {
                ApplyResultWinPillRect(
                    _resultWinLivesRoot,
                    layout.counter,
                    layout.referenceWidth,
                    _hudTopInsetUnits,
                    _hudRightInsetUnits,
                    rightSide: false);
            }
        }

        private static void ApplyResultWinPillRect(
            RectTransform target,
            Rect rect,
            float referenceWidth,
            float topInset,
            float rightInset,
            bool rightSide)
        {
            if (target == null) return;
            float top = rect.y + topInset;
            target.sizeDelta = new Vector2(rect.width, rect.height);

            if (rightSide)
            {
                float right = referenceWidth - (rect.x + rect.width) + rightInset;
                target.anchorMin = new Vector2(1f, 1f);
                target.anchorMax = new Vector2(1f, 1f);
                target.pivot = new Vector2(1f, 1f);
                target.anchoredPosition = new Vector2(-right, -top);
            }
            else
            {
                target.anchorMin = new Vector2(0f, 1f);
                target.anchorMax = new Vector2(0f, 1f);
                target.pivot = new Vector2(0f, 1f);
                target.anchoredPosition = new Vector2(rect.x, -top);
            }
        }

        private void EnsureResultLoseLayout()
        {
            EnsureResultPanelLayoutRefs();
            if (_resultPanelLayoutRoot == null) return;

            if (_resultLoseCardRoot == null)
            {
                _resultLoseCardRoot = _resultPanelLayoutRoot.Find("LoseCard") as RectTransform;
                if (_resultLoseCardRoot != null)
                {
                    _resultLoseCardBg = _resultLoseCardRoot.GetComponent<Image>();
                    _resultLoseCardIcon = _resultLoseCardRoot.Find("Icon")?.GetComponent<Image>();
                    _resultLoseCardDesc = _resultLoseCardRoot.Find("Desc")?.GetComponent<TMP_Text>();
                }
                if (_resultLoseCardRoot == null)
                {
                    if (!AllowRuntimeUiAutoCreate) return;
                    var cardGO = new GameObject("LoseCard");
                    cardGO.transform.SetParent(_resultPanelLayoutRoot, false);
                    MarkRuntimeUi(cardGO);
                    var rect = cardGO.AddComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, 40f);
                    rect.sizeDelta = new Vector2(760f, 380f);
                    _resultLoseCardRoot = rect;

                    var bg = cardGO.AddComponent<Image>();
                    if (LoopSortingUIKit.IsAvailable())
                    {
                        bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                        bg.type = bg.sprite != null && bg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                        bg.color = Color.white;
                    }
                    else
                    {
                        bg.color = new Color(0.22f, 0.32f, 0.48f, 0.9f);
                    }
                    _resultLoseCardBg = bg;

                    var iconGO = new GameObject("Icon");
                    iconGO.transform.SetParent(cardGO.transform, false);
                    var iconImg = iconGO.AddComponent<Image>();
                    iconImg.raycastTarget = false;
                    iconImg.preserveAspect = true;
                    var lockSprite = LoopSortingUIKit.IsAvailable() ? LoopSortingUIKit.LoadSpriteByKey("ui.icon.lock") : null;
                    iconImg.sprite = lockSprite;
                    iconImg.color = lockSprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
                    var iconRect = iconGO.GetComponent<RectTransform>();
                    iconRect.anchorMin = new Vector2(0.5f, 0.65f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.65f);
                    iconRect.pivot = new Vector2(0.5f, 0.5f);
                    iconRect.anchoredPosition = new Vector2(0f, 0f);
                    iconRect.sizeDelta = new Vector2(160f, 160f);
                    _resultLoseCardIcon = iconImg;

                    var descGO = new GameObject("Desc");
                    descGO.transform.SetParent(cardGO.transform, false);
                    var desc = descGO.AddComponent<TextMeshProUGUI>();
                    desc.raycastTarget = false;
                    desc.alignment = TextAlignmentOptions.Center;
                    desc.enableWordWrapping = true;
                    desc.color = Color.white;
                    desc.fontSize = 32;
                    desc.font = _resultText != null ? _resultText.font : TMP_Settings.defaultFontAsset;
                    desc.text = LocalizedText.ResultRevive;
                    ApplyTmpOutlineUnderlay(
                        desc,
                        outlineWidth: 0.18f,
                        outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                        underlayColor: new Color(0f, 0f, 0f, 0.25f),
                        underlayOffset: new Vector2(2f, -2f),
                        underlaySoftness: 0.24f,
                        underlayDilate: 0.02f);
                    var descRect = desc.GetComponent<RectTransform>();
                    descRect.anchorMin = new Vector2(0.5f, 0.2f);
                    descRect.anchorMax = new Vector2(0.5f, 0.2f);
                    descRect.pivot = new Vector2(0.5f, 0.5f);
                    descRect.anchoredPosition = new Vector2(0f, 0f);
                    descRect.sizeDelta = new Vector2(640f, 100f);
                    _resultLoseCardDesc = desc;
                }
            }
        }

        private void EnsureResultLoseTitleImage()
        {
            EnsureResultPanelLayoutRefs();
            if (_resultPanelLayoutRoot == null) return;

            if (_resultLoseTitleImage == null)
            {
                var existing = _resultPanelLayoutRoot.Find("LoseTitle") as RectTransform;
                if (existing != null) _resultLoseTitleImage = existing.GetComponent<Image>();
            }

            if (_resultLoseTitleImage == null && AllowRuntimeUiAutoCreate)
            {
                var titleGO = new GameObject("LoseTitle");
                titleGO.transform.SetParent(_resultPanelLayoutRoot, false);
                MarkRuntimeUi(titleGO);
                var img = titleGO.AddComponent<Image>();
                img.raycastTarget = false;
                img.preserveAspect = true;
                var rect = img.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -84f);
                _resultLoseTitleImage = img;
            }
        }

        private void UpdateResultWinStats()
        {
            if (showWinCoinsPill && _resultWinCoinsText != null) _resultWinCoinsText.text = FormatCurrencyValue(_progress.Coins);
            if (_resultWinLivesText != null) _resultWinLivesText.text = _progress.Lives.ToString();

            if (_resultWinCoinsRoot != null)
            {
                _resultWinCoinsRoot.gameObject.SetActive(showWinCoinsPill);
            }
            if (_resultWinLivesRoot != null)
            {
                _resultWinLivesRoot.gameObject.SetActive(livesHudEnabled);
            }

            if (_resultWinPercentText != null)
            {
                float percent = Mathf.Lerp(1.9f, 99f, (float)_rng.NextDouble());
                percent = Mathf.Clamp(Mathf.Round(percent * 100f) / 100f, 1.9f, 99f);
                string percentText = percent.ToString("0.00", CultureInfo.InvariantCulture);
                _resultWinPercentText.text = $"击败了<color=#FFD24F>{percentText}%</color>的玩家";
            }
        }

        private void UpdateResultWinFeatureProgress()
        {
            if (_resultWinFeatureRoot == null) return;

            if (!TryComputeResultWinFeatureProgress(out int current, out int total))
            {
                _resultWinFeatureRoot.gameObject.SetActive(false);
                return;
            }

            _resultWinFeatureRoot.gameObject.SetActive(true);
            if (_resultWinFeatureLabel != null) _resultWinFeatureLabel.text = "新机制";
            if (_resultWinFeatureProgress != null) _resultWinFeatureProgress.text = $"{current}/{total}";
            if (_resultWinFeatureFill != null)
            {
                _resultWinFeatureFill.fillAmount = total > 0 ? Mathf.Clamp01((float)current / total) : 0f;
            }
            if (_resultWinFeatureIcon != null)
            {
                _resultWinFeatureIcon.gameObject.SetActive(_resultWinFeatureIcon.sprite != null);
            }
        }

        private bool TryComputeResultWinFeatureProgress(out int current, out int total)
        {
            current = 0;
            total = 0;
            if (_flow == null || _flow.levels == null || _flow.levels.Count == 0) return false;
            if (resultNewMechanicLevelIndices == null || resultNewMechanicLevelIndices.Count == 0) return false;

            int currentIndex = _flowIndex;
            int prevIndex = -1;
            int nextIndex = -1;

            var sorted = resultNewMechanicLevelIndices
                .Where(i => i >= 0)
                .Distinct()
                .OrderBy(i => i)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
            {
                int idx = sorted[i];
                if (idx <= currentIndex) prevIndex = idx;
                if (idx > currentIndex)
                {
                    nextIndex = idx;
                    break;
                }
            }

            if (nextIndex < 0) return false;
            total = nextIndex - prevIndex;
            if (total <= 0) return false;
            current = Mathf.Clamp(currentIndex - prevIndex, 0, total);
            return true;
        }

        private static Sprite _resultWinTitleSprite;
        private static bool _resultWinTitleSpriteTried;
        private static Sprite _resultLoseTitleSprite;
        private static bool _resultLoseTitleSpriteTried;

        private static Sprite LoadResultWinTitleSprite()
        {
            if (_resultWinTitleSpriteTried) return _resultWinTitleSprite;
            _resultWinTitleSpriteTried = true;
            var tex = Resources.Load<Texture2D>("ResultPanel/title_level_completed_placeholder");
            if (tex != null)
            {
                _resultWinTitleSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
            return _resultWinTitleSprite;
        }

        private static Sprite LoadResultLoseTitleSprite()
        {
            if (_resultLoseTitleSpriteTried) return _resultLoseTitleSprite;
            _resultLoseTitleSpriteTried = true;
            var tex = Resources.Load<Texture2D>("ResultPanel/title_level_failed_placeholder");
            if (tex != null)
            {
                _resultLoseTitleSprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
            }
            return _resultLoseTitleSprite;
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
                if (!AllowRuntimeUiAutoCreate) return;
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
                MarkRuntimeUi(_resultCloseButton.gameObject);
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
                EnsureResultDimLayer();
                EnsureResultGlassOverlay();
                EnsureResultPanelLayoutRefs();
                EnsureResultWinLayout();
                EnsureResultLoseLayout();
                CaptureResultPanelBaseLayoutIfNeeded();
                _resultPanel.SetActive(false);
                return;
            }

            if (!AllowRuntimeUiAutoCreate) return;

            var panelGO = new GameObject("ResultPanel");
            panelGO.transform.SetParent(_uiCanvas.transform, false);
            MarkRuntimeUi(panelGO);
            _resultPanel = panelGO;

            var dim = panelGO.AddComponent<Image>();
            dim.raycastTarget = true;
            // Use a solid full-screen dim (no sprite) for consistent readability across themes.
            dim.sprite = null;
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            var rect = panelGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            EnsureResultGlassOverlay();

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
                label: LocalizedText.ResultNext,
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
                label: LocalizedText.SettingsRetry,
                out _secondaryLabel,
                reserveIconSpace: true);
            _secondaryButton.onClick.AddListener(OnSecondaryClicked);

            _resultSecondaryIcon = CreateButtonIcon(_secondaryButton.transform);

            EnsureResultPanelLayoutRefs();
            EnsureResultWinLayout();
            EnsureResultLoseLayout();
            CaptureResultPanelBaseLayoutIfNeeded();
            _resultPanel.SetActive(false);
        }

    }
}



