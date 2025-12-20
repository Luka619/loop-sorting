using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    [DisallowMultipleComponent]
    public sealed class GameplayHudPrefabRefs : MonoBehaviour
    {
        [Header("Authoring Insets (units)")]
        [Tooltip("Top inset already baked into this prefab's layout (used to avoid double-applying safe-area offsets).")]
        public float authoredTopInsetUnits = 0f;
        [Tooltip("Right inset already baked into this prefab's layout (used to avoid double-applying safe-area offsets).")]
        public float authoredRightInsetUnits = 0f;
        [Tooltip("Bottom inset already baked into this prefab's layout (used to avoid double-applying safe-area offsets).")]
        public float authoredBottomInsetUnits = 0f;

        public RectTransform rootRect;

        [Header("Top HUD")]
        public BeltCounterUI beltCounterUI;
        public TMP_Text levelText;
        public Button shopButton;

        [Header("Currency")]
        public TMP_Text coinText;
        public Button coinPlusButton;
        public TMP_Text lifeText;
        public Button lifePlusButton;

        [Header("Buttons")]
        public Button speedButton;
        public TMP_Text speedLabel;
        public Button settingsButton;

        [Header("Boosters")]
        public GameObject boosterPanel;
        public Button boosterSortButton;
        public Button boosterShuffleButton;

        public void AutoAssign()
        {
            rootRect = rootRect != null ? rootRect : GetComponent<RectTransform>();

            beltCounterUI = beltCounterUI != null ? beltCounterUI : FindInChildren<BeltCounterUI>("Value");
            levelText = levelText != null ? levelText : FindLabelUnder("LevelLabel", "Text");
            shopButton = shopButton != null ? shopButton : Find<Button>("ShopButton");

            coinText = coinText != null ? coinText : FindLabelUnder("CoinsPill", "Value");
            coinPlusButton = coinPlusButton != null ? coinPlusButton : Find<Button>("CoinsPill/Plus");
            lifeText = lifeText != null ? lifeText : FindLabelUnder("LivesPill", "Value");
            lifePlusButton = lifePlusButton != null ? lifePlusButton : Find<Button>("LivesPill/Plus");

            speedButton = speedButton != null ? speedButton : Find<Button>("SpeedButton");
            speedLabel = speedLabel != null ? speedLabel : FindLabelUnder("SpeedButton", "Label");
            settingsButton = settingsButton != null ? settingsButton : Find<Button>("SettingsButton");

            boosterPanel = boosterPanel != null ? boosterPanel : FindTransform("BoosterPanel")?.gameObject;
            boosterSortButton = boosterSortButton != null ? boosterSortButton : Find<Button>("BoosterSort");
            boosterShuffleButton = boosterShuffleButton != null ? boosterShuffleButton : Find<Button>("BoosterShuffle");
        }

        private TMP_Text FindLabelUnder(string rootName, string labelName)
        {
            var root = FindTransform(rootName);
            if (root == null) return null;
            foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            {
                if (t != null && t.name == labelName) return t;
            }
            return null;
        }

        private T FindInChildren<T>(string name) where T : Component
        {
            foreach (var c in GetComponentsInChildren<T>(true))
            {
                if (c != null && c.name == name) return c;
            }
            return null;
        }

        private T Find<T>(string pathOrName) where T : Component
        {
            var t = FindTransform(pathOrName);
            return t != null ? t.GetComponent<T>() : null;
        }

        private Transform FindTransform(string pathOrName)
        {
            if (string.IsNullOrEmpty(pathOrName)) return null;

            // Support simple paths like "CoinsPill/Plus".
            var direct = transform.Find(pathOrName);
            if (direct != null) return direct;

            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == pathOrName) return t;
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

            // Counter BG + icon
            if (beltCounterUI != null)
            {
                var bg = beltCounterUI.transform.parent != null ? beltCounterUI.transform.parent.GetComponent<Image>() : null;
                if (bg != null)
                {
                    UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                        bg,
                        LoopSortingUIKit.LoadSprite("UI_Sprites/hud_pill_dark_small_base_9slice.png") ??
                        LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg"));
                }

                var icon = beltCounterUI.transform.parent != null ? beltCounterUI.transform.parent.Find("Icon") : null;
                if (icon != null)
                {
                    var img = icon.GetComponent<Image>();
                    if (img != null)
                    {
                        UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey("ui.counter.icon"), preserveAspect: true);
                    }
                }
            }

            // Level BG
            var levelLabel = FindTransform("LevelLabel");
            var levelBg = levelLabel != null ? levelLabel.GetComponent<Image>() : null;
            if (levelBg != null)
            {
                UIPrefabPreviewUtil.ApplyNineSliceIfMissing(levelBg, LoopSortingUIKit.LoadSpriteByKey("ui.hud.level_bg"));
            }

            // Shop/settings/speed/boosters buttons
            void RebindButton(Button b, string normalKey)
            {
                if (b == null) return;
                var img = b.GetComponent<Image>();
                if (img != null)
                {
                    UIPrefabPreviewUtil.ApplyNineSliceIfMissing(img, LoopSortingUIKit.LoadSpriteByKey(normalKey));
                }
            }

            RebindButton(shopButton, "ui.button.mint_square.normal");
            RebindButton(speedButton, "ui.button.mint_square.normal");
            RebindButton(settingsButton, "ui.button.mint_square.normal");
            RebindButton(boosterSortButton, "ui.button.mint_square.normal");
            RebindButton(boosterShuffleButton, "ui.button.purple_square.normal");
            RebindButton(coinPlusButton, "ui.button.mint_square.normal");
            RebindButton(lifePlusButton, "ui.button.mint_square.normal");

            void RebindIcon(Transform buttonRoot, string iconKey)
            {
                if (buttonRoot == null) return;
                var icon = buttonRoot.Find("Icon");
                if (icon == null) return;
                var img = icon.GetComponent<Image>();
                if (img != null)
                {
                    UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey(iconKey), preserveAspect: true);
                }
            }

            RebindIcon(shopButton != null ? shopButton.transform : null, "ui.icon.shop");
            RebindIcon(settingsButton != null ? settingsButton.transform : null, "ui.icon.gear");
            RebindIcon(boosterSortButton != null ? boosterSortButton.transform : null, "ui.icon.sort");
            RebindIcon(boosterShuffleButton != null ? boosterShuffleButton.transform : null, "ui.icon.shuffle");
            RebindIcon(coinPlusButton != null ? coinPlusButton.transform : null, "ui.icon.plus");
            RebindIcon(lifePlusButton != null ? lifePlusButton.transform : null, "ui.icon.plus");

            void RebindBadge(Button b)
            {
                if (b == null) return;
                var bg = b.transform.Find("Badge/BadgeBG")?.GetComponent<Image>();
                if (bg != null)
                {
                    UIPrefabPreviewUtil.ApplySimpleIfMissing(bg, LoopSortingUIKit.LoadSpriteByKey("ui.badge.bg"), preserveAspect: true);
                }
            }

            RebindBadge(boosterSortButton);
            RebindBadge(boosterShuffleButton);

            // Currency pill BG + icon
            void RebindPill(TMP_Text valueText, string iconKey)
            {
                if (valueText == null) return;
                var root = valueText.transform.parent;
                if (root == null) return;
                var bg = root.GetComponent<Image>();
                if (bg != null)
                {
                    UIPrefabPreviewUtil.ApplyNineSliceIfMissing(
                        bg,
                        LoopSortingUIKit.LoadSprite("UI_Sprites/hud_pill_dark_small_base_9slice.png") ??
                        LoopSortingUIKit.LoadSpriteByKey("ui.counter.bg"));
                }

                var icon = root.Find("Icon");
                if (icon != null)
                {
                    var img = icon.GetComponent<Image>();
                    if (img != null)
                    {
                        UIPrefabPreviewUtil.ApplySimpleIfMissing(img, LoopSortingUIKit.LoadSpriteByKey(iconKey), preserveAspect: true);
                    }
                }
            }

            RebindPill(coinText, "ui.icon.coin");
            RebindPill(lifeText, "ui.icon.heart");
        }
#endif
    }
}
