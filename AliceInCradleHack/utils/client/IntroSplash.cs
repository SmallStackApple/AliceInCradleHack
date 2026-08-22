using AliceInCradleHack.events;
using System;
using UnityEngine;

namespace AliceInCradleHack.utils.client
{
    /// <summary>
    /// One-time full-screen intro created from XX.IN.Update on Unity's main thread.
    /// </summary>
    public sealed class IntroSplash : OnGuiRenderer<IntroSplash>, IClientComponent
    {
        private const float BackgroundFadeInDuration = 0.8f;
        private const float PrefixStart = 1.1f;
        private const float PrefixAppearDuration = 0.9f;
        private const float PrefixHoldDuration = 0.7f;
        private const float PrefixSlideDuration = 0.5f;
        private const float SuffixDelay = 0.3f;
        private const float SuffixAppearDuration = 0.5f;
        private const float LogoHoldDuration = 1.3f;
        private const float FadeOutDuration = 0.7f;

        private const string Prefix = "A";
        private const string Suffix = "liceInCradleHack";

        private static readonly Lazy<IntroSplash> LazyInstance = new(() => new IntroSplash());

        private bool _waitingForUnityUpdate;
        private bool _running;
        private float _startedAt;
        private GUIStyle _prefixStyle;
        private GUIStyle _suffixStyle;

        public static IntroSplash Controller => LazyInstance.Value;

        private IntroSplash()
        {
        }

        public void Initialize()
        {
            if (_waitingForUnityUpdate || _running) return;

            // Client.Initialize runs on the injector's background thread. XX.IN.Update
            // is the existing, proven bridge to Unity's main thread.
            _waitingForUnityUpdate = true;
            XxINEvents.EventPostUpdate += OnPostUpdate;
        }

        public void Dispose()
        {
            XxINEvents.EventPostUpdate -= OnPostUpdate;
            _waitingForUnityUpdate = false;
            _running = false;
        }

        private void OnPostUpdate(object sender, XxINEvents.UpdateEventArgs e)
        {
            XxINEvents.EventPostUpdate -= OnPostUpdate;
            _waitingForUnityUpdate = false;

            EnsureCreated("AliceInCradleHack.IntroSplash");
            if (OnGuiRenderer<IntroSplash>.Instance != null)
            {
                OnGuiRenderer<IntroSplash>.Instance.Begin();
            }
        }

        private void Begin()
        {
            _startedAt = Time.realtimeSinceStartup;
            _running = true;
        }

        protected override void Render()
        {
            if (!_running) return;

            float elapsed = Time.realtimeSinceStartup - _startedAt;
            float fadeOutStart = PrefixStart + PrefixAppearDuration + PrefixHoldDuration +
                                 PrefixSlideDuration + SuffixDelay + SuffixAppearDuration +
                                 LogoHoldDuration;
            if (elapsed >= fadeOutStart + FadeOutDuration)
            {
                _running = false;
                return;
            }

            float backgroundAlpha = elapsed <= BackgroundFadeInDuration
                ? 0.6f * EaseOutCubic(elapsed / BackgroundFadeInDuration)
                : elapsed <= fadeOutStart
                    ? 0.6f
                    : 0.6f * (1f - EaseInCubic((elapsed - fadeOutStart) / FadeOutDuration));

            int previousDepth = GUI.depth;
            Color previousColor = GUI.color;
            try
            {
                GUI.depth = -1000;
                GUI.color = new Color(0f, 0f, 0f, Mathf.Clamp01(backgroundAlpha));
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
                DrawLogo(elapsed, fadeOutStart);
            }
            finally
            {
                GUI.color = previousColor;
                GUI.depth = previousDepth;
            }
        }

        private void DrawLogo(float elapsed, float fadeOutStart)
        {
            float prefixProgress = Mathf.Clamp01((elapsed - PrefixStart) / PrefixAppearDuration);
            if (prefixProgress <= 0f) return;

            float fadeFactor = elapsed <= fadeOutStart
                ? 1f
                : 1f - Mathf.Clamp01((elapsed - fadeOutStart) / FadeOutDuration);
            float prefixScale = Mathf.Lerp(2f, 1f, EaseOutCubic(prefixProgress));
            float prefixAlpha = EaseOutCubic(prefixProgress) * fadeFactor;

            float slideStart = PrefixStart + PrefixAppearDuration + PrefixHoldDuration;
            float slideProgress = elapsed <= slideStart
                ? 0f
                : EaseOutCubic((elapsed - slideStart) / PrefixSlideDuration);
            float suffixStart = slideStart + PrefixSlideDuration + SuffixDelay;
            float suffixAlpha = elapsed <= suffixStart
                ? 0f
                : EaseOutCubic((elapsed - suffixStart) / SuffixAppearDuration) * fadeFactor;

            GUIStyle prefixStyle = ScaledStyle(PrefixStyle, prefixScale);
            GUIStyle suffixStyle = SuffixStyle;
            float prefixWidth = prefixStyle.CalcSize(new GUIContent(Prefix)).x;
            float suffixWidth = suffixStyle.CalcSize(new GUIContent(Suffix)).x;
            float centeredPrefixX = Screen.width * 0.5f - prefixWidth * 0.5f;
            float logoLeft = Screen.width * 0.5f - (prefixWidth + suffixWidth) * 0.5f;
            float prefixX = Mathf.Lerp(centeredPrefixX, logoLeft, slideProgress);
            float prefixY = Screen.height * 0.5f - prefixStyle.fontSize * 0.5f;

            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(prefixAlpha));
            GUI.Label(new Rect(prefixX, prefixY, prefixWidth + 4f, prefixStyle.fontSize * 1.3f), Prefix, prefixStyle);

            if (suffixAlpha <= 0f) return;

            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(suffixAlpha));
            float suffixY = Screen.height * 0.5f - suffixStyle.fontSize * 0.5f;
            GUI.Label(new Rect(prefixX + prefixWidth, suffixY, suffixWidth + 4f, suffixStyle.fontSize * 1.3f), Suffix, suffixStyle);
        }

        private GUIStyle PrefixStyle => _prefixStyle ??= CreateStyle(64);
        private GUIStyle SuffixStyle => _suffixStyle ??= CreateStyle(42);

        private static GUIStyle CreateStyle(int fontSize)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                richText = false,
                wordWrap = false
            };
        }

        private static GUIStyle ScaledStyle(GUIStyle source, float scale)
        {
            return new GUIStyle(source)
            {
                fontSize = Mathf.Max(1, Mathf.RoundToInt(source.fontSize * scale))
            };
        }

        private static float EaseOutCubic(float value)
        {
            float t = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private static float EaseInCubic(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * t;
        }
    }
}
