using AliceInCradleHack.utils.client;
using System;

namespace AliceInCradleHack.events
{
    /// <summary>
    /// Events raised around <c>nel.SceneGame.runIRD(float)</c> by
    /// <see cref="patch.patches.PatchNelSceneGame"/>.
    /// </summary>
    public static class SceneGameEvents
    {
        public static event EventHandler<PreRunIrdEventArgs> EventPreRunIRD;
        public static event EventHandler<PostRunIrdEventArgs> EventPostRunIRD;

        internal static void PreRunIrd(nel.SceneGame scene, float frameCount)
        {
            InvokeHandlers(EventPreRunIRD, scene, new PreRunIrdEventArgs(scene, frameCount));
        }

        internal static void PostRunIrd(nel.SceneGame scene, float frameCount, bool result)
        {
            InvokeHandlers(EventPostRunIRD, scene, new PostRunIrdEventArgs(scene, frameCount, result));
        }

        private static void InvokeHandlers<TArgs>(EventHandler<TArgs> handlers, object sender, TArgs eventArgs)
            where TArgs : EventArgs
        {
            if (handlers == null) return;

            foreach (EventHandler<TArgs> handler in handlers.GetInvocationList())
            {
                try
                {
                    handler.Invoke(sender, eventArgs);
                }
                catch (Exception ex)
                {
                    Log.Error("SceneGameEvents handler exception", ex);
                }
            }
        }

        public class PreRunIrdEventArgs : EventArgs
        {
            public nel.SceneGame Scene { get; }
            public float FrameCount { get; }

            public PreRunIrdEventArgs(nel.SceneGame scene, float frameCount)
            {
                Scene = scene;
                FrameCount = frameCount;
            }
        }

        public sealed class PostRunIrdEventArgs : PreRunIrdEventArgs
        {
            public bool Result { get; }

            public PostRunIrdEventArgs(nel.SceneGame scene, float frameCount, bool result)
                : base(scene, frameCount)
            {
                Result = result;
            }
        }
    }
}
