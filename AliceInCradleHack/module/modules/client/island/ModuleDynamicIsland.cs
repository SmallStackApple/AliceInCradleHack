using AliceInCradleHack.config;
using AliceInCradleHack.events;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class ModuleDynamicIsland : Module
    {
        public ModuleDynamicIsland() : base("DynamicIsland", "Shows a dynamic island HUD at the top of the screen.", "Client")
        {
        }

        public override bool IsEnabled { get; set; } = true;

        public readonly RangedValue<float> Height = new(28f, 16f, 48f, "px", "Height of the dynamic island content area.");
        public readonly RangedValue<float> TopMargin = new(25f, 0f, 200f, "px", "Distance between the dynamic island and the top of the screen.");
        public readonly RangedValue<float> PaddingX = new(15f, 0f, 64f, "px", "Horizontal padding between content and the island edges.");
        public readonly RangedValue<float> PaddingY = new(1.5f, 0f, 32f, "px", "Vertical padding between content and the island edges.");
        public readonly RangedValue<float> FontSize = new(14f, 8f, 48f, "px", "Font size of the island title text.");
        public readonly RangedValue<float> SubFontSize = new(10.5f, 6f, 40f, "px", "Font size of the island subtitle text.");
        public readonly RangedValue<float> ClientNameFontSize = new(14f, 8f, 48f, "px", "Font size of the client name in the watermark.");
        public readonly RangedValue<float> TitleSubtitleSpacing = new(0f, 0f, 16f, "px", "Vertical spacing between the island title and subtitle text.");
        public readonly RangedValue<float> BackgroundOpacity = new(0.16f, 0f, 1f, "", "Opacity of the dynamic island background.");
        public readonly ColorValue BackgroundColor = new("#000000", "Background color of the dynamic island (hex).");
        public readonly ColorValue TextColor = new("#FFFFFF", "Color of primary text (hex).");
        public readonly ColorValue SubTextColor = new("#BFBFBF", "Color of secondary sub text (hex).");
        public readonly ColorValue ClientNameColor = new("#7EE787", "Color of the client name in the watermark (hex).");
        public readonly RangedValue<int> NotificationDuration = new(3000, 250, 10000, "ms", "How long a notification remains visible.");
        public readonly RangedValue<float> NotificationPadding = new(8f, 0f, 32f, "px", "Horizontal padding around notification text.");
        public readonly RangedValue<float> NotificationMaxWidth = new(640f, 100f, 1200f, "px", "Maximum width of a notification.");

        private static Color ParseColor(string hex, Color fallback)
        {
            return ColorUtility.TryParseHtmlString(hex, out var color) ? color : fallback;
        }

        public override void Initialize()
        {
            if (IsEnabled)
            {
                XxINEvents.EventPostUpdate += OnXxInPostUpdate;
            }

            Height.OnChanged(height => DynamicIsland.Instance.ContentHeight = height);
            TopMargin.OnChanged(margin => DynamicIsland.Instance.TopMargin = margin);
            PaddingX.OnChanged(padding => DynamicIsland.Instance.PaddingX = padding);
            PaddingY.OnChanged(padding => DynamicIsland.Instance.PaddingY = padding);
            FontSize.OnChanged(size => ImGuiRenderUtil.FontSize = size);
            SubFontSize.OnChanged(size => ImGuiRenderUtil.SubFontSize = size);
            ClientNameFontSize.OnChanged(size => ImGuiRenderUtil.ClientNameFontSize = size);
            TitleSubtitleSpacing.OnChanged(spacing => ImGuiRenderUtil.TitleSubtitleSpacing = spacing);
            BackgroundOpacity.OnChanged(opacity => ImGuiRenderUtil.BackgroundOpacity = opacity);
            BackgroundColor.OnChanged(hex => ImGuiRenderUtil.BackgroundColor = ParseColor(hex, Color.black));
            TextColor.OnChanged(hex => ImGuiRenderUtil.TextColor = ParseColor(hex, Color.white));
            SubTextColor.OnChanged(hex => ImGuiRenderUtil.SubTextColor = ParseColor(hex, new Color(0.75f, 0.75f, 0.75f)));
            ClientNameColor.OnChanged(hex => WatermarkHudElement.ClientNameColorHex = hex);
            NotificationDuration.OnChanged(duration => NotificationHudElement.DurationMs = duration);
            NotificationPadding.OnChanged(padding => NotificationHudElement.Padding = padding);
            NotificationMaxWidth.OnChanged(width => NotificationHudElement.MaxWidth = width);

            ApplySettings();
        }

        public override void Enable()
        {
            XxINEvents.EventPostUpdate += OnXxInPostUpdate;
            ApplySettings();
            DynamicIsland.Instance.Enabled = true;
        }

        public override void Disable()
        {
            XxINEvents.EventPostUpdate -= OnXxInPostUpdate;
            DynamicIsland.Instance.Enabled = false;
        }

        private void OnXxInPostUpdate(object sender, XxINEvents.UpdateEventArgs e)
        {
            GuiBehaviour.EnsureCreated();
        }

        private void ApplySettings()
        {
            DynamicIsland.Instance.ContentHeight = Height.Get();
            DynamicIsland.Instance.TopMargin = TopMargin.Get();
            DynamicIsland.Instance.PaddingX = PaddingX.Get();
            DynamicIsland.Instance.PaddingY = PaddingY.Get();
            ImGuiRenderUtil.FontSize = FontSize.Get();
            ImGuiRenderUtil.SubFontSize = SubFontSize.Get();
            ImGuiRenderUtil.ClientNameFontSize = ClientNameFontSize.Get();
            ImGuiRenderUtil.TitleSubtitleSpacing = TitleSubtitleSpacing.Get();
            ImGuiRenderUtil.BackgroundOpacity = BackgroundOpacity.Get();
            ImGuiRenderUtil.BackgroundColor = ParseColor(BackgroundColor.Get(), Color.black);
            ImGuiRenderUtil.TextColor = ParseColor(TextColor.Get(), Color.white);
            ImGuiRenderUtil.SubTextColor = ParseColor(SubTextColor.Get(), new Color(0.75f, 0.75f, 0.75f));
            WatermarkHudElement.ClientNameColorHex = ClientNameColor.Get();
            NotificationHudElement.DurationMs = NotificationDuration.Get();
            NotificationHudElement.Padding = NotificationPadding.Get();
            NotificationHudElement.MaxWidth = NotificationMaxWidth.Get();
        }
    }
}
