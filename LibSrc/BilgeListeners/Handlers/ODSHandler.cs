

namespace Plisky.Diagnostics.Listeners {

    using System;
    using System.Threading.Tasks;

    public class ODSHandler : IBilgeMessageHandler {
        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(ODSHandler);

        public void HandleMessage(MessageMetadata msgMeta) {
            string msg = Formatter.ConvertToString(msgMeta);
            LegacyFlimFlamFormatter.internalOutputDebugString(msg);
        }

#if NET452 || NETSTANDARD2_0
        public Task HandleMessageAsync(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }

#else
        public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }
#endif

        public void Flush() {
            // No caching, therefore flush not required.
        }

        public void CleanUpResources() {
            // No unmanaged resources.
        }

        public ODSHandler() {
            Formatter = new LegacyFlimFlamFormatter();
        }
    }
}

