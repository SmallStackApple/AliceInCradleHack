using AliceInCradleHack.config;

namespace AliceInCradleHack.module.modules.client.island
{
    public class ModuleDynamicIsland : Module
    {
        public ModuleDynamicIsland() : base("DynamicIsland", "Shows a dynamic island HUD at the top of the screen.", "Client")
        {
        }

        public override bool IsEnabled { get; set; } = true;

        public readonly RangedValue<float> Height = new(28f, 16f, 48f, "px", "Height of the dynamic island content area.");
        public readonly RangedValue<float> BackgroundOpacity = new(0.16f, 0f, 1f, "", "Opacity of the dynamic island background.");
        public readonly RangedValue<int> NotificationDuration = new(3000, 250, 10000, "ms", "How long a notification remains visible.");
        public readonly RangedValue<float> NotificationPadding = new(8f, 0f, 32f, "px", "Horizontal padding around notification text.");
        public readonly RangedValue<float> NotificationMaxWidth = new(640f, 100f, 1200f, "px", "Maximum width of a notification.");

        public override void Initialize()
        {
            Height.OnChanged(height => DynamicIsland.Instance.ContentHeight = height);
            BackgroundOpacity.OnChanged(opacity => ImGuiRenderUtil.BackgroundOpacity = opacity);
            NotificationDuration.OnChanged(duration => NotificationHudElement.DurationMs = duration);
            NotificationPadding.OnChanged(padding => NotificationHudElement.Padding = padding);
            NotificationMaxWidth.OnChanged(width => NotificationHudElement.MaxWidth = width);

            DynamicIsland.Instance.ContentHeight = Height.Get();
            ImGuiRenderUtil.BackgroundOpacity = BackgroundOpacity.Get();
            NotificationHudElement.DurationMs = NotificationDuration.Get();
            NotificationHudElement.Padding = NotificationPadding.Get();
            NotificationHudElement.MaxWidth = NotificationMaxWidth.Get();
        }

        public override void Enable()
        {
            DynamicIsland.Instance.ContentHeight = Height.Get();
            ImGuiRenderUtil.BackgroundOpacity = BackgroundOpacity.Get();
            NotificationHudElement.DurationMs = NotificationDuration.Get();
            NotificationHudElement.Padding = NotificationPadding.Get();
            NotificationHudElement.MaxWidth = NotificationMaxWidth.Get();
            DynamicIsland.Instance.Enabled = true;
        }

        public override void Disable()
        {
            DynamicIsland.Instance.Enabled = false;
        }
    }
}
