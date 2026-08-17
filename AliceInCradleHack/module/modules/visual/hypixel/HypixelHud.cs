using AliceInCradleHack.utils.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using SdBitmap = System.Drawing.Bitmap;
using SdBrushes = System.Drawing.Brushes;
using SdColor = System.Drawing.Color;
using SdFont = System.Drawing.Font;
using SdFontFamily = System.Drawing.FontFamily;
using SdGraphics = System.Drawing.Graphics;
using SdImageLockMode = System.Drawing.Imaging.ImageLockMode;
using SdPixelFormat = System.Drawing.Imaging.PixelFormat;
using SdPrivateFontCollection = System.Drawing.Text.PrivateFontCollection;
using SdStringFormat = System.Drawing.StringFormat;
using SdTextRenderingHint = System.Drawing.Text.TextRenderingHint;

namespace AliceInCradleHack.module.modules.visual.hypixel
{
    /// <summary>
    /// Fullscreen center overlay that renders Hypixel-style battle HUD text
    /// (countdown, victory, defeat) using the embedded Minecraft font.
    /// </summary>
    public class HypixelHud : OnGuiRenderer<HypixelHud>
    {
        private enum Phase
        {
            None,
            Countdown,
            Victory,
            Defeat
        }

        public static float TitleFontSize = 48f;
        public static float SubtitleFontSize = 24f;
        public static float Offset = 0f;
        public static float CountdownInterval = 1f;
        public static float ResultDuration = 4f;

        private static Phase _phase = Phase.None;
        private static string _title = "";
        private static string _subtitle = "";
        private static Color _titleColor = Color.white;
        private static Color _subtitleColor = Color.white;
        private static float _elapsed;

        private static readonly Color ColorCountdownTitle = new(1f, 0.333f, 0.333f);
        private static readonly Color ColorCountdownSubtitle = new(1f, 1f, 0.333f);
        private static readonly Color ColorVictoryTitle = new(1f, 0.667f, 0f);
        private static readonly Color ColorResultSubtitle = new(0.667f, 0.667f, 0.667f);
        private static readonly Color ColorDefeatTitle = new(1f, 0.333f, 0.333f);

        public static void EnsureCreated()
        {
            EnsureCreated("AliceInCradleHack.HypixelHud");
        }

        public static void Clear()
        {
            _phase = Phase.None;
            _title = "";
            _subtitle = "";
            _elapsed = 0f;
        }

        public static void OnBattleStart()
        {
            _phase = Phase.Countdown;
            _title = "3";
            _subtitle = "Prepare to fight!";
            _titleColor = ColorCountdownTitle;
            _subtitleColor = ColorCountdownSubtitle;
            _elapsed = 0f;
        }

        public static void OnVictory()
        {
            _phase = Phase.Victory;
            _title = "VICTORY!";
            _subtitle = "You were the last (wo)man standing!";
            _titleColor = ColorVictoryTitle;
            _subtitleColor = ColorResultSubtitle;
            _elapsed = 0f;
        }

        public static void OnBattleEnd()
        {
            if (_phase == Phase.Countdown)
            {
                Clear();
            }
        }

        public static void OnDefeat()
        {
            if (_phase == Phase.None) return;
            _phase = Phase.Defeat;
            _title = "YOU DIED!";
            _subtitle = "You are now a spectator!";
            _titleColor = ColorDefeatTitle;
            _subtitleColor = ColorResultSubtitle;
            _elapsed = 0f;
        }

        private void Update()
        {
            if (_phase == Phase.None) return;

            _elapsed += Time.unscaledDeltaTime;

            if (_phase == Phase.Countdown)
            {
                int digit = 3 - (int)(_elapsed / Mathf.Max(0.05f, CountdownInterval));
                if (digit >= 1)
                {
                    _title = digit.ToString();
                }
                else
                {
                    _phase = Phase.None;
                }
            }
            else if (_elapsed >= ResultDuration)
            {
                _phase = Phase.None;
            }
        }

        protected override void Render()
        {
            if (_phase == Phase.None) return;
            Draw();
        }

