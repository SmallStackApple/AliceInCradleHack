using System.Collections.Generic;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public static class ImGuiRenderUtil
    {
        public static float BackgroundOpacity { get; set; } = 0.16f;

        public static Color BackgroundColor { get; set; } = Color.black;

        public static Color TextColor { get; set; } = Color.white;

        public static Color SubTextColor { get; set; } = new Color(0.75f, 0.75f, 0.75f);

        public static float FontSize { get; set; } = 14f;

        public static float SubFontSize { get; set; } = 10.5f;

        public static float TitleSubtitleSpacing { get; set; } = 0f;

        public static float ClientNameFontSize { get; set; } = 14f;

        private static readonly Dictionary<int, GUIStyle> BackgroundStyles = new();
        private static GUIStyle _labelStyle;
        private static GUIStyle _subLabelStyle;

        private static GUIStyle BackgroundStyle(int radius)
        {
            radius = Mathf.Max(1, radius);
            if (!BackgroundStyles.TryGetValue(radius, out var style))
            {
                var texture = CreateRoundedRectTexture(radius * 4, radius * 2, radius);
                style = new GUIStyle
                {
                    border = new RectOffset(radius, radius, radius, radius),
                    normal = { background = texture }
                };
                BackgroundStyles[radius] = style;
            }
            return style;
        }

        private static GUIStyle LabelStyle => LabelStyleFor(FontSize);

        private static GUIStyle LabelStyleFor(float fontSize)
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle
                {
                    alignment = TextAnchor.MiddleCenter,
                    richText = true,
                    wordWrap = false
                };
            }
            _labelStyle.fontSize = Mathf.Max(8, Mathf.RoundToInt(fontSize));
            _labelStyle.normal.textColor = TextColor;
            return _labelStyle;
        }

        private static GUIStyle SubLabelStyle
        {
            get
            {
                if (_subLabelStyle == null)
                {
                    _subLabelStyle = new GUIStyle
                    {
                        alignment = TextAnchor.MiddleCenter,
                        richText = true,
                        wordWrap = false
                    };
                }
                _subLabelStyle.fontSize = Mathf.Max(6, Mathf.RoundToInt(SubFontSize));
                _subLabelStyle.normal.textColor = SubTextColor;
                return _subLabelStyle;
            }
        }

        public static void DrawBackground(Rect rect)
        {
            var previous = GUI.color;
            var color = BackgroundColor;
            GUI.color = new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(BackgroundOpacity));
            int radius = Mathf.Max(1, Mathf.RoundToInt(rect.height / 2f));
            GUI.Box(rect, GUIContent.none, BackgroundStyle(radius));
            GUI.color = previous;
        }

        public static void DrawLabel(Rect rect, string text, float alpha, float titleFontSize = -1f)
        {
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            GUI.Label(rect, text, titleFontSize >= 0f ? LabelStyleFor(titleFontSize) : LabelStyle);
            GUI.color = previous;
        }

        public static void DrawLabel(Rect rect, string title, string subtitle, float alpha, float titleFontSize = -1f)
        {
            if (string.IsNullOrEmpty(subtitle))
            {
                DrawLabel(rect, title, alpha, titleFontSize);
                return;
            }
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            var titleStyle = titleFontSize >= 0f ? LabelStyleFor(titleFontSize) : LabelStyle;
            float titleHeight = titleStyle.CalcSize(new GUIContent(title)).y;
            float subHeight = SubLabelStyle.CalcSize(new GUIContent(subtitle)).y;
            float totalHeight = titleHeight + TitleSubtitleSpacing + subHeight;
            float y = rect.y + (rect.height - totalHeight) / 2f;
            GUI.Label(new Rect(rect.x, y, rect.width, titleHeight), title, titleStyle);
            GUI.Label(new Rect(rect.x, y + titleHeight + TitleSubtitleSpacing, rect.width, subHeight), subtitle, SubLabelStyle);
            GUI.color = previous;
        }

        public static float MeasureWidth(string text)
        {
            return LabelStyle.CalcSize(new GUIContent(text)).x;
        }

        public static Vector2 MeasureSize(string title, string subtitle, float titleFontSize = -1f)
        {
            var titleStyle = titleFontSize >= 0f ? LabelStyleFor(titleFontSize) : LabelStyle;
            var titleSize = titleStyle.CalcSize(new GUIContent(title));
            if (string.IsNullOrEmpty(subtitle)) return titleSize;
            var subSize = SubLabelStyle.CalcSize(new GUIContent(subtitle));
            return new Vector2(Mathf.Max(titleSize.x, subSize.x), titleSize.y + TitleSubtitleSpacing + subSize.y);
        }

        private static float SeparatorGap => FontSize * 0.6f;

        private static float SeparatorWidth => SubLabelStyle.CalcSize(new GUIContent("|")).x;

        public static Vector2 MeasureSegments(IReadOnlyList<(string Title, string Subtitle)> segments, float titleFontSize = -1f)
        {
            float width = 0f;
            float height = 0f;
            for (int i = 0; i < segments.Count; i++)
            {
                float fontSize = i == 0 && titleFontSize >= 0f ? titleFontSize : FontSize;
                var size = MeasureSize(segments[i].Title, segments[i].Subtitle, fontSize);
                width += size.x;
                height = Mathf.Max(height, size.y);
                if (i < segments.Count - 1)
                {
                    width += SeparatorGap * 2f + SeparatorWidth;
                }
            }
            return new Vector2(width, height);
        }

        public static void DrawSegment(Rect rect, string title, string subtitle, float alpha, float titleFontSize = -1f)
        {
            DrawLabel(rect, title, subtitle, alpha, titleFontSize);
        }

        public static void DrawSegments(Rect rect, IReadOnlyList<(string Title, string Subtitle)> segments, float alpha, float titleFontSize = -1f)
        {
            if (segments == null || segments.Count == 0) return;
            if (segments.Count == 1)
            {
                DrawSegment(rect, segments[0].Title, segments[0].Subtitle, alpha,
                    titleFontSize >= 0f ? titleFontSize : FontSize);
                return;
            }
            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            var total = MeasureSegments(segments, titleFontSize);
            float cursorX = rect.x + (rect.width - total.x) / 2f;
            for (int i = 0; i < segments.Count; i++)
            {
                float fontSize = i == 0 && titleFontSize >= 0f ? titleFontSize : FontSize;
                var size = MeasureSize(segments[i].Title, segments[i].Subtitle, fontSize);
                DrawSegment(new Rect(cursorX, rect.y, size.x, rect.height),
                    segments[i].Title, segments[i].Subtitle, alpha, fontSize);
                cursorX += size.x;
                if (i < segments.Count - 1)
                {
                    cursorX += SeparatorGap;
                    GUI.Label(new Rect(cursorX, rect.y, SeparatorWidth, rect.height), "|", SubLabelStyle);
                    cursorX += SeparatorWidth + SeparatorGap;
                }
            }
            GUI.color = previous;
        }

        private static Texture2D CreateRoundedRectTexture(int width, int height, int radius)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sx = x + 0.5f;
                    float sy = y + 0.5f;
                    float cx = Mathf.Clamp(sx, radius, width - radius);
                    float cy = Mathf.Clamp(sy, radius, height - radius);
                    float dx = sx - cx;
                    float dy = sy - cy;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    byte alpha = (byte)(Mathf.Clamp01(radius - distance) * 255f);
                    pixels[y * width + x] = new Color32(255, 255, 255, alpha);
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }
    }
}
