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
        private void EnsureBoosterPurchaseUI()
        {
            if (_uiCanvas == null) return;
            if (_boosterPurchasePanel != null && _boosterPurchaseCloseButton != null && _boosterPurchaseCoinsButton != null && _boosterPurchaseAdButton != null)
            {
                return;
            }

            bool hasKit = LoopSortingUIKit.IsAvailable();

            if (TryInstantiateUiPrefab(BoosterPurchasePanelPrefabResourcePath, out BoosterPurchasePanelPrefabRefs prefab))
            {
                prefab.AutoAssign();

                if (prefab.titleText != null) prefab.titleText.text = LocalizedText.BoosterTitle;
                if (prefab.subtitleText != null) prefab.subtitleText.text = string.Empty;
                if (prefab.adLabel != null) prefab.adLabel.text = LocalizedText.BoosterFree;

                _boosterPurchasePanel = prefab.gameObject;
                _boosterPurchasePopupRect = prefab.popupRect;
                _boosterPurchaseHeaderRect = prefab.headerRect;
                _boosterPurchaseIconRect = prefab.iconRect;
                _boosterPurchaseCloseRect = prefab.closeRect;
                _boosterPurchaseSubtitleRect = prefab.subtitleRect;
                _boosterPurchaseCoinsRect = prefab.coinsRect;
                _boosterPurchaseAdRect = prefab.adRect;

                _boosterPurchaseCloseButton = prefab.closeButton;
                _boosterPurchaseCloseImage = prefab.closeImage;
                _boosterPurchaseCoinsButton = prefab.coinsButton;
                _boosterPurchaseCoinsImage = prefab.coinsImage;
                _boosterPurchaseCoinsLabel = prefab.coinsLabel;
                _boosterPurchaseCoinsPriceCover = prefab.coinsPriceCover;
                _boosterPurchaseAdButton = prefab.adButton;
                _boosterPurchaseAdImage = prefab.adImage;
                _boosterPurchaseAdLabel = prefab.adLabel;

                _boosterPurchaseTitleText = prefab.titleText;
                _boosterPurchaseSubtitleText = prefab.subtitleText;
                _boosterPurchaseBackground = prefab.background;
                _boosterPurchaseHeader = prefab.header;
                _boosterPurchaseIcon = prefab.icon;
                _boosterPurchaseSubtitleBg = prefab.subtitleBg;

                if (_boosterPurchaseSubtitleBg != null) _boosterPurchaseSubtitleBg.gameObject.SetActive(false);

                if (_boosterPurchaseCloseButton != null)
                {
                    _boosterPurchaseCloseButton.onClick.RemoveAllListeners();
                    _boosterPurchaseCloseButton.onClick.AddListener(() => CloseBoosterPurchase());
                }
                if (_boosterPurchaseCoinsButton != null)
                {
                    _boosterPurchaseCoinsButton.onClick.RemoveAllListeners();
                    _boosterPurchaseCoinsButton.onClick.AddListener(() => PurchaseBoosterWithCoins());
                }
                if (_boosterPurchaseAdButton != null)
                {
                    _boosterPurchaseAdButton.onClick.RemoveAllListeners();
                    _boosterPurchaseAdButton.onClick.AddListener(() => PurchaseBoosterWithAd());
                }

                ApplyButtonPressScale(_boosterPurchaseCloseButton, pressedScale: 0.92f);
                ApplyButtonPressScale(_boosterPurchaseCoinsButton, pressedScale: 0.96f);
                ApplyButtonPressScale(_boosterPurchaseAdButton, pressedScale: 0.96f);

                RebindBoosterPurchasePanelPrefabSprites(prefab, hasKit);
                CaptureBoosterPurchaseBasePose();
                _boosterPurchasePanel.SetActive(false);
                return;
            }

            if (_boosterPurchasePanel != null)
            {
                Destroy(_boosterPurchasePanel);
            }

            _boosterPurchasePanel = new GameObject("BoosterPurchasePanel");
            _boosterPurchasePanel.transform.SetParent(_uiCanvas.transform, false);

            var dim = _boosterPurchasePanel.AddComponent<Image>();
            dim.raycastTarget = true;
            // Use a solid full-screen dim (no sprite) to keep the background consistent across themes.
            dim.sprite = null;
            dim.color = new Color(0f, 0f, 0f, 0.55f);

            var overlayRect = _boosterPurchasePanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var popupGO = new GameObject("Popup");
            popupGO.transform.SetParent(_boosterPurchasePanel.transform, false);
            _boosterPurchasePopupRect = popupGO.AddComponent<RectTransform>();
            _boosterPurchasePopupRect.anchorMin = new Vector2(0.5f, 0.5f);
            _boosterPurchasePopupRect.anchorMax = new Vector2(0.5f, 0.5f);
            _boosterPurchasePopupRect.pivot = new Vector2(0.5f, 0.5f);
            _boosterPurchasePopupRect.anchoredPosition = ModalPopupAnchoredPos;
            _boosterPurchasePopupRect.sizeDelta = ModalPopupSize;

            _boosterPurchaseBackground = popupGO.AddComponent<Image>();
            _boosterPurchaseBackground.raycastTarget = false;
            _boosterPurchaseBackground.color = Color.white;
            if (hasKit)
            {
                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                ApplySplitBackground(
                    baseImage: _boosterPurchaseBackground,
                    parent: popupGO.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/panel_modal_base_9slice.png",
                    decorPath: null,
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(1f, 1f, 1f, 0.92f));
            }
            else
            {
                _boosterPurchaseBackground.color = new Color(1f, 1f, 1f, 0.92f);
            }

            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(popupGO.transform, false);
            _boosterPurchaseHeaderRect = headerGO.AddComponent<RectTransform>();
            _boosterPurchaseHeaderRect.anchorMin = new Vector2(0.5f, 1f);
            _boosterPurchaseHeaderRect.anchorMax = new Vector2(0.5f, 1f);
            _boosterPurchaseHeaderRect.pivot = new Vector2(0.5f, 1f);
            _boosterPurchaseHeaderRect.anchoredPosition = new Vector2(0f, -70f);
            _boosterPurchaseHeaderRect.sizeDelta = new Vector2(820f, 210f);
            _boosterPurchaseHeader = headerGO.AddComponent<Image>();
            _boosterPurchaseHeader.raycastTarget = false;
            _boosterPurchaseHeader.color = Color.white;
            if (hasKit)
            {
                var headerBg = LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal");
                if (headerBg != null)
                {
                    _boosterPurchaseHeader.sprite = headerBg;
                    _boosterPurchaseHeader.type = headerBg.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                }
            }

            var titleGO = new GameObject("TitleText");
            titleGO.transform.SetParent(headerGO.transform, false);
            _boosterPurchaseTitleText = titleGO.AddComponent<TextMeshProUGUI>();
            _boosterPurchaseTitleText.raycastTarget = false;
            _boosterPurchaseTitleText.text = LocalizedText.BoosterTitle;
            _boosterPurchaseTitleText.alignment = TextAlignmentOptions.Center;
            _boosterPurchaseTitleText.fontSize = 92;
            _boosterPurchaseTitleText.color = new Color(1f, 1f, 1f, 0.98f);
            _boosterPurchaseTitleText.outlineWidth = 0.22f;
            _boosterPurchaseTitleText.outlineColor = new Color(0.12f, 0.06f, 0.02f, 0.88f);
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            var closeGO = new GameObject("CloseButton");
            closeGO.transform.SetParent(popupGO.transform, false);
            _boosterPurchaseCloseRect = closeGO.AddComponent<RectTransform>();
            _boosterPurchaseCloseRect.anchorMin = new Vector2(1f, 1f);
            _boosterPurchaseCloseRect.anchorMax = new Vector2(1f, 1f);
            _boosterPurchaseCloseRect.pivot = new Vector2(1f, 1f);
            _boosterPurchaseCloseRect.anchoredPosition = new Vector2(-36f, -36f);
            _boosterPurchaseCloseRect.sizeDelta = new Vector2(120f, 120f);
            _boosterPurchaseCloseImage = closeGO.AddComponent<Image>();
            _boosterPurchaseCloseImage.raycastTarget = true;
            _boosterPurchaseCloseImage.color = Color.white;
            _boosterPurchaseCloseButton = closeGO.AddComponent<Button>();
            _boosterPurchaseCloseButton.onClick.AddListener(() => CloseBoosterPurchase());

            var closeSprite = TryLoadBoosterPurchaseSprite("btn_close");
            if (closeSprite != null)
            {
                _boosterPurchaseCloseImage.sprite = closeSprite;
                _boosterPurchaseCloseImage.type = closeSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            }
            else if (hasKit)
            {
                _boosterPurchaseCloseImage.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal");
                _boosterPurchaseCloseImage.type = Image.Type.Simple;
            }
            else
            {
                _boosterPurchaseCloseImage.color = new Color(0.9f, 0.25f, 0.25f, 0.95f);
            }

            var iconGO = new GameObject("BoosterIcon");
            iconGO.transform.SetParent(popupGO.transform, false);
            _boosterPurchaseIconRect = iconGO.AddComponent<RectTransform>();
            _boosterPurchaseIconRect.anchorMin = new Vector2(0.5f, 0.5f);
            _boosterPurchaseIconRect.anchorMax = new Vector2(0.5f, 0.5f);
            _boosterPurchaseIconRect.pivot = new Vector2(0.5f, 0.5f);
            _boosterPurchaseIconRect.anchoredPosition = new Vector2(0f, 150f);
            _boosterPurchaseIconRect.sizeDelta = new Vector2(460f, 460f);
            _boosterPurchaseIcon = iconGO.AddComponent<Image>();
            _boosterPurchaseIcon.raycastTarget = false;
            _boosterPurchaseIcon.color = Color.white;
            _boosterPurchaseIcon.preserveAspect = true;

            var subtitleGO = new GameObject("Subtitle");
            subtitleGO.transform.SetParent(popupGO.transform, false);
            _boosterPurchaseSubtitleRect = subtitleGO.AddComponent<RectTransform>();
            _boosterPurchaseSubtitleRect.anchorMin = new Vector2(0.5f, 0.5f);
            _boosterPurchaseSubtitleRect.anchorMax = new Vector2(0.5f, 0.5f);
            _boosterPurchaseSubtitleRect.pivot = new Vector2(0.5f, 0.5f);
            _boosterPurchaseSubtitleRect.anchoredPosition = new Vector2(0f, -240f);
            _boosterPurchaseSubtitleRect.sizeDelta = new Vector2(760f, 120f);

            var subtitleBgGO = new GameObject("BG");
            subtitleBgGO.transform.SetParent(subtitleGO.transform, false);
            var subtitleBgRect = subtitleBgGO.AddComponent<RectTransform>();
            subtitleBgRect.anchorMin = Vector2.zero;
            subtitleBgRect.anchorMax = Vector2.one;
            subtitleBgRect.offsetMin = Vector2.zero;
            subtitleBgRect.offsetMax = Vector2.zero;
            _boosterPurchaseSubtitleBg = subtitleBgGO.AddComponent<Image>();
            _boosterPurchaseSubtitleBg.raycastTarget = false;
            _boosterPurchaseSubtitleBg.color = Color.white;
            if (hasKit)
            {
                var pill = LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info");
                if (pill != null)
                {
                    _boosterPurchaseSubtitleBg.sprite = pill;
                    _boosterPurchaseSubtitleBg.type = pill.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                }
                else
                {
                    _boosterPurchaseSubtitleBg.color = new Color(1f, 1f, 1f, 0.55f);
                }
            }
            else
            {
                _boosterPurchaseSubtitleBg.color = new Color(1f, 1f, 1f, 0.55f);
            }
            _boosterPurchaseSubtitleBg.gameObject.SetActive(false);

            var subtitleTextGO = new GameObject("Text");
            subtitleTextGO.transform.SetParent(subtitleGO.transform, false);
            var subtitleTextRect = subtitleTextGO.AddComponent<RectTransform>();
            subtitleTextRect.anchorMin = Vector2.zero;
            subtitleTextRect.anchorMax = Vector2.one;
            subtitleTextRect.offsetMin = new Vector2(20f, 0f);
            subtitleTextRect.offsetMax = new Vector2(-20f, 0f);
            _boosterPurchaseSubtitleText = subtitleTextGO.AddComponent<TextMeshProUGUI>();
            _boosterPurchaseSubtitleText.raycastTarget = false;
            _boosterPurchaseSubtitleText.text = LocalizedText.BoosterPurchaseTitle;
            _boosterPurchaseSubtitleText.alignment = TextAlignmentOptions.Center;
            _boosterPurchaseSubtitleText.fontSize = 60;
            _boosterPurchaseSubtitleText.color = new Color(0.18f, 0.14f, 0.10f, 1f);

            _boosterPurchaseCoinsButton = CreateBoosterPurchaseActionButton(
                parent: popupGO.transform,
                name: "BuyWithCoins",
                anchoredPos: new Vector2(-210f, -480f),
                size: new Vector2(380f, 220f),
                fallbackSpriteKey: hasKit ? "ui.button.price_green.normal" : null,
                labelText: "0",
                out _boosterPurchaseCoinsLabel);
            _boosterPurchaseCoinsButton.onClick.AddListener(() => PurchaseBoosterWithCoins());
            _boosterPurchaseCoinsRect = _boosterPurchaseCoinsButton.GetComponent<RectTransform>();
            _boosterPurchaseCoinsImage = _boosterPurchaseCoinsButton.GetComponent<Image>();
            if (_boosterPurchaseCoinsLabel != null)
            {
                _boosterPurchaseCoinsLabel.outlineWidth = 0.25f;
                _boosterPurchaseCoinsLabel.outlineColor = new Color(0f, 0f, 0f, 0.65f);
                _boosterPurchaseCoinsLabel.enableAutoSizing = true;
                _boosterPurchaseCoinsLabel.fontSizeMax = 78f;
                _boosterPurchaseCoinsLabel.fontSizeMin = 40f;
                var labelRect = _boosterPurchaseCoinsLabel.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = Vector2.zero;
                    labelRect.anchorMax = Vector2.one;
                    labelRect.offsetMin = new Vector2(150f, 0f);
                    labelRect.offsetMax = new Vector2(-24f, 0f);
                }
            }
            _boosterPurchaseCoinsPriceCover = EnsureBoosterPurchaseCoinsPriceCover(_boosterPurchaseCoinsButton.transform);
            if (_boosterPurchaseCoinsPriceCover != null) _boosterPurchaseCoinsPriceCover.gameObject.SetActive(false);

            _boosterPurchaseAdButton = CreateBoosterPurchaseActionButton(
                parent: popupGO.transform,
                name: "BuyWithAd",
                anchoredPos: new Vector2(210f, -480f),
                size: new Vector2(380f, 220f),
                fallbackSpriteKey: hasKit ? "ui.button.mint_long.normal" : null,
                labelText: "FREE",
                out _boosterPurchaseAdLabel);
            _boosterPurchaseAdButton.onClick.AddListener(() => PurchaseBoosterWithAd());
            _boosterPurchaseAdRect = _boosterPurchaseAdButton.GetComponent<RectTransform>();
            _boosterPurchaseAdImage = _boosterPurchaseAdButton.GetComponent<Image>();

            ApplyButtonPressScale(_boosterPurchaseCloseButton, pressedScale: 0.92f);
            ApplyButtonPressScale(_boosterPurchaseCoinsButton, pressedScale: 0.96f);
            ApplyButtonPressScale(_boosterPurchaseAdButton, pressedScale: 0.96f);

            CaptureBoosterPurchaseBasePose();
            _boosterPurchasePanel.SetActive(false);
        }

        private static Image EnsureBoosterPurchaseCoinsPriceCover(Transform coinButtonTransform)
        {
            if (coinButtonTransform == null) return null;

            var existing = coinButtonTransform.Find("PriceCover");
            if (existing != null)
            {
                return existing.GetComponent<Image>();
            }

            var go = new GameObject("PriceCover");
            go.transform.SetParent(coinButtonTransform, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.38f, 0.18f);
            rect.anchorMax = new Vector2(0.95f, 0.82f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            // Approximate the green inside the coin button to cover the baked-in "80".
            img.color = new Color(0.45f, 0.82f, 0.32f, 1f);

            // Keep behind the label.
            go.transform.SetAsFirstSibling();
            go.SetActive(false);
            return img;
        }

        private Button CreateBoosterPurchaseActionButton(
            Transform parent,
            string name,
            Vector2 anchoredPos,
            Vector2 size,
            string fallbackSpriteKey,
            string labelText,
            out TMP_Text label)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            img.raycastTarget = true;
            img.color = Color.white;

            var btn = go.AddComponent<Button>();

            Sprite authored = name == "BuyWithAd"
                ? TryLoadBoosterPurchaseSprite("btn_watch_ad_free")
                : (name == "BuyWithCoins" ? TryLoadBoosterPurchaseSprite("btn_buy_coins_80") : null);

            if (authored != null)
            {
                img.sprite = authored;
                img.type = authored.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            }
            else if (hasKit && !string.IsNullOrEmpty(fallbackSpriteKey))
            {
                var s = LoopSortingUIKit.LoadSpriteByKey(fallbackSpriteKey);
                img.sprite = s;
                img.type = s != null && s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            }
            else
            {
                img.color = new Color(0f, 0f, 0f, 0.22f);
            }

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(go.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = labelText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 72;
            tmp.color = Color.white;
            var tRect = txtGO.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            label = tmp;

            if (authored != null && name == "BuyWithAd")
            {
                tmp.gameObject.SetActive(false);
            }

            return btn;
        }

	        private void OpenBoosterPurchase(BoosterType type)
	        {
	            EnsureBoosterPurchaseUI();
	            if (_boosterPurchasePanel == null) return;

            _boosterPurchaseType = type;
            ConfigureBoosterPurchaseUI(type);

	            SettingsUi.HideImmediate();
	            if (_resultPanel != null) _resultPanel.SetActive(false);
	            HideUiPanelImmediate(_shopPanel);

	            AnimateUiPanel(_boosterPurchasePanel, true, seconds: 0.20f);
	            StartBoosterPurchaseEffects();
	            PlaySfx(SfxId.UiPopupOpen);
	        }

        private void CloseBoosterPurchase()
        {
            if (_boosterPurchasePanel == null) return;
            StopBoosterPurchaseEffects();
            AnimateUiPanel(_boosterPurchasePanel, false, seconds: 0.18f);
            PlaySfx(SfxId.UiPopupClose);
        }

        private void ConfigureBoosterPurchaseUI(BoosterType type)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();
            bool isShuffle = type == BoosterType.Shuffle;

            string title = isShuffle ? LocalizedText.BoosterShuffle : LocalizedText.BoosterSort;
            if (_boosterPurchaseTitleText != null) _boosterPurchaseTitleText.text = title;
            if (_boosterPurchaseSubtitleText != null)
            {
                string desc = isShuffle ? LocalizedText.BoosterShuffleDesc : LocalizedText.BoosterSortDesc;
                _boosterPurchaseSubtitleText.text = desc;
            }

            // Use the split UI (header/icon/buttons) for a higher-quality animated popup.
            // Keep full-popup sprites only as an optional fallback for missing assets.
            bool useFullPopup = false;
            var fullBg = (Sprite)null;

            int coinPrice = GetBoosterCoinPrice(type);
            if (_boosterPurchaseCoinsLabel != null) _boosterPurchaseCoinsLabel.text = coinPrice.ToString();

            // Prefer authored coin button art; use a cover + TMP to support dynamic prices.
            bool coinUsesAuthored = false;
            if (_boosterPurchaseCoinsImage != null)
            {
                var authoredCoin = TryLoadBoosterPurchaseSprite("btn_buy_coins_80");
                if (authoredCoin != null)
                {
                    _boosterPurchaseCoinsImage.sprite = authoredCoin;
                    _boosterPurchaseCoinsImage.type = authoredCoin.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    _boosterPurchaseCoinsImage.color = Color.white;
                    coinUsesAuthored = true;
                }
                else if (hasKit)
                {
                    var normal = LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal");
                    if (normal != null)
                    {
                        _boosterPurchaseCoinsImage.sprite = normal;
                        _boosterPurchaseCoinsImage.type = normal.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                        _boosterPurchaseCoinsImage.color = Color.white;
                    }
                }
            }

            if (_boosterPurchaseBackground != null)
            {
                if (hasKit)
                {
                    var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                    ApplySplitBackground(
                        baseImage: _boosterPurchaseBackground,
                        parent: _boosterPurchaseBackground.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/panel_modal_base_9slice.png",
                        decorPath: null,
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(1f, 1f, 1f, 0.92f));
                }
            }

            if (_boosterPurchaseHeader != null)
            {
                _boosterPurchaseHeader.gameObject.SetActive(true);
                if (_boosterPurchaseTitleText != null) _boosterPurchaseTitleText.gameObject.SetActive(true);
            }

            if (_boosterPurchaseIcon != null)
            {
                // Keep booster icons consistent across HUD + purchase popup.
                // Prefer UIKit icons (same keys as HUD); fall back to BoosterPurchase-specific icons only if needed.
                Sprite icon = null;
                if (hasKit)
                {
                    icon = LoopSortingUIKit.LoadSpriteByKey(isShuffle ? "ui.icon.shuffle" : "ui.icon.sort");
                }
                if (icon == null)
                {
                    icon = isShuffle
                        ? TryLoadBoosterPurchaseSprite("icon_booster_shuffle")
                        : (TryLoadBoosterPurchaseSprite("icon_booster_sort") ?? TryLoadBoosterPurchaseSprite("icon_booster_Sort"));
                }
                _boosterPurchaseIcon.sprite = icon;
                _boosterPurchaseIcon.color = icon != null ? Color.white : new Color(0f, 0f, 0f, 0.15f);
                _boosterPurchaseIcon.type = icon != null && icon.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                _boosterPurchaseIcon.gameObject.SetActive(true);
            }

            if (_boosterPurchaseSubtitleText != null)
            {
                _boosterPurchaseSubtitleText.gameObject.SetActive(true);
            }

            bool showCoinPriceLabel = !coinUsesAuthored || coinPrice != 80;
            if (_boosterPurchaseCoinsPriceCover != null) _boosterPurchaseCoinsPriceCover.gameObject.SetActive(coinUsesAuthored && coinPrice != 80);

            SetPurchaseButtonVisuals(useFullPopup, showCoinPriceLabel);
            if (useFullPopup)
            {
                ApplyBoosterPurchaseLayoutFromManifest(type, useFullPopup, fullBg);
            }
        }

        private void StartBoosterPurchaseEffects()
        {
            StopBoosterPurchaseEffects();
            if (_boosterPurchasePanel == null || !_boosterPurchasePanel.activeInHierarchy) return;
            ResetBoosterPurchasePose();
            _boosterPurchaseIntroRoutine = StartCoroutine(AnimateBoosterPurchaseIntro());
        }

        private void StopBoosterPurchaseEffects()
        {
            if (_boosterPurchaseIntroRoutine != null) StopCoroutine(_boosterPurchaseIntroRoutine);
            _boosterPurchaseIntroRoutine = null;
            if (_boosterPurchaseIdleRoutine != null) StopCoroutine(_boosterPurchaseIdleRoutine);
            _boosterPurchaseIdleRoutine = null;

            if (_boosterPurchaseIconRect != null)
            {
                _boosterPurchaseIconRect.localRotation = Quaternion.identity;
            }
        }

        private void ResetBoosterPurchasePose()
        {
            if (!_boosterPurchaseBasePoseCaptured)
            {
                CaptureBoosterPurchaseBasePose();
            }

            if (_boosterPurchaseHeaderRect != null)
            {
                _boosterPurchaseHeaderRect.anchoredPosition = _boosterPurchaseHeaderBasePos;
                _boosterPurchaseHeaderRect.localScale = Vector3.one;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseHeaderRect.gameObject).alpha = 1f;
            }
            if (_boosterPurchaseIconRect != null)
            {
                _boosterPurchaseIconRect.anchoredPosition = _boosterPurchaseIconBasePos;
                _boosterPurchaseIconRect.localScale = Vector3.one;
                _boosterPurchaseIconRect.localRotation = Quaternion.identity;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseIconRect.gameObject).alpha = 1f;
            }
            if (_boosterPurchaseSubtitleRect != null)
            {
                _boosterPurchaseSubtitleRect.anchoredPosition = _boosterPurchaseSubtitleBasePos;
                _boosterPurchaseSubtitleRect.localScale = Vector3.one;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseSubtitleRect.gameObject).alpha = 1f;
            }
            if (_boosterPurchaseCoinsRect != null)
            {
                _boosterPurchaseCoinsRect.anchoredPosition = _boosterPurchaseCoinsBasePos;
                _boosterPurchaseCoinsRect.localScale = Vector3.one;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseCoinsRect.gameObject).alpha = 1f;
            }
            if (_boosterPurchaseAdRect != null)
            {
                _boosterPurchaseAdRect.anchoredPosition = _boosterPurchaseAdBasePos;
                _boosterPurchaseAdRect.localScale = Vector3.one;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseAdRect.gameObject).alpha = 1f;
            }
            if (_boosterPurchaseCloseRect != null)
            {
                _boosterPurchaseCloseRect.anchoredPosition = _boosterPurchaseCloseBasePos;
                _boosterPurchaseCloseRect.localScale = Vector3.one;
                MotionUtil.EnsureCanvasGroup(_boosterPurchaseCloseRect.gameObject).alpha = 1f;
            }
        }

        private void CaptureBoosterPurchaseBasePose()
        {
            if (_boosterPurchaseHeaderRect == null ||
                _boosterPurchaseIconRect == null ||
                _boosterPurchaseSubtitleRect == null ||
                _boosterPurchaseCoinsRect == null ||
                _boosterPurchaseAdRect == null ||
                _boosterPurchaseCloseRect == null)
            {
                _boosterPurchaseBasePoseCaptured = false;
                return;
            }

            _boosterPurchaseHeaderBasePos = _boosterPurchaseHeaderRect.anchoredPosition;
            _boosterPurchaseIconBasePos = _boosterPurchaseIconRect.anchoredPosition;
            _boosterPurchaseSubtitleBasePos = _boosterPurchaseSubtitleRect.anchoredPosition;
            _boosterPurchaseCoinsBasePos = _boosterPurchaseCoinsRect.anchoredPosition;
            _boosterPurchaseAdBasePos = _boosterPurchaseAdRect.anchoredPosition;
            _boosterPurchaseCloseBasePos = _boosterPurchaseCloseRect.anchoredPosition;
            _boosterPurchaseBasePoseCaptured = true;
        }

        private IEnumerator AnimateBoosterPurchaseIntro()
        {
            if (_boosterPurchasePanel == null) yield break;
            if (_boosterPurchasePopupRect == null) yield break;

            // Wait one frame so the panel scale/alpha animation is applied first.
            yield return null;

            if (_boosterPurchasePanel == null || !_boosterPurchasePanel.activeInHierarchy) yield break;

            var header = _boosterPurchaseHeaderRect;
            var icon = _boosterPurchaseIconRect;
            var subtitle = _boosterPurchaseSubtitleRect;
            var coins = _boosterPurchaseCoinsRect;
            var ad = _boosterPurchaseAdRect;
            var close = _boosterPurchaseCloseRect;

            if (header == null || icon == null || subtitle == null || coins == null || ad == null || close == null) yield break;

            var headerCg = MotionUtil.EnsureCanvasGroup(header.gameObject);
            var iconCg = MotionUtil.EnsureCanvasGroup(icon.gameObject);
            var subtitleCg = MotionUtil.EnsureCanvasGroup(subtitle.gameObject);
            var coinsCg = MotionUtil.EnsureCanvasGroup(coins.gameObject);
            var adCg = MotionUtil.EnsureCanvasGroup(ad.gameObject);
            var closeCg = MotionUtil.EnsureCanvasGroup(close.gameObject);

            Vector2 headerPos0 = header.anchoredPosition;
            Vector2 iconPos0 = icon.anchoredPosition;
            Vector2 subtitlePos0 = subtitle.anchoredPosition;
            Vector2 coinsPos0 = coins.anchoredPosition;
            Vector2 adPos0 = ad.anchoredPosition;
            Vector2 closePos0 = close.anchoredPosition;

            header.anchoredPosition = headerPos0 + new Vector2(0f, 26f);
            header.localScale = Vector3.one * 0.92f;
            icon.anchoredPosition = iconPos0 + new Vector2(0f, -40f);
            icon.localScale = Vector3.one * 0.72f;
            subtitle.anchoredPosition = subtitlePos0 + new Vector2(0f, -18f);
            subtitle.localScale = Vector3.one * 0.98f;
            coins.anchoredPosition = coinsPos0 + new Vector2(0f, -28f);
            coins.localScale = Vector3.one * 0.96f;
            ad.anchoredPosition = adPos0 + new Vector2(0f, -28f);
            ad.localScale = Vector3.one * 0.96f;
            close.anchoredPosition = closePos0 + new Vector2(0f, 18f);
            close.localScale = Vector3.one * 0.9f;

            headerCg.alpha = 0f;
            iconCg.alpha = 0f;
            subtitleCg.alpha = 0f;
            coinsCg.alpha = 0f;
            adCg.alpha = 0f;
            closeCg.alpha = 0f;

            float seconds = 0.34f;
            float t = 0f;
            while (t < seconds)
            {
                if (_boosterPurchasePanel == null || !_boosterPurchasePanel.activeInHierarchy) yield break;
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / Mathf.Max(0.0001f, seconds));

                float e0 = MotionUtil.EaseOutCubic(u);
                float eBack = MotionUtil.EaseOutBack(u);

                headerCg.alpha = Mathf.Lerp(0f, 1f, e0);
                header.anchoredPosition = Vector2.LerpUnclamped(headerPos0 + new Vector2(0f, 26f), headerPos0, eBack);
                header.localScale = Vector3.one * Mathf.LerpUnclamped(0.92f, 1f, eBack);

                // Stagger icon slightly after header.
                float uIcon = Mathf.Clamp01((u - 0.10f) / 0.90f);
                float eIcon = MotionUtil.EaseOutBack(uIcon);
                iconCg.alpha = Mathf.Lerp(0f, 1f, MotionUtil.EaseOutCubic(uIcon));
                icon.anchoredPosition = Vector2.LerpUnclamped(iconPos0 + new Vector2(0f, -40f), iconPos0, eIcon);
                icon.localScale = Vector3.one * Mathf.LerpUnclamped(0.72f, 1f, eIcon);

                // Subtitle and buttons after icon.
                float uSub = Mathf.Clamp01((u - 0.22f) / 0.78f);
                float eSub = MotionUtil.EaseOutCubic(uSub);
                subtitleCg.alpha = Mathf.Lerp(0f, 1f, eSub);
                subtitle.anchoredPosition = Vector2.LerpUnclamped(subtitlePos0 + new Vector2(0f, -18f), subtitlePos0, MotionUtil.EaseOutBack(uSub));

                float uBtns = Mathf.Clamp01((u - 0.28f) / 0.72f);
                float eBtns = MotionUtil.EaseOutBack(uBtns);
                coinsCg.alpha = Mathf.Lerp(0f, 1f, MotionUtil.EaseOutCubic(uBtns));
                adCg.alpha = coinsCg.alpha;
                coins.anchoredPosition = Vector2.LerpUnclamped(coinsPos0 + new Vector2(0f, -28f), coinsPos0, eBtns);
                ad.anchoredPosition = Vector2.LerpUnclamped(adPos0 + new Vector2(0f, -28f), adPos0, eBtns);
                coins.localScale = Vector3.one * Mathf.LerpUnclamped(0.96f, 1f, eBtns);
                ad.localScale = coins.localScale;

                // Close button last.
                float uClose = Mathf.Clamp01((u - 0.35f) / 0.65f);
                float eClose = MotionUtil.EaseOutBack(uClose);
                closeCg.alpha = Mathf.Lerp(0f, 1f, MotionUtil.EaseOutCubic(uClose));
                close.anchoredPosition = Vector2.LerpUnclamped(closePos0 + new Vector2(0f, 18f), closePos0, eClose);
                close.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1f, eClose);

                yield return null;
            }

            header.anchoredPosition = headerPos0;
            icon.anchoredPosition = iconPos0;
            subtitle.anchoredPosition = subtitlePos0;
            coins.anchoredPosition = coinsPos0;
            ad.anchoredPosition = adPos0;
            close.anchoredPosition = closePos0;
            header.localScale = Vector3.one;
            icon.localScale = Vector3.one;
            subtitle.localScale = Vector3.one;
            coins.localScale = Vector3.one;
            ad.localScale = Vector3.one;
            close.localScale = Vector3.one;

            headerCg.alpha = 1f;
            iconCg.alpha = 1f;
            subtitleCg.alpha = 1f;
            coinsCg.alpha = 1f;
            adCg.alpha = 1f;
            closeCg.alpha = 1f;

            _boosterPurchaseIntroRoutine = null;
            _boosterPurchaseIdleRoutine = StartCoroutine(AnimateBoosterPurchaseIdle(iconPos0));
        }

        private IEnumerator AnimateBoosterPurchaseIdle(Vector2 iconBasePos)
        {
            if (_boosterPurchaseIconRect == null) yield break;
            float t = 0f;
            while (_boosterPurchasePanel != null && _boosterPurchasePanel.activeInHierarchy && _boosterPurchaseIconRect != null)
            {
                t += Time.unscaledDeltaTime;
                float bob = Mathf.Sin(t * 2.0f) * 10f;
                float tilt = Mathf.Sin(t * 1.7f) * 2.0f;
                _boosterPurchaseIconRect.anchoredPosition = iconBasePos + new Vector2(0f, bob);
                _boosterPurchaseIconRect.localRotation = Quaternion.Euler(0f, 0f, tilt);
                yield return null;
            }
            if (_boosterPurchaseIconRect != null)
            {
                _boosterPurchaseIconRect.anchoredPosition = iconBasePos;
                _boosterPurchaseIconRect.localRotation = Quaternion.identity;
            }
            _boosterPurchaseIdleRoutine = null;
        }

        private static void ApplyButtonPressScale(Button button, float pressedScale)
        {
            if (button == null) return;
            var baseScale = button.transform.localScale;
            var trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }
            if (trigger.triggers == null)
            {
                trigger.triggers = new List<EventTrigger.Entry>();
            }

            void AddOrReplace(EventTriggerType type, System.Action<BaseEventData> action)
            {
                trigger.triggers.RemoveAll(e => e != null && e.eventID == type);
                var entry = new EventTrigger.Entry { eventID = type };
                entry.callback.AddListener(data => action?.Invoke((BaseEventData)data));
                trigger.triggers.Add(entry);
            }

            AddOrReplace(EventTriggerType.PointerDown, _ => button.transform.localScale = baseScale * pressedScale);
            AddOrReplace(EventTriggerType.PointerUp, _ => button.transform.localScale = baseScale);
            AddOrReplace(EventTriggerType.PointerExit, _ => button.transform.localScale = baseScale);
            AddOrReplace(EventTriggerType.Cancel, _ => button.transform.localScale = baseScale);
        }

        private void SetPurchaseButtonVisuals(bool useFullPopup, bool showCoinPriceLabel)
        {
            // When the full popup image is used, hide all child visuals and keep only invisible clickable areas.
            if (_boosterPurchaseCloseImage != null)
            {
                if (useFullPopup)
                {
                    _boosterPurchaseCloseImage.sprite = null;
                    _boosterPurchaseCloseImage.color = new Color(1f, 1f, 1f, 0f);
                }
                else
                {
                    _boosterPurchaseCloseImage.color = Color.white;
                    if (_boosterPurchaseCloseImage.sprite == null)
                    {
                        _boosterPurchaseCloseImage.sprite = TryLoadBoosterPurchaseSprite("btn_close") ?? LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal");
                    }
                }
            }

            if (_boosterPurchaseCoinsImage != null)
            {
                _boosterPurchaseCoinsImage.color = useFullPopup ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
            if (_boosterPurchaseCoinsLabel != null) _boosterPurchaseCoinsLabel.gameObject.SetActive(showCoinPriceLabel);

            if (_boosterPurchaseAdImage != null)
            {
                _boosterPurchaseAdImage.color = useFullPopup ? new Color(1f, 1f, 1f, 0f) : Color.white;
            }
            // Keep the authored FREE button label hidden (the PNG includes it). Only force-hide when using full-popup mode.
            if (_boosterPurchaseAdLabel != null && useFullPopup) _boosterPurchaseAdLabel.gameObject.SetActive(false);
        }

        private void ApplyBoosterPurchaseLayoutFromManifest(BoosterType type, bool useFullPopup, Sprite fullPopupSprite)
        {
            var manifest = LoadBoosterPurchaseManifest();
            if (manifest?.assets?.popup_shuffle_full?.size == null || manifest.assets.popup_shuffle_full.box == null)
            {
                return;
            }

            // Use manifest's shuffle popup as the reference layout, and apply it proportionally to any popup image size.
            var refPopupAsset = manifest.assets.popup_shuffle_full;
            var refPopupSize = new Vector2(refPopupAsset.size[0], refPopupAsset.size[1]);
            var refPopupTL = new Vector2(refPopupAsset.box[0], refPopupAsset.box[1]);

            Vector2 targetPopupSize = refPopupSize;
            if (useFullPopup)
            {
                // Keep the popup at our designed size (in UI units), and fit the full sprite proportionally.
                var maxSize = _boosterPurchasePopupRect != null ? _boosterPurchasePopupRect.sizeDelta : refPopupSize;
                float spriteAspect = fullPopupSprite != null
                    ? (float)fullPopupSprite.rect.width / Mathf.Max(0.01f, (float)fullPopupSprite.rect.height)
                    : refPopupSize.x / Mathf.Max(0.01f, refPopupSize.y);

                float maxAspect = maxSize.x / Mathf.Max(0.01f, maxSize.y);
                if (maxAspect >= spriteAspect)
                {
                    // Limited by height.
                    float h = maxSize.y;
                    float w = h * spriteAspect;
                    targetPopupSize = new Vector2(w, h);
                }
                else
                {
                    // Limited by width.
                    float w = maxSize.x;
                    float h = w / Mathf.Max(0.01f, spriteAspect);
                    targetPopupSize = new Vector2(w, h);
                }

                if (_boosterPurchasePopupRect != null)
                {
                    _boosterPurchasePopupRect.sizeDelta = targetPopupSize;
                }

                if (_boosterPurchaseBackground != null)
                {
                    _boosterPurchaseBackground.preserveAspect = true;
                }
            }
            else
            {
                if (_boosterPurchaseBackground != null)
                {
                    _boosterPurchaseBackground.preserveAspect = false;
                }
            }

            ApplyRectFromManifestNormalized(_boosterPurchaseCloseRect, manifest.assets.btn_close, refPopupSize, refPopupTL, targetPopupSize);
            ApplyRectFromManifestNormalized(_boosterPurchaseCoinsRect, manifest.assets.btn_buy_coins_80, refPopupSize, refPopupTL, targetPopupSize);
            ApplyRectFromManifestNormalized(_boosterPurchaseAdRect, manifest.assets.btn_watch_ad_free, refPopupSize, refPopupTL, targetPopupSize);

            if (!useFullPopup)
            {
                ApplyRectFromManifestNormalized(_boosterPurchaseHeaderRect, manifest.assets.header_title_shuffle, refPopupSize, refPopupTL, targetPopupSize);
                ApplyRectFromManifestNormalized(_boosterPurchaseIconRect, manifest.assets.icon_booster_shuffle, refPopupSize, refPopupTL, targetPopupSize);
            }
        }

        private static void ApplyRectFromManifestNormalized(
            RectTransform target,
            BoosterPurchaseManifestAsset asset,
            Vector2 refPopupSize,
            Vector2 refPopupTopLeftInSource,
            Vector2 targetPopupSize)
        {
            if (target == null) return;
            if (asset?.box == null || asset.box.Length < 4) return;

            target.anchorMin = new Vector2(0.5f, 0.5f);
            target.anchorMax = new Vector2(0.5f, 0.5f);
            target.pivot = new Vector2(0.5f, 0.5f);

            float x1 = asset.box[0] - refPopupTopLeftInSource.x;
            float y1 = asset.box[1] - refPopupTopLeftInSource.y;
            float x2 = asset.box[2] - refPopupTopLeftInSource.x;
            float y2 = asset.box[3] - refPopupTopLeftInSource.y;

            float cx = (x1 + x2) * 0.5f;
            float cy = (y1 + y2) * 0.5f;
            float w = Mathf.Abs(x2 - x1);
            float h = Mathf.Abs(y2 - y1);

            float nx = refPopupSize.x <= 0.0001f ? 0.5f : (cx / refPopupSize.x);
            float ny = refPopupSize.y <= 0.0001f ? 0.5f : (cy / refPopupSize.y);
            float nw = refPopupSize.x <= 0.0001f ? 0.1f : (w / refPopupSize.x);
            float nh = refPopupSize.y <= 0.0001f ? 0.1f : (h / refPopupSize.y);

            // Convert from top-left origin to RectTransform centered coords, scaling to current popup size.
            target.anchoredPosition = new Vector2((nx - 0.5f) * targetPopupSize.x, (0.5f - ny) * targetPopupSize.y);
            target.sizeDelta = new Vector2(nw * targetPopupSize.x, nh * targetPopupSize.y);
        }

        private static BoosterPurchaseManifest LoadBoosterPurchaseManifest()
        {
            if (_boosterPurchaseManifestCache != null)
            {
                return _boosterPurchaseManifestCache;
            }

            var text = Resources.Load<TextAsset>("BoosterPurchase/assets_manifest");
            if (text == null)
            {
                return null;
            }

            try
            {
                _boosterPurchaseManifestCache = JsonUtility.FromJson<BoosterPurchaseManifest>(text.text);
            }
            catch
            {
                _boosterPurchaseManifestCache = null;
            }

            return _boosterPurchaseManifestCache;
        }

        private static Sprite TryLoadBoosterPurchaseSprite(string fileNameOrKey)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrKey)) return null;
            string key = fileNameOrKey.Trim();
            if (key.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring(0, key.Length - 4);
            }

            if (BoosterPurchaseSpriteCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite TryLoadSprite(string path) => Resources.Load<Sprite>(path);
            Texture2D TryLoadTexture(string path) => Resources.Load<Texture2D>(path);

            var s = TryLoadSprite(key) ?? TryLoadSprite($"BoosterPurchase/{key}") ?? TryLoadSprite($"BoosterPurchase/Sprites/{key}");
            if (s != null)
            {
                BoosterPurchaseSpriteCache[key] = s;
                return s;
            }

            // Fallback: if PNGs are imported as Texture2D (Texture Type = Default), create a runtime sprite.
            var tex = TryLoadTexture(key) ?? TryLoadTexture($"BoosterPurchase/{key}") ?? TryLoadTexture($"BoosterPurchase/Sprites/{key}");
            if (tex == null)
            {
                return null;
            }

            var created = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            BoosterPurchaseSpriteCache[key] = created;
            return created;
        }

        private static Sprite TryLoadSettingsPageSprite(string fileNameOrKey)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrKey)) return null;
            string key = fileNameOrKey.Trim();
            if (key.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring(0, key.Length - 4);
            }

            if (SettingsPageSpriteCache.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite TryLoadSprite(string path) => Resources.Load<Sprite>(path);
            Texture2D TryLoadTexture(string path) => Resources.Load<Texture2D>(path);

            var s = TryLoadSprite($"setting_page_assets/{key}") ?? TryLoadSprite(key);
            if (s != null)
            {
                SettingsPageSpriteCache[key] = s;
                return s;
            }

            var tex = TryLoadTexture($"setting_page_assets/{key}") ?? TryLoadTexture(key);
            if (tex == null)
            {
                return null;
            }

            var created = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            SettingsPageSpriteCache[key] = created;
            return created;
        }

        private void PurchaseBoosterWithCoins()
        {
            if (_gameOver) return;

            int price = GetBoosterCoinPrice(_boosterPurchaseType);
            if (_progress.Coins < price)
            {
                PlaySfx(SfxId.UiDenied);
                CloseBoosterPurchase();
                if (shopEnabled) OpenShop(ShopTab.Coins);
                return;
            }

            _progress.Coins -= price;
            AddBooster(_boosterPurchaseType, BoosterPurchaseGrantCount);
            RefreshEconomyHUD();
            RequestSave(SaveDelayStrongSeconds);
            PlaySfx(SfxId.UiConfirm);
            CloseBoosterPurchase();
        }

        private void PurchaseBoosterWithAd()
        {
            if (_gameOver) return;

            // Placeholder: grant immediately. Hook your ad SDK here.
            AddBooster(_boosterPurchaseType, BoosterPurchaseGrantCount);
            PlaySfx(SfxId.UiConfirm);
            CloseBoosterPurchase();
        }

    }
}



