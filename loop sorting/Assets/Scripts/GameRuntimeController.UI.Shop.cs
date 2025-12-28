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
        private enum ShopTab
        {
            Coins,
            Lives
        }

        private void RefreshEconomyHUD()
        {
            if (_coinText != null) _coinText.text = FormatCurrencyValue(_progress.Coins);
            if (_lifeText != null) _lifeText.text = _progress.Lives.ToString();
            if (_shopCoinValue != null) _shopCoinValue.text = _progress.Coins.ToString();
            if (_shopLifeValue != null) _shopLifeValue.text = _progress.Lives.ToString();
        }

        private static string FormatCurrencyValue(int value)
        {
            if (value < 0) return value.ToString();
            if (value <= 9_999) return value.ToString();

                        // Compact notation for HUD (Chinese units) to keep text readable in narrow pills.
            if (value < 100_000_000)
            {
                return FormatCompact(value, 10_000, LocalizedText.CurrencyTenThousandSuffix, decimals: value < 1_000_000 ? 1 : 0);
            }
            return FormatCompact(value, 100_000_000, LocalizedText.CurrencyHundredMillionSuffix, decimals: 1);
        }

	        private void OpenShop(ShopTab tab)
	        {
	            if (!shopEnabled) return;

	            EnsureShopUI();
	            RefreshEconomyHUD();
	            PopulateShop(tab);
	            if (_shopPanel != null) AnimateUiPanel(_shopPanel, true, seconds: 0.20f);
	            SettingsUi.HideImmediate();
	            if (_resultPanel != null) _resultPanel.SetActive(false);
	            PlaySfx(SfxId.UiPopupOpen);
	        }

        private void EnsureShopUI()
        {
            if (_uiCanvas == null) return;
            if (_shopPanel != null && _shopContentRoot != null && _shopScroll != null) return;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            if (TryInstantiateUiPrefab(ShopPanelPrefabResourcePath, out ShopPanelPrefabRefs prefab))
            {
                prefab.AutoAssign();

                if (prefab.title != null) prefab.title.text = LocalizedText.ShopTitle;

                _shopPanel = prefab.gameObject;
                _shopTitle = prefab.title;
                _shopScroll = prefab.scroll;
                _shopContentRoot = prefab.contentRoot != null ? prefab.contentRoot : (_shopScroll != null ? _shopScroll.content : null);
                _shopCoinValue = prefab.coinValue;
                _shopLifeValue = prefab.lifeValue;
                _shopScrollFadeTop = prefab.scrollFadeTop;
                _shopScrollFadeBottom = prefab.scrollFadeBottom;

                if (prefab.closeButton != null)
                {
                    prefab.closeButton.onClick.RemoveAllListeners();
                    prefab.closeButton.onClick.AddListener(() =>
                    {
                        PlaySfx(SfxId.UiPopupClose);
                        AnimateUiPanel(_shopPanel, false, seconds: 0.18f);
                    });
                }

                RebindShopPanelPrefabSprites(prefab, hasKit);
                _shopPanel.SetActive(false);
                return;
            }

            if (!AllowRuntimeUiAutoCreate) return;

            _shopPanel = new GameObject("ShopPanel");
            _shopPanel.transform.SetParent(_uiCanvas.transform, false);
            MarkRuntimeUi(_shopPanel);

            var dim = _shopPanel.AddComponent<Image>();
            dim.raycastTarget = true;
            if (hasKit)
            {
                // Always use a solid full-screen dim. Some themed overlay sprites can be partially transparent
                // (e.g. top/bottom gradients), which makes the HUD behind look like a layout bug.
                dim.sprite = null;
                dim.color = new Color(0f, 0f, 0f, 0.55f);
            }
            else
            {
                dim.color = new Color(0f, 0f, 0f, 0.55f);
            }
            var overlayRect = _shopPanel.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var panelGO = new GameObject("Panel");
            panelGO.transform.SetParent(_shopPanel.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = ModalPopupAnchoredPos;
            panelRect.sizeDelta = ModalPopupSize;

            var panelImg = panelGO.AddComponent<Image>();
            panelImg.raycastTarget = false;
            Transform layoutParent = panelGO.transform;
            if (hasKit)
            {
                // Prefer split panel (base + decor) if available; otherwise fall back to the legacy combined sprite.
                var baseSprite =
                    LoopSortingUIKit.LoadSprite("UI_Sprites/panel_gold_blue_base_9slice.png") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/panel_modal_base_9slice.png") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.panel_shop") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");

                panelImg.sprite = baseSprite;
                panelImg.type = panelImg.sprite != null && panelImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                panelImg.color = Color.white;
                ApplyFakeDecorShadow(panelImg, alpha: 0.22f);

                var existingDecor = panelGO.transform.Find("Decor");
                if (existingDecor != null) existingDecor.gameObject.SetActive(false);

                // Layout should be based on the visible silhouette (excluding transparent padding), otherwise UI elements
                // appear misaligned and 9-slice guides look "wrong" when the source texture has large margins.
                layoutParent = TryCreatePaddingTrimmedLayoutRoot(
                    parent: panelGO.transform,
                    panelRect: panelRect,
                    sprite: baseSprite,
                    desiredVisibleSizeUnits: ModalPopupSize,
                    centerStretchFraction: 1f / 3f);
            }
            else
            {
                panelImg.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
            }

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(layoutParent, false);
            _shopTitle = titleGO.AddComponent<TextMeshProUGUI>();
            _shopTitle.raycastTarget = false;
            _shopTitle.text = LocalizedText.ShopTitle;
            _shopTitle.alignment = TextAlignmentOptions.Center;
            _shopTitle.fontSize = 70;
            _shopTitle.color = Color.white;
            var titleRect = _shopTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -60f);
            titleRect.sizeDelta = new Vector2(700f, 100f);

            var closeBtn = CreateIconButton(
                parent: layoutParent,
                name: "CloseButton",
                anchor: new Vector2(1f, 1f),
                anchoredPos: ModalCloseInset,
                size: new Vector2(128f, 128f),
                normal: hasKit ? "ui.button.close_red.normal" : null,
                pressed: hasKit ? "ui.button.close_red.pressed" : null,
                disabled: hasKit ? "ui.button.close_red.disabled" : null,
                icon: hasKit ? "ui.icon.close" : null);
            var closeRect = closeBtn.GetComponent<RectTransform>();
            closeRect.pivot = new Vector2(1f, 1f);
            closeRect.anchoredPosition = ModalCloseInset;
            closeBtn.onClick.AddListener(() =>
            {
                PlaySfx(SfxId.UiPopupClose);
                AnimateUiPanel(_shopPanel, false, seconds: 0.18f);
            });

            // Currency row (matches UI kit blueprint intent).
            var currencyRowGO = new GameObject("CurrencyRow");
            currencyRowGO.transform.SetParent(layoutParent, false);
            var currencyRowRect = currencyRowGO.AddComponent<RectTransform>();
            currencyRowRect.anchorMin = new Vector2(0.5f, 1f);
            currencyRowRect.anchorMax = new Vector2(0.5f, 1f);
            currencyRowRect.pivot = new Vector2(0.5f, 1f);
            currencyRowRect.anchoredPosition = new Vector2(0f, -190f);
            currencyRowRect.sizeDelta = new Vector2(860f, 120f);

            var heartsStrip = CreateCurrencyStrip(currencyRowGO.transform, "Hearts", Vector2.zero, hasKit ? "ui.icon.heart" : null, out _shopLifeValue);
            heartsStrip.anchorMin = new Vector2(0f, 0.5f);
            heartsStrip.anchorMax = new Vector2(0f, 0.5f);
            heartsStrip.pivot = new Vector2(0f, 0.5f);
            heartsStrip.anchoredPosition = Vector2.zero;
            heartsStrip.sizeDelta = new Vector2(480f, 120f);

            var coinsStrip = CreateCurrencyStrip(currencyRowGO.transform, "Coins", Vector2.zero, hasKit ? "ui.icon.coin" : null, out _shopCoinValue);
            coinsStrip.anchorMin = new Vector2(1f, 0.5f);
            coinsStrip.anchorMax = new Vector2(1f, 0.5f);
            coinsStrip.pivot = new Vector2(1f, 0.5f);
            coinsStrip.anchoredPosition = Vector2.zero;
            coinsStrip.sizeDelta = new Vector2(480f, 120f);

            // Scroll list (v04_3 spec): ScrollRect -> Viewport (RectMask2D) -> Content (VerticalLayoutGroup + ContentSizeFitter)
            var scrollGO = new GameObject("ShopScrollList");
            scrollGO.transform.SetParent(layoutParent, false);
            var scrollRect = scrollGO.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            // Leave space at the top for title + currency row, and at the bottom for breathing room.
            scrollRect.offsetMin = new Vector2(50f, 100f);
            scrollRect.offsetMax = new Vector2(-50f, -300f);

            _shopScroll = scrollGO.AddComponent<ScrollRect>();
            _shopScroll.horizontal = false;
            _shopScroll.vertical = true;
            _shopScroll.movementType = ScrollRect.MovementType.Elastic;
            _shopScroll.inertia = true;
            _shopScroll.decelerationRate = 0.135f;
            _shopScroll.scrollSensitivity = 25f;

            var viewportGO = new GameObject("Viewport");
            viewportGO.transform.SetParent(scrollGO.transform, false);
            var viewportRect = viewportGO.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportImg = viewportGO.AddComponent<Image>();
            viewportImg.raycastTarget = true;
            viewportImg.color = new Color(1f, 1f, 1f, 0.001f);
            viewportGO.AddComponent<RectMask2D>();
            _shopScroll.viewport = viewportRect;

            var contentGO = new GameObject("Content");
            contentGO.transform.SetParent(viewportGO.transform, false);
            _shopContentRoot = contentGO.AddComponent<RectTransform>();
            _shopContentRoot.anchorMin = new Vector2(0f, 1f);
            _shopContentRoot.anchorMax = new Vector2(1f, 1f);
            _shopContentRoot.pivot = new Vector2(0.5f, 1f);
            _shopContentRoot.anchoredPosition = Vector2.zero;
            _shopContentRoot.sizeDelta = new Vector2(0f, 0f);

            var layout = contentGO.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 28f;
            layout.padding = new RectOffset(0, 0, 24, 60);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            var fitter = contentGO.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _shopScroll.content = _shopContentRoot;

            // Optional scroll fades (visual only).
            if (hasKit)
            {
                var fadeTopGO = new GameObject("FadeTop");
                fadeTopGO.transform.SetParent(scrollGO.transform, false);
                _shopScrollFadeTop = fadeTopGO.AddComponent<Image>();
                _shopScrollFadeTop.raycastTarget = false;
                _shopScrollFadeTop.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_top.png", pixelsPerUnit: 100f, applyNineSlice: false);
                _shopScrollFadeTop.color = Color.white;
                var fadeTopRect = fadeTopGO.GetComponent<RectTransform>();
                fadeTopRect.anchorMin = new Vector2(0f, 1f);
                fadeTopRect.anchorMax = new Vector2(1f, 1f);
                fadeTopRect.pivot = new Vector2(0.5f, 1f);
                fadeTopRect.anchoredPosition = Vector2.zero;
                fadeTopRect.sizeDelta = new Vector2(0f, 140f);

                var fadeBottomGO = new GameObject("FadeBottom");
                fadeBottomGO.transform.SetParent(scrollGO.transform, false);
                _shopScrollFadeBottom = fadeBottomGO.AddComponent<Image>();
                _shopScrollFadeBottom.raycastTarget = false;
                _shopScrollFadeBottom.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_bottom.png", pixelsPerUnit: 100f, applyNineSlice: false);
                _shopScrollFadeBottom.color = Color.white;
                var fadeBottomRect = fadeBottomGO.GetComponent<RectTransform>();
                fadeBottomRect.anchorMin = new Vector2(0f, 0f);
                fadeBottomRect.anchorMax = new Vector2(1f, 0f);
                fadeBottomRect.pivot = new Vector2(0.5f, 0f);
                fadeBottomRect.anchoredPosition = Vector2.zero;
                fadeBottomRect.sizeDelta = new Vector2(0f, 160f);
            }

            _shopPanel.SetActive(false);
        }

        private RectTransform CreateCurrencyStrip(Transform parent, string name, Vector2 anchoredPos, string iconKey, out TMP_Text valueText)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(420f, 120f);

            var bg = go.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                ApplySplitBackground(
                    baseImage: bg,
                    parent: go.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                    decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.35f);
            }

            if (!string.IsNullOrEmpty(iconKey) && hasKit)
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(go.transform, false);
                var icon = iconGO.AddComponent<Image>();
                icon.raycastTarget = false;
                icon.sprite = LoopSortingUIKit.LoadSpriteByKey(iconKey);
                icon.color = Color.white;
                var iRect = iconGO.GetComponent<RectTransform>();
                iRect.anchorMin = new Vector2(0f, 0.5f);
                iRect.anchorMax = new Vector2(0f, 0.5f);
                iRect.pivot = new Vector2(0f, 0.5f);
                iRect.anchoredPosition = new Vector2(24f, 0f);
                iRect.sizeDelta = new Vector2(90f, 90f);
            }

            var txtGO = new GameObject("Value");
            txtGO.transform.SetParent(go.transform, false);
            var tmp = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "0";
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.fontSize = 56;
            tmp.color = Color.white;
            var tRect = txtGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0f, 0.5f);
            tRect.pivot = new Vector2(0f, 0.5f);
            tRect.anchoredPosition = new Vector2(190f, 0f);
            tRect.sizeDelta = new Vector2(240f, 90f);

            valueText = tmp;
            return rect;
        }

        private void PopulateShop(ShopTab tab)
        {
            if (_shopContentRoot == null) return;

            for (int i = _shopContentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_shopContentRoot.GetChild(i).gameObject);
            }

            if (_shopTitle != null) _shopTitle.text = tab == ShopTab.Coins ? LocalizedText.ShopTitle : LocalizedText.ShopMoreLives;

            if (tab == ShopTab.Coins)
            {
                AddShopSectionHeader(_shopContentRoot, LocalizedText.ShopSectionCoins);
                AddShopCoinPackRow(_shopContentRoot, "Coins_1000", LocalizedText.ShopCoinPackTitle(1000), "+1000", () => { _progress.Coins += 1000; RefreshEconomyHUD(); RequestSave(SaveDelayStrongSeconds); PlaySfx(SfxId.UiConfirm); });
                AddShopCoinPackRow(_shopContentRoot, "Coins_5000", LocalizedText.ShopCoinPackTitle(5000), "+5000", () => { _progress.Coins += 5000; RefreshEconomyHUD(); RequestSave(SaveDelayStrongSeconds); PlaySfx(SfxId.UiConfirm); });
                AddShopCoinPackRow(_shopContentRoot, "Coins_10000", LocalizedText.ShopCoinPackTitle(10000), "+10000", () => { _progress.Coins += 10000; RefreshEconomyHUD(); RequestSave(SaveDelayStrongSeconds); PlaySfx(SfxId.UiConfirm); });
            }
            else
            {
                AddShopSectionHeader(_shopContentRoot, LocalizedText.ShopSectionLives);
                AddShopItem(_shopContentRoot, "Lives_1", LocalizedText.ShopLifePackTitle(1), "+1", () => { _progress.Lives += 1; RefreshEconomyHUD(); RequestSave(SaveDelayStrongSeconds); PlaySfx(SfxId.UiConfirm); });
                AddShopItem(_shopContentRoot, "Lives_5", LocalizedText.ShopLifeRefillTitle, "+5", () => { _progress.Lives = Mathf.Max(_progress.Lives, 5); RefreshEconomyHUD(); RequestSave(SaveDelayStrongSeconds); PlaySfx(SfxId.UiConfirm); });
            }

            if (_shopScroll != null)
            {
                Canvas.ForceUpdateCanvases();
                _shopScroll.verticalNormalizedPosition = 1f;
            }
        }

        private void AddShopItem(RectTransform parent, string name, string title, string rightLabel, Action onClick)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var itemGO = new GameObject(name);
            itemGO.transform.SetParent(parent, false);
            var rect = itemGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 260f);

            var le = itemGO.AddComponent<LayoutElement>();
            le.preferredHeight = 260f;

            var img = itemGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (hasKit)
            {
                // Prefer card background for lives; coins are rendered as rows by AddShopCoinPackRow().
                var baseSprite =
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_card_beige_base_9slice.png") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.shop.item_bg") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_card_beige.png") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                img.sprite = baseSprite;
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
                ApplyFakeDecorShadow(img, alpha: 0.18f);

                var decor = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_card_beige_decor.png");
                if (decor != null)
                {
                    var existingDecor = itemGO.transform.Find("Decor");
                    if (existingDecor != null) existingDecor.gameObject.SetActive(false);
                }
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }

            var btn = itemGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            ApplyButtonPressScale(btn, pressedScale: 0.98f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(itemGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.raycastTarget = false;
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0f, 0.5f);
            tRect.pivot = new Vector2(0f, 0.5f);
            tRect.anchoredPosition = new Vector2(70f, 0f);
            tRect.sizeDelta = new Vector2(560f, 120f);

            // Price button background (visual only; the whole card is clickable).
            if (hasKit)
            {
                var priceBgGO = new GameObject("PriceBG");
                priceBgGO.transform.SetParent(itemGO.transform, false);
                var priceImg = priceBgGO.AddComponent<Image>();
                priceImg.raycastTarget = false;
                priceImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal");
                priceImg.type = priceImg.sprite != null && priceImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                priceImg.color = Color.white;
                var pRect = priceBgGO.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(1f, 0.5f);
                pRect.anchorMax = new Vector2(1f, 0.5f);
                pRect.pivot = new Vector2(1f, 0.5f);
                pRect.anchoredPosition = new Vector2(-60f, 0f);
                pRect.sizeDelta = new Vector2(240f, 120f);
            }

            var rightGO = new GameObject("Right");
            rightGO.transform.SetParent(itemGO.transform, false);
            var rightText = rightGO.AddComponent<TextMeshProUGUI>();
            rightText.raycastTarget = false;
            rightText.text = rightLabel;
            rightText.alignment = TextAlignmentOptions.Center;
            rightText.fontSize = 52;
            rightText.color = Color.white;
            var rRect = rightGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1f, 0.5f);
            rRect.anchorMax = new Vector2(1f, 0.5f);
            rRect.pivot = new Vector2(1f, 0.5f);
            rRect.anchoredPosition = new Vector2(-70f, 0f);
            rRect.sizeDelta = new Vector2(220f, 120f);
        }

        private void AddShopSectionHeader(RectTransform parent, string title)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var go = new GameObject($"Section_{title}");
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 96f);

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 96f;

            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            if (hasKit)
            {
                var baseSprite =
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_group_bar_base.png") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_group_bar.png");
                img.sprite = baseSprite;
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
                ApplyFakeDecorShadow(img, alpha: 0.14f);

                var decor = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_group_bar_decor.png");
                if (decor != null)
                {
                    var existingDecor = go.transform.Find("Decor");
                    if (existingDecor != null) existingDecor.gameObject.SetActive(false);
                }
            }
            else
            {
                img.color = new Color(0f, 0f, 0f, 0.35f);
            }

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = title;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 48;
            tmp.color = Color.white;
            var tRect = tmp.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
        }

        private void AddShopCoinPackRow(RectTransform parent, string name, string title, string rightLabel, Action onClick)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var itemGO = new GameObject(name);
            itemGO.transform.SetParent(parent, false);
            var rect = itemGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 200f);

            var le = itemGO.AddComponent<LayoutElement>();
            le.preferredHeight = 200f;

            var img = itemGO.AddComponent<Image>();
            img.raycastTarget = true;
            if (hasKit)
            {
                var baseSprite =
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_row_yellow_base_9slice.png") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/shop_row_yellow.png");
                img.sprite = baseSprite;
                img.type = img.sprite != null && img.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                img.color = Color.white;
                ApplyFakeDecorShadow(img, alpha: 0.14f);

                var decor = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_row_yellow_decor.png");
                if (decor != null)
                {
                    var existingDecor = itemGO.transform.Find("Decor");
                    if (existingDecor != null) existingDecor.gameObject.SetActive(false);
                }
            }
            else
            {
                img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            }

            var btn = itemGO.AddComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            var colors = btn.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.95f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 0.95f);
            btn.colors = colors;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            ApplyButtonPressScale(btn, pressedScale: 0.98f);

            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(itemGO.transform, false);
            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.raycastTarget = false;
            titleText.text = title;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.fontSize = 56;
            titleText.color = Color.white;
            var tRect = titleGO.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0f, 0.5f);
            tRect.anchorMax = new Vector2(0f, 0.5f);
            tRect.pivot = new Vector2(0f, 0.5f);
            tRect.anchoredPosition = new Vector2(70f, 0f);
            tRect.sizeDelta = new Vector2(560f, 120f);

            // Price background (visual only; row click triggers action).
            if (hasKit)
            {
                var priceBgGO = new GameObject("PriceBG");
                priceBgGO.transform.SetParent(itemGO.transform, false);
                var priceImg = priceBgGO.AddComponent<Image>();
                priceImg.raycastTarget = false;
                priceImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal");
                priceImg.type = priceImg.sprite != null && priceImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                priceImg.color = Color.white;
                var pRect = priceBgGO.GetComponent<RectTransform>();
                pRect.anchorMin = new Vector2(1f, 0.5f);
                pRect.anchorMax = new Vector2(1f, 0.5f);
                pRect.pivot = new Vector2(1f, 0.5f);
                pRect.anchoredPosition = new Vector2(-60f, 0f);
                pRect.sizeDelta = new Vector2(240f, 120f);
            }

            var rightGO = new GameObject("Right");
            rightGO.transform.SetParent(itemGO.transform, false);
            var rightText = rightGO.AddComponent<TextMeshProUGUI>();
            rightText.raycastTarget = false;
            rightText.text = rightLabel;
            rightText.alignment = TextAlignmentOptions.Center;
            rightText.fontSize = 52;
            rightText.color = Color.white;
            var rRect = rightGO.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(1f, 0.5f);
            rRect.anchorMax = new Vector2(1f, 0.5f);
            rRect.pivot = new Vector2(1f, 0.5f);
            rRect.anchoredPosition = new Vector2(-70f, 0f);
            rRect.sizeDelta = new Vector2(220f, 120f);
        }

        private void CreateCurrencyBar(
            Transform parent,
            string name,
            Rect coinsTopLeft,
            Rect livesTopLeft,
            float referenceWidth,
            float safeTopUnits,
            float extraRightUnits,
            out TMP_Text coinText,
            out TMP_Text lifeText,
            out Button coinButton,
            out Button lifeButton)
        {
            coinText = null;
            lifeText = null;
            coinButton = null;
            lifeButton = null;

            bool hasKit = LoopSortingUIKit.IsAvailable();

            float barX = Mathf.Min(coinsTopLeft.x, livesTopLeft.x);
            float barY = Mathf.Min(coinsTopLeft.y, livesTopLeft.y);
            float barRight = Mathf.Max(coinsTopLeft.x + coinsTopLeft.width, livesTopLeft.x + livesTopLeft.width);
            float barW = Mathf.Max(1f, barRight - barX);
            float barH = Mathf.Max(1f, Mathf.Max(coinsTopLeft.height, livesTopLeft.height));

            float right = referenceWidth - (barX + barW) + extraRightUnits;

            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-right, -(barY + safeTopUnits));
            rect.sizeDelta = new Vector2(barW, barH);

            var bg = root.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                var fallback =
                    LoopSortingUIKit.LoadSpriteByKey("ui.hud.pill_dark") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                ApplySplitBackground(
                    baseImage: bg,
                    parent: root.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/hud_pill_dark_base_9slice.png",
                    decorPath: "UI_Sprites/hud_pill_dark_decor.png",
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.35f);
            }

            float coinOffsetX = coinsTopLeft.x - barX;
            float livesOffsetX = livesTopLeft.x - barX;

            float height = barH;
            float padding = Mathf.Clamp(height * 0.10f, 8f, 14f);
            float plusSize = Mathf.Clamp(height * 0.30f, 22f, 38f);
            float iconSize = Mathf.Clamp(height * 0.46f, 32f, 54f);
            float fontMax = Mathf.Clamp(height * 0.60f, 32f, 56f);

            Button CreateSegment(
                string segmentName,
                float xOffset,
                float width,
                string currencyIconKey,
                out TMP_Text value)
            {
                value = null;

                var segGO = new GameObject(segmentName);
                segGO.transform.SetParent(root.transform, false);
                var segRect = segGO.AddComponent<RectTransform>();
                segRect.anchorMin = new Vector2(0f, 0f);
                segRect.anchorMax = new Vector2(0f, 1f);
                segRect.pivot = new Vector2(0f, 0.5f);
                segRect.anchoredPosition = new Vector2(xOffset, 0f);
                segRect.sizeDelta = new Vector2(Mathf.Max(1f, width), 0f);

                var hit = segGO.AddComponent<Image>();
                hit.raycastTarget = true;
                hit.color = new Color(1f, 1f, 1f, 0f);

                var btn = segGO.AddComponent<Button>();
                btn.targetGraphic = hit;
                btn.transition = Selectable.Transition.None;
                ApplyButtonPressScale(btn, pressedScale: 0.98f);

                // Plus icon (left)
                var plusGO = new GameObject("Plus");
                plusGO.transform.SetParent(segGO.transform, false);
                var plusImg = plusGO.AddComponent<Image>();
                plusImg.raycastTarget = false;
                if (hasKit)
                {
                    plusImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.plus");
                    plusImg.color = Color.white;
                }
                var plusRect = plusGO.GetComponent<RectTransform>();
                plusRect.anchorMin = new Vector2(0f, 0.5f);
                plusRect.anchorMax = new Vector2(0f, 0.5f);
                plusRect.pivot = new Vector2(0f, 0.5f);
                plusRect.anchoredPosition = new Vector2(padding, 0f);
                plusRect.sizeDelta = new Vector2(plusSize, plusSize);

                // Currency icon (right)
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(segGO.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.raycastTarget = false;
                if (hasKit)
                {
                    iconImg.sprite = LoopSortingUIKit.LoadSpriteByKey(currencyIconKey);
                    iconImg.color = Color.white;
                }
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(1f, 0.5f);
                iconRect.anchorMax = new Vector2(1f, 0.5f);
                iconRect.pivot = new Vector2(1f, 0.5f);
                iconRect.anchoredPosition = new Vector2(-padding, 0f);
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);

                // Value
                var valueGO = new GameObject("Value");
                valueGO.transform.SetParent(segGO.transform, false);
                var tmp = valueGO.AddComponent<TextMeshProUGUI>();
                tmp.raycastTarget = false;
                tmp.text = "0";
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                tmp.enableWordWrapping = false;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMax = fontMax;
                tmp.fontSizeMin = Mathf.Clamp(fontMax * 0.58f, 18f, fontMax);
                tmp.fontSize = fontMax;
                tmp.color = Color.white;
                ApplyTmpOutlineUnderlay(
                    tmp,
                    outlineWidth: 0.22f,
                    outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                    underlayColor: new Color(0f, 0f, 0f, 0.35f),
                    underlayOffset: new Vector2(2f, -3f),
                    underlaySoftness: 0.32f,
                    underlayDilate: 0.05f);
                var valueRect = tmp.GetComponent<RectTransform>();
                valueRect.anchorMin = new Vector2(0f, 0f);
                valueRect.anchorMax = new Vector2(1f, 1f);
                valueRect.offsetMin = new Vector2(padding + plusSize + 8f, 0f);
                valueRect.offsetMax = new Vector2(-(padding + iconSize + 8f), 0f);

                value = tmp;
                return btn;
            }

            coinButton = CreateSegment(
                segmentName: "Coins",
                xOffset: coinOffsetX,
                width: coinsTopLeft.width,
                currencyIconKey: "ui.icon.coin",
                out coinText);

            lifeButton = CreateSegment(
                segmentName: "Lives",
                xOffset: livesOffsetX,
                width: livesTopLeft.width,
                currencyIconKey: "ui.icon.heart",
                out lifeText);

            // Divider between segments (optional)
            float dividerX = Mathf.Clamp(livesOffsetX, 0f, barW);
            if (dividerX > 1f && dividerX < barW - 1f)
            {
                var divGO = new GameObject("Divider");
                divGO.transform.SetParent(root.transform, false);
                var divImg = divGO.AddComponent<Image>();
                divImg.raycastTarget = false;
                divImg.color = new Color(1f, 1f, 1f, 0.12f);
                var divRect = divGO.GetComponent<RectTransform>();
                divRect.anchorMin = new Vector2(0f, 0f);
                divRect.anchorMax = new Vector2(0f, 1f);
                divRect.pivot = new Vector2(0.5f, 0.5f);
                divRect.anchoredPosition = new Vector2(dividerX, 0f);
                divRect.sizeDelta = new Vector2(2f, 0f);
            }
        }

        private void CreateCurrencyPill(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            string iconKey,
            bool showPlusButton,
            out TMP_Text valueText,
            out Button plusButton)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            var rect = root.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(Mathf.Clamp01(anchor.x), Mathf.Clamp01(anchor.y));
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            float height = Mathf.Max(1f, size.y);
            float padding = Mathf.Clamp(height * 0.12f, 8f, 18f);
            float iconSize = Mathf.Clamp(height - padding * 2f, 18f, height);
            float plusSize = Mathf.Clamp(height - padding * 2f, 18f, height);

            var bg = root.AddComponent<Image>();
            bg.raycastTarget = false;
            if (hasKit)
            {
                var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                ApplySplitBackground(
                    baseImage: bg,
                    parent: root.transform,
                    decorName: "Decor",
                    basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                    decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                    fallbackSprite: fallback,
                    noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
            }
            else
            {
                bg.color = new Color(0f, 0f, 0f, 0.35f);
            }

            // Icon
            float leftInset = padding;
            if (!string.IsNullOrEmpty(iconKey) && hasKit)
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(root.transform, false);
                var icon = iconGO.AddComponent<Image>();
                icon.raycastTarget = false;
                icon.sprite = LoopSortingUIKit.LoadSpriteByKey(iconKey);
                icon.color = Color.white;
                var iRect = iconGO.GetComponent<RectTransform>();
                iRect.anchorMin = new Vector2(0f, 0.5f);
                iRect.anchorMax = new Vector2(0f, 0.5f);
                iRect.pivot = new Vector2(0f, 0.5f);
                iRect.anchoredPosition = new Vector2(padding, 0f);
                iRect.sizeDelta = new Vector2(iconSize, iconSize);
                leftInset = padding + iconSize + padding;
            }

            // Value
            var valueGO = new GameObject("Value");
            valueGO.transform.SetParent(root.transform, false);
            var tmp = valueGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = "0";
            tmp.alignment = TextAlignmentOptions.MidlineRight;
            float maxSize = Mathf.Clamp(height * 0.72f, 34f, 56f);
            tmp.fontSize = maxSize;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = maxSize;
            tmp.fontSizeMin = Mathf.Clamp(maxSize * 0.55f, 20f, maxSize);
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.color = Color.white;
            ApplyTmpOutlineUnderlay(
                tmp,
                outlineWidth: 0.22f,
                outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -3f),
                underlaySoftness: 0.32f,
                underlayDilate: 0.05f);
             var vRect = tmp.GetComponent<RectTransform>();
             vRect.anchorMin = new Vector2(0f, 0f);
             vRect.anchorMax = new Vector2(1f, 1f);
             float rightInset = showPlusButton ? (padding + plusSize + padding) : padding;
             vRect.offsetMin = new Vector2(leftInset, 0f);
             vRect.offsetMax = new Vector2(-rightInset, 0f);

             // Plus button
             plusButton = null;
             if (showPlusButton)
             {
                 plusButton = CreateIconButton(
                     parent: root.transform,
                     name: "Plus",
                     anchor: new Vector2(1f, 0.5f),
                     anchoredPos: new Vector2(-padding - plusSize * 0.5f, 0f),
                     size: new Vector2(plusSize, plusSize),
                     normal: hasKit ? "ui.button.mint_square.normal" : null,
                     pressed: hasKit ? "ui.button.mint_square.pressed" : null,
                     disabled: hasKit ? "ui.button.mint_square.disabled" : null,
                     icon: hasKit ? "ui.icon.plus" : null);
                 ApplyButtonPressScale(plusButton, pressedScale: 0.96f);
             }

             valueText = tmp;
         }

    }
}




