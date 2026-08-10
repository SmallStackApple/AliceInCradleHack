using System.Diagnostics;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class NotificationHudElement : HudElement
    {
        public static long DurationMs { get; set; } = 3000;
        public static float Padding { get; set; } = 8f;
        public static float MaxWidth { get; set; } = 640f;

        private static readonly object Sync = new();
        private static readonly Stopwatch Clock = Stopwatch.StartNew();
        private static string _current;
        private static long _currentDeadline;

        public static void Push(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            lock (Sync)
            {
                _current = message;
                _currentDeadline = Clock.ElapsedMilliseconds + DurationMs;
            }
        }

        private static string CurrentText()
        {
            lock (Sync)
            {
                long now = Clock.ElapsedMilliseconds;
                if (_current != null && now >= _currentDeadline)
                {
                    _current = null;
                }
                return _current;
            }
        }

        public override bool IsVisible => CurrentText() != null;

        public override bool HasBackground => true;

        public override IHudElement.Size HudSize
        {
            get
            {
                string text = CurrentText();
                if (text == null) return new IHudElement.Size(0f, DynamicIsland.Instance.ContentHeight);
                float width = ImGuiRenderUtil.MeasureWidth(text);
                return new IHudElement.Size(Mathf.Min(width + Padding, MaxWidth), DynamicIsland.Instance.ContentHeight);
            }
        }

        public override void Render(float x, float y, float width, float height, float alpha)
        {
            string text = CurrentText();
            if (text == null) return;
            ImGuiRenderUtil.DrawLabel(new Rect(x, y, width, height), text, alpha);
        }
    }
}
