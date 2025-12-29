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
        private void HideUiPanelImmediate(GameObject panel)
        {
            _uiModalService.HideImmediate(this, panel, OnModalPanelHidden);
        }

        private static RectTransform FindRectTransformByName(Transform root, string name)
        {
            if (root == null || string.IsNullOrEmpty(name)) return null;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t != null && t.name == name) return t as RectTransform;
            }
            return null;
        }

        private static Image EnsureToggleKnobImage(Image trackImage, Sprite knobSprite)
        {
            if (trackImage == null) return null;
            if (knobSprite == null) return null;

            var existing = trackImage.transform.Find("Knob");
            Image knobImg = null;
            if (existing != null)
            {
                knobImg = existing.GetComponent<Image>();
            }
            if (knobImg == null)
            {
                var knobGO = new GameObject("Knob");
                knobGO.transform.SetParent(trackImage.transform, false);
                knobImg = knobGO.AddComponent<Image>();
                knobImg.raycastTarget = false;
                var knobRect = knobGO.GetComponent<RectTransform>();
                knobRect.anchorMin = new Vector2(0.5f, 0.5f);
                knobRect.anchorMax = new Vector2(0.5f, 0.5f);
                knobRect.pivot = new Vector2(0.5f, 0.5f);
                knobRect.anchoredPosition = Vector2.zero;
            }

            knobImg.sprite = knobSprite;
            knobImg.type = Image.Type.Simple;
            knobImg.preserveAspect = true;
            knobImg.color = Color.white;
            return knobImg;
        }

        private static void LayoutSplitToggle(RectTransform rootRect, RectTransform knobRect, bool isOn)
        {
            if (rootRect == null || knobRect == null) return;
            var r = rootRect.rect;
            float w = Mathf.Max(1f, r.width);
            float h = Mathf.Max(1f, r.height);

            float knobSide = Mathf.Clamp(h * 0.85f, 8f, 9999f);
            knobRect.sizeDelta = new Vector2(knobSide, knobSide);

            float margin = knobSide * 0.58f;
            float x = isOn ? (w * 0.5f - margin) : (-w * 0.5f + margin);
            knobRect.anchoredPosition = new Vector2(x, 0f);
        }

        private static Image EnsureOverlayImage(Transform parent, string name, Sprite sprite)
        {
            if (parent == null || sprite == null) return null;
            var existing = parent.Find(name);
            Image img = null;
            if (existing != null) img = existing.GetComponent<Image>();
            if (img == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                img = go.AddComponent<Image>();
                img.raycastTarget = false;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
            img.sprite = sprite;
            img.type = sprite.border.sqrMagnitude > 0.0001f ? Image.Type.Sliced : Image.Type.Simple;
            img.preserveAspect = false;
            img.color = Color.white;
            return img;
        }

        private static void ApplyFakeDecorShadow(Image image, float alpha = 0.16f, float yOffsetFrac = 0.012f)
        {
            if (image == null) return;

            float h = image.rectTransform != null ? image.rectTransform.sizeDelta.y : 0f;
            if (h <= 0.01f) h = 900f;
            float dy = -Mathf.Clamp(h * Mathf.Clamp(yOffsetFrac, 0f, 0.2f), 3f, 18f);

            var shadow = image.GetComponent<Shadow>();
            if (shadow == null) shadow = image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, Mathf.Clamp01(alpha));
            shadow.effectDistance = new Vector2(0f, dy);
            shadow.useGraphicAlpha = true;
        }

        private static Transform TryCreatePaddingTrimmedLayoutRoot(
            Transform parent,
            RectTransform panelRect,
            Sprite sprite,
            Vector2 desiredVisibleSizeUnits,
            float centerStretchFraction = 0.5f)
        {
            if (parent == null || panelRect == null || sprite == null) return parent;
            if (desiredVisibleSizeUnits.x <= 1f || desiredVisibleSizeUnits.y <= 1f) return parent;

            int wPx = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.width));
            int hPx = Mathf.Max(1, Mathf.RoundToInt(sprite.rect.height));

            // Borders are generated as: border = padding + visible * sideFraction,
            // where visible excludes transparent padding, and the center stretch region is the middle `centerStretchFraction`.
            // sideFraction = (1 - centerStretchFraction) / 2. Default centerStretchFraction=0.5 -> sideFraction=0.25.
            float center = Mathf.Clamp(centerStretchFraction, 0.1f, 0.9f);
            float sideFrac = (1f - center) * 0.5f;
            float denom = Mathf.Max(0.05f, 1f - (2f * sideFrac));

            float borderL = Mathf.Max(0f, sprite.border.x);
            float borderB = Mathf.Max(0f, sprite.border.y);
            float borderR = Mathf.Max(0f, sprite.border.z);
            float borderT = Mathf.Max(0f, sprite.border.w);

            float visibleWPx = (wPx - (borderL + borderR)) / denom;
            float visibleHPx = (hPx - (borderT + borderB)) / denom;
            visibleWPx = Mathf.Clamp(visibleWPx, 1f, wPx);
            visibleHPx = Mathf.Clamp(visibleHPx, 1f, hPx);

            float padL = borderL - (sideFrac * visibleWPx);
            float padR = borderR - (sideFrac * visibleWPx);
            float padT = borderT - (sideFrac * visibleHPx);
            float padB = borderB - (sideFrac * visibleHPx);

            padL = Mathf.Clamp(padL, 0f, wPx - 2f);
            padR = Mathf.Clamp(padR, 0f, wPx - 2f);
            padT = Mathf.Clamp(padT, 0f, hPx - 2f);
            padB = Mathf.Clamp(padB, 0f, hPx - 2f);

            float visibleFracX = Mathf.Clamp01(visibleWPx / wPx);
            float visibleFracY = Mathf.Clamp01(visibleHPx / hPx);
            if (visibleFracX <= 0.05f || visibleFracY <= 0.05f) return parent;

            panelRect.sizeDelta = new Vector2(desiredVisibleSizeUnits.x / visibleFracX, desiredVisibleSizeUnits.y / visibleFracY);

            float unitsPerPxX = panelRect.sizeDelta.x / wPx;
            float unitsPerPxY = panelRect.sizeDelta.y / hPx;

            var layoutRootGO = new GameObject("LayoutRoot");
            layoutRootGO.transform.SetParent(parent, false);
            var contentRect = layoutRootGO.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.pivot = new Vector2(0.5f, 0.5f);
            contentRect.offsetMin = new Vector2(padL * unitsPerPxX, padB * unitsPerPxY);
            contentRect.offsetMax = new Vector2(-padR * unitsPerPxX, -padT * unitsPerPxY);

            return layoutRootGO.transform;
        }

        private static float GetSpriteAspect(Sprite sprite)
        {
            if (sprite == null) return 1f;
            var r = sprite.rect;
            if (r.height <= 0.0001f) return 1f;
            return r.width / r.height;
        }

        private static void ApplySplitBackground(
            Image baseImage,
            Transform parent,
            string decorName,
            string basePath,
            string decorPath,
            Sprite fallbackSprite,
            Color noSpriteColor)
        {
            if (baseImage == null || parent == null) return;

            var baseSprite = !string.IsNullOrEmpty(basePath) ? (LoopSortingUIKit.LoadSprite(basePath) ?? fallbackSprite) : fallbackSprite;
            baseImage.sprite = baseSprite;
            baseImage.type = baseSprite != null && baseSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            baseImage.color = baseSprite != null ? Color.white : noSpriteColor;

            // Do not use authored decor overlays: many of them are mismatched aspect and warp when stretched.
            // Simulate a subtle drop shadow by reusing the base silhouette (via UI Shadow effect).
            var existingDecor = !string.IsNullOrEmpty(decorName) ? parent.Find(decorName) : null;
            if (existingDecor != null) existingDecor.gameObject.SetActive(false);
            ApplyFakeDecorShadow(baseImage);

            var existingTopLight = parent.Find("TopLightClip") ?? parent.Find("TopLightMask");
            if (existingTopLight != null)
            {
                if (Application.isPlaying) Destroy(existingTopLight.gameObject);
                else DestroyImmediate(existingTopLight.gameObject);
            }
        }
	
	        private void OnModalPanelShown(GameObject panel)
	        {
	            if (panel == null) return;
	            if (panel == _settingsPanel || panel == _shopPanel || panel == _boosterPurchasePanel)
	            {
	                _uiModalService.HoldPanel(panel);
	            }
	        }
	
	        private void OnModalPanelHidden(GameObject panel)
	        {
	            if (panel == null) return;
	            if (panel == _settingsPanel || panel == _shopPanel || panel == _boosterPurchasePanel)
	            {
	                _uiModalService.ReleasePanel(panel);
	            }
            if (panel == _settingsPanel)
            {
                SettingsUi.OnHidden();
            }
            else if (panel == _boosterPurchasePanel)
            {
                StopBoosterPurchaseEffects();
            }
            else if (panel == _resultPanel)
            {
                StopResultWinFireworks();
                StopResultLoseCardIconIdle();
                StopResultLoseTitleShake();
                StopResultLoseParticles();
            }
	        }

	        private void AnimateUiPanel(GameObject panel, bool show, float seconds = 0.18f)
	        {
	            _uiModalService.AnimatePanel(this, panel, show, seconds, OnModalPanelShown, OnModalPanelHidden);
	        }

        private static float ComputeCanvasScaleFactor(CanvasScaler scaler)
        {
            if (scaler == null) return 1f;
            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) return 1f;

            float refW = Mathf.Max(1f, scaler.referenceResolution.x);
            float refH = Mathf.Max(1f, scaler.referenceResolution.y);
            float sw = Mathf.Max(1f, Screen.width);
            float sh = Mathf.Max(1f, Screen.height);

            float widthScale = sw / refW;
            float heightScale = sh / refH;
            float m = Mathf.Clamp01(scaler.matchWidthOrHeight);

            // Unity's CanvasScaler uses a logarithmic lerp between the two scales.
            return Mathf.Pow(widthScale, 1f - m) * Mathf.Pow(heightScale, m);
        }

    }
}


