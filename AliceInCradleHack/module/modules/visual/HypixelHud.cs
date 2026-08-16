using AliceInCradleHack.utils.client;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AliceInCradleHack.module.modules.visual
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

        private static Font _font;
        private static bool _fontResolved;

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
            var font = GetFont();
            float alpha = ComputeAlpha();
            if (alpha <= 0f) return;

            var titleStyle = MakeStyle(font, TitleFontSize, _titleColor);
            var subtitleStyle = MakeStyle(font, SubtitleFontSize, _subtitleColor);

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

        private static Font GetFont()
        {
            if (_font != null) return _font;
            if (_fontResolved) return null;
            _fontResolved = true;
            try
            {
                _font = MinecraftFontLoader.Create();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load the Minecraft font", ex);
                _font = null;
            }
            return _font;
        }

        /// <summary>
        /// Extracts the embedded Minecraft.ttf to a temp file, registers it with the
        /// Windows font API and loads it as a dynamic Unity font.
        /// </summary>
        private static class MinecraftFontLoader
        {
            private const string ResourceName = "AliceInCradleHack.resources.fonts.Minecraft.ttf";
            private const string FontFamilyName = "Minecraft";

            private const uint FR_PRIVATE = 0x10;
            private const uint HWND_BROADCAST = 0xFFFF;
            private const uint WM_FONTCHANGE = 0x001D;

            [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

            [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern int AddFontResource(string lpszFilename);

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

            private static string _fontPath;
            private static bool _fontRegistered;

            public static Font Create()
            {
                string path = ExtractFont();
                if (path != null)
                {
                    RegisterFont(path);
                }

                int size = Mathf.Max(8, Mathf.RoundToInt(TitleFontSize));
                for (int i = 0; i < 4; i++)
                {
                    var font = Font.CreateDynamicFontFromOSFont(FontFamilyName, size);
                    if (font != null) return font;
                }

                var fallback = new Font(FontFamilyName);
                return fallback;
            }

            private static void RegisterFont(string path)
            {
                if (_fontRegistered)
                {
                    RefreshFontCaches();
                    return;
                }

                if (AddFontResourceEx(path, FR_PRIVATE, IntPtr.Zero) != 0)
                {
                    _fontRegistered = true;
                }
                else if (AddFontResource(path) != 0)
                {
                    _fontRegistered = true;
                }

                RefreshFontCaches();
            }

            private static void RefreshFontCaches()
            {
                SendMessageTimeout(new IntPtr(HWND_BROADCAST), WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);
            }

            private static string ExtractFont()
            {
                if (_fontPath != null && File.Exists(_fontPath)) return _fontPath;

                var assembly = typeof(MinecraftFontLoader).Assembly;
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
                    return null;
                }

                try
                {
                    using (stream)
                    using (var file = new FileStream(FontPath(), FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(file);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Failed to extract the Minecraft font", ex);
                    return null;
                }

                _fontPath = FontPath();
                return _fontPath;
            }

            private static string FontPath()
            {
                string dir = Path.Combine(Path.GetTempPath(), "AliceInCradleHack");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "Minecraft.ttf");
            }
        }
    }
}