        private static void Draw()
        {
            float alpha = ComputeAlpha();
            if (alpha <= 0f) return;

            if (!MinecraftFontRenderer.IsAvailable)
            {
                DrawWithDefaultFont(alpha);
                return;
            }

            const float spacing = 10f;
            float centerX = Screen.width * 0.5f;
            float anchorY = Screen.height * 0.5f + Offset;

            var title = MinecraftFontRenderer.Render(_title, TitleFontSize);
            var subtitle = string.IsNullOrEmpty(_subtitle) ? null : MinecraftFontRenderer.Render(_subtitle, SubtitleFontSize);

            float totalHeight = title.Height + spacing + (subtitle?.Height ?? 0);
            float titleTop = anchorY - totalHeight * 0.5f;

            var previous = GUI.color;

            GUI.color = Tinted(_titleColor, alpha);
            GUI.DrawTexture(new Rect(centerX - title.Width * 0.5f, titleTop, title.Width, title.Height), title.Texture, ScaleMode.StretchToFill, true);

            if (subtitle != null)
            {
                float subTop = titleTop + title.Height + spacing;
                GUI.color = Tinted(_subtitleColor, alpha);
                GUI.DrawTexture(new Rect(centerX - subtitle.Width * 0.5f, subTop, subtitle.Width, subtitle.Height), subtitle.Texture, ScaleMode.StretchToFill, true);
            }

            GUI.color = previous;
        }

        private static Color Tinted(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, color.a * alpha);
        }

        /// <summary>
        /// Fallback path used when the embedded Minecraft font failed to load:
        /// renders with the default IMGUI font instead.
        /// </summary>
        private static void DrawWithDefaultFont(float alpha)
        {
            var titleStyle = MakeStyle(null, TitleFontSize, _titleColor);
            var subtitleStyle = MakeStyle(null, SubtitleFontSize, _subtitleColor);

            float titleWidth = titleStyle.CalcSize(new GUIContent(_title)).x;
            float subtitleWidth = 0f;
            if (!string.IsNullOrEmpty(_subtitle))
            {
                subtitleWidth = subtitleStyle.CalcSize(new GUIContent(_subtitle)).x;
            }

            const float spacing = 10f;
            float totalWidth = Mathf.Max(titleWidth, subtitleWidth);
            float centerX = Screen.width * 0.5f;
            float anchorY = Screen.height * 0.5f + Offset;

            float titleTop = anchorY - (titleStyle.CalcSize(new GUIContent(_title)).y + spacing + (string.IsNullOrEmpty(_subtitle) ? 0f : subtitleStyle.CalcSize(new GUIContent(_subtitle)).y)) * 0.5f;

            var previous = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            GUI.Label(new Rect(centerX - totalWidth * 0.5f, titleTop, totalWidth, titleStyle.CalcSize(new GUIContent(_title)).y), _title, titleStyle);
            if (!string.IsNullOrEmpty(_subtitle))
            {
                float subTop = titleTop + titleStyle.CalcSize(new GUIContent(_title)).y + spacing;
                GUI.Label(new Rect(centerX - totalWidth * 0.5f, subTop, totalWidth, subtitleStyle.CalcSize(new GUIContent(_subtitle)).y), _subtitle, subtitleStyle);
            }

            GUI.color = previous;
        }

        private static GUIStyle MakeStyle(Font font, float fontSize, Color color)
        {
            var style = new GUIStyle
            {
                font = font,
                fontSize = Mathf.Max(8, Mathf.RoundToInt(fontSize)),
                alignment = TextAnchor.MiddleCenter,
                richText = false,
                wordWrap = false
            };
            style.normal.textColor = color;
            return style;
        }

        private static float ComputeAlpha()
        {
            if (_phase == Phase.Countdown)
            {
                float digitProgress = (_elapsed % Mathf.Max(0.05f, CountdownInterval)) / Mathf.Max(0.05f, CountdownInterval);
                return Mathf.SmoothStep(0f, 1f, digitProgress);
            }

            float remain = ResultDuration - _elapsed;
            if (remain <= 0f) return 0f;
            if (remain <= 0.3f) return remain / 0.3f;
            return Mathf.Clamp01(_elapsed / 0.1f);
        }

        /// <summary>
        /// Renders text with the embedded Minecraft font via GDI+ (System.Drawing)
        /// into textures, bypassing Unity's OS font lookup entirely: Unity 2021+
        /// cannot enumerate fonts registered with AddFontResource, so
        /// Font.CreateDynamicFontFromOSFont silently falls back to a default font.
        /// </summary>
        private static class MinecraftFontRenderer
        {
            private const string ResourceName = "AliceInCradleHack.resources.fonts.Minecraft.ttf";

