﻿namespace Plisky.Diagnostics.Listeners {

    using Plisky.Plumbing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Threading.Tasks;

    public class TCPHandler : BaseHandler, IBilgeMessageHandler {
        internal string target;
        internal string status = "Untouched";
        internal AsyncTCPClient tcpClient;
        internal bool failsAreHarsh;


        public Exception LastFault { get; internal set; }

        
        public string Name => nameof(TCPHandler);


        public async void HandleMessageAsync(MessageMetadata msgMeta) {
            try {
                string msg = Formatter.Convert(msgMeta);
                await tcpClient.WriteToExternalSocket(msg);
                status = "ok";
            } catch (Exception ex) {
                LastFault = ex;
                status = ex.Message;
                throw;
            }
        }

        public void HandleMessage40(MessageMetadata[] msg) {
            HandleMessageAsync(msg).Wait();            
        }



        public async Task HandleMessageAsync(MessageMetadata[] msg) {
            try {
                // TODO : Be a bit smart here, dont do them ALL at once.
                string whatToWrite = null;
                bool assertFailFoud = false;
                
 
                var sb = new StringBuilder();
                for (int i = 0; i < msg.Length; i++) {
                    sb.Append(Formatter.Convert(msg[i]));
                    sb.Append(Constants.TCPEND_MARKERTAG);
                    if (msg[i].CommandType == TraceCommandTypes.AssertionFailed) {
                        assertFailFoud = true;
                    }
                }
                whatToWrite = sb.ToString();
                Emergency.Diags.Log("Writing to tcp client " + whatToWrite);
                await tcpClient.WriteToExternalSocket(whatToWrite).ConfigureAwait(false);
                status = "ok";
                if ((assertFailFoud) && (failsAreHarsh)) {
                    try {
                        Process.GetCurrentProcess().Kill();
                    } catch (NotSupportedException) {
                    } catch (InvalidOperationException) {
                    }
                }
            } catch (Exception ex) {
                LastFault = ex;
                throw;
            }
        }

        public void Flush() {

        }

        public TCPHandler(string targetIp, int targetPort, bool harshFails = false) {
            Priority = 1;
            try {
                target = $"{targetIp}:{targetPort}";
                failsAreHarsh = harshFails;
                tcpClient = new AsyncTCPClient(targetIp, targetPort);
                Formatter = DefaultFormatter(false);
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

