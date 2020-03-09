

namespace Plisky.Diagnostics.Listeners {
    using Plisky.Plumbing;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    
    public class ODSHandler : IBilgeMessageHandler {

        [SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api", Justification = "Want to send to ODS not to a debugger")]
        [DllImport("kernel32.dll", EntryPoint = "OutputDebugStringA", SetLastError = false)]
        public static extern void OutputDebugString(String s);

        public static void internalOutputDebugString(string thisMsg) {
            try {
                // The output debug string API seems to be a little strange when it comes to handeling large amounts of
                // data and does not seem to be able to handle long strings properly.  It is likely that it is my code
                // that is at fault but untill I can get to the bottom of it this listener will chop long strings up
                // into chunks and send them as separate chunks of 1024 bytes in each.  It is then the viewers job
                // to reassemble them, however in order to help the viewer specialist string truncated identifiers will
                // be sent as the markers to the extended strings.
                if (thisMsg.Length > Constants.LIMIT_OUTPUT_DATA_TO) {
                    
                    string[] messageParts = LegacyFlimFlamFormatter.MakeManyStrings(thisMsg, Constants.LIMIT_OUTPUT_DATA_TO);
                    // Truncation identifier is #TNK#[MACHINENAME][TRUNCJOINID]XEX
                   
                    for (int partct = 0; partct < messageParts.Length; partct++) {
                        OutputDebugString( messageParts[partct] ); 
                    }
                   

                    return;
                }

                OutputDebugString(thisMsg);
            } catch (Exception ex) {
                InternalUtil.LogInternalError(InternalUtil.InternalErrorCodes.ODSListenerError, "There was an error trying to send data to outputdebugstring. " + ex.Message);
                throw;
            }
        }

        private string status;
        public LegacyFlimFlamFormatter Formatter { get; private set; }

        public int Priority => 5;
        public string Name => nameof(ODSHandler);

        public void HandleMessage(MessageMetadata msgMeta) {
            try {
                string msg = Formatter.ConvertToString(msgMeta);
                internalOutputDebugString(msg);
                status = "ok";
            } catch (Exception ex) {
                status = "write failed "+ex.Message;
                throw;
            }
        }


        public Task HandleMessageAsync(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }


        public void HandleMessage40(MessageMetadata[] msg) {
            throw new NotImplementedException();
        }


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