            public sealed class Entry
            {
                public Texture2D Texture;
                public int Width;
                public int Height;
            }

            private static readonly Dictionary<string, Entry> _cache = new();
            private static SdPrivateFontCollection _collection;
            private static SdFontFamily _family;
            private static IntPtr _fontData; // must stay alive while the collection is used
            private static bool _loadAttempted;

            public static bool IsAvailable => EnsureLoaded();

            public static Entry Render(string text, float sizePx)
            {
                if (string.IsNullOrEmpty(text)) text = " ";
                int size = Mathf.Max(8, Mathf.RoundToInt(sizePx));
                string key = size + "|" + text;
                if (_cache.TryGetValue(key, out var entry)) return entry;

                entry = RenderUncached(text, size);
                _cache[key] = entry;
                return entry;
            }

            private static bool EnsureLoaded()
            {
                if (_family != null) return true;
                if (_loadAttempted) return false;
                _loadAttempted = true;

                try
                {
                    var assembly = typeof(MinecraftFontRenderer).Assembly;
                    var stream = assembly.GetManifestResourceStream(ResourceName);
                    if (stream == null)
                    {
                        foreach (var name in assembly.GetManifestResourceNames())
                        {
                            if (name.EndsWith("Minecraft.ttf", StringComparison.OrdinalIgnoreCase))
                            {
                                stream = assembly.GetManifestResourceStream(name);
                                break;
                            }
                        }
                    }

                    if (stream == null)
                    {
                        Log.Error("Minecraft font resource not found in assembly");
                        return false;
                    }

                    byte[] data;
                    using (stream)
                    {
                        using var ms = new MemoryStream();
                        stream.CopyTo(ms);
                        data = ms.ToArray();
                    }

                    // AddMemoryFont does not copy the font data; the buffer must
                    // stay valid for the lifetime of the collection.
                    _fontData = Marshal.AllocCoTaskMem(data.Length);
                    Marshal.Copy(data, 0, _fontData, data.Length);

                    _collection = new SdPrivateFontCollection();
                    _collection.AddMemoryFont(_fontData, data.Length);
                    _family = _collection.Families[0];
                    Log.Info($"Loaded the embedded Minecraft font family '{_family.Name}' via GDI+");
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to load the Minecraft font", ex);
                    return false;
                }
            }

            private static Entry RenderUncached(string text, int sizePx)
            {
                using var font = new SdFont(_family, sizePx, System.Drawing.GraphicsUnit.Pixel);

                int width, height;
                using (var probe = new SdBitmap(1, 1))
                using (var g = SdGraphics.FromImage(probe))
                {
                    var measured = g.MeasureString(text, font, System.Drawing.PointF.Empty, SdStringFormat.GenericTypographic);
                    width = Math.Max(1, (int)Math.Ceiling(measured.Width));
                    height = Math.Max(1, (int)Math.Ceiling(measured.Height));
                }

                byte[] pixels;
                using (var bitmap = new SdBitmap(width, height, SdPixelFormat.Format32bppArgb))
                {
                    using (var g = SdGraphics.FromImage(bitmap))
                    {
                        g.Clear(SdColor.Transparent);
                        g.TextRenderingHint = SdTextRenderingHint.SingleBitPerPixelGridFit;
                        g.DrawString(text, font, SdBrushes.White, 0f, 0f, SdStringFormat.GenericTypographic);
                    }

                    var rect = new System.Drawing.Rectangle(0, 0, width, height);
                    var data = bitmap.LockBits(rect, SdImageLockMode.ReadOnly, SdPixelFormat.Format32bppArgb);
                    try
                    {
                        pixels = new byte[width * height * 4];
                        var row = new byte[width * 4];
                        for (int y = 0; y < height; y++)
                        {
                            // GDI+ bitmaps are top-down, Unity textures bottom-up: flip rows.
                            Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, row.Length);
                            Buffer.BlockCopy(row, 0, pixels, (height - 1 - y) * row.Length, row.Length);
                        }
                    }
                    finally
                    {
                        bitmap.UnlockBits(data);
                    }
                }

                var texture = new Texture2D(width, height, TextureFormat.BGRA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                texture.LoadRawTextureData(pixels);
                texture.Apply(false, false);

                return new Entry { Texture = texture, Width = width, Height = height };
            }
        }
    }
}
