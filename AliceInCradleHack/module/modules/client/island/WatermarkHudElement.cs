using System;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class WatermarkHudElement : HudElement
    {
        public override bool HasBackground => true;

        private static string Text
        {
            get
            {
                var version = typeof(InjectEntry).Assembly.GetName().Version;
                return $"<color=#7EE787><b>AliceInCradleHack</b></color> <color=#FFFFFF66>|</color> " +
                       $"v{version.Major}.{version.Minor}.{version.Build} <color=#FFFFFF66>|</color> " +
                       $"<color=#FFFFFFBF>{DateTime.Now:HH:mm:ss}</color>";
            }
        }

        public override IHudElement.Size HudSize
        {
            get
            {
                float width = ImGuiRenderUtil.MeasureWidth(Text);
                return new IHudElement.Size(Mathf.Min(width + 8f, 640f), DynamicIsland.Instance.ContentHeight);
            }
        }

        public override void Render(float x, float y, float width, float height, float alpha)
        {
            ImGuiRenderUtil.DrawLabel(new Rect(x, y, width, height), Text, alpha);
        }
    }
}
