using System;
using System.Runtime.CompilerServices;
namespace AliceInCradleHack.Utils
{
    //何意味？
    [Obsolete("This kotlin extension is not recommended to use.It can't fully replicate Kotlin's behavior.")]
    public static class KotlinExtension
    {
        // ==============================
        // Kotlin: inline fun T.apply(block: T.() -> Unit): T
        // ==============================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("This kotlin extension method is not recommended to use.It can't fully replicate Kotlin's behavior.")]
        public static T apply<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        // ==============================
        // Kotlin: inline fun T.also(block: (T) -> Unit): T
        // ==============================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("This kotlin extension method is not recommended to use.It can't fully replicate Kotlin's behavior.")]
        public static T also<T>(this T self, Action<T> block)
        {
            block(self);
            return self;
        }

        // ==============================
        // Kotlin: inline fun <T, R> T.let(block: (T) -> R): R
        // ==============================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("This kotlin extension method is not recommended to use.It can't fully replicate Kotlin's behavior.")]
        public static R let<T, R>(this T self, Func<T, R> block) => block(self);

        // ==============================
        // Kotlin: inline fun <T, R> T.run(block: T.() -> R): R
        // ==============================
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Obsolete("This kotlin extension method is not recommended to use.It can't fully replicate Kotlin's behavior.")]
        public static R run<T, R>(this T self, Func<T, R> block) => block(self);
    }
}
