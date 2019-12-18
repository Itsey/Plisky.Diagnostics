using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Plisky.Diagnostics {

    /// <summary>
    /// Current active configuration applied to the various writers.  Holds context data as well as the current trace level etc.
    /// </summary>
    public class ConfigSettings {
        public Dictionary<string, string> metaContexts { get; set; }
        public TraceLevel activeTraceLevel { get; internal set; }

        public string InstanceContext { 
            get {
                return metaContexts[Bilge.BILGE_INSTANCE_CONTEXT_STR];
            }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    value = "default";
                }
                metaContexts[Bilge.BILGE_INSTANCE_CONTEXT_STR] = value;
            }
        }
        public bool AddTimingsToEnterExit { get; internal set; }
        public string SessionContext {
            get {
                return metaContexts[Bilge.BILGE_SESSION_CONTEXT_STR];
            }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    value = Guid.NewGuid().ToString();
                }
                metaContexts[Bilge.BILGE_SESSION_CONTEXT_STR] = value;
            }
        }

        public ConfigSettings() {
            metaContexts = new Dictionary<string, string>();
            InstanceContext = null;
        }
    }
}