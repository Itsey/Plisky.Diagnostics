namespace Plisky.Diagnostics.Listeners {

    using Plisky.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class TCPHandler : IBilgeMessageHandler {
        public Exception LastFault { get; internal set; }

        //private TCPClient client;
        private AsyncTCPClient client2;

        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 1;
        public string Name => nameof(TCPHandler);

#if NET452
        public async void HandleMessageAsync(MessageMetadata msgMeta) {
            try {
                string msg = Formatter.ConvertToString(msgMeta);
                await client2.Writestufftest(msg);
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }
#else
        public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }
#endif


        public async Task HandleMessageAsync(MessageMetadata[] msg) {
            try {
                // TODO : Be a bit smart here, dont do them ALL at once.
                string whatToWrite = null;

                var sb = new StringBuilder();
                for (int i = 0; i < msg.Length; i++) {
                    sb.Append(Formatter.ConvertToString(msg[i]));
                    sb.Append(Constants.TCPEND_MARKERTAG);
                }
                whatToWrite = sb.ToString();
                Emergency.Diags.Log("Writing to tcp client " + whatToWrite);
                await client2.Writestufftest(whatToWrite).ConfigureAwait(false);
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }

        public void Flush() {

        }

        public TCPHandler(string targetIp, int targetPort) {
            try {
                client2 = new AsyncTCPClient(targetIp, targetPort);
                //client.initialize();
                Formatter = new LegacyFlimFlamFormatter();
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }


        public void CleanUpResources() {
            if (client2 != null) {
                client2.Dispose();
            }
        }

    }
}

