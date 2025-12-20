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
        private void ApplyUIKitButtonSprites(Button button, Image image, string normal, string pressed, string disabled)
        {
            if (button == null || image == null) return;

            // Fallback: keep previous theme behavior if UI kit isn't available.
            if (string.IsNullOrEmpty(normal))
            {
                image.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
                if (uiTheme != null && uiTheme.buttonSprite != null) image.sprite = uiTheme.buttonSprite;
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                ApplyButtonPressScale(button, pressedScale: 0.96f);
                return;
            }

            var normalSprite = LoopSortingUIKit.LoadSpriteByKey(normal);
            var pressedSprite = !string.IsNullOrEmpty(pressed) ? LoopSortingUIKit.LoadSpriteByKey(pressed) : null;
            var disabledSprite = !string.IsNullOrEmpty(disabled) ? LoopSortingUIKit.LoadSpriteByKey(disabled) : null;
            if (normalSprite == null)
            {
                image.color = uiTheme != null ? uiTheme.buttonColor : new Color(0.2f, 0.2f, 0.2f, 0.85f);
                if (uiTheme != null && uiTheme.buttonSprite != null) image.sprite = uiTheme.buttonSprite;
                button.targetGraphic = image;
                button.transition = Selectable.Transition.ColorTint;
                ApplyButtonPressScale(button, pressedScale: 0.96f);
                return;
            }

            image.sprite = normalSprite;
            image.type = normalSprite != null && normalSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;

            var state = button.spriteState;
            state.highlightedSprite = normalSprite;
            state.pressedSprite = usePressedButtonSprites && pressedSprite != null ? pressedSprite : normalSprite;
            state.disabledSprite = usePressedButtonSprites && disabledSprite != null ? disabledSprite : normalSprite;
            button.spriteState = state;

            ApplyButtonPressScale(button, pressedScale: 0.96f);

            if (!_didLogOrangeLongNineSlice && string.Equals(normal, "ui.button.orange_long.normal", StringComparison.Ordinal))
            {
                _didLogOrangeLongNineSlice = true;
                Debug.Log(
                    $"[NineSliceCheck] {normal} -> sprite='{normalSprite.name}', rect={normalSprite.rect.width:0}x{normalSprite.rect.height:0}, " +
                    $"border(L,B,R,T)={normalSprite.border} pressedBorder={pressedSprite?.border.ToString() ?? "(null)"}");
            }
        }

        private void RebindGameplayHudPrefabSprites(GameplayHudPrefabRefs prefab, bool hasKit)
        {
            if (prefab == null) return;

            void ApplyButton(Button btn, string normal, string pressed, string disabled)
            {
                if (btn == null) return;
                var img = btn.GetComponent<Image>();
                if (img == null) return;
                ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);
            }

            void ApplyIcon(Transform parent, string iconKey, bool preserveAspect)
            {
                if (parent == null) return;
                var iconT = parent.Find("Icon");
                if (iconT == null) return;
                var img = iconT.GetComponent<Image>();
                if (img == null) img = iconT.gameObject.AddComponent<Image>();
                img.raycastTarget = false;

                Sprite s = hasKit && !string.IsNullOrEmpty(iconKey) ? LoopSortingUIKit.LoadSpriteByKey(iconKey) : null;
                img.sprite = s;
                img.type = Image.Type.Simple;
                img.color = s != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                img.preserveAspect = preserveAspect;
            }

            // Free slots counter
            if (prefab.beltCounterUI != null)
            {
                var counterBgT = prefab.beltCounterUI.transform.parent;
                var bgImg = counterBgT != null ? counterBgT.GetComponent<Image>() : null;
                if (bgImg != null)
                {
                    var fallback = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg") : null;
                    ApplySplitBackground(
                        baseImage: bgImg,
                        parent: bgImg.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                        decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(0.1f, 0.1f, 0.1f, 0.55f));
                }

                var iconT = counterBgT != null ? counterBgT.Find("Icon") : null;
                var iconImg = iconT != null ? iconT.GetComponent<Image>() : null;
                if (iconImg != null)
                {
                    var s = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.counter.icon") : null;
                    iconImg.sprite = s;
                    iconImg.type = Image.Type.Simple;
                    iconImg.color = s != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                    iconImg.preserveAspect = true;
                }
            }

            // Level label background + text styling.
            var levelLabel = prefab.transform.Find("LevelLabel");
            var levelBg = levelLabel != null ? levelLabel.GetComponent<Image>() : null;
            if (levelBg != null)
            {
                if (hasKit)
                {
                    levelBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.hud.level_bg");
                    levelBg.type = levelBg.sprite != null && levelBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    levelBg.color = levelBg.sprite != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                }
                else
                {
                    levelBg.sprite = null;
                    levelBg.color = new Color(0f, 0f, 0f, 0.25f);
                }
            }

            if (hasKit && prefab.levelText != null)
            {
                prefab.levelText.raycastTarget = false;
                prefab.levelText.enableWordWrapping = false;
                prefab.levelText.alignment = TextAlignmentOptions.Center;
                prefab.levelText.color = Color.white;
                ApplyTmpOutlineUnderlay(
                    prefab.levelText,
                    outlineWidth: 0.20f,
                    outlineColor: new Color(0.04f, 0.08f, 0.16f, 1f),
                    underlayColor: new Color(0f, 0f, 0f, 0.32f),
                    underlayOffset: new Vector2(2f, -3f),
                    underlaySoftness: 0.30f,
                    underlayDilate: 0.04f);
            }

            // Buttons + icons
            ApplyButton(prefab.shopButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
            ApplyIcon(prefab.shopButton != null ? prefab.shopButton.transform : null, "ui.icon.shop", preserveAspect: true);

            ApplyButton(prefab.speedButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");

            ApplyButton(prefab.settingsButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
            ApplyIcon(prefab.settingsButton != null ? prefab.settingsButton.transform : null, "ui.icon.gear", preserveAspect: true);

            // Currency pills
            if (prefab.coinText != null)
            {
                var root = prefab.coinText.transform.parent;
                var bg = root != null ? root.GetComponent<Image>() : null;
                if (bg != null)
                {
                    var fallback = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg") : null;
                    ApplySplitBackground(
                        baseImage: bg,
                        parent: bg.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                        decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
                }
                ApplyIcon(root, "ui.icon.coin", preserveAspect: true);
            }
            ApplyButton(prefab.coinPlusButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
            ApplyIcon(prefab.coinPlusButton != null ? prefab.coinPlusButton.transform : null, "ui.icon.plus", preserveAspect: true);

            if (prefab.lifeText != null)
            {
                var root = prefab.lifeText.transform.parent;
                var bg = root != null ? root.GetComponent<Image>() : null;
                if (bg != null)
                {
                    var fallback = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg") : null;
                    ApplySplitBackground(
                        baseImage: bg,
                        parent: bg.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                        decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
                }
                ApplyIcon(root, "ui.icon.heart", preserveAspect: true);
            }
            ApplyButton(prefab.lifePlusButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
            ApplyIcon(prefab.lifePlusButton != null ? prefab.lifePlusButton.transform : null, "ui.icon.plus", preserveAspect: true);

            // Boosters
            ApplyButton(prefab.boosterSortButton, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
            ApplyIcon(prefab.boosterSortButton != null ? prefab.boosterSortButton.transform : null, "ui.icon.sort", preserveAspect: true);

            ApplyButton(prefab.boosterShuffleButton, "ui.button.purple_square.normal", "ui.button.purple_square.pressed", "ui.button.purple_square.disabled");
            ApplyIcon(prefab.boosterShuffleButton != null ? prefab.boosterShuffleButton.transform : null, "ui.icon.shuffle", preserveAspect: true);

            void RebindExistingBadge(Button b)
            {
                if (b == null) return;
                var badge = b.transform.Find("Badge");
                if (badge == null) return;

                var bg = badge.Find("BadgeBG")?.GetComponent<Image>();
                if (bg != null)
                {
                    UIPrefabPreviewUtil.ApplySimpleIfMissing(bg, LoopSortingUIKit.LoadSpriteByKey("ui.badge.bg"), preserveAspect: true);
                    bg.raycastTarget = false;
                }

                var tmp = badge.Find("Text")?.GetComponent<TextMeshProUGUI>();
                if (tmp == null) tmp = badge.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
                if (tmp != null)
                {
                    tmp.raycastTarget = false;
                    tmp.enableWordWrapping = false;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = Color.white;
                    ApplyTmpOutlineUnderlay(
                        tmp,
                        outlineWidth: 0.20f,
                        outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                        underlayColor: new Color(0f, 0f, 0f, 0.35f),
                        underlayOffset: new Vector2(2f, -2f),
                        underlaySoftness: 0.28f,
                        underlayDilate: 0.02f);
                }
            }

            RebindExistingBadge(prefab.boosterSortButton);
            RebindExistingBadge(prefab.boosterShuffleButton);

            // Fast tag (optional)
            if (_hudRootRect != null)
            {
                var fastBg = _hudRootRect.Find("FastTag/BG")?.GetComponent<Image>();
                if (fastBg != null)
                {
                    if (hasKit)
                    {
                        fastBg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info");
                        fastBg.type = fastBg.sprite != null && fastBg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                        fastBg.color = fastBg.sprite != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                    }
                    else
                    {
                        fastBg.sprite = null;
                        fastBg.color = new Color(0f, 0f, 0f, 0.35f);
                    }
                }
            }
        }

        private static void ApplyTmpOutlineUnderlay(
            TMP_Text tmp,
            float outlineWidth,
            Color outlineColor,
            Color underlayColor,
            Vector2 underlayOffset,
            float underlaySoftness,
            float underlayDilate)
        {
            if (tmp == null) return;
            var source = tmp.fontSharedMaterial;
            if (source == null)
            {
                var font = tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
                if (font != null) tmp.font = font;
                if (tmp.fontSharedMaterial == null && font != null && font.material != null)
                {
                    tmp.fontSharedMaterial = font.material;
                }
                source = tmp.fontSharedMaterial;
            }
            if (source == null) return;

            // Clone material so we don't mutate shared TMP materials globally.
            var mat = new Material(source) { hideFlags = HideFlags.HideAndDontSave };
            tmp.fontSharedMaterial = mat;

            if (mat.HasProperty(ShaderUtilities.ID_OutlineWidth))
            {
                mat.EnableKeyword("OUTLINE_ON");
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, Mathf.Clamp01(outlineWidth));
            }
            if (mat.HasProperty(ShaderUtilities.ID_OutlineColor))
            {
                mat.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            }

            if (mat.HasProperty(ShaderUtilities.ID_UnderlayColor))
            {
                mat.EnableKeyword("UNDERLAY_ON");
                mat.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
            }
            if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetX)) mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffset.x);
            if (mat.HasProperty(ShaderUtilities.ID_UnderlayOffsetY)) mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffset.y);
            if (mat.HasProperty(ShaderUtilities.ID_UnderlaySoftness)) mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, Mathf.Clamp01(underlaySoftness));
            if (mat.HasProperty(ShaderUtilities.ID_UnderlayDilate)) mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, Mathf.Clamp(underlayDilate, -1f, 1f));

            tmp.UpdateMeshPadding();
        }

        private static void RemoveButtonFrame(Button button)
        {
            if (button == null) return;

            var img = button.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = null;
                img.type = Image.Type.Simple;
                img.color = new Color(1f, 1f, 1f, 0f);
            }

            // Prevent SpriteSwap from re-applying the framed sprites on press/highlight.
            button.transition = Selectable.Transition.None;
            var state = button.spriteState;
            state.highlightedSprite = null;
            state.pressedSprite = null;
            state.disabledSprite = null;
            button.spriteState = state;
        }

        private Button CreateIconButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            Vector2 size,
            string normal,
            string pressed,
            string disabled,
            string icon)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);

            if (!string.IsNullOrEmpty(icon))
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(go.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.raycastTarget = false;
                iconImg.sprite = LoopSortingUIKit.LoadSpriteByKey(icon);
                iconImg.color = Color.white;
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                float iconSize = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.68f, 12f, 9999f);
                iconRect.anchoredPosition = new Vector2(0f, iconSize * 0.05f);
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
            }

            return btn;
        }

        private bool TryInstantiateUiPrefab<T>(string resourcePath, out T component) where T : Component
        {
            component = null;
            if (_uiCanvas == null) return false;
            if (string.IsNullOrWhiteSpace(resourcePath)) return false;

            var prefab = Resources.Load<GameObject>(resourcePath.Trim());
            if (prefab == null) return false;

            var instance = Instantiate(prefab, _uiCanvas.transform, false);
            instance.name = prefab.name;
            component = instance.GetComponent<T>();
            if (component == null)
            {
                Destroy(instance);
                return false;
            }
            return true;
        }

        private void RebindMainMenuCanvasPrefabSprites(MainMenuCanvasPrefabRefs prefab, bool hasKit)
        {
            if (!hasKit) return;

            var root = prefab != null ? prefab.transform : (_mainMenuCanvas != null ? _mainMenuCanvas.transform : null);
            if (root == null) return;

            var bg = prefab != null ? prefab.backgroundImage : null;
            if (bg == null)
            {
                var bgT = root.Find("BG");
                if (bgT != null) bg = bgT.GetComponent<Image>();
            }
            if (bg != null)
            {
                var bgSprite = LoopSortingUIKit.LoadSpriteByKey("ui.bg_main");
                if (bgSprite != null)
                {
                    bg.sprite = bgSprite;
                    bg.color = Color.white;
                    bg.type = Image.Type.Simple;
                    bg.preserveAspect = false;
                }
            }

            var settingsButton = prefab != null ? prefab.settingsButton : null;
            if (settingsButton == null)
            {
                var t = root.Find("SafeArea/SettingsButton") ?? root.Find("SettingsButton");
                if (t != null) settingsButton = t.GetComponent<Button>();
            }
            if (settingsButton != null)
            {
                var img = settingsButton.GetComponent<Image>();
                if (img != null)
                {
                    ApplyUIKitButtonSprites(settingsButton, img, "ui.button.mint_square.normal", "ui.button.mint_square.pressed", "ui.button.mint_square.disabled");
                    var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.gear");
                    if (iconSprite != null)
                    {
                        var iconImg = EnsureOverlayImage(img.transform, "Icon", iconSprite);
                        if (iconImg != null)
                        {
                            iconImg.raycastTarget = false;
                            iconImg.preserveAspect = true;
                            var r = iconImg.rectTransform;
                            float side = Mathf.Min(img.rectTransform.rect.width, img.rectTransform.rect.height) * 0.62f;
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

            var playButton = prefab != null ? prefab.playButton : null;
            if (playButton == null)
            {
                var t = root.Find("SafeArea/PlayButton") ?? root.Find("PlayButton");
                if (t != null) playButton = t.GetComponent<Button>();
            }
            if (playButton != null)
            {
                var img = playButton.GetComponent<Image>();
                if (img != null)
                {
                    ApplyUIKitButtonSprites(playButton, img, "ui.button.orange_long.normal", "ui.button.orange_long.pressed", "ui.button.orange_long.disabled");
                }
            }

            var titleImg = prefab != null ? prefab.titleImage : null;
            if (titleImg == null)
            {
                var t = root.Find("SafeArea/Title") ?? root.Find("Title");
                if (t != null) titleImg = t.GetComponent<Image>();
            }
            if (titleImg != null)
            {
                var titleSprite =
                    LoopSortingUIKit.LoadSpriteByKey("ui.title.main") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/title_fangkuai_zhuan_bu_ting.png", pixelsPerUnit: 100f, applyNineSlice: false);
                if (titleSprite != null)
                {
                    titleImg.sprite = titleSprite;
                    titleImg.color = Color.white;
                    titleImg.type = Image.Type.Simple;
                    titleImg.preserveAspect = true;
                }
            }

            var levelPillBg = prefab != null ? prefab.levelPillBackground : null;
            if (levelPillBg == null)
            {
                var t = root.Find("SafeArea/LevelPill") ?? root.Find("LevelPill");
                if (t != null) levelPillBg = t.GetComponent<Image>();
            }
            if (levelPillBg != null)
            {
                var pillSprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info");
                if (pillSprite != null)
                {
                    levelPillBg.sprite = pillSprite;
                    levelPillBg.color = Color.white;
                    levelPillBg.type = pillSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    levelPillBg.preserveAspect = false;
                }
            }
        }

        private void RebindSettingsPanelPrefabSprites(SettingsPanelPrefabRefs prefab, bool hasKit)
        {
            if (prefab == null) return;

            if (hasKit)
            {
                var popupImg = prefab.popupRect != null ? prefab.popupRect.GetComponent<Image>() : null;
                if (popupImg != null)
                {
                    var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                    ApplySplitBackground(
                        baseImage: popupImg,
                        parent: popupImg.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/panel_modal_base_9slice.png",
                        decorPath: null,
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(1f, 1f, 1f, 0.92f));
                }
            }

            if (prefab.closeButton != null)
            {
                var closeImg = prefab.closeImage != null ? prefab.closeImage : prefab.closeButton.GetComponent<Image>();
                if (closeImg != null)
                {
                    ApplyUIKitButtonSprites(prefab.closeButton, closeImg, "ui.button.close_red.normal", "ui.button.close_red.pressed", "ui.button.close_red.disabled");
                    if (hasKit)
                    {
                        var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                        if (iconSprite != null)
                        {
                            var iconImg = EnsureOverlayImage(closeImg.transform, "Icon", iconSprite);
                            if (iconImg != null)
                            {
                                iconImg.preserveAspect = true;
                                var r = iconImg.rectTransform;
                                float side = Mathf.Min(closeImg.rectTransform.rect.width, closeImg.rectTransform.rect.height) * 0.62f;
                                r.anchorMin = new Vector2(0.5f, 0.5f);
                                r.anchorMax = new Vector2(0.5f, 0.5f);
                                r.pivot = new Vector2(0.5f, 0.5f);
                                r.anchoredPosition = Vector2.zero;
                                r.sizeDelta = new Vector2(side, side);
                            }
                        }
                    }
                }
            }

            if (prefab.retryButton != null)
            {
                var retryImg = prefab.retryImage != null ? prefab.retryImage : prefab.retryButton.GetComponent<Image>();
                if (retryImg != null)
                {
                    ApplyUIKitButtonSprites(prefab.retryButton, retryImg, "ui.button.orange_long.normal", "ui.button.orange_long.pressed", "ui.button.orange_long.disabled");
                }
            }
        }

        private void RebindShopPanelPrefabSprites(ShopPanelPrefabRefs prefab, bool hasKit)
        {
            if (prefab == null) return;

            if (hasKit && prefab.panelRect != null)
            {
                var panelImg = prefab.panelRect.GetComponent<Image>();
                if (panelImg != null)
                {
                    var baseSprite =
                        LoopSortingUIKit.LoadSprite("UI_Sprites/panel_gold_blue_base_9slice.png") ??
                        LoopSortingUIKit.LoadSprite("UI_Sprites/panel_modal_base_9slice.png") ??
                        LoopSortingUIKit.LoadSpriteByKey("ui.panel_shop") ??
                        LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");

                    panelImg.sprite = baseSprite;
                    panelImg.type = panelImg.sprite != null && panelImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    panelImg.color = Color.white;
                    ApplyFakeDecorShadow(panelImg, alpha: 0.22f);

                    var existingDecor = prefab.panelRect.transform.Find("Decor");
                    if (existingDecor != null) existingDecor.gameObject.SetActive(false);
                }
            }

            if (hasKit && prefab.closeButton != null)
            {
                var closeImg = prefab.closeButton.GetComponent<Image>();
                if (closeImg != null)
                {
                    ApplyUIKitButtonSprites(prefab.closeButton, closeImg, "ui.button.close_red.normal", "ui.button.close_red.pressed", "ui.button.close_red.disabled");
                    var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                    if (iconSprite != null)
                    {
                        var iconImg = EnsureOverlayImage(closeImg.transform, "Icon", iconSprite);
                        if (iconImg != null)
                        {
                            iconImg.preserveAspect = true;
                            var r = iconImg.rectTransform;
                            float side = Mathf.Min(closeImg.rectTransform.rect.width, closeImg.rectTransform.rect.height) * 0.68f;
                            r.anchorMin = new Vector2(0.5f, 0.5f);
                            r.anchorMax = new Vector2(0.5f, 0.5f);
                            r.pivot = new Vector2(0.5f, 0.5f);
                            r.anchoredPosition = new Vector2(0f, side * 0.05f);
                            r.sizeDelta = new Vector2(side, side);
                        }
                    }
                }
            }

            if (hasKit)
            {
                if (prefab.scrollFadeTop != null)
                {
                    prefab.scrollFadeTop.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_top.png", pixelsPerUnit: 100f, applyNineSlice: false);
                    prefab.scrollFadeTop.color = Color.white;
                }
                if (prefab.scrollFadeBottom != null)
                {
                    prefab.scrollFadeBottom.sprite = LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_bottom.png", pixelsPerUnit: 100f, applyNineSlice: false);
                    prefab.scrollFadeBottom.color = Color.white;
                }
            }

            if (hasKit)
            {
                void RebindStrip(TMP_Text valueText, string iconKey)
                {
                    if (valueText == null) return;
                    var strip = valueText.transform.parent;
                    if (strip == null) return;

                    var bg = strip.GetComponent<Image>();
                    if (bg != null)
                    {
                        var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg");
                        ApplySplitBackground(
                            baseImage: bg,
                            parent: strip,
                            decorName: "Decor",
                            basePath: "UI_Sprites/hud_pill_dark_small_base_9slice.png",
                            decorPath: "UI_Sprites/hud_pill_dark_small_decor.png",
                            fallbackSprite: fallback,
                            noSpriteColor: new Color(0f, 0f, 0f, 0.35f));
                    }

                    var icon = strip.Find("Icon");
                    if (icon != null)
                    {
                        var img = icon.GetComponent<Image>();
                        if (img != null)
                        {
                            img.sprite = LoopSortingUIKit.LoadSpriteByKey(iconKey);
                            img.color = Color.white;
                            img.preserveAspect = true;
                        }
                    }
                }

                RebindStrip(prefab.lifeValue, "ui.icon.heart");
                RebindStrip(prefab.coinValue, "ui.icon.coin");
            }
        }

        private void RebindResultPanelPrefabSprites(bool hasKit)
        {
            if (_resultPanel == null) return;

            var box = _resultPanel.transform.Find("Panel");
            if (box != null)
            {
                var img = box.GetComponent<Image>();
                if (img != null)
                {
                    if (hasKit)
                    {
                        var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_result");
                        ApplySplitBackground(
                            baseImage: img,
                            parent: box.transform,
                            decorName: "Decor",
                            basePath: "UI_Sprites/panel_result_base_9slice.png",
                            decorPath: "UI_Sprites/panel_result_decor.png",
                            fallbackSprite: fallback,
                            noSpriteColor: new Color(0.12f, 0.12f, 0.12f, 0.95f));
                    }
                    else
                    {
                        img.sprite = null;
                        img.type = Image.Type.Simple;
                        img.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);
                        img.preserveAspect = false;
                    }
                }
            }

            var banner = _resultPanel.transform.Find("Panel/LayoutRoot/Banner") ?? _resultPanel.transform.Find("Panel/Banner") ?? _resultPanel.transform.Find("Banner");
            if (banner != null)
            {
                var bannerImg = banner.GetComponent<Image>();
                if (bannerImg != null)
                {
                    if (hasKit)
                    {
                        bannerImg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info");
                        bannerImg.type = bannerImg.sprite != null && bannerImg.sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                        bannerImg.color = Color.white;
                    }
                    else
                    {
                        bannerImg.sprite = null;
                        bannerImg.color = new Color(1f, 1f, 1f, 0f);
                    }
                }
            }

            if (_primaryButton != null)
            {
                var img = _primaryButton.GetComponent<Image>();
                if (img != null)
                {
                    ApplyUIKitButtonSprites(
                        _primaryButton,
                        img,
                        normal: hasKit ? "ui.button.mint_long.normal" : null,
                        pressed: hasKit ? "ui.button.mint_long.pressed" : null,
                        disabled: hasKit ? "ui.button.mint_long.disabled" : null);
                }
            }
            if (_secondaryButton != null)
            {
                var img = _secondaryButton.GetComponent<Image>();
                if (img != null)
                {
                    ApplyUIKitButtonSprites(
                        _secondaryButton,
                        img,
                        normal: hasKit ? "ui.button.orange_long.normal" : null,
                        pressed: hasKit ? "ui.button.orange_long.pressed" : null,
                        disabled: hasKit ? "ui.button.orange_long.disabled" : null);
                }
            }
        }

        private void RebindBoosterPurchasePanelPrefabSprites(BoosterPurchasePanelPrefabRefs prefab, bool hasKit)
        {
            if (prefab == null) return;

            if (hasKit && prefab.popupRect != null)
            {
                var popupImg = prefab.popupRect.GetComponent<Image>();
                if (popupImg != null)
                {
                    var fallback = LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal");
                    ApplySplitBackground(
                        baseImage: popupImg,
                        parent: popupImg.transform,
                        decorName: "Decor",
                        basePath: "UI_Sprites/panel_modal_base_9slice.png",
                        decorPath: null,
                        fallbackSprite: fallback,
                        noSpriteColor: new Color(1f, 1f, 1f, 0.92f));
                }
            }

            if (hasKit && prefab.headerRect != null)
            {
                var headerImg = prefab.headerRect.GetComponent<Image>();
                if (headerImg != null)
                {
                    var s = LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal");
                    headerImg.sprite = s;
                    headerImg.type = s != null && s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    headerImg.color = Color.white;
                }
            }

            if (hasKit && prefab.subtitleBg != null)
            {
                var pill = LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info");
                if (pill != null)
                {
                    prefab.subtitleBg.sprite = pill;
                    prefab.subtitleBg.type = pill.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    prefab.subtitleBg.color = Color.white;
                }
                else
                {
                    prefab.subtitleBg.sprite = null;
                    prefab.subtitleBg.color = new Color(1f, 1f, 1f, 0.55f);
                }
            }

            if (prefab.closeImage != null)
            {
                var s = TryLoadBoosterPurchaseSprite("btn_close") ?? (hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal") : null);
                prefab.closeImage.sprite = s;
                prefab.closeImage.type = s != null && s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                prefab.closeImage.color = Color.white;
            }

            if (prefab.coinsImage != null)
            {
                var s = TryLoadBoosterPurchaseSprite("btn_buy_coins_80") ?? (hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal") : null);
                prefab.coinsImage.sprite = s;
                prefab.coinsImage.type = s != null && s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                prefab.coinsImage.color = Color.white;
            }

            if (prefab.coinsPriceCover != null)
            {
                prefab.coinsPriceCover.sprite = null;
                prefab.coinsPriceCover.color = new Color(1f, 1f, 1f, 0f);
                prefab.coinsPriceCover.raycastTarget = false;
            }

            if (prefab.coinsLabel != null)
            {
                var r = prefab.coinsLabel.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(160f, 0f);
                r.offsetMax = new Vector2(-60f, 0f);
            }

            Image EnsureIcon(Transform buttonTransform)
            {
                if (buttonTransform == null) return null;
                var icon = buttonTransform.Find("Icon")?.GetComponent<Image>();
                if (icon == null) icon = CreateButtonIcon(buttonTransform);
                if (icon == null) return null;
                icon.raycastTarget = false;
                return icon;
            }

            if (prefab.coinsImage != null)
            {
                var icon = EnsureIcon(prefab.coinsImage.transform);
                if (icon != null)
                {
                    var s = hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.icon.coin") : null;
                    icon.sprite = s;
                    icon.type = Image.Type.Simple;
                    icon.preserveAspect = true;
                    icon.color = s != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                }
            }

            if (prefab.adImage != null)
            {
                var authored = TryLoadBoosterPurchaseSprite("btn_watch_ad_free");
                var s = authored ?? (hasKit ? LoopSortingUIKit.LoadSpriteByKey("ui.button.mint_long.normal") : null);
                prefab.adImage.sprite = s;
                prefab.adImage.type = s != null && s.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                prefab.adImage.color = Color.white;

                // Keep TMP label hidden when authored PNG already contains "FREE".
                if (prefab.adLabel != null && authored != null)
                {
                    prefab.adLabel.gameObject.SetActive(false);
                }
            }

            if (prefab.adLabel != null)
            {
                var r = prefab.adLabel.rectTransform;
                r.anchorMin = Vector2.zero;
                r.anchorMax = Vector2.one;
                r.offsetMin = new Vector2(160f, 0f);
                r.offsetMax = new Vector2(-60f, 0f);
            }

            if (prefab.adImage != null)
            {
                var icon = EnsureIcon(prefab.adImage.transform);
                if (icon != null)
                {
                    var s =
                        (hasKit ? LoopSortingUIKit.LoadSprite("UI_Sprites/icon_video.png", 100f, applyNineSlice: false) : null) ??
                        TryLoadBoosterPurchaseSprite("icon_video");
                    icon.sprite = s;
                    icon.type = Image.Type.Simple;
                    icon.preserveAspect = true;
                    icon.color = s != null ? Color.white : new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        private Button CreateLongButton(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 size,
            string normal,
            string pressed,
            string disabled,
            string label,
            out TMP_Text labelText,
            bool reserveIconSpace = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            var img = go.AddComponent<Image>();
            var btn = go.AddComponent<Button>();
            ApplyUIKitButtonSprites(btn, img, normal, pressed, disabled);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = label;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 64;
            tmp.color = Color.white;
            var tRect = tmp.GetComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            if (reserveIconSpace)
            {
                tRect.offsetMin = new Vector2(160f, 0f);
                tRect.offsetMax = new Vector2(-60f, 0f);
            }
            else
            {
                tRect.offsetMin = new Vector2(0f, 0f);
                tRect.offsetMax = new Vector2(0f, 0f);
            }
            labelText = tmp;

            return btn;
        }

    }
}

