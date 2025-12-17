using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class SettingsPanelPrefabRefs : MonoBehaviour
    {
        public RectTransform popupRect;
        public Button closeButton;
        public Image closeImage;

        public Button musicToggleButton;
        public Image musicToggleImage;
        public Button sfxToggleButton;
        public Image sfxToggleImage;
        public Button vibrationToggleButton;
        public Image vibrationToggleImage;

        public Button retryButton;
        public Image retryImage;
        public TMP_Text retryLabel;

        public void AutoAssign()
        {
            popupRect = popupRect != null ? popupRect : FindRect("Popup");

            closeButton = closeButton != null ? closeButton : Find<Button>("CloseButton");
            closeImage = closeImage != null ? closeImage : (closeButton != null ? closeButton.GetComponent<Image>() : Find<Image>("CloseButton"));

            FindToggleRow("MUSICRow", ref musicToggleButton, ref musicToggleImage);
            FindToggleRow("SFXRow", ref sfxToggleButton, ref sfxToggleImage);
            FindToggleRow("VIBRATIONRow", ref vibrationToggleButton, ref vibrationToggleImage);

            retryButton = retryButton != null ? retryButton : Find<Button>("RetryButton");
            retryImage = retryImage != null ? retryImage : (retryButton != null ? retryButton.GetComponent<Image>() : Find<Image>("RetryButton"));
            retryLabel = retryLabel != null ? retryLabel : Find<TMP_Text>("Text");
        }

        private void FindToggleRow(string rowName, ref Button button, ref Image image)
        {
            if (button != null && image != null) return;
            var row = FindTransform(rowName);
            if (row == null) return;
            var toggle = row.Find("Toggle");
            if (toggle == null) return;
            button = button != null ? button : toggle.GetComponent<Button>();
            image = image != null ? image : toggle.GetComponent<Image>();
        }

        private RectTransform FindRect(string name)
        {
            var t = FindTransform(name);
            return t != null ? t.GetComponent<RectTransform>() : null;
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

        private void EditorPreviewRebindSprites()
        {
            if (!LoopSortingUIKit.IsAvailable()) return;

            var popupImg = popupRect != null ? popupRect.GetComponent<Image>() : null;
            UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                popupImg,
                LoopSortingUIKit.LoadSprite("UI_Sprites/panel_modal_base_9slice.png") ?? LoopSortingUIKit.LoadSpriteByKey("ui.panel_modal"));

            if (closeImage != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(closeImage, LoopSortingUIKit.LoadSpriteByKey("ui.button.close_red.normal"), preserveAspect: true);
                var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.close");
                if (iconSprite != null)
                {
                    var icon = UIPrefabPreviewUtil.EnsureChildImage(closeImage.transform, "Icon", iconSprite);
                    if (icon != null)
                    {
                        float side = Mathf.Min(closeImage.rectTransform.rect.width, closeImage.rectTransform.rect.height) * 0.62f;
                        icon.rectTransform.sizeDelta = new Vector2(side, side);
                    }
                }
            }

            if (retryImage != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(retryImage, LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal"));
            }

            void RebindToggle(Image img)
            {
                if (img == null) return;
                UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.toggle.track_off"), preserveAspect: true);
                UIPrefabPreviewUtil.EnsureToggleKnobIfMissing(img, isOn: false);
            }

            RebindToggle(musicToggleImage);
            RebindToggle(sfxToggleImage);
            RebindToggle(vibrationToggleImage);
        }
#endif
    }
}
