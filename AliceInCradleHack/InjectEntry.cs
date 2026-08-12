using System.Threading;

namespace AliceInCradleHack
{
    public class InjectEntry
    {
        // Entry point invoked by the injector: AliceInCradleHack.InjectEntry:Inject()
        private static void Inject()
        {
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
