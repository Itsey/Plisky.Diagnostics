using System.Diagnostics;

namespace Plisky.Diagnostics {
    internal class ConfigSettings {
        public TraceLevel activeTraceLevel { get; internal set; }
        public string InstanceContext { get; internal set; }
        public bool AddTimingsToEnterExit { get; internal set; }
    }
}