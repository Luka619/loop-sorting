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
        private Image CreateButtonIcon(Transform buttonTransform)
        {
            if (buttonTransform == null) return null;

            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(buttonTransform, false);
            var icon = iconGO.AddComponent<Image>();
            icon.raycastTarget = false;

            var rect = iconGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.18f, 0.5f);
            rect.anchorMax = new Vector2(0.18f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 0f);

            float side = 150f;
            var btnRect = buttonTransform.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                side = Mathf.Clamp(btnRect.rect.height * 0.72f, 110f, 150f);
            }
            rect.sizeDelta = new Vector2(side, side);

            return icon;
        }

        private void AttachBoosterBadge(Transform buttonTransform, int count)
        {
            if (buttonTransform == null) return;
            var existing = buttonTransform.Find("Badge");
            if (existing != null) return;

            count = Mathf.Clamp(count, 0, 99);

            float buttonSize = 420f;
            var btnRect = buttonTransform.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                buttonSize = Mathf.Max(200f, Mathf.Min(btnRect.rect.width, btnRect.rect.height));
            }
            float badgeSize = Mathf.Clamp(buttonSize * 0.34f, 110f, 140f);

            var badgeGO = new GameObject("Badge");
            badgeGO.transform.SetParent(buttonTransform, false);
            var badgeRect = badgeGO.AddComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1f, 0f);
            badgeRect.anchorMax = new Vector2(1f, 0f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(-badgeSize * 0.15f, badgeSize * 0.28f);
            badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);

            var bgGO = new GameObject("BadgeBG");
            bgGO.transform.SetParent(badgeGO.transform, false);
            var bg = bgGO.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.sprite = LoopSortingUIKit.LoadSpriteByKey("ui.badge.bg");
            bg.color = Color.white;
            var bgRect = bgGO.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.5f, 0.5f);
            bgRect.anchorMax = new Vector2(0.5f, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(badgeSize * 0.86f, badgeSize * 0.86f);

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(badgeGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.raycastTarget = false;
            tmp.text = count.ToString();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.color = Color.white;
            tmp.fontSize = Mathf.Clamp(badgeSize * 0.58f, 36f, 72f);
            ApplyTmpOutlineUnderlay(
                tmp,
                outlineWidth: 0.20f,
                outlineColor: new Color(0.10f, 0.06f, 0.04f, 1f),
                underlayColor: new Color(0f, 0f, 0f, 0.35f),
                underlayOffset: new Vector2(2f, -2f),
                underlaySoftness: 0.28f,
                underlayDilate: 0.02f);

            var textRect = tmp.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10f, 6f);
            textRect.offsetMax = new Vector2(-10f, -6f);
        }

        private Button CreateBoosterButton(
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
                float iconSide = Mathf.Clamp(Mathf.Min(size.x, size.y) * 0.42f, 120f, 190f);
                float iconY = Mathf.Clamp(size.y * 0.10f, 26f, 40f);
                iconRect.anchoredPosition = new Vector2(0f, iconY);
                iconRect.sizeDelta = new Vector2(iconSide, iconSide);
            }

            return btn;
        }

        private Toggle CreateToggleRow(
            Transform parent,
            string name,
            Vector2 anchor,
            Vector2 anchoredPos,
            string label,
            string icon,
            bool initial,
            out Image toggleImage)
        {
            bool hasKit = LoopSortingUIKit.IsAvailable();

            var rowGO = new GameObject(name);
            rowGO.transform.SetParent(parent, false);
            var rowRect = rowGO.AddComponent<RectTransform>();
            rowRect.anchorMin = anchor;
            rowRect.anchorMax = anchor;
            rowRect.pivot = new Vector2(0.5f, 0.5f);
            rowRect.anchoredPosition = anchoredPos;
            rowRect.sizeDelta = new Vector2(820f, 160f);

            var rowBg = rowGO.AddComponent<Image>();
            rowBg.raycastTarget = true;
            if (hasKit)
            {
                var bgSprite = LoopSortingUIKit.LoadSpriteByKey("ui.card.setting_row");
                if (bgSprite != null)
                {
                    rowBg.sprite = bgSprite;
                    rowBg.type = bgSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                    rowBg.color = Color.white;
                }
                else
                {
                    rowBg.color = new Color(1f, 1f, 1f, 0.9f);
                }
            }
            else
            {
                rowBg.color = new Color(1f, 1f, 1f, 0.9f);
            }

            var labelColor = new Color(0.35f, 0.22f, 0.12f, 1f);

            float labelStartX = 70f;

            if (!string.IsNullOrEmpty(icon) && hasKit)
            {
                var iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(rowGO.transform, false);
                var iconImg = iconGO.AddComponent<Image>();
                iconImg.raycastTarget = false;
                iconImg.sprite = LoopSortingUIKit.LoadSpriteByKey(icon);
                iconImg.color = labelColor;
                var iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0f, 0.5f);
                iconRect.anchorMax = new Vector2(0f, 0.5f);
                iconRect.pivot = new Vector2(0f, 0.5f);
                iconRect.anchoredPosition = new Vector2(54f, 0f);
                iconRect.sizeDelta = new Vector2(96f, 96f);

                labelStartX = 170f;
            }

            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(rowGO.transform, false);
            var labelText = labelGO.AddComponent<TextMeshProUGUI>();
            labelText.raycastTarget = false;
            labelText.text = label;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
            labelText.fontSize = 64;
            labelText.color = labelColor;
            labelText.enableWordWrapping = true;
            labelText.overflowMode = TextOverflowModes.Overflow;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(labelStartX, 0f);
            labelRect.sizeDelta = new Vector2(420f, 120f);

            var toggleGO = new GameObject("Toggle");
            toggleGO.transform.SetParent(rowGO.transform, false);
            var toggleRect = toggleGO.AddComponent<RectTransform>();
            toggleRect.anchorMin = new Vector2(1f, 0.5f);
            toggleRect.anchorMax = new Vector2(1f, 0.5f);
            toggleRect.pivot = new Vector2(1f, 0.5f);
            toggleRect.anchoredPosition = new Vector2(-54f, 0f);
            toggleRect.sizeDelta = new Vector2(300f, 110f);

            toggleImage = toggleGO.AddComponent<Image>();
            toggleImage.raycastTarget = true;
            toggleImage.preserveAspect = true;

            var toggle = toggleGO.AddComponent<Toggle>();
            toggle.isOn = initial;
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = toggleImage;
            toggle.graphic = toggleImage;

            var rowBtn = rowGO.AddComponent<Button>();
            rowBtn.transition = Selectable.Transition.None;
            rowBtn.onClick.AddListener(() => toggle.isOn = !toggle.isOn);
            ApplyButtonPressScale(rowBtn, pressedScale: 0.98f);

            UpdateToggleVisual(toggleImage, initial);
            return toggle;
        }

	        private void UpdateToggleVisual(Image toggleImage, bool isOn)
	        {
	            if (toggleImage == null) return;

            if (LoopSortingUIKit.IsAvailable())
            {
                var track = LoopSortingUIKit.LoadSpriteByKey(isOn ? "ui.toggle.track_on" : "ui.toggle.track_off");
                var knobSprite = LoopSortingUIKit.LoadSpriteByKey("ui.toggle.knob");
                if (track != null && knobSprite != null)
                {
                    toggleImage.sprite = track;
                    toggleImage.type = Image.Type.Simple;
                    toggleImage.preserveAspect = true;
                    toggleImage.color = Color.white;

                    var knobImg = EnsureToggleKnobImage(toggleImage, knobSprite);
                    LayoutSplitToggle(toggleImage.rectTransform, knobImg.rectTransform, isOn);
                    return;
                }

                var fallback = LoopSortingUIKit.LoadSpriteByKey(isOn ? "ui.toggle.full_on" : "ui.toggle.full_off");
                if (fallback != null)
                {
                    toggleImage.sprite = fallback;
                    toggleImage.color = Color.white;
                    return;
                }
            }

	            toggleImage.color = isOn ? new Color(0.2f, 0.75f, 0.2f, 1f) : new Color(0.6f, 0.6f, 0.6f, 1f);
	        }

        private void BuildSlotMarkers()
        {
            // Clean previous markers
            foreach (var m in _slotMarkers)
            {
                if (m != null) Destroy(m);
            }
            _slotMarkers.Clear();
            _slotBasePositions.Clear();
            _slotCurrentPositions.Clear();

            if (_beltSlots == null || _beltSlots.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _beltSlots.Count; i++)
            {
                var slot = _beltSlots[i];
                var pos = slot != null ? slot.position : transform.position;
                _slotBasePositions.Add(pos);
                SetSlotCurrent(i, pos);
            }

            if (!showSlotMarkersRuntime)
            {
                return;
            }

            var parent = new GameObject("SlotMarkers");
            parent.transform.SetParent(transform, false);

            var cam = Camera.main;
            var markerRotation = cam != null ? cam.transform.rotation : Quaternion.identity;

            bool hasKit = LoopSortingUIKit.IsAvailable();
            var slotTex = hasKit ? LoopSortingUIKit.LoadTextureByKey("world.conveyor_slot") : null;

            float spacing = _beltSpacingUsed > 0.0001f ? _beltSpacingUsed : beltSlotSpacing;
            float aspect = slotTex != null && slotTex.width > 0 ? (float)slotTex.height / slotTex.width : 1f;
            float beltWidth = _beltWidthUsed > 0.0001f ? _beltWidthUsed : spacing;
            float baseSide = Mathf.Max(0.02f, spacing * Mathf.Max(0.01f, slotMarkerScale));
            // Ensure marker height is one-third of the belt width (keeps slots readable without overpowering the belt).
            float minSideForBelt = Mathf.Max(0.02f, (beltWidth / 3f) / Mathf.Max(0.01f, aspect));
            baseSide = Mathf.Max(baseSide, minSideForBelt);
            var scale = new Vector3(baseSide, baseSide * aspect, 1f);

            for (int i = 0; i < _slotBasePositions.Count; i++)
            {
                var marker = RuntimePrimitives.CreateQuad($"SlotMarker_{i}");
                marker.transform.SetParent(parent.transform, false);
                marker.transform.position = _slotBasePositions[i] + new Vector3(0f, 0f, 0.1f);
                marker.transform.rotation = ComputeSlotMarkerRotation(i, _slotBasePositions, _beltLoop, markerRotation);
                marker.transform.localScale = scale;

                var renderer = marker.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = null;
                    if (slotTex != null)
                    {
                        mat = LoopSortingUIKit.CreateUnlitTextureMaterial(slotTex, slotMarkerColor, 2900);
                    }

                    if (mat == null)
                    {
                        var shader =
                            Shader.Find("LoopSorting/UnlitTexture") ??
                            Shader.Find("Unlit/Transparent Colored") ??
                            Shader.Find("Unlit/Texture") ??
                            Shader.Find("Sprites/Default") ??
                            Shader.Find("UI/Default") ??
                            Shader.Find("Standard");
                        if (shader != null)
                        {
                            mat = new Material(shader);
                            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", slotTex != null ? slotTex : Texture2D.whiteTexture);
                            else mat.mainTexture = slotTex != null ? slotTex : Texture2D.whiteTexture;
                            if (mat.HasProperty("_Color")) mat.SetColor("_Color", slotMarkerColor);
                            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", slotMarkerColor);
                            else if (mat.HasProperty("_TintColor")) mat.SetColor("_TintColor", slotMarkerColor);
                            else mat.color = slotMarkerColor;
                            mat.renderQueue = 2900;
                            if (mat.HasProperty("_ZWrite")) mat.SetInt("_ZWrite", 0);
                            if (mat.HasProperty("_ZTest")) mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
                            if (mat.HasProperty("_Cull")) mat.SetInt("_Cull", 0);
                            if (mat.HasProperty("_CullMode")) mat.SetInt("_CullMode", 0);
                        }
                    }

                    if (mat != null)
                    {
                        renderer.sharedMaterial = mat;
                    }
                }

                _slotMarkers.Add(marker);
            }
        }

    }
}

