namespace Plisky.Diagnostics.Listeners {

    using Plisky.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class TCPHandler : IBilgeMessageHandler {
        private string target;
        private string status = "Untouched";
        public Exception LastFault { get; internal set; }

        private AsyncTCPClient tcpClient;

        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 1;
        public string Name => nameof(TCPHandler);

#if NET452
        public async void HandleMessageAsync(MessageMetadata msgMeta) {
            try {
                string msg = Formatter.ConvertToString(msgMeta);
                await tcpClient.Writestufftest(msg);
                status = "ok";
            } catch (Exception ex) {
                LastFault = ex;
                status = ex.Message;
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
                await tcpClient.Writestufftest(whatToWrite).ConfigureAwait(false);
                status = "ok";
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }

        public void Flush() {

        }

        public TCPHandler(string targetIp, int targetPort) {
            try {
                target = $"{targetIp}:{targetPort}";
                tcpClient = new AsyncTCPClient(targetIp, targetPort);
                //client.initialize();
                Formatter = new LegacyFlimFlamFormatter();
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }


        public void CleanUpResources() {
            if (tcpClient != null) {
                tcpClient.Dispose();
            }
        }

        public string GetStatus() {
            
            return $"To: {target} status:{status} + {tcpClient.GetStatus()}.";
        }
    }
}

