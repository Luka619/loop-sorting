using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class MainMenuCanvasPrefabRefs : MonoBehaviour
    {
        public RectTransform safeAreaRect;
        public Image backgroundImage;
        public Button settingsButton;
        public Button playButton;
        public TMP_Text playText;
        public Image titleImage;
        public TMP_Text titleText;
        public Image levelPillBackground;
        public TMP_Text levelPillText;

        public void AutoAssign()
        {
            safeAreaRect = safeAreaRect != null ? safeAreaRect : Find<RectTransform>("SafeArea");
            backgroundImage = backgroundImage != null ? backgroundImage : Find<Image>("BG");
            settingsButton = settingsButton != null ? settingsButton : Find<Button>("SettingsButton");
            playButton = playButton != null ? playButton : Find<Button>("PlayButton");

            if (playText == null)
            {
                var t = FindTransform("PlayButton")?.Find("Text");
                playText = t != null ? t.GetComponent<TMP_Text>() : null;
            }

            titleImage = titleImage != null ? titleImage : Find<Image>("Title");
            if (titleText == null)
            {
                titleText = Find<TMP_Text>("Title");
            }

            levelPillBackground = levelPillBackground != null ? levelPillBackground : Find<Image>("LevelPill");
            if (levelPillText == null)
            {
                var t = FindTransform("LevelPill")?.Find("Text");
                levelPillText = t != null ? t.GetComponent<TMP_Text>() : null;
            }
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

            if (backgroundImage != null)
            {
                UIPrefabPreviewUtil.ApplySimpleIfMissing(backgroundImage, LoopSortingUIKit.LoadSpriteByKey("ui.bg_main"), preserveAspect: false);
            }

            if (settingsButton != null)
            {
                var img = settingsButton.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.mint_square.normal"));
                if (img != null)
                {
                    var iconSprite = LoopSortingUIKit.LoadSpriteByKey("ui.icon.gear");
                    if (iconSprite != null)
                    {
                        var icon = UIPrefabPreviewUtil.EnsureChildImage(img.transform, "Icon", iconSprite);
                        if (icon != null) icon.preserveAspect = true;
                    }
                }
            }

            if (playButton != null)
            {
                var img = playButton.GetComponent<Image>();
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.button.orange_long.normal"));
            }

            if (titleImage != null)
            {
                var s =
                    LoopSortingUIKit.LoadSpriteByKey("ui.title.main") ??
                    LoopSortingUIKit.LoadSprite("UI_Sprites/title_fangkuai_zhuan_bu_ting.png", pixelsPerUnit: 100f, applyNineSlice: false);
                UIPrefabPreviewUtil.ApplySimpleIfMissing(titleImage, s, preserveAspect: true);
            }

            if (levelPillBackground != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(levelPillBackground, LoopSortingUIKit.LoadSpriteByKey("ui.tag_small.info"));
            }
        }
#endif
    }
}

