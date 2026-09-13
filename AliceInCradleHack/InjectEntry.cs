using System.Diagnostics;
using System.Threading;

namespace AliceInCradleHack
{
    public class InjectEntry
    {
        // Kept alive for the process lifetime so the injector can detect an
        // already-injected process via Mutex.TryOpenExisting.
        private static Mutex _injectedMarker;

        private static string InjectedMutexName =>
            $"AliceInCradleHack.Injected.{Process.GetCurrentProcess().Id}";

        // Entry point invoked by the injector: AliceInCradleHack.InjectEntry:Inject()
        private static void Inject()
        {
            if (Mutex.TryOpenExisting(InjectedMutexName, out _))
                return; // already injected into this process

            _injectedMarker = new Mutex(true, InjectedMutexName);

            var injectThread = new Thread(Client.Initialize)
            {
                IsBackground = true
            };
            injectThread.SetApartmentState(ApartmentState.STA);
            injectThread.Start();
        }

        // Note: ejecting the DLL with SharpInjector crashes the host process, reason unknown.
        private static void Eject()
        {
            Client.Dispose();
        }
    }
}
