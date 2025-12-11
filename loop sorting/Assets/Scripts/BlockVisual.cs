using UnityEngine;

namespace LoopSorting
{
    public static class BlockVisual
    {
        public static GameObject CreateBlock(BlockColor color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Block_{color}";
            go.transform.localScale = new Vector3(0.45f, 0.45f, 0.25f);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = ToUnityColor(color)
                };
            }

            return go;
        }

        public static Color ToUnityColor(BlockColor color)
        {
            switch (color)
            {
                case BlockColor.Red: return new Color(0.9f, 0.2f, 0.2f);
                case BlockColor.Blue: return new Color(0.2f, 0.4f, 0.9f);
                case BlockColor.Yellow: return new Color(0.98f, 0.8f, 0.15f);
                case BlockColor.Green: return new Color(0.25f, 0.8f, 0.35f);
                case BlockColor.Purple: return new Color(0.6f, 0.35f, 0.9f);
                case BlockColor.Orange: return new Color(1.0f, 0.6f, 0.2f);
                default: return Color.white;
            }
        }
    }
}
