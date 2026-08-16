using System.Diagnostics;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class NotificationHudElement : HudElement
    {
        public static long DurationMs { get; set; } = 3000;
        public static float Padding { get; set; } = 8f;
        public static float MaxWidth { get; set; } = 640f;

        private static readonly object _sync = new();
        private static readonly Stopwatch _clock = Stopwatch.StartNew();
        private static string _currentTitle;
        private static string _currentSubtitle;
        private static long _currentDeadline;

        public static void Push(string message)
        {
            Push(message, null);
        }

        public static void Push(string title, string subtitle)
        {
            if (string.IsNullOrEmpty(title)) return;
            lock (_sync)
            {
                _currentTitle = title;
                _currentSubtitle = subtitle;
                _currentDeadline = _clock.ElapsedMilliseconds + DurationMs;
            }
        }

        private static (string Title, string Subtitle)? Current()
        {
            lock (_sync)
            {
                if (_currentTitle != null && _clock.ElapsedMilliseconds >= _currentDeadline)
                {
                    _currentTitle = null;
                    _currentSubtitle = null;
                }
                return _currentTitle == null ? null : (_currentTitle, _currentSubtitle);
            }
        }

        public override bool IsVisible => Current() != null;

        public override bool HasBackground => true;

        public override IHudElement.Size HudSize
        {
            get
            {
                var current = Current();
                if (current == null) return new IHudElement.Size(0f, DynamicIsland.Instance.ContentHeight);
                var size = ImGuiRenderUtil.MeasureSize(current.Value.Title, current.Value.Subtitle);
                return new IHudElement.Size(
                    Mathf.Min(size.x + Padding, MaxWidth),
                    Mathf.Max(size.y, DynamicIsland.Instance.ContentHeight));
            }
        }

        public override void Render(float x, float y, float width, float height, float alpha)
        {
            var current = Current();
            if (current == null) return;
            ImGuiRenderUtil.DrawSegment(new Rect(x, y, width, height), current.Value.Title, current.Value.Subtitle, alpha);
        }
    }
}
