using UnityEngine;

namespace LoopSorting
{
    [CreateAssetMenu(menuName = "LoopSorting/UI Theme", fileName = "UITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("Background")]
        public Color gradientTop = new Color(1f, 0.92f, 0.78f);
        public Color gradientBottom = new Color(1f, 0.87f, 0.65f);
        public Texture2D backgroundTexture;

        [Header("Fonts & Sprites")]
        public Font font;
        public Sprite buttonSprite;
        public Color buttonColor = new Color(0.2f, 0.2f, 0.2f, 0.85f);
        public Color buttonTextColor = Color.white;

        [Header("Text")]
        public int counterFontSize = 24;
        public Color counterColor = Color.white;
    }
}
