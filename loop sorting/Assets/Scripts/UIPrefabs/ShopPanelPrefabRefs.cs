using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class ShopPanelPrefabRefs : MonoBehaviour
    {
        public RectTransform panelRect;
        public TMP_Text title;
        public Button closeButton;
        public ScrollRect scroll;
        public RectTransform contentRoot;
        public TMP_Text coinValue;
        public TMP_Text lifeValue;
        public Image scrollFadeTop;
        public Image scrollFadeBottom;

        public void AutoAssign()
        {
            panelRect = panelRect != null ? panelRect : FindRect("Panel");
            title = title != null ? title : Find<TMP_Text>("Title");
            closeButton = closeButton != null ? closeButton : Find<Button>("CloseButton");
            scroll = scroll != null ? scroll : Find<ScrollRect>("ShopScrollList");
            if (contentRoot == null && scroll != null) contentRoot = scroll.content;

            coinValue = coinValue != null ? coinValue : FindValueTextUnder("Coins");
            lifeValue = lifeValue != null ? lifeValue : FindValueTextUnder("Hearts");

            scrollFadeTop = scrollFadeTop != null ? scrollFadeTop : Find<Image>("FadeTop");
            scrollFadeBottom = scrollFadeBottom != null ? scrollFadeBottom : Find<Image>("FadeBottom");
        }

        private RectTransform FindRect(string name)
        {
            var t = FindTransform(name);
            return t != null ? t.GetComponent<RectTransform>() : null;
        }

        private TMP_Text FindValueTextUnder(string rootName)
        {
            var root = FindTransform(rootName);
            if (root == null) return null;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t != null && t.name == "Value") return t;
            }
            return null;
        }

        private T Find<T>(string name) where T : Component
        {
            foreach (var c in GetComponentsInChildren<T>(true))
            {
                if (c != null && c.name == name) return c;
            }
            return null;
        }

        private Transform FindTransform(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name) return t;
            }
            return null;
        }

#if UNITY_EDITOR
        [ContextMenu("Editor Preview/Rebind Sprites")]
        private void EditorPreviewRebindSpritesMenu()
        {
            AutoAssign();
            EditorPreviewRebindSprites();
        }

        private void Reset() => AutoAssign();
        private void OnValidate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                AutoAssign();
                EditorPreviewRebindSprites();
            }
        }
#endif

#if UNITY_EDITOR
        private void EditorPreviewRebindSprites()
        {
            if (!LoopSortingUIKit.IsAvailable()) return;

            if (panelRect != null)
            {
                var img = panelRect.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                    img,
                    LoopSortingUIKit.LoadSprite("UI_Sprites/panel_gold_blue_base_9slice.png") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.panel_shop") ??
                    LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal"));
            }

            if (closeButton != null)
            {
                var img = closeButton.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal"), preserveAspect: true);
                if (img != null)
                {
                    var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                    if (iconSprite != null)
                    {
                        UIPrefabPreviewUtil.EnsureChildImage(img.transform, "Icon", iconSprite);
                    }
                }
            }

            if (scrollFadeTop != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(scrollFadeTop, LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_top.png", 100f, applyNineSlice: false));
            }
            if (scrollFadeBottom != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(scrollFadeBottom, LoopSortingUIKit.LoadSprite("UI_Sprites/shop_scroll_fade_bottom.png", 100f, applyNineSlice: false));
            }

            void RebindStrip(TMP_Text valueText, string iconKey)
            {
                if (valueText == null) return;
                var strip = valueText.transform.parent;
                if (strip == null) return;

                var bg = strip.GetComponent<Image>();
                if (bg != null)
                {
                    UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                        bg,
                        LoopSortingUIKit.LoadSprite("UI_Sprites/hud_pill_dark_small_base_9slice.png") ?? LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg"));
                }

                var icon = strip.Find("Icon");
                if (icon != null)
                {
                    var img = icon.GetComponent<Image>();
                    if (img != null)
                    {
                        UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey(iconKey), preserveAspect: true);
                    }
                }
            }

            RebindStrip(lifeValue, "ui.icon.heart");
            RebindStrip(coinValue, "ui.icon.coin");
        }
#endif
    }
}
