using System.Threading;

namespace AliceInCradleHack
{
    public class InjectEntry
    {
        private static readonly Thread _injectThread = new(Client.Initialize);

        // Entry point invoked by the injector: AliceInCradleHack.InjectEntry:Inject()
        private static void Inject()
        {
            _injectThread.SetApartmentState(ApartmentState.STA);
            _injectThread.Start();
        }

        private static void Eject()
        {
        }
    }
}
