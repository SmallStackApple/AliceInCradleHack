using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public static class ImGuiRenderUtil
    {
        public const float CornerRadius = 12f;

        public static float BackgroundOpacity { get; set; } = 0.16f;

        public static float FontSize { get; set; } = 14f;

        private const int BackgroundTextureSize = 32;

        private static Texture2D _backgroundTexture;
        private static GUIStyle _backgroundStyle;
        private static GUIStyle _labelStyle;

        private static GUIStyle BackgroundStyle
        {
            get
            {
                if (_backgroundStyle == null)
                {
                    int radius = (int)CornerRadius;
                    _backgroundTexture = CreateRoundedRectTexture(BackgroundTextureSize, radius);
                    _backgroundStyle = new GUIStyle
                    {
                        border = new RectOffset(radius, radius, radius, radius),
                        normal = { background = _backgroundTexture }
                    };
                }
                return _backgroundStyle;
            }
        }

        private static GUIStyle LabelStyle
        {
            get
            {
                if (_labelStyle == null)
                {
                    _labelStyle = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        richText = true,
                        wordWrap = false,
                        normal = { textColor = Color.white }
                    };
                }
                _labelStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(FontSize));
                return _labelStyle;
            }
        }

        public static void DrawBackground(Rect rect)
        {
            var previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(BackgroundOpacity));
            GUI.Box(rect, GUIContent.none, BackgroundStyle);
            GUI.color = previous;
        }

        public static void DrawLabel(Rect rect, string text, float alpha)
        {
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            GUI.Label(rect, text, LabelStyle);
            GUI.color = previous;
        }

        public static float MeasureWidth(string text)
        {
            return LabelStyle.CalcSize(new GUIContent(text)).x;
        }

        private static Texture2D CreateRoundedRectTexture(int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float sx = x + 0.5f;
                    float sy = y + 0.5f;
                    float cx = Mathf.Clamp(sx, radius, size - radius);
                    float cy = Mathf.Clamp(sy, radius, size - radius);
                    float dx = sx - cx;
                    float dy = sy - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
