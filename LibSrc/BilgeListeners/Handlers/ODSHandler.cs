

namespace Plisky.Diagnostics.Listeners {

    using System;
    using System.Threading.Tasks;

    public class ODSHandler : IBilgeMessageHandler {
        private string status;
        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(ODSHandler);

        public void HandleMessage(MessageMetadata msgMeta) {
            try {
                string msg = Formatter.ConvertToString(msgMeta);
                LegacyFlimFlamFormatter.internalOutputDebugString(msg);
                status = "ok";
            } catch (Exception ex) {
                status = "write failed "+ex.Message;
                throw;
            }
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

        public string GetStatus() {
            return $"write status {status}";
        }

        public ODSHandler() {
            Formatter = new LegacyFlimFlamFormatter();
        }
    }
}

