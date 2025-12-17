using UnityEngine;
using UnityEngine.UI;

namespace LoopSorting
{
    internal static class UIPrefabPreviewUtil
    {
        public static void ApplyNineSliceIfMissing(Image image, Sprite sprite)
        {
            if (image == null) return;
            if (image.sprite != null) return;
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = false;
        }

        public static void ApplySimpleIfMissing(Image image, Sprite sprite, bool preserveAspect = true)
        {
            if (image == null) return;
            if (image.sprite != null) return;
            if (sprite == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.preserveAspect = preserveAspect;
        }

        public static Image EnsureChildImage(Transform parent, string name, Sprite sprite)
        {
            if (parent == null || sprite == null) return null;
            var t = parent.Find(name);
            Image img = null;
            if (t != null) img = t.GetComponent<Image>();
            if (img == null)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                img = go.AddComponent<Image>();
                img.raycastTarget = false;
                var rect = go.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(64f, 64f);
            }
            img.sprite = sprite;
            img.type = sprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
            img.color = Color.white;
            img.preserveAspect = true;
            return img;
        }

        public static void EnsureToggleKnobIfMissing(Image trackImage, bool isOn)
        {
            if (trackImage == null) return;
            if (trackImage.sprite == null) return;

            var knobSprite = LoopSortingUIKit.LoadSpriteByKey("ui.toggle.knob");
            if (knobSprite == null) return;

            var knob = EnsureChildImage(trackImage.transform, "Knob", knobSprite);
            if (knob == null) return;

            var r = trackImage.rectTransform.rect;
            float w = Mathf.Max(1f, r.width);
            float h = Mathf.Max(1f, r.height);
            float knobSide = Mathf.Clamp(h * 0.85f, 8f, 9999f);

            var knobRect = knob.rectTransform;
            knobRect.sizeDelta = new Vector2(knobSide, knobSide);

            float margin = knobSide * 0.58f;
            float x = isOn ? (w * 0.5f - margin) : (-w * 0.5f + margin);
            knobRect.anchoredPosition = new Vector2(x, 0f);
        }
    }
}

