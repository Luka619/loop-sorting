using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class ResultPanelPrefabRefs : MonoBehaviour
    {
        public TMP_Text resultText;
        public Button primaryButton;
        public TMP_Text primaryLabel;
        public Image primaryIcon;
        public Button secondaryButton;
        public TMP_Text secondaryLabel;
        public Image secondaryIcon;

        public void AutoAssign()
        {
            resultText = resultText != null ? resultText : Find<TMP_Text>("Title");

            primaryButton = primaryButton != null ? primaryButton : Find<Button>("PrimaryButton");
            primaryLabel = primaryLabel != null ? primaryLabel : FindLabelUnder("PrimaryButton");
            primaryIcon = primaryIcon != null ? primaryIcon : FindIconUnder("PrimaryButton");

            secondaryButton = secondaryButton != null ? secondaryButton : Find<Button>("SecondaryButton");
            secondaryLabel = secondaryLabel != null ? secondaryLabel : FindLabelUnder("SecondaryButton");
            secondaryIcon = secondaryIcon != null ? secondaryIcon : FindIconUnder("SecondaryButton");
        }

        private TMP_Text FindLabelUnder(string rootName)
        {
            var root = FindTransform(rootName);
            if (root == null) return null;
            var t = root.Find("Text");
            return t != null ? t.GetComponent<TMP_Text>() : null;
        }

        private Image FindIconUnder(string rootName)
        {
            var root = FindTransform(rootName);
            if (root == null) return null;
            var t = root.Find("Icon");
            return t != null ? t.GetComponent<Image>() : null;
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

            var panel = transform.Find("Panel");
            if (panel != null)
            {
                var img = panel.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                    img,
                    LoopSortingUIKit.LoadSprite("UI_Sprites/panel_result_base_9slice.png") ?? LoopSortingUIKit.LoadSpriteByKey("ui.panel_result"));
            }

            var banner = transform.Find("Panel/Banner") ?? transform.Find("Banner");
            if (banner != null)
            {
                var img = banner.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.tag_fast.info"));
            }

            if (primaryButton != null)
            {
                var img = primaryButton.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.mint_long.normal"));
            }
            if (secondaryButton != null)
            {
                var img = secondaryButton.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal"));
            }

            if (primaryIcon != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(primaryIcon, LoopSortingUIKit.LoadSpriteByKey("ui.icon.next"), preserveAspect: true);
            }
            if (secondaryIcon != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(secondaryIcon, LoopSortingUIKit.LoadSpriteByKey("ui.icon.retry"), preserveAspect: true);
            }
        }
#endif
    }
}
