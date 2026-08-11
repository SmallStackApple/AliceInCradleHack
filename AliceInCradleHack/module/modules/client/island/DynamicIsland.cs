using AliceInCradleHack.utils.animation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AliceInCradleHack.module.modules.client.island
{
    public class DynamicIsland
    {
        private sealed class ActiveElementSelector
        {
            private readonly DynamicIsland _owner;

            public ActiveElementSelector(DynamicIsland owner)
            {
                _owner = owner;
            }

            public IHudElement Visible()
            {
                foreach (var element in _owner._elements)
                {
                    if (element.IsVisible) return element;
                }
                return null;
            }
        }

        public static readonly DynamicIsland Instance = new();

        public bool Enabled { get; set; } = true;

        public float ContentHeight { get; set; } = 28f;

        public float TopMargin { get; set; } = 25f;

        public float PaddingX { get; set; } = 15f;

        public float PaddingY { get; set; } = 1.5f;

        private readonly List<IHudElement> _elements = new()
        {
            new NotificationHudElement(),
            new WatermarkHudElement()
        };

        private readonly ActiveElementSelector _activeElementSelector;
        private readonly SpringAnimation _widthAnim = new(300f, 1.2f, 20f, 170f);
        private readonly SpringAnimation _heightAnim = new(300f, 1.2f, 20f, 18f);
        private readonly SpringAnimation _transitionAnim = new(250f, 1.0f, 22f, 1f);
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
        private IHudElement _activeElement;
        private IHudElement _outgoingElement;
        private long _lastFrameTimestamp;

        private DynamicIsland()
        {
            _activeElementSelector = new ActiveElementSelector(this);
        }

        public void Render()
        {
            if (!Enabled)
            {
                _activeElement = null;
                _outgoingElement = null;
                _lastFrameTimestamp = 0L;
                return;
            }

            long now = _stopwatch.ElapsedMilliseconds;
            if (_lastFrameTimestamp == 0L)
            {
                _lastFrameTimestamp = now;
            }
            float deltaSec = (now - _lastFrameTimestamp) / 1000f;
            _lastFrameTimestamp = now;
            deltaSec = Math.Min(deltaSec, 0.033333335f);

            var visibleElement = _activeElementSelector.Visible();
            if (visibleElement == null)
            {
                _activeElement = null;
                _outgoingElement = null;
                return;
            }

            if (_activeElement != visibleElement)
            {
                _outgoingElement = _activeElement;
                _activeElement = visibleElement;
                _transitionAnim.Reset(0f);
                _transitionAnim.TargetValue = 1f;
                if (_outgoingElement == null)
                {
                    var initialSize = _activeElement.HudSize;
                    _widthAnim.Reset(initialSize.Width);
                    _heightAnim.Reset(initialSize.Height);
                    _transitionAnim.Reset(1f);
                }
            }

            var size = _activeElement.HudSize;
            float targetWidth = size.Width;
            float targetHeight = size.Height;
            float progress = _transitionAnim.CurrentValue;
            if (_outgoingElement != null && progress < 1f)
            {
                var outgoingSize = _outgoingElement.HudSize;
                targetWidth = Mathf.LerpUnclamped(outgoingSize.Width, size.Width, progress);
                targetHeight = Mathf.LerpUnclamped(outgoingSize.Height, size.Height, progress);
            }

            _widthAnim.TargetValue = targetWidth;
            _heightAnim.TargetValue = targetHeight;
            _widthAnim.Update(deltaSec);
            _heightAnim.Update(deltaSec);
            _transitionAnim.Update(deltaSec);

            float islandWidth = Math.Max(0f, _widthAnim.CurrentValue + PaddingX * 2f);
            float islandHeight = Math.Max(0f, _heightAnim.CurrentValue + PaddingY * 2f);
            float islandX = (Screen.width - islandWidth) / 2f;
            const float anchorSize = 25f;
            float anchorCenterY = TopMargin + anchorSize / 2f;
            float activeY = _activeElement.HudAnchor == IHudElement.Alignment.CENTER
                ? anchorCenterY - islandHeight / 2f
                : TopMargin;
            float islandY;
            if (_outgoingElement != null && progress < 1f)
            {
                float outgoingY = _outgoingElement.HudAnchor == IHudElement.Alignment.CENTER
                    ? anchorCenterY - islandHeight / 2f
                    : TopMargin;
                islandY = Mathf.LerpUnclamped(outgoingY, activeY, progress);
            }
            else
            {
                islandY = activeY;
            }

            var islandRect = new Rect(islandX, islandY, islandWidth, islandHeight);
            if (_activeElement.HasBackground)
            {
                ImGuiRenderUtil.DrawBackground(islandRect);
            }

            GUI.BeginGroup(islandRect);
            _activeElement.Render(0f, 0f, islandWidth, islandHeight, progress);
            GUI.EndGroup();

            if (progress >= 1f)
            {
                _outgoingElement = null;
            }
        }
    }
}
