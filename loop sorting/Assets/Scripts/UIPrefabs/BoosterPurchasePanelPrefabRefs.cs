using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class BoosterPurchasePanelPrefabRefs : MonoBehaviour
    {
        public RectTransform popupRect;
        public RectTransform headerRect;
        public RectTransform iconRect;
        public RectTransform closeRect;
        public RectTransform subtitleRect;
        public RectTransform coinsRect;
        public RectTransform adRect;

        public Button closeButton;
        public Image closeImage;
        public Button coinsButton;
        public Image coinsImage;
        public TMP_Text coinsLabel;
        public Image coinsPriceCover;
        public Button adButton;
        public Image adImage;
        public TMP_Text adLabel;

        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public Image background;
        public Image header;
        public Image icon;
        public Image subtitleBg;

        public void AutoAssign()
        {
            popupRect = popupRect != null ? popupRect : FindRect("Popup");
            headerRect = headerRect != null ? headerRect : FindRect("Header");
            iconRect = iconRect != null ? iconRect : FindRect("BoosterIcon");
            closeRect = closeRect != null ? closeRect : FindRect("CloseButton");
            subtitleRect = subtitleRect != null ? subtitleRect : FindRect("Subtitle");
            coinsRect = coinsRect != null ? coinsRect : FindRect("BuyWithCoins");
            adRect = adRect != null ? adRect : FindRect("BuyWithAd");

            closeButton = closeButton != null ? closeButton : Find<Button>("CloseButton");
            closeImage = closeImage != null ? closeImage : (closeRect != null ? closeRect.GetComponent<Image>() : Find<Image>("CloseButton"));

            coinsButton = coinsButton != null ? coinsButton : Find<Button>("BuyWithCoins");
            coinsImage = coinsImage != null ? coinsImage : (coinsButton != null ? coinsButton.GetComponent<Image>() : Find<Image>("BuyWithCoins"));
            coinsLabel = coinsLabel != null ? coinsLabel : FindLabelUnder("BuyWithCoins");
            coinsPriceCover = coinsPriceCover != null ? coinsPriceCover : Find<Image>("PriceCover");

            adButton = adButton != null ? adButton : Find<Button>("BuyWithAd");
            adImage = adImage != null ? adImage : (adButton != null ? adButton.GetComponent<Image>() : Find<Image>("BuyWithAd"));
            adLabel = adLabel != null ? adLabel : FindLabelUnder("BuyWithAd");

            titleText = titleText != null ? titleText : Find<TMP_Text>("TitleText");
            subtitleText = subtitleText != null ? subtitleText : Find<TMP_Text>("SubtitleText");

            background = background != null ? background : Find<Image>("Popup");
            header = header != null ? header : Find<Image>("Header");
            icon = icon != null ? icon : Find<Image>("BoosterIcon");
            subtitleBg = subtitleBg != null ? subtitleBg : Find<Image>("BG");
        }

        private RectTransform FindRect(string name)
        {
            var t = FindTransform(name);
            return t != null ? t.GetComponent<RectTransform>() : null;
        }

        private TMP_Text FindLabelUnder(string rootName)
        {
            var root = FindTransform(rootName);
            if (root == null) return null;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t != null && t.name == "Label") return t;
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

            if (popupRect != null)
            {
                var img = popupRect.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                    img,
                    LoopSortingUIKit.LoadSprite("UI_Sprites/panel_modal_base_9slice.png") ?? LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal"));
            }

            if (headerRect != null)
            {
                var img = headerRect.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal"));
            }

            if (subtitleBg != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(subtitleBg, LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info"));
            }

            if (closeImage != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(closeImage, LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal"), preserveAspect: true);
            }
            if (coinsImage != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(coinsImage, LoopSortingUIKit.LoadSpriteByKey("ui.button.price_green.normal"));
            }
            if (adImage != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(adImage, LoopSortingUIKit.LoadSpriteByKey("ui.button.mint_long.normal"));
            }
        }
#endif
    }
}
