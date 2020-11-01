namespace Plisky.Diagnostics {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;    
    using System.Text;


    /// <summary>
    /// Manages Alert type records for Bilge, that do not go through the traditional trace level selection but are deisgned to push notifications to the monitoring
    /// infrastructure irrespective of trace level.
    /// </summary>
    public class BilgeAlert {
        BilgeRouter rt;
        private DateTime onlineAt;
        private string AlertContextId = "Alerting";

        private Dictionary<string, string> alertingContexts { get; set; }


        internal BilgeAlert() {
            rt = BilgeRouter.Router;
            alertingContexts = new Dictionary<string, string>();
            alertingContexts.Add(Bilge.BILGE_INSTANCE_CONTEXT_STR, AlertContextId);
        }

        private void AlertQueue(string message, Dictionary<string, string> values) {
            string meth = "-ba-alert-";
            string pth = "alerting";
            int ln = 0;
            

            MessageMetadata mmd = new MessageMetadata(meth, pth, ln);
            mmd.CommandType = TraceCommandTypes.Alert;
            mmd.Body = message;
            mmd.FurtherDetails = JsonTheValues(values);

            rt.PrepareMetaData(mmd, alertingContexts);
            rt.QueueMessage(mmd);
        }

        private string JsonTheValues(Dictionary<string, string> values) {
            if (values.Count > 0) {
                StringBuilder sb = new StringBuilder();
                sb.Append("{ ");
                foreach (var v in values.Keys) {
                    sb.Append($" \"{v}\": \"{values[v]}\"");
                }
                sb.Append("{ ");
                return sb.ToString();
            } else {
                return string.Empty;
            }
        }
        /// <summary>
        /// Alerting level call to indicate that an application is online and ready to begin processing.  Starts the uptime counter and sends basic telemetry to the
        /// trace stream.
        /// </summary>
        /// <param name="appName">An identifier for the application</param>
        public string Online(string appName) {
            if (string.IsNullOrWhiteSpace(appName)) {
                appName = "Unknown";
            }

            string ver = GetVersionFromAssembly();
            alertingContexts[Bilge.BILGE_INSTANCE_CONTEXT_STR] = appName;
            onlineAt = DateTime.Now;
            string toWrite = $"{appName} Online. v-{ver}. @{onlineAt}"; // [{rt.MachineNameCache}]";

            Dictionary<string, string> avals = new Dictionary<string, string>();
            avals.Add("alert-name", "online");
            avals.Add("onelineAt", onlineAt.ToString());
            avals.Add("machine-name", rt.MachineNameCache);
            avals.Add("app-name", appName);
            avals.Add("app-ver", ver);

            AlertQueue(toWrite, avals);
            return toWrite;
        }

        private string GetVersionFromAssembly() {
            var asm = Assembly.GetEntryAssembly();
            if (asm!=null) {
                return asm.GetName().Version.ToString();
            }
            return "unknown";
        }
    }


}
