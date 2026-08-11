using System;
using System.Collections.Generic;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class WatermarkHudElement : HudElement
    {
        public override bool HasBackground => true;

        public static string ClientNameColorHex { get; set; } = "#7EE787";

        private static string Title
        {
            get
            {
                return $"<color={ClientNameColorHex}><b>{Client.ClientName}</b></color>";
            }
        }

        private static string VersionType => Client.VersionType;

        private static string Version => $"v{Client.Version}-{(Client.GitHash != "unknow" ? Client.GitHash.Substring(0, 7) : "unknow")}";

        private static (string, string, string, string, string) Texts => (Title, Version, VersionType, $"{DateTime.Now:yyyy/MM/dd}", $"{DateTime.Now:HH:mm:ss}");

        private static readonly List<(string Title, string Subtitle)> Segments = new(3);

        private static List<(string Title, string Subtitle)> CurrentSegments()
        {
            var (title, version, versionType, date, time) = Texts;
            Segments.Clear();
            Segments.Add((title, null));
            Segments.Add((versionType, version));
            Segments.Add((date, time));
            return Segments;
        }

        public override IHudElement.Size HudSize
        {
            get
            {
                var size = ImGuiRenderUtil.MeasureSegments(CurrentSegments(), ImGuiRenderUtil.ClientNameFontSize);
                return new IHudElement.Size(
                    Mathf.Min(size.x + 8f, 640f),
                    Mathf.Max(size.y, DynamicIsland.Instance.ContentHeight));
            }
        }

        public override void Render(float x, float y, float width, float height, float alpha)
        {
            ImGuiRenderUtil.DrawSegments(new Rect(x, y, width, height), CurrentSegments(), alpha, ImGuiRenderUtil.ClientNameFontSize);
        }
    }
}
