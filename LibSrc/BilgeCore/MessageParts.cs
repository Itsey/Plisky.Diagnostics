using System;

namespace Plisky.Plumbing {

    /// <summary>
    /// Holds all of the parts that are used to create messages.
    /// </summary>
    public class MessageParts {
        public bool mpRequiresReplace;
        public bool TriggerRefresh;
        public string ParameterInfo;
        public string ClassName;
        public string MethodName;
        public string MachineName;
        public string ModuleName;
        public string lineNumber;
        public string osThreadId;
        public string netThreadId;
        public string MessageType;
        public string DebugMessage;
        public string SecondaryMessage;
        public string AdditionalLocationData;
        public string ProcessId;
        public bool Prepend;

        public MessageParts() {
            MachineName = ModuleName = ClassName = MethodName = lineNumber = osThreadId = netThreadId = MessageType = DebugMessage = SecondaryMessage = AdditionalLocationData = ProcessId = String.Empty;
        }
    }
}